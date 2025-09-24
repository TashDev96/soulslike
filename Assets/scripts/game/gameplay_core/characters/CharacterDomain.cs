using System.Collections.Generic;
using Animancer;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
using dream_lib.src.utils.serialization;
using game.gameplay_core.characters.ai;
using game.gameplay_core.characters.config;
using game.gameplay_core.characters.logic;
using game.gameplay_core.characters.player;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.characters.state_machine;
using game.gameplay_core.characters.state_machine.states;
using game.gameplay_core.characters.state_machine.states.stagger;
using game.gameplay_core.characters.view;
using game.gameplay_core.characters.view.ui;
using game.gameplay_core.damage_system;
using game.gameplay_core.inventory;
using game.gameplay_core.inventory.items_logic;
using game.gameplay_core.inventory.serialized_data;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

namespace game.gameplay_core.characters
{
	public class CharacterDomain : MonoBehaviour, IOnSceneUniqueIdOwner
	{
		[SerializeField]
		private CharacterDebugDrawer _debugDrawer;

		[SerializeField]
		private CharacterConfig _config;

		[SerializeField]
		private GameObject _deadStateRoot;

		[SerializeField]
		private Transform _uiPivot;
		[SerializeField]
		private MovementLogic _movementLogic;

		private CharacterWorldSpaceUi _worldSpaceUi;

		private ICharacterBrain _brain;
		private CharacterContext _context;
		private ReadOnlyTransform _transform;

		private HealthLogic _healthLogic;
		private PoiseLogic _poiseLogic;
		private StaminaLogic _staminaLogic;
		private StatsLogic _statsLogic;
		private LockOnLogic _lockOnLogic;
		private InvulnerabilityLogic _invulnerabilityLogic;
		private FallDamageLogic _fallDamageLogic;
		private BlockLogic _blockLogic;
		private InventoryLogic _inventoryLogic;

		[ShowInInspector]
		private CharacterStats _characterStats;

		[field: SerializeField]
		public string UniqueId { get; private set; }

		[field: SerializeField]
		private WeaponView DebugWeapon { get; set; }
		[field: SerializeField]
		private WeaponView DebugWeaponLeft { get; set; }

		public CharacterExternalData ExternalData { get; private set; }
		public CharacterConfig Config => _config;
		public CharacterStateMachine CharacterStateMachine { get; private set; }

