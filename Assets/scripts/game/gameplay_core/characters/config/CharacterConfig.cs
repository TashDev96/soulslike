using game.gameplay_core.characters.runtime_data;
using UnityEngine;

namespace game.gameplay_core.characters.config
{
	[CreateAssetMenu(menuName = "Configs/Character")]
	public class CharacterConfig : ScriptableObject
	{
		[field: SerializeField]
		public CharacterConfig ParentConfig { get; private set; }
		

		[field: SerializeField]
		public RollConfig Roll;
		[field: SerializeField]
		public AnimationClip IdleAnimation { get; private set; }
		[field: SerializeField]
		public AnimationClip StaggerAnimation { get; private set; }
		[field: SerializeField]
		public AnimationClip ParryStunAnimation { get; private set; }
		[field: SerializeField]
		public AnimationClip RipostedAnimation { get; private set; }
		[field: SerializeField]
		public AnimationClip DeathAnimation { get; set; }
		[field: SerializeField]
		public AnimationClip FallAnimation { get; set; }
		[field: SerializeField]
		public AnimationClip WalkAnimation { get; set; }
		[field: SerializeField]
		public AnimationClip RunAnimation { get; set; }

		
		
		[field: SerializeField]
		public LocomotionConfig Locomotion { get; private set; }

		[field: SerializeField]
		public RotationSpeedData RotationSpeed { get; private set; }

		[field: SerializeField]
		public BaseStats BaseStats { get; private set; }
		
		[field: SerializeField]
		public bool CanDoBackstabs { get; private set; }
	}
}
