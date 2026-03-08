using dream_lib.src.extensions;
using game.gameplay_core.characters.config.animation;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public abstract class CharacterAnimationStateBase : CharacterStateBase
	{
		protected AnimationConfig AnimationConfig;
		private float _forwardMovementDone;
		public abstract float Time { get; protected set; }
		protected float NormalizedTime => Time / Duration;
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

		public override void Update(float deltaTime)
		{
			var previousNormalizedTime = NormalizedTime;
			Time += deltaTime;
			if(AnimationConfig != null)
			{
				if(AnimationConfig.CheckSoundBegin(previousNormalizedTime, NormalizedTime, out var soundName, out var hearDistance))
				{
					EmitNoise(hearDistance);
				}
			}
		}

		public override string GetDebugString()
		{
			return $"{Time.RoundFormat()}/{Duration.RoundFormat()}";
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
			return timing.Contains(NormalizedTime);
		}
	}
}
