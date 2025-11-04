using System;
using Animancer;
using dream_lib.src.utils.editor;
using game.editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.inventory.item_configs
{
	[Serializable]
	public class ItemAnimationConfig
	{
		public bool DisableRightHandWeapon { get; private set; }

		[field: SerializeField]
		public ClipTransition Animation { get; private set; }

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
		public float ApplyEffectTiming { get; private set; }

#if UNITY_EDITOR

		private PreviewAnimationDrawer _animationPreview;

		[OnInspectorGUI]
		private void DrawCustomHitsInspector()
		{
			if(_animationPreview == null || _animationPreview.Clip != Animation.Clip)
			{
				_animationPreview = new PreviewAnimationDrawer(AddressableAssetNames.Player, Animation.Clip);
			}

			RotationDisabledTime = CharacterInspectorUtils.DrawTimingSliderMinMax("Rotation Disabled Time:", RotationDisabledTime, Animation.Clip, _animationPreview);
			StaminaRegenDisabledTime = CharacterInspectorUtils.DrawTimingSliderMinMax("Stamina Regen Disabled Time:", StaminaRegenDisabledTime, Animation.Clip, _animationPreview);
			LockedStateTime = CharacterInspectorUtils.DrawTimingSliderMinMax("Locked State Time:", LockedStateTime, Animation.Clip, _animationPreview);
			ApplyEffectTiming = CharacterInspectorUtils.DrawTimingSlider("Apply Effect Time:", ApplyEffectTiming, Animation.Clip, _animationPreview);

			_animationPreview.CalculateTimeFromChanges();
			_animationPreview.Draw();

			GUILayout.Space(40);

			if(EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(Selection.activeObject);
			}
		}

#endif
	}
}
