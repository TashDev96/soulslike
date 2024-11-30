using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.ai
{
	[Serializable,HideReferenceObjectPicker]
	public abstract class UtilityWeightBase
	{
		public const float MinValue = 0;
		public const float MaxValue = 0;

		public float Evaluate(float seedValue)
		{
			var unclampedValue = EvaluateInternal(seedValue);
			return Mathf.Clamp(MinValue, MaxValue, unclampedValue);
		}

		protected abstract float EvaluateInternal(float seedValue);
	}
}
