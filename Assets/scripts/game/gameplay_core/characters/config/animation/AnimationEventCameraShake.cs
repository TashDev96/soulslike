using System;

namespace game.gameplay_core.characters.config.animation
{
	[Serializable]
	public class AnimationEventCameraShake : AnimationEventBase
	{
		public float Strength = 0.45f;
		public float VertMultiplier = 1f;
		public float HorMultiplier = 1f;
	}
}
