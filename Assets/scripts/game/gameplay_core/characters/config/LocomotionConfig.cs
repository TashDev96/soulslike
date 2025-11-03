using System;
using UnityEngine;

namespace game.gameplay_core.characters.config
{
	[Serializable]
	public class LocomotionConfig
	{
		[field: SerializeField]
		public float HalfTurnDurationSeconds { get; private set; } = 0.22f;

		[field: SerializeField]
		public float WalkSpeed { get; private set; } = 5f;

		[field: SerializeField]
		public float RunSpeed { get; private set; } = 15f;

		[field: SerializeField]
		public float WalkAcceleration { get; private set; } = 1f;
		[field: SerializeField]
		public float WalkDeceleration { get; set; } = 0.1f;

		[field: SerializeField]
		public AnimationCurve WalkAccelerationCurve { get; private set; } = new(new Keyframe(0, 0), new Keyframe(0.33f, 1));
		}
}
