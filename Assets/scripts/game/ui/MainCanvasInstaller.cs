using game.gameplay_core.ui;
using game.ui.inventory;
using game.ui.stats;
using UnityEngine;

namespace game.ui
{
	public class MainCanvasInstaller : MonoBehaviour
	{
		[field: SerializeField]
		public RectTransform WorldToScreenRoot { get; private set; }

		[field: SerializeField]
		public UiLocationHUD UiLocationHUD { get; private set; }
		[field: SerializeField]
		public InventoryScreenView Inventory { get; set; }
		[field: SerializeField]
		public StatsScreenView StatsScreen { get; set; }
	}
}
