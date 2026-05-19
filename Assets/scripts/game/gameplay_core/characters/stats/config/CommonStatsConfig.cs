using System;
using dream_lib.src.utils.data_types;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.characters.stats.config
{
	[CreateAssetMenu(menuName = "Configs/CommonStatsConfig")]
	public class CommonStatsConfig : ScriptableObject
	{
		[ValidateInput(nameof(ValidateStats), "Stats must have all keys")]
		public SerializableDictionary<StatKey, StatConfig> Stats;

		private bool ValidateStats()
		{
			var allStats = Enum.GetValues(typeof(StatKey)) as StatKey[];
			return Stats.Count == allStats.Length;
		}

#if UNITY_EDITOR
		[HideIf("@ValidateStats()")]
		[Button]
		private void FillMissingKeys()
		{
			var allStats = Enum.GetValues(typeof(StatKey)) as StatKey[];
			foreach(var key in allStats)
			{
				if(!Stats.ContainsKey(key))
				{
					Stats.Add(key, new StatConfig());
				}
			}

			EditorUtility.SetDirty(this);
		}
#endif
	}
}
