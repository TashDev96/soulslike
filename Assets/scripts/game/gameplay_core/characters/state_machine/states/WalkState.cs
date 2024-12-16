using game.gameplay_core.characters.commands;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public class WalkState : CharacterStateBase
	{
		public WalkState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			IsComplete = false;
			_context.Animator.Play(_context.Config.IdleAnimation, 0.3f);
		}

		public override bool TryContinueWithCommand(CharacterCommand nextCommand)
		{
			switch(nextCommand)
			{
				case CharacterCommand.Walk:
				case CharacterCommand.Run:
					IsComplete = false;
					return true;
				default:
					return false;
			}
		}

		public override void Update(float deltaTime)
		{
			var inputWorld = _context.InputData.DirectionWorld;

			if(!_context.LockOnLogic.LockOnTarget.HasValue)
			{
				_context.MovementLogic.RotateCharacter(inputWorld, deltaTime);
			}

			var directionMultiplier = Mathf.Clamp01(Vector3.Dot(_context.Transform.Forward, inputWorld));
			if(_context.LockOnLogic.LockOnTarget.HasValue)
			{
				directionMultiplier = 1;
			}

			var speed = _context.InputData.Command == CharacterCommand.Run ? _context.RunSpeed.Value : _context.WalkSpeed.Value;
			var velocity = inputWorld * (directionMultiplier * speed);

			_context.MovementLogic.Walk(velocity * deltaTime, deltaTime);

			IsComplete = true;
		}
	}
}
