using System.Collections.Generic;

namespace game.gameplay_core.characters.ai.blackbox
{
	public class UtilityBrainContext
	{
		public CharacterContext CharacterContext;
		public List<ActionHistoryNode> PerformedActionsHistory = new();
	}
}
