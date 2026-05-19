using System;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

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
		[SerializeField]
		private bool _normalizeCurveLength;
		[SerializeField]
		[ReadOnly]
		[ShowIf(nameof(_normalizeCurveLength))]
		private float _curveDuration;

		[HideReferenceObjectPicker]
		public AnimationCurve Curve = new();

		public override void OnValidateEditor()
		{
			base.OnValidateEditor();
			if(_normalizeCurveLength)
			{
				_curveDuration = Curve.keys[Curve.length - 1].time;
			}
		}

		protected override float EvaluateInternal(float statValue)
		{
			if(_normalizeCurveLength)
			{
				statValue *= _curveDuration;
			}
			return Curve.Evaluate(statValue) * Multiplier;
		}
	}
}
