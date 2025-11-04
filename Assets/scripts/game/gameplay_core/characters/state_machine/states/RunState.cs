using game.gameplay_core.characters.commands;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public class RunState : CharacterStateBase
	{
		private const string StaminaRegenLockKey = "RunState";
		private float _time;

		public override float RequiredStaminaOffset => _context.CharacterStats.StaminaMax.Value * 0.2f;

		public RunState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			_time = 0;
			IsComplete = false;
			_context.Animator.Play(_context.Config.RunAnimation, 0.3f);
			_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenLockKey,true);
			_context.LockOnLogic.LockOnTarget.Value = null;
		}

		public override void OnExit()
		{
			_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenLockKey,false);
			base.OnExit();
		}

		public override bool TryContinueWithCommand(CharacterCommand nextCommand)
		{
			switch(nextCommand)
			{
				case CharacterCommand.Run:
					IsComplete = false;
					return true;
				default:
					return false;
			}
		}

		public override void Update(float deltaTime)
		{
			_time += deltaTime;
			var inputWorld = _context.InputData.DirectionWorld.normalized;
			var acceleration = _context.Config.Locomotion.WalkAccelerationCurve.Evaluate(_time);
			var speed = _context.RunSpeed.Value * acceleration;

			_context.MovementLogic.ApplyInputMovement(inputWorld, speed, deltaTime);
			
			const float staminaCostPerSecond = 2f;
			_context.StaminaLogic.SpendStamina(staminaCostPerSecond * deltaTime);

			IsComplete = true;
		}

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			if(nextCommand == CharacterCommand.RegularAttack)
			{
				return true;
			}
			return base.CheckIsReadyToChangeState(nextCommand);
		}
	}
}
