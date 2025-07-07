using game.gameplay_core.characters.commands;

namespace game.gameplay_core.characters.state_machine.states
{
	public class StayBlockState : BlockStateBase
	{
		private const string StaminaRegenKey = nameof(StayBlockState);

		public StayBlockState(CharacterContext context) : base(context)
		{
		}

		public override bool TryContinueWithCommand(CharacterCommand nextCommand)
		{
			switch(nextCommand)
			{
				case CharacterCommand.StayBlock:
					IsComplete = false;
					return true;
				case CharacterCommand.WalkBlock:
					IsComplete = true;
					return false;
				default:
					IsComplete = true;
					return false;
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

		public override void OnEnter()
		{
			base.OnEnter();
			_context.StaminaLogic.SetStaminaRegenMultiplier(StaminaRegenKey, 0.3f);
		}

		public override void OnExit()
		{
			base.OnExit();
			_context.StaminaLogic.RemoveStaminaRegenMultiplier(StaminaRegenKey);
		}

		protected override void PlayBlockAnimation()
		{
			_context.Animator.Play(BlockingWeapon.Config.BlockStayAnimation, 0.2f);
		}
	}
}
