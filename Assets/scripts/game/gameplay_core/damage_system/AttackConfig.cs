using System;
using System.Collections.Generic;
using Animancer;
using dream_lib.src.extensions;
using dream_lib.src.utils.editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	[Serializable]
	public class AttackConfig
	{
		[field: SerializeField]
		public ClipTransition Animation { get; private set; }

		[field: SerializeField]
		public float BaseDamage { get; private set; }

		[ShowInInspector]
		public float Duration
		{
			get => Animation.Clip ? Animation.Clip.length : 0.1f;
		}

		[field: SerializeField]
		public AnimationCurve CharacterRotationSpeed { get; private set; }

		[field: SerializeField]
		[field: HideInInspector]
		public List<HitConfig> HitConfigs { get; private set; }

#if UNITY_EDITOR

		const int FPS = 60;

		private PreviewAnimationDrawer _animationPreview;

		[OnInspectorGUI]
		private void DrawCustomHitsInspector()
		{
			if(_animationPreview == null)
			{
				_animationPreview = new PreviewAnimationDrawer(AddressableAssetNames.Player, Animation.Clip);
			}
			
			EditorGUI.BeginChangeCheck();
			
			_animationPreview.ClearTimeChanges();

			GUILayout.Space(20);
			GUILayout.Label($"Hit Configs:");
			GUILayout.Space(10);

			for(int i = 0; i < HitConfigs.Count; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label($"Hit {i}");
				if(GUILayout.Button("-", GUILayout.Width(30)))
				{
					HitConfigs.RemoveAt(i);
					EditorUtility.SetDirty(Selection.activeObject);
					return;
				}

				GUILayout.EndHorizontal();

				HitConfigs[i].Timing = DrawTimingSlider("Timing:", HitConfigs[i].Timing);
				HitConfigs[i].DamageMultiplier = SirenixEditorFields.FloatField($"Damage Multiplier:", HitConfigs[i].DamageMultiplier);
				GUILayout.Space(10);
			}

			if(GUILayout.Button("Add"))
			{
				HitConfigs.Add(new()
				{
					Timing = new Vector2(0.5f, 0.6f),
					DamageMultiplier = 1,
				});
			}

			_animationPreview.CalculateTimeFromChanges();
			_animationPreview.Draw();

			if(EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(Selection.activeObject);
			}
		}

		private Vector2 DrawTimingSlider(string label, Vector2 value)
		{
			GUILayout.Label(label);
			_animationPreview.RegisterTimeBefore(value.x);
			_animationPreview.RegisterTimeBefore(value.y);
			var denormalizedTiming = value * Duration * FPS;
			var framedTiming = SirenixEditorFields.MinMaxSlider(denormalizedTiming, new Vector2(0, Duration * FPS), false);
			var result = framedTiming.Round(1) / Duration / FPS;
			_animationPreview.RegisterTimeAfter(result.x);
			_animationPreview.RegisterTimeAfter(result.y);
			return result;
		}
#endif
	}
}
