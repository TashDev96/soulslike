using game.gameplay_core.characters;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class DamageInfo
	{
		public float DamageAmount;
		public float PoiseDamageAmount;
		public Vector3 WorldPos;
		public bool DoneByPlayer;
		public CharacterDomain DamageDealer;
		public int DeflectionRating;
	}
}
