using System;
using System.Collections.Generic;
using dream_lib.src.utils.data_types;
using game.enums;
using game.gameplay_core.inventory;
using game.gameplay_core.inventory.item_configs;
using game.gameplay_core.inventory.items_logic;
using game.gameplay_core.inventory.serialized_data;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class CharacterInventoryLogic
	{
		private readonly List<BaseItemLogic> _items = new();
		private CharacterContext _context;
		private InventoryData _data;

		private readonly Dictionary<EquipmentSlotAdress, BaseItemLogic> _equippedItems = new();
		public IReadOnlyDictionary<EquipmentSlotAdress, BaseItemLogic> EquippedItems => _equippedItems;

		private EquipmentSlotType[] ParrySlots => new[] { EquipmentSlotType.LeftHand, EquipmentSlotType.RightHand };

		public WeaponItemLogic RightWeapon => GetEquipment(EquipmentSlotType.RightHand) as WeaponItemLogic;
		public WeaponItemLogic LeftWeapon => GetEquipment(EquipmentSlotType.LeftHand) as WeaponItemLogic;

		public event Action<EquipmentSlotAdress, BaseItemLogic> OnEquipChanged;

		public void Initialize(CharacterContext context, InventoryData data)
		{
			_context = context;
			_data = data;
			data.Items ??= new List<InventoryItemSaveData>();
			data.EquippedItems ??= new SerializableDictionary<EquipmentSlotAdress, string>();
			foreach(var itemSaveData in data.Items)
			{
				var itemLogic = InventoryItemsFabric.CreateItemFromSave(itemSaveData);
				itemLogic.InitializeForLocation(context);
				itemLogic.LoadData(itemSaveData);
				_items.Add(itemLogic);

				if(!context.CurrentConsumableItem.HasValue)
				{
					if(itemLogic is MainHealingItemLogic mainHealingItem)
					{
						context.CurrentConsumableItem.Value = mainHealingItem;
					}
				}
			}

			foreach(var kvp in data.EquippedItems)
			{
				var itemToEquip = _items.Find(item => item.UniqueId == kvp.Value);
				_equippedItems[kvp.Key] = itemToEquip;
			}

			EnsureHandsNotNull();
		}

		public void PickUpItem(InventoryItemSaveData itemSaveData)
		{
			var createdItemLogic = InventoryItemsFabric.CreateItemFromSave(itemSaveData);

			switch(createdItemLogic)
			{
				case BaseConsumableItemLogic baseConsumableItemLogic:
					HandleConsumableItemPickup(itemSaveData, baseConsumableItemLogic);
					break;
				case MainHealingItemLogic mainHealingItemLogic:
					HandleMainHealingItemPickup(itemSaveData, mainHealingItemLogic);
					break;
				case WeaponItemLogic weaponItemLogic:
					break;
			}
		}

		public BaseItemLogic GetEquipment(EquipmentSlotType slot, int index = 0)
		{
			return _equippedItems.GetValueOrDefault(new EquipmentSlotAdress(slot, index));
		}

		public void EquipItem(BaseItemLogic item, EquipmentSlotType slotType, int slotIndex)
		{
			if(item == null && (slotType == EquipmentSlotType.LeftHand || slotType == EquipmentSlotType.RightHand))
			{
				item = CreateEmptyHandItem();
			}

			var key = new EquipmentSlotAdress(slotType, slotIndex);
			_equippedItems[key] = item;
			if(item != null)
			{
				_data.EquippedItems[key] = item.UniqueId;
			}
			else
			{
				_data.EquippedItems.Remove(key);
			}

			OnEquipChanged?.Invoke(key, item);
		}

		public void UnequipItem(EquipmentSlotType slotType, int slotIndex)
		{
			EquipItem(null, slotType, slotIndex);
		}

		public void EquipItemAuto(BaseEquipmentItemConfig config, BaseItemLogic item)
		{
			var index = 0;

			var maxIndex = GetMaxItemsForSlot(config.EquipmentSlotType);

			for(var i = 0; i < maxIndex; i++)
			{
				var existing = GetEquipment(config.EquipmentSlotType, i);
				if(existing == null)
				{
					index = i;
					break;
				}
			}

			EquipItem(item, config.EquipmentSlotType, index);
		}

		public static int GetMaxItemsForSlot(EquipmentSlotType configEquipmentSlotType)
		{
			switch(configEquipmentSlotType)
			{
				case EquipmentSlotType.RightHand:
					return 1;
				case EquipmentSlotType.LeftHand:
					return 1;
				case EquipmentSlotType.QuickUse:
					return 1;
				case EquipmentSlotType.Head:
					return 1;
				case EquipmentSlotType.Body:
					return 1;
				case EquipmentSlotType.Hands:
					return 1;
				case EquipmentSlotType.Legs:
					return 1;
				case EquipmentSlotType.Talisman:
					return 4;
				default:
					throw new ArgumentOutOfRangeException(nameof(configEquipmentSlotType), configEquipmentSlotType, null);
			}
		}

		public IEnumerable<BaseItemLogic> GetAllItems()
		{
			return _items;
		}

		public EquipmentSlotType GetBlockingWeaponSlot()
		{
			if(_equippedItems.TryGetValue(new EquipmentSlotAdress(EquipmentSlotType.LeftHand, 0), out var leftItem))
			{
				if(leftItem is WeaponItemLogic)
				{
					return EquipmentSlotType.LeftHand;
				}
			}

			return EquipmentSlotType.RightHand;
		}

		public bool CheckHasParryWeapon()
		{
			return TryGetParryWeapon(out _, out _);
		}

		public bool TryGetParryWeapon(out WeaponItemLogic parryWeapon, out EquipmentSlotType slot)
		{
			foreach(var parrySlot in ParrySlots)
			{
				if(EquippedItems.TryGetValue(new EquipmentSlotAdress(parrySlot, 0), out var item))
				{
					if(item is WeaponItemLogic weaponItem)
					{
						if(weaponItem.Config.CanParry)
						{
							parryWeapon = weaponItem;
							slot = parrySlot;
							return true;
						}
					}
				}
			}

			slot = EquipmentSlotType.Undefined;
			parryWeapon = null;
			return false;
		}

		public void HandleRespawn()
		{
			foreach(var baseItemLogic in _items)
			{
				baseItemLogic.HandleLocationRespawn();
			}
		}

		private void EnsureHandsNotNull()
		{
			if(GetEquipment(EquipmentSlotType.LeftHand) == null)
			{
				EquipItem(null, EquipmentSlotType.LeftHand, 0);
			}

			if(GetEquipment(EquipmentSlotType.RightHand) == null)
			{
				EquipItem(null, EquipmentSlotType.RightHand, 0);
			}
		}

		private BaseItemLogic CreateEmptyHandItem()
		{
			var configId = _context.Config.EmptyHandItemConfigId;
			if(string.IsNullOrEmpty(configId))
			{
				Debug.LogError("EmptyHandItemConfigId is not set in CharacterConfig!");
				return null;
			}

			var itemSaveData = new InventoryItemSaveData
			{
				ConfigId = configId,
				UniqueId = "EmptyHand_" + Guid.NewGuid()
			};
			var itemLogic = InventoryItemsFabric.CreateItemFromSave(itemSaveData);
			itemLogic.InitializeForLocation(_context);
			itemLogic.LoadData(itemSaveData);
			return itemLogic;
		}

		private void HandleConsumableItemPickup(InventoryItemSaveData itemSaveData, BaseConsumableItemLogic createdItemLogic)
		{
			if(TryGetExistingItem<BaseConsumableItemLogic>(itemSaveData.ConfigId, out var existingItemLogic))
			{
				existingItemLogic.HandlePickupAdditionalItem(itemSaveData);
			}
			else
			{
				_items.Add(createdItemLogic);
				createdItemLogic.InitializeForLocation(_context);
				createdItemLogic.LoadData(itemSaveData);
			}
		}

		private void HandleMainHealingItemPickup(InventoryItemSaveData itemSaveData, MainHealingItemLogic createdItemLogic)
		{
			if(TryGetExistingItem<MainHealingItemLogic>(out var existingItemLogic))
			{
				existingItemLogic.HandlePickupAdditionalItem(itemSaveData);
			}
			else
			{
				_items.Add(createdItemLogic);
				createdItemLogic.InitializeForLocation(_context);
				createdItemLogic.LoadData(itemSaveData);
				if(!_context.CurrentConsumableItem.HasValue)
				{
					if(createdItemLogic is MainHealingItemLogic mainHealingItem)
					{
						_context.CurrentConsumableItem.Value = mainHealingItem;
					}
				}
			}
		}

		private bool TryGetExistingItem<T>(string itemId, out T result) where T : BaseItemLogic
		{
			foreach(var item in _items)
			{
				if(item is T tItem)
				{
					if(tItem.ConfigId == itemId)
					{
						result = tItem;
						return true;
					}
				}
			}
			result = null;
			return false;
		}

		private bool TryGetExistingItem<T>(out T result) where T : BaseItemLogic
		{
			foreach(var item in _items)
			{
				if(item is T tItem)
				{
					result = tItem;
					return true;
				}
			}
			result = null;
			return false;
		}
	}
}
