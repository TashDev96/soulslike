using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace game.ui
{
	public class UiMessageView : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private Image _icon;

		public void Show(string text, Sprite icon)
		{
			_text.text = text;
			_icon.sprite = icon;
			_icon.gameObject.SetActive(icon != null);
		}
	}
}
