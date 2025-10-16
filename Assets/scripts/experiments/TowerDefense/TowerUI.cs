using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace TowerDefense
{
	public class TowerUI : MonoBehaviour
	{
		[Header("UI Elements")]
		[SerializeField] private TextMeshProUGUI towerNameText;
		[SerializeField] private TextMeshProUGUI towerLevelText;
		[SerializeField] private Button upgradeButton;
		[SerializeField] private TextMeshProUGUI upgradePriceText;
		[SerializeField] private Button closeButton;

		[Header("Settings")]
		[SerializeField] private float followSpeed = 5f;

		private TowerUnit targetTower;
	public TowerUnit TargetTower => targetTower;
		private Camera playerCamera;
		private Canvas parentCanvas;
		private GameManager gameManager;
		private RectTransform rectTransform;

		public Action<TowerUI> OnClosed;

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			playerCamera = Camera.main;
			parentCanvas = GetComponentInParent<Canvas>();
			gameManager = FindFirstObjectByType<GameManager>();

			if (upgradeButton != null)
			{
				upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
			}

			if (closeButton != null)
			{
				closeButton.onClick.AddListener(OnCloseButtonClicked);
			}
		}

		public void Initialize(TowerUnit tower)
		{
			targetTower = tower;
			UpdateUI();
		}

		private void Update()
		{
			if (targetTower != null)
			{
				UpdatePosition();
				UpdateUI();
			}
		}

		private void UpdatePosition()
		{
			if (targetTower == null || playerCamera == null || parentCanvas == null)
			{
				return;
			}

			Vector3 worldPosition = targetTower.transform.position + Vector3.up * 2f;
			Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(playerCamera, worldPosition);

			Vector2 localPoint;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
				parentCanvas.transform as RectTransform, 
				screenPosition, 
				parentCanvas.worldCamera, 
				out localPoint))
			{
				Vector2 targetPosition = localPoint;
				rectTransform.localPosition = Vector2.Lerp(rectTransform.localPosition, targetPosition, followSpeed * Time.deltaTime);
			}
		}

		private void UpdateUI()
		{
			if (targetTower == null)
			{
				return;
			}

			var config = targetTower.GetConfig();
			if (config == null)
			{
				return;
			}

			if (towerNameText != null)
			{
				towerNameText.text = config.TowerName;
			}

			if (towerLevelText != null)
			{
				int currentLevel = targetTower.GetCurrentUpgradeLevel();
				towerLevelText.text = $"Level {currentLevel + 1}";
			}

			if (upgradeButton != null && upgradePriceText != null)
			{
				bool canUpgrade = targetTower.CanUpgrade();
				upgradeButton.interactable = canUpgrade && gameManager != null && gameManager.GetCurrentMoney() >= targetTower.GetUpgradePrice();

				if (canUpgrade)
				{
					upgradePriceText.text = $"Upgrade: ${targetTower.GetUpgradePrice()}";
				}
				else
				{
					upgradePriceText.text = "Max Level";
					upgradeButton.interactable = false;
				}
			}
		}

		private void OnUpgradeButtonClicked()
		{
			if (targetTower != null && targetTower.CanUpgrade() && gameManager != null)
			{
				int upgradePrice = targetTower.GetUpgradePrice();
				if (gameManager.GetCurrentMoney() >= upgradePrice)
				{
					gameManager.SpendMoney(upgradePrice);
					targetTower.UpgradeLevel();
					UpdateUI();
				}
			}
		}

		private void OnCloseButtonClicked()
		{
			Close();
		}

		public void Close()
		{
			OnClosed?.Invoke(this);
			Destroy(gameObject);
		}

		private void OnDestroy()
		{
			if (upgradeButton != null)
			{
				upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
			}

			if (closeButton != null)
			{
				closeButton.onClick.RemoveListener(OnCloseButtonClicked);
			}
		}
	}
}
