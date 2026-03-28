using System;
using UnityEngine;

namespace game.gameplay_core.characters.config
{
	[Serializable]
	public class FlyingConfig
	{
		[field: SerializeField]
		public float BaseSpeed { get; private set; } = 10f;
		[field: SerializeField]
		public float MaxSpeed { get; private set; } = 30f;
		[field: SerializeField]
		public Vector3 Friction { get; private set; } = new Vector3(0.005f, 0.1f, 0.01f);
		[field: SerializeField]
		public float PitchSpeed { get; private set; } = 40f;
		[field: SerializeField]
		public AnimationCurve YawSpeedByForwardSpeed { get; private set; }
		[field: SerializeField]
		public float MaxRollAngle { get; private set; } = 45f;
		[field: SerializeField]
		public float RollSpeed { get; private set; } = 5f;
		[field: SerializeField]
		public float AltitudeSpeedGain { get; private set; } = 2f;
		[field: SerializeField]
		public float FlapForce { get; private set; } = 5f;
		[field: SerializeField]
		public float FlapStaminaCost { get; private set; } = 10f;
		[field: SerializeField]
		public float FlapCooldown { get; private set; } = 0.5f;
		[field: SerializeField]
		public float MaxUpPitchWithNoEnergy { get; private set; } = 15f;
		[field: SerializeField]
		public AnimationCurve StallPitchDownSpeedCurve { get; private set; } = AnimationCurve.Linear(0, 0, 10, 60);
		[field: SerializeField]
		public AnimationCurve MinPitchPerSpeed { get; private set; } = AnimationCurve.Linear(0, 0, 10, 60);
		[field: SerializeField]
		public float StallRecoveryPitch { get; private set; } = 20f;
		[field: SerializeField]
		public float LiftForceCoeff { get; set; } = 0.1f;
	}
}
