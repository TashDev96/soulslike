using System;
using dream_lib.src.utils.editor;
using game.editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.characters.config
{
	[Serializable]
	public class RollConfig
	{
		[field: SerializeField]
		public AnimationClip ForwardAnimation { get; private set; }
		[field: SerializeField]
		public AnimationClip BackwardAnimation { get; private set; }
		[field: SerializeField]
		public AnimationClip RightAnimation { get; private set; }
		[field: SerializeField]
		public AnimationClip LeftAnimation { get; private set; }

		[field: SerializeField]
		[field: ReadOnly]
		public Vector2 RollInvulnerabilityTiming { get; private set; }
		[field: SerializeField]
		[field: ReadOnly]
		public Vector2 BodyAttackTiming { get; private set; }
		[field: SerializeField]
		[field: ReadOnly]
		public Vector2 ExitToRollAttackTiming { get; private set; }

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
				_animationPreview = new PreviewAnimationDrawer(AddressableAssetNames.Player, ForwardAnimation);
			}

			EditorGUI.BeginChangeCheck();

			_animationPreview.ClearTimeChanges();
			RollInvulnerabilityTiming = CharacterInspectorUtils.DrawTimingSliderMinMax("Roll Invulnerability Timing:", RollInvulnerabilityTiming, ForwardAnimation, _animationPreview);
			BodyAttackTiming = CharacterInspectorUtils.DrawTimingSliderMinMax("Roll Body Attack Timing:", BodyAttackTiming, ForwardAnimation, _animationPreview);
			ExitToRollAttackTiming = CharacterInspectorUtils.DrawTimingSliderMinMax("Exit To Attack Animation Timing:", ExitToRollAttackTiming, ForwardAnimation, _animationPreview);

			RotationDisabledTime = CharacterInspectorUtils.DrawTimingSliderMinMax("Rotation Disabled Time:", RotationDisabledTime, ForwardAnimation, _animationPreview);
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
