using System;
using Animancer;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.state_machine.states.attack;
using game.gameplay_core.damage_system;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.inventory.item_configs
{
	[CreateAssetMenu(menuName = "Configs/WeaponConfig")]
	public class WeaponItemConfig : BaseItemConfig
	{
		[field: ValueDropdown("@AddressableAssetNames.WeaponNames")]
		[field: SerializeField]
		public string WeaponPrefabName { get; private set; }

		[field: SerializeField]
		public AttackConfig[] RegularAttacks { get; private set; }
		[field: SerializeField]
		public AttackConfig[] StrongAttacks { get; private set; }

		[field: FoldoutGroup("Roll")]
		[field: SerializeField]
		public AttackConfig RollAttack { get; private set; }
		[field: FoldoutGroup("Roll")]
		[field: SerializeField]
		public AttackConfig RollAttackStrong { get; private set; }

		[field: FoldoutGroup("Run")]
		[field: SerializeField]
		public AttackConfig RunAttack { get; private set; }
		[field: FoldoutGroup("Run")]
		[field: SerializeField]
		public AttackConfig RunAttackStrong { get; private set; }

		[field: FoldoutGroup("Ranged")]
		[field: SerializeField]
		public float ProjectileSpeed { get; private set; } = 20f;

		[field: FoldoutGroup("Block")]
		[field: SerializeField]
		[field: Range(0, 100)]
		public float BlockStability { get; private set; } = 100f;
		[field: FoldoutGroup("Block")]
		[field: SerializeField]
		[field: Range(0f, 1f)]
		public float DamageReduction { get; private set; } = 0.8f;
		[field: FoldoutGroup("Block")]
		[field: SerializeField]
		public float BlockStaminaCost { get; private set; } = 5f;
		[field: FoldoutGroup("Block")]
		[field: SerializeField]
		public int BlockDeflectionRating { get; private set; } = 5;
		[field: FoldoutGroup("Block")]
		[field: SerializeField]
		public int AttackDeflectionRating { get; private set; } = 5;

		[field: FoldoutGroup("Block/Animations")]
		[field: SerializeField]
		public ClipTransition BlockStayAnimation { get; private set; }
		[field: FoldoutGroup("Block/Animations")]
		[field: SerializeField]
		public ClipTransition BlockWalkAnimation { get; private set; }
		[field: FoldoutGroup("Block/Animations")]
		[field: SerializeField]
		public ClipTransition BlockBreakAnimation { get; private set; }
		[field: FoldoutGroup("Block/Animations")]
		[field: SerializeField]
		public ClipTransition BlockHitAnimation { get; private set; }

		[field: FoldoutGroup("Parry")]
		[field: SerializeField]
		public bool CanParry { get; private set; }

		[field: FoldoutGroup("Parry")]
		[field: SerializeField]
		public AttackConfig Parry { get; private set; }

		[field: SerializeField]
		public bool CanRiposte { get; private set; } = true;
		
		[field: ShowIf("CanRiposte")]
		[field: FoldoutGroup("Riposte")]

		[field: SerializeField]
		
		public AttackConfig RiposteAttack { get; private set; }

		[field: SerializeField]
		public bool CanBackstab { get; private set; } = true;

		[field: FoldoutGroup("BackStab")]
		[field: SerializeField]
		
		[field: ShowIf("CanBackstab")]
		public AnimationClip BackstabbedEnemyAnimation { get; set; }
		[field: FoldoutGroup("BackStab")]
		[field: ShowIf("CanBackstab")]

		[field: SerializeField]
		public AttackConfig BackstabAttack { get; private set; }

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

		public AttackConfig GetRiposteAttack()
		{
			return RiposteAttack;
		}
	}
}
