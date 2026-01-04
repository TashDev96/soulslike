using System;
using game.gameplay_core.characters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.location.interactive_objects.common
{
	public class InteractionZone : MonoBehaviour
	{
		public string InteractionTextHint;
		private Action<GameObject> _onEnterHandler;

		public bool IsActive
		{
			set => gameObject.SetActive(value);
			get => gameObject.activeSelf;
		}

		public event Action<CharacterDomain> OnInteractionTriggered;

		public void SetData(bool isActive, string interactionHint, Action<CharacterDomain> callback)
		{
			InteractionTextHint = interactionHint;
			OnInteractionTriggered += callback;
			IsActive = isActive;
		}

		public void InteractFromUi(CharacterDomain interactedCharacter)
		{
			OnInteractionTriggered?.Invoke(interactedCharacter);
		}

		[Button]
		private void SetLayer()
		{
			gameObject.layer = LayerMask.NameToLayer("Triggers");
		}
	}
}
