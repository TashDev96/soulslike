using application;
using Cysharp.Threading.Tasks;
using dream_lib.src.reactive;
using game.gameplay_core;
using game.gameplay_core.inventory;
using game.ui;
using UnityEngine;

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

			await _inventoryDomain.Initialize(_sceneDebugMode);

			_uiDomain = new UiDomain();

			GameStaticContext.Instance = new GameStaticContext
			{
				WorldToScreenUiParent = new ReactiveProperty<RectTransform>(),
				MainCamera = new ReactiveProperty<Camera>(),
				UiDomain = _uiDomain,
				InventoryDomain = _inventoryDomain
			};

			await _uiDomain.Initialize();

			if(_sceneDebugMode)
			{
				//TODO fake initialize meta game
				_coreGameDomain = new CoreGameDomain();
				await _coreGameDomain.PlayOnDebugLocation();
			}

			//TODO: open main menu
		}
	}
}
