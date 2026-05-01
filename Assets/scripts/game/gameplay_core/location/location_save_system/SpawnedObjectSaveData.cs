using System;
using UnityEngine;

namespace game.gameplay_core.location.location_save_system
{
	[Serializable]
	public class SpawnedObjectSaveData : BaseSaveData
	{
		public string UniqueId;
		public string PrefabName;
		public Vector3 Position;
	}
}
