using System;
using System.Collections.Generic;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	[Serializable]
	public class HitConfig : IHitConfig
	{
		[field: SerializeField]
		public List<bool> InvolvedColliders { get; set; } = new() { true, false, false };

		[field: SerializeField]
		public bool FriendlyFire { get; set; }

		[field: SerializeField]
		public float DamageMultiplier { get; set; } = 1;

		[field: SerializeField]
		public float PoiseDamage { get; set; } = 1;

		public float StartTime { get; set; }
		public float EndTime { get; set; }
	}
}
