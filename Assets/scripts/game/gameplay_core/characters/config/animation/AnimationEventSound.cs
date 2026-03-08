using System;

namespace game.gameplay_core.characters.config.animation
{
	[Serializable]
	public class AnimationEventSound : AnimationEventBase
	{
		public float NormalizedHearDistance = 1.5f;
		public string SoundName;
	}
}
