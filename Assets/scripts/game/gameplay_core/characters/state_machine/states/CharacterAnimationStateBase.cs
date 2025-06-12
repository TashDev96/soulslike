using dream_lib.src.extensions;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public abstract class CharacterAnimationStateBase : CharacterStateBase
	{
		private float _forwardMovementDone;
		public abstract float Time { get; protected set; }
		protected float NormalizedTime => Time / Duration;
		protected float TimeLeft => Duration - Time;
		protected abstract float Duration { get; set; }

		protected CharacterAnimationStateBase(CharacterContext context) : base(context)
		{
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
	}
}
