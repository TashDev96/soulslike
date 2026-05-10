using dream_lib.src.extensions;
using game.gameplay_core.characters.stats.runtime_data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace game.ui.stats
{
	public class StatView : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _name;
		[SerializeField]
		private TMP_Text _value;
		[SerializeField]
		private Image _icon;

		private StatData _data;

		public void Initialize(StatData data)
		{
			_data = data;
			_name.text = data.Id.ToString();
			_value.text = data.MaxValue.RoundFormat(5);
			_icon.sprite = data.Config.Icon;
			_icon.gameObject.SetActive(data.Config.Icon != null);
		}
	}
}
