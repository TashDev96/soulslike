using System;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.state_machine.states.attack;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	[Serializable]
	public class WeaponConfig
	{
		[field: SerializeField]
		public AttackConfig[] RegularAttacks { get; private set; }
		[field: SerializeField]
		public AttackConfig[] StrongAttacks { get; private set; }
		[field: SerializeField]
		public AttackConfig[] SpecialAttacks { get; private set; }

		[field: SerializeField]
		public SerializableDictionary<int, int> RegularToRegularCustomOrder { get; private set; }
		[field: SerializeField]
		public SerializableDictionary<int, int> RegularToStrongCustomOrder { get; private set; }
		[field: SerializeField]
		public SerializableDictionary<int, int> StrongToRegularCustomOrder { get; private set; }

		public AttackConfig[] GetAttacksSequence(AttackType attackType)
		{
			switch(attackType)
			{
				case AttackType.Regular:
					return RegularAttacks;
				case AttackType.Strong:
					return StrongAttacks;
				case AttackType.Special:
					return SpecialAttacks;
				default:
					throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null);
			}
		}
	}
}
