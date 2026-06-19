using System;
using Animancer;
using dream_lib.src.extensions;
using game.gameplay_core.characters.config.animation;
using game.gameplay_core.location;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine
{
	public class AnimationConfigPlayer
	{
		private const string RotationLockKey = "by_animation";

		private readonly CharacterContext _context;
		private AnimancerState _animancerState;

		public AnimationConfig AnimationConfig { get; private set; }

		public float Time { get; private set; }
		public float Duration { get; private set; }
		public float NormalizedAnimationTime => Time % Duration / Duration;
		public AnimationConfig NextAnimation { get; set; }

		public AnimationConfigPlayer(CharacterContext context)
		{
			_context = context;
		}

		public AnimancerState Play(AnimationConfig config)
		{
			AnimationConfig = config;
			Time = 0;
			Duration = Mathf.Max(config.Duration, 0.1f);
			_animancerState =_context.Views.Animator.Play(config.Clip, 0.1f, FadeMode.FromStart);
			return _animancerState;
		}
		
		public bool TryPlay(AnimationConfig config)
		{
			if(AnimationConfig == config)
			{
				return false;
			}
			AnimationConfig = config;
			Time = 0;
			Duration = Mathf.Max(config.Duration, 0.1f);
			_context.Views.Animator.Play(config.Clip, 0.1f, FadeMode.FromStart);
			return true;
		}

		public void Update(float deltaTime)
		{
			if(Duration == 0)
			{
				throw new Exception($"duration not set for {GetType().Name} of {_context.SelfLink.transform.GetFullPathInScene()}");
			}
			var previousNormalizedTime = NormalizedAnimationTime;
			Time += deltaTime;
			if(AnimationConfig != null)
			{
				var rotationDisabled = AnimationConfig.HasFlag(AnimationFlags.RotationLocked, NormalizedAnimationTime);
				if(rotationDisabled)
				{
					if(AnimationConfig.CheckFlagBegin(AnimationFlags.RotationLocked, previousNormalizedTime, NormalizedAnimationTime))
					{
						_context.Logic.MovementLogic.SetRotationLockedBy(RotationLockKey, true);
					}
				}
				else if(AnimationConfig.CheckFlagEnded(AnimationFlags.RotationLocked, previousNormalizedTime, NormalizedAnimationTime))
				{
					_context.Logic.MovementLogic.SetRotationLockedBy(RotationLockKey, false);
				}

				if(AnimationConfig.CheckSoundBegin(previousNormalizedTime, NormalizedAnimationTime, out var soundName, out var hearDistance))
				{
					_context.Events.EmitNoise.Execute(hearDistance);
				}
				if(AnimationConfig.CheckCameraShakeBegin(previousNormalizedTime, NormalizedAnimationTime, out var duration, out var strength, out var vertMultiplier, out var horMultiplier))
				{
					LocationStaticContext.Instance.CameraController.Shake(duration, strength, vertMultiplier, horMultiplier);
				}
			}

			if(Time >= Duration && NextAnimation != null)
			{
				Play(NextAnimation);
				NextAnimation = null;
			}
		}

		public bool CheckTiming(Vector2 timing)
		{
			return timing.Contains(NormalizedAnimationTime);
		}

		public void SetTime(float time)
		{
			Time = time;
			_animancerState.Time = time;
		}
	}
}
