using Animancer;
using game.gameplay_core.characters.config.animation;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.state_machine.states
{
	public class LockedInAnimationState : CharacterAnimationStateBase
	{
		private AnimancerState _animation;
		private readonly bool _canInterruptByStagger;
		private DamageReceiver[] _damageReceivers;

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }
		public override bool CanInterruptByStagger => _canInterruptByStagger;

		public LockedInAnimationState(CharacterContext context, AnimationConfig animationClip, bool canInterruptByStagger = false) : base(context)
		{
			AnimationConfig = animationClip;
			_canInterruptByStagger = canInterruptByStagger;
			IsReadyToRememberNextCommand = false;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			_context.MovementLogic.ResetVelocity();
			_animation = _context.Animator.Play(AnimationConfig.Clip, 0.1f, FadeMode.FromStart);
			_context.MovementLogic.SetRotationAndMovementLocked(true);
			Duration = AnimationConfig.Duration;
		}

		public override void OnExit()
		{
			_context.MovementLogic.SetRotationAndMovementLocked(false);
			base.OnExit();
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if(Time >= Duration)
			{
				IsComplete = true;
			}
		}

		public override string GetDebugString()
		{
			return $"Locked animation: {Time:F2}/{Duration:F2}  ({NormalizedTime}), Animation: {AnimationConfig.Clip?.name ?? "None"}";
		}
	}
}
