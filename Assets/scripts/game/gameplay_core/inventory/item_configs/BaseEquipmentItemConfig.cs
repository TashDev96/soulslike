using game.enums;
using UnityEngine;

namespace game.gameplay_core.inventory.item_configs
{
	public class BaseEquipmentItemConfig : BaseItemConfig
	{
		[field: SerializeField]
		public EquipmentSlotType EquipmentSlotType { get; private set; }
	}
}
