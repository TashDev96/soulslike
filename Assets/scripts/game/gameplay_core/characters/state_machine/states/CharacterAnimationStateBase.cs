using System;
using dream_lib.src.extensions;
using game.gameplay_core.characters.config.animation;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public abstract class CharacterAnimationStateBase : CharacterStateBase
	{
		private const string RotationLockKey = "by_animation";

		protected AnimationConfig AnimationConfig;
		private float _forwardMovementDone;
		public abstract float Time { get; protected set; }
		protected float NormalizedTime => Time / Duration;
		protected float NormalizedAnimationTime => Time % Duration / Duration;
		protected float TimeLeft => Duration - Time;
		protected abstract float Duration { get; set; }

		protected CharacterAnimationStateBase(CharacterContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			Time = 0;
			base.OnEnter();
		}

		public override void OnExit()
		{
			_context.MovementLogic.SetRotationLockedBy(RotationLockKey, false);
			base.OnExit();
		}

		public override void Update(float deltaTime)
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
						_context.MovementLogic.SetRotationLockedBy(RotationLockKey, true);
					}
				}
				else if(AnimationConfig.CheckFlagEnded(AnimationFlags.RotationLocked, previousNormalizedTime, NormalizedAnimationTime))
				{
					_context.MovementLogic.SetRotationLockedBy(RotationLockKey, false);
				}

				if(AnimationConfig.CheckSoundBegin(previousNormalizedTime, NormalizedAnimationTime, out var soundName, out var hearDistance))
				{
					EmitNoise(hearDistance);
				}
				if(AnimationConfig.CheckCameraShakeBegin(previousNormalizedTime, NormalizedAnimationTime, out var duration, out var strength, out var vertMultiplier, out var horMultiplier))
				{
					LocationStaticContext.Instance.CameraController.Shake(duration, strength, vertMultiplier, horMultiplier);
				}
			}
		}

		public override string GetDebugString()
		{
			return $"{Time.RoundFormat()}/{Duration.RoundFormat()}";
		}

		protected void RecalculateFlagsImmediate()
		{
			var rotationDisabled = AnimationConfig.HasFlag(AnimationFlags.RotationLocked, NormalizedAnimationTime);
			_context.MovementLogic.SetRotationLockedBy(RotationLockKey, rotationDisabled);
		}

		protected void ResetForwardMovement(float initialValue = 0f)
		{
			_forwardMovementDone = initialValue;
		}

		protected void UpdateForwardMovement(float currentForwardDistance, float deltaTime)
		{
			_context.MovementLogic.ApplyLocomotion(_context.Transform.Forward * (currentForwardDistance - _forwardMovementDone), deltaTime);
			_forwardMovementDone = currentForwardDistance;
		}

		protected void UpdateForwardMovement(float currentForwardDistance, Vector3 overrideDirection, float deltaTime)
		{
			_context.MovementLogic.ApplyLocomotion(overrideDirection * (currentForwardDistance - _forwardMovementDone), deltaTime);
			_forwardMovementDone = currentForwardDistance;
		}

		protected bool CheckTiming(Vector2 timing)
		{
			return timing.Contains(NormalizedAnimationTime);
		}
	}
}
