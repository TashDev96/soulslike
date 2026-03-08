using game.gameplay_core.characters.commands;

namespace game.gameplay_core.characters.state_machine.states
{
	public class RunState : CharacterStateBase
	{
		private const float NoiseDistance = 6f;
		private const float NoiseEmitPeriod = 0.23f;

		private const string StaminaRegenLockKey = "RunState";
		private float _time;
		private float _noiseTimer;

		public override float RequiredStaminaOffset => _context.CharacterStats.StaminaMax.Value * 0.2f;

		public RunState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			_time = 0;
			_noiseTimer = 0;
			IsComplete = false;
			_context.Animator.Play(_context.Config.RunAnimation, 0.3f);
			_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenLockKey, true);
			_context.LockOnLogic.LockOnTarget.Value = null;
		}

		public override void OnExit()
		{
			_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenLockKey, false);
			EmitNoise(NoiseDistance);
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

			_noiseTimer += deltaTime;
			if(_noiseTimer > NoiseEmitPeriod)
			{
				_noiseTimer = 0;
				EmitNoise(NoiseDistance);
			}
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
