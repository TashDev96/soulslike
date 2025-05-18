using game.gameplay_core.characters;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.utils;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class BodyAttackView : MonoBehaviour
	{
		[SerializeField]
		private FakeCapsuleCollider[] _rollColliders;

		[SerializeField]
		private FakeCapsuleCollider[] _fallColliders;

		private CharacterContext _context;

		private readonly HitConfig _rollHitConfig = new();
		private HitData _rollHitData;
		private float _rollDamage;

		public void Initialize(CharacterContext characterContext)
		{
			_context = characterContext;
		}

		public void CastRollAttack()
		{
			foreach(var rollCollider in _rollColliders)
			{
				AttackHelpers.CastAttack(_rollDamage, _rollHitData, rollCollider, _context, true);	
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
	}
}
