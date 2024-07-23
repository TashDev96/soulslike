using System;
using System.Collections.Generic;
using dream_lib.src.extensions;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	[Serializable]
	public class AttackConfig
	{
		[field: SerializeField]
		public float BaseDamage { get; private set; }

		[field: SerializeField] 
		public float Duration { get; private set; }


		[field: SerializeField]
		[field: HideInInspector]
		public List<HitConfig> HitConfigs { get; private set; }


#if UNITY_EDITOR
		[OnInspectorGUI]
		private void DrawCustomHitsInspector()
		{
			GUILayout.Space(20);
			GUILayout.Label($"Hit Configs:");
			GUILayout.Space(10);

			for (int i = 0; i < HitConfigs.Count; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label($"Hit {i}");
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					HitConfigs.RemoveAt(i);
					return;
				}

				GUILayout.EndHorizontal();

				const int fps = 60;
				var denormalizedTiming = HitConfigs[i].Timing * Duration * fps;
				denormalizedTiming = SirenixEditorFields.MinMaxSlider($"Timing:", denormalizedTiming, new Vector2(0, Duration * fps));
				HitConfigs[i].Timing = denormalizedTiming.Round(1) / Duration / fps;
				HitConfigs[i].DamageMultiplier = SirenixEditorFields.FloatField($"Damage Multiplier:", HitConfigs[i].DamageMultiplier);
				GUILayout.Space(10);
			}

			if (GUILayout.Button("Add"))
			{
				HitConfigs.Add(new()
				{
					Timing = new Vector2(0.5f, 0.6f),
					DamageMultiplier = 1,
				});
				 
			}
		}
#endif
	}
}