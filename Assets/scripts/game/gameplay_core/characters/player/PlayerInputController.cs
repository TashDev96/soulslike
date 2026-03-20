using System.Text;
using game.gameplay_core.camera;
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
		private readonly ICameraController _cameraController;

		private CharacterContext _characterContext;
		private Vector2 _directionInputScreenSpace;
		private float _rollDashHoldDuration;
		private RollDashInputState _rollInputState;
		private CharacterCommand _nextFrameForcedCommand;

		public PlayerInputController(ICameraController cameraController)
		{
			_cameraController = cameraController;
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

			_inputData.InputScreenSpace = _directionInputScreenSpace;

			if(_directionInputScreenSpace.sqrMagnitude > 0)
			{
				_inputData.DirectionWorld = _cameraController.ConvertScreenSpaceDirectionToWorld(_directionInputScreenSpace);
			}

			if(InputAdapter.GetButton(InputAxesNames.RollDash))
			{
				_rollDashHoldDuration += deltaTime;
			}

			_inputData.Command = CalculateCommand(_directionInputScreenSpace);
			_inputData.HoldBlock = InputAdapter.GetButton(InputAxesNames.Block);

			if(IsAttackCommand(_inputData.Command))
			{
				if(_cameraController.OverrideAttackDirectionOnClick(out var newDirectionWorld))
				{
					_inputData.DirectionWorld = newDirectionWorld;
				}
			}

			if(InputAdapter.GetButtonDown(InputAxesNames.LockOn))
			{
				_characterContext.LockOnLogic.HandleLockOnTriggerInput();
			}

			if(!InputAdapter.GetButton(InputAxesNames.RollDash))
			{
				_rollDashHoldDuration = 0;
			}
		}

		public void GetDebugString(StringBuilder sb)
		{
			sb.Append("Player Input ").Append(_directionInputScreenSpace);
		}

		public void Reset()
		{
		}

		private bool IsAttackCommand(CharacterCommand command)
		{
			return command == CharacterCommand.RegularAttack || command == CharacterCommand.StrongAttack;
		}

		private CharacterCommand CalculateCommand(Vector3 directionInputLocalSpace)
		{
			var hasDirectionInput = directionInputLocalSpace.sqrMagnitude > 0;
			var hasBlockInput = InputAdapter.GetButton(InputAxesNames.Block);
			var hasRunInput = _rollDashHoldDuration > .33f;
			var attackInput = GetAttackInput();
			var parryInput = GetParryInput();

			if(GameStaticContext.Instance.UiDomain.IsInventoryOpen)
			{
				return CharacterCommand.None;
			}

			if(_nextFrameForcedCommand != CharacterCommand.None)
			{
				var result = _nextFrameForcedCommand;
				_nextFrameForcedCommand = CharacterCommand.None;
				return result;
			}

			if(parryInput != CharacterCommand.None)
			{
				return parryInput;
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

			if(_characterContext.FlyingMode.Value)
			{
				if(InputAdapter.GetButton(InputAxesNames.Flap))
				{
					return CharacterCommand.FlapWings;
				}
			}

			if(InputAdapter.GetButtonDown(InputAxesNames.Transform))
			{
				return CharacterCommand.Transform;
			}

			if(hasDirectionInput)
			{
				if(hasRunInput)
				{
					return CharacterCommand.Run;
				}
				return hasBlockInput ? CharacterCommand.WalkBlock : CharacterCommand.Walk;
			}

			if(hasBlockInput)
			{
				return CharacterCommand.StayBlock;
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

		private CharacterCommand GetParryInput()
		{
			if(InputAdapter.GetButtonDown(InputAxesNames.Attack) && InputAdapter.GetButton(InputAxesNames.Block))
			{
				return CharacterCommand.Parry;
			}

			if(InputAdapter.GetButtonDown(InputAxesNames.Parry) && _characterContext.InventoryLogic.CheckHasParryWeapon())
			{
				return CharacterCommand.Parry;
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
