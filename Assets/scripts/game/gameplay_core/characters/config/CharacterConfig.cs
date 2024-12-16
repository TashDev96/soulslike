using System;
using game.gameplay_core.characters.runtime_data;
using UnityEngine;

namespace game.gameplay_core.characters
{
	[Serializable]
	public class CharacterConfig
	{
		[field: SerializeField]
		public RollConfig Roll;
		[field: SerializeField]
		public AnimationClip IdleAnimation { get; private set; }
		[field: SerializeField]
		public AnimationClip StaggerAnimation { get; private set; }
		[field: SerializeField]
		public AnimationClip DeathAnimation { get; set; }
		
		[field: Space]
		[field: SerializeField]
		public float WalkSpeed { get; private set; } = 5f;	
		[field: Space]
		[field: SerializeField]
		public float RunSpeed { get; private set; } = 15f;
		[field: SerializeField]
		public RotationSpeedData RotationSpeed { get; private set; }

		[field: SerializeField]
		public CharacterStats DefaultStats { get; private set; }
		
	}
}
