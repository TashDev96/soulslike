using game.gameplay_core.inventory.item_configs;
using game.gameplay_core.inventory.serialized_data;

namespace game.gameplay_core.inventory
{
	public class WeaponItemLogic : BaseItemLogic
	{
		private const string DurabilityKey = "durability";

		public readonly WeaponItemConfig Config;

		public float Durability;

		public WeaponItemLogic(WeaponItemConfig config)
		{
			Config = config;
		}

		public override void LoadData(InventoryItemSaveData saveData)
		{
			base.LoadData(saveData);
			if(saveData.IsInitialized)
			{
				Durability = SaveableData.GetFloat(DurabilityKey);
			}
			else
			{
				Durability = 100f;
				SaveData();
			}
		}

		public override void SaveData()
		{
			SaveableData.SetFloat(DurabilityKey, Durability);
		}
	}
}
