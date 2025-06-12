using UnityEngine;

namespace game.ui
{
	public class MainCanvasInstaller : MonoBehaviour
	{
		[field: SerializeField]
		public RectTransform WorldToScreenRoot { get; private set; }

		[field: SerializeField]
		public UiLocationHUD UiLocationHUD { get; private set; }
	}
}
