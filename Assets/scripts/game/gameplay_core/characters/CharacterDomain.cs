using Animancer;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.utils.serialization;
using game.gameplay_core.characters.player;
using game.gameplay_core.characters.state_machine;
using game.gameplay_core.damage_system;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
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
				Animator = GetComponent<AnimancerComponent>(),
				DeltaTimeMultiplier = new ReactiveProperty<float>(1),
				MaxDeltaTime = new ReactiveProperty<float>(1),
			};

			_stateMachine = new CharacterStateMachine(_context);
			_context.Animator.Playable.UpdateMode = DirectorUpdateMode.Manual;
			_context.Animator.Animator.enabled = true;

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
			var deltaTimeLeft = deltaTime * _context.DeltaTimeMultiplier.Value;
			Debug.Log($"{(1f/deltaTime):0.##}");

			while(deltaTimeLeft > 0f)
			{
				var deltaTimeStep = Mathf.Min(deltaTimeLeft, _context.MaxDeltaTime.Value);
				deltaTimeLeft -= deltaTimeStep;
				_brain.Think(deltaTimeStep);
				_stateMachine.Update(deltaTimeStep);
				_context.Animator.Playable.Graph.Evaluate(deltaTimeStep);
			}
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
