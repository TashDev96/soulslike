using System;
using System.Collections.Generic;
using dream_lib.src.reactive;
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
		public TransformCache RespawnTransform;

		public ReactivePropertyWithDelayedDisplayInt SoftCurrency = new();

		public List<string> UniqueLootDropped = new();
	}
}
