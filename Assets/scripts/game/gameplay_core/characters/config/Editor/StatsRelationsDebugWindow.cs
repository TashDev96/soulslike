using game.gameplay_core.characters.stats;
using game.gameplay_core.characters.stats.config;
using game.gameplay_core.inventory.item_configs;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.characters.config.Editor
{
	public class StatsRelationsDebugWindow : OdinEditorWindow
	{
		[SerializeField]
		private CommonStatsConfig _commonStatsConfig;
		[SerializeField]
		private CharacterConfig _targetCharacter;
		[SerializeField]
		private WeaponItemConfig _weapon;

		[MenuItem("Tools/RPG Stats Debugger")]
		public static void ShowWindow()
		{
			var window = GetWindow<StatsRelationsDebugWindow>("RPG Stats Debugger");
			window.Show();
		}

		protected override void OnImGUI()
		{
			base.OnImGUI();

			var statsCalculated = CharacterStatsLogic.CalcAllStatMaxValues(_commonStatsConfig, _targetCharacter.DefaultStatsValueOverride, _weapon);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			foreach(var statConfig in _commonStatsConfig.Stats)
			{
				if(statConfig.Value.Type == StatType.Base)
				{
					EditorGUILayout.LabelField(statConfig.Key + ": " + statsCalculated[statConfig.Key]);
				}
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			foreach(var statConfig in _commonStatsConfig.Stats)
			{
				if(statConfig.Value.Type == StatType.Result)
				{
					EditorGUILayout.LabelField(statConfig.Key + ": " + statsCalculated[statConfig.Key]);
				}
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
		}
	}
}
