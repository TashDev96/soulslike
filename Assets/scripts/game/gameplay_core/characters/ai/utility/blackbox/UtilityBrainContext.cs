using System.Collections.Generic;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.ai.navigation;
using game.gameplay_core.characters.ai.utility.considerations.value_sources;

namespace game.gameplay_core.characters.ai.utility.blackbox
{
	public class UtilityBrainContext
	{
		public CharacterContext CharacterContext;
		public List<ActionHistoryNode> PerformedActionsHistory = new();
		public Dictionary<BlackboardValues, float> BlackboardValues = new();
		public ReadOnlyTransform TargetTransform => Target.ExternalData.Transform;
		public CharacterDomain Target { get; set; }
		public AiNavigationModule NavigationModule { get; set; }
		public float BrainTime { get; set; }
	}
}
