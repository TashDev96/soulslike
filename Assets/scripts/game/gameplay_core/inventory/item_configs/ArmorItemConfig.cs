using game.enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.inventory.item_configs
{
	[AddressableAssetTag(nameof(AddressableCollections.ItemConfigs))]
	[CreateAssetMenu(menuName = "Configs/Items/Armor")]
	public class ArmorItemConfig : BaseEquipmentItemConfig
	{
		[field: FoldoutGroup("Stats")]
		[field: SerializeField]
		public float PhysicalDefense { get; private set; }
		[field: FoldoutGroup("Stats")]
		[field: SerializeField]
		public float MagicDefense { get; private set; }
		[field: FoldoutGroup("Stats")]
		[field: SerializeField]
		public float Poise { get; private set; }

		// Add visual prefabs or skinned mesh references here if needed
	}
}
