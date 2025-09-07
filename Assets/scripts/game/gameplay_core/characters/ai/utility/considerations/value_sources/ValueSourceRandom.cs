using System;
using game.gameplay_core.characters.ai.utility.blackbox;
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

		public AnimationCurve RandomValuesOverTime => _randomValuesOverTime;

		public override float GetValue(UtilityBrainContext context)
		{
			return _randomValuesOverTime.Evaluate(_timeOffset + context.CharacterContext.LocationTime.Value);
		}
	}
}
