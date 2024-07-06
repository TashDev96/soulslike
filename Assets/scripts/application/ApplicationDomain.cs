namespace application
{
	public static class ApplicationDomain
	{
		private static IGameDomain _gameDomain;
		public static bool Initialized { get; private set; }

		public static void Initialize(IGameDomain gameDomain)
		{
			if(Initialized)
			{
				return;
			}

			Initialized = true;

			_gameDomain = gameDomain;
			_gameDomain.Initialize();
		}
	}
}
