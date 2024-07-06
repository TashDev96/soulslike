namespace game.gameplay_core.characters.state_machine
{
	public class CharacterStateMachine
	{
		private CharacterCommand _nextCommand;
		private readonly CharacterContext _context;
		public BaseCharacterState CurrentState { get; }

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
