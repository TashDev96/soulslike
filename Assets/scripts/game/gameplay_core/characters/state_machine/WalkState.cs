using UnityEngine;

namespace game.gameplay_core.characters.state_machine
{
	public class WalkState : BaseCharacterState
	{
		public WalkState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
			IsComplete = true;
		}

		public override void Update(float deltaTime)
		{
			Context.Transform.Translate(Context.InputData.DirectionWorld * deltaTime, Space.World);
		}

		public override bool IsContinuousForCommand(CharacterCommand command)
		{
			return command == CharacterCommand.Walk;
		}
	}
}
