using System;
using game.gameplay_core.characters.stats;

namespace game.gameplay_core.characters
{
	[Serializable]
	public class CharacterStats
	{
		public Hp Hp;
		public MaxHp MaxHp;
		public Stamina Stamina;
		public MaxStamina MaxStamina;
	}
}
