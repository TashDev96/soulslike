using game.gameplay_core.characters.commands;

namespace game.gameplay_core.characters.state_machine.states
{
	public class StayBlockState : CharacterStateBase
	{
		private const string StaminaRegenKey = nameof(StayBlockState);
		private bool _isBlocking;
		
		

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
			
			if(_context.WeaponView.Value != null)
			{
				_context.WeaponView.Value.SetBlockColliderActive(true);
			}
		}

		public override void OnExit()
		{
			_isBlocking = false;
			_context.StaminaLogic.RemoveStaminaRegenMultiplier(StaminaRegenKey);
			
			if(_context.WeaponView.Value != null)
			{
				_context.WeaponView.Value.SetBlockColliderActive(false);
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