using System.Collections.Generic;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine
{
	public class HitData
	{
		public bool IsStarted;
		public bool IsEnded;
		public HitConfig Config;

		public HashSet<string> ImpactedCharacters = new();
		public HashSet<Collider> ImpactedColliders = new();

		public bool IsActive => IsStarted && !IsEnded;
	}
}