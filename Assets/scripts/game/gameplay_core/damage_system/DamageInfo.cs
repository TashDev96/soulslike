using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public struct DamageInfo
	{
		public float DamageAmount;
		public float PoiseDamageAmount;
		public Vector3 WorldPos;
		public bool DoneByPlayer;
	}
}
