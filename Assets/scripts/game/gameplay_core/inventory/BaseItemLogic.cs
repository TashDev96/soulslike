using game.gameplay_core.inventory.serialized_data;

namespace game.gameplay_core.inventory
{
	public abstract class BaseItemLogic
	{
		protected InventoryItemSaveData SaveableData;
		
		public virtual void LoadData(InventoryItemSaveData saveData)
		{
			SaveableData = saveData;
		}

		public abstract void SaveData();
	}
}
