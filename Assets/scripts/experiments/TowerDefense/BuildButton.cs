using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace TowerDefense
{
	public class BuildButton : MonoBehaviour
	{
		[Header("UI Elements")]
		[SerializeField] private Button buildButton;
		[SerializeField] private TextMeshProUGUI buildPriceText;

		[Header("Settings")]
		[SerializeField] private float followSpeed = 5f;

		private TowerGroup targetTowerGroup;
		private Camera playerCamera;
		private Canvas parentCanvas;
		private GameManager gameManager;
		private RectTransform rectTransform;

		public Action<BuildButton> OnClosed;

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			playerCamera = Camera.main;
			parentCanvas = GetComponentInParent<Canvas>();
			gameManager = FindFirstObjectByType<GameManager>();

			if(buildButton != null)
			{
				buildButton.onClick.AddListener(OnBuildButtonClicked);
			}
		}

		public void Initialize(TowerGroup towerGroup)
		{
			targetTowerGroup = towerGroup;
			UpdateUI();
		}

		private void Update()
		{
			if(targetTowerGroup != null)
			{
				UpdatePosition();
				UpdateUI();
			}
		}

		private void UpdatePosition()
		{
			if(targetTowerGroup == null || playerCamera == null || parentCanvas == null)
			{
				return;
			}

			Vector3 worldPosition = targetTowerGroup.transform.position + Vector3.up * 2f;
			Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(playerCamera, worldPosition);

			Vector2 localPoint;
			if(RectTransformUtility.ScreenPointToLocalPointInRectangle(
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
			if(targetTowerGroup == null || !targetTowerGroup.HasAnyTowers())
			{
				if(buildButton != null && buildPriceText != null)
				{
					int buildPrice = targetTowerGroup?.GetBuildPrice() ?? 0;
					buildButton.interactable = gameManager != null && gameManager.GetCurrentMoney() >= buildPrice;
					buildPriceText.text = $"Build: ${buildPrice}";
				}
			}
			else
			{
				Close();
			}
		}

		private void OnBuildButtonClicked()
		{
			if(targetTowerGroup != null && !targetTowerGroup.HasAnyTowers() && gameManager != null)
			{
				int buildPrice = targetTowerGroup.GetBuildPrice();
				if(gameManager.GetCurrentMoney() >= buildPrice)
				{
					gameManager.SpendMoney(buildPrice);
					targetTowerGroup.BuildFirstTower();
					Close();
				}
			}
		}

		public void Close()
		{
			OnClosed?.Invoke(this);
			Destroy(gameObject);
		}

		private void OnDestroy()
		{
			if(buildButton != null)
			{
				buildButton.onClick.RemoveListener(OnBuildButtonClicked);
			}
		}
	}
}

