using dream_lib.src.extensions;
using game.gameplay_core.characters.commands;
using UnityEngine;

namespace game.gameplay_core.characters.ai
{
	public class DummyBrain : MonoBehaviour, ICharacterBrain
	{
		private CharacterContext _characterContext;

		private float _timer;
		private Vector3 _startPos;
		private CharacterCommand _selectedCommand;

		public void Initialize()
		{
			_startPos = transform.position;
		}

		public void Initialize(CharacterContext context)
		{
			_characterContext = context;
		}

		public void Think(float deltaTime)
		{
			_timer -= deltaTime;
			if(_timer <= 0)
			{
				switch(Random.value)
				{
					case < 0.2f:
						_selectedCommand = CharacterCommand.Walk;
						_characterContext.InputData.DirectionWorld = new Vector3().AddRandom(-1, 1).SetY(0).normalized;
						_timer = Random.Range(1, 5f);
						break;
					case < 0.5f:
						_selectedCommand = CharacterCommand.Walk;
						_characterContext.InputData.DirectionWorld = (_startPos - _characterContext.Transform.position).normalized;
						_timer = Random.Range(1, 5f);
						break;
					default:
						_selectedCommand = CharacterCommand.None;
						_timer = Random.Range(1, 2f);
						break;
				}
			}

			_characterContext.InputData.Command = _selectedCommand;
		}
	}
}
