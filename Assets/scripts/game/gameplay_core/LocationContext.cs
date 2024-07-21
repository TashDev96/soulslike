using System.Collections.Generic;
using dream_lib.src.reactive;
using game.gameplay_core.characters;
using game.gameplay_core.location_save_system;
using UnityEngine;

namespace game.gameplay_core
{
	public class LocationContext
	{
		public LocationSaveData LocationSaveData;

		public CharacterDomain Player;

		public SceneSavableObjectBase[] SceneSavableObjects;
		
		public List<CharacterDomain> Characters { get; set; }
		public List<SpawnedObjectController> SpawnedObjects { get; set; }
		public ReactiveCommand<float> LocationUpdate { get; set; }
		public ReactiveProperty<Camera> MainCamera { get; set; } = new();
	}
}
