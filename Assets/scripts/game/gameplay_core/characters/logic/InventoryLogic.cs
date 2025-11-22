using System;
using System.Collections.Generic;
using dream_lib.src.extensions;
using game.gameplay_core.inventory;
using game.gameplay_core.inventory.items_logic;
using game.gameplay_core.inventory.serialized_data;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class InventoryLogic
	{
		private readonly List<BaseItemLogic> _items = new();
		private CharacterContext _context;

		public void Initialize(CharacterContext context, List<InventoryItemSaveData> saveData)
		{
			_context = context;
			foreach(var itemSaveData in saveData)
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
