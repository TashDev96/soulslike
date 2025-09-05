using System;
using System.Collections.Generic;

namespace game.gameplay_core.inventory.serialized_data
{
	[Serializable]
	public class InventoryData
	{
		public List<InventoryItemSaveData> Items;
	}
}
