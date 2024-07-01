using System;
using dream_lib.src.utils.data_types;

namespace game.gameplay_core
{
	[Serializable]
	public class LocationSaveData
	{
		public SerializableDictionary<string, BaseSaveData> SavableObjects = new();
	}
}
