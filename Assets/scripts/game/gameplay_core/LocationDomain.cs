using System.Collections.Generic;
using System.Linq;
using game.gameplay_core.location_save_system;
using UnityEngine;

namespace game.gameplay_core
{
	public class LocationDomain
	{
		private LocationContext _locationContext = new();

		public void Initialize()
		{
			var sceneBinder = Object.FindAnyObjectByType<GameSceneBinder>();
			_locationContext = new LocationContext();

			_locationContext.LocationSaveData = new LocationSaveData();

			//TODO Load Saved Data

			sceneBinder.BindObjects(_locationContext);

			LoadSceneObjects();
			LoadSpawnedObjects();
		}

		private void LoadSceneObjects()
		{
			var locationSave = _locationContext.LocationSaveData;

			var usedIds = new HashSet<string>();

			foreach(var sceneSavableObject in _locationContext.SceneSavableObjects)
			{
				var objectId = sceneSavableObject.UniqueId;

				if(locationSave.SceneObjects.TryGetValue(objectId, out var objectSave))
				{
					sceneSavableObject.LoadSave(objectSave);
				}
				else
				{
					sceneSavableObject.InitializeFirstTime();
					locationSave.SceneObjects.Add(objectId, sceneSavableObject.GetSave());
				}

				usedIds.Add(objectId);
			}

			var keysInSave = locationSave.SceneObjects.Keys.ToArray();

			foreach(var keyInSave in keysInSave)
			{
				if(usedIds.Contains(keyInSave))
				{
					Debug.LogWarning($"remove from save unused object id {keyInSave}");
					locationSave.SceneObjects.Remove(keyInSave);
				}
			}
		}

		private void LoadSpawnedObjects()
		{
			var locationSave = _locationContext.LocationSaveData;

			foreach(var spawnedObjectSave in locationSave.SpawnedObjects)
			{
				var prefab = Resources.Load<GameObject>(spawnedObjectSave.PrefabName);
				var instance = Object.Instantiate(prefab);

				var spawnedObjectController = new SpawnedObjectController
				{
					SceneInstance = instance.GetComponent<SceneSavableObjectBase>()
				};

				spawnedObjectController.LoadSave(spawnedObjectSave);
				_locationContext.SpawnedObjects.Add(spawnedObjectController);
			}
		}
	}
}
