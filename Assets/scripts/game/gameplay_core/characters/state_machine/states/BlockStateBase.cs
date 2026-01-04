using Animancer;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.state_machine.states
{
	public abstract class BlockStateBase : CharacterStateBase
	{
		private const string StaminaRegenKey = nameof(BlockStateBase);

		private const string HitReactReason = "BlockHitReactReason";
		protected bool _isBlocking;
		protected AnimancerState _receiveHitAnimation;
		public WeaponView BlockingWeaponView { get; private set; }

		public override bool CanInterruptByStagger => true;

		protected BlockStateBase(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();

			_context.StaminaLogic.SetStaminaRegenMultiplier(StaminaRegenKey, 0.3f);

			_isBlocking = true;

			var blockingSlot = _context.InventoryLogic.GetBlockingWeaponSlot();
			BlockingWeaponView = _context.EquippedWeaponViews[blockingSlot];

			if(BlockingWeaponView != null)
			{
				BlockingWeaponView.SetBlockColliderActive(true);
				PlayBlockAnimation();
			}

			_context.BlockLogic.OnBlockTriggered.OnExecute += HandleBlockTriggered;
		}

		public override void OnExit()
		{
			_isBlocking = false;

			if(BlockingWeaponView != null)
			{
				BlockingWeaponView.SetBlockColliderActive(false);
			}

			_context.StaminaLogic.RemoveStaminaRegenMultiplier(StaminaRegenKey);

			_context.BlockLogic.OnBlockTriggered.OnExecute -= HandleBlockTriggered;

			_context.StaminaLogic.SetStaminaRegenLock(HitReactReason, false);

			base.OnExit();
		}

		public override void Update(float deltaTime)
		{
			if(_receiveHitAnimation != null)
			{
				if(_receiveHitAnimation.NormalizedTime >= 1)
				{
					PlayBlockAnimation();
					_receiveHitAnimation = null;
					_context.StaminaLogic.SetStaminaRegenLock(HitReactReason, false);
				}
				else
				{
					return;
				}
			}

			if(_isBlocking)
			{
				if(_context.CharacterStats.Stamina.Value <= 0)
				{
					IsComplete = true;
				}
			}
			else
			{
				IsComplete = true;
			}
		}

		public override float GetEnterStaminaCost()
		{
			return 1f;
		}

		protected abstract void PlayBlockAnimation();

		private void HandleBlockTriggered()
		{
			_receiveHitAnimation = _context.Animator.Play(BlockingWeaponView.Config.BlockHitAnimation);
			_context.StaminaLogic.SetStaminaRegenLock(HitReactReason, true);
		}
	}
}
