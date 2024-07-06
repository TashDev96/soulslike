using game.input;
using UnityEngine;

namespace game.gameplay_core.characters.player
{
	public class PlayerInputController : ICharacterBrain
	{
		private readonly CharacterInputData _inputData;
		private readonly Transform _characterTransform;

		public PlayerInputController(CharacterInputData inputData, Transform characterTransform)
		{
			_inputData = inputData;
			_characterTransform = characterTransform;
		}

		public void Update(float deltaTime)
		{
			var directionInputLocalSpace = Vector3.zero;

			directionInputLocalSpace.x += InputAdapter.GetAxis(InputAxesNames.Horizontal);
			directionInputLocalSpace.z += InputAdapter.GetAxis(InputAxesNames.Vertical);
			directionInputLocalSpace = directionInputLocalSpace.normalized;

			if(directionInputLocalSpace.sqrMagnitude > 0)
			{
				_inputData.DirectionLocal = directionInputLocalSpace;
				_inputData.DirectionWorld = _characterTransform.TransformDirection(_inputData.DirectionLocal);
			}

			_inputData.Command = CalculateCommand(directionInputLocalSpace);
			_inputData.HoldBlock = InputAdapter.GetButton(InputAxesNames.Block);
		}

		private CharacterCommand CalculateCommand(Vector3 directionInputLocalSpace)
		{
			var hasDirectionInput = directionInputLocalSpace.sqrMagnitude > 0;

			if(InputAdapter.GetButtonDown(InputAxesNames.Roll) && hasDirectionInput)
			{
				return CharacterCommand.Roll;
			}
			if(InputAdapter.GetButtonDown(InputAxesNames.Attack))
			{
				return CharacterCommand.Attack;
			}
			if(InputAdapter.GetButtonDown(InputAxesNames.UseItem))
			{
				return CharacterCommand.UseItem;
			}
			if(InputAdapter.GetButtonDown(InputAxesNames.Interact))
			{
				return CharacterCommand.Interact;
			}

			if(hasDirectionInput)
			{
				return InputAdapter.GetButton(InputAxesNames.Run) ? CharacterCommand.Run : CharacterCommand.Walk;
			}

			return CharacterCommand.None;
		}
	}
}
