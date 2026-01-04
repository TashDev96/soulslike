using game.gameplay_core.characters.commands;

namespace game.gameplay_core.characters.state_machine.states
{
	public class StayBlockState : BlockStateBase
	{
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

		protected override void PlayBlockAnimation()
		{
			_context.Animator.Play(BlockingWeaponView.Config.BlockStayAnimation, 0.2f);
		}
	}
}