		public void Initialize(LocationContext locationContext)
		{
			var isPlayer = UniqueId == "Player";

			_transform = new ReadOnlyTransform(transform);
			var isDead = new IsDead();

			_lockOnLogic = new LockOnLogic(new LockOnLogic.Context
			{
				CharacterTransform = _transform,
				AllCharacters = locationContext.Characters,
				Self = this,
				MovementLogic = _movementLogic,
				IsDead = isDead,
			});

			_blockLogic = new BlockLogic();
			_invulnerabilityLogic = new InvulnerabilityLogic();
			_fallDamageLogic = new FallDamageLogic();
			_staminaLogic = new StaminaLogic();
			_poiseLogic = new PoiseLogic();
			_inventoryLogic = new InventoryLogic();

			var isFalling = new ReactiveProperty<bool>();

			_characterStats = new CharacterStats();
			_statsLogic = new StatsLogic(new StatsLogic.Context
			{
				CharacterStats = _characterStats,
				CharacterConfig = _config
			});

			_context = new CharacterContext
			{
				LocationTime = locationContext.LocationTime,
				SelfLink = this,

				MovementLogic = _movementLogic,
				LockOnLogic = _lockOnLogic,
				InvulnerabilityLogic = _invulnerabilityLogic,
				IsFalling = isFalling,
				FallDamageLogic = _fallDamageLogic,
				StaminaLogic = _staminaLogic,
				PoiseLogic = _poiseLogic,
				BlockLogic = _blockLogic,

				Config = _config,
				Transform = _transform,
				Animator = GetComponent<AnimancerComponent>(),
				DeadStateRoot = _deadStateRoot,
				CharacterStats = _characterStats,
				LockOnTargets = GetComponentsInChildren<LockOnTargetView>(),
				InputData = new CharacterInputData(),

				RightWeapon = new ReactiveProperty<WeaponView>(DebugWeapon),
				LeftWeapon = new ReactiveProperty<WeaponView>(DebugWeaponLeft),
				CurrentConsumableItem = new ReactiveProperty<IConsumableItemLogic>(),
				BodyAttackView = GetComponentInChildren<BodyAttackView>(),
				ParryReceiver = GetComponentInChildren<ParryReceiver>(true),

				WalkSpeed = new ReactiveProperty<float>(_config.Locomotion.WalkSpeed),
				RunSpeed = new ReactiveProperty<float>(_config.Locomotion.RunSpeed),
				RotationSpeed = new ReactiveProperty<RotationSpeedData>(_config.RotationSpeed),
				DeltaTimeMultiplier = new ReactiveProperty<float>(1),
				MaxDeltaTime = new ReactiveProperty<float>(1),
				CharacterId = new ReactiveProperty<string>(UniqueId),
				Team = new ReactiveProperty<Team>(isPlayer ? Team.Player : Team.HostileNPC),
				IsPlayer = new ReactiveProperty<bool>(isPlayer),
				ApplyDamage = new ApplyDamageCommand(),
				IsDead = isDead,
				TriggerStagger = new ReactiveCommand<StaggerReason>(),

				DebugDrawer = new ReactiveProperty<CharacterDebugDrawer>(),
				OnStateChanged = new ReactiveCommand<CharacterStateBase, CharacterStateBase>(),
				DeflectCurrentAttack = new ReactiveCommand(),

				OnParryTriggered = new ReactiveCommand<CharacterDomain>()
			};


			InitializeInventory();

			ExternalData = new CharacterExternalData(_context);

			_movementLogic.SetContext(new MovementLogic.Context
			{
				CharacterTransform = transform,
				CharacterCollider = GetComponent<CapsuleCharacterCollider>(),
				IsDead = _context.IsDead,
				RotationSpeed = _context.RotationSpeed,
				IsFalling = _context.IsFalling,
				LocomotionConfig = _config.Locomotion
			});

			_blockLogic.SetContext(new BlockLogic.Context
			{
				Team = _context.Team,
				CharacterId = _context.CharacterId,
				ApplyDamage = _context.ApplyDamage,
				InvulnerabilityLogic = _context.InvulnerabilityLogic,
				StaminaLogic = _context.StaminaLogic,
				PoiseLogic = _context.PoiseLogic
			});

			_fallDamageLogic.SetContext(new FallDamageLogic.Context
			{
				ApplyDamage = _context.ApplyDamage,
				IsDead = _context.IsDead,
				CharacterTransform = transform,
				CharacterStats = _context.CharacterStats,
				IsFalling = _context.IsFalling,
				InvulnerabilityLogic = _context.InvulnerabilityLogic,
				TriggerStagger = _context.TriggerStagger,
				BodyAttackView = _context.BodyAttackView,
				StaminaLogic = _context.StaminaLogic,

				MinimumFallDamageHeight = 3.0f,
				LethalFallHeight = 15.0f,
				StaggerThreshold = 5.0f
			});

			_staminaLogic.Initialize(new StaminaLogic.Context
			{
				CharacterConfig = _context.Config,
				CurrentWeapon = _context.RightWeapon,
				Stamina = _context.CharacterStats.Stamina,
				StaminaMax = _context.CharacterStats.StaminaMax
			});

			CharacterStateMachine = new CharacterStateMachine(_context);
			_context.CurrentState = CharacterStateMachine.CurrentState;
			_context.Animator.Playable.UpdateMode = DirectorUpdateMode.Manual;
			_context.Animator.Animator.enabled = true;
			_context.Animator.Animator.runtimeAnimatorController = null;

			_context.RightWeapon.Value.Initialize(_context);
			_context.LeftWeapon.Value?.Initialize(_context);
			_context.BodyAttackView.Initialize(_context);

			var damageReceivers = GetComponentsInChildren<DamageReceiver>();
			foreach(var damageReceiver in damageReceivers)
			{
				damageReceiver.Initialize(new DamageReceiver.DamageReceiverContext
				{
					Team = _context.Team,
					CharacterId = _context.CharacterId,
					ApplyDamage = _context.ApplyDamage,
					InvulnerabilityLogic = _context.InvulnerabilityLogic
				});
			}

		 
			if(_context.ParryReceiver != null)
			{
				_context.ParryReceiver.Initialize(new ParryReceiver.Context
				{
					Team = _context.Team,
					CharacterId = _context.CharacterId,
					OnParryTriggered = _context.OnParryTriggered
				});
			}

			_healthLogic = new HealthLogic(new HealthLogic.Context
			{
				ApplyDamage = _context.ApplyDamage,
				IsDead = _context.IsDead,
				CharacterStats = _context.CharacterStats
			});

			_poiseLogic.SetContext(new PoiseLogic.Context
			{
				ApplyDamage = _context.ApplyDamage,
				Stats = _context.CharacterStats,
				TriggerStagger = _context.TriggerStagger
			});

			if(isPlayer)
			{
				_brain = new PlayerInputController(locationContext.MainCamera);
				_brain.Initialize(_context);
			}
			else
			{
				_brain = GetComponent<ICharacterBrain>();
				if(_brain != null)
				{
					_brain.Initialize(_context);
				}
				else
				{
					Debug.LogError($"Brain not found for character {transform.GetFullPathInScene()}");
					return;
				}

				CreateCharacterUi();
			}

			locationContext.LocationUpdate.OnExecute += CustomUpdate;
			_debugDrawer.Initialize(transform, _context, CharacterStateMachine, _brain);
			_context.DebugDrawer.Value = _debugDrawer;

			void CreateCharacterUi()
			{
				var uiPrefab = AddressableManager.GetPreloadedAsset<GameObject>(AddressableAssetNames.CharacterUi);
				_worldSpaceUi = Instantiate(uiPrefab).GetComponent<CharacterWorldSpaceUi>();
				_worldSpaceUi.Initialize(new CharacterWorldSpaceUi.CharacterWorldSpaceUiContext
				{
					CharacterStats = _context.CharacterStats,
					UiPivotWorld = _uiPivot,
					LocationUiUpdate = locationContext.LocationUiUpdate
				});
			}
		}

