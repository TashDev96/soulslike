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
		public List<Vector2> HitTimings { get; private set; } = new();

		[field: SerializeField]
		[field: HideInInspector]
		public List<float> HitDamageMultipliers { get; private set; } = new();

#if UNITY_EDITOR
		[OnInspectorGUI]
		private void What()
		{
			GUILayout.Space(20);
			GUILayout.Label($"Hit Configs:");
			GUILayout.Space(10);

			for(int i = 0; i < HitTimings.Count; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label($"Hit {i}");
				if(GUILayout.Button("-", GUILayout.Width(30)))
				{
					HitTimings.RemoveAt(i);
					HitDamageMultipliers.RemoveAt(i);
					return;
				}
				GUILayout.EndHorizontal();

				const int fps = 60;
				var denormalizedTiming = HitTimings[i] * Duration * fps;
				denormalizedTiming = SirenixEditorFields.MinMaxSlider($"Timing:", denormalizedTiming, new Vector2(0, Duration*fps));
				HitTimings[i] = denormalizedTiming.Round(1) / Duration/fps;
				HitDamageMultipliers[i] = SirenixEditorFields.FloatField($"Damage Multiplier:", HitDamageMultipliers[i]);
				GUILayout.Space(10);
			}

			if(GUILayout.Button("Add"))
			{
				HitTimings.Add(new Vector2(0.5f, 0.6f));
				HitDamageMultipliers.Add(1f);
			}
		}
#endif
	}
}
