using System;
using ControlFreak2;
using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters;
using game.input;
using UnityEngine;

namespace game.gameplay_core.camera
{
	public class FirstPersonCameraController : ICameraController
	{
		public struct Context
		{
			public IReadOnlyReactiveProperty<Camera> Camera;
			public ReactiveProperty<CharacterDomain> Player;
			public FirstPersonCameraSettings CameraSettings;
		}

		private readonly Context _context;
		private Vector3 _currentRotation;
		private Vector3 _currentPivotPosition;
		private Vector3 _pivotVelocity;
		private Vector3 _lastPlayerPosition;
		private bool _hasInitializedPosition;
		private Vector3 _lastCameraUnsafePos;
		private Transform _playerTransform;

		private bool _playerInitialized;

		public Camera Camera => _context.Camera.Value;

		public FirstPersonCameraController(Context context)
		{
			_context = context;
			var cameraTransform = _context.Camera.Value.transform;
			_context.Camera.Value.orthographic = false;
			_currentRotation = cameraTransform.eulerAngles;

			CFCursor.lockState = CursorLockMode.Locked;
			CFCursor.visible = false;
		}

		private bool TryInitialize()
		{
			if(_playerInitialized)
			{
				return true;
			}

			if(!_context.Player.HasValue)
			{
				return false;
			}

			_playerTransform = _context.Player.Value.transform;
			_playerInitialized = true;
			return true;
		}

		public void Update(float deltaTime)
		{
			if(!_playerInitialized)
			{
				if(!TryInitialize())
				{
					return;
				}
			}

			var player = _context.Player.Value;

			player.Context.MovementLogic.RotationIsControlledByCamera = true;
			_context.Camera.Value.fieldOfView = _context.CameraSettings.FOV;

			var cameraTransform = _context.Camera.Value.transform;
			var playerTransform = player.ExternalData.Transform;
			var settings = _context.CameraSettings;
			var playerPos = playerTransform.Position;

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

			_playerTransform.rotation = Quaternion.AngleAxis(_currentRotation.y, Vector3.up);
			cameraTransform.position = playerPos + playerTransform.Forward * settings.Offset.z + Vector3.up * settings.Offset.y;
			cameraTransform.rotation = Quaternion.Euler(_currentRotation);
		}

		public Vector3 ConvertScreenSpaceDirectionToWorld(Vector3 screenSpaceInput)
		{
			var result = _playerTransform.rotation * new Vector3(screenSpaceInput.x, 0, screenSpaceInput.y);
			Debug.DrawLine(_playerTransform.position, _playerTransform.position + result, Color.red);
			return result;
		}

		public bool OverrideAttackDirectionOnClick(out Vector3 newDirectionWorld)
		{
			newDirectionWorld = default;
			return false;
		}

		public void ShowCriticalAttackAnimation(CharacterTransform contextTransform, float expectedDuration)
		{
			throw new NotImplementedException();
		}

		public void Shake(float duration, float strength, float vertMultiplier = 1f, float horMultiplier = 1f)
		{
			throw new NotImplementedException();
		}
	}
}
