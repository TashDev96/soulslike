using Animancer;

namespace game.gameplay_core.characters.state_machine.states
{
	public class Roll : CharacterStateBase
	{
		private float _timeLeft;

		public Roll(CharacterContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			base.OnEnter();
			IsComplete = false;
			var animation = _context.Config.RollAnimation;

			_timeLeft = animation.length;
			_context.Animator.Play(animation, 0.1f, FadeMode.FromStart);
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
