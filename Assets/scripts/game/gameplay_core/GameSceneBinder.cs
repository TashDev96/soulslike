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
		public List<SavableSceneObjectAbstract> _savableObjects;

		[Button]
		private void FindObjectsOnScene()
		{
			_savableObjects = FindObjectsByType<SavableSceneObjectAbstract>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();

		}

#if UNITY_EDITOR

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

		private void HandleComponentCreateOrDelete(GameObject obj)
		{
			FindObjectsOnScene();
		}
#endif

		//enemies

		//interactive objects
		//spawn points
		//loot
		//doors
		//elevators
		//ladders

		//destructible objects
	}
}
