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
		private float _baseDamage;
		[SerializeField]
		private int _towersToAdd;
		[SerializeField]
		private float _damageMultiplier;

		public int LevelUpPrice
		{
			get => _levelUpPrice;
			set => _levelUpPrice = value;
		}

		public float BaseDamage
		{
			get => _baseDamage;
			set => _baseDamage = value;
		}

		public int TowersToAdd
		{
			get => _towersToAdd;
			set => _towersToAdd = value;
		}

		public float DamageMultiplier
		{
			get => _damageMultiplier;
			set => _damageMultiplier = value;
		}

		public UpgradeLevelConfig()
		{
			_levelUpPrice = 0;
			_baseDamage = 0f;
			_towersToAdd = 0;
			_damageMultiplier = 0f;
		}

		public UpgradeLevelConfig(int levelUpPrice, float baseDamage, int towersToAdd, float damageMultiplier = 0f)
		{
			_levelUpPrice = levelUpPrice;
			_baseDamage = baseDamage;
			_towersToAdd = towersToAdd;
			_damageMultiplier = damageMultiplier;
		}
	}
}
