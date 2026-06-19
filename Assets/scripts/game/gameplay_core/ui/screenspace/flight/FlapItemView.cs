using UnityEngine;

namespace game.gameplay_core.ui.screenspace.flight
{
	public class FlapItemView : MonoBehaviour
	{
		[SerializeField]
		private GameObject _fullState;
		
		[SerializeField]
		private GameObject _glidingBg;
		[SerializeField]
		private GameObject _emptyState;
		
		[SerializeField]
		private UnityEngine.UI.Image _fillImage;

		public void SetIsEmpty(bool empty)
		{
			_fullState.SetActive(!empty);
			_emptyState.SetActive(true);
			if(empty)
			{
				_glidingBg.SetActive(false);
			}
		}

		public void SetFillAmount(float fill)
		{
			_glidingBg.SetActive(fill <1 && fill > 0);
			if(_fillImage != null)
			{
				_fillImage.fillAmount = fill;
			}
		}
	}
}
