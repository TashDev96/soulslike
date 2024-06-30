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
		private Animator _animator;

		[SerializeField]
		private bool _openWithKey;
		[ShowIf(nameof(_openWithKey))]
		[SerializeField]
		private KeyId[] _keys;

		private static readonly int IsOpen = Animator.StringToHash("IsOpen");
		private static readonly int Immediate = Animator.StringToHash("Immediate");

		public override void InitializeFirstTime()
		{
			Data.IsOpened = !_isClosedByDefault;

			UpdateAnimatorState(true);
		}

		public override void Deserialize(string data)
		{
			base.Deserialize(data);
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
