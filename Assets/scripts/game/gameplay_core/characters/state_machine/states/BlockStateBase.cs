using Animancer;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public abstract class BlockStateBase : CharacterStateBase
	{
		protected bool _isBlocking;
		protected AnimancerState _receiveHitAnimation;
		public WeaponView BlockingWeapon { get; private set; }

		public override bool CanInterruptByStagger => true;

		protected BlockStateBase(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			IsComplete = false;
			_isBlocking = true;

			BlockingWeapon = _context.LeftWeapon.HasValue ? _context.LeftWeapon.Value : _context.RightWeapon.Value;

			if(BlockingWeapon != null)
			{
				BlockingWeapon.SetBlockColliderActive(true);
				PlayBlockAnimation();
			}

			_context.BlockLogic.OnBlockTriggered.OnExecute += HandleBlockTriggered;
		}

		public override void OnExit()
		{
			_isBlocking = false;

			if(BlockingWeapon != null)
			{
				BlockingWeapon.SetBlockColliderActive(false);
			}

			_context.BlockLogic.OnBlockTriggered.OnExecute -= HandleBlockTriggered;

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
			_receiveHitAnimation = _context.Animator.Play(BlockingWeapon.Config.BlockHitAnimation);
		}
	}
}
