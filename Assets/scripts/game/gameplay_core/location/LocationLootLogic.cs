using System.Collections.Generic;
using game.gameplay_core.characters;
using game.gameplay_core.location.interactive_objects;
using game.gameplay_core.location.view;
using UnityEngine;

namespace game.gameplay_core.location
{
	public class LocationLootLogic
	{
		public void TrySpawnLoot(Vector3 position, List<LootConfig> lootList)
		{
			position += Vector3.up * 0.5f;
			foreach(var lootConfig in lootList)
			{
				if(lootConfig.DropsOnlyOnce)
				{
					if(GameStaticContext.Instance.PlayerSave.UniqueLootDropped.Contains(lootConfig.DropOnceId))
					{
						continue;
					}
				}

				if(Random.Range(0, 100) < lootConfig.DropChance)
				{
					SpawnLootGameObject(position, lootConfig);

					if(lootConfig.DropsOnlyOnce)
					{
						GameStaticContext.Instance.PlayerSave.UniqueLootDropped.Add(lootConfig.DropOnceId);
					}
				}
			}
		}

		public void HandleLootInteracted(LootItemSaveData saveData, CharacterDomain interactedCharacter)
		{
			//both pick up and remove happens at the same time, so we are ok to wait 5 sec to next auto save
			interactedCharacter.Context.Logic.InventoryLogic.PickUpItem(saveData.Item);

			var locationSave = LocationStaticContext.Instance.LocationSaveData;
			for(var i = 0; i < locationSave.SpawnedObjects.Count; i++)
			{
				if(locationSave.SpawnedObjects[i].UniqueId == saveData.UniqueId)
				{
					locationSave.SpawnedObjects.RemoveAt(i--);
				}
			}
		}

		private void SpawnLootGameObject(Vector3 position, LootConfig lootConfig)
		{
			var prefab = AddressableManager.GetPreloadedAsset<GameObject>(lootConfig.LootVfxPrefab);

			var lootView = Object.Instantiate(prefab, position, Quaternion.identity).GetComponent<LootItem>();

			lootView.InitializeFromLoot(lootConfig);

			var locationSave = LocationStaticContext.Instance.LocationSaveData;
			locationSave.SceneObjects.Add(lootView.UniqueId, lootView.GetSave());
		}
	}
}
