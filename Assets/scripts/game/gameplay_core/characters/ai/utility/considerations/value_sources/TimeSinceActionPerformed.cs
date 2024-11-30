using System;
using game.gameplay_core.characters.ai.considerations.value_sources;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility.considerations.value_sources
{
	public class TimeSinceActionPerformed:ValueSourceBase
	{
		[Header("Time since action performed:")]
		[ValueDropdown("@UtilityEditorHelper.GetActionsDropDown()")]
		[HideIf("UseActionType")]
		public string ActionId;
		[ShowIf("UseActionType")]
		public UtilityAction.ActionType ActionType;
		public bool UseActionType;
		[NonSerialized]
		public UtilityBrain Brain;
		
		public override float GetValue()
		{
			return Brain.GetTimeSinceActionPerformed(ActionId);
		}
	}
}
