using System;
using System.Collections.Generic;
using System.Linq;
using dream_lib.src.utils.data_types;
using Sirenix.OdinInspector;

namespace game.gameplay_core.characters.ai
{
	[Serializable]
	public class UtilityGoal
	{
		public float WeightAdd;
		[ValueDropdown("@UtilityEditorHelper.GetActionsDropDown()")]
		public string Action;
		
#if UNITY_EDITOR
		[NonSerialized]
		// ReSharper disable once InconsistentNaming
		public Func<IList<string>> GetActionsList;
#endif
		
	}
}
