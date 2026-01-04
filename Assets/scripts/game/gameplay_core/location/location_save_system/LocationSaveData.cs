using System;
using System.Collections.Generic;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters;

namespace game.gameplay_core.location.location_save_system
{
	[Serializable]
	public class LocationSaveData
	{
		public bool Initialized;
		public SerializableDictionary<string, BaseSaveData> SceneObjects = new();
		public SerializableDictionary<string, CharacterSaveData> Enemies = new();
		public List<SpawnedObjectSaveData> SpawnedObjects = new();
	}
}
