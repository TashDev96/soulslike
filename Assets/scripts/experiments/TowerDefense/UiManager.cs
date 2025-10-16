using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace TowerDefense
{
	public class UiManager : MonoBehaviour
	{
		[Header("UI Prefabs")]
		[SerializeField] private GameObject towerUIPrefab;

		[Header("Settings")]
		[SerializeField] private LayerMask towerLayerMask = 1 << LayerMask.NameToLayer("Character");

		[Header("UI Elements")]
		[SerializeField] private TextMeshProUGUI moneyText;

		private Camera playerCamera;
		private Canvas uiCanvas;
		private List<TowerUI> activeTowerUIs = new List<TowerUI>();
		private GameManager gameManager;

		private void Start()
		{
			playerCamera = Camera.main;
			uiCanvas = FindFirstObjectByType<Canvas>();
			gameManager = FindFirstObjectByType<GameManager>();

			gameManager.OnMoneyChanged+=OnMoneyChanged;

			if (uiCanvas == null)
			{
				Debug.LogError("UiManager: No Canvas found in scene!");
			}

			if (towerUIPrefab == null)
			{
				Debug.LogError("UiManager: Tower UI Prefab is not assigned!");
			}
		}

		private void OnMoneyChanged(int money){
			moneyText.text = money.ToString();
		}

		private void Update()
		{
			HandleTowerSelection();
		}

		private void HandleTowerSelection()
		{
			if (Input.GetMouseButtonDown(1))
			{
				TrySelectTower();
			}
		}

		private void TrySelectTower()
		{
			if (playerCamera == null)
			{
				return;
			}

			Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, towerLayerMask))
			{
				TowerUnit tower = hit.collider.GetComponent<TowerUnit>();
				if (tower == null)
				{
					tower = hit.collider.GetComponentInParent<TowerUnit>();
				}

				if (tower != null)
				{
					OpenTowerUI(tower);
				}
			}
		}

		private void OpenTowerUI(TowerUnit tower)
		{
			if (towerUIPrefab == null || uiCanvas == null)
			{
				return;
			}

			CloseTowerUI(tower);

			GameObject uiInstance = Instantiate(towerUIPrefab, uiCanvas.transform);
			TowerUI towerUI = uiInstance.GetComponent<TowerUI>();

			if (towerUI != null)
			{
				towerUI.Initialize(tower);
				towerUI.OnClosed += OnTowerUIClosed;
				activeTowerUIs.Add(towerUI);
			}
			else
			{
				Debug.LogError("UiManager: Tower UI Prefab does not have TowerUI component!");
				Destroy(uiInstance);
			}
		}

		private void CloseTowerUI(TowerUnit tower)
		{
			for (int i = activeTowerUIs.Count - 1; i >= 0; i--)
			{
				if (activeTowerUIs[i] != null && activeTowerUIs[i].gameObject != null)
				{
					TowerUI towerUI = activeTowerUIs[i];
					if (towerUI.TargetTower == tower)
					{
						towerUI.Close();
					}
				}
			}
		}

		public void CloseAllTowerUIs()
		{
			for (int i = activeTowerUIs.Count - 1; i >= 0; i--)
			{
				if (activeTowerUIs[i] != null)
				{
					activeTowerUIs[i].Close();
				}
			}
			activeTowerUIs.Clear();
		}

		private void OnTowerUIClosed(TowerUI towerUI)
		{
			if (activeTowerUIs.Contains(towerUI))
			{
				activeTowerUIs.Remove(towerUI);
			}
		}

		private void OnDestroy()
		{
			CloseAllTowerUIs();
		}
	}
}
