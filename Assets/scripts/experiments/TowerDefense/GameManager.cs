using UnityEngine;
using System.Collections.Generic;

namespace TowerDefense
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private int startingMoney = 500;
        [SerializeField] private int towerCost = 100;
        
        [Header("Tower Prefab")]
        [SerializeField] private GameObject towerPrefab;
        
        [Header("UI References")]
        [SerializeField] private GameUI gameUI;

        private int currentMoney;
        private ZombieManager zombieManager;
        private List<TowerUnit> towers = new List<TowerUnit>();
        private Camera playerCamera;

        public System.Action<int> OnMoneyChanged;

        private void Start()
        {
            currentMoney = startingMoney;
            zombieManager = FindFirstObjectByType<ZombieManager>();
            playerCamera = Camera.main;
            
            if (gameUI != null)
            {
                gameUI.Initialize(this);
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
                    if (IsValidTowerPlacement(worldPosition))
                    {
                        PlaceTower(worldPosition);
                    }
                }
            }
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
            Debug.Log($"Zombie killed! Bonus money: {zombie.KillBonusValue}");
        }

        private void HandleZombieReachedGoal(ZombieUnit zombie)
        {
            Debug.Log("Zombie reached the goal! No bonus money.");
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
            return new List<TowerUnit>(towers);
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
