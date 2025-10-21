using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TowerDefense
{
	public static class ReloadWorkersManager
	{
		public static Dictionary<Transform, ReloadWorker> reservedStoragePoints = new();
		public static HashSet<TowerUnit> towersBeingServiced = new();

		public static Transform TryReserveStoragePoint(AmmoStorage storage, ReloadWorker worker)
		{
			if(storage == null || worker == null)
			{
				return null;
			}

			var loadingPoints = storage.GetLoadingPointTransforms();
			if(loadingPoints == null || loadingPoints.Length == 0)
			{
				return null;
			}

			foreach(var pointTransform in loadingPoints)
			{
				if(pointTransform != null && !reservedStoragePoints.ContainsKey(pointTransform))
				{
					reservedStoragePoints[pointTransform] = worker;
					return pointTransform;
				}
			}

			return null;
		}

		public static void ReleaseStoragePoint(ReloadWorker worker)
		{
			var pointToRelease = reservedStoragePoints.FirstOrDefault(kvp => kvp.Value == worker).Key;
			if(pointToRelease != null)
			{
				reservedStoragePoints.Remove(pointToRelease);
			}
		}

		public static bool IsStoragePointReservedBy(Transform pointTransform, ReloadWorker worker)
		{
			return reservedStoragePoints.ContainsKey(pointTransform) && reservedStoragePoints[pointTransform] == worker;
		}

		public static bool IsStoragePointReserved(Transform pointTransform)
		{
			return reservedStoragePoints.ContainsKey(pointTransform);
		}

		public static TowerUnit FindAvailableTowerNeedingReload(GameManager gameManager, ReloadWorker requestingWorker)
		{
			var towersManager = TowersManager.Instance;
			if(towersManager == null || requestingWorker == null)
			{
				return null;
			}

			var towers = towersManager.GetAllTowers();
			var availableTowers = towers.Where(tower =>
				tower != null &&
				tower.GetCurrentAmmo() == 0 &&
				!IsTowerBeingServiced(tower)).ToList();

			if(availableTowers.Count == 0)
			{
				return null;
			}

			var allWorkers = Object.FindObjectsByType<ReloadWorker>(FindObjectsSortMode.None);
			var waitingWorkers = allWorkers.Where(w =>
				w != null &&
				w.GetCurrentState() == ReloadWorker.WorkerState.WaitingAtStorage).ToList();

			if(waitingWorkers.Count == 0)
			{
				return null;
			}

			TowerUnit bestTowerForThisWorker = null;
			var bestScore = float.MaxValue;

			foreach(var tower in availableTowers)
			{
				var closestWorkerToTower = GetClosestWorkerToTower(tower, waitingWorkers);

				if(closestWorkerToTower == requestingWorker)
				{
					var distance = Vector3.Distance(requestingWorker.transform.position, tower.transform.position);
					if(distance < bestScore)
					{
						bestScore = distance;
						bestTowerForThisWorker = tower;
					}
				}
			}

			return bestTowerForThisWorker;
		}

		public static bool IsTowerBeingServiced(TowerUnit tower)
		{
			if(tower == null)
			{
				return false;
			}

			return towersBeingServiced.Contains(tower);
		}

		public static void ReserveTowerForService(TowerUnit tower, ReloadWorker worker)
		{
			if(tower != null && worker != null)
			{
				towersBeingServiced.Add(tower);
			}
		}

		public static void ReleaseTowerFromService(TowerUnit tower)
		{
			if(tower != null)
			{
				towersBeingServiced.Remove(tower);
			}
		}

		public static void CleanupWorkerReservations(ReloadWorker worker)
		{
			if(worker == null)
			{
				return;
			}

			ReleaseStoragePoint(worker);

			var towersToRelease = towersBeingServiced.Where(tower =>
			{
				var allWorkers = Object.FindObjectsByType<ReloadWorker>(FindObjectsSortMode.None);
				return !allWorkers.Any(w => w != worker && w.GetCurrentTarget() == tower);
			}).ToList();

			foreach(var tower in towersToRelease)
			{
				towersBeingServiced.Remove(tower);
			}
		}

		public static Dictionary<Transform, ReloadWorker> GetReservedStoragePoints()
		{
			return new Dictionary<Transform, ReloadWorker>(reservedStoragePoints);
		}

		public static void ClearAllReservations()
		{
			reservedStoragePoints.Clear();
			towersBeingServiced.Clear();
		}

		private static ReloadWorker GetClosestWorkerToTower(TowerUnit tower, List<ReloadWorker> workers)
		{
			if(tower == null || workers == null || workers.Count == 0)
			{
				return null;
			}

			ReloadWorker closestWorker = null;
			var closestDistance = float.MaxValue;

			foreach(var worker in workers)
			{
				var distance = Vector3.Distance(worker.transform.position, tower.transform.position);
				if(distance < closestDistance)
				{
					closestDistance = distance;
					closestWorker = worker;
				}
			}

			return closestWorker;
		}
	}
}
