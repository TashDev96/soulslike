using game.gameplay_core.characters.commands;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public class WalkState : CharacterStateBase
	{
		public WalkState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
			IsComplete = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			_context.Animator.Play(_context.Config.IdleAnimation, 0.3f);
		}

		public override void Update(float deltaTime)
		{
			var inputWorld = _context.InputData.DirectionWorld;

			RotateCharacter(inputWorld, _context.RotationSpeed.Value.DegreesPerSecond, deltaTime);

			var directionMultiplier = Mathf.Clamp01(Vector3.Dot(_context.Transform.forward, inputWorld));
			var velocity = inputWorld * (directionMultiplier * _context.WalkSpeed.Value);

			_context.MovementLogic.Move(velocity * deltaTime);
		}

		public override bool IsContinuousForCommand(CharacterCommand command)
		{
			return command == CharacterCommand.Walk;
		}
	}
}
