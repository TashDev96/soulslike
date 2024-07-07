using System;
using dream_lib.src.utils.serialization;
using game.gameplay_core.characters.player;
using game.gameplay_core.characters.state_machine;
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

		public void Initialize(LocationContext locationContext)
		{
			_context = new CharacterContext(transform);
			_stateMachine = new CharacterStateMachine(_context);

			if(UniqueId == "Player")
			{
				_brain = new PlayerInputController(_context.InputData, transform, locationContext.MainCamera);
			}
			else
			{
				_brain = GetComponent<ICharacterBrain>();
			}

			locationContext.LocationUpdate.OnExecute += CustomUpdate;

			_debugDrawer.Initialize(transform, _context, _stateMachine);
		}

		public void CustomUpdate(float deltaTime)
		{
			_brain.Update(deltaTime);
			_stateMachine.Update(deltaTime);
		}

		[Button]
		public void GenerateUniqueId()
		{
			UniqueId = name + Random.value;
		}

		private void OnDrawGizmos()
		{
			_debugDrawer.OnDrawGizmos();	
		}
	}
}
