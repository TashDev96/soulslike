using System;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.stats.config;
using UnityEngine;

namespace game.gameplay_core.characters
{
	[Serializable]
	public class CharacterSaveData
	{
		public bool Initialized;
		public Vector3 Position;
		public Vector3 Euler;
		public float Hp;
		public float Stamina;
		public string LeftWeaponId;
		public string RightWeaponId;

		public SerializableDictionary<StatKey, int> StatUpgrades = new();
	}
}
