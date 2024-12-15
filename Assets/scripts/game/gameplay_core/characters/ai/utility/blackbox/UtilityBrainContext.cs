using System.Collections.Generic;
using UnityEngine;

namespace game.gameplay_core.characters.ai.blackbox
{
	public class UtilityBrainContext
	{
		public CharacterContext CharacterContext;
		public List<ActionHistoryNode> PerformedActionsHistory = new();
		public Transform TargetTransform { get; set; }
	}
}
