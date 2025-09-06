using System.Collections.Generic;
using game.gameplay_core.inventory;
using game.gameplay_core.inventory.items_logic;
using game.gameplay_core.inventory.serialized_data;

namespace game.gameplay_core.characters.logic
{
	public class InventoryLogic
	{
		private readonly List<BaseItemLogic> _items = new();

		public void Initialize(CharacterContext context, List<InventoryItemSaveData> saveData)
		{
			foreach(var itemSaveData in saveData)
			{
				var itemLogic = InventoryItemsFabric.CreateItem(itemSaveData);
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
	}
}
