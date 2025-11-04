using System.Collections.Generic;
using game.gameplay_core.inventory.serialized_data;
using UnityEngine;

namespace game.gameplay_core.inventory
{
	public class NpcInventoryConfigView : MonoBehaviour
	{
		[field: SerializeField]
		public List<InventoryItemSaveData> Items { get; private set; }
	}
}
