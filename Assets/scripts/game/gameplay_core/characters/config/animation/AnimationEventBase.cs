using System;
using dream_lib.src.extensions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.characters.config.animation
{
	[Serializable]
	public class AnimationEventBase
	{
		public string Name;
		[FoldoutGroup("Internal"), PropertyOrder(1000)]
		public float StartTimeNormalized;
		[FoldoutGroup("Internal"), PropertyOrder(1000)]
		public float EndTimeNormalized;
		[FoldoutGroup("Internal"), PropertyOrder(1000)]
		public int LayerIndex;

		[HideInInspector]
		public float ClipDuration;

		[OnInspectorGUI]
		private void OnGui()
		{
			var denormalizeMult = ClipDuration * AnimationConfig.EditorTimelineFps;
			EditorGUILayout.LabelField($"Frames: {(StartTimeNormalized * denormalizeMult).Round(1)} - {(EndTimeNormalized * denormalizeMult).Round(1)}");
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
