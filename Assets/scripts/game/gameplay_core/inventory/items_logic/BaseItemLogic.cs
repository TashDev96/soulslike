using game.gameplay_core.characters;
using game.gameplay_core.inventory.serialized_data;

namespace game.gameplay_core.inventory.items_logic
{
	public abstract class BaseItemLogic
	{
		protected InventoryItemSaveData SaveableData;

		public abstract string ConfigId { get; }
		public string UniqueId => SaveableData.UniqueId;

		public virtual void InitializeForLocation(CharacterContext context)
		{
		}

		public virtual void LoadData(InventoryItemSaveData saveData)
		{
			SaveableData = saveData;
		}

		public abstract void SaveData();
	}
}
