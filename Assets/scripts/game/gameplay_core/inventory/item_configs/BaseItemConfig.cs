using game.enums;
using UnityEngine;

namespace game.gameplay_core.inventory.item_configs
{
	[AddressableAssetTag(nameof(AddressableCollections.ItemConfigs))]
	public class BaseItemConfig : ScriptableObject
	{
		[field: SerializeField]
		public Sprite Icon { get; private set; }
	}
}
