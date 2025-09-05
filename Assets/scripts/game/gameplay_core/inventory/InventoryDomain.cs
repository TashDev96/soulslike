using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using game.gameplay_core.debug;
using game.gameplay_core.inventory.serialized_data;
using UnityEngine;

namespace game.gameplay_core.inventory
{
	public class InventoryDomain
	{
		private bool _sceneDebugMode;
		
		public List<InventoryItemSaveData> InventoryItemsData { get; private set; }

		public async UniTask Initialize(bool sceneDebugMode)
		{
			_sceneDebugMode = sceneDebugMode;

#if UNITY_EDITOR
			if(_sceneDebugMode)
			{
				LoadTestInventory();
			}
#endif
			//TODOload inventory save data
		}

		public void SaveInventory()
		{
			if(_sceneDebugMode)
			{
				return;
			}
			
			//todo save inventory to file
		}

 

		private void LoadTestInventory()
		{
			var charConfig = Object.FindAnyObjectByType<DebugSceneCharacterConfig>(FindObjectsInactive.Include);
			InventoryItemsData = charConfig.InventoryData;
		}
	}
}
