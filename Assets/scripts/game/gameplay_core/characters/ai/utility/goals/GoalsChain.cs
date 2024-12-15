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
	}
}
