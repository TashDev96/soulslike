using game.gameplay_core.characters.commands;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public class WalkBlockState : BlockStateBase
	{
		private const string StaminaRegenLockKey = nameof(WalkBlockState);
		private float _time;

		public WalkBlockState(CharacterContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			_time = 0;
			base.OnEnter();
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

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			if(nextCommand == CharacterCommand.StayBlock)
			{
				return true;
			}
			return base.CheckIsReadyToChangeState(nextCommand);
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			var slowDownWalk = _receiveHitAnimation != null;

			_time += deltaTime;

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
			if(slowDownWalk)
			{
				velocity *= 0.3f;
			}

			_context.MovementLogic.ApplyLocomotion(velocity * deltaTime, deltaTime);

			IsComplete = true;
		}

		protected override void PlayBlockAnimation()
		{
			_context.Animator.Play(BlockingWeapon.Config.BlockWalkAnimation);
		}
	}
}
