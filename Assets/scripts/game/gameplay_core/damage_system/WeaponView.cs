using System;
using System.Collections.Generic;
using game.enums;
using game.gameplay_core.characters;
using game.gameplay_core.inventory.item_configs;
using game.gameplay_core.utils;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	[AddressableAssetTag(nameof(AddressableCollections.WeaponNames))]
	public class WeaponView : MonoBehaviour
	{
		[SerializeField]
		private CapsuleCaster[] _hitColliders;

		[SerializeField]
		private CapsuleCaster[] _preciseHitColliders;

		[SerializeField]
		private CapsuleCaster[] _handleColliders;

		[SerializeField]
		private BlockReceiver[] _blockColliders;

		[SerializeField]
		private Transform _projectileSpawnPoint;

		[NonSerialized]
		public WeaponItemConfig Config;

		public Vector3 ProjectileSpawnPosition => _projectileSpawnPoint != null ? _projectileSpawnPoint.position : transform.position;

		private readonly InterpolatedCapsuleCaster _interpolatedCaster = new();

		private CharacterContext _context;
		private readonly List<CapsuleCaster> _castCollidersCache = new();

		public void Initialize(CharacterContext characterContext)
		{
			_context = characterContext;
			SetBlockColliderActive(false);

			foreach(var blockReceiver in _blockColliders)
			{
				blockReceiver.Initialize(new BlockReceiver.Context
				{
					Team = _context.Team,
					CharacterId = _context.CharacterId,
					WeaponConfig = Config,
					BlockLogic = _context.BlockLogic
				});
			}
		}

		public void SetBlockColliderActive(bool active)
		{
			if(_blockColliders != null)
			{
				for(var i = 0; i < _blockColliders.Length; i++)
				{
					_blockColliders[i].gameObject.SetActive(active);
				}
			}
		}

		public InterpolatedCapsuleCaster StartInterpolatedCast(WeaponColliderType colliderType, IReadOnlyList<bool> involvedColliders = null)
		{
			var castColliders = colliderType switch
			{
				WeaponColliderType.Attack => _hitColliders,
				WeaponColliderType.PreciseContact => _preciseHitColliders,
				WeaponColliderType.Handle => _handleColliders,
				_ => _hitColliders
			};

			_castCollidersCache.Clear();

			for(var i = 0; i < castColliders.Length; i++)
			{
				if(involvedColliders == null || involvedColliders[i])
				{
					_castCollidersCache.Add(castColliders[i]);
				}
			}

			_interpolatedCaster.Start(_castCollidersCache);
			return _interpolatedCaster;
		}

		public void StorePreviousTransform()
		{
			for(var i = 0; i < _hitColliders.Length; i++)
			{
				_hitColliders[i].StorePreviousPosition();
			}
			for(var i = 0; i < _preciseHitColliders.Length; i++)
			{
				_preciseHitColliders[i].StorePreviousPosition();
			}
			for(var i = 0; i < _handleColliders.Length; i++)
			{
				_handleColliders[i].StorePreviousPosition();
			}
		}
	}

	public enum WeaponColliderType
	{
		Attack,
		PreciseContact,
		Handle,
		Block
	}
}
