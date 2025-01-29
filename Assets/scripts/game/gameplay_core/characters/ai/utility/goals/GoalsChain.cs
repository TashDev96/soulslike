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
		[field: SerializeField]
		public float InertiaWeight { get; private set; }
		
		[NonSerialized]
		[ShowInInspector]
		[ShowIf("@UtilityAiEditorHelper.DebugEnabled")]
		[GUIColor(0,1,0,1)]
		public float LastWeight;

#if UNITY_EDITOR
		public void PropagateEditorData(SubUtilityBase data)
		{
			foreach(var goal in Goals)
			{
				goal.PropagateEditorData(data);
			}
		}
#endif
	}
}
