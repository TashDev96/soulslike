using System.Collections.Generic;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.runtime_data
{
	using game.gameplay_core.characters.config.animation;

	public class HitData
	{
		public bool IsStarted;
		public bool IsEnded;
		public IHitConfig Config;

		public HashSet<string> ImpactedCharacters = new();
		public HashSet<Collider> ImpactedTargets = new();

		public float BlockDamageMultiplier = 1f;

		public bool IsActive => IsStarted && !IsEnded;
	}
}
