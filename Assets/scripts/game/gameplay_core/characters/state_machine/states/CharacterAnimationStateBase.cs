using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public abstract class CharacterAnimationStateBase : CharacterStateBase
	{
		public abstract float Time { get; protected set; }
		protected float NormalizedTime => Time / Duration;
		protected float TimeLeft => Duration - Time;
		protected abstract float Duration { get; set; }

		private float _forwardMovementDone;

		protected CharacterAnimationStateBase(CharacterContext context) : base(context)
		{
		}

		protected void ResetForwardMovement(float initialValue = 0f)
		{
			_forwardMovementDone = initialValue;
		}

		protected void UpdateForwardMovement(float currentForwardDistance)
		{
			_context.MovementLogic.Move(_context.Transform.Forward * (currentForwardDistance - _forwardMovementDone));
			_forwardMovementDone = currentForwardDistance;
		}
		
		protected void UpdateForwardMovement(float currentForwardDistance, Vector3 overrideDirection)
		{
			_context.MovementLogic.Move(overrideDirection * (currentForwardDistance - _forwardMovementDone));
			_forwardMovementDone = currentForwardDistance;
		}
	}
}
