using dream_lib.src.camera;
using dream_lib.src.reactive;
using game.input;
using UnityEngine;

namespace game.gameplay_core.characters.player
{
	public class PlayerInputController : ICharacterBrain
	{
		private CharacterInputData _inputData;
		private Transform _characterTransform;
		private readonly ReactiveProperty<Camera> _mainCamera;
		private CharacterContext _characterContext;

		public PlayerInputController(ReactiveProperty<Camera> locationContextMainCamera)
		{
			_mainCamera = locationContextMainCamera;
		}

		public void Initialize(CharacterContext context)
		{
			_characterContext = context;
			_inputData = context.InputData;
			_characterTransform = context.Transform;
		}

		public void Think(float deltaTime)
		{
			var directionInputScreenSpace = Vector2.zero;

			directionInputScreenSpace.x += InputAdapter.GetAxisRaw(InputAxesNames.Horizontal);
			directionInputScreenSpace.y += InputAdapter.GetAxisRaw(InputAxesNames.Vertical);
			directionInputScreenSpace = directionInputScreenSpace.normalized;

			if(directionInputScreenSpace.sqrMagnitude > 0)
			{
				_inputData.DirectionWorld = _mainCamera.Value.ProjectScreenVectorToWorldPlaneWithSkew(directionInputScreenSpace);
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
