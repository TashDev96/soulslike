using game.gameplay_core.inventory.item_configs;

namespace game.gameplay_core.inventory.items_logic
{
	public class ResourceItemLogic : BaseItemLogic
	{
		public ResourceItemConfig Config { get; }
		public override BaseItemConfig BaseConfig => Config;

		public ResourceItemLogic(ResourceItemConfig config)
		{
			Config = config;
		}

		public override void SaveData()
		{
		}

		public override void GetCountData(out bool countAvailable, out int count)
		{
			countAvailable = false;
			count = 0;
		}
	}
}
