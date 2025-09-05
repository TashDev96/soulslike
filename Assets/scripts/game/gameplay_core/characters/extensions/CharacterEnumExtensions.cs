using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.state_machine.states.attack;

namespace game.gameplay_core.characters.extensions
{
	public static class CharacterEnumExtensions
	{
		public static bool IsMovementCommand(this CharacterCommand command)
		{
			return command is CharacterCommand.Walk or CharacterCommand.Run;
		}

		public static bool IsAttackCommand(this CharacterCommand command)
		{
			return command is CharacterCommand.RegularAttack or CharacterCommand.StrongAttack or CharacterCommand.AttackByIndex;
		}

		public static bool IsParryCommand(this CharacterCommand command)
		{
			return command == CharacterCommand.Parry;
		}

		public static bool IsRollAttack(this AttackType attackType)
		{
			return attackType is AttackType.RollAttackRegular or AttackType.RollAttackStrong;
		}
	}
}
