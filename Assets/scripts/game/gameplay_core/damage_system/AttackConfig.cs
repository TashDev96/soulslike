using System;
using System.Collections.Generic;
using Animancer;
using dream_lib.src.extensions;
#if UNITY_EDITOR
using dream_lib.src.utils.editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif
using Sirenix.OdinInspector;
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
		public float Duration => Animation.Clip ? Animation.Clip.length : 0.1f;

		[field: SerializeField]
		public Vector2 RotationDisabledTime { get; private set; }
		[field: SerializeField]
		public Vector2 LockedStateTime { get; private set; } = new(0, 1f);
		[field: SerializeField]
		public Vector2 ExitToComboTime { get; private set; } = new(0, 1f);
		[field: SerializeField]
		public float EnterComboTime { get; private set; }
		[field:SerializeField]
		public AnimationCurve ForwardMovement { get; private set; }

		[field: SerializeField]
		[field: HideInInspector]
		public List<HitConfig> HitConfigs { get; private set; }

#if UNITY_EDITOR

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
			RotationDisabledTime = DrawTimingSliderMinMax("Rotation Disabled Time:", RotationDisabledTime);
			LockedStateTime = DrawTimingSliderMinMax("Locked State Time:", LockedStateTime);
			ExitToComboTime = DrawTimingSliderMinMax("Exit To Next Combo Time:", ExitToComboTime);
			EnterComboTime = DrawTimingSlider("Enter Combo Time:", EnterComboTime);

			GUILayout.Space(20);
			GUILayout.Label("Hit Configs:");
			GUILayout.Space(10);

			for(var i = 0; i < HitConfigs.Count; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label($"Hit {i}");
				if(GUILayout.Button("Remove", GUILayout.Width(30)))
				{
					HitConfigs.RemoveAt(i);
					EditorUtility.SetDirty(Selection.activeObject);
					return;
				}
				GUILayout.EndHorizontal();

				DrawSelectColliders(HitConfigs[i]);

				HitConfigs[i].Timing = DrawTimingSliderMinMax("Timing:", HitConfigs[i].Timing);
				HitConfigs[i].DamageMultiplier = SirenixEditorFields.FloatField("Damage Multiplier:", HitConfigs[i].DamageMultiplier);
				HitConfigs[i].PoiseDamage = SirenixEditorFields.FloatField("Poise Damage:", HitConfigs[i].PoiseDamage);
				GUILayout.Space(10);
			}

			if(GUILayout.Button("Add Hit"))
			{
				HitConfigs.Add(new HitConfig
				{
					Timing = new Vector2(0.5f, 0.6f),
					DamageMultiplier = 1
				});
			}

			_animationPreview.CalculateTimeFromChanges();
			_animationPreview.Draw();

			GUILayout.Space(40);

			if(EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(Selection.activeObject);
			}
		}

		private void DrawSelectColliders(HitConfig hitConfig)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Colliders involved: ");
			for(var i = 0; i < 3; i++)
			{
				hitConfig.InvolvedColliders[i] = GUILayout.Toggle(hitConfig.InvolvedColliders[i], $"{i}");
			}
			GUILayout.EndHorizontal();
		}

		private Vector2 DrawTimingSliderMinMax(string label, Vector2 value)
		{
			var fps = Animation.Clip ? Animation.Clip.frameRate : 60f;

			GUILayout.Label(label);
			_animationPreview.RegisterTimeBefore(value.x);
			_animationPreview.RegisterTimeBefore(value.y);
			var denormalizedTiming = value * Duration * fps;
			var framedTiming = SirenixEditorFields.MinMaxSlider(denormalizedTiming, new Vector2(0, Duration * fps), false);
			var result = framedTiming.Round(1) / Duration / fps;
			_animationPreview.RegisterTimeAfter(result.x);
			_animationPreview.RegisterTimeAfter(result.y);
			return result;
		}

		private float DrawTimingSlider(string label, float value)
		{
			var fps = Animation.Clip ? Animation.Clip.frameRate : 60f;

			GUILayout.Label(label);
			_animationPreview.RegisterTimeBefore(value);
			var denormalizedTiming = value * Duration * fps;
			var framedTiming = EditorGUILayout.Slider(denormalizedTiming, 0, Duration * fps);
			var result = framedTiming.Round(1) / Duration / fps;
			_animationPreview.RegisterTimeAfter(result);
			return result;
		}
#endif
	}
}
