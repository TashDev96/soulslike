using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using game.gameplay_core.inventory.serialized_data;

namespace game.gameplay_core.inventory
{
	public class InventoryDomain
	{
		private bool _sceneDebugMode;

		public InventoryData InventoryData { get; private set; }
		public List<InventoryItemSaveData> InventoryItemsData { get; private set; }

		public async UniTask Initialize(bool sceneDebugMode)
		{
			_sceneDebugMode = sceneDebugMode;
			InventoryData = GameStaticContext.Instance.PlayerSave.InventoryData;
			InventoryItemsData = InventoryData.Items;

			await UniTask.Delay(1);
		}

		public void SaveInventory()
		{
			if(_sceneDebugMode)
			{
			}

			//todo save inventory to file
		}
	}
}
