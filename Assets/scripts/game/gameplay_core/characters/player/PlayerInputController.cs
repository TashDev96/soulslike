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
		private float _rollDashHoldDuration;
		private RollDashInputState _rollInputState;
		private CharacterCommand _nextFrameForcedCommand;

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

			if(InputAdapter.GetButton(InputAxesNames.RollDash))
			{
				_rollDashHoldDuration += deltaTime;
			}

			_inputData.Command = CalculateCommand(_directionInputScreenSpace);
			_inputData.HoldBlock = InputAdapter.GetButton(InputAxesNames.Block);

			if(InputAdapter.GetButtonDown(InputAxesNames.LockOn))
			{
				_characterContext.LockOnLogic.HandleLockOnTriggerInput();
			}

			if(!InputAdapter.GetButton(InputAxesNames.RollDash))
			{
				_rollDashHoldDuration = 0;
			}
		}

		public string GetDebugSting()
		{
			return $"Player Input {_directionInputScreenSpace}";
		}

		private CharacterCommand CalculateCommand(Vector3 directionInputLocalSpace)
		{
			var hasDirectionInput = directionInputLocalSpace.sqrMagnitude > 0;
			var hasRunInput = _rollDashHoldDuration > .33f;
			var attackInput = GetAttackInput();

			if(_nextFrameForcedCommand != CharacterCommand.None)
			{
				var result = _nextFrameForcedCommand;
				_nextFrameForcedCommand = CharacterCommand.None;
				return result;
			}

			if(InputAdapter.GetButtonDown(InputAxesNames.RollDash))
			{
				_rollInputState = RollDashInputState.HasInput;
			}

			if(InputAdapter.GetButtonUp(InputAxesNames.RollDash))
			{
				_rollInputState = _rollInputState == RollDashInputState.Triggered ? RollDashInputState.None : RollDashInputState.Released;
			}

			var forceRoll = attackInput != CharacterCommand.None && _rollInputState == RollDashInputState.HasInput;

			if((forceRoll || _rollInputState == RollDashInputState.Released) && !hasRunInput && hasDirectionInput)
			{
				_nextFrameForcedCommand = attackInput;
				_rollInputState = forceRoll ? RollDashInputState.Triggered : RollDashInputState.None;
				return CharacterCommand.Roll;
			}

			if(_rollInputState == RollDashInputState.Released)
			{
				_rollInputState = RollDashInputState.None;
			}

			if(attackInput != CharacterCommand.None)
			{
				return attackInput;
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
				return hasRunInput ? CharacterCommand.Run : CharacterCommand.Walk;
			}

			return CharacterCommand.None;
		}

		private CharacterCommand GetAttackInput()
		{
			if(InputAdapter.GetButtonDown(InputAxesNames.StrongAttack))
			{
				return CharacterCommand.StrongAttack;
			}
			if(InputAdapter.GetButtonDown(InputAxesNames.Attack))
			{
				return CharacterCommand.RegularAttack;
			}
			return CharacterCommand.None;
		}

		private enum RollDashInputState
		{
			None,
			HasInput,
			Released,
			Triggered
		}
	}
}
