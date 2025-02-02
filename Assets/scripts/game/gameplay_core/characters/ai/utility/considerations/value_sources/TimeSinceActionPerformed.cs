using game.gameplay_core.characters.ai.utility.blackbox;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility.considerations.value_sources
{
	[UsedImplicitly]
	public class TimeSinceActionPerformed : ValueSourceBase
	{
		[Header("Time since action performed:")]
		[ValueDropdown("@UtilityEditorHelper.GetActionsDropDown()")]
		[HideIf("UseActionType")]
		public string ActionId;
		[ShowIf("UseActionType")]
		public UtilityAction.ActionType ActionType;
		public bool UseActionType;

		public override float GetValue(UtilityBrainContext context)
		{
			var lastHistoryId = -1;

			for(var i = context.PerformedActionsHistory.Count - 1; i >= 0; i--)
			{
				var node = context.PerformedActionsHistory[i];
				if(UseActionType)
				{
					if(node.Action.Type != ActionType)
					{
						continue;
					}
				}
				else if(node.Action.Id != ActionId)
				{
					continue;
				}

				lastHistoryId = i;
				break;
			}

			if(lastHistoryId >= 0)
			{
				return Time.time - context.PerformedActionsHistory[lastHistoryId].EndTime;
			}

			return float.MaxValue;
		}
	}
}
