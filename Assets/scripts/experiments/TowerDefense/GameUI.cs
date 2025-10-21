using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense
{
	public class GameUI : MonoBehaviour
	{
		[Header("UI Elements")]
		[SerializeField]
		private TextMeshProUGUI moneyText;
		[SerializeField]
		private TextMeshProUGUI waveText;
		[SerializeField]
		private TextMeshProUGUI zombieCountText;
		[SerializeField]
		private TextMeshProUGUI towerCountText;
		[SerializeField]
		private Button placeTowerButton;
		[SerializeField]
		private TextMeshProUGUI towerCostText;

		[Header("Instructions")]
		[SerializeField]
		private TextMeshProUGUI instructionsText;

		private GameManager gameManager;
		private ZombieManager zombieManager;

		public void Initialize(GameManager manager)
		{
			gameManager = manager;
			zombieManager = FindFirstObjectByType<ZombieManager>();

			if(gameManager != null)
			{
				gameManager.OnMoneyChanged += UpdateMoneyDisplay;
			}

			SetupUI();
		}

		private void SetupUI()
		{
			if(placeTowerButton != null)
			{
				placeTowerButton.onClick.AddListener(OnPlaceTowerButtonClicked);
			}

			if(towerCostText != null && gameManager != null)
			{
				towerCostText.text = $"Tower Cost: ${gameManager.GetTowerCost()}";
			}

			if(instructionsText != null)
			{
				instructionsText.text = "Click on the ground to place towers!\nEarn money by hitting and killing zombies.";
			}
		}

		private void Update()
		{
			UpdateUI();
		}

		private void UpdateUI()
		{
			if(zombieManager != null)
			{
				if(waveText != null)
				{
				}

				if(zombieCountText != null)
				{
					zombieCountText.text = $"Zombies: {zombieManager.GetAliveZombieCount()}";
				}
			}

			if(gameManager != null)
			{
				if(towerCountText != null)
				{
					towerCountText.text = $"Towers: {gameManager.GetTowerCount()}";
				}

				if(placeTowerButton != null)
				{
					placeTowerButton.interactable = gameManager.CanAffordTower();
				}
			}
		}

		private void UpdateMoneyDisplay(int money)
		{
			if(moneyText != null)
			{
				moneyText.text = $"Money: ${money}";
			}
		}

		private void OnPlaceTowerButtonClicked()
		{
			if(instructionsText != null)
			{
				instructionsText.text = "Click on the ground to place a tower!";
			}
		}

		private void OnDestroy()
		{
			if(gameManager != null)
			{
				gameManager.OnMoneyChanged -= UpdateMoneyDisplay;
			}

			if(placeTowerButton != null)
			{
				placeTowerButton.onClick.RemoveListener(OnPlaceTowerButtonClicked);
			}
		}
	}
}
