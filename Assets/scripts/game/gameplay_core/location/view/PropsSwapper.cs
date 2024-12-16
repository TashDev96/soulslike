using dream_lib.src.utils.components;
using UnityEngine;

namespace game.gameplay_core.location.view
{
	public class PropsSwapper : MonoBehaviour
	{
		[SerializeField]
		private TriggerEventsListener _trigger;

		[SerializeField]
		private GameObject _normalState;
		[SerializeField]
		private GameObject _swappedState;

		private void Awake()
		{
			_trigger.IsTriggered.OnChanged += HandleTrigger;
		}

		private void HandleTrigger(bool isTriggered)
		{
			if(_normalState!=null)
			{
				_normalState?.SetActive(!isTriggered);
			}
			if(_swappedState!=null)
			{
				_swappedState.SetActive(isTriggered);
			}
		}
	}
}
