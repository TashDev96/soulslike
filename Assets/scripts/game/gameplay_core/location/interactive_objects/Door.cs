using System;
using game.enums;
using game.gameplay_core.characters;
using game.gameplay_core.location.interactive_objects.common;
using game.gameplay_core.location.location_save_system;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.location.interactive_objects
{
	public class Door : SavableSceneObjectGeneric<DoorSaveData>
	{
		private static readonly int IsOpen = Animator.StringToHash("IsOpen");
		private static readonly int Immediate = Animator.StringToHash("Immediate");
		[SerializeField]
		private bool _isClosedByDefault = true;
		[SerializeField]
		private bool _canOpenFromFrontSide = true;
		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private InteractionZone _interactionFront;
		[SerializeField]
		private InteractionZone _interactionBack;

		[SerializeField]
		private bool _openWithKey;
		[ShowIf(nameof(_openWithKey))]
		[SerializeField]
		private KeyId[] _keys;

		public override void InitializeFirstTime()
		{
			SaveData = new DoorSaveData
			{
				IsOpened = !_isClosedByDefault
			};

			InitializeAfterSaveLoaded();
		}

		protected override void InitializeAfterSaveLoaded()
		{
			UpdateAnimatorState(true);
		}

		private void Awake()
		{
			_interactionFront?.SetData(true, "Open Door", TryOpenFromFrontSide);
			_interactionBack?.SetData(true, "Open Door", TryOpen);
		}

		private void TryOpenFromFrontSide(CharacterDomain interactedCharacter)
		{
			if(!_canOpenFromFrontSide)
			{
				interactedCharacter.Context.Logic.InteractionLogic.AddNotification("Can not be opened from this side");
				return;
			}
			TryOpen(interactedCharacter);
		}

		private void TryOpen(CharacterDomain interactedCharacter)
		{
			if(_openWithKey)
			{
				if(!interactedCharacter.Context.Logic.InventoryLogic.CheckHasKeys(_keys))
				{
					interactedCharacter.Context.Logic.InteractionLogic.AddNotification("Need key");
				}
			}

			SaveData.IsOpened = true;
			UpdateAnimatorState();
		}

		private void UpdateAnimatorState(bool immediate = false)
		{
			SaveData.IsOpened = !SaveData.IsOpened;
			if(_animator != null)
			{
				_animator.SetBool(IsOpen, SaveData.IsOpened);
				if(immediate)
				{
					_animator.SetTrigger(Immediate);
				}
			}
		}
	}

	[Serializable]
	public class DoorSaveData : BaseSaveData
	{
		[field: SerializeField]
		public bool IsOpened { get; set; }
	}
}
