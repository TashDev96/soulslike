namespace game.gameplay_core.characters
{
	public interface ICharacterBrain
	{
		public void Initialize(CharacterContext context);
		public void Think(float deltaTime);
	}
}
