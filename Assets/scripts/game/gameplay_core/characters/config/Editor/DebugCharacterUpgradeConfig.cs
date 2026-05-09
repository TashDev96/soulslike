using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.stats.config;
using UnityEngine;

namespace game.gameplay_core.characters.config.Editor
{
	[CreateAssetMenu(menuName = "Configs/DebugCharacterUpgradeConfig")]
	public class DebugCharacterUpgradeConfig : ScriptableObject
	{
		public SerializableDictionary<StatKey, int> StatsUpgrades = new();
	}
}
