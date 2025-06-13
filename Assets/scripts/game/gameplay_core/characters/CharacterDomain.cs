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
using game.gameplay_core.characters.view;
using game.gameplay_core.characters.view.ui;
using game.gameplay_core.damage_system;
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

		private CharacterStateMachine _stateMachine;
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

		[ShowInInspector]
		private CharacterStats _characterStats;

		[field: SerializeField]
		public string UniqueId { get; private set; }

		[field: SerializeField]
		private WeaponView DebugWeapon { get; set; }

		public CharacterExternalData ExternalData { get; private set; }
		public CharacterConfig Config => _config;

		public void Initialize(LocationContext locationContext)
		{
			var isPlayer = UniqueId == "Player";

			_transform = new ReadOnlyTransform(transform);

			_lockOnLogic = new LockOnLogic(new LockOnLogic.Context
			{
				CharacterTransform = _transform,
				AllCharacters = locationContext.Characters,
				Self = this,
				MovementLogic = _movementLogic
			});

			_invulnerabilityLogic = new InvulnerabilityLogic();
			_fallDamageLogic = new FallDamageLogic();
			_staminaLogic = new StaminaLogic();

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

				Config = _config,
				Transform = _transform,
				Animator = GetComponent<AnimancerComponent>(),
				DeadStateRoot = _deadStateRoot,
				CharacterStats = _characterStats,
				LockOnTargets = GetComponentsInChildren<LockOnTargetView>(),
				InputData = new CharacterInputData(),

				WeaponView = new ReactiveProperty<WeaponView>(DebugWeapon),
				BodyAttackView = GetComponentInChildren<BodyAttackView>(),

				WalkSpeed = new ReactiveProperty<float>(_config.Locomotion.WalkSpeed),
				RunSpeed = new ReactiveProperty<float>(_config.Locomotion.RunSpeed),
				RotationSpeed = new ReactiveProperty<RotationSpeedData>(_config.RotationSpeed),
				DeltaTimeMultiplier = new ReactiveProperty<float>(1),
				MaxDeltaTime = new ReactiveProperty<float>(1),
				CharacterId = new ReactiveProperty<string>(UniqueId),
				Team = new ReactiveProperty<Team>(isPlayer ? Team.Player : Team.HostileNPC),
				IsPlayer = new ReactiveProperty<bool>(isPlayer),
				ApplyDamage = new ApplyDamageCommand(),
				IsDead = new IsDead(),
				TriggerStagger = new ReactiveCommand(),

				DebugDrawer = new ReactiveProperty<CharacterDebugDrawer>(),
				OnStateChanged = new ReactiveCommand<CharacterStateBase, CharacterStateBase>()
			};

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
				CurrentWeapon = _context.WeaponView,
				Stamina = _context.CharacterStats.Stamina,
				StaminaMax = _context.CharacterStats.StaminaMax
			});

			_stateMachine = new CharacterStateMachine(_context);
			_context.CurrentState = _stateMachine.CurrentState;
			_context.Animator.Playable.UpdateMode = DirectorUpdateMode.Manual;
			_context.Animator.Animator.enabled = true;
			_context.Animator.Animator.runtimeAnimatorController = null;

			_context.WeaponView.Value.Initialize(_context);
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

			_healthLogic = new HealthLogic(new HealthLogic.Context
			{
				ApplyDamage = _context.ApplyDamage,
				IsDead = _context.IsDead,
				CharacterStats = _context.CharacterStats,
				InvulnerabilityLogic = _context.InvulnerabilityLogic
			});

			_poiseLogic = new PoiseLogic(new PoiseLogic.Context
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
			_debugDrawer.Initialize(transform, _context, _stateMachine, _brain);
			_context.DebugDrawer.Value = _debugDrawer;
		}

		[Button]
		public void GenerateUniqueId()
		{
			UniqueId = name + Random.value;
		}

		private void CreateCharacterUi()
		{
			var uiPrefab = AddressableManager.GetPreloadedAsset<GameObject>(AddressableAssetNames.CharacterUi);
			_worldSpaceUi = Instantiate(uiPrefab).GetComponent<CharacterWorldSpaceUi>();
			_worldSpaceUi.Initialize(new CharacterWorldSpaceUi.CharacterWorldSpaceUiContext
			{
				CharacterStats = _context.CharacterStats,
				UiPivotWorld = _uiPivot
			});
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
				_stateMachine.Update(deltaTimeStep, calculateInputLogic);
				_context.WeaponView.Value?.CustomUpdate(deltaTimeStep);
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
