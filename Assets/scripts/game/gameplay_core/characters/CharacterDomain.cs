using Animancer;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.utils.serialization;
using game.gameplay_core.characters.bindings;
using game.gameplay_core.characters.logic;
using game.gameplay_core.characters.player;
using game.gameplay_core.characters.state_machine;
using game.gameplay_core.characters.ui;
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
		private CharacterStateMachine _stateMachine;
		private CharacterContext _context;

		private ICharacterBrain _brain;

		private CharacterHealthLogic _healthLogic;
		private CharacterMovementLogic _movementLogic;
		private CharacterWorldSpaceUi _worldSpaceUi;

		[field: SerializeField]
		public string UniqueId { get; private set; }

		[field: SerializeField]
		public WeaponView DebugWeapon { get; private set; }

		public void Initialize(LocationContext locationContext)
		{
			var isPlayer = UniqueId == "Player";

			_movementLogic = new CharacterMovementLogic();

			_context = new CharacterContext
			{
				WalkSpeed = new ReactiveProperty<float>(_config.WalkSpeed),
				RotationSpeed = new ReactiveProperty<RotationSpeedData>(_config.RotationSpeed),
				Config = _config,
				CurrentWeapon = new ReactiveProperty<WeaponView>(DebugWeapon),
				Animator = GetComponent<AnimancerComponent>(),
				DeltaTimeMultiplier = new ReactiveProperty<float>(1),
				MaxDeltaTime = new ReactiveProperty<float>(1),
				CharacterId = new ReactiveProperty<string>(UniqueId),
				Team = new ReactiveProperty<Team>(isPlayer ? Team.Player : Team.Enemy),
				IsPlayer = new ReactiveProperty<bool>(isPlayer),
				Transform = transform,
				InputData = new CharacterInputData(),
				ApplyDamage = new ReactiveCommand<DamageInfo>(),
				CharacterStats = _config.DefaultStats,
				IsDead = new IsDead(),
				DeadStateRoot = _deadStateRoot,
				MovementLogic = _movementLogic
			};

			_movementLogic.SetContext(new CharacterMovementLogic.Context
			{
				CharacterTransform = transform,
				UnityCharacterController = GetComponent<CharacterController>(),
				IsDead = _context.IsDead
			});

			_stateMachine = new CharacterStateMachine(_context);
			_context.Animator.Playable.UpdateMode = DirectorUpdateMode.Manual;
			_context.Animator.Animator.enabled = true;
			_context.CurrentWeapon.Value.Initialize(_context);

			var damageReceivers = GetComponentsInChildren<DamageReceiver>();
			foreach(var damageReceiver in damageReceivers)
			{
				damageReceiver.Initialize(new DamageReceiver.DamageReceiverContext
				{
					Team = _context.Team,
					CharacterId = _context.CharacterId,
					ApplyDamage = _context.ApplyDamage
				});
			}

			_healthLogic = new CharacterHealthLogic(new CharacterHealthLogic.Context
			{
				ApplyDamage = _context.ApplyDamage,
				IsDead = _context.IsDead,
				CharacterStats = _context.CharacterStats
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

			_debugDrawer.Initialize(transform, _context, _stateMachine);
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
				_context.CurrentWeapon.Value?.CustomUpdate(deltaTimeStep);
				_movementLogic.Update(deltaTimeStep);
				_context.Animator.Playable.Graph.Evaluate(deltaTimeStep);
				calculateInputLogic = false;
			}

			_worldSpaceUi?.CustomUpdate(deltaTime);
		}
#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			_debugDrawer.OnDrawGizmos();
		}
#endif
	}
}
