using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TowerDefense
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private int startingMoney = 500;
        [SerializeField] private int towerCost = 100;
        
        [Header("Tower Prefab")]
        [SerializeField] private GameObject towerPrefab;
        
        [Header("Ammo System")]
        [SerializeField] private AmmoStorage existingAmmoStorage;
        
        [Header("UI References")]
        [SerializeField] private GameUI gameUI;
        [SerializeField] private UiManager uiManager;

        private int currentMoney;
        private ZombieManager zombieManager;
        private List<TowerUnit> towers = new List<TowerUnit>();
        private List<ReloadWorker> reloadWorkers = new List<ReloadWorker>();
        private AmmoStorage ammoStorage;
        private Camera playerCamera;

        public System.Action<int> OnMoneyChanged;

        private void Start()
        {
            currentMoney = startingMoney;
            zombieManager = FindFirstObjectByType<ZombieManager>();
            towers = GameObject.FindObjectsByType<TowerUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID).ToList();
           Debug.LogError(towers.Count);
            playerCamera = Camera.main;
            
            TargetingManager.Initialize(zombieManager);
            
            InitializeAmmoSystem();
            
            if (gameUI != null)
            {
                gameUI.Initialize(this);
            }

            if (uiManager == null)
            {
                uiManager = FindFirstObjectByType<UiManager>();
                if (uiManager == null)
                {
                    var uiManagerGO = new GameObject("UiManager");
                    uiManager = uiManagerGO.AddComponent<UiManager>();
                }
            }
            
            SetupEventListeners();
            OnMoneyChanged?.Invoke(currentMoney);
        }

        private void SetupEventListeners()
        {
            if (zombieManager != null)
            {
                zombieManager.OnZombieDied += HandleZombieDied;
                zombieManager.OnZombieReachedGoal += HandleZombieReachedGoal;
            }
        }

        private void Update()
        {
            HandleTowerPlacement();
        }

        private void HandleTowerPlacement()
        {
            if (Input.GetMouseButtonDown(0) && CanAffordTower())
            {
                Vector3 worldPosition;
                if (GetMouseWorldPosition(out worldPosition))
                {
                    if (IsValidTowerPlacement(worldPosition) && !IsClickingOnTower())
                    {
                        PlaceTower(worldPosition);
                    }
                }
            }
        }

        private bool IsClickingOnTower()
        {
            if (playerCamera == null) return false;
            
            LayerMask characterLayer = 1 << LayerMask.NameToLayer("Character");
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, characterLayer))
            {
                TowerUnit tower = hit.collider.GetComponent<TowerUnit>();
                if (tower == null)
                {
                    tower = hit.collider.GetComponentInParent<TowerUnit>();
                }
                return tower != null;
            }
            return false;
        }

        private bool GetMouseWorldPosition(out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;
            
            if (playerCamera == null) return false;
            
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                worldPosition = hit.point;
                return true;
            }
            
            return false;
        }

        private bool IsValidTowerPlacement(Vector3 position)
        {
            float minDistance = 3f;
            foreach (var tower in towers)
            {
                if (tower != null && Vector3.Distance(position, tower.transform.position) < minDistance)
                {
                    return false;
                }
            }
            return true;
        }

        private void PlaceTower(Vector3 position)
        {
            if (towerPrefab == null)
            {
                Debug.LogError("GameManager: Tower prefab is not assigned!");
                return;
            }

            var towerGO = Instantiate(towerPrefab, position, Quaternion.identity);
            var towerUnit = towerGO.GetComponent<TowerUnit>();
            
            if (towerUnit == null)
            {
                towerUnit = towerGO.AddComponent<TowerUnit>();
            }
            
            towers.Add(towerUnit);
            SpendMoney(towerCost);
            
            Debug.Log($"Tower placed at {position}. Money remaining: {currentMoney}");
        }

        public void OnZombieHit(ZombieUnit zombie, float damage)
        {
            AddMoney(zombie.MoneyValue);
        }

        private void HandleZombieDied(ZombieUnit zombie)
        {
            AddMoney(zombie.KillBonusValue);
        }

        private void HandleZombieReachedGoal(ZombieUnit zombie)
        {
        }

        public void AddMoney(int amount)
        {
            currentMoney += amount;
            OnMoneyChanged?.Invoke(currentMoney);
        }

        public void SpendMoney(int amount)
        {
            currentMoney = Mathf.Max(0, currentMoney - amount);
            OnMoneyChanged?.Invoke(currentMoney);
        }

        public bool CanAffordTower()
        {
            return currentMoney >= towerCost;
        }

        public int GetCurrentMoney()
        {
            return currentMoney;
        }

        public int GetTowerCost()
        {
            return towerCost;
        }

        public int GetTowerCount()
        {
            return towers.Count;
        }

        public List<TowerUnit> GetTowers()
        {
	        return towers;
        }

        public AmmoStorage GetAmmoStorage()
        {
            return ammoStorage;
        }

        public List<ReloadWorker> GetReloadWorkers()
        {
            return new List<ReloadWorker>(reloadWorkers);
        }

        private void InitializeAmmoSystem()
        {
            if (existingAmmoStorage != null)
            {
                ammoStorage = existingAmmoStorage;
            }
            else
            {
                ammoStorage = FindFirstObjectByType<AmmoStorage>();
                if (ammoStorage == null)
                {
                    Debug.LogWarning("GameManager: No AmmoStorage found in scene and none assigned!");
                }
            }

            var existingReloadWorkers = FindObjectsByType<ReloadWorker>(FindObjectsSortMode.None);
            if (existingReloadWorkers != null && existingReloadWorkers.Length > 0)
            {
                reloadWorkers.Clear();
                foreach (var worker in existingReloadWorkers)
                {
                    if (worker != null)
                    {
                        reloadWorkers.Add(worker);
                    }
                }
            }
            else
            {
                var foundWorkers = FindObjectsByType<ReloadWorker>(FindObjectsSortMode.None);
                reloadWorkers.Clear();
                reloadWorkers.AddRange(foundWorkers);
                
                if (reloadWorkers.Count == 0)
                {
                    Debug.LogWarning("GameManager: No ReloadWorkers found in scene and none assigned!");
                }
            }
            
            Debug.Log($"GameManager: Initialized ammo system with {reloadWorkers.Count} workers and {(ammoStorage != null ? "1" : "0")} storage");
        }

        private void OnDestroy()
        {
            if (zombieManager != null)
            {
                zombieManager.OnZombieDied -= HandleZombieDied;
                zombieManager.OnZombieReachedGoal -= HandleZombieReachedGoal;
            }
        }
    }
}
