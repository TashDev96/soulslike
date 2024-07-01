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
			var saveData = locationContext.LocationSaveData;

			foreach(var sceneSavableObject in _allSavableObjects)
			{
				if(!saveData.SavableObjects.ContainsKey(sceneSavableObject.UniqueId))
				{
					sceneSavableObject.InitializeFirstTime();
					saveData.SavableObjects.Add(sceneSavableObject.UniqueId, sceneSavableObject.GetSave());
				}
			}
		}
	}
}
