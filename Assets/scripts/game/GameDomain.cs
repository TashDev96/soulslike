using application;
using dream_lib.src.utils.components;

namespace gameplay_meta
{
	public class GameDomain : IGameDomain
	{
		private UnityEventsListener _unityEventsListener = UnityEventsListener.Create("__gameDomainEventsListener");

		private readonly bool _sceneDebugMode;

		public GameDomain(bool sceneDebugMode)
		{
			_sceneDebugMode = sceneDebugMode;
		}

		public void Initialize()
		{
			if(_sceneDebugMode)
			{
				
				//fake initialize everything

				//start core gameplay
			}
			else
			{
				//open main menu
			}
		}
	}
}
