using System;
using System.Collections.Generic;
using dream_lib.src.extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.config.animation
{
	[Serializable]
	public class AnimationConfig
	{
		public const float EditorPrecisionFps = 200;
		public const float EditorTimelineFps = 60;

		public AnimationClip Clip;
		[FoldoutGroup("Clip Settings")]
		public float Speed = 1;

		[HideInInspector]
		[SerializeReference]
		public List<AnimationEventBase> Events = new();
		public List<string> LayerNames = new() { "Default" };

		public int MaxFrame => (Duration * EditorPrecisionFps).RoundToInt();

		public float Duration => Clip ? Clip.length / Speed : 1;

		public int PercentToFrame(float percent)
		{
			return (percent * Duration * EditorPrecisionFps).RoundToInt();
		}
	}
}
