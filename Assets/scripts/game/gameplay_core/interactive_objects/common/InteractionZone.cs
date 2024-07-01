using System;
using UnityEngine;

namespace game.gameplay_core.interactive_objects
{
	public class InteractionZone : MonoBehaviour
	{
		public string InteractionTextHint;
		public event Action OnInteractionTriggered;

		public bool IsActive
		{
			set => gameObject.SetActive(value);
			get => gameObject.activeSelf;
		}

		private void OnCollisionEnter(Collision other)
		{
			OnInteractionTriggered?.Invoke();
		}

		public void SetData(bool isActive, string interactionHint, Action callback)
		{
			InteractionTextHint = interactionHint;
			OnInteractionTriggered += callback;
			IsActive = isActive;
		}
	}
}
