using game.gameplay_core.characters.ai.blackbox;
using game.gameplay_core.characters.ai.considerations.value_sources;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility.considerations.value_sources
{
	public class ValueSourceRandom:ValueSourceBase
	{
		public override float GetValue(UtilityBrainContext context)
		{
			return Random.value;
		}
	}
}
