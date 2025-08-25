namespace game.gameplay_core.characters.state_machine.states
{
	public class IdleState : CharacterStateBase
	{
		public IdleState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			IsComplete = true;
			_context.Animator.Play(_context.Config.IdleAnimation, 0.2f);
		}

		public override void Update(float deltaTime)
		{
		}
	}
}
