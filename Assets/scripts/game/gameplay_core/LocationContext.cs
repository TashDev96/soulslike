using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace game.gameplay_core
{
	public class LocationContext
	{
		
		public LocationSaveData LocationSaveData;
		
		//player
		public GameObject Player;
		
		//npcs
		public List<GameObject> Npcs;

		public SceneSavableObjectBase[] SceneSavableObjects;
		
		
		public List<SpawnedObjectController> SpawnedObjects { get; set; }
	}
}
