using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TowerDefense
{
	[Serializable]
	public class TowerConfig
	{
		[SerializeField]
		private string _towerName = "Tower";
		[SerializeField]
		private AttackType _attackType = AttackType.Single;
		[SerializeField]
		private int _clipSize = 5;
		[SerializeField]
		private int _bulletsPerShot = 1;
		[SerializeField]
		private float _baseDamage = 1;
		[SerializeField]
		private float _attackRange = 20;
		[SerializeField]
		private float _attackCooldown = 0.1f;
		[SerializeField]
		private float _reloadTime = 3f;
		[SerializeField]
		private float _shotgunAngle = 45f;
		[SerializeField]
		private List<UpgradeLevelConfig> _upgradeLevels;
		[SerializeField]
		private float _vfxDuration = 0.1f;
		[SerializeField]
		private float _damageDelay = 0.05f;

		public string TowerName => _towerName;
		public AttackType AttackType => _attackType;
		public int ClipSize => _clipSize;
		public int BulletsPerShot => _bulletsPerShot;
		public float BaseDamage => _baseDamage;
		public float AttackRange => _attackRange;
		public float AttackCooldown => _attackCooldown;
		public float ReloadTime => _reloadTime;
		public float ShotgunAngle => _shotgunAngle;
		public List<UpgradeLevelConfig> UpgradeLevels => _upgradeLevels;
		public float VfxDuration => _vfxDuration;
		public float DamageDelay => _damageDelay;

		[Button("Paste Level Up Prices")]
		private void PasteLevelUpPrices()
		{
			string clipboardText = GUIUtility.systemCopyBuffer;
			if (string.IsNullOrEmpty(clipboardText))
			{
				Debug.LogWarning("Clipboard is empty");
				return;
			}

			string[] lines = clipboardText.Split('\n');
			EnsureUpgradeLevelsExist(lines.Length - 1);

			for (int i = 1; i < lines.Length; i++)
			{
				if (int.TryParse(lines[i].Trim(), out int price))
				{
					_upgradeLevels[i - 1].LevelUpPrice = price;
				}
			}
			Debug.Log($"Pasted {lines.Length - 1} level up prices");
		}

		[Button("Paste Damage Bonuses")]
		private void PasteDamageBonuses()
		{
			string clipboardText = GUIUtility.systemCopyBuffer;
			if (string.IsNullOrEmpty(clipboardText))
			{
				Debug.LogWarning("Clipboard is empty");
				return;
			}

			string[] lines = clipboardText.Split('\n');
			EnsureUpgradeLevelsExist(lines.Length - 1);

			for (int i = 1; i < lines.Length; i++)
			{
				if (float.TryParse(lines[i].Trim(), out float bonus))
				{
					_upgradeLevels[i - 1].BaseDamage = bonus;
				}
			}
			Debug.Log($"Pasted {lines.Length - 1} damage bonuses");
		}

		[Button("Paste Towers Count")]
		private void PasteTowersCount()
		{
			string clipboardText = GUIUtility.systemCopyBuffer;
			if (string.IsNullOrEmpty(clipboardText))
			{
				Debug.LogWarning("Clipboard is empty");
				return;
			}

			string[] lines = clipboardText.Split('\n');
			EnsureUpgradeLevelsExist(lines.Length - 1);

			for (int i = 1; i < lines.Length; i++)
			{
				if (int.TryParse(lines[i].Trim(), out int count))
				{
					_upgradeLevels[i - 1].TowersCount = count;
				}
			}
			Debug.Log($"Pasted {lines.Length - 1} towers count values");
		}

		private void EnsureUpgradeLevelsExist(int requiredCount = 25)
		{
			if (_upgradeLevels == null)
			{
				_upgradeLevels = new List<UpgradeLevelConfig>();
			}

			while (_upgradeLevels.Count < requiredCount)
			{
				_upgradeLevels.Add(new UpgradeLevelConfig());
			}
		}
	}
}
