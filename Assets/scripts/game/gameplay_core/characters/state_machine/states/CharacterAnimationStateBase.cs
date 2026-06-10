using System;
using Animancer;
using dream_lib.src.extensions;
using game.gameplay_core.characters.config.animation;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public abstract class CharacterAnimationStateBase : CharacterStateBase
	{
		private const string RotationLockKey = "by_animation";

		protected AnimationConfig AnimationConfig { get; private set; }
		private float _forwardMovementDone;

		private bool _legacyMode;

		private readonly AnimationConfigPlayer _player;
		public abstract float Time { get; protected set; }
		protected float NormalizedTime => Time / Duration;
		protected float NormalizedAnimationTime => Time % Duration / Duration;
		protected float TimeLeft => Duration - Time;
		protected float Duration { get; private set; }

		protected CharacterAnimationStateBase(CharacterContext context) : base(context)
		{
			_player = new AnimationConfigPlayer(context);
		}

		public override void OnEnter()
		{
			Time = 0;
			base.OnEnter();
		}

		public override void OnExit()
		{
			_context.Logic.MovementLogic.SetRotationLockedBy(RotationLockKey, false);
			base.OnExit();
		}

		public override void Update(float deltaTime)
		{
			if(Duration == 0)
			{
				throw new Exception($"duration not set for {GetType().Name} of {_context.SelfLink.transform.GetFullPathInScene()}");
			}

			if(_legacyMode)
			{
				Time += deltaTime;
				return;
			}
			_player.Update(deltaTime);
			Time = _player.Time;
		}

		public override string GetDebugString()
		{
			return $"{Time.RoundFormat()}/{Duration.RoundFormat()}";
		}

		protected AnimancerState Play(AnimationConfig config)
		{
			AnimationConfig = config;
			Duration = config.Duration;
			_legacyMode = false;
			return _player.Play(config);
		}

		protected AnimancerState PlayLegacy(ClipTransition transition)
		{
			Duration = transition.Length;
			if(Duration == 0)
			{
				Duration = 0.1f;
			}
			_legacyMode = true;
			return _context.Views.Animator.Play(transition, 0.1f, FadeMode.FromStart);
		}
		
		protected AnimancerState PlayLegacy(AnimationClip clip)
		{
			Duration = clip.length;
			if(Duration == 0)
			{
				Duration = 0.1f;
			}
			_legacyMode = true;
			return _context.Views.Animator.Play(clip, 0.1f, FadeMode.FromStart);
		}

		protected void RecalculateFlagsImmediate()
		{
			var rotationDisabled = AnimationConfig.HasFlag(AnimationFlags.RotationLocked, NormalizedAnimationTime);
			_context.Logic.MovementLogic.SetRotationLockedBy(RotationLockKey, rotationDisabled);
		}

		protected void ResetForwardMovement(float initialValue = 0f)
		{
			_forwardMovementDone = initialValue;
		}

		protected void UpdateForwardMovement(float currentForwardDistance, float deltaTime)
		{
			_context.Logic.MovementLogic.ApplyLocomotion(_context.Transform.Forward * (currentForwardDistance - _forwardMovementDone), deltaTime);
			_forwardMovementDone = currentForwardDistance;
		}

		protected void UpdateForwardMovement(float currentForwardDistance, Vector3 overrideDirection, float deltaTime)
		{
			_context.Logic.MovementLogic.ApplyLocomotion(overrideDirection * (currentForwardDistance - _forwardMovementDone), deltaTime);
			_forwardMovementDone = currentForwardDistance;
		}

		protected bool CheckTiming(Vector2 timing)
		{
			return _player.CheckTiming(timing);
		}
	}
}
