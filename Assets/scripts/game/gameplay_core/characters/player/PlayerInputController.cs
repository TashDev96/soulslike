using dream_lib.src.camera;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using game.input;
using UnityEngine;

namespace game.gameplay_core.characters.player
{
	public class PlayerInputController : ICharacterBrain
	{
		private readonly CharacterInputData _inputData;
		private readonly Transform _characterTransform;
		private readonly ReactiveProperty<Camera> _mainCamera;

		public PlayerInputController(CharacterInputData inputData, Transform characterTransform, ReactiveProperty<Camera> locationContextMainCamera)
		{
			_inputData = inputData;
			_characterTransform = characterTransform;
			_mainCamera = locationContextMainCamera;
		}

		public void Update(float deltaTime)
		{
			var directionInputScreenSpace = Vector2.zero;

			directionInputScreenSpace.x += InputAdapter.GetAxis(InputAxesNames.Horizontal);
			directionInputScreenSpace.y += InputAdapter.GetAxis(InputAxesNames.Vertical);
			directionInputScreenSpace = directionInputScreenSpace.normalized;

			if(directionInputScreenSpace.sqrMagnitude > 0)
			{
				_inputData.DirectionWorld = _mainCamera.Value.ProjectScreenVectorToWorldPlane(directionInputScreenSpace);
				_inputData.DirectionLocal = _characterTransform.InverseTransformDirection(_inputData.DirectionWorld);
			}

			_inputData.Command = CalculateCommand(directionInputScreenSpace);
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
