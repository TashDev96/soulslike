using System;
using UnityEngine;

namespace game.gameplay_core.interactive_objects
{
	public class InteractionZone : MonoBehaviour
	{
		private string _hudText;
		private Action _onInteractionTriggered;

		public void SetData(bool active, string hudText = null, Action onInteractionTriggered = null)
		{
			gameObject.SetActive(active);
			_hudText = hudText;
			_onInteractionTriggered = onInteractionTriggered;
		}
	}
}
