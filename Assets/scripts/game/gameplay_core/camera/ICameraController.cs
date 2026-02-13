using UnityEngine;

namespace game.gameplay_core.camera
{
	public interface ICameraController
	{
		Camera Camera { get; }
		void Update(float deltaTime);

		public Vector3 ConvertScreenSpaceDirectionToWorld(Vector3 screenSpaceInput);
		public bool OverrideAttackDirectionOnClick(out Vector3 newDirectionWorld);
	}
}
