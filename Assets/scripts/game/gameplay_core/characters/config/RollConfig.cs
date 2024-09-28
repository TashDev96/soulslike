using System;
using dream_lib.src.utils.editor;
using game.editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.characters
{
	[Serializable]
	public class RollConfig
	{
		[field: SerializeField]
		public AnimationClip RollAnimation { get; private set; }

		[field: SerializeField]
		[field: ReadOnly]
		public Vector2 RollInvulnerabilityTiming { get; private set; }
		
		[field: SerializeField]
		[field: ReadOnly]
		public Vector2 RotationDisabledTime { get; private set; }

		[field: SerializeField]
		public AnimationCurve ForwardMovement { get; set; }

#if UNITY_EDITOR
		private PreviewAnimationDrawer _animationPreview;

		[OnInspectorGUI]
		private void DrawCustomHitsInspector()
		{
			if(_animationPreview == null)
			{
				_animationPreview = new PreviewAnimationDrawer(AddressableAssetNames.Player, RollAnimation);
			}

			EditorGUI.BeginChangeCheck();

			_animationPreview.ClearTimeChanges();
			var oldValue = RollInvulnerabilityTiming;
			RollInvulnerabilityTiming = CharacterInspectorUtils.DrawTimingSliderMinMax("Roll Invulnerability Timing:", RollInvulnerabilityTiming, RollAnimation, _animationPreview);
			RotationDisabledTime = CharacterInspectorUtils.DrawTimingSliderMinMax("Rotation Disabled Time:", RotationDisabledTime, RollAnimation, _animationPreview);
			if(oldValue != RollInvulnerabilityTiming)
			{
				_animationPreview.Clip = RollAnimation;
			}
			_animationPreview.CalculateTimeFromChanges();
			_animationPreview.Draw();
			
			
			if(EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(Selection.activeObject);
			}
		}

#endif
	}
}
