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

		//I don't want single polymorphic list, for optimization and code simplicity purpose, even if there is some boilerplate, it's ok
		public List<AnimationFlagEvent> FlagEvents = new();
		public List<AnimationEventHit> HitEvents = new();
		public List<AnimationEventSound> SoundEvents = new();
		public List<AnimationEventCameraShake> CameraShakeEvents = new();
		public List<string> LayerNames = new() { "Default" };

		public int MaxFrame => (Duration * EditorPrecisionFps).RoundToInt();

		public float Duration => Clip ? Clip.length / Speed : 1;

		public int PercentToFrame(float percent)
		{
			return (percent * Duration * EditorPrecisionFps).RoundToInt();
		}

		public bool HasFlag(AnimationFlags flag, float timeNormalized)
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

		public bool CheckFlagBegin(AnimationFlags flag, float startTime, float endTime)
		{
			foreach(var evt in FlagEvents)
			{
				if(evt.Flag == flag && evt.StartTimeNormalized >= startTime && evt.StartTimeNormalized <= endTime)
				{
					return true;
				}
			}
			return false;
		}

		public bool CheckFlagEnded(AnimationFlags flag, float startTime, float endTime)
		{
			foreach(var evt in FlagEvents)
			{
				if(evt.Flag == flag && evt.EndTimeNormalized > startTime && evt.EndTimeNormalized <= endTime)
				{
					return true;
				}
			}
			return false;
		}

		public bool CheckSoundBegin(float startTime, float endTime, out string soundName, out float normalizedHearDistance)
		{
			foreach(var evt in SoundEvents)
			{
				if(evt.StartTimeNormalized >= startTime && evt.StartTimeNormalized <= endTime)
				{
					soundName = evt.SoundName;
					normalizedHearDistance = evt.NormalizedHearDistance;
					return true;
				}
			}
			soundName = null;
			normalizedHearDistance = 0;
			return false;
		}

		public bool CheckCameraShakeBegin(float startTime, float endTime, out float duration, out float strength, out float vertMultiplier, out float horMultiplier)
		{
			foreach(var evt in CameraShakeEvents)
			{
				if(evt.StartTimeNormalized >= startTime && evt.StartTimeNormalized <= endTime)
				{
					duration = (evt.EndTimeNormalized - evt.StartTimeNormalized) * Duration;
					strength = evt.Strength;
					vertMultiplier = evt.VertMultiplier;
					horMultiplier = evt.HorMultiplier;
					return true;
				}
			}
			duration = 0;
			strength = 0;
			vertMultiplier = 0;
			horMultiplier = 0;
			return false;
		}

		public bool GetFlagTime(AnimationFlags flag, float timeNormalized, out float start, out float end)
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

		public bool GetEventRange(AnimationFlags flag, out float start, out float end)
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

		public float? GetMarkerTime(AnimationFlags flag)
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

		public AnimationFlagEvent GetFlag(AnimationFlags flag)
		{
			foreach(var animationFlagEvent in FlagEvents)
			{
				if(animationFlagEvent.Flag == flag)
				{
					return animationFlagEvent;
				}
			}
			return null;
		}
	}
}
