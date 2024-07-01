using UnityEngine;

namespace game.gameplay_core.interactive_objects
{
	public abstract class InteractiveObjectBase<T> : SavableSceneObject<T>
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
