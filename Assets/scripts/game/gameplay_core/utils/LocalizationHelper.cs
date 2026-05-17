using game.gameplay_core.inventory.item_configs;

namespace game.gameplay_core.utils
{
	public class LocalizationHelper
	{
		public static string BuildItemPickupLocale(BaseItemConfig baseConfig)
		{
			return $"Picked Up {baseConfig.name}";
		}
	}
}
