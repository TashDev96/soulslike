using System;
using game.gameplay_core.characters.ai.blackbox;
using game.gameplay_core.characters.ai.considerations.value_sources;
using game.gameplay_core.characters.ai.editor;
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

		[NonSerialized]
		[ShowInInspector]
		[ShowIf("@UtilityAiEditorHelper.DebugEnabled")]
		[GUIColor(0,1,0,1)]
		public float LastWeight;

		public float Evaluate(UtilityBrainContext context)
		{
			var value = ValueSource.GetValue(context);
			LastWeight = Weight.Evaluate(value);
			return LastWeight;
		}

		private void ToggleComment()
		{
			_commentEnabled = !_commentEnabled;
		}
	}
}
