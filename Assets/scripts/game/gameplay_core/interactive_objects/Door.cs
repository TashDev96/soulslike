using System;
using game.enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.interactive_objects
{
	public class Door : SavableSceneObjectGeneric<DoorSaveData>
	{
		[SerializeField]
		private bool _isClosedByDefault = true;
		[SerializeField]
		private bool _canNotOpenFromFrontSide;
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

		private static readonly int IsOpen = Animator.StringToHash("IsOpen");
		private static readonly int Immediate = Animator.StringToHash("Immediate");

		public override void InitializeFirstTime()
		{
			SaveData.IsOpened = !_isClosedByDefault;

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

		private void TryOpenFromFrontSide()
		{
			if(_canNotOpenFromFrontSide)
			{
				//TODO message can not be open from this side
				return;
			}
			TryOpen();
		}

		private void TryOpen()
		{
			if(_openWithKey)
			{
				//TODO key logic
			}

			SaveData.IsOpened = true;
			UpdateAnimatorState();
		}

		private void UpdateAnimatorState(bool immediate = false)
		{
			SaveData.IsOpened = !SaveData.IsOpened;
			_animator.SetBool(IsOpen, SaveData.IsOpened);
			if(immediate)
			{
				_animator.SetTrigger(Immediate);
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
