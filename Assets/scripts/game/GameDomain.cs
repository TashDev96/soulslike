using application;
using Cysharp.Threading.Tasks;
using dream_lib.src.reactive;
using game.gameplay_core;
using game.gameplay_core.characters;
using game.gameplay_core.debug;
using game.gameplay_core.inventory;
using game.ui;
using UnityEngine;
using Object = UnityEngine.Object;

namespace game
{
	public class GameDomain : IGameDomain
	{
		private readonly bool _sceneDebugMode;
		private CoreGameDomain _coreGameDomain;
		private InventoryDomain _inventoryDomain;
		private UiDomain _uiDomain;

		public GameDomain(bool sceneDebugMode)
		{
			_sceneDebugMode = sceneDebugMode;
		}

		public void Initialize()
		{
			InitializeAsync().Forget();
		}

		private async UniTask InitializeAsync()
		{
			_inventoryDomain = new InventoryDomain();

			_uiDomain = new UiDomain();

			GameStaticContext.Instance = new GameStaticContext
			{
				WorldToScreenUiParent = new ReactiveProperty<RectTransform>(),
				MainCamera = new ReactiveProperty<Camera>(),
				UiDomain = _uiDomain,
				InventoryDomain = _inventoryDomain,
				ReloadLocation = new ReactiveCommand()
			};

			GameStaticContext.Instance.ReloadLocation.OnExecute += ReloadLocation;

			await _uiDomain.Initialize();

			if(_sceneDebugMode)
			{
				//fake initialize meta game
				var charDebugConfig = Object.FindAnyObjectByType<DebugSceneCharacterConfig>(FindObjectsInactive.Include);
				var sceneInstaller = Object.FindAnyObjectByType<GameSceneInstaller>(FindObjectsInactive.Include);

				var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

				_coreGameDomain = new CoreGameDomain();

				var debugSaveData = new PlayerSaveData
				{
					InventoryData = charDebugConfig.InventoryData,
					CurrentLocationId = sceneName,
					CharacterData = new CharacterSaveData()
				};

				GameStaticContext.Instance.SaveSlotId = $"debug_{sceneName}";
				GameStaticContext.Instance.PlayerSave = debugSaveData;

				await _inventoryDomain.Initialize(_sceneDebugMode);

				await _coreGameDomain.PlayOnDebugLocation(sceneName, sceneInstaller != null && sceneInstaller.ResetState);
			}

			//TODO: open main menu
		}

		private void ReloadLocation()
		{
			_coreGameDomain.RespawnAndReloadLocation();
		}
	}
}
