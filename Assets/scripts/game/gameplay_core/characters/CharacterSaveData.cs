using System;
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
		
	}
}
