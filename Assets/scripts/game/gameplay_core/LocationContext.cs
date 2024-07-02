using System.Collections.Generic;

namespace game.gameplay_core
{
	public class LocationContext
	{
		public LocationSaveData LocationSaveData;

		public List<SpawnedObjectController> SpawnedObjects { get; set; }
	}
}
