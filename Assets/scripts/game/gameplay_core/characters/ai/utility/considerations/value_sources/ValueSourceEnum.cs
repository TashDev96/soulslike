using System;
using game.gameplay_core.characters.ai.utility.blackbox;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility.considerations.value_sources
{
	public enum BlackboardValues
	{
		StupidityWeight,
		NoAttacksWeight
	}

	[Serializable]
	public class ValueSourceEnum : ValueSourceBase
	{
		[SerializeField]
		private BlackboardValues _valueKey;
		[SerializeField]
		private float _multiplier = 1;
		
		public override float GetValue(UtilityBrainContext context)
		{
			return context.BlackboardValues[_valueKey] * _multiplier;
		}
	}
}
