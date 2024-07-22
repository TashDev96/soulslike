using System;
using dream_lib.src.utils.data_types;
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
	}
}
