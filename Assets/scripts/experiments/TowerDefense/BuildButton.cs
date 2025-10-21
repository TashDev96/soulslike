using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense
{
	public class BuildButton : MonoBehaviour
	{
		[Header("UI Elements")]
		[SerializeField]
		private Button buildButton;
		[SerializeField]
		private TextMeshProUGUI buildPriceText;

		[Header("Settings")]
		[SerializeField]
		private float followSpeed = 5f;

		public Action<BuildButton> OnClosed;

		private TowerGroup targetTowerGroup;
		private Camera playerCamera;
		private Canvas parentCanvas;
		private GameManager gameManager;
		private RectTransform rectTransform;

		public void Initialize(TowerGroup towerGroup)
		{
			targetTowerGroup = towerGroup;
			UpdateUI();
		}

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

		public void Close()
		{
			OnClosed?.Invoke(this);
			Destroy(gameObject);
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

			var worldPosition = targetTowerGroup.transform.position + Vector3.up * 2f;
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
			if(targetTowerGroup == null || !targetTowerGroup.HasAnyTowers())
			{
				if(buildButton != null && buildPriceText != null)
				{
					var buildPrice = targetTowerGroup?.GetBuildPrice() ?? 0;
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
				var buildPrice = targetTowerGroup.GetBuildPrice();
				if(gameManager.GetCurrentMoney() >= buildPrice)
				{
					gameManager.SpendMoney(buildPrice);
					targetTowerGroup.BuildFirstTower();
					Close();
				}
			}
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
