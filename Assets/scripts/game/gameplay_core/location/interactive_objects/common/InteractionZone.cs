using System;
using UnityEngine;

namespace game.gameplay_core.location.interactive_objects.common
{
	public class InteractionZone : MonoBehaviour
	{
		public string InteractionTextHint;

		public bool IsActive
		{
			set => gameObject.SetActive(value);
			get => gameObject.activeSelf;
		}

		public event Action OnInteractionTriggered;

		public void SetData(bool isActive, string interactionHint, Action callback)
		{
			InteractionTextHint = interactionHint;
			OnInteractionTriggered += callback;
			IsActive = isActive;
		}

		private void OnCollisionEnter(Collision other)
		{
			OnInteractionTriggered?.Invoke();
		}
	}
}
