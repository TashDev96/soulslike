using dream_lib.src.extensions;
using game.gameplay_core.characters.commands;
using UnityEngine;

namespace game.gameplay_core.characters.ai
{
	public class DummyBrain : MonoBehaviour, ICharacterBrain
	{
		[SerializeField]
		private CharacterCommand _movementCommand;

		[Header("Forced command")]
		[SerializeField]
		private bool _forceCommand;
		[SerializeField]
		private CharacterCommand _commandToForce;
		[SerializeField]
		private float _forceCommandInterval;
		[SerializeField]
		private Vector3 _directionToForce;
		private CharacterContext _characterContext;

		private float _timer;
		private Vector3 _startPos;
		private CharacterCommand _selectedCommand;

		public void Initialize(CharacterContext context)
		{
			_characterContext = context;
			_startPos = transform.position.AddRandom(-0.1f, 0.1f);
		}

		public void Think(float deltaTime)
		{
			_timer -= deltaTime;

			if(_forceCommand)
			{
				if(_timer <= 0)
				{
					_characterContext.InputData.Command = _commandToForce;
					_timer = _forceCommandInterval;
					return;
				}
			}

			if(_timer <= 0)
			{
				switch(Random.value)
				{
					case < 0.1f:
						_selectedCommand = _movementCommand;
						_characterContext.InputData.DirectionWorld = new Vector3().AddRandom(-1, 1).SetY(0).normalized;
						_timer = Random.Range(3, 5f);
						break;
					case < 0.6f:
						_selectedCommand = _movementCommand;
						_characterContext.InputData.DirectionWorld = (_startPos - _characterContext.Transform.Position).normalized;
						_timer = Random.Range(3, 5f);
						break;
					default:
						_selectedCommand = CharacterCommand.None;
						_timer = Random.Range(1, 2f);
						break;
				}
			}

			_characterContext.InputData.Command = _selectedCommand;
		}

		public string GetDebugSting()
		{
			return "Dummy Brain";
		}
	}
}
