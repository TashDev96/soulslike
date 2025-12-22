using ControlFreak2;
using dream_lib.src.reactive;
using game.gameplay_core.characters;
using game.input;
using UnityEngine;

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

		public Camera Camera => _context.Camera.Value;

		public ThirdPersonCameraController(Context context)
		{
			_context = context;
			var cameraTransform = _context.Camera.Value.transform;
			_currentRotation = cameraTransform.eulerAngles;

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

			// 1. Smooth Pivot Follow
			_currentPivotPosition = Vector3.SmoothDamp(
				_currentPivotPosition,
				targetPivot,
				ref _pivotVelocity,
				1f / settings.FollowSpeed
			);

			// 2. Input
			var input = new Vector2(
				InputAdapter.GetAxisRaw(InputAxesNames.CameraHorizontal),
				InputAdapter.GetAxisRaw(InputAxesNames.CameraVertical)
			);

			// 3. Rotate Camera (Manual)
			if(Mathf.Abs(input.x) > 0.001f || Mathf.Abs(input.y) > 0.001f)
			{
				_currentRotation.y += input.x * settings.RotationSpeed * deltaTime;
				_currentRotation.x -= input.y * settings.RotationSpeed * deltaTime;
				_currentRotation.x = Mathf.Clamp(_currentRotation.x, settings.MinPitch, settings.MaxPitch);
			}

			// 4. LockOn Rotation
			else if(player.ExternalData.LockOnTarget.HasValue)
			{
				var target = player.ExternalData.LockOnTarget.Value.ExternalData.Transform.Position;
				var dirToTarget = (target - playerPos).normalized;

				if(dirToTarget.sqrMagnitude > 0.001f)
				{
					var targetRotation = Quaternion.LookRotation(dirToTarget);
					var targetEuler = targetRotation.eulerAngles;

					// Smoothly rotate towards target
					// We use a faster speed for LockOn tracking than auto-rotate
					var lockOnSpeed = settings.RotationSpeed * 2f;

					_currentRotation.y = Mathf.LerpAngle(_currentRotation.y, targetEuler.y, lockOnSpeed * deltaTime);
					_currentRotation.x = Mathf.LerpAngle(_currentRotation.x, 15f, lockOnSpeed * deltaTime); // Slight look down angle
					_currentRotation.x = Mathf.Clamp(_currentRotation.x, settings.MinPitch, settings.MaxPitch);
				}
			}
			else if(settings.AutoRotateSpeed > 0)
			{
				var vector = playerPos - cameraTransform.position;
				vector.y = 0;

				var targetRotation = Quaternion.LookRotation(vector);
				var targetYaw = targetRotation.eulerAngles.y;

				_currentRotation.y = Mathf.LerpAngle(_currentRotation.y, targetYaw, settings.AutoRotateSpeed * deltaTime);
			}

			_lastPlayerPosition = playerPos;

			// 6. Calculate Target Position & Rotation
			var rotation = Quaternion.Euler(_currentRotation);

			// Note: Use _currentPivotPosition here instead of direct player pivot
			var targetCamPos = _currentPivotPosition + rotation * new Vector3(0f, settings.HeightOffset, -settings.Distance);

			// 7. Obstacle Avoidance (SphereCast)
			// Cast from the SMOOTHED pivot to ensure stability
			var directionToCam = (targetCamPos - _currentPivotPosition).normalized;
			var distanceToCam = Vector3.Distance(_currentPivotPosition, targetCamPos);

			_lastCameraUnsafePos = targetCamPos;

			if(Physics.SphereCast(_currentPivotPosition, settings.ObstacleCheckRadius, directionToCam, out var hit, distanceToCam, settings.ObstacleLayerMask))
			{
				// Move camera to hit point (minus radius to not clip)
				var hitDistance = hit.distance;

				// Ensure we don't get too close (min distance clamp optional but recommended)
				hitDistance = Mathf.Max(hitDistance, 0.1f);
				targetCamPos = _currentPivotPosition + directionToCam * hitDistance;
			}

			cameraTransform.position = targetCamPos;
			cameraTransform.rotation = rotation;
		}
	}
}
