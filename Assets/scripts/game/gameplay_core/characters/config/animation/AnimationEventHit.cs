using System;
using System.Collections.Generic;
using UnityEngine;

namespace game.gameplay_core.characters.config.animation
{
	[Serializable]
	public class AnimationEventHit : AnimationEventBase
	{
		[field: SerializeField]
		public List<bool> InvolvedColliders { get; set; } = new() { true, false, false };

		[field: SerializeField]
		public bool FriendlyFire { get; set; }

		[field: SerializeField]
		public float DamageMultiplier { get; set; } = 1;

		[field: SerializeField]
		public float PoiseDamage { get; set; } = 1;
	}
}
