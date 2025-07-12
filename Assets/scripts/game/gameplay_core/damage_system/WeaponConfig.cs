using System;
using Animancer;
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

		[field: BoxGroup("Block")]
		[field: SerializeField]
		[field: Range(0,100)]
		public float BlockStability { get; private set; } = 100f;
		[field: BoxGroup("Block")]
		[field: SerializeField]
		[field: Range(0f, 1f)]
		public float DamageReduction { get; private set; } = 0.8f;
		[field: BoxGroup("Block")]
		[field: SerializeField]
		public float BlockStaminaCost { get; private set; } = 5f;
		[field: BoxGroup("Block")]
		[field: SerializeField]
		public int BlockDeflectionRating { get; private set; } = 5;
		[field: BoxGroup("Block")]
		[field: SerializeField]
		public int AttackDeflectionRating { get; private set; } = 5;
		
		[field: BoxGroup("Block/Animations")]
		[field: SerializeField]
		public ClipTransition BlockStayAnimation { get; private set; }
		[field: BoxGroup("Block/Animations")]
		[field: SerializeField]
		public ClipTransition BlockWalkAnimation { get; private set; }
		[field: BoxGroup("Block/Animations")]
		[field: SerializeField]
		public ClipTransition BlockBreakAnimation { get; private set; }
		[field: BoxGroup("Block/Animations")]
		[field: SerializeField]
		public ClipTransition BlockHitAnimation { get; private set; }
		
		[field: BoxGroup("Parry")]
		[field: SerializeField]
		public bool CanParry { get; private set; } = false;
		
		[field: BoxGroup("Parry")]
		[field: SerializeField]
		public float ParryActiveFrameStart { get; private set; } = 0.1f;
		
		[field: BoxGroup("Parry")]
		[field: SerializeField]
		public float ParryActiveFrameEnd { get; private set; } = 0.3f;
		
		[field: BoxGroup("Parry")]
		[field: SerializeField]
		public float ParryRecoveryFrameEnd { get; private set; } = 0.8f;
		
		[field: BoxGroup("Parry")]
		[field: SerializeField]
		public float ParryStaminaCost { get; private set; } = 15f;
		
		[field: BoxGroup("Parry/Animations")]
		[field: SerializeField]
		public ClipTransition ParryAnimation { get; private set; }
		
		[field: BoxGroup("Parry/Animations")]
		[field: SerializeField]
		public ClipTransition ParrySuccessAnimation { get; private set; }
		
		[field: BoxGroup("Riposte")]
		[field: SerializeField]
		public AttackConfig RiposteAttack { get; private set; }
		
		[field: BoxGroup("Riposte")]
		[field: SerializeField]
		public float RiposteDamageMultiplier { get; private set; } = 2.0f;

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
				case AttackType.Riposte:
					return new[] { RiposteAttack };
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
