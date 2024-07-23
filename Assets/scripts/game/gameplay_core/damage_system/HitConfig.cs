using System;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	[Serializable]
	public class HitConfig
	{
		[field: SerializeField]
#if UNITY_EDITOR
		public Vector2 Timing { get; set; }
#else
		public Vector2 Timing { get; private set; }
#endif

		[field: SerializeField]
#if UNITY_EDITOR
		public float DamageMultiplier { get; set; } = 1;
#else
		public float DamageMultiplier { get; private set; } = 1;
#endif
	}
}