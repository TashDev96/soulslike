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

		private void Start()
		{
			InteractionZone.OnInteractionTriggered += HandleInteractionTriggered;
			InteractionZone.InteractionTextHint = GetInteractionTextHint();
			InteractionZone.IsActive = true;
		}

		protected abstract void HandleInteractionTriggered();
		protected abstract string GetInteractionTextHint();
	}
}
