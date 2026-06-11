using System;
using dream_lib.src.extensions;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.config.animation;
using game.gameplay_core.characters.view.bird;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states.bird
{
	public class BirdState : CharacterStateBase
	{
		private Vector3 _flyingVelocity;
		private float _currentPitch;
		private float _currentYaw;
		private float _currentRoll;
		private float _lastFlapTime;
		private readonly Transform _transform;
		private Vector3 _localVelocity;

		private SubState _subState;
		private CharacterFlyingBodyView _view;

		private readonly AnimationConfigPlayer _player;

		public BirdState(CharacterContext context) : base(context)
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
			_localVelocity = _transform.InverseTransformVector(_context.Logic.MovementLogic.LastUpdateVelocity);
			_lastFlapTime = -100f;
			_player = new AnimationConfigPlayer(_context);
		}

		public override void OnEnter()
		{
			base.OnEnter();

			_context.Views.BodyView.SetFlyingMode(true);

			_view = _context.Views.BodyView.FlyingBodyView;

			if(_context.Logic.MovementLogic.IsGrounded)
			{
				_subState = SubState.SitOnTheGround;
				_player.Play(_view.Animations.Sit);
			}
			else
			{
				_subState = SubState.Fly;
				_player.Play(_view.Animations.Glide);
			}

			_context.Logic.MovementLogic.SetFlyingMode(true, Vector3.zero);
		}

		public override void OnExit()
		{
			base.OnExit();
			_context.Logic.MovementLogic.SetFlyingMode(false, _flyingVelocity);
			_context.Views.BodyView.SetFlyingMode(false);
		}

		public override string GetDebugString()
		{
			return $"{_flyingVelocity.magnitude.RoundFormat()} {_currentPitch.RoundFormat()}";
		}

		public override void Update(float deltaTime)
		{
			_player.Update(deltaTime);

			switch(_subState)
			{
				case SubState.SitOnTheGround:
					UpdateSitOnTheGround(deltaTime);

					break;
				case SubState.Fly:
					UpdateFlying(deltaTime);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void UpdateFlying(float deltaTime)
		{
			var config = _context.Config.Flying;

			var steerInput = _context.InputData.InputScreenSpace;
			var flapInput = _context.InputData.Command == CharacterCommand.FlapWings;

			// Yaw
			_currentYaw += steerInput.x * config.YawSpeedByForwardSpeed.Evaluate(_localVelocity.z) * deltaTime;

			// Pitch
			var targetPitch = _currentPitch + steerInput.y * config.PitchSpeed * deltaTime;

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
			var targetRoll = -steerInput.x * config.MaxRollAngle;
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

			var flapInProgress = _player.AnimationConfig != _view.Animations.Glide;

			if(_player.AnimationConfig.TryGetCustomEvent<AnimEventFlapWings>(_player.NormalizedAnimationTime, out var flapEvent))
			{
				var inputPitchUp = -flapEvent.PitchCurve.Evaluate(-steerInput.y);

				var liftDirection = Quaternion.Euler(inputPitchUp, 0, 0) * Vector3.forward;
				Debug.DrawLine(_transform.position, _transform.position + _transform.TransformVector(liftDirection * 10f), Color.violet, 2f, false);

				if(liftDirection.z < 0)
				{
					var angle = Vector3.Angle(liftDirection, Vector3.up);
					liftDirection = Quaternion.Euler(-angle, 0, 0) * liftDirection;
					Debug.DrawLine(_transform.position, _transform.position + _transform.TransformVector(liftDirection * 10f), Color.blue, 2f, false);
				}

				_localVelocity += liftDirection.Scaled(flapEvent.LiftForceAxesMultipliers) * (deltaTime * flapEvent.GetLiftForce(_player.NormalizedAnimationTime));
			}

			// Flaps
			if(flapInput && Time.time > _lastFlapTime + config.FlapCooldown)
			{
				_lastFlapTime = Time.time;

				if(flapInProgress)
				{
					//_player.NextAnimation = _view.Animations.Flap;
				}
				else if(_context.Logic.FlyingLogic.TryFlap())
				{
					_player.Play(_view.Animations.Flap);
					_player.NextAnimation = _view.Animations.Glide;
				}
			}

			_flyingVelocity = _transform.TransformVector(_localVelocity);
	
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

		private void UpdateSitOnTheGround(float deltaTime)
		{
			var input = _context.InputData.InputScreenSpace;
			var flap = _context.InputData.Command == CharacterCommand.FlapWings;

			if(flap)
			{
				_currentYaw = _transform.eulerAngles.y;
				_subState = SubState.Fly;
				_player.Play(_view.Animations.TakeOff);
				_player.NextAnimation = _view.Animations.Glide;
				return;
			}

			//walk
			if(_context.InputData.Command == CharacterCommand.Walk)
			{
				if(_player.AnimationConfig != _view.Animations.Walk)
				{
					_player.Play(_view.Animations.Walk);
				}

				_currentYaw += input.x * _context.Config.Flying.LandedYawSpeed * deltaTime;
				_currentPitch = 0;
				_currentRoll = 0;

				_context.Logic.MovementLogic.ApplyInputMovement(Quaternion.Euler(0, _currentYaw, 0) * Vector3.forward, 2f, deltaTime);
			}
			else
			{
				if(_player.AnimationConfig != _view.Animations.Sit)
				{
					_player.Play(_view.Animations.Sit);
				}
			}
		}

		private enum SubState
		{
			SitOnTheGround,
			Fly
		}
	}
}
