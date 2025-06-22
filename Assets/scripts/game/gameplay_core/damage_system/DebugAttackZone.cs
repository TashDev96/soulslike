using dream_lib.src.reactive;
using game.gameplay_core.characters;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.utils;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class DebugAttackZone : MonoBehaviour
	{
		[SerializeField]
		private CapsuleCaster _capsuleCaster;

		[SerializeField]
		private float _baseDamage = 10f;

		[SerializeField]
		private float _attackInterval = 2f;

		[SerializeField]
		private bool _drawDebugVisuals = true;

		private float _lastAttackTime;
		private HitData _hitData;
		private readonly HitConfig _hitConfig = new();
		private CharacterContext _fakeContext;

		private void Start()
		{
			_hitConfig.DamageMultiplier = 1f;
			_hitConfig.PoiseDamage = 5f;
			_hitConfig.FriendlyFire = true;

			CreateFakeCharacterContext();
		}

		private void Update()
		{
			if(Time.time - _lastAttackTime >= _attackInterval)
			{
				PerformDebugAttack();
				_lastAttackTime = Time.time;
			}
		}

		private void PerformDebugAttack()
		{
			if(_capsuleCaster == null)
			{
				return;
			}

			_hitData = new HitData
			{
				Config = _hitConfig
			};

			AttackHelpers.CastAttack(_baseDamage, _hitData, _capsuleCaster, _fakeContext, 999, _drawDebugVisuals);
		}

		private void CreateFakeCharacterContext()
		{
			_fakeContext = new CharacterContext
			{
				Team = new ReactiveProperty<Team>(Team.HostileNPC),
				CharacterId = new ReactiveProperty<string>("DebugAttackZone"),
				IsPlayer = new ReactiveProperty<bool>(),
				SelfLink = null
			};
		}
	}
}
