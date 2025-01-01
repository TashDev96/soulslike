using System.Collections.Generic;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.ai.navigation;

namespace game.gameplay_core.characters.ai.blackbox
{
	public class UtilityBrainContext
	{
		public CharacterContext CharacterContext;
		public List<ActionHistoryNode> PerformedActionsHistory = new();
		public ReadOnlyTransform TargetTransform => Target.ExternalData.Transform;
		public CharacterDomain Target { get; set; }
		public AiNavigationModule NavigationModule { get; set; }
	}
}
