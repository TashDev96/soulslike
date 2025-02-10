namespace game.gameplay_core.characters.commands
{
	public static class CommandExtensions
	{
		public static bool IsMovementCommand(this CharacterCommand command)
		{
			return command is CharacterCommand.Walk or CharacterCommand.Run;
		}
	}
}