using game.gameplay_core.characters.commands;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public class WalkState : CharacterAnimationStateBase
	{
		private const float NoiseDistance = 3f;
		private const float NoiseEmitPeriod = 0.33f;
		private float _noiseTimer;

		public WalkState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override float Time { get; protected set; }

		protected override float Duration { get; set; }

		public override void OnEnter()
		{
			base.OnEnter();
			AnimationConfig = _context.Config.Locomotion.WalkAnimation;
			Duration = AnimationConfig.Duration;
			_noiseTimer = 0;
			_context.Animator.Play(AnimationConfig.Clip, 0.3f);
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
			base.Update(deltaTime);
			var inputWorld = _context.InputData.DirectionWorld.normalized;
			var acceleration = _context.Config.Locomotion.WalkAccelerationCurve.Evaluate(Time);
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
