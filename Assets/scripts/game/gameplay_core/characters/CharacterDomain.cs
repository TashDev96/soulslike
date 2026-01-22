using System.Collections.Generic;
using Animancer;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
using dream_lib.src.utils.serialization;
using game.enums;
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
using game.gameplay_core.worldspace_ui;
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
		private DeathLogic _deathLogic;

		[ShowInInspector]
		private CharacterStats _characterStats;

		private readonly Dictionary<ArmamentSlot, WeaponView> _equippedWeaponsViews = new();
		private CharacterBodyView _characterBodyView;
		private CharacterSaveData _saveData;
		private TransformCache _respawnTransform;

		[field: SerializeField]
		public string UniqueId { get; private set; }

		[field: SerializeField]
		private SerializableDictionary<ArmamentSlot, Transform> ArmSockets { get; set; }

		public CharacterExternalData ExternalData { get; private set; }
		public CharacterContext Context => _context;
		public CharacterConfig Config => _config;
		public CharacterStateMachine CharacterStateMachine { get; private set; }
		public CharacterInventoryLogic InventoryLogic { get; private set; }

		public void Initialize()
		{
			var isPlayer = UniqueId == "Player";

			_transform = new ReadOnlyTransform(transform);

			var isDead = new IsDead();
			isDead.OnChanged += HandleDeath;

			_lockOnLogic = new LockOnLogic();
			_blockLogic = new BlockLogic();
			_invulnerabilityLogic = new InvulnerabilityLogic();
			_fallDamageLogic = new FallDamageLogic();
			_staminaLogic = new StaminaLogic();
			_poiseLogic = new PoiseLogic();
			_statsLogic = new StatsLogic();

			InventoryLogic = new CharacterInventoryLogic();

			var isFalling = new ReactiveProperty<bool>();

			_characterStats = new CharacterStats();

			var characterCollider = GetComponent<CapsuleCharacterCollider>();

			_context = new CharacterContext
			{
				LocationTime = LocationStaticContext.Instance.LocationTime,
				SelfLink = this,

				MovementLogic = _movementLogic,
				LockOnLogic = _lockOnLogic,
				InvulnerabilityLogic = _invulnerabilityLogic,
				IsFalling = isFalling,
				FallDamageLogic = _fallDamageLogic,
				StaminaLogic = _staminaLogic,
				PoiseLogic = _poiseLogic,
				BlockLogic = _blockLogic,
				InventoryLogic = InventoryLogic,

				Config = _config,
				Transform = _transform,
				Animator = GetComponent<AnimancerComponent>(),
				DeadStateRoot = _deadStateRoot,
				CharacterStats = _characterStats,
				LockOnPoints = GetComponentsInChildren<LockOnPointView>(),
				InputData = new CharacterInputData(),
				CharacterCollider = characterCollider,

				EquippedWeaponViews = _equippedWeaponsViews,
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
				EnteredTriggers = new ReactiveHashSet<Collider>(),

				DebugDrawer = new ReactiveProperty<CharacterDebugDrawer>(),
				OnStateChanged = new ReactiveCommand<CharacterStateBase, CharacterStateBase>(),
				DeflectCurrentAttack = new ReactiveCommand(),

				OnParryTriggered = new ReactiveCommand<CharacterDomain>()
			};

			_statsLogic.SetContext(_context);
			_lockOnLogic.SetContext(_context);

			InitializeInventory();

			ExternalData = new CharacterExternalData(_context);

			characterCollider.SetContext(_context);

			_deathLogic = new DeathLogic(_context);

			_movementLogic.SetContext(_context, transform);

			_blockLogic.SetContext(_context);

			_fallDamageLogic.SetContext(_context);

			_staminaLogic.Initialize(_context);

			CharacterStateMachine = new CharacterStateMachine(_context);
			_context.CurrentState = CharacterStateMachine.CurrentState;
			_context.Animator.Playable.UpdateMode = DirectorUpdateMode.Manual;
			_context.Animator.Animator.enabled = true;
			_context.Animator.Animator.runtimeAnimatorController = null;

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
				_context.ParryReceiver.Initialize(_context);
			}

			_healthLogic = new HealthLogic(_context);

			_poiseLogic.SetContext(_context);

			if(isPlayer)
			{
				_brain = new PlayerInputController(LocationStaticContext.Instance.CameraController);
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

			_characterBodyView = GetComponentInChildren<CharacterBodyView>();
			_characterBodyView.Initialize(_context.ApplyDamage);

			LocationStaticContext.Instance.LocationUpdate.OnExecute += CustomUpdate;
			_debugDrawer.Initialize(transform, _context, CharacterStateMachine, _brain);
			_context.DebugDrawer.Value = _debugDrawer;

			_context.ApplyDamage.Subscribe(info =>
			{
				if(_context.IsDead.Value)
				{
					return;
				}
				if(info.DamageAmount <= 0)
				{
					return;
				}
				GameStaticContext.Instance.FloatingTextsManager.ShowFloatingText(info.DamageAmount.RoundFormat(), FloatingTextView.TextColorVariant.Red, _characterBodyView.GetTopPos());
			});

			void CreateCharacterUi()
			{
				var uiPrefab = AddressableManager.GetPreloadedAsset<GameObject>(AddressableAssetNames.CharacterUi);
				_worldSpaceUi = Instantiate(uiPrefab).GetComponent<CharacterWorldSpaceUi>();
				_worldSpaceUi.Initialize(new CharacterWorldSpaceUi.CharacterWorldSpaceUiContext
				{
					CharacterContext = _context,
					UiPivotWorld = _uiPivot,
					LocationUiUpdate = LocationStaticContext.Instance.LocationUiUpdate
				});
			}
		}

		private void InitializeInventory()
		{
			if(_context.IsPlayer.Value)
			{
				InventoryLogic.Initialize(_context, GameStaticContext.Instance.InventoryDomain.InventoryData);
			}
			else
			{
				var npcInventoryComponent = gameObject.GetComponent<NpcInventoryConfigView>();
				if(npcInventoryComponent != null)
				{
					InventoryLogic.Initialize(_context, npcInventoryComponent.InventoryData);
				}
				else
				{
					InventoryLogic.Initialize(_context, new InventoryData());
				}
			}

			SetWeapon(ArmamentSlot.Left, InventoryLogic.GetArmament(ArmamentSlot.Left));
			SetWeapon(ArmamentSlot.Right, InventoryLogic.GetArmament(ArmamentSlot.Right));
		}

		[Button]
		public void GenerateUniqueId()
		{
			UniqueId = name + Random.value;
		}

		public void SetWeapon(ArmamentSlot slot, BaseItemLogic logic)
		{
			if(logic == null)
			{
				if(_equippedWeaponsViews.TryGetValue(slot, out var view))
				{
					Destroy(view.gameObject);
					_equippedWeaponsViews.Remove(slot);
				}
				return;
			}

			var socket = ArmSockets[slot];
			if(socket == null)
			{
				Debug.LogError($"{slot} hand socket is not assigned!");
				return;
			}

			if(logic is WeaponItemLogic weaponLogic)
			{
				if(_equippedWeaponsViews.TryGetValue(slot, out var oldView))
				{
					Destroy(oldView.gameObject);
				}

				var config = weaponLogic.Config;
				var prefab = AddressableManager.GetPreloadedAsset<GameObject>(config.WeaponPrefabName);
				var weaponInstance = Instantiate(prefab, socket);
				weaponInstance.name = prefab.name;
				weaponInstance.transform.ResetLocal();
				var weaponView = weaponInstance.GetComponent<WeaponView>();
				weaponView.Config = config;
				weaponView.Initialize(_context);

				_equippedWeaponsViews[slot] = weaponView;
			}
		}

		public void SetSaveData(CharacterSaveData data, bool ignoreStats = false)
		{
			_saveData = data;
			_respawnTransform = new TransformCache(transform);
			if(data.Initialized)
			{
				transform.position = data.Position;
				transform.eulerAngles = data.Euler;
				if(!ignoreStats)
				{
					_context.CharacterStats.Hp.Value = data.Hp;
					_context.CharacterStats.Stamina.Value = data.Stamina;
				}
			}
			else
			{
				_context.CharacterStats.SetStatsToMax();
			}
		}

		public void WriteStateToSaveData()
		{
			_saveData.Position = transform.position;
			_saveData.Euler = transform.eulerAngles;
			_saveData.Hp = _context.CharacterStats.Hp.Value;
			_saveData.Stamina = _context.CharacterStats.Stamina.Value;
			_saveData.Initialized = true;
		}

		public void HandleLocationRespawn()
		{
			_context.CharacterStats.SetStatsToMax();
			_context.InventoryLogic.HandleRespawn();
			_context.IsDead.Value = false;
			_movementLogic.Teleport(_respawnTransform);
			CharacterStateMachine.Reset();
			_brain.Reset();
			_worldSpaceUi?.Reset();
		}

		public void SetRespawnTransform(TransformCache transformCache)
		{
			_respawnTransform = transformCache;
		}

		private void HandleDeath(bool isDead)
		{
			gameObject.GetComponent<CapsuleCollider>().enabled = !isDead;
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
				_movementLogic.Update(deltaTimeStep);
				_context.BodyAttackView.CustomUpdate(deltaTimeStep);
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
