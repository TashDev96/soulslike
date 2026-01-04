using System;

namespace game.gameplay_core.characters.config.animation
{
	[Serializable]
	public class AnimationFlagEvent : AnimationEventBase
	{
		public enum AnimationFlags
		{
			RotationLocked,
			StateLocked,
			StaminaRegenDisabled,
			BodyAttack,
			TimingExitToAttack,
			Invulnerability,
			TimingExitToNextCombo,
			TimingEnterFromCombo,
			TimingEnterFromRoll,
			StartHandleObstacleCast
		}

		public AnimationFlags Flag;

		public override string ToString()
		{
			return Flag.ToString();
		}
	}
}
