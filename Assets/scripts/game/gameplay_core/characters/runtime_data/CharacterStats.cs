using System;
using game.gameplay_core.characters.runtime_data.bindings.stats;

namespace game.gameplay_core.characters.runtime_data
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
