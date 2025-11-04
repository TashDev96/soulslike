using System;
using game.gameplay_core.characters.ai.utility.blackbox;
using game.gameplay_core.characters.ai.utility.considerations.utils;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility.considerations.value_sources
{
	[Serializable]
	public class ValueSourceRandom : ValueSourceBase
	{
		[SerializeField]
		private PerlinConfig _randomValuesOverTime = new();

		public override float GetValue(UtilityBrainContext context)
		{
			return _randomValuesOverTime.Evaluate(context.BrainTime);
		}
	}
}
