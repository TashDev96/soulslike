using System;
using System.Collections;
using dream_lib.src.extensions;
using dream_lib.src.utils.drawers;
using game.enums;
using game.gameplay_core.characters;
using game.gameplay_core.inventory.serialized_data;
using game.gameplay_core.location.interactive_objects.common;
using game.gameplay_core.location.location_save_system;
using game.gameplay_core.location.view;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.location.interactive_objects
{
	[AddressableAssetTag(nameof(AddressableCollections.LootVfxPrefabs))]
	public class LootItem : InteractiveObjectBase<LootItemSaveData>
	{
		[SerializeField]
		private Rigidbody _lootAnimationRigidBody;

		[SerializeField]
		private RandomBounceConfig[] _randomBouncesMultipliers;

		private Vector3 _defPos;

		public void InitializeFromLoot(LootConfig lootConfig)
		{
			var itemData = new InventoryItemSaveData
			{
				ConfigId = lootConfig.ConfigId,
				IsInitialized = false
			};

			GenerateUniqueId();

			var layerMask = LayerMask.GetMask("Default", "LevelGeometry", "Doors");
			var fallbackPosition = transform.position;
			if(Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out var hit, 10f, layerMask))
			{
				fallbackPosition = hit.point;
			}
			SaveData = new LootItemSaveData
			{
				UniqueId = UniqueId,
				Item = itemData,
				PrefabName = lootConfig.LootVfxPrefab,
				Position = fallbackPosition
			};

			StartCoroutine(AnimateDrop());

			base.Initialize();
		}

		public override void InitializeFirstTime()
		{
		}

		protected override void InitializeAfterSaveLoaded()
		{
			transform.position = SaveData.Position;
			UniqueId = SaveData.UniqueId;
		}

		private void Awake()
		{
			_defPos = transform.position;
		}

		protected override void HandleInteractionTriggered(CharacterDomain interactedCharacter)
		{
			gameObject.SetActive(false);
			LocationStaticContext.Instance.LootLogic.HandleLootInteracted(SaveData, interactedCharacter);
		}

		protected override string GetInteractionTextHint()
		{
			return "Pick Up Item";
		}

		[Button]
		private void TestDrop()
		{
			if(!Application.isPlaying)
			{
				return;
			}
			transform.position = _defPos;
			StartCoroutine(AnimateDrop());
		}

		private IEnumerator AnimateDrop()
		{
			var randomBounceIndex = 0;
			_lootAnimationRigidBody.transform.parent = null;
			_lootAnimationRigidBody.transform.position = transform.position;
			_lootAnimationRigidBody.gameObject.SetActive(true);

			_lootAnimationRigidBody.linearVelocity += GetRandomAcceleration(_randomBouncesMultipliers[randomBounceIndex]);
			randomBounceIndex++;

			var prevSpeed = _lootAnimationRigidBody.linearVelocity;

			while(_lootAnimationRigidBody.linearVelocity.sqrMagnitude > 0.001f && prevSpeed.sqrMagnitude > 0.001f)
			{
				transform.position = _lootAnimationRigidBody.transform.position;
				if(randomBounceIndex < _randomBouncesMultipliers.Length)
				{
					var yVelocityInverted = prevSpeed.y < 0 && _lootAnimationRigidBody.linearVelocity.y >= 0;
					if(yVelocityInverted)
					{
						_lootAnimationRigidBody.linearVelocity += GetRandomAcceleration(_randomBouncesMultipliers[randomBounceIndex]);
						randomBounceIndex++;
					}
				}
				prevSpeed = _lootAnimationRigidBody.linearVelocity;
				yield return null;
			}

			_lootAnimationRigidBody.gameObject.SetActive(false);

			Vector3 GetRandomAcceleration(RandomBounceConfig config)
			{
				var horSpeed = config.HorSpeedRange.GetRandomInRange();
				var vertSpeed = config.VertSpeedRange.GetRandomInRange();
				var angleChange = config.AngleChangeRange.GetRandomInRange();

				var currentAngle = 0f;
				if(_lootAnimationRigidBody.linearVelocity.sqrMagnitude > 0.01f)
				{
					currentAngle = Quaternion.LookRotation(_lootAnimationRigidBody.linearVelocity.SetY(0)).eulerAngles.y;
				}
				var rotation = Quaternion.Euler(0f, currentAngle + angleChange, 0f);
				return rotation * Vector3.forward * horSpeed + Vector3.up * vertSpeed;
			}
		}

		[Serializable]
		private struct RandomBounceConfig
		{
			public Vector2 VertSpeedRange;
			public Vector2 HorSpeedRange;
			public Vector2 AngleChangeRange;
		}
	}

	[Serializable]
	public class LootItemSaveData : SpawnedObjectSaveData
	{
		public InventoryItemSaveData Item;
	}
}
