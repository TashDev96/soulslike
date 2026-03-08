using game.gameplay_core.characters.commands;

namespace game.gameplay_core.characters.state_machine.states
{
	public class WalkState : CharacterStateBase
	{
		private const float NoiseDistance = 3f;
		private const float NoiseEmitPeriod = 0.33f;
		private float _time;
		private float _noiseTimer;

		public WalkState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			_time = 0;
			_noiseTimer = 0;
			_context.Animator.Play(_context.Config.WalkAnimation, 0.3f);
		}

		public override bool TryContinueWithCommand(CharacterCommand nextCommand)
		{
			switch(nextCommand)
			{
				case CharacterCommand.Walk:
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
			var speed = _context.WalkSpeed.Value * acceleration;

			_context.MovementLogic.ApplyInputMovement(inputWorld, speed, deltaTime);

			IsComplete = true;

			_noiseTimer += deltaTime;
			if(_noiseTimer > NoiseEmitPeriod)
			{
				_noiseTimer = 0;
				EmitNoise(NoiseDistance);
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			EmitNoise(NoiseDistance);
		}
	}
}
