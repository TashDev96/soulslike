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
			locationContext.SceneSavableObjects = _allSavableObjects;
		}
	}
}
