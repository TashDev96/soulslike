namespace game.gameplay_core.characters.state_machine
{
	public abstract class BaseCharacterState
	{
		protected CharacterContext _context;
		public bool IsComplete { get; protected set; }
		public bool IsReadyToRememberNextCommand { get; set; }

		protected BaseCharacterState(CharacterContext context)
		{
			_context = context;
		}

		public abstract void Update(float deltaTime);

		public virtual bool TryChangeStateByCustomLogic(out BaseCharacterState newState)
		{
			newState = null;
			return false;
		}

		public virtual bool IsContinuousForCommand(CharacterCommand command)
		{
			return false;
		}
	}
}
