using System.Collections.Generic;
using game.gameplay_core.inventory;

namespace game.gameplay_core.characters.logic
{
	public class InventoryLogic
	{
		
		private List<BaseItemLogic> _items = new();
		
		
		public void Initialize(CharacterContext context)
		{
			var items = GameStaticContext.Instance.InventoryDomain.InventoryItemsData;

			foreach(var itemSaveData in items)
			{
				var itemLogic = InventoryItemsFabric.CreateItem(itemSaveData);
				itemLogic.LoadData(itemSaveData);
				_items.Add(itemLogic);
			}
			
		}
	}
}
