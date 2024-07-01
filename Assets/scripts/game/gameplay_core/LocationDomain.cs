using dream_lib.src.utils.data_types;
using UnityEngine;

namespace game.gameplay_core
{
	public class LocationDomain
	{
		private LocationContext _locationContext;
		
		

		public LocationDomain(GameSceneBinder sceneBinder)
		{
			_locationContext = new LocationContext();

			_locationContext.LocationSaveData = new LocationSaveData();
			//TODO Load Saved Data

			sceneBinder.BindObjects(_locationContext);
		}

		
	}
}
