using game.enums;
using UnityEngine;

namespace game.gameplay_core.inventory.item_configs
{
	[CreateAssetMenu(menuName = "Configs/Items/Key")]
	public class KeyItemConfig : BaseItemConfig
	{
		[field: SerializeField]
		public KeyId Key { get; private set; }
	}
}
