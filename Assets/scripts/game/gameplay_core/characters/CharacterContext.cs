using Animancer;
using dream_lib.src.reactive;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters
{
	public class CharacterContext
	{
		public CharacterInputData InputData;
		public Transform Transform;
		public CharacterController MovementController;

		public CharacterContext(Transform transform)
		{
			Transform = transform;
			InputData = new CharacterInputData();
		}

		//inputs

		//stats

		//hp
		//stamina
		//
		public ReactiveProperty<float> WalkSpeed { get; set; }
		public ReactiveProperty<RotationSpeedData> RotationSpeed { get; set; }
		public ReactiveProperty<WeaponDomain> CurrentWeapon { get; set; }
		public CharacterConfig Config { get; set; }
		public AnimancerComponent Animator { get; set; }
	}
}
