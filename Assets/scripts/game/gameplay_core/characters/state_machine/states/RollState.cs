using Animancer;
using dream_lib.src.extensions;
using game.gameplay_core.characters.logic;

namespace game.gameplay_core.characters.state_machine.states
{
	public class RollState : CharacterStateBase
	{
		private float _duration;
		public float Time { get; private set; }

		private float NormalizedTime => Time / _duration;
		private float TimeLeft => _duration - Time;

		public RollState(CharacterContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			base.OnEnter();
			IsComplete = false;
			var animation = _context.Config.Roll.RollAnimation;

			Time = 0;
			_duration = animation.length;

			_context.Animator.Play(animation, 0.1f, FadeMode.FromStart);
		}

		public override void Update(float deltaTime)
		{
			Time += deltaTime;

			if(TimeLeft <= 0f)
			{
				IsComplete = true;
			}

			if(_context.Config.Roll.RollInvulnerabilityTiming.Contains(NormalizedTime))
			{
				_context.InvulnerabilityLogic.SetInvulnerability(InvulnerabilityReason.Roll, true);
			}
			else
			{
				_context.InvulnerabilityLogic.SetInvulnerability(InvulnerabilityReason.Roll, false);
			}
		}
	}
}
