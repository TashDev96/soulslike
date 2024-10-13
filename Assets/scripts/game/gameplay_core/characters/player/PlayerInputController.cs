using dream_lib.src.camera;
using dream_lib.src.reactive;
using game.gameplay_core.characters.ai;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.runtime_data;
using game.input;
using UnityEngine;

namespace game.gameplay_core.characters.player
{
	public class PlayerInputController : ICharacterBrain
	{
		private CharacterInputData _inputData;
		private readonly ReactiveProperty<Camera> _mainCamera;
		private CharacterContext _characterContext;
		private Vector2 _directionInputScreenSpace;

		public PlayerInputController(ReactiveProperty<Camera> locationContextMainCamera)
		{
			_mainCamera = locationContextMainCamera;
		}

		public void Initialize(CharacterContext context)
		{
			_characterContext = context;
			_inputData = context.InputData;
		}

		public void Think(float deltaTime)
		{
			_directionInputScreenSpace = Vector2.zero;

			_directionInputScreenSpace.x += InputAdapter.GetAxisRaw(InputAxesNames.Horizontal);
			_directionInputScreenSpace.y += InputAdapter.GetAxisRaw(InputAxesNames.Vertical);
			_directionInputScreenSpace = _directionInputScreenSpace.normalized;

			if(_directionInputScreenSpace.sqrMagnitude > 0)
			{
				_inputData.DirectionWorld = _mainCamera.Value.ProjectScreenVectorToWorldPlaneWithSkew(_directionInputScreenSpace);
			}

			_inputData.Command = CalculateCommand(_directionInputScreenSpace);
			_inputData.HoldBlock = InputAdapter.GetButton(InputAxesNames.Block);

			if(InputAdapter.GetButtonDown(InputAxesNames.LockOn))
			{
				_characterContext.LockOnLogic.HandleLockOnTriggerInput();
			}
		}

		public string GetDebugSting()
		{
			return $"Player Input {_directionInputScreenSpace}";
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
