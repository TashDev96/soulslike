using dream_lib.src.utils.serialization;
using game.gameplay_core.characters.player;
using game.gameplay_core.characters.state_machine;
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

		public void Initialize(bool isPlayer)
		{
			_context.InputData = new CharacterInputData();

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

		public void GenerateUniqueId()
		{
			UniqueId = "Character" + Random.value;
		}
	}
}
