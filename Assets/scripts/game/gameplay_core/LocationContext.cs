using System.Collections.Generic;
using game.gameplay_core.characters;
using game.gameplay_core.location_save_system;

namespace game.gameplay_core
{
	public class LocationContext
	{
		public LocationSaveData LocationSaveData;

		public CharacterDomain Player;

		public SceneSavableObjectBase[] SceneSavableObjects;
		public CharacterDomain[] Characters { get; set; }
		public List<SpawnedObjectController> SpawnedObjects { get; set; }
	}
}
