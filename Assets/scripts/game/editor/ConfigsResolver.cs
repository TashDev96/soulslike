using game.gameplay_core.inventory.item_configs;
using UnityEditor;

namespace game.editor
{
	public class ConfigsResolver
	{
		public static string[] GetAllItemConfigs()
		{
			var guids = AssetDatabase.FindAssets($"t:{nameof(BaseItemConfig)}");
			var paths = new string[guids.Length];
			for(var i = 0; i < guids.Length; i++)
			{
				paths[i] = AssetDatabase.LoadAssetAtPath<BaseItemConfig>(AssetDatabase.GUIDToAssetPath(guids[i])).name;
			}
			return paths;
		}
	}
}
