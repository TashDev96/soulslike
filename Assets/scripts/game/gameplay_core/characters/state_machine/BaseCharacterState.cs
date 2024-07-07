namespace game.gameplay_core.characters.state_machine
{
	public abstract class BaseCharacterState
	{
		public CharacterContext Context { get; set; }
		public bool IsComplete { get; protected set; }
		public bool IsReadyToRememberNextCommand { get; set; }

		public abstract void Update(float deltaTime);

		protected BaseCharacterState(CharacterContext context)
		{
			Context = context;
		}

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
