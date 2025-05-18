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
		}

		private readonly Context _context;
		private readonly float _cameraAltitude;

		public IsometricCameraController(Context context)
		{
			_context = context;
			_cameraAltitude = context.Camera.Value.transform.position.y;
		}

		public void Update(float deltaTime)
		{
			var cameraTransform = _context.Camera.Value.transform;
			var vectorToTarget = _context.Player.Value.ExternalData.Transform.Position - cameraTransform.position;

			var cameraRight = cameraTransform.right;
			var cameraUp = cameraTransform.up;

			var offsetRight = Vector3.Dot(vectorToTarget, cameraRight);
			var offsetUp = Vector3.Dot(vectorToTarget, cameraUp);

			cameraTransform.position = cameraTransform.position + cameraRight * offsetRight + cameraUp * offsetUp;
		}
	}
}
