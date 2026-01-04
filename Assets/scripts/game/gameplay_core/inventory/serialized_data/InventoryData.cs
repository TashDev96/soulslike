using System;
using System.Collections.Generic;
using dream_lib.src.utils.data_types;
using game.enums;

namespace game.gameplay_core.inventory.serialized_data
{
	[Serializable]
	public class InventoryData
	{
		public List<InventoryItemSaveData> Items;
		public SerializableDictionary<ArmamentSlot, string> EquippedItems;
	}
}
