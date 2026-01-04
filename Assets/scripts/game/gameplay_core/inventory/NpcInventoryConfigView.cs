using game.gameplay_core.inventory.serialized_data;
using UnityEngine;

namespace game.gameplay_core.inventory
{
	public class NpcInventoryConfigView : MonoBehaviour
	{
		[field: SerializeField]
		public InventoryData InventoryData { get; private set; }
	}
}
