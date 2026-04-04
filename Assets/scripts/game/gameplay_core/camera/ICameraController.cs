using dream_lib.src.utils.data_types;
using UnityEngine;

namespace game.gameplay_core.camera
{
	public interface ICameraController
	{
		Camera Camera { get; }
		void Update(float deltaTime);

		public Vector3 ConvertScreenSpaceDirectionToWorld(Vector3 screenSpaceInput);
		public bool OverrideAttackDirectionOnClick(out Vector3 newDirectionWorld);
		void ShowCriticalAttackAnimation(CharacterTransform contextTransform, float expectedDuration);
		void Shake(float duration, float strength, float vertMultiplier = 1f, float horMultiplier = 1f);
	}
}
