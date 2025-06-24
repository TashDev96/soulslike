using game.gameplay_core.characters.commands;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public class WalkBlockState : CharacterStateBase
	{
		private const string StaminaRegenLockKey = nameof(WalkBlockState);
		private bool _isBlocking;
		private float _time;
		private WeaponView _weapon;

		public WalkBlockState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			IsComplete = false;
			_isBlocking = true;
			_time = 0;
			
			_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenLockKey, true);
			
			_weapon = _context.LeftWeapon.HasValue ? _context.LeftWeapon.Value : _context.RightWeapon.Value;
			
			if(_weapon != null)
			{
				_context.Animator.Play(_weapon.Config.BlockStayAnimation, 0.2f);
				_weapon.SetBlockColliderActive(true);
			}
		}

		public override void OnExit()
		{
			_isBlocking = false;
			_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenLockKey, false);
			
			if(_weapon != null)
			{
				_weapon.SetBlockColliderActive(false);
			}
			
			base.OnExit();
		}

		public override bool TryContinueWithCommand(CharacterCommand nextCommand)
		{
			switch(nextCommand)
			{
				case CharacterCommand.WalkBlock:
					IsComplete = false;
					return true;
				case CharacterCommand.StayBlock:
					return false;
				default:
					return false;
			}
		}

		public override void Update(float deltaTime)
		{
			_time += deltaTime;
			
			if(_isBlocking)
			{
				if(_context.CharacterStats.Stamina.Value <= 0)
				{
					IsComplete = true;
					return;
				}
			}

			var inputWorld = _context.InputData.DirectionWorld.normalized;

			if(!_context.LockOnLogic.LockOnTarget.HasValue)
			{
				_context.MovementLogic.RotateCharacter(inputWorld, deltaTime);
			}

			var directionMultiplier = Mathf.Clamp01(Vector3.Dot(_context.Transform.Forward, inputWorld));
			if(_context.LockOnLogic.LockOnTarget.HasValue)
			{
				directionMultiplier = 1;
			}

			var acceleration = _context.Config.Locomotion.WalkAccelerationCurve.Evaluate(_time);
			var velocity = inputWorld * (directionMultiplier * _context.WalkSpeed.Value * 0.5f * acceleration);

			_context.MovementLogic.ApplyLocomotion(velocity * deltaTime, deltaTime);

			IsComplete = true;
		}

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			if(nextCommand == CharacterCommand.StayBlock)
			{
				return true;
			}
			return base.CheckIsReadyToChangeState(nextCommand);
		}

		public override bool CanInterruptByStagger => false;

		public override float GetEnterStaminaCost()
		{
			return 1f;
		}
	}
} 