		private void InitializeInventory()
		{
			List<InventoryItemSaveData> saveData;
			
			if(_context.IsPlayer.Value)
			{
				saveData = GameStaticContext.Instance.InventoryDomain.InventoryItemsData;	
			}
			else
			{
				var npcInventoryComponent = gameObject.GetComponent<NpcInventoryConfigView>();
				if(npcInventoryComponent != null)
				{
					saveData = npcInventoryComponent.Items;
				}
				else
				{
					saveData = new List<InventoryItemSaveData>();
				}
			}
			
			_inventoryLogic.Initialize(_context, saveData );
		}

		[Button]
		public void GenerateUniqueId()
		{
			UniqueId = name + Random.value;
		}

		private void CustomUpdate(float deltaTime)
		{
			var personalDeltaTime = deltaTime * _context.DeltaTimeMultiplier.Value;

			_brain.Think(personalDeltaTime);
			var calculateInputLogic = true;

			while(personalDeltaTime > 0f)
			{
				var deltaTimeStep = Mathf.Min(personalDeltaTime, _context.MaxDeltaTime.Value);
				personalDeltaTime -= deltaTimeStep;
				CharacterStateMachine.Update(deltaTimeStep, calculateInputLogic);
				_context.RightWeapon.Value?.CustomUpdate(deltaTimeStep);
				_movementLogic.Update(deltaTimeStep);
				_context.Animator.Playable.Graph.Evaluate(deltaTimeStep);
				_lockOnLogic.Update(deltaTimeStep);
				_staminaLogic.Update(deltaTimeStep);
				_poiseLogic.Update(deltaTimeStep);

				_fallDamageLogic.CustomUpdate(deltaTimeStep);

				calculateInputLogic = false;
			}
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			_debugDrawer.OnDrawGizmos();
		}
#endif
	}
}
