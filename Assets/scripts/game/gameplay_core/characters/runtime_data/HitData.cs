using System.Collections.Generic;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.runtime_data
{
	public class HitData
	{
		public bool IsStarted;
		public bool IsEnded;
		public HitConfig Config;

		public HashSet<string> ImpactedCharacters = new();
		public HashSet<Collider> ImpactedTargets = new();

		public bool IsActive => IsStarted && !IsEnded;
	}
}
