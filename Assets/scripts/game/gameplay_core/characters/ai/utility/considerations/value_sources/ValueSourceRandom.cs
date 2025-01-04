using System;
using game.gameplay_core.characters.ai.blackbox;
using game.gameplay_core.characters.ai.considerations.value_sources;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility.considerations.value_sources
{
	[Serializable]
	public class ValueSourceRandom : ValueSourceBase
	{
		[SerializeField]
		private AnimationCurve _randomValuesOverTime;
		[SerializeField]
		private float _timeOffset;

		public override float GetValue(UtilityBrainContext context)
		{
			return _randomValuesOverTime.Evaluate(_timeOffset + context.CharacterContext.LocationTime.Value);
		}
	}
}
