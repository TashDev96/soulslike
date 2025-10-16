using System;
using UnityEngine;

namespace TowerDefense
{
	[Serializable]
	public class UpgradeLevelConfig
	{
		[SerializeField]
		private int _levelUpPrice;
		[SerializeField]
		private float _damageBonus;

		public int LevelUpPrice => _levelUpPrice;
		public float DamageBonus => _damageBonus;
	}
}
