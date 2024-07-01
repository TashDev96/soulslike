using System;
using game.enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.interactive_objects
{
	public class Door : SavableSceneObject<DoorSaveData>
	{
		[SerializeField]
		private bool _isClosedByDefault = true;
		[SerializeField]
		private bool _canNotOpenFromFrontSide = false;
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

			Data.IsOpened = true;
			UpdateAnimatorState();
		}

		public override void InitializeFirstTime()
		{
			Data.IsOpened = !_isClosedByDefault;

			UpdateAnimatorState(true);
		}

		public override void OnDeserialize()
		{
			UpdateAnimatorState(true);	
		}

		private void UpdateAnimatorState(bool immediate = false)
		{
			Data.IsOpened = !Data.IsOpened;
			_animator.SetBool(IsOpen, Data.IsOpened);
			if(immediate)
			{
				_animator.SetTrigger(Immediate);
			}
		}
	}

	[Serializable]
	public class DoorSaveData
	{
		[field: SerializeField]
		public bool IsOpened { get; set; }
	}
}
