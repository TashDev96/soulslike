using System;
using System.Collections.Generic;
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
	private float _damageMultiplier = 1;
		[SerializeField]
		private float _attackRange = 20;
		[SerializeField]
		private float _attackCooldown = 0.1f;
		[SerializeField]
		private float _reloadTime = 3f;



	[SerializeField]
	private float _shotgunAngle = 45f;
	[SerializeField]
	private UpgradeLevelsData _upgradeLevelsData;
	[SerializeField]
	private float _vfxDuration = 0.1f;
		[SerializeField]
		private float _damageDelay = 0.05f;

		public string TowerName => _towerName;
		public AttackType AttackType => _attackType;
		public int ClipSize => _clipSize;
		public int BulletsPerShot => _bulletsPerShot;
		public float DamageMultiplier => _damageMultiplier;
		public float AttackRange => _attackRange;
	public float AttackCooldown => _attackCooldown;
	public float ReloadTime => _reloadTime;
	public float ShotgunAngle => _shotgunAngle;
	public List<UpgradeLevelConfig> UpgradeLevels => _upgradeLevelsData?.UpgradeLevels;
	public float VfxDuration => _vfxDuration;
	public float DamageDelay => _damageDelay;
	}
}
