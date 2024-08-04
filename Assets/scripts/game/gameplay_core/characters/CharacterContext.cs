using Animancer;
using dream_lib.src.reactive;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters
{
	public class CharacterContext
	{
		public CharacterInputData InputData = new();
		public Transform Transform;
		public CharacterController MovementController;

		public ReactiveProperty<float> WalkSpeed { get; set; }
		public ReactiveProperty<RotationSpeedData> RotationSpeed { get; set; }
		public ReactiveProperty<WeaponView> CurrentWeapon { get; set; }
		public CharacterConfig Config { get; set; }
		public AnimancerComponent Animator { get; set; }
		public ReactiveProperty<float> DeltaTimeMultiplier { get; set; }
		public ReactiveProperty<float> MaxDeltaTime { get; set; }
		public ReactiveProperty<Team> Team { get; set; }
		public IReadOnlyReactiveProperty<string> CharacterId { get; set; }
		public IReadOnlyReactiveProperty<bool> IsPlayer { get; set; }
		public ReactiveCommand<DamageInfo> ApplyDamage { get; set; }
	}
}
