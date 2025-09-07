using dream_lib.src.extensions;
using dream_lib.src.utils.components;
using game.gameplay_core.characters.player;
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
			_trigger.OnTriggerEnterEvent += HandleTriggerEnter;
			_trigger.OnTriggerExitEvent += HandleTriggerExit;
		}

		private void HandleTriggerExit(GameObject obj)
		{
			if(obj.HasComponent<PlayerFlagComponent>())
			{
				SwitchTrigger(false);
			}
		}

		private void HandleTriggerEnter(GameObject obj)
		{
			if(obj.HasComponent<PlayerFlagComponent>())
			{
				SwitchTrigger(true);
			}
		}

		private void SwitchTrigger(bool isTriggered)
		{
			if(_normalState != null)
			{
				_normalState?.SetActive(!isTriggered);
			}
			if(_swappedState != null)
			{
				_swappedState.SetActive(isTriggered);
			}
		}
	}
}
