using application;
using Cysharp.Threading.Tasks;
using dream_lib.src.reactive;
using dream_lib.src.utils.components;
using game.gameplay_core;
using game.ui;
using UnityEngine;

namespace game
{
	public class GameDomain : IGameDomain
	{
		private readonly bool _sceneDebugMode;
		private CoreGameDomain _coreGameDomain;
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
			GameStaticContext.Instance = new GameStaticContext
			{
				WorldToScreenUiParent = new ReactiveProperty<RectTransform>(),
				MainCamera = new ReactiveProperty<Camera>()
			};

			_uiDomain = new UiDomain();
			await _uiDomain.Initialize();

			if(_sceneDebugMode)
			{
				//TODO fake initialize meta game
				_coreGameDomain = new CoreGameDomain();
				await _coreGameDomain.PlayOnDebugLocation();
			}
			else
			{
				//TODO: open main menu
			}

		}
	}
}
