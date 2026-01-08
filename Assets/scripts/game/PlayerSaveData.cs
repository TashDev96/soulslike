using System;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters;
using game.gameplay_core.inventory.serialized_data;

namespace game
{
	[Serializable]
	public class PlayerSaveData
	{
		public string CurrentLocationId;
		public InventoryData InventoryData;
		public CharacterSaveData CharacterData;
		public string RespawnLocationId;
		public TransformCache RespawnTransform { get; set; }
	}
}
