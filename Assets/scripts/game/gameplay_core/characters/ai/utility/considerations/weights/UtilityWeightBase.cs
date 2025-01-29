using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility.considerations.weights
{
	[Serializable] [HideReferenceObjectPicker]
	public abstract class UtilityWeightBase
	{
		public const float MinValue = -100;
		public const float MaxValue = 100;

		public float Evaluate(float seedValue)
		{
			var unclampedValue = EvaluateInternal(seedValue);
			return Mathf.Clamp(unclampedValue, MinValue, MaxValue);
		}

		protected abstract float EvaluateInternal(float seedValue);
	}
}
