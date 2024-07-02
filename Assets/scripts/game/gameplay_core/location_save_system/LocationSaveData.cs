using System;
using System.Collections.Generic;
using dream_lib.src.utils.data_types;

namespace game.gameplay_core
{
	[Serializable]
	public class LocationSaveData
	{
		public SerializableDictionary<string, BaseSaveData> SceneObjects = new();
		public List<SpawnedObjectSaveData> SpawnedObjects = new();
	}
}
