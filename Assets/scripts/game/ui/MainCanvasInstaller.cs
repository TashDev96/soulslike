using UnityEngine;

namespace game.ui
{
	public class MainCanvasInstaller : MonoBehaviour
	{
		[field: SerializeField]
		public RectTransform WorldToScreenRoot { get; private set; }
	}
}
