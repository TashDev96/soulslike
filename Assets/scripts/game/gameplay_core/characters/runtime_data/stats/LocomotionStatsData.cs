using System;

namespace game.gameplay_core.characters.runtime_data.stats
{
	[Serializable]
	public class LocomotionStatsData
	{
		public float HalfTurnDurationSeconds { get; set; } = 0.22f;
		public float HalfTurnDurationSecondsLockOn { get; set; } = 0.22f;
		public float WalkSpeed { get; set; } = 5f;
		public float RunSpeed { get; set; } = 15f;
		public float WalkAcceleration { get; set; } = 1f;
		public float WalkDeceleration { get; set; } = 0.1f;
	}
}
