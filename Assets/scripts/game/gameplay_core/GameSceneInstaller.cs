using dream_lib.src.utils.serialization;
using game.gameplay_core.characters;
using game.gameplay_core.location.location_save_system;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core
{
	[ExecuteInEditMode]
	public class GameSceneInstaller : MonoBehaviour
	{
		[field: SerializeField]
		public SceneSavableObjectBase[] SavableObjects { get; private set; }
		[field: SerializeField]
		public CharacterDomain[] Characters { get; private set; }
		[field: SerializeField]
		public Camera MainCamera { get; private set; }
		[field: SerializeField]
		public Transform TestPlayerSpawnPos { get; private set; }

		[field: SerializeField]
		public bool OnlySpawnPlayer { get; private set; }

#if UNITY_EDITOR

		[Button]
		private void FindObjectsOnScene()
		{
			SavableObjects = FindObjectsByType<SceneSavableObjectBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			Characters = FindObjectsByType<CharacterDomain>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
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
