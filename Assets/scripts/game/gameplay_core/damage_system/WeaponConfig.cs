using System;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.state_machine.states.attack;
using Sirenix.OdinInspector;
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

		[field: BoxGroup("Roll")]
		[field: SerializeField]
		public AttackConfig RollAttack { get; private set; }
		[field: BoxGroup("Roll")]
		[field: SerializeField]
		public AttackConfig RollAttackStrong { get; private set; }
		[field: BoxGroup("Run")]
		[field: SerializeField]
		public AttackConfig RunAttack { get; private set; }
		[field: BoxGroup("Run")]
		[field: SerializeField]
		public AttackConfig RunAttackStrong { get; private set; }

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
				default:
					throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null);
			}
		}
	}
}
