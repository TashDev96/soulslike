using System;
using UnityEngine;

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

		private float lastRefillTime;

		public int CurrentAmmo { get; private set; }

		public int MaxCapacity => maxCapacity;
		public bool IsFull => CurrentAmmo >= maxCapacity;
		public bool IsEmpty => CurrentAmmo <= 0;

		private void Start()
		{
			CurrentAmmo = maxCapacity;
			lastRefillTime = Time.time;
			OnAmmoChanged?.Invoke(CurrentAmmo, maxCapacity);
		}

		public bool TryTakeAmmo(int amount)
		{
			if(CurrentAmmo >= amount)
			{
				CurrentAmmo -= amount;
				OnAmmoChanged?.Invoke(CurrentAmmo, maxCapacity);
				return true;
			}
			return false;
		}

		public int TakeAmmoUpTo(int maxAmount)
		{
			var ammoTaken = Mathf.Min(CurrentAmmo, maxAmount);
			CurrentAmmo -= ammoTaken;
			OnAmmoChanged?.Invoke(CurrentAmmo, maxCapacity);
			return ammoTaken;
		}

		public void AddAmmo(int amount)
		{
			CurrentAmmo = Mathf.Min(CurrentAmmo + amount, maxCapacity);
			OnAmmoChanged?.Invoke(CurrentAmmo, maxCapacity);
		}

		public float GetRefillProgress()
		{
			if(IsFull)
			{
				return 1f;
			}
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

		private void Update()
		{
			if(!IsFull && Time.time >= lastRefillTime + refillInterval)
			{
				RefillAmmo();
			}
		}

		private void RefillAmmo()
		{
			var ammoToAdd = Mathf.FloorToInt(refillRate);
			var previousAmmo = CurrentAmmo;

			CurrentAmmo = Mathf.Min(CurrentAmmo + ammoToAdd, maxCapacity);
			lastRefillTime = Time.time;

			if(CurrentAmmo != previousAmmo)
			{
				OnAmmoChanged?.Invoke(CurrentAmmo, maxCapacity);
			}
		}

		private void OnDrawGizmosSelected()
		{
			if(loadingPointTransforms != null)
			{
				foreach(var pointTransform in loadingPointTransforms)
				{
					if(pointTransform != null)
					{
						var isReserved = ReloadWorkersManager.IsStoragePointReserved(pointTransform);
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
