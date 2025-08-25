using Animancer;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.state_machine.states
{
	public class ParryState : CharacterAnimationStateBase
	{
		private float _activeFrameStart;
		private float _activeFrameEnd;
		private float _recoveryFrameEnd;

		private WeaponView _parryWeapon;

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		public override bool CanInterruptByStagger => !IsInRecoveryFrames;
		public bool IsParrySuccessful { get; private set; }

		public bool IsInActiveFrames { get; private set; }

		public bool IsInRecoveryFrames { get; private set; }

		public ParryState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();

			_parryWeapon = _context.LeftWeapon.HasValue ? _context.LeftWeapon.Value : _context.RightWeapon.Value;

			if(_parryWeapon != null)
			{
				_activeFrameStart = _parryWeapon.Config.ParryActiveFrameStart;
				_activeFrameEnd = _parryWeapon.Config.ParryActiveFrameEnd;
				_recoveryFrameEnd = _parryWeapon.Config.ParryRecoveryFrameEnd;

				_parryWeapon.SetBlockColliderActive(true);

				var animation = _parryWeapon.Config.ParryAnimation;
				if(animation != null)
				{
					var animationState = _context.Animator.Play(animation, 0.1f, FadeMode.FromStart);
					Duration = animationState.Duration;
				}
			}

			_context.StaminaLogic.SpendStamina(_parryWeapon?.Config.ParryStaminaCost ?? 15f);
		}

		public override void Update(float deltaTime)
		{
			Time += deltaTime;

			if(Duration > 0)
			{
				var normalizedTime = Time / Duration;

				IsInActiveFrames = normalizedTime >= _activeFrameStart && normalizedTime <= _activeFrameEnd;
				IsInRecoveryFrames = normalizedTime > _activeFrameEnd && normalizedTime <= _recoveryFrameEnd;

				if(normalizedTime >= 1f)
				{
					IsComplete = true;
				}
			}
		}

		public void OnParryFailed()
		{
			IsParrySuccessful = false;

			if(_parryWeapon != null)
			{
				IsComplete = true;
			}
		}

		public override void OnExit()
		{
			if(_parryWeapon != null)
			{
				_parryWeapon.SetBlockColliderActive(false);
			}

			base.OnExit();
		}

		public override float GetEnterStaminaCost()
		{
			return _parryWeapon?.Config.ParryStaminaCost ?? 15f;
		}

		public override string GetDebugString()
		{
			return $"Active: {IsInActiveFrames}, Recovery: {IsInRecoveryFrames}, Success: {IsParrySuccessful}";
		}
	}
}
