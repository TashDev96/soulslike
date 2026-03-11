using Animancer;

namespace game.gameplay_core.characters.state_machine.states
{
	public class DeadState : CharacterStateBase
	{
		private readonly bool _immediate;

		public DeadState(CharacterContext context, bool immediate = false) : base(context)
		{
			_immediate = immediate;
			context.IsDead.Value = true;
		}

		public override void Update(float deltaTime)
		{
		}

		public override void OnEnter()
		{
			if(_immediate)
			{
				var anim = _context.Animator.Play(_context.Config.DeathAnimation, 0.1f, FadeMode.FromStart);
				anim.NormalizedTime = 1f;
			}
			else
			{
				_context.Animator.Play(_context.Config.DeathAnimation, 0.1f, FadeMode.FromStart);
			}
			_context.DeadStateRoot.SetActive(true);
		}

		public override void OnExit()
		{
			_context.Animator.Play(_context.Config.IdleAnimation, 0.1f, FadeMode.FromStart);
			_context.DeadStateRoot.SetActive(false);
		}
	}
}
