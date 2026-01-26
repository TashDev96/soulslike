using game.enums;
using UnityEngine;

namespace game.gameplay_core.inventory.item_configs
{
	[AddressableAssetTag(nameof(AddressableCollections.ItemConfigs))]
	[CreateAssetMenu(menuName = "Configs/TalismanConfig")]
	public class TalismanItemConfig : BaseEquipmentItemConfig
	{
	}
}
