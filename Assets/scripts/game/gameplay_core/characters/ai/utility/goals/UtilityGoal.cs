using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace game.gameplay_core.characters.ai
{
	[Serializable]
	public class UtilityGoal
	{
		public float WeightAdd;
		[ValueDropdown("@GetActionsDropDown()")]
		public string Action;

		public float Duration;

#if UNITY_EDITOR

		private SubUtilityBase _editorData;

		public void PropagateEditorData(SubUtilityBase data)
		{
			_editorData = data;
		}

		private List<string> GetActionsDropDown()
		{
			return _editorData.Actions.Select(a => a.Id).ToList();
		}

#endif
	}
}
