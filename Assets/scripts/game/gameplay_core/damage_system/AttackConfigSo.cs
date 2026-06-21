using System;
using UnityEngine;


namespace game.gameplay_core.damage_system
{
	[CreateAssetMenu(menuName = "Configs/AttackConfig")]
	[Serializable]
	public class AttackConfigSo : ScriptableObject
	{
		public AttackConfig Config;
	}
}
