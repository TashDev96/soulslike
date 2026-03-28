using dream_lib.src.extensions;
using game.gameplay_core.characters.commands;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public class FlyState : CharacterStateBase
	{
		private Vector3 _flyingVelocity;
		private float _currentPitch;
		private float _currentYaw;
		private float _currentRoll;
		private float _lastFlapTime;
		private readonly Transform _transform;
		private Vector3 _localVelocity;

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
			_localVelocity = _transform.InverseTransformVector(_context.MovementLogic.LastUpdateVelocity);
			_lastFlapTime = -100f;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			_context.MovementLogic.SetFlyingMode(true, Vector3.zero);
			_context.BodyView.SetFlyingMode(true);
		}

		public override void OnExit()
		{
			base.OnExit();
			_context.MovementLogic.SetFlyingMode(false, _flyingVelocity);
			_context.BodyView.SetFlyingMode(false);
		}

		public override string GetDebugString()
		{
			return $"{_flyingVelocity.magnitude.RoundFormat()} {_currentPitch.RoundFormat()}";
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
			_currentYaw += input.x * config.YawSpeedByForwardSpeed.Evaluate(_localVelocity.z) * deltaTime;

			// Pitch
			var targetPitch = _currentPitch + input.y * config.PitchSpeed * deltaTime;

			// Pitch correction if no energy to fly up
			if(targetPitch < 0 && _context.CharacterStats.Stamina.Value <= 0)
			{
				targetPitch = Mathf.MoveTowards(targetPitch, 0, config.PitchSpeed * deltaTime);
			}

			// Stall prevention: force nose down if speed is too low
			var minPitch = config.MinPitchPerSpeed.Evaluate(_localVelocity.z);
			if(targetPitch < minPitch)
			{
				targetPitch = minPitch;
			}

			_currentPitch = Mathf.Clamp(targetPitch, -85f, 85f);

			// Roll
			var targetRoll = -input.x * config.MaxRollAngle;
			_currentRoll = Mathf.Lerp(_currentRoll, targetRoll, deltaTime * config.RollSpeed);

			// Apply rotation
			_transform.rotation = Quaternion.Euler(_currentPitch, _currentYaw, _currentRoll);

			// Friction
			var frictionForce = config.Friction;

			frictionForce.x *= _localVelocity.x * _localVelocity.x;
			frictionForce.y *= _localVelocity.y * _localVelocity.y;
			frictionForce.z *= _localVelocity.z * _localVelocity.z;
			_localVelocity = _localVelocity.MoveTowardsSeparate(Vector3.zero, frictionForce * deltaTime);

			// Speed gain/loss by altitude
			// var pitchRad = _currentPitch * Mathf.Deg2Rad;
			// _flyingSpeed += Mathf.Sin(pitchRad) * config.AltitudeSpeedGain * deltaTime;

			//Gravity
			var xCache = _localVelocity.x;
			_localVelocity += _transform.InverseTransformVector(Physics.gravity) * deltaTime;
			_localVelocity.y += _localVelocity.z * _localVelocity.z * config.LiftForceCoeff * deltaTime;
			_localVelocity.x = xCache; //disable sliding to the side with gravity TODO: try enable a little for realism

			// Flaps
			if(flap && Time.time > _lastFlapTime + config.FlapCooldown)
			{
				if(_context.CharacterStats.Stamina.Value >= config.FlapStaminaCost)
				{
					_context.StaminaLogic.SpendStamina(config.FlapStaminaCost);
					var flapDirection = Vector3.forward;
					var inputPitchUp = Mathf.Clamp01(-input.y);
					if(inputPitchUp>0)
					{
						flapDirection = Vector3.RotateTowards(flapDirection, Vector3.up, inputPitchUp * 70 * Mathf.Deg2Rad, 0);
					}
					_localVelocity += flapDirection * config.FlapForce;
					_lastFlapTime = Time.time;
				}
			}

			_flyingVelocity = _transform.TransformVector(_localVelocity);
			Debug.DrawRay(_transform.position, _flyingVelocity * 2f, Color.red);

			_context.CharacterCollider.MoveFlying(_flyingVelocity * deltaTime, out var collisionFlags);
			if(collisionFlags.HasFlag(CollisionFlags.Below) && _flyingVelocity.y < 0)
			{
				_flyingVelocity.y = 0;
			}
			if(collisionFlags.HasFlag(CollisionFlags.Sides))
			{
				//_flyingVelocity.x = 0;
				//_flyingVelocity.z = 0;
			}
		}
	}
}
