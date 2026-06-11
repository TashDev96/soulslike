using UnityEngine;

namespace game.gameplay_core.ui.hud_screenspace.flight
{
	public class FlapItemView : MonoBehaviour
	{
		[SerializeField]
		private GameObject _fullState;
		
		[SerializeField]
		private GameObject _emptyState;

		public void SetIsFull(bool full)
		{
			_fullState.SetActive(full);
			_emptyState.SetActive(!full);
		}
	}
}
