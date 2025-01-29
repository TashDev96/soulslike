using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility.considerations.weights
{
	[Serializable]
	public class UtilityWeightCompare : UtilityWeightBase
	{
		[SerializeField]
		[EnumToggleButtons]
		private CompareType _compareType;
		[SerializeField]
		private float _value;

		[Space]
		[SerializeField]
		private float _metWeight;
		[SerializeField]
		private float _notMetWeight;

		protected override float EvaluateInternal(float seedValue)
		{
			if(CheckMetCriteria(seedValue))
			{
				return _metWeight;
			}

			return _notMetWeight;
		}

		private bool CheckMetCriteria(float seedValue)
		{
			switch(_compareType)
			{
				case CompareType.Equal:
					return Mathf.Approximately(seedValue, _value);
				case CompareType.Less:
					return seedValue < _value;
				case CompareType.Greater:
					return seedValue > _value;
				case CompareType.LessOrEqual:
					return seedValue <= _value;
				case CompareType.GreaterOrEqual:
					return seedValue >= _value;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private enum CompareType
		{
			[LabelText("==")]
			Equal,
			[LabelText("<")]
			Less,
			[LabelText(">")]
			Greater,
			[LabelText("<=")]
			LessOrEqual,
			[LabelText(">=")]
			GreaterOrEqual
		}
	}
}
