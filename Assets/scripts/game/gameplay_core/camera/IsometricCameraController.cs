using dream_lib.src.reactive;
using game.gameplay_core.characters;
using UnityEngine;

namespace game.gameplay_core.camera
{
	public class IsometricCameraController
	{
		public struct Context
		{
			public IReadOnlyReactiveProperty<Camera> Camera;
			public ReactiveProperty<CharacterDomain> Player;
			public CameraSettings CameraSettings;
		}

		private readonly Context _context;

		public IsometricCameraController(Context context)
		{
			_context = context;
		}

		public void Update(float deltaTime)
		{
			var cameraTransform = _context.Camera.Value.transform;
			var targetPosition = _context.Player.Value.ExternalData.Transform.Position;
			var forward = cameraTransform.forward;
			var altitude = _context.CameraSettings.CameraAltitude;

			if (forward.y > -1e-3f && forward.y < 1e-3f)
			{
				var vectorToTarget = targetPosition - cameraTransform.position;
				var cameraRight = cameraTransform.right;
				var cameraUp = cameraTransform.up;
				var offsetRight = Vector3.Dot(vectorToTarget, cameraRight);
				var offsetUp = Vector3.Dot(vectorToTarget, cameraUp);
				cameraTransform.position = cameraTransform.position + cameraRight * offsetRight + cameraUp * offsetUp;
				return;
			}

			var distance = -altitude / forward.y;
			var newPosition = targetPosition - forward * distance;
			cameraTransform.position = newPosition;
		}
	}
}
