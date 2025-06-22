using game.gameplay_core.characters.commands;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.state_machine.states
{
	public class StayBlockState : CharacterStateBase
	{
		private const string StaminaRegenKey = nameof(StayBlockState);
		private bool _isBlocking;
		private WeaponView _weapon;

		public StayBlockState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			IsComplete = false;
			_isBlocking = true;
			
			_context.Animator.Play(_context.Config.IdleAnimation, 0.2f);
			_context.StaminaLogic.SetStaminaRegenMultiplier(StaminaRegenKey, 0.3f);

			_weapon = _context.LeftWeapon.HasValue ? _context.LeftWeapon.Value : _context.RightWeapon.Value;
			
			if(_weapon != null)
			{
				_weapon.SetBlockColliderActive(true);
			}
		}

		public override void OnExit()
		{
			_isBlocking = false;
			_context.StaminaLogic.RemoveStaminaRegenMultiplier(StaminaRegenKey);
			
			if(_weapon != null)
			{
				_weapon.SetBlockColliderActive(false);
			}
			
			base.OnExit();
		}

		public override bool TryContinueWithCommand(CharacterCommand nextCommand)
		{
			switch(nextCommand)
			{
				case CharacterCommand.StayBlock:
					IsComplete = false;
					return true;
				case CharacterCommand.WalkBlock:
					return false;
				default:
					return false;
			}
		}

		public override void Update(float deltaTime)
		{
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

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			if(nextCommand == CharacterCommand.WalkBlock)
			{
				return true;
			}
			return base.CheckIsReadyToChangeState(nextCommand);
		}

		public override bool CanInterruptByStagger => true;

		public override float GetEnterStaminaCost()
		{
			return 1f;
		}
	}
} 