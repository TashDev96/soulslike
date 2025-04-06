using System;
using game.gameplay_core.characters.runtime_data;
using UnityEngine;

namespace game.gameplay_core.characters.config
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
		[field: SerializeField]
		public AnimationClip FallAnimation { get; set; }

		[field: SerializeField]
		public LocomotionConfig Locomotion { get; private set; }
		
		[field: SerializeField]
		public RotationSpeedData RotationSpeed { get; private set; }

		[field: SerializeField]
		public CharacterStats DefaultStats { get; private set; }
	}
}
