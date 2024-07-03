namespace game.gameplay_core
{
	public class CoreGameDomain
	{
		private LocationDomain _locationDomain;

		public void InitializeDebugLocation()
		{
			_locationDomain = new LocationDomain();
			_locationDomain.Initialize();
		}

		public void LoadLocation()
		{
		}
	}
}
