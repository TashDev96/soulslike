using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using src.editor;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core
{
	[ExecuteInEditMode]
	public class GameSceneBinder : MonoBehaviour
	{
		[SerializeField]
		private SceneSavableObjectBase[] _allSavableObjects;
		private LocationContext _locationContext;

#if UNITY_EDITOR

		[Button]
		private void FindObjectsOnScene()
		{
			_allSavableObjects = FindObjectsByType<SceneSavableObjectBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		}

		private void OnEnable()
		{
			if(!EditorApplication.isPlaying)
			{
				SceneChangesInEditorTracker.OnAnyComponentCreateOrDelete += HandleComponentCreateOrDelete;
			}
		}

		private void OnDisable()
		{
			SceneChangesInEditorTracker.OnAnyComponentCreateOrDelete -= HandleComponentCreateOrDelete;
		}

		private void HandleComponentCreateOrDelete(GameObject _)
		{
			FindObjectsOnScene();
		}
#endif

		public void BindObjects(LocationContext locationContext)
		{
			_locationContext = locationContext;

			LoadLocationObjects();
			LoadSpawnedObjects();
		}

		private void LoadSpawnedObjects()
		{
			var locationSave = _locationContext.LocationSaveData;

			foreach(var spawnedObjectSave in locationSave.SpawnedObjects)
			{
				var prefab = Resources.Load<GameObject>(spawnedObjectSave.PrefabName);
				var instance = Instantiate(prefab);

				var spawnedObjectController = new SpawnedObjectController()
				{
					SceneInstance = instance.GetComponent<SceneSavableObjectBase>(),
				};

				spawnedObjectController.LoadSave(spawnedObjectSave);
				_locationContext.SpawnedObjects.Add(spawnedObjectController);
			}
		}

		private void LoadLocationObjects()
		{
			var locationSave = _locationContext.LocationSaveData;

			var usedIds = new HashSet<string>();

			foreach(var sceneSavableObject in _allSavableObjects)
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
	}
}
