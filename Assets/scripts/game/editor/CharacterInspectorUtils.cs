using dream_lib.src.extensions;
using dream_lib.src.utils.editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace game.editor
{
	public static class CharacterInspectorUtils
	{
		public static Vector2 DrawTimingSliderMinMax(string label, Vector2 value, AnimationClip animation, PreviewAnimationDrawer animationPreview = null, float fps = 60f)
		{
			GetAnimationParams(animation, out var duration);

			GUILayout.Label(label);
			animationPreview?.RegisterTimeBefore(value.x);
			animationPreview?.RegisterTimeBefore(value.y);
			var denormalizedTiming = value * duration * fps;
			var framedTiming = SirenixEditorFields.MinMaxSlider(denormalizedTiming, new Vector2(0, duration * fps), false);
			var result = framedTiming.Round(1) / duration / fps;
			animationPreview?.RegisterTimeAfter(result.x);
			animationPreview?.RegisterTimeAfter(result.y);
			return result;
		}

		public static float DrawTimingSlider(string label, float value, AnimationClip animation, PreviewAnimationDrawer animationPreview = null, float fps = 60f)
		{
			GetAnimationParams(animation, out var duration);

			GUILayout.Label(label);
			animationPreview?.RegisterTimeBefore(value);
			var denormalizedTiming = value * duration * fps;
			var framedTiming = EditorGUILayout.Slider(denormalizedTiming, 0, duration * fps);
			var result = framedTiming.Round(1) / duration / fps;
			animationPreview?.RegisterTimeAfter(result);
			return result;
		}

		private static void GetAnimationParams(AnimationClip animation, out float duration)
		{
			duration = animation ? animation.length : 0.1f;
		}
	}
}
