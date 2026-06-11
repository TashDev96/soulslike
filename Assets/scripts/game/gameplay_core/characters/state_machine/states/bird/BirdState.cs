using dream_lib.src.extensions;
using dream_lib.src.utils.drawers;
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
		private readonly Transform _transform;
		private Vector3 _localVelocity;

		private CharacterFlyingBodyView _view;

		private readonly AnimationConfigPlayer _player;

		private Vector3? _lockDirectionInputUntilChanged;
		private float _time;

		public BirdState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
			_transform = context.SelfLink.transform;

			_currentYaw = _transform.eulerAngles.y;
			if(_currentPitch > 180)
			{
				_currentPitch -= 360;
			}
			_currentRoll = 0;
			_time = 0;
			_player = new AnimationConfigPlayer(_context);
		}

		public override void OnEnter()
		{
			base.OnEnter();

			if(_context.InputData.HasDirectionInput)
			{
				_lockDirectionInputUntilChanged = _context.InputData.DirectionWorld;
			}
			else
			{
				_lockDirectionInputUntilChanged = null;
			}

			_context.IsBirdMode.Value = true;
			_context.Views.BodyView.SetBirdMode(true);

			_view = _context.Views.BodyView.FlyingBodyView;

			_currentYaw = _transform.eulerAngles.y;
			_localVelocity = _transform.InverseTransformVector(_context.Logic.MovementLogic.LastUpdateVelocity);
			if(_context.Logic.MovementLogic.IsGrounded)
			{
				_currentPitch = -5;
				_localVelocity.y += 0.2f;
				_transform.position += Vector3.up * 0.1f;
			}
			_context.Logic.MovementLogic.SetFlyingMode(true);

			if(_context.Logic.FlyingLogic.TryFlap())
			{
				_player.Play(_view.Animations.TakeOff);
				_player.NextAnimation = _view.Animations.Glide;
			}
			else
			{
				_player.Play(_view.Animations.Glide);
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			_context.IsBirdMode.Value = false;
			_context.Logic.MovementLogic.SetFlyingMode(false);
			_context.Views.BodyView.SetBirdMode(false);
			_transform.rotation = Quaternion.Euler(0, _currentYaw, 0);
		}

		public override string GetDebugString()
		{
			return $"{_flyingVelocity.magnitude.RoundFormat()} {_currentPitch.RoundFormat()}";
		}

		public override void Update(float deltaTime)
		{
			_player.Update(deltaTime);
			_time += deltaTime;

			var config = _context.Config.Flying;

			var steerInput = _context.InputData.InputScreenSpace;
			if(_lockDirectionInputUntilChanged.HasValue && !TryUnlockDirectionInput())
			{
				steerInput.x = 0;
				if(_time < 1f)
				{
					steerInput.y = 0;
				}
			}

			var flapInput = _context.InputData.Command == CharacterCommand.FlapWings;

			// Yaw
			_currentYaw += steerInput.x * config.YawSpeedByForwardSpeed.Evaluate(_localVelocity.z) * deltaTime;

			// Pitch
			var targetPitch = _currentPitch + steerInput.y * config.PitchSpeed * deltaTime;

			// Stall prevention: force nose down if speed is too low
			if(_time > 0.5f)
			{
				var minPitch = config.MinPitchPerSpeed.Evaluate(_localVelocity.z);
				if(targetPitch < minPitch)
				{
					targetPitch = Mathf.Lerp(targetPitch, minPitch, deltaTime * config.PitchSpeed);
				}
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

				if(liftDirection.z < 0)
				{
					var angle = Vector3.Angle(liftDirection, Vector3.up);
					liftDirection = Quaternion.Euler(-angle, 0, 0) * liftDirection;
				}

				_localVelocity += liftDirection.Scaled(flapEvent.LiftForceAxesMultipliers) * (deltaTime * flapEvent.GetLiftForce(_player.NormalizedAnimationTime));
			}

			// Flaps
			if(flapInput && !flapInProgress)
			{
				if(_context.Logic.FlyingLogic.TryFlap())
				{
					_player.Play(_view.Animations.Flap);
					_player.NextAnimation = _view.Animations.Glide;
				}
			}

			_flyingVelocity = _transform.TransformVector(_localVelocity);

			_context.CharacterCollider.ResetFlags();
			var prevPos = _transform.position;
			_context.Logic.MovementLogic.MoveFlying(_flyingVelocity * deltaTime);
			_flyingVelocity = (_transform.position - prevPos) / deltaTime;
			_localVelocity = _transform.InverseTransformVector(_flyingVelocity);
			_context.Logic.MovementLogic.MoveFlying(Vector3.down * 0.0001f);

			if(_context.CharacterCollider.IsGrounded)
			{
				DebugDrawUtils.DrawHandlesSphere(_transform.position, 1f, Color.magenta, 0.2f);
			}

			if(_context.CharacterCollider.IsGrounded && _flyingVelocity.y <= 0)
			{
				_flyingVelocity.y = 0;
				if(!flapInProgress)
				{
					IsComplete = true;
				}
			}
			if(_context.CharacterCollider.Flags.HasFlag(CollisionFlags.Sides))
			{
				HandleWallsCollision();
			}

			_context.Logic.MovementLogic.SetFallVelocity(_flyingVelocity);

			void HandleWallsCollision()
			{
				var wallHitNormal = _context.CharacterCollider.WallsNormal;

				Debug.DrawLine(_transform.position, _transform.position + wallHitNormal * 10, Color.red, 5);

				var horizontalVelocity = new Vector3(_flyingVelocity.x, 0, _flyingVelocity.z);
				var speed = horizontalVelocity.magnitude;

				if(speed > 0f)
				{
					var angle = Vector3.Angle(horizontalVelocity, -wallHitNormal);

					if(angle <= 30)
					{
						var dot = Vector3.Dot(horizontalVelocity, wallHitNormal);
						if(dot < 0)
						{
							_flyingVelocity = _transform.TransformVector(_localVelocity);
							_flyingVelocity.x -= wallHitNormal.x * dot;
							_flyingVelocity.z -= wallHitNormal.z * dot;
							_localVelocity = _transform.InverseTransformVector(_flyingVelocity);
						}
					}
				}
			}
		}

		private bool TryUnlockDirectionInput()
		{
			if(!_lockDirectionInputUntilChanged.HasValue)
			{
				return true;
			}
			var savedInput = _lockDirectionInputUntilChanged.Value;
			var lastInput = _context.InputData.DirectionWorld;

			const float magnitudeThreshold = 0.3f;
			const float angleThreshold = 20;

			var magnitudeDifference = Mathf.Abs(savedInput.magnitude - lastInput.magnitude);
			var isMagnitudeChanged = magnitudeDifference > magnitudeThreshold;

			var isDirectionChanged = false;

			if(savedInput.sqrMagnitude > 0.01f && lastInput.sqrMagnitude > 0.01f)
			{
				var angleDifference = Vector3.Angle(savedInput, lastInput);
				isDirectionChanged = angleDifference > angleThreshold;
			}

			if(isMagnitudeChanged || isDirectionChanged)
			{
				_lockDirectionInputUntilChanged = null;
				return true;
			}
			return false;
		}
	}
}
