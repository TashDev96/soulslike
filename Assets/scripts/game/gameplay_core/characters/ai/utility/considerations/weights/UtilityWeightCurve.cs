using System;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility.considerations.weights
{
	[Serializable]
#if UNITY_EDITOR
	[OdinDontRegister]
#endif

	[HideReferenceObjectPicker]
	public class UtilityWeightCurve : UtilityWeightBase
	{
		[SerializeField]
		public float Multiplier = 1f;
		[HideReferenceObjectPicker]
		public AnimationCurve Curve = new();

		protected override float EvaluateInternal(float statValue)
		{
			return Curve.Evaluate(statValue) * Multiplier;
		}
	}
}
