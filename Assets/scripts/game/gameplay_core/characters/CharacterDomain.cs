using System.Collections.Generic;
using Animancer;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
using dream_lib.src.utils.serialization;
using game.enums;
using game.gameplay_core.characters.ai;
using game.gameplay_core.characters.ai.sensors;
using game.gameplay_core.characters.bosses;
using game.gameplay_core.characters.config;
using game.gameplay_core.characters.logic;
using game.gameplay_core.characters.player;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.characters.state_machine;
using game.gameplay_core.characters.state_machine.states;
using game.gameplay_core.characters.state_machine.states.stagger;
using game.gameplay_core.characters.stats;
using game.gameplay_core.characters.stats.runtime_data;
using game.gameplay_core.characters.view;
using game.gameplay_core.characters.view.ui;
using game.gameplay_core.damage_system;
using game.gameplay_core.inventory;
using game.gameplay_core.inventory.items_logic;
using game.gameplay_core.inventory.serialized_data;
using game.gameplay_core.location;
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

		[BoxGroup("Pivots Setup")]
		
		[SerializeField]
		[BoxGroup("Pivots Setup")]
		private Transform _uiPivot;

		private ICharacterWorldSpaceUi _worldSpaceUi;

		private ICharacterBrain _brain;
		private CharacterContext _context;

		private CharacterStatsData _characterStats;

		private CharacterSaveData _saveData;
		private TransformCache _respawnTransform;
		private CharacterSensorsDomain _sensorsDomain;

		[field: SerializeField]
		public string UniqueId { get; private set; }
		[field: SerializeField]
		public bool IsBoss { get; private set; }
		[field: SerializeField]
		[field: BoxGroup("Pivots Setup")]
		public List<PlungeAttackTargetView> PlungeAttackPivots { get; private set; }

		[field: SerializeField]
		[field: BoxGroup("Pivots Setup")]
		private SerializableDictionary<EquipmentSlotType, Transform> ArmSockets { get; set; }

		public CharacterExternalData ExternalData { get; private set; }
		public CharacterContext Context => _context;
		public CharacterConfig Config => _config;
		public CharacterStateMachine CharacterStateMachine { get; private set; }

		public void Initialize()
		{
			var isPlayer = UniqueId == "Player";


			var isFalling = new ReactiveProperty<bool>();

			_characterStats = new CharacterStatsData();

			var characterCollider = GetComponent<CapsuleCharacterCollider>();

			_context = new CharacterContext
			{
				LocationTime = LocationStaticContext.Instance.LocationTime,
				SelfLink = this,

				IsFalling = isFalling,

				Config = _config,
				Transform = new CharacterTransform(transform),
				RigidBody = new RigidBodyWrapper(GetComponent<Rigidbody>()),

				CharacterStats = _characterStats,
				InputData = new CharacterInputData(),
				CharacterCollider = characterCollider,

				CurrentConsumableItem = new ReactiveProperty<IConsumableItemLogic>(),

				DeltaTimeMultiplier = new ReactiveProperty<float>(1),
				MaxDeltaTime = new ReactiveProperty<float>(1),
				CharacterId = new ReactiveProperty<string>(UniqueId),
				Team = new ReactiveProperty<Team>(isPlayer ? Team.Player : Team.HostileNPC),
				IsPlayer = new ReactiveProperty<bool>(isPlayer),
				IsDead = new IsDead(),

				EnteredTriggers = new ReactiveHashSet<Collider>(),

				Events =
				{
					ApplyDamage = new ApplyDamageCommand(),
					OnStateChanged = new ReactiveCommand<CharacterStateBase, CharacterStateBase>(),
					DeflectCurrentAttack = new ReactiveCommand(),
					OnParryTriggered = new ReactiveCommand<CharacterDomain>(),
					TriggerStagger = new ReactiveCommand<StaggerReason>(),
					TriggerPlungeAttack = new ReactiveCommand<CharacterDomain, PlungeAttackTargetView>()
				},

				Views =
				{
					Animator = GetComponent<AnimancerComponent>(),
					BodyView = GetComponentInChildren<CharacterBodyView>(),
					LockOnPoints = GetComponentsInChildren<LockOnPointView>(),
					EquippedWeaponViews = new Dictionary<EquipmentSlotType, WeaponView>(),
					BodyAttackView = GetComponentInChildren<BodyAttackView>(),
					ParryReceiver = GetComponentInChildren<ParryReceiver>(true),
					DebugDrawer = new ReactiveProperty<CharacterDebugDrawer>()
				},
				Logic =
				{
					MovementLogic = new MovementLogic(),
					LockOnLogic = new LockOnLogic(),
					InvulnerabilityLogic = new InvulnerabilityLogic(),
					FallDamageLogic = new FallDamageLogic(),
					StaminaLogic = new StaminaLogic(),
					PoiseLogic = new PoiseLogic(),
					BlockLogic = new BlockLogic(),
					HealthLogic = new HealthLogic(),
					StatsLogic = new CharacterStatsLogic(),
					DeathLogic = new DeathLogic(),
					InventoryLogic = new CharacterInventoryLogic(),
					InteractionLogic = new InteractionLogic()
				}
			};

			_context.Logic.StatsLogic.SetContext(_context);
			_context.Logic.LockOnLogic.SetContext(_context);
			_context.Logic.HealthLogic.SetContext(_context);
			_context.Logic.DeathLogic.SetContext(_context);
			_context.Logic.MovementLogic.SetContext(_context);
			_context.Logic.BlockLogic.SetContext(_context);
			_context.Logic.FallDamageLogic.SetContext(_context);
			_context.Logic.StaminaLogic.Initialize(_context);
			_context.Logic.PoiseLogic.SetContext(_context);
			_context.Logic.InteractionLogic.SetContext(_context);

			InitializeInventory();

			_context.Logic.StatsLogic.RecalculateStats();

			ExternalData = new CharacterExternalData(_context);

			characterCollider.SetContext(_context);

			CharacterStateMachine = new CharacterStateMachine(_context);
			_context.CurrentState = CharacterStateMachine.CurrentState;
			_context.Views.Animator.Playable.UpdateMode = DirectorUpdateMode.Manual;
			_context.Views.Animator.Animator.enabled = true;
			_context.Views.Animator.Animator.runtimeAnimatorController = null;

			if(_context.Views.BodyAttackView == null)
			{
				Debug.LogError($"{transform.GetFullPathInScene()}");
			}
			_context.Views.BodyAttackView.Initialize(_context);

			var damageReceivers = GetComponentsInChildren<DamageReceiver>();
			foreach(var damageReceiver in damageReceivers)
			{
				damageReceiver.Initialize(_context);
			}

			if(_context.Views.ParryReceiver != null)
			{
				_context.Views.ParryReceiver.Initialize(_context);
			}

			_sensorsDomain = GetComponent<CharacterSensorsDomain>();
			if(_sensorsDomain != null)
			{
				_sensorsDomain.Initialize(this);
			}
			_context.SensorsDomain = _sensorsDomain;

			if(isPlayer)
			{
				_brain = new PlayerInputController(LocationStaticContext.Instance.CameraController);
				_brain.Initialize(_context);

				CreatePlayerWorldspaceUi();
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

				if(!IsBoss)
				{
					CreateNpcUi();
				}
			}

			_context.Views.BodyView.Initialize(_context);

			LocationStaticContext.Instance.LocationUpdate.OnExecute += CustomUpdate;
			_debugDrawer.Initialize(_context, CharacterStateMachine, _brain);
			_context.Views.DebugDrawer.Value = _debugDrawer;

			var customScripts = GetComponentsInChildren<AbstractCharacterScript>();
			foreach(var customScript in customScripts)
			{
				customScript.SetContext(_context);
			}

			_context.Events.ApplyDamage.Subscribe(info =>
			{
				if(info.DamageAmount <= 0)
				{
					return;
				}
				if(_context.IsPlayer.Value)
				{
					var damageString = info.DamageAmount < 20 ? info.DamageAmount.RoundFormat() : info.DamageAmount.RoundFormat(1);
					GameStaticContext.Instance.FloatingTextsManager.ShowFloatingText(damageString, FloatingTextView.TextColorVariant.Red, _context.Views.BodyView.GetTopPos());
				}
			});

			void CreateNpcUi()
			{
				var uiPrefab = AddressableManager.GetPreloadedAsset<GameObject>(AddressableAssetNames.CharacterWorldSpaceUi);
				var worldUi = Instantiate(uiPrefab).GetComponent<CharacterWorldSpaceUi>();
				worldUi.Initialize(new CharacterWorldSpaceUi.CharacterWorldSpaceUiContext
				{
					CharacterContext = _context,
					UiPivotWorld = _uiPivot,
					LocationUiUpdate = LocationStaticContext.Instance.LocationUiUpdate
				});
				_worldSpaceUi = worldUi;
			}

			void CreatePlayerWorldspaceUi()
			{
				var uiPrefab = AddressableManager.GetPreloadedAsset<GameObject>(AddressableAssetNames.PlayerWorldSpaceUi);
				var worldUi = Instantiate(uiPrefab).GetComponent<PlayerWorldSpaceUi>();
				worldUi.Initialize(_context);
				_worldSpaceUi = worldUi;
			}
		}

		private void InitializeInventory()
		{
			if(_context.IsPlayer.Value)
			{
				_context.Logic.InventoryLogic.Initialize(_context, GameStaticContext.Instance.InventoryDomain.InventoryData);
			}
			else
			{
				var npcInventoryComponent = gameObject.GetComponent<NpcInventoryConfigView>();
				if(npcInventoryComponent != null)
				{
					_context.Logic.InventoryLogic.Initialize(_context, npcInventoryComponent.InventoryData);
				}
				else
				{
					_context.Logic.InventoryLogic.Initialize(_context, new InventoryData());
				}
			}

			_context.Logic.InventoryLogic.OnEquipChanged += HandleEquipChanged;

			SetWeapon(EquipmentSlotType.LeftHand, _context.Logic.InventoryLogic.GetEquipment(EquipmentSlotType.LeftHand));
			SetWeapon(EquipmentSlotType.RightHand, _context.Logic.InventoryLogic.GetEquipment(EquipmentSlotType.RightHand));
		}

		private void Awake()
		{
			if(Application.isPlaying)
			{
				//destroy weapon views added while editing animations
				ArmSockets[EquipmentSlotType.RightHand]?.DestroyAllChildren();
			}
		}

		[Button]
		public void GenerateUniqueId()
		{
			UniqueId = name + Random.value;
		}

		public void SetWeapon(EquipmentSlotType slot, BaseItemLogic logic)
		{
			if(logic == null)
			{
				if(_context.Views.EquippedWeaponViews.TryGetValue(slot, out var view))
				{
					Destroy(view.gameObject);
					_context.Views.EquippedWeaponViews.Remove(slot);
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
				if(_context.Views.EquippedWeaponViews.TryGetValue(slot, out var oldView))
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

				_context.Views.EquippedWeaponViews[slot] = weaponView;
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
				if(data.Hp <= 0)
				{
					CharacterStateMachine.ForceDeadStateOnLoad();
					if(IsBoss)
					{
						gameObject.SetActive(false);
					}
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
			if(IsBoss && _context.IsDead.Value)
			{
				return;
			}
			_context.CharacterStats.SetStatsToMax();
			_context.Logic.InventoryLogic.HandleRespawn();
			_context.IsDead.Value = false;
			_context.Logic.MovementLogic.Teleport(_respawnTransform);
			CharacterStateMachine.Reset();
			_brain.Reset();
			_sensorsDomain.Reset();
			_worldSpaceUi?.Reset();
		}

		public void SetRespawnTransform(TransformCache transformCache)
		{
			_respawnTransform = transformCache;
		}

		private void HandleEquipChanged(EquipmentSlotAdress address, BaseItemLogic item)
		{
			if(address.SlotType == EquipmentSlotType.LeftHand || address.SlotType == EquipmentSlotType.RightHand)
			{
				SetWeapon(address.SlotType, item);
			}
		}

		private void CustomUpdate(float deltaTime)
		{
			if(_context.IsDead.Value)
			{
				CharacterStateMachine.Update(deltaTime, true);
				_context.Logic.MovementLogic.Update(deltaTime);
				_context.Logic.HealthLogic.Update(deltaTime);
				_context.Views.Animator.Playable.Graph.Evaluate(deltaTime);
				return;
			}

			var personalDeltaTime = deltaTime * _context.DeltaTimeMultiplier.Value;

			if(_sensorsDomain != null)
			{
				_sensorsDomain.CustomUpdate(deltaTime);
			}
			_brain.Think(personalDeltaTime);
			var calculateInputLogic = true;

			while(personalDeltaTime > 0f)
			{
				var deltaTimeStep = Mathf.Min(personalDeltaTime, _context.MaxDeltaTime.Value);
				personalDeltaTime -= deltaTimeStep;
				CharacterStateMachine.Update(deltaTimeStep, calculateInputLogic);
				_context.Logic.MovementLogic.Update(deltaTimeStep);
				_context.Views.BodyAttackView.CustomUpdate(deltaTimeStep);
				_context.Views.Animator.Playable.Graph.Evaluate(deltaTimeStep);
				_context.Logic.LockOnLogic.Update(deltaTimeStep);
				_context.Logic.StaminaLogic.Update(deltaTimeStep);
				_context.Logic.PoiseLogic.Update(deltaTimeStep);
				_context.Logic.HealthLogic.Update(deltaTimeStep);

				_context.Logic.FallDamageLogic.CustomUpdate(deltaTimeStep);

				calculateInputLogic = false;
			}

#if UNITY_EDITOR
			_debugDrawer.CustomUpdate(deltaTime);
			_context.Transform.ClearLog();
#endif
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			_debugDrawer.OnDrawGizmos();
		}

		private void OnDrawGizmosSelected()
		{
			foreach(var plungeAttackPivot in PlungeAttackPivots)
			{
				plungeAttackPivot.DrawGizmos();
			}
		}
#endif
	}
}
