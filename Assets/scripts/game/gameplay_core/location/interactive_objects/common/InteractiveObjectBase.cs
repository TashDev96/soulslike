using game.gameplay_core.characters;
using game.gameplay_core.location.location_save_system;
using UnityEngine;

namespace game.gameplay_core.location.interactive_objects.common
{
	public abstract class InteractiveObjectBase<T> : SavableSceneObjectGeneric<T> where T : BaseSaveData
	{
		[SerializeField]
		protected InteractionZone InteractionZone;

		protected string InteractionTextHint
		{
			set => InteractionZone.InteractionTextHint = value;
			get => InteractionZone.InteractionTextHint;
		}

		protected virtual void Initialize()
		{
			InteractionZone.OnInteractionTriggered += HandleInteractionTriggered;
			InteractionZone.InteractionTextHint = GetInteractionTextHint();
			InteractionZone.IsActive = true;
		}

		protected abstract void HandleInteractionTriggered(CharacterDomain interactedCharacter);
		protected abstract string GetInteractionTextHint();
	}
}
