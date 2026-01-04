using System;
using System.Collections.Generic;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.config.animation
{
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
