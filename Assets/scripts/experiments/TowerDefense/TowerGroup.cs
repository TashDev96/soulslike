using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
	public class TowerGroup : MonoBehaviour
	{
		[SerializeField]
		private TowerConfig config;
		[SerializeField]
		private GameObject towerUnitPrefab;
		[SerializeField]
		private Transform[] spawnPositions;

		private List<TowerUnit> _towerUnits = new List<TowerUnit>();
		private int _currentUpgradeLevel;

		private void Start()
		{
			InitializeTowers();
		}

		private void InitializeTowers()
		{
			if (config == null || towerUnitPrefab == null || spawnPositions == null || spawnPositions.Length == 0)
			{
				return;
			}

			var initialTowerCount = GetTowerCountForLevel(0);
			SpawnTowers(initialTowerCount);
		}

		private void SpawnTowers(int targetCount)
		{
			while (_towerUnits.Count < targetCount && _towerUnits.Count < spawnPositions.Length)
			{
				var spawnIndex = _towerUnits.Count;
				var spawnPosition = spawnPositions[spawnIndex];
				
				var towerObject = Instantiate(towerUnitPrefab, spawnPosition.position, spawnPosition.rotation, transform);
				var towerUnit = towerObject.GetComponent<TowerUnit>();
                towerObject.name = $"{config.TowerName}_{spawnIndex}";
				
				if (towerUnit != null)
				{
					towerUnit.Initialize(config);
					_towerUnits.Add(towerUnit);
				}
			}
		}

		private int GetTowerCountForLevel(int level)
		{
			if (config?.UpgradeLevels == null || level < 0)
			{
				return 1;
			}

			var totalCount = 1;
			for (var i = 0; i <= level && i < config.UpgradeLevels.Count; i++)
			{
				totalCount += config.UpgradeLevels[i].TowersCount;
			}

			return totalCount;
		}

		public bool CanUpgrade()
		{
			return config != null && config.UpgradeLevels != null && _currentUpgradeLevel < config.UpgradeLevels.Count;
		}

		public int GetUpgradePrice()
		{
			if (!CanUpgrade())
			{
				return 0;
			}
			return config.UpgradeLevels[_currentUpgradeLevel].LevelUpPrice;
		}

		public void UpgradeLevel()
		{
			if (!CanUpgrade())
			{
				return;
			}

			var previousTowerCount = GetTowerCountForLevel(_currentUpgradeLevel - 1);
			_currentUpgradeLevel++;
			var newTowerCount = GetTowerCountForLevel(_currentUpgradeLevel - 1);

			if (newTowerCount > previousTowerCount)
			{
				SpawnTowers(newTowerCount);
			}

			foreach (var tower in _towerUnits)
			{
				tower.SetUpgradeLevel(_currentUpgradeLevel);
			}
		}

		public int GetCurrentUpgradeLevel()
		{
			return _currentUpgradeLevel;
		}

		public TowerConfig GetConfig()
		{
			return config;
		}

		public List<TowerUnit> GetTowerUnits()
		{
			return new List<TowerUnit>(_towerUnits);
		}

		public float GetCurrentDamage()
		{
			if (_towerUnits.Count == 0)
			{
				return 0f;
			}
			return _towerUnits[0].GetCurrentDamage();
		}
	}
}
