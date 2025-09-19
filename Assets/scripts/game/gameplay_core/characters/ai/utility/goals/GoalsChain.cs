using System;
using System.Collections.Generic;
using game.gameplay_core.characters.ai.utility.considerations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility.goals
{
	[Serializable]
	
	public class GoalsChain
	{
		public string Id;
		public List<UtilityGoal> Goals = new();
		[SerializeReference] [HideReferenceObjectPicker]
		public List<Consideration> Considerations = new();

		[NonSerialized]
		[ShowInInspector]
		[ShowIf("@UtilityAiEditorHelper.DebugEnabled")]
		[GUIColor(0, 1, 0)]
		public float LastWeight;
		[field: SerializeField]
		public float InertiaWeight { get; private set; }


	}
}
