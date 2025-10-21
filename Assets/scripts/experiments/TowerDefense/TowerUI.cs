using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense
{
	public class TowerUI : MonoBehaviour
	{
		[Header("UI Elements")]
		[SerializeField]
		private TextMeshProUGUI towerNameText;
		[SerializeField]
		private TextMeshProUGUI towerLevelText;
		[SerializeField]
		private TextMeshProUGUI damagePerShotText;
		[SerializeField]
		private Button upgradeButton;
		[SerializeField]
		private TextMeshProUGUI upgradePriceText;
		[SerializeField]
		private Button closeButton;

		[Header("Settings")]
		[SerializeField]
		private float followSpeed = 5f;

		public Action<TowerUI> OnClosed;

		private Camera playerCamera;
		private Canvas parentCanvas;
		private GameManager gameManager;
		private RectTransform rectTransform;
		public TowerGroup TargetTower { get; private set; }

		public void Initialize(TowerGroup tower)
		{
			TargetTower = tower;
			UpdateUI();
		}

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			playerCamera = Camera.main;
			parentCanvas = GetComponentInParent<Canvas>();
			gameManager = FindFirstObjectByType<GameManager>();

			if(upgradeButton != null)
			{
				upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
			}

			if(closeButton != null)
			{
				closeButton.onClick.AddListener(OnCloseButtonClicked);
			}
		}

		public void Close()
		{
			OnClosed?.Invoke(this);
			Destroy(gameObject);
		}

		private void Update()
		{
			if(TargetTower != null)
			{
				UpdatePosition();
				UpdateUI();
			}
		}

		private void UpdatePosition()
		{
			if(TargetTower == null || playerCamera == null || parentCanvas == null)
			{
				return;
			}

			var worldPosition = TargetTower.transform.position + Vector3.up * 2f;
			var screenPosition = RectTransformUtility.WorldToScreenPoint(playerCamera, worldPosition);

			Vector2 localPoint;
			if(RectTransformUtility.ScreenPointToLocalPointInRectangle(
				   parentCanvas.transform as RectTransform,
				   screenPosition,
				   parentCanvas.worldCamera,
				   out localPoint))
			{
				var targetPosition = localPoint;
				rectTransform.localPosition = Vector2.Lerp(rectTransform.localPosition, targetPosition, followSpeed * Time.deltaTime);
			}
		}

		private void UpdateUI()
		{
			if(TargetTower == null)
			{
				return;
			}

			var config = TargetTower.GetConfig();
			if(config == null)
			{
				return;
			}

			if(towerNameText != null)
			{
				towerNameText.text = config.TowerName;
			}

			if(towerLevelText != null)
			{
				var currentLevel = TargetTower.GetCurrentUpgradeLevel();
				towerLevelText.text = $"Level {currentLevel + 1}";
			}

			if(damagePerShotText != null)
			{
				var damagePerShot = TargetTower.GetDamagePerShot();
				damagePerShotText.text = $"Damage: {damagePerShot:F2}";
			}

			if(upgradeButton != null && upgradePriceText != null)
			{
				var canUpgrade = TargetTower.CanUpgrade();
				upgradeButton.interactable = canUpgrade && gameManager != null && gameManager.GetCurrentMoney() >= TargetTower.GetUpgradePrice();

				if(canUpgrade)
				{
					upgradePriceText.text = $"Upgrade: ${TargetTower.GetUpgradePrice()}";
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
			if(TargetTower != null && TargetTower.CanUpgrade() && gameManager != null)
			{
				var upgradePrice = TargetTower.GetUpgradePrice();
				if(gameManager.GetCurrentMoney() >= upgradePrice)
				{
					gameManager.SpendMoney(upgradePrice);
					TargetTower.UpgradeLevel();
					UpdateUI();
				}
			}
		}

		private void OnCloseButtonClicked()
		{
			Close();
		}

		private void OnDestroy()
		{
			if(upgradeButton != null)
			{
				upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
			}

			if(closeButton != null)
			{
				closeButton.onClick.RemoveListener(OnCloseButtonClicked);
			}
		}
	}
}
