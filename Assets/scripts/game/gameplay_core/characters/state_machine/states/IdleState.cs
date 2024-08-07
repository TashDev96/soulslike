using UnityEngine;

namespace game.gameplay_core.characters.state_machine
{
	public class IdleState : BaseCharacterState
	{
		public IdleState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
			IsComplete = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			_context.Animator.Play(_context.Config.IdleAnimation);
		}

		public override bool IsContinuousForCommand(CharacterCommand command)
		{
			return command == CharacterCommand.None;
		}

		public override void Update(float deltaTime)
		{
		}
	}
}