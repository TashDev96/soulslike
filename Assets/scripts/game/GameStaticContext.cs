using dream_lib.src.reactive;
using game.gameplay_core.inventory;
using game.ui;
using UnityEngine;

namespace game
{
	public class GameStaticContext
	{
		public static GameStaticContext Instance { get; set; }

		public ReactiveProperty<Camera> MainCamera { get; set; }
		public ReactiveProperty<RectTransform> WorldToScreenUiParent { get; set; }
		public UiDomain UiDomain { get; set; }
		public InventoryDomain InventoryDomain { get; set; }
		public ReactiveCommand<float> CurrentLocationUpdate { get; set; }
	}
}
