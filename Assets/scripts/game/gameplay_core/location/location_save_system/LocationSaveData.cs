using System;
using System.Collections.Generic;
using dream_lib.src.utils.data_types;
using dream_lib.src.utils.serialization;
using game.gameplay_core.characters;
using UnityEngine;
using Random = UnityEngine.Random;

namespace game.gameplay_core.location.location_save_system
{
	[Serializable]
	public class LocationSaveData
	{
		public int uid { get; set; } = Random.Range(int.MinValue, int.MaxValue);
		public bool Initialized;
		public SerializableDictionary<string, PolymorphicJsonObject> SceneObjects = new();
		public SerializableDictionary<string, CharacterSaveData> Enemies = new();
		public SerializableDictionary<string, PolymorphicJsonObject> SpawnedObjects = new();

		 
 
	}
}
