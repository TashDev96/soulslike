using System.Collections.Generic;
using game.enums;
using game.gameplay_core.inventory;
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

		private readonly Dictionary<ArmamentSlot, BaseItemLogic> _equippedItems = new();
		public IReadOnlyDictionary<ArmamentSlot, BaseItemLogic> EquippedItems => _equippedItems;

		private ArmamentSlot[] ParrySlots => new[] { ArmamentSlot.Left, ArmamentSlot.Right };

		public WeaponItemLogic RightWeapon => _equippedItems[ArmamentSlot.Right] as WeaponItemLogic;
		public WeaponItemLogic LeftWeapon => _equippedItems[ArmamentSlot.Left] as WeaponItemLogic;

		public void Initialize(CharacterContext context, InventoryData data)
		{
			_context = context;
			_data = data;
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
				_equippedItems[kvp.Key] = _items.Find(item => item.UniqueId == kvp.Value);
			}
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
		

		public BaseItemLogic GetArmament(ArmamentSlot slot)
		{
			return _equippedItems.GetValueOrDefault(slot);
		}

		public ArmamentSlot GetBlockingWeaponSlot()
		{
			if(_equippedItems.TryGetValue(ArmamentSlot.Left, out var leftItem))
			{
				if(leftItem is WeaponItemLogic)
				{
					return ArmamentSlot.Left;
				}
			}

			return ArmamentSlot.Right;
		}

		public bool CheckHasParryWeapon()
		{
			return TryGetParryWeapon(out _, out _);
		}

		public bool TryGetParryWeapon(out WeaponItemLogic parryWeapon, out ArmamentSlot slot)
		{
			foreach(var parrySlot in ParrySlots)
			{
				if(EquippedItems.TryGetValue(parrySlot, out var item))
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

			slot = ArmamentSlot.Undefined;
			parryWeapon = null;
			return false;
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

				Debug.Log("picked healing item first time");
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
