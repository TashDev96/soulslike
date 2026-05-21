using System.Collections.Generic;
using Animancer;
using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
using game.enums;
using game.gameplay_core.characters.ai.sensors;
using game.gameplay_core.characters.config;
using game.gameplay_core.characters.logic;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.characters.state_machine.states;
using game.gameplay_core.characters.state_machine.states.stagger;
using game.gameplay_core.characters.stats;
using game.gameplay_core.characters.stats.runtime_data;
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

		public CharacterConfig Config;
		public CharacterTransform Transform;
		public RigidBodyWrapper RigidBody;
		public CharacterStatsData CharacterStats;
		public CharacterInputData InputData;
		public CapsuleCharacterCollider CharacterCollider;

		public ReactiveProperty<float> DeltaTimeMultiplier;
		public ReactiveProperty<float> MaxDeltaTime;
		public ReactiveProperty<Team> Team;
		public IReadOnlyReactiveProperty<string> CharacterId;
		public IReadOnlyReactiveProperty<bool> IsPlayer;
		public IsDead IsDead;

		public ReactiveProperty<bool> IsFalling;
		public ReactiveHashSet<Collider> EnteredTriggers;

		public IReadOnlyReactiveProperty<CharacterStateBase> CurrentState;

		public Events Events;
		public Views Views;
		public Logics Logic;

		public ReactiveProperty<IConsumableItemLogic> CurrentConsumableItem;
		public CharacterSensorsDomain SensorsDomain;
	}

	public struct Logics
	{
		public LockOnLogic LockOnLogic;
		public MovementLogic MovementLogic;
		public InvulnerabilityLogic InvulnerabilityLogic;
		public FallDamageLogic FallDamageLogic;
		public StaminaLogic StaminaLogic;
		public PoiseLogic PoiseLogic;
		public BlockLogic BlockLogic;
		public HealthLogic HealthLogic;
		public CharacterStatsLogic StatsLogic;
		public DeathLogic DeathLogic;
		public CharacterInventoryLogic InventoryLogic;
		public InteractionLogic InteractionLogic;
	}

	public struct Views
	{
		public AnimancerComponent Animator;
		public LockOnPointView[] LockOnPoints;

		public BodyAttackView BodyAttackView;
		public ParryReceiver ParryReceiver;

		public ReactiveProperty<CharacterDebugDrawer> DebugDrawer;
		public Dictionary<EquipmentSlotType, WeaponView> EquippedWeaponViews;
		public CharacterBodyView BodyView;
	}

	public struct Events
	{
		public ApplyDamageCommand ApplyDamage;
		public ReactiveCommand<CharacterDomain> OnParryTriggered;
		public ReactiveCommand DeflectCurrentAttack;
		public ReactiveCommand<CharacterStateBase, CharacterStateBase> OnStateChanged;
		public ReactiveCommand<StaggerReason> TriggerStagger;
		public ReactiveCommand<CharacterDomain, PlungeAttackTargetView> TriggerPlungeAttack;
	}
}
