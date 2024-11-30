using System.Collections.Generic;
using System.Linq;
using dream_lib.src.utils.data_types;

namespace game.gameplay_core.characters.ai.editor
{
	public class UtilityEditorHelper
	{
		public static SerializableDictionary<string, UtilityAction> CurrentContextActions = new ();

		public static IList<string> GetActionsDropDown()
		{
			return CurrentContextActions.Keys.ToList();
		}
	}
}
