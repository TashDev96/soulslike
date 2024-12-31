using System;
using System.Collections.Generic;
using game.gameplay_core.characters.ai.considerations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.ai
{
	[Serializable]
	public class GoalsChain
	{
		public string Id;
		public List<UtilityGoal> Goals = new();
		[SerializeReference] [HideReferenceObjectPicker]
		public List<Consideration> Considerations = new();

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
