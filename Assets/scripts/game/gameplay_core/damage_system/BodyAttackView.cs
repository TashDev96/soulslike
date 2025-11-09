using game.gameplay_core.characters;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.utils;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class BodyAttackView : MonoBehaviour
	{
		[SerializeField]
		private CapsuleCaster[] _rollColliders;

		[SerializeField]
		private CapsuleCaster[] _fallColliders;

		private CharacterContext _context;

		private readonly HitConfig _rollHitConfig = new();
		private HitData _rollHitData;
		private float _rollDamage;

		private readonly HitConfig _fallHitConfig = new();
		private HitData _fallHitData;
		private float _fallDamage;

		public void Initialize(CharacterContext characterContext)
		{
			_context = characterContext;
		}

		public void Update(float deltaTime)
		{
			foreach(var rollCollider in _rollColliders)
			{
				rollCollider.UpdateMovementDirectionCache();
			}
		}

		public void CastRollAttack()
		{
			foreach(var rollCollider in _rollColliders)
			{
				rollCollider.UpdateMovementDirectionCache();
				AttackHelpers.CastAttack(_rollDamage, _rollHitData, rollCollider, _context, 999, true);
			}
		}

		public void PrepareRollBodyAttack()
		{
			_rollHitConfig.DamageMultiplier = 1;
			_rollHitConfig.PoiseDamage = 1; //todo depends on armor weight
			_rollDamage = 0; //todo spiked armor

			_rollHitData = new HitData
			{
				Config = _rollHitConfig
			};
		}

		public void CastFallAttack(float fallDistance)
		{
			_fallHitConfig.PoiseDamage = fallDistance;
			_fallHitConfig.DamageMultiplier = 1; //todo depends on armor weight and spikes
			_fallDamage = fallDistance;

			_fallHitData = new HitData
			{
				Config = _fallHitConfig
			};

			foreach(var fallColliders in _fallColliders)
			{
				AttackHelpers.CastAttack(_fallDamage, _fallHitData, fallColliders, _context, 999, true);
			}
		}
	}
}
