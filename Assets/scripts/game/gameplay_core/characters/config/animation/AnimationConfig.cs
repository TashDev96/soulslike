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
#if UNITY_EDITOR
		public string WeaponForPreview;
#endif

		public AnimationClip Clip;
		[FoldoutGroup("Clip Settings")]
		public float Speed = 1;

		public List<AnimationFlagEvent> FlagEvents = new();
		public List<AnimationEventHit> HitEvents = new();
		public List<string> LayerNames = new() { "Default" };

		public int MaxFrame => (Duration * EditorPrecisionFps).RoundToInt();

		public float Duration => Clip ? Clip.length / Speed : 1;

		public int PercentToFrame(float percent)
		{
			return (percent * Duration * EditorPrecisionFps).RoundToInt();
		}

		public bool HasFlag(AnimationFlagEvent.AnimationFlags flag, float timeNormalized)
		{
			foreach(var evt in FlagEvents)
			{
				if(evt.Flag == flag)
				{
					if(timeNormalized >= evt.StartTimeNormalized && timeNormalized <= evt.EndTimeNormalized)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool GetFlagTime(AnimationFlagEvent.AnimationFlags flag, float timeNormalized, out float start, out float end)
		{
			foreach(var evt in FlagEvents)
			{
				if(evt.Flag == flag)
				{
					if(timeNormalized >= evt.StartTimeNormalized && timeNormalized <= evt.EndTimeNormalized)
					{
						start = evt.StartTimeNormalized;
						end = evt.EndTimeNormalized;
						return true;
					}
				}
			}
			start = 0;
			end = 0;
			return false;
		}

		public bool GetEventRange(AnimationFlagEvent.AnimationFlags flag, out float start, out float end)
		{
			foreach(var evt in FlagEvents)
			{
				if(evt.Flag == flag)
				{
					start = evt.StartTimeNormalized;
					end = evt.EndTimeNormalized;
					return true;
				}
			}
			start = 0;
			end = 0;
			return false;
		}

		public float? GetMarkerTime(AnimationFlagEvent.AnimationFlags flag)
		{
			foreach(var evt in FlagEvents)
			{
				if(evt.Flag == flag)
				{
					return evt.StartTimeNormalized;
				}
			}
			return null;
		}

		public IEnumerable<AnimationEventHit> GetHitEvents()
		{
			return HitEvents;
		}
	}
}
