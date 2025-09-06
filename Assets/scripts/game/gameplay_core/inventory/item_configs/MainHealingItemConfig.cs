using UnityEngine;

namespace game.gameplay_core.inventory.item_configs
{
	public class MainHealingItemConfig : BaseConsumableItemConfig
	{
		[field: SerializeField]
		public float BaseHealingAmount { get; private set; }
		[field: SerializeField]
		public AnimationCurve HealingOverTime { get; private set; }
	}
}
