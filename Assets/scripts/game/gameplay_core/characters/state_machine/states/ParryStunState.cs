using Animancer;

namespace game.gameplay_core.characters.state_machine.states
{
	public class ParryStunState : CharacterAnimationStateBase
	{
		private float _stunDuration;
		private float _stunTimeLeft;
		private AnimancerState _animation;

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		public override bool CanInterruptByStagger => false;
		public bool CanReceiveRiposte { get; private set; }

		public ParryStunState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = false;
		}

		public override void OnEnter()
		{
			base.OnEnter();

			_stunDuration = 2.0f;
			_stunTimeLeft = _stunDuration;
			CanReceiveRiposte = true;

			PlayStunAnimation();
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			_stunTimeLeft -= deltaTime;

			if(_animation != null && _animation.NormalizedTime >= 0.9999f)
			{
				IsComplete = true;
			}
			else if(_stunTimeLeft <= 0)
			{
				IsComplete = true;
			}
		}

		public void OnRiposteReceived()
		{
			CanReceiveRiposte = false;
		}

		public override string GetDebugString()
		{
			return $"Stun time left: {_stunTimeLeft:F2}, Can riposte: {CanReceiveRiposte}";
		}

		private void PlayStunAnimation()
		{
			var animation = _context.Config.ParryStunAnimation;
			if(animation != null)
			{
				_animation = _context.Animator.Play(animation, 0.1f, FadeMode.FromStart);
				Duration = _animation.Length;
			}
		}
	}
}
