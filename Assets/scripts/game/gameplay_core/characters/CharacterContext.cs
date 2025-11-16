using Animancer;
using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.config;
using game.gameplay_core.characters.logic;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.characters.state_machine.states;
using game.gameplay_core.characters.state_machine.states.stagger;
using game.gameplay_core.characters.view;
using game.gameplay_core.damage_system;
using game.gameplay_core.inventory.items_logic;
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
		public FallDamageLogic FallDamageLogic;
		public StaminaLogic StaminaLogic;
		public PoiseLogic PoiseLogic;
		public BlockLogic BlockLogic;

		public CharacterConfig Config;
		public ReadOnlyTransform Transform;
		public AnimancerComponent Animator;
		public GameObject DeadStateRoot;
		public LockOnTargetView[] LockOnTargets;
		public CharacterStats CharacterStats;
		public CharacterInputData InputData;
		public ReactiveProperty<WeaponView> RightWeapon;
		public ReactiveProperty<WeaponView> LeftWeapon;

		public ReactiveProperty<float> WalkSpeed;
		public ReactiveProperty<float> RunSpeed;
		public ReactiveProperty<RotationSpeedData> RotationSpeed;
		public ReactiveProperty<float> DeltaTimeMultiplier;
		public ReactiveProperty<float> MaxDeltaTime;
		public ReactiveProperty<Team> Team;
		public IReadOnlyReactiveProperty<string> CharacterId;
		public IReadOnlyReactiveProperty<bool> IsPlayer;
		public ApplyDamageCommand ApplyDamage;
		public IsDead IsDead;
		public ReactiveCommand<StaggerReason> TriggerStagger;

		public ReactiveProperty<bool> IsFalling;
		public ReactiveHashSet<Collider> EnteredTriggers;

		public ReactiveProperty<CharacterDebugDrawer> DebugDrawer;
		public IReadOnlyReactiveProperty<CharacterStateBase> CurrentState;
		public ReactiveCommand<CharacterStateBase, CharacterStateBase> OnStateChanged;

		public BodyAttackView BodyAttackView;
		public ParryReceiver ParryReceiver;

		public ReactiveCommand DeflectCurrentAttack;

		// Parry support
		public ReactiveCommand<CharacterDomain> OnParryTriggered;
		public ReactiveProperty<IConsumableItemLogic> CurrentConsumableItem { get; set; }
		
	}
}
