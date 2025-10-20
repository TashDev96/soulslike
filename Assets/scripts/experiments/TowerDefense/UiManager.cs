using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace TowerDefense
{
	public class UiManager : MonoBehaviour
	{
	[Header("UI Prefabs")]
	[SerializeField] private GameObject towerUIPrefab;
	[SerializeField] private GameObject buildButtonPrefab;

	[Header("Settings")]
	[SerializeField]
	private LayerMask towerLayerMask;

	[Header("UI Elements")]
	[SerializeField] private TextMeshProUGUI moneyText;

	private Camera playerCamera;
	private Canvas uiCanvas;
	private List<TowerUI> activeTowerUIs = new List<TowerUI>();
	private List<BuildButton> activeBuildButtons = new List<BuildButton>();
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

		if (buildButtonPrefab == null)
		{
			Debug.LogError("UiManager: Build Button Prefab is not assigned!");
		}

		ShowBuildButtonsForEmptyGroups();
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
			if (Input.GetMouseButtonDown(0))
			{
				TrySelectTower();
			}
		}

	private void TrySelectTower()
	{
		Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, towerLayerMask))
		{
			TowerGroup tower = hit.collider.GetComponent<TowerGroup>();
			if (tower == null)
			{
				tower = hit.collider.GetComponentInParent<TowerGroup>();
			}

			if (tower != null)
			{
				if(tower.HasAnyTowers())
				{
					OpenTowerUI(tower);
				}
			}
		}
	}

		private void OpenTowerUI(TowerGroup tower)
		{
			if (towerUIPrefab == null || uiCanvas == null)
			{
				return;
			}

			CloseAllTowerUIs();

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

		private void CloseTowerUI(TowerGroup tower)
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

	private void ShowBuildButtonsForEmptyGroups()
	{
		CloseAllBuildButtons();

		TowerGroup[] allTowerGroups = FindObjectsByType<TowerGroup>(FindObjectsSortMode.None);

		List<TowerGroup> emptyGroups = new List<TowerGroup>();
		foreach(var group in allTowerGroups)
		{
			if(!group.HasAnyTowers())
			{
				emptyGroups.Add(group);
			}
		}

		if(emptyGroups.Count > 0)
		{
			emptyGroups.Sort((a, b) => a.GetBuildPrice().CompareTo(b.GetBuildPrice()));
			OpenBuildButton(emptyGroups[0]);
		}
	}

	private void OpenBuildButton(TowerGroup towerGroup)
	{
		if(buildButtonPrefab == null || uiCanvas == null)
		{
			return;
		}

		GameObject buttonInstance = Instantiate(buildButtonPrefab, uiCanvas.transform);
		BuildButton buildButton = buttonInstance.GetComponent<BuildButton>();

		if(buildButton != null)
		{
			buildButton.Initialize(towerGroup);
			buildButton.OnClosed += OnBuildButtonClosed;
			activeBuildButtons.Add(buildButton);
		}
		else
		{
			Debug.LogError("UiManager: Build Button Prefab does not have BuildButton component!");
			Destroy(buttonInstance);
		}
	}

	private void OnBuildButtonClosed(BuildButton buildButton)
	{
		if(activeBuildButtons.Contains(buildButton))
		{
			activeBuildButtons.Remove(buildButton);
		}

		ShowBuildButtonsForEmptyGroups();
	}

	private void CloseAllBuildButtons()
	{
		for(int i = activeBuildButtons.Count - 1; i >= 0; i--)
		{
			if(activeBuildButtons[i] != null)
			{
				activeBuildButtons[i].Close();
			}
		}
		activeBuildButtons.Clear();
	}

	private void OnDestroy()
	{
		CloseAllTowerUIs();
		CloseAllBuildButtons();
	}
	}
}
