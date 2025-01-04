using Animancer;
using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.logic;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.characters.state_machine.states;
using game.gameplay_core.characters.view;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters
{
	public struct CharacterContext
	{
		public ReactiveProperty<float> LocationTime;
		public CharacterDomain SelfLink;
		public LockOnLogic LockOnLogic;
		public MovementLogic MovementLogic;
		public InvulnerabilityLogic InvulnerabilityLogic;

		public CharacterConfig Config;
		public ReadOnlyTransform Transform;
		public AnimancerComponent Animator;
		public GameObject DeadStateRoot;
		public LockOnTargetView[] LockOnTargets;
		public CharacterStats CharacterStats;
		public CharacterInputData InputData;

		public ReactiveProperty<float> WalkSpeed;
		public ReactiveProperty<float> RunSpeed;
		public ReactiveProperty<RotationSpeedData> RotationSpeed;
		public ReactiveProperty<WeaponView> CurrentWeapon;
		public ReactiveProperty<float> DeltaTimeMultiplier;
		public ReactiveProperty<float> MaxDeltaTime;
		public ReactiveProperty<Team> Team;
		public IReadOnlyReactiveProperty<string> CharacterId;
		public IReadOnlyReactiveProperty<bool> IsPlayer;
		public ApplyDamageCommand ApplyDamage;
		public IsDead IsDead;
		public ReactiveCommand TriggerStagger;

		public ReactiveProperty<CharacterDebugDrawer> DebugDrawer;
		public IReadOnlyReactiveProperty<CharacterStateBase> CurrentState;
		public ReactiveCommand<CharacterStateBase, CharacterStateBase> OnStateChanged;
	}
}
