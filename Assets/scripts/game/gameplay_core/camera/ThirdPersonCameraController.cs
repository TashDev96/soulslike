using System;
using ControlFreak2;
using DG.Tweening;
using dream_lib.src.camera;
using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters;
using game.input;
using UnityEngine;
using Random = UnityEngine.Random;

namespace game.gameplay_core.camera
{
	public class ThirdPersonCameraController : ICameraController
	{
		public struct Context
		{
			public IReadOnlyReactiveProperty<Camera> Camera;
			public ReactiveProperty<CharacterDomain> Player;
			public ThirdPersonCameraSettings CameraSettings;
		}

		private readonly Context _context;
		private Vector3 _currentRotation;
		private Vector3 _currentPivotPosition;
		private Vector3 _pivotVelocity;
		private Vector3 _lastPlayerPosition;
		private bool _hasInitializedPosition;
		private Vector3 _lastCameraUnsafePos;
		private Tweener _shakeTweener;
		private Vector3 _shakeOffset;
		private bool _flightMode;

		public Camera Camera => _context.Camera.Value;

		public ThirdPersonCameraController(Context context)
		{
			_context = context;
			var cameraTransform = _context.Camera.Value.transform;
			_currentRotation = cameraTransform.eulerAngles;

			_context.Camera.Value.orthographic = false;
			_context.Camera.Value.fieldOfView = 60;

			CFCursor.lockState = CursorLockMode.Locked;
			CFCursor.visible = false;
		}

		public void Update(float deltaTime)
		{
			var player = _context.Player.Value;
			if(player == null)
			{
				return;
			}

			var cameraTransform = _context.Camera.Value.transform;
			var playerTransform = player.ExternalData.Transform;
			var settings = _context.CameraSettings;
			var playerPos = playerTransform.Position;
			var targetPivot = playerPos + settings.PivotOffset;

			if(!_hasInitializedPosition)
			{
				_lastPlayerPosition = playerPos;
				_currentPivotPosition = targetPivot;
				_hasInitializedPosition = true;
			}

			_currentPivotPosition = Vector3.SmoothDamp(
				_currentPivotPosition,
				targetPivot,
				ref _pivotVelocity,
				1f / settings.FollowSpeed
			);

			var input = new Vector2(
				InputAdapter.GetAxisRaw(InputAxesNames.CameraHorizontal),
				InputAdapter.GetAxisRaw(InputAxesNames.CameraVertical)
			);

			if(Mathf.Abs(input.x) > 0.001f || Mathf.Abs(input.y) > 0.001f)
			{
				_currentRotation.y += input.x * settings.RotationSpeed * deltaTime;
				_currentRotation.x -= input.y * settings.RotationSpeed * deltaTime;
				_currentRotation.x = Mathf.Clamp(_currentRotation.x, settings.MinPitch, settings.MaxPitch);
			}

			else if(player.ExternalData.LockOnTarget.HasValue)
			{
				var target = player.ExternalData.LockOnTarget.Value.ExternalData.Transform.Position;
				var dirToTarget = (target - playerPos).normalized;

				if(dirToTarget.sqrMagnitude > 0.001f)
				{
					var targetRotation = Quaternion.LookRotation(dirToTarget);
					var targetEuler = targetRotation.eulerAngles;

					var lockOnSpeed = settings.RotationSpeed * 2f;
					
					

					_currentRotation.y = Mathf.LerpAngle(_currentRotation.y, targetEuler.y, lockOnSpeed * deltaTime);
					_currentRotation.x = Mathf.LerpAngle(_currentRotation.x, targetEuler.x+15f, lockOnSpeed * deltaTime/3f); // Slight look down angle
					_currentRotation.x = Mathf.Clamp(_currentRotation.x, settings.MinPitch, settings.MaxPitch);
				}
			}
			else if(settings.AutoRotateSpeed > 0 && !_flightMode)
			{
				var vector = playerPos - cameraTransform.position;
				vector.y = 0;

				var targetRotation = Quaternion.LookRotation(vector);
				var targetYaw = targetRotation.eulerAngles.y;

				_currentRotation.y = Mathf.LerpAngle(_currentRotation.y, targetYaw, settings.AutoRotateSpeed * deltaTime);
			}

			_lastPlayerPosition = playerPos;

			var rotation = Quaternion.Euler(_currentRotation);

			var targetCamPos = _currentPivotPosition + rotation * new Vector3(0f, settings.HeightOffset, -settings.Distance);

			var directionToCam = (targetCamPos - _currentPivotPosition).normalized;
			var distanceToCam = Vector3.Distance(_currentPivotPosition, targetCamPos);

			_lastCameraUnsafePos = targetCamPos;

			if(Physics.SphereCast(_currentPivotPosition, settings.ObstacleCheckRadius, directionToCam, out var hit, distanceToCam, settings.ObstacleLayerMask))
			{
				var hitDistance = hit.distance;

				hitDistance = Mathf.Max(hitDistance, 0.1f);
				targetCamPos = _currentPivotPosition + directionToCam * hitDistance;
			}

			cameraTransform.position = targetCamPos;
			cameraTransform.rotation = rotation;
		}

		public Vector3 ConvertScreenSpaceDirectionToWorld(Vector3 screenSpaceInput)
		{
			return Camera.ProjectScreenVectorToWorldPlaneWithSkew(screenSpaceInput);
		}

		public bool OverrideAttackDirectionOnClick(out Vector3 newDirectionWorld)
		{
			newDirectionWorld = default;
			return false;
		}

		public void ShowCriticalAttackAnimation(CharacterTransform contextTransform, float expectedDuration)
		{
			//TODO: fix rotation
		}

		public void Shake(float duration, float strength, float vertMultiplier = 1f, float horMultiplier = 1f)
		{
			_shakeTweener?.Kill();
			_shakeOffset = Vector3.zero;

			var randomOffset = Random.value * 100f;
			_shakeTweener = DOVirtual.Float(strength, 0f, duration, value =>
			{
				var seed = strength < 0.5f ? value * 20f : Time.time * 20f + randomOffset;
				_shakeOffset.x = (Mathf.PerlinNoise(seed, 0f) - 0.5f) * 2f * value * horMultiplier;
				_shakeOffset.y = (Mathf.PerlinNoise(0f, seed) - 0.5f) * 2f * value * vertMultiplier;
			}).OnComplete(() => _shakeOffset = Vector3.zero);
		}

		public void SetFlightMode(bool on)
		{
			_flightMode = on;
		}
	}
}
