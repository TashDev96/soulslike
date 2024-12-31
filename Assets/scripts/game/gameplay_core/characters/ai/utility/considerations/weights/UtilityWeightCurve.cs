using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace game.gameplay_core.characters.ai
{
	[Serializable]
	[OdinDontRegister]
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
