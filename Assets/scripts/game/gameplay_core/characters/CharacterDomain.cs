using Animancer;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.utils.serialization;
using game.gameplay_core.characters.player;
using game.gameplay_core.characters.state_machine;
using game.gameplay_core.damage_system;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace game.gameplay_core.characters
{
	public class CharacterDomain : MonoBehaviour, IOnSceneUniqueIdOwner
	{
		private CharacterStateMachine _stateMachine;
		private CharacterContext _context;

		private ICharacterBrain _brain;

		[SerializeField]
		private CharacterDebugDrawer _debugDrawer;

		[field: SerializeField]
		public string UniqueId { get; private set; }

		[SerializeField]
		private CharacterConfig _config;

		[field: SerializeField]
		public WeaponDomain DebugWeapon { get; private set; }

		public void Initialize(LocationContext locationContext)
		{
			_context = new CharacterContext(transform)
			{
				WalkSpeed = new ReactiveProperty<float>(_config.WalkSpeed),
				RotationSpeed = new ReactiveProperty<RotationSpeedData>(_config.RotationSpeed),
				Config = _config,
				MovementController = GetComponent<CharacterController>(),
				CurrentWeapon = new ReactiveProperty<WeaponDomain>(DebugWeapon),
				Animator = GetComponent<AnimancerComponent>()
			};
			_stateMachine = new CharacterStateMachine(_context);

			if(UniqueId == "Player")
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
			}

			locationContext.LocationUpdate.OnExecute += CustomUpdate;

			_debugDrawer.Initialize(transform, _context, _stateMachine);
		}

		private void CustomUpdate(float deltaTime)
		{
			_brain.Think(deltaTime);
			_stateMachine.Update(deltaTime);
		}

		[Button]
		public void GenerateUniqueId()
		{
			UniqueId = name + Random.value;
		}
#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			_debugDrawer.OnDrawGizmos();
		}
#endif
	}
}
