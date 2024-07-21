using UnityEngine;

namespace game.gameplay_core.characters.state_machine
{
	public class WalkState : BaseCharacterState
	{
		public WalkState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
			IsComplete = true;
		}

		public override void Update(float deltaTime)
		{
			var inputWorld = _context.InputData.DirectionWorld;

			var targetRotation = Quaternion.LookRotation(inputWorld);
			_context.Transform.rotation = Quaternion.RotateTowards(_context.Transform.rotation, targetRotation, _context.RotationSpeed.Value * deltaTime);

			var directionMultiplier = Mathf.Clamp01(Vector3.Dot(_context.Transform.forward, inputWorld));
			var velocity = inputWorld * directionMultiplier * _context.WalkSpeed.Value;

			_context.MovementController.Move(velocity * deltaTime);
		}

		public override bool IsContinuousForCommand(CharacterCommand command)
		{
			return command == CharacterCommand.Walk;
		}
	}
}
