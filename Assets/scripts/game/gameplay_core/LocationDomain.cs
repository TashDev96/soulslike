using System.Collections.Generic;
using System.Linq;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.utils.components;
using dream_lib.src.utils.editor;
using game.gameplay_core.camera;
using game.gameplay_core.characters;
using game.gameplay_core.location.interactive_objects;
using game.gameplay_core.location.location_save_system;
using game.ui;
using UnityEngine;
using Object = UnityEngine.Object;

namespace game.gameplay_core
{
	public class LocationDomain
	{
		private LocationContext _locationContext;
		private UnityEventsListener _unityEventsListener;
		private GameSceneInstaller _sceneInstaller;
		private ICameraController _cameraController;
		private readonly ReactiveProperty<CharacterDomain> _player = new();

		private float _frameDelayDebug;

		public void Initialize()
		{
			_sceneInstaller = Object.FindAnyObjectByType<GameSceneInstaller>();

			_locationContext = new LocationContext
			{
				LocationSaveData = new LocationSaveData(),
				LocationUpdate = new ReactiveCommand<float>(),
				LocationUiUpdate = new ReactiveCommand<float>(),
				MainCamera = new ReactiveProperty<Camera>(_sceneInstaller.MainCamera),
				LocationTime = new ReactiveProperty<float>()
			};

			GameStaticContext.Instance.MainCamera.Value = _sceneInstaller.MainCamera;

			_cameraController = CameraControllerFactory.Create(_sceneInstaller.CameraSettings, _locationContext.MainCamera, _player);

			LoadSceneObjects();
			LoadSpawnedObjects();
			LoadCharacters();

			_unityEventsListener = UnityEventsListener.Create("__locationDomainUnityEvents");
			_unityEventsListener.OnUpdate += HandleUpdate;

			GameStaticContext.Instance.UiDomain.ShowLocationUi(new UiLocationHUD.Context
			{
				Player = _player.Value,
				LocationUiUpdate = _locationContext.LocationUiUpdate
			});

			RegisterCheats();
		}

		private void HandleUpdate()
		{
			var deltaTime = Time.deltaTime;
			if(_frameDelayDebug > 0)
			{
				_frameDelayDebug -= Time.unscaledDeltaTime;
			}
			else
			{
				_locationContext.LocationTime.Value += deltaTime;
				_locationContext.LocationUpdate.Execute(deltaTime);
				_cameraController.Update(deltaTime);
#if UNITY_EDITOR
				_frameDelayDebug = EditorComfortWindow.FrameDelay;
#endif
			}

			_locationContext.LocationUiUpdate.Execute(deltaTime);
		}

		private void LoadCharacters()
		{
			_locationContext.Characters = new List<CharacterDomain>();
			var playerPrefab = AddressableManager.GetPreloadedAsset<GameObject>(AddressableAssetNames.Player);
			_player.Value = Object.Instantiate(playerPrefab).GetComponent<CharacterDomain>();
			_player.Value.Initialize(_locationContext);
			if(_sceneInstaller.TestPlayerSpawnPos != null)
			{
				_player.Value.transform.SetTo(_sceneInstaller.TestPlayerSpawnPos);
			}
			_locationContext.Characters.Add(_player.Value);

			if(!_sceneInstaller.OnlySpawnPlayer)
			{
				foreach(var character in _sceneInstaller.Characters)
				{
					if(!character.gameObject.activeSelf)
					{
						continue;
					}
					character.Initialize(_locationContext);
					_locationContext.Characters.Add(character);
				}
			}
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

				switch(sceneSavableObject)
				{
					case PickupItem pickupItem:
						//pickupItem.SetContext(_locationContext);
						break;
					case Bonfire bonfire:
						break;
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

		private void RegisterCheats()
		{
			EditorComfortWindow.RegisterCheatButton("Return to Spawn", () => { _player.Value.transform.SetTo(_sceneInstaller.TestPlayerSpawnPos); });
		}
	}
}
