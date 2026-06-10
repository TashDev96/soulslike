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
		private readonly AnimationConfig _animConfig;

		public override float Time { get; protected set; }
		public override bool CanInterruptByStagger => _canInterruptByStagger;

		public LockedInAnimationState(CharacterContext context, AnimationConfig animation, bool canInterruptByStagger = false) : base(context)
		{
			_animConfig = animation;
			_canInterruptByStagger = canInterruptByStagger;
			IsReadyToRememberNextCommand = false;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			_context.Logic.MovementLogic.ResetVelocity();
			_context.Logic.MovementLogic.SetRotationAndMovementLocked(true);
			_animation = Play(_animConfig);
		}

		public override void OnExit()
		{
			_context.Logic.MovementLogic.SetRotationAndMovementLocked(false);
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
