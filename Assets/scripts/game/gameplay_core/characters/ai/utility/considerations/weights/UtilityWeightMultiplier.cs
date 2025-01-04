using System;
using UnityEngine;

namespace game.gameplay_core.characters.ai
{
	[Serializable]
	public class UtilityWeightMultiplier : UtilityWeightBase
	{
		[SerializeField]
		private float _multiplier = 1;

		protected override float EvaluateInternal(float seedValue)
		{
			return seedValue * _multiplier;
		}
	}
}
