using System;
using UnityEngine;

namespace game.gameplay_core.characters.ai.world_reflection
{
	[Serializable]
	public class VisualInfoPoint
	{
#if UNITY_EDITOR
		[SerializeField]
		private string _name;
#endif
		[SerializeField]
		private Transform _parentTransform;
		[SerializeField]
		private Vector3 _localPosition;

		[NonSerialized]
		public CharacterDomain Character;

		public Vector3 Position => _parentTransform.TransformPoint(_localPosition);

		public void Initialize(CharacterDomain character)
		{
			Character = character;
			LocationStaticContext.Instance.WorldInfo.RegisterVisualInfo(this);
		}
	}
}
