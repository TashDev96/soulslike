using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;

namespace game.gameplay_core.characters.ai.utility.goals
{
	[Serializable]
	public class UtilityGoal
	{
		public float WeightAdd;
		[ValueDropdown("@GetActionsDropDown()")]
		public string Action;

		public float Duration;

#if UNITY_EDITOR


		

		private List<string> GetActionsDropDown()
		{
			if(Selection.activeGameObject != null)
			{
				var selectedTarget = Selection.activeGameObject.GetComponent<SubUtilityBase>();
				if(selectedTarget != null)
				{
					return selectedTarget.Actions.Select(a => a.Id).ToList();
				}
			}
			return new List<string>(){"Error"};
		}

#endif
	}
}
