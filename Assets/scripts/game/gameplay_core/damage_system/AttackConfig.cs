using System;
using System.Collections.Generic;
using Animancer;
using game.editor;
using game.gameplay_core.inventory.item_configs;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using dream_lib.src.utils.editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif

namespace game.gameplay_core.damage_system
{
	[Serializable]
	public class AttackConfig
	{
		[field: SerializeField]
		public ClipTransition Animation { get; private set; }

		[field: SerializeField]
		public bool IsRangedAttack { get; private set; }

		[field: ValueDropdown("@AddressableAssetNames.ProjectilePrefabs")]
		[field: SerializeField]
		[field: ShowIf(nameof(IsRangedAttack))]
		public string ProjectilePrefabNames { get; private set; }
		[field: SerializeField]
		[field: ShowIf(nameof(IsRangedAttack))]
		public float MaxProjectileHorizontalAngleCorrection { get; private set; }

		[field: SerializeField]
		public float BaseDamage { get; private set; }
		[field: SerializeField]
		public float StaminaCost { get; private set; } = 10;
		[field: SerializeField]
		public int AttackDeflectionRatingBonus { get; private set; }

		[ShowInInspector]
		public float Duration => Animation.Clip ? Animation.Clip.length / Animation.Speed : 0.1f;

		[field: HideInInspector]
		[field: SerializeField]
		public Vector2 RotationDisabledTime { get; private set; }
		[field: HideInInspector]
		[field: SerializeField]
		public Vector2 StaminaRegenDisabledTime { get; private set; }

		[field: HideInInspector]
		[field: SerializeField]
		public Vector2 LockedStateTime { get; private set; } = new(0, 1f);

		[field: HideInInspector]
		[field: SerializeField]
		public Vector2 ExitToComboTime { get; private set; } = new(0, 1f);
		[field: HideInInspector]
		[field: SerializeField]
		public float EnterComboTime { get; private set; }
		[field: HideInInspector]
		[field: SerializeField]
		public float EnterFromRollTime { get; private set; }

		[field: HideInInspector]
		[field: SerializeField]
		public float StartHandleObstacleCastTime { get; set; }

		[field: SerializeField]
		public AnimationCurve ForwardMovement { get; private set; }

		[field: BoxGroup("Ai Data")]
		[field: SerializeField]
		public float Range { get; set; } = 1f;

		[field: SerializeField]

		//[field: HideInInspector]
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

			if(_animationPreview.Clip != Animation.Clip)
			{
				_animationPreview.Clip = Animation.Clip;
			}

			var weaponKey = GetWeaponPrefabKeyFromSelection();
			if(_animationPreview.WeaponPrefabKey != weaponKey)
			{
				_animationPreview.WeaponPrefabKey = weaponKey;
			}

			EditorGUI.BeginChangeCheck();

			_animationPreview.ClearTimeChanges();
			RotationDisabledTime = CharacterInspectorUtils.DrawTimingSliderMinMax("Rotation Disabled Time:", RotationDisabledTime, Animation.Clip, _animationPreview);
			StaminaRegenDisabledTime = CharacterInspectorUtils.DrawTimingSliderMinMax("Stamina Regen Disabled Time:", StaminaRegenDisabledTime, Animation.Clip, _animationPreview);
			LockedStateTime = CharacterInspectorUtils.DrawTimingSliderMinMax("Locked State Time:", LockedStateTime, Animation.Clip, _animationPreview);
			StartHandleObstacleCastTime = CharacterInspectorUtils.DrawTimingSlider("Start Handle Obstacle Cast Time", StartHandleObstacleCastTime, Animation.Clip, _animationPreview);

			EditorGUILayout.Space();
			SirenixEditorGUI.BeginBox("Cross-state Timings");
			ExitToComboTime = CharacterInspectorUtils.DrawTimingSliderMinMax("Exit To Next Combo Time:", ExitToComboTime, Animation.Clip, _animationPreview);
			EnterComboTime = CharacterInspectorUtils.DrawTimingSlider("Enter Combo Time:", EnterComboTime, Animation.Clip, _animationPreview);
			EnterFromRollTime = CharacterInspectorUtils.DrawTimingSlider("Enter From Roll Time:", EnterFromRollTime, Animation.Clip, _animationPreview);
			SirenixEditorGUI.EndBox();

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

				HitConfigs[i].Timing = CharacterInspectorUtils.DrawTimingSliderMinMax("Timing (120 fps):", HitConfigs[i].Timing, Animation.Clip, _animationPreview, 120f);
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

		private string GetWeaponPrefabKeyFromSelection()
		{
			if(Selection.activeObject is WeaponItemConfig weaponConfig)
			{
				return weaponConfig.WeaponPrefabName;
			}
			return null;
		}

#endif
	}
}
