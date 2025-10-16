using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TowerDefense
{
	public class AmmoStorage : MonoBehaviour
	{
	[Header("Storage Configuration")]
	[SerializeField]
	private int maxCapacity = 100;
	[SerializeField]
	private float refillRate = 1f;
	[SerializeField]
	private float refillInterval = 2f;
	
	[Header("Loading Points")]
	[SerializeField]
	private Transform[] loadingPointTransforms;

		public Action<int, int> OnAmmoChanged;

		private int currentAmmo;
		private float lastRefillTime;

	public int CurrentAmmo => currentAmmo;
	public int MaxCapacity => maxCapacity;
	public bool IsFull => currentAmmo >= maxCapacity;
	public bool IsEmpty => currentAmmo <= 0;

		private void Start()
		{
			currentAmmo = maxCapacity;
			lastRefillTime = Time.time;
			OnAmmoChanged?.Invoke(currentAmmo, maxCapacity);
		}

		private void Update()
		{
			if (!IsFull && Time.time >= lastRefillTime + refillInterval)
			{
				RefillAmmo();
			}
		}

		private void RefillAmmo()
		{
			int ammoToAdd = Mathf.FloorToInt(refillRate);
			int previousAmmo = currentAmmo;
			
			currentAmmo = Mathf.Min(currentAmmo + ammoToAdd, maxCapacity);
			lastRefillTime = Time.time;

			if (currentAmmo != previousAmmo)
			{
				OnAmmoChanged?.Invoke(currentAmmo, maxCapacity);
			}
		}

		public bool TryTakeAmmo(int amount)
		{
			if (currentAmmo >= amount)
			{
				currentAmmo -= amount;
				OnAmmoChanged?.Invoke(currentAmmo, maxCapacity);
				return true;
			}
			return false;
		}

		public int TakeAmmoUpTo(int maxAmount)
		{
			int ammoTaken = Mathf.Min(currentAmmo, maxAmount);
			currentAmmo -= ammoTaken;
			OnAmmoChanged?.Invoke(currentAmmo, maxCapacity);
			return ammoTaken;
		}

		public void AddAmmo(int amount)
		{
			currentAmmo = Mathf.Min(currentAmmo + amount, maxCapacity);
			OnAmmoChanged?.Invoke(currentAmmo, maxCapacity);
		}

		public float GetRefillProgress()
		{
			if (IsFull) return 1f;
			return (Time.time - lastRefillTime) / refillInterval;
		}

	public Transform TryReserveLoadingPoint(ReloadWorker worker)
	{
		return ReloadWorkersManager.TryReserveStoragePoint(this, worker);
	}

		public void ReleaseLoadingPoint(ReloadWorker worker)
		{
			ReloadWorkersManager.ReleaseStoragePoint(worker);
		}

		public bool IsPointReservedBy(Transform pointTransform, ReloadWorker worker)
		{
			return ReloadWorkersManager.IsStoragePointReservedBy(pointTransform, worker);
		}

		public Transform[] GetLoadingPointTransforms()
		{
			return loadingPointTransforms;
		}

		private void OnDrawGizmosSelected()
		{
			if (loadingPointTransforms != null)
			{
				foreach (var pointTransform in loadingPointTransforms)
				{
					if (pointTransform != null)
					{
						bool isReserved = ReloadWorkersManager.IsStoragePointReserved(pointTransform);
						Gizmos.color = isReserved ? Color.red : Color.cyan;
						Gizmos.DrawWireSphere(pointTransform.position, 0.5f);
					}
				}
			}

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, 1f);
		}
	}
}
