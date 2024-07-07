using dream_lib.src.utils.serialization;
using game.gameplay_core.characters.player;
using game.gameplay_core.characters.state_machine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters
{
	public class CharacterDomain : MonoBehaviour, IOnSceneUniqueIdOwner
	{
		private CharacterStateMachine _stateMachine;
		private readonly CharacterContext _context = new();

		private ICharacterBrain _brain;

		[field: SerializeField]
		public string UniqueId { get; private set; }

		public void Initialize()
		{
			_context.InputData = new CharacterInputData();

			_stateMachine = new CharacterStateMachine(_context);

			if(UniqueId == "Player")
			{
				_brain = new PlayerInputController(_context.InputData, transform);
			}
			else
			{
				_brain = GetComponent<ICharacterBrain>();
			}
		}

		public void CustomUpdate(float deltaTime)
		{
			_brain.Update(deltaTime);
		}

		[Button]
		public void GenerateUniqueId()
		{
			UniqueId = name + Random.value;
		}
	}
}
