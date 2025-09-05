using System;
using game.gameplay_core.inventory.item_configs;
using game.gameplay_core.inventory.serialized_data;

namespace game.gameplay_core.inventory
{
	public class InventoryItemsFabric
	{
		public static BaseItemLogic CreateItem(InventoryItemSaveData saveableData)
		{
			var config = AddressableManager.LoadAssetImmediately<BaseItemConfig>(saveableData.ConfigId, AssetOwner.Game);
			switch(config)
			{
				case WeaponItemConfig weaponItemConfig:
					return new WeaponItemLogic(weaponItemConfig);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(config));
			}
		}
	}
}
