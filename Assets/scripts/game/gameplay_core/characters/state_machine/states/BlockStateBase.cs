using game.gameplay_core.characters.commands;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.state_machine.states
{
	public abstract class BlockStateBase : CharacterStateBase
	{
		protected bool _isBlocking;
		public WeaponView Weapon { get; private set; }

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

			Weapon = _context.LeftWeapon.HasValue ? _context.LeftWeapon.Value : _context.RightWeapon.Value;

			if(Weapon != null)
			{
				_context.Animator.Play(Weapon.Config.BlockStayAnimation, 0.2f);
				Weapon.SetBlockColliderActive(true);
			}

			OnEnterStaminaLogic();
		}

		public override void OnExit()
		{
			_isBlocking = false;

			if(Weapon != null)
			{
				Weapon.SetBlockColliderActive(false);
			}

			OnExitStaminaLogic();
			base.OnExit();
		}

		public override void Update(float deltaTime)
		{
			if(_isBlocking)
			{
				if(_context.CharacterStats.Stamina.Value <= 0)
				{
					IsComplete = true;
					return;
				}
			}
			else
			{
				IsComplete = true;
				return;
			}

			UpdateBlockLogic(deltaTime);
		}

		public override float GetEnterStaminaCost()
		{
			return 1f;
		}

		protected abstract void OnEnterStaminaLogic();
		protected abstract void OnExitStaminaLogic();
		protected abstract void UpdateBlockLogic(float deltaTime);
	}
} 