namespace game.gameplay_core.character.state_machine
{
	public class CharacterStateMachine
	{
		public BaseCharacterState CurrentState { get; private set; }

		private CharacterCommand _nextCommand;
		private readonly CharacterContext _context;

		public CharacterStateMachine(CharacterContext characterContext)
		{
			_context = characterContext;

			CurrentState = new IdleState();
		}

		public void Update(float deltaTime)
		{
			if(_nextCommand == CharacterCommand.None)
			{
				if(_context.InputData.Command != CharacterCommand.None)
				{
					if(CurrentState.IsReadyToNextInput)
					{
						_nextCommand = _context.InputData.Command;
						return;
					}
				}
			}

			CurrentState.Update(deltaTime);

			// if(CurrentState.CanSwitchToNextCommand(_nextCommand))
			// {
			// 	
			// }
		}
	}
}
