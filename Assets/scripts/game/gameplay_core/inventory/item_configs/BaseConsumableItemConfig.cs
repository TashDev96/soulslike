using UnityEngine;

namespace game.gameplay_core.inventory.item_configs
{
	public class BaseConsumableItemConfig : BaseItemConfig
	{
		[field: SerializeField]
		public bool HasInfiniteCharges { get; private set; }
		[field: SerializeField]
		public int ChargesCount { get; private set; }
		
		[field:SerializeField]
		public ItemAnimationConfig AnimationConfig { get; private set; }
	}
}
