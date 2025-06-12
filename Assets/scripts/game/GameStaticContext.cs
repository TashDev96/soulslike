using dream_lib.src.reactive;
using game.ui;
using UnityEngine;

namespace game
{
	public struct GameStaticContext
	{
		public static GameStaticContext Instance { get; set; }

		public ReactiveProperty<Camera> MainCamera { get; set; }
		public ReactiveProperty<RectTransform> WorldToScreenUiParent { get; set; }
		public UiDomain UiDomain { get; set; }
	}
}
