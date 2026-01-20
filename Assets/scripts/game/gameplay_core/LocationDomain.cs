using System.Collections.Generic;
using System.Linq;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.utils.components;
using dream_lib.src.utils.data_types;
using dream_lib.src.utils.editor;
using game.gameplay_core.camera;
using game.gameplay_core.characters;
using game.gameplay_core.location.interactive_objects;
using game.gameplay_core.location.location_save_system;
using game.gameplay_core.worldspace_ui;
using game.ui;
using UnityEngine;
using Object = UnityEngine.Object;

namespace game.gameplay_core
{
	public class LocationDomain
	{
		private UnityEventsListener _unityEventsListener;
		private GameSceneInstaller _sceneInstaller;
		private ICameraController _cameraController;
		private readonly ReactiveProperty<CharacterDomain> _player = new();

		private float _frameDelayDebug;

		public void Initialize(LocationSaveData saveData)
		{
			_sceneInstaller = Object.FindAnyObjectByType<GameSceneInstaller>();

			var mainCamera = new ReactiveProperty<Camera>(_sceneInstaller.MainCamera);
			_cameraController = CameraControllerFactory.Create(_sceneInstaller.CameraSettings, mainCamera, _player);

			LocationStaticContext.Instance = new LocationStaticContext
			{
				LocationSaveData = saveData,
				LocationUpdate = new ReactiveCommand<float>(),
				LocationUiUpdate = new ReactiveCommand<float>(),
				LocationTime = new ReactiveProperty<float>(),
				CameraController = _cameraController
			};

			GameStaticContext.Instance.MainCamera.Value = _sceneInstaller.MainCamera;
			GameStaticContext.Instance.CurrentLocationUpdate = LocationStaticContext.Instance.LocationUpdate;
			GameStaticContext.Instance.FloatingTextsManager = new FloatingTextsManager(LocationStaticContext.Instance.CameraController, LocationStaticContext.Instance.LocationUpdate);

			LoadSceneObjects();
			LoadSpawnedObjects();
			LoadCharacters();

			_unityEventsListener = UnityEventsListener.Create("__locationDomainUnityEvents");
			_unityEventsListener.OnUpdate += HandleUpdate;

			GameStaticContext.Instance.UiDomain.ShowLocationUi(new UiLocationHUD.Context
			{
				Player = _player.Value,
				LocationUiUpdate = LocationStaticContext.Instance.LocationUiUpdate
			});

			RegisterCheats();
		}

		public void RespawnAndReloadLocation()
		{
			_player.Value.SetRespawnTransform(new TransformCache(_player.Value.transform));

			foreach(var character in LocationStaticContext.Instance.Characters)
			{
				character.HandleLocationRespawn();
			}
			SaveCurrentStateToData();
		}

		public LocationSaveData SaveCurrentStateToData()
		{
			foreach(var character in LocationStaticContext.Instance.Characters)
			{
				character.WriteStateToSaveData();
			}

			return LocationStaticContext.Instance.LocationSaveData;
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
				LocationStaticContext.Instance.LocationTime.Value += deltaTime;
				LocationStaticContext.Instance.LocationUpdate.Execute(deltaTime);
				_cameraController.Update(deltaTime);
#if UNITY_EDITOR
				_frameDelayDebug = EditorComfortWindow.FrameDelay;
#endif
			}

			LocationStaticContext.Instance.LocationUiUpdate.Execute(deltaTime);
		}

		private void LoadCharacters()
		{
			LocationStaticContext.Instance.Characters = new List<CharacterDomain>();

			var playerPrefab = AddressableManager.GetPreloadedAsset<GameObject>(AddressableAssetNames.Player);
			_player.Value = Object.Instantiate(playerPrefab).GetComponent<CharacterDomain>();
			LocationStaticContext.Instance.Player = _player.Value;
			_player.Value.Initialize();

			var playerSave = GameStaticContext.Instance.PlayerSave;
			var isFirstTimePlayerCreated = false;
			if(_sceneInstaller.TestPlayerSpawnPos != null && !playerSave.CharacterData.Initialized)
			{
				playerSave.CharacterData.Position = _sceneInstaller.TestPlayerSpawnPos.position;
				playerSave.CharacterData.Euler = _sceneInstaller.TestPlayerSpawnPos.eulerAngles;
				playerSave.CharacterData.Initialized = true;
				playerSave.RespawnTransform = new TransformCache(_sceneInstaller.TestPlayerSpawnPos);
				isFirstTimePlayerCreated = true;
			}
			_player.Value.SetSaveData(playerSave.CharacterData, isFirstTimePlayerCreated);

			LocationStaticContext.Instance.Characters.Add(_player.Value);

			if(!_sceneInstaller.OnlySpawnPlayer)
			{
				foreach(var character in _sceneInstaller.Characters)
				{
					if(!character.gameObject.activeSelf)
					{
						continue;
					}
					character.Initialize();
					if(LocationStaticContext.Instance.LocationSaveData.Enemies.TryGetValue(character.UniqueId, out var enemySaveData))
					{
						character.SetSaveData(enemySaveData);
					}
					else
					{
						var saveData = new CharacterSaveData();
						character.SetSaveData(saveData);
						LocationStaticContext.Instance.LocationSaveData.Enemies.Add(character.UniqueId, saveData);
					}
					LocationStaticContext.Instance.Characters.Add(character);
				}
			}
		}

		private void LoadSceneObjects()
		{
			LocationStaticContext.Instance.SceneSavableObjects = _sceneInstaller.SavableObjects.ToArray();

			var locationSave = LocationStaticContext.Instance.LocationSaveData;

			var usedIds = new HashSet<string>();

			foreach(var sceneSavableObject in LocationStaticContext.Instance.SceneSavableObjects)
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
						bonfire.SetContext(LocationStaticContext.Instance.Player);
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
			var locationSave = LocationStaticContext.Instance.LocationSaveData;

			foreach(var spawnedObjectSave in locationSave.SpawnedObjects)
			{
				var prefab = Resources.Load<GameObject>(spawnedObjectSave.PrefabName);
				var instance = Object.Instantiate(prefab);

				var spawnedObjectController = new SpawnedObjectController
				{
					SceneInstance = instance.GetComponent<SceneSavableObjectBase>()
				};

				spawnedObjectController.LoadSave(spawnedObjectSave);
				LocationStaticContext.Instance.SpawnedObjects.Add(spawnedObjectController);
			}
		}

		private void RegisterCheats()
		{
			EditorComfortWindow.RegisterCheatButton("Return to Spawn", () => { _player.Value.transform.SetTo(_sceneInstaller.TestPlayerSpawnPos); });
		}
	}
}
