using System;

namespace game.gameplay_core.location.location_save_system
{
	[Serializable]
	public class SpawnedObjectSaveData
	{
		public string UniqueId;
		public string PrefabName;
		public BaseSaveData ObjectSaveData;
	}
}
