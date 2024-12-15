using System;
using game.gameplay_core.characters.ai.considerations.value_sources;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.ai.considerations
{
	[Serializable]
	public class Consideration
	{
		[HideInInspector]
		[SerializeField]
		private bool _commentEnabled;
		[CustomContextMenu("Toggle Comment", "ToggleComment")]
		[HorizontalGroup("Comment")]
		[ShowIf(nameof(_commentEnabled))]
		public string Comment;

		[CustomContextMenu("Toggle Comment", "ToggleComment")]
		[LabelText("@ValueSource.GetType().Name")]
		[SerializeReference]
		[HideReferenceObjectPicker]
		[InlineProperty]
		public ValueSourceBase ValueSource;

		[CustomContextMenu("Toggle Comment", "ToggleComment")]
		[SerializeReference]
		[HideReferenceObjectPicker]
		[InlineProperty]
		public UtilityWeightBase Weight;

		public float Evaluate()
		{
			var value = ValueSource.GetValue();
			return Weight.Evaluate(value);
		}

		private void ToggleComment()
		{
			_commentEnabled = !_commentEnabled;
		}
	}
}
