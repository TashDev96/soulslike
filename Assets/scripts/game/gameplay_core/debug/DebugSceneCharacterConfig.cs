using game.gameplay_core.inventory.serialized_data;
using UnityEngine;

namespace game.gameplay_core.debug
{
	public class DebugSceneCharacterConfig : MonoBehaviour
	{
		[field: SerializeField]
		public InventoryData InventoryData { get; set; }
	}
}
