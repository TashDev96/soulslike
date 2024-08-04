using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using dream_lib.src.reactive;
using dream_lib.src.utils.components;
using game.gameplay_core.characters;
using game.gameplay_core.location_save_system;
using UnityEngine;

namespace game.gameplay_core
{
	public class LocationDomain
	{
		private LocationContext _locationContext = new();
		private UnityEventsListener _unityEventsListener;
		private GameSceneInstaller _sceneInstaller;

		public void Initialize()
		{
			InitializeAsync().Forget();
		}

		private async UniTask InitializeAsync()
		{
			_sceneInstaller = Object.FindAnyObjectByType<GameSceneInstaller>();

			_locationContext = new LocationContext
			{
				LocationSaveData = new LocationSaveData(),
				LocationUpdate = new ReactiveCommand<float>(),
				MainCamera = new ReactiveProperty<Camera>(_sceneInstaller.MainCamera)
			};

			LoadSceneObjects();
			LoadSpawnedObjects();
			LoadCharacters();

			_unityEventsListener = UnityEventsListener.Create("__locationDomainUnityEvents");
			_unityEventsListener.OnUpdate += HandleUpdate;
		}

		private void HandleUpdate()
		{
			_locationContext.LocationUpdate.Execute(Time.deltaTime);
		}

		private void LoadCharacters()
		{
			_locationContext.Characters = new List<CharacterDomain>();
			var playerPrefab = AddressableManager.GetPreloadedAsset<GameObject>(AddressableAssetNames.Player);
			var player = Object.Instantiate(playerPrefab).GetComponent<CharacterDomain>();
			player.Initialize(_locationContext);

			foreach(var character in _sceneInstaller.Characters)
			{
				if(!character.gameObject.activeSelf)
				{
					continue;
				}
				character.Initialize(_locationContext);
				_locationContext.Characters.Add(character);
			}

			_locationContext.Characters.Add(player);
		}

		private void LoadSceneObjects()
		{
			_locationContext.SceneSavableObjects = _sceneInstaller.SavableObjects.ToArray();

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
