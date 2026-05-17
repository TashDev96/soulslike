using game.gameplay_core.inventory.item_configs;

namespace game.gameplay_core.inventory.items_logic
{
	public class KeyItemLogic : BaseItemLogic
	{
		public KeyItemConfig Config { get; }

		public override BaseItemConfig BaseConfig => Config;

		public KeyItemLogic(KeyItemConfig config)
		{
			Config = config;
		}

		public override void SaveData()
		{
		}

		public override void GetCountData(out bool countAvailable, out int count)
		{
			countAvailable = false;
			count = 1;
		}
	}
}
