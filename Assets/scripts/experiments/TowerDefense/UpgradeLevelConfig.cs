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
		private int _towersCount;

		public UpgradeLevelConfig()
		{
			_levelUpPrice = 0;
			_baseDamage = 0f;
			_towersCount = 1;
		}

		public UpgradeLevelConfig(int levelUpPrice, float baseDamage, int towersCount)
		{
			_levelUpPrice = levelUpPrice;
			_baseDamage = baseDamage;
			_towersCount = towersCount;
		}

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
		public int TowersCount 
		{ 
			get => _towersCount; 
			set => _towersCount = value; 
		}
	}
}
