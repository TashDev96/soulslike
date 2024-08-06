using System;
using UnityEngine;

namespace game.gameplay_core.characters
{
	[Serializable]
	public class CharacterConfig
	{
		[field: SerializeField]
		public AnimationClip IdleAnimation { get; private set; }

		[field: SerializeField]
		public float WalkSpeed { get; private set; } = 5f;
		[field: SerializeField]
		public RotationSpeedData RotationSpeed { get; private set; }

		[field: SerializeField]
		public CharacterStats DefaultStats { get; private set; }
	}
}
