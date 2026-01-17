using System.Collections.Generic;
using dream_lib.src.utils.serialization;
using game.gameplay_core.camera;
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
		public CameraSettings CameraSettings { get; private set; }
		[field: SerializeField]
		public Transform TestPlayerSpawnPos { get; private set; }

		[field: SerializeField]
		public bool OnlySpawnPlayer { get; private set; }
		
		[field: SerializeField]
		private bool OnlySpawnCustomEnemies { get; set; }
		[field: SerializeField]
		[field:ShowIf("OnlySpawnCustomEnemies")]
		private CharacterDomain[] CustomEnemies { get; set; }

#if UNITY_EDITOR

		private void Awake()
		{
			FindObjectsOnScene();
		}

		[Button]
		private void FindObjectsOnScene()
		{
			SavableObjects = FindObjectsByType<SceneSavableObjectBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			Characters = FindObjectsByType<CharacterDomain>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			if(OnlySpawnCustomEnemies)
			{
				Characters = CustomEnemies;
			}
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
