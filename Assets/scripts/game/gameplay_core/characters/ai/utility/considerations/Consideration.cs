using System;
using game.gameplay_core.characters.ai.considerations.value_sources;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.ai.considerations
{
	[Serializable]
	public class Consideration
	{
		public string Comment;
		
		[SerializeReference, HideReferenceObjectPicker]
		[InlineProperty]
		public ValueSourceBase ValueSource;
		[SerializeReference, HideReferenceObjectPicker]
		[InlineProperty]
		public UtilityWeightBase Weight;


		public float Evaluate()
		{
			var value = ValueSource.GetValue();
			return Weight.Evaluate(value);
		}

		//target
	}
}
