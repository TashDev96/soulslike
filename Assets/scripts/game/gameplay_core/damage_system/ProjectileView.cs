using System;
using System.Collections.Generic;
using game.enums;
using game.gameplay_core.characters;
using game.gameplay_core.utils;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	[AddressableAssetTag(nameof(AddressableCollections.ProjectilePrefabs))]
	public class ProjectileView : MonoBehaviour
	{
		private const float StuckInWallDuration = 60f;
		[SerializeField]
		private CapsuleCaster[] _hitCasters;
		[SerializeField]
		private float _maxLifetime = 10f;

		private float _speed;
		private float _baseDamage;
		private HitConfig _hitConfig;
		private CharacterContext _casterContext;
		private int _deflectionRating;
		private float _lifetime;
		private bool _stuckInWall;
		private float _stuckTimer;

		private readonly HashSet<string> _impactedCharacters = new();
		private readonly HashSet<Collider> _impactedTargets = new();

		private readonly InterpolatedCapsuleCaster _interpolatedCaster = new();
		private readonly List<CapsuleCaster> _castCollidersCache = new();

		private IDisposable _updateSubscription;

		private static int _layerMaskDamageReceivers;
		private static int _layerMaskWalls;
		private static bool _layerMasksInitialized;
		private static readonly Collider[] Results = new Collider[40];

		public void Initialize(ProjectileData data)
		{
			if(!_layerMasksInitialized)
			{
				_layerMaskDamageReceivers = LayerMask.GetMask("DamageReceivers");
				_layerMaskWalls = LayerMask.GetMask("Default");
				_layerMasksInitialized = true;
			}

			_speed = data.Speed;
			_baseDamage = data.BaseDamage;
			_hitConfig = data.HitConfig;
			_casterContext = data.CasterContext;
			_deflectionRating = data.DeflectionRating;
			_lifetime = 0f;

			transform.position = data.SpawnPosition;
			transform.forward = data.Direction;

			StorePreviousTransform();

			_updateSubscription = GameStaticContext.Instance.CurrentLocationUpdate.Subscribe(CustomUpdate);
		}

		private void OnDestroy()
		{
			_updateSubscription?.Dispose();
		}

		private void CustomUpdate(float deltaTime)
		{
			if(_stuckInWall)
			{
				_stuckTimer += deltaTime;
				if(_stuckTimer >= StuckInWallDuration)
				{
					Destroy(gameObject);
				}
				return;
			}

			_lifetime += deltaTime;

			if(_lifetime >= _maxLifetime)
			{
				Destroy(gameObject);
				return;
			}

			var moveDistance = _speed * deltaTime;
			transform.position += transform.forward * moveDistance;

			var hitResult = CastDamageInterpolated();
			if(hitResult == HitResult.HitWall)
			{
				_stuckInWall = true;
				return;
			}
			if(hitResult == HitResult.HitTarget)
			{
				Destroy(gameObject);
				return;
			}

			StorePreviousTransform();
		}

		private void StorePreviousTransform()
		{
			for(var i = 0; i < _hitCasters.Length; i++)
			{
				_hitCasters[i].StorePreviousPosition();
			}
		}

		private HitResult CastDamageInterpolated()
		{
			_castCollidersCache.Clear();
			for(var i = 0; i < _hitCasters.Length; i++)
			{
				_castCollidersCache.Add(_hitCasters[i]);
			}

			_interpolatedCaster.Start(_castCollidersCache);

			while(_interpolatedCaster.MoveNext())
			{
				foreach(var caster in _interpolatedCaster.GetActiveColliders())
				{
					var result = CastDamage(caster);
					if(result != HitResult.None)
					{
						_interpolatedCaster.ResetOnInterrupted();
						return result;
					}
				}
			}

			return HitResult.None;
		}

		private HitResult CastDamage(CapsuleCaster caster)
		{
			var radius = caster.Radius;
			caster.GetCapsulePoints(out var point0, out var point1);

			var wallCount = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, Results, _layerMaskWalls);
			if(wallCount > 0)
			{
				return HitResult.HitWall;
			}

			var count = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, Results, _layerMaskDamageReceivers);

			for(var j = 0; j < count; j++)
			{
				if(_impactedTargets.Contains(Results[j]))
				{
					continue;
				}

				if(Results[j].TryGetComponent<BlockReceiver>(out var blockReceiver))
				{
					if(blockReceiver.OwnerTeam == _casterContext.Team.Value && !_hitConfig.FriendlyFire)
					{
						continue;
					}

					if(_impactedCharacters.Contains(blockReceiver.CharacterId) || blockReceiver.CharacterId == _casterContext.CharacterId.Value)
					{
						continue;
					}

					_impactedTargets.Add(Results[j]);
					_impactedCharacters.Add(blockReceiver.CharacterId);

					blockReceiver.ApplyDamage(CreateDamageInfo(Results[j], caster), out _);
					return HitResult.HitTarget;
				}
			}

			for(var j = 0; j < count; j++)
			{
				if(_impactedTargets.Contains(Results[j]))
				{
					continue;
				}

				_impactedTargets.Add(Results[j]);
				if(!Results[j].TryGetComponent<DamageReceiver>(out var damageReceiver))
				{
					continue;
				}

				if(damageReceiver.OwnerTeam == _casterContext.Team.Value && !_hitConfig.FriendlyFire)
				{
					continue;
				}

				if(_impactedCharacters.Contains(damageReceiver.CharacterId) || damageReceiver.CharacterId == _casterContext.CharacterId.Value)
				{
					continue;
				}

				_impactedCharacters.Add(damageReceiver.CharacterId);

				if(damageReceiver.IsInvulnerable)
				{
					//hit rolling target
					return HitResult.None;
				}

				damageReceiver.ApplyDamage(CreateDamageInfo(Results[j], caster));
				return HitResult.HitTarget;
			}

			return HitResult.None;
		}

		private DamageInfo CreateDamageInfo(Collider targetCollider, CapsuleCaster caster)
		{
			caster.GetCapsulePoints(out var point0, out var point1);
			var hitCapsuleCenter = (point0 + point1) / 2f;
			var worldPos = targetCollider.ClosestPoint(hitCapsuleCenter);
			var movementDirection = caster.SampleMoveDirection(worldPos);
			var direction = movementDirection.sqrMagnitude > 0.0001f ? movementDirection.normalized : transform.forward;

			return new DamageInfo
			{
				DamageAmount = _baseDamage * _hitConfig.DamageMultiplier,
				PoiseDamageAmount = _hitConfig.PoiseDamage,
				WorldPos = worldPos,
				Direction = direction,
				DoneByPlayer = _casterContext.IsPlayer.Value,
				DamageDealer = _casterContext.SelfLink,
				DeflectionRating = _deflectionRating
			};
		}

		private enum HitResult
		{
			None,
			HitWall,
			HitTarget
		}
	}

	public struct ProjectileData
	{
		public float Speed;
		public float BaseDamage;
		public HitConfig HitConfig;
		public CharacterContext CasterContext;
		public int DeflectionRating;
		public Vector3 SpawnPosition;
		public Vector3 Direction;
	}
}
