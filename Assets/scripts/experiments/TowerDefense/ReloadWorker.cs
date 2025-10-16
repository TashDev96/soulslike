using System.Linq;
using dream_lib.src.utils.drawers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace TowerDefense
{
	public class ReloadWorker : MonoBehaviour
	{
	public enum WorkerState
	{
		WaitingAtStorage,
		MovingToStorage,
		LoadingAmmo,
		MovingToTower,
		ReloadingTower
	}

		[Header("Worker Configuration")]
		[SerializeField]
		private float moveSpeed = 5f;
		[SerializeField]
		private float reloadSpeed = 2f;
		[SerializeField]
		private float stoppingDistance = 1f;

		[Header("Components")]
		[SerializeField]
		private NavMeshAgent navMeshAgent;
		[SerializeField]
		private CharacterController characterController;

		[ShowInInspector] [ReadOnly]
		private AmmoStorage ammoStorage;
		[ShowInInspector] [ReadOnly]
		private GameManager gameManager;
		[ShowInInspector] [ReadOnly]
		private TowerUnit currentTarget;
		[ShowInInspector] [ReadOnly]
		private bool isReloading;
		[ShowInInspector] [ReadOnly]
		private float reloadStartTime;
		[ShowInInspector] [ReadOnly]
		private float loadingStartTime;
		[ShowInInspector] [ReadOnly]
		private Transform reservedLoadingPoint;

	[ShowInInspector]
	private WorkerState currentState = WorkerState.WaitingAtStorage;

	private void Start()
	{
		ammoStorage = FindFirstObjectByType<AmmoStorage>();
		gameManager = FindFirstObjectByType<GameManager>();

			if(navMeshAgent == null)
			{
				navMeshAgent = GetComponent<NavMeshAgent>();
			}
			if(characterController == null)
			{
				characterController = GetComponent<CharacterController>();
			}

		if(navMeshAgent != null)
		{
			navMeshAgent.speed = moveSpeed;
			navMeshAgent.stoppingDistance = stoppingDistance;
		}

		if(ammoStorage != null)
		{
			reservedLoadingPoint = ammoStorage.TryReserveLoadingPoint(this);
			if(reservedLoadingPoint != null)
			{
				SetState(WorkerState.MovingToStorage);
			}
		}
		}

		public WorkerState GetCurrentState()
		{
			return currentState;
		}

		public TowerUnit GetCurrentTarget()
		{
			return currentTarget;
		}

		public bool IsMoving()
		{
			return navMeshAgent != null && navMeshAgent.velocity.magnitude > 0.1f;
		}

		public float GetRemainingDistance()
		{
			return navMeshAgent != null ? navMeshAgent.remainingDistance : 0f;
		}

		private void Update()
		{
		switch(currentState)
		{
			case WorkerState.WaitingAtStorage:
				HandleWaitingAtStorage();
				break;
			case WorkerState.MovingToStorage:
				HandleMovingToStorage();
				break;
			case WorkerState.LoadingAmmo:
				HandleLoadingAmmo();
				break;
			case WorkerState.MovingToTower:
				HandleMovingToTower();
				break;
			case WorkerState.ReloadingTower:
				HandleReloadingTower();
				break;
		}
		}

	private void HandleWaitingAtStorage()
	{
		if(reservedLoadingPoint == null)
		{
			reservedLoadingPoint = ammoStorage?.TryReserveLoadingPoint(this);
			if(reservedLoadingPoint != null)
			{
				SetState(WorkerState.MovingToStorage);
				return;
			}
		}

		var towerToReload = FindTowerNeedingReload();
		if(towerToReload != null && ammoStorage != null && !ammoStorage.IsEmpty)
		{
			currentTarget = towerToReload;
			SetState(WorkerState.LoadingAmmo);
			loadingStartTime = Time.time;
		}
		else if(reservedLoadingPoint != null)
		{
			MoveTowards(reservedLoadingPoint.position);
		}
	}

		private void HandleMovingToStorage()
		{
			if(MoveTowards(reservedLoadingPoint.position))
			{
				SetState(WorkerState.LoadingAmmo);
				loadingStartTime = Time.time;
			}
		}

	private void HandleLoadingAmmo()
	{
		if(Time.time >= loadingStartTime + 1f)
		{
			var clipSize = GetClipSizeForTarget();
			var ammoTaken = ammoStorage.TakeAmmoUpTo(clipSize);

			if(ammoTaken >= clipSize && currentTarget != null)
			{
				SetState(WorkerState.MovingToTower);
			}
			else
			{
				currentTarget = null;
				SetState(WorkerState.WaitingAtStorage);
			}
		}
	}

	private void HandleMovingToTower()
	{
		if(currentTarget == null)
		{
			SetState(WorkerState.WaitingAtStorage);
			return;
		}

		var targetPos = currentTarget.transform.position;
		targetPos -= Vector3.right * 2f;

		if(MoveTowards(targetPos))
		{
			SetState(WorkerState.ReloadingTower);
			reloadStartTime = Time.time;
		}
	}

	private void HandleReloadingTower()
	{
		if(currentTarget == null)
		{
			SetState(WorkerState.WaitingAtStorage);
			return;
		}

		if(Time.time >= reloadStartTime + reloadSpeed)
		{
			ReloadTower();
			currentTarget = null;
			SetState(WorkerState.WaitingAtStorage);
		}
	}


		private bool MoveTowards(Vector3 targetPosition)
		{
			if(navMeshAgent == null)
			{
				return false;
			}

			navMeshAgent.SetDestination(targetPosition);

			if(!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
			{
				return true;
			}

			if(navMeshAgent.velocity.magnitude > 0.1f)
			{
				var direction = navMeshAgent.velocity.normalized;
				transform.rotation = Quaternion.LookRotation(direction);
			}

			return false;
		}


		private void SetState(WorkerState newState)
		{
			currentState = newState;
		}

	private TowerUnit FindTowerNeedingReload()
	{
		if(gameManager == null)
		{
			return null;
		}

		var towers = gameManager.GetTowers();
		var availableTowers = towers.Where(tower =>
			tower != null &&
			tower.GetCurrentAmmo() == 0 &&
			!tower.IsReloading() &&
			!IsTowerBeingServiced(tower)).ToList();

		return availableTowers.FirstOrDefault();
	}

	private bool IsTowerBeingServiced(TowerUnit tower)
	{
		var allWorkers = FindObjectsByType<ReloadWorker>(FindObjectsSortMode.None);
		return allWorkers.Any(worker => 
			worker != this && 
			worker.currentTarget == tower &&
			(worker.currentState == WorkerState.LoadingAmmo ||
			 worker.currentState == WorkerState.MovingToTower ||
			 worker.currentState == WorkerState.ReloadingTower));
	}

		private int GetClipSizeForTarget()
		{
			if(currentTarget == null)
			{
				return 0;
			}
			return currentTarget.GetClipSize();
		}

		private void ReloadTower()
		{
			if(currentTarget == null)
			{
				return;
			}

			var clipSize = currentTarget.GetClipSize();
			currentTarget.ReloadWithAmmo(clipSize);
		}

		private void ReleaseLoadingPoint()
		{
			if(ammoStorage != null && reservedLoadingPoint != null)
			{
				ammoStorage.ReleaseLoadingPoint(this);
				reservedLoadingPoint = null;
			}
		}

		private void OnDestroy()
		{
			ReleaseLoadingPoint();
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(transform.position, 1f);
			
			DebugDrawUtils.DrawText(currentState.ToString(), transform.position + Vector3.up * 1.5f, 0.02f);

			if(currentTarget != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(transform.position, currentTarget.transform.position);
			}

			if(reservedLoadingPoint != null)
			{
				Gizmos.color = Color.magenta;
				Gizmos.DrawWireSphere(reservedLoadingPoint.position, 0.3f);
				Gizmos.DrawLine(transform.position, reservedLoadingPoint.position);
			}

			if(navMeshAgent != null && navMeshAgent.hasPath)
			{
				Gizmos.color = Color.green;
				var pathCorners = navMeshAgent.path.corners;
				for(var i = 0; i < pathCorners.Length - 1; i++)
				{
					Gizmos.DrawLine(pathCorners[i], pathCorners[i + 1]);
				}
			}
		}
	}
}
