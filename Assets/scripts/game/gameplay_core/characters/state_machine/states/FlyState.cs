using game.gameplay_core.characters.commands;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public class FlyState : CharacterStateBase
	{
		private float _flyingSpeed;
		private float _currentPitch;
		private float _currentYaw;
		private float _currentRoll;
		private float _lastFlapTime;
		private readonly Transform _transform;

		public FlyState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
			_transform = context.SelfLink.transform;

			_currentYaw = _transform.eulerAngles.y;
			_currentPitch = _transform.eulerAngles.x;
			if(_currentPitch > 180)
			{
				_currentPitch -= 360;
			}
			_currentRoll = 0;
			_flyingSpeed = _context.MovementLogic.LastUpdateVelocity.magnitude;
			_lastFlapTime = -100f;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			_context.MovementLogic.SetFlyingMode(true);
			_context.BodyView.SetFlyingMode(true);
		}

		public override void OnExit()
		{
			base.OnExit();
			_context.MovementLogic.SetFlyingMode(false);
			_context.BodyView.SetFlyingMode(false);
		}

		public override void Update(float deltaTime)
		{
			var config = _context.Config.Flying;
			if(config == null)
			{
				return;
			}
			var input = _context.InputData.InputScreenSpace;
			var flap = _context.InputData.Command == CharacterCommand.FlapWings;

			// Yaw
			_currentYaw += input.x * config.YawSpeed * deltaTime;

			// Pitch
			var targetPitch = _currentPitch - input.y * config.PitchSpeed * deltaTime;

			// Pitch correction if no energy to fly up
			if(targetPitch < 0 && _context.CharacterStats.Stamina.Value <= 0)
			{
				targetPitch = Mathf.MoveTowards(targetPitch, 0, config.PitchSpeed * deltaTime);
			}

			_currentPitch = Mathf.Clamp(targetPitch, -85f, 85f);

			// Roll
			var targetRoll = -input.x * config.MaxRollAngle;
			_currentRoll = Mathf.Lerp(_currentRoll, targetRoll, deltaTime * config.RollSpeed);

			// Apply rotation
			_transform.rotation = Quaternion.Euler(_currentPitch, _currentYaw, _currentRoll);

			// Friction
			_flyingSpeed = Mathf.MoveTowards(_flyingSpeed, config.BaseSpeed, config.Friction * deltaTime);

			// Speed gain/loss by altitude
			var pitchRad = _currentPitch * Mathf.Deg2Rad;
			_flyingSpeed += Mathf.Sin(pitchRad) * config.AltitudeSpeedGain * deltaTime;

			// Flaps
			if(flap && Time.time > _lastFlapTime + config.FlapCooldown)
			{
				if(_context.CharacterStats.Stamina.Value >= config.FlapStaminaCost)
				{
					_context.StaminaLogic.SpendStamina(config.FlapStaminaCost);
					_flyingSpeed += config.FlapForce;
					_lastFlapTime = Time.time;
				}
			}

			_flyingSpeed = Mathf.Clamp(_flyingSpeed, 0, config.MaxSpeed);

			var velocity = _context.Transform.Forward * _flyingSpeed;

			_context.CharacterCollider.Move(velocity * deltaTime, false);
		}
	}
}
