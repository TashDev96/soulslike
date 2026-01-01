using System;
using System.Collections.Generic;
using UnityEngine;

namespace game.gameplay_core.characters.config.animation
{
	using game.gameplay_core.damage_system;

	[Serializable]
	public class AnimationEventHit : AnimationEventBase, IHitConfig
	{
		[field: SerializeField]
		public List<bool> InvolvedColliders { get; set; } = new() { true, false, false };

		[field: SerializeField]
		public bool FriendlyFire { get; set; }

		[field: SerializeField]
		public float DamageMultiplier { get; set; } = 1;

		[field: SerializeField]
		public float PoiseDamage { get; set; } = 1;

		float IHitConfig.StartTime => StartTimeNormalized;
		float IHitConfig.EndTime => EndTimeNormalized;
	}
}
