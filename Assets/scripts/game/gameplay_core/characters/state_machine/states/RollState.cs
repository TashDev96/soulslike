using Animancer;
using dream_lib.src.extensions;
using game.gameplay_core.characters.logic;

namespace game.gameplay_core.characters.state_machine.states
{
	public class RollState : CharacterAnimationStateBase
	{
		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		private RollConfig _config;
		
		public RollState(CharacterContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			base.OnEnter();
			IsComplete = false;

			_config = _context.Config.Roll;
			var animation = _config.RollAnimation;

			Time = 0;
			Duration = animation.length;
			
			ResetForwardMovement();

			_context.Animator.Play(animation, 0.1f, FadeMode.FromStart);
		}
		
		

		public override void Update(float deltaTime)
		{
			Time += deltaTime;
			
			if(_context.InputData.HasDirectionInput && !_config.RotationDisabledTime.Contains(NormalizedTime))
			{
				_context.MovementLogic.RotateCharacter(_context.InputData.DirectionWorld, deltaTime);
			}

			IsReadyToRememberNextCommand = NormalizedTime > 0.3f;

			if(TimeLeft <= 0f)
			{
				IsComplete = true;
			}

			UpdateForwardMovement(_config.ForwardMovement.Evaluate(Time));

			
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
