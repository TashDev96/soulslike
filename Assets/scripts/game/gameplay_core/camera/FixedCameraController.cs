using dream_lib.src.reactive;
using game.gameplay_core.characters;
using UnityEngine;

namespace game.gameplay_core.camera
{
	public class FixedCameraController : ICameraController
	{
		public struct Context
		{
			public IReadOnlyReactiveProperty<Camera> Camera;
			public ReactiveProperty<CharacterDomain> Player;
			public FixedCameraSettings CameraSettings;
		}

		private readonly Context _context;
		private FixedCameraZone _currentZone;
		private Vector3 _transitionStartPosition;
		private Quaternion _transitionStartRotation;
		private Vector3 _targetPositionForTransition;
		private Quaternion _targetRotationForTransition;
		private float _transitionProgress;
		private bool _isTransitioning;
		private Vector3 _currentFollowPosition;
		private Quaternion _currentLookAtRotation;

		public FixedCameraController(Context context)
		{
			_context = context;
			_transitionProgress = 1f;
		}

		public void Update(float deltaTime)
		{
			var player = _context.Player.Value;
			if(player == null)
			{
				return;
			}

			var cameraTransform = _context.Camera.Value.transform;
			var playerPosition = player.ExternalData.Transform.Position;
			var settings = _context.CameraSettings;

			var newZone = FindCameraZoneForPosition(playerPosition);

			if(newZone != _currentZone && newZone != null)
			{
				StartTransitionToZone(cameraTransform.position, cameraTransform.rotation, newZone, playerPosition);
				_currentZone = newZone;
			}

			if(_currentZone == null)
			{
				if(settings.Zones.Count > 0)
				{
					_currentZone = settings.Zones[0];
					var targetPos = GetTargetPositionForZone(_currentZone, playerPosition);
					var targetRot = GetTargetRotationForZone(_currentZone, playerPosition, targetPos);
					StartTransitionToZone(cameraTransform.position, cameraTransform.rotation, _currentZone, playerPosition);
				}
				return;
			}

			if(_isTransitioning)
			{
				_transitionProgress += deltaTime / settings.TransitionDuration;

				if(_transitionProgress >= 1f)
				{
					_transitionProgress = 1f;
					_isTransitioning = false;
					_currentFollowPosition = _targetPositionForTransition;
					_currentLookAtRotation = _targetRotationForTransition;
				}

				var t = EaseInOutCubic(_transitionProgress);
				cameraTransform.position = Vector3.Lerp(_transitionStartPosition, _targetPositionForTransition, t);
				cameraTransform.rotation = Quaternion.Slerp(_transitionStartRotation, _targetRotationForTransition, t);
			}
			else
			{
				UpdateCameraBehavior(cameraTransform, playerPosition, deltaTime);
			}
		}

		private void UpdateCameraBehavior(Transform cameraTransform, Vector3 playerPosition, float deltaTime)
		{
			var zone = _currentZone;
			var zoneTransform = zone.CameraTransform;

			switch(zone.Behavior)
			{
				case CameraZoneBehavior.Fixed:
					cameraTransform.position = zoneTransform.position;
					cameraTransform.rotation = zoneTransform.rotation;
					break;

				case CameraZoneBehavior.LookAtPlayer:
					cameraTransform.position = zoneTransform.position;
					var targetLookAt = Quaternion.LookRotation(playerPosition + zone.LookAtOffset - zoneTransform.position);
					_currentLookAtRotation = Quaternion.Slerp(_currentLookAtRotation, targetLookAt, zone.RotationSmoothness * deltaTime);
					cameraTransform.rotation = _currentLookAtRotation;
					break;

				case CameraZoneBehavior.FollowPlayer:
					var offset = zoneTransform.position - zoneTransform.parent.position;
					var targetFollowPos = playerPosition + offset;
					_currentFollowPosition = Vector3.Lerp(_currentFollowPosition, targetFollowPos, zone.FollowSmoothness * deltaTime);
					cameraTransform.position = _currentFollowPosition;

					var followLookAt = Quaternion.LookRotation(playerPosition + zone.LookAtOffset - _currentFollowPosition);
					_currentLookAtRotation = Quaternion.Slerp(_currentLookAtRotation, followLookAt, zone.RotationSmoothness * deltaTime);
					cameraTransform.rotation = _currentLookAtRotation;
					break;
			}
		}

		private FixedCameraZone FindCameraZoneForPosition(Vector3 position)
		{
			foreach(var zone in _context.CameraSettings.Zones)
			{
				if(zone.Trigger != null && zone.Trigger.bounds.Contains(position))
				{
					return zone;
				}
			}
			return _currentZone;
		}

		private void StartTransitionToZone(Vector3 currentPosition, Quaternion currentRotation, FixedCameraZone zone, Vector3 playerPosition)
		{
			_transitionStartPosition = currentPosition;
			_transitionStartRotation = currentRotation;
			_targetPositionForTransition = GetTargetPositionForZone(zone, playerPosition);
			_targetRotationForTransition = GetTargetRotationForZone(zone, playerPosition, _targetPositionForTransition);
			_transitionProgress = 0f;
			_isTransitioning = true;
		}

		private Vector3 GetTargetPositionForZone(FixedCameraZone zone, Vector3 playerPosition)
		{
			if(zone.Behavior == CameraZoneBehavior.FollowPlayer && zone.CameraTransform.parent != null)
			{
				var offset = zone.CameraTransform.position - zone.CameraTransform.parent.position;
				return playerPosition + offset;
			}
			return zone.CameraTransform.position;
		}

		private Quaternion GetTargetRotationForZone(FixedCameraZone zone, Vector3 playerPosition, Vector3 cameraPosition)
		{
			if(zone.Behavior == CameraZoneBehavior.LookAtPlayer || zone.Behavior == CameraZoneBehavior.FollowPlayer)
			{
				return Quaternion.LookRotation(playerPosition + zone.LookAtOffset - cameraPosition);
			}
			return zone.CameraTransform.rotation;
		}

		private float EaseInOutCubic(float t)
		{
			return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
		}
	}
}
