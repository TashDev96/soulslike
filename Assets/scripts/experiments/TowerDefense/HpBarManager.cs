using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
	public class HpBarManager : MonoBehaviour
	{
		[SerializeField]
		private GameObject hpBarPrefab;
		[SerializeField]
		private int poolSize = 50;

		private Camera playerCamera;
		private Canvas uiCanvas;
		private readonly List<HpBar> hpBarPool = new();
		private readonly Dictionary<ZombieUnit, HpBar> activeHpBars = new();
		public static HpBarManager Instance { get; private set; }

		private void InitializePool()
		{
			for(var i = 0; i < poolSize; i++)
			{
				CreateHpBar();
			}
		}

		private void Awake()
		{
			if(Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this;
		}

		private void Start()
		{
			playerCamera = Camera.main;
			uiCanvas = FindFirstObjectByType<Canvas>();

			if(uiCanvas == null)
			{
				Debug.LogError("HpBarManager: No Canvas found in scene!");
				return;
			}

			if(hpBarPrefab == null)
			{
				Debug.LogError("HpBarManager: HP Bar Prefab is not assigned!");
				return;
			}

			InitializePool();
		}

		public void ShowHpBar(ZombieUnit zombie)
		{
			if(zombie == null || zombie.IsDead)
			{
				return;
			}

			if(activeHpBars.ContainsKey(zombie))
			{
				return;
			}

			var hpBar = GetAvailableHpBar();
			if(hpBar != null)
			{
				hpBar.AttachToZombie(zombie);
				activeHpBars[zombie] = hpBar;
			}
		}

		public void HideHpBar(ZombieUnit zombie)
		{
			if(zombie == null || !activeHpBars.ContainsKey(zombie))
			{
				return;
			}

			var hpBar = activeHpBars[zombie];
			hpBar.Detach();
			activeHpBars.Remove(zombie);
		}

		private HpBar CreateHpBar()
		{
			var hpBarObject = Instantiate(hpBarPrefab, uiCanvas.transform);
			var hpBar = hpBarObject.GetComponent<HpBar>();

			if(hpBar == null)
			{
				Debug.LogError("HpBarManager: HP Bar Prefab does not have HpBar component!");
				Destroy(hpBarObject);
				return null;
			}

			hpBar.Initialize(playerCamera, uiCanvas);
			hpBar.gameObject.SetActive(false);
			hpBarPool.Add(hpBar);

			return hpBar;
		}

		private HpBar GetAvailableHpBar()
		{
			foreach(var hpBar in hpBarPool)
			{
				if(!hpBar.IsActive)
				{
					return hpBar;
				}
			}

			return CreateHpBar();
		}

		private void LateUpdate()
		{
			var zombiesToRemove = new List<ZombieUnit>();

			foreach(var kvp in activeHpBars)
			{
				var zombie = kvp.Key;
				if(zombie == null || zombie.IsDead)
				{
					zombiesToRemove.Add(zombie);
				}
				else if(zombie.GetHealthPercentage() >= 1f)
				{
					zombiesToRemove.Add(zombie);
				}
			}

			foreach(var zombie in zombiesToRemove)
			{
				HideHpBar(zombie);
			}
		}

		private void OnDestroy()
		{
			if(Instance == this)
			{
				Instance = null;
			}

			activeHpBars.Clear();
			hpBarPool.Clear();
		}
	}
}
