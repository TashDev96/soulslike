using application;
using dream_lib.src.utils.components;
using game.gameplay_core;

namespace game
{
	public class GameDomain : IGameDomain
	{
		private UnityEventsListener _unityEventsListener = UnityEventsListener.Create("__gameDomainEventsListener");

		private readonly bool _sceneDebugMode;
		private CoreGameDomain _coreGameDomain;

		public GameDomain(bool sceneDebugMode)
		{
			_sceneDebugMode = sceneDebugMode;
		}

		public void Initialize()
		{
			if(_sceneDebugMode)
			{
				//TODO fake initialize meta game
				_coreGameDomain = new CoreGameDomain();
				_coreGameDomain.InitializeDebugLocation();
			}

			//open main menu
		}
	}
}
