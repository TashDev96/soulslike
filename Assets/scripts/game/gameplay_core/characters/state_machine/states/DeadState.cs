using Animancer;

namespace game.gameplay_core.characters.state_machine.states
{
	public class DeadState : CharacterStateBase
	{
		public DeadState(CharacterContext context) : base(context)
		{
		}

		public override void Update(float deltaTime)
		{
		}

		public override void OnEnter()
		{
			_context.Animator.Play(_context.Config.DeathAnimation, 0.1f, FadeMode.FromStart);
			_context.DeadStateRoot.SetActive(true);
		}

		public override void OnExit()
		{
			_context.Animator.Play(_context.Config.IdleAnimation, 0.1f, FadeMode.FromStart);
			_context.DeadStateRoot.SetActive(false);
		}
	}
}
