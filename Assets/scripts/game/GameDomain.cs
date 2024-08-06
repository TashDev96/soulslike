using application;
using Cysharp.Threading.Tasks;
using dream_lib.src.utils.components;
using game.gameplay_core;
using game.ui;

namespace game
{
	public class GameDomain : IGameDomain
	{
		private UnityEventsListener _unityEventsListener = UnityEventsListener.Create("__gameDomainEventsListener");

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
			_uiDomain = new UiDomain();
			await _uiDomain.Initialize();

			if(_sceneDebugMode)
			{
				//TODO fake initialize meta game
				_coreGameDomain = new CoreGameDomain();
				_coreGameDomain.PlayOnDebugLocation();
			}

			//open main menu
		}
	}
}
