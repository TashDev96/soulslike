using System;
using Animancer;

namespace game.gameplay_core.characters.state_machine.states
{
	public class StaggerState : CharacterStateBase
	{
		private readonly CharacterStateBase _previousState;
		private readonly StaggerReason _staggerReason;
		private float _timeLeft;
		private AnimancerState _animation;

		public StaggerState(CharacterContext context, CharacterStateBase previousState, StaggerReason staggerReason) : base(context)
		{
			_previousState = previousState;
			_staggerReason = staggerReason;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			IsComplete = false;

			PlayStaggerAnimation();
		}

		public override void Update(float deltaTime)
		{
			if(_animation.NormalizedTime >= 0.9999f)
			{
				IsComplete = true;
			}
		}

		private void PlayStaggerAnimation()
		{
			var animation = _context.Config.StaggerAnimation;

			switch(_staggerReason)
			{
				case StaggerReason.Poise:
					animation = _context.Config.StaggerAnimation;
					break;
				case StaggerReason.BlockBreak:
					if(_previousState is BlockStateBase blockStateBase)
					{
						_animation = _context.Animator.Play(blockStateBase.BlockingWeapon.Config.BlockBreakAnimation, 0.1f, FadeMode.FromStart);
						return;
					}
					break;
				case StaggerReason.Fall:
					animation = _context.Config.StaggerAnimation;

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			_animation = _context.Animator.Play(animation, 0.1f, FadeMode.FromStart);
		}
	}
}
