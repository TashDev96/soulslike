using System;

namespace game.gameplay_core
{
	[Serializable]
	public class SpawnedObjectSaveData
	{
		public string UniqueId;
		public string PrefabName;
		public BaseSaveData ObjectSaveData;
	}
}
