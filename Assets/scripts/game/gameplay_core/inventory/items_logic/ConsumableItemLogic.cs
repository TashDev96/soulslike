using game.gameplay_core.inventory.item_configs;

namespace game.gameplay_core.inventory
{
	public class ConsumableItemLogic
	{
		private readonly ConsumableItemConfig _config;

		public ConsumableItemLogic(ConsumableItemConfig config)
		{
			_config = config;
		}
	}
}
