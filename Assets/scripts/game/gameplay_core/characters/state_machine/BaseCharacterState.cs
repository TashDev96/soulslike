namespace game.gameplay_core.characters.state_machine
{
	public abstract class BaseCharacterState
	{
		public bool IsComplete { get; protected set; }
		public bool IsReadyToNextInput { get; set; }

		public void Update(float deltaTime)
		{
		}
	}
}
