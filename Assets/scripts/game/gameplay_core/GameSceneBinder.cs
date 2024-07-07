using dream_lib.src.utils.serialization;
using game.gameplay_core.characters;
using game.gameplay_core.location_save_system;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core
{
	[ExecuteInEditMode]
	public class GameSceneBinder : MonoBehaviour
	{
		[SerializeField]
		private SceneSavableObjectBase[] _allSavableObjects;
		[SerializeField]
		private CharacterDomain[] _allCharacters;
		[SerializeField]
		private Camera _mainCamera;

		public void BindObjects(LocationContext locationContext)
		{
			locationContext.SceneSavableObjects = _allSavableObjects;
			locationContext.Characters = _allCharacters;
			locationContext.MainCamera.Value = _mainCamera;
		}

#if UNITY_EDITOR

		[Button]
		private void FindObjectsOnScene()
		{
			_allSavableObjects = FindObjectsByType<SceneSavableObjectBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			_allCharacters = FindObjectsByType<CharacterDomain>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
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
	}
}
