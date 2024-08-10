namespace game.gameplay_core.characters.state_machine.states
{
	public class StaggerState : CharacterStateBase
	{
		private float _timeLeft;

		public StaggerState(CharacterContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			base.OnEnter();
			IsComplete = false;
			_timeLeft = 2f;
			_context.Animator.Play(_context.Config.StaggerAnimation, 0.1f);
		}

		public override void Update(float deltaTime)
		{
			_timeLeft -= deltaTime;
			if(_timeLeft <= 0f)
			{
				IsComplete = true;
			}
		}
	}
}
