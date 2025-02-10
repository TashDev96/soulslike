using Animancer;
using dream_lib.src.extensions;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.config;
using game.gameplay_core.characters.logic;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public class RollState : CharacterAnimationStateBase
	{
		private Vector3 _characterDirectionTarget;
		private RollConfig _config;
		private Vector3 _rollDirectionWorld;

		public RollState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		public override void OnEnter()
		{
			base.OnEnter();
			IsComplete = false;

			_config = _context.Config.Roll;

			var animation = _config.ForwardAnimation;

			_rollDirectionWorld = _context.InputData.HasDirectionInput ? _context.InputData.DirectionWorld : -_context.Transform.Forward;
			_characterDirectionTarget = _rollDirectionWorld;

			if(_context.LockOnLogic.IsLockedOn && _context.InputData.HasDirectionInput)
			{
				var localDir = _context.Transform.InverseTransformDirection(_context.InputData.DirectionWorld); //maybe should save this at OnEnter
				var directionType = localDir.GetDirectionHor();

				switch(directionType)
				{
					case Direction.Forward:
						animation = _config.ForwardAnimation;
						_characterDirectionTarget = _rollDirectionWorld;
						break;
					case Direction.Right:
						animation = _config.RightAnimation;
						_characterDirectionTarget = Quaternion.AngleAxis(90, Vector3.up) * -_rollDirectionWorld;
						break;
					case Direction.Back:
						animation = _config.BackwardAnimation;
						_characterDirectionTarget = -_rollDirectionWorld;
						break;
					case Direction.Left:
						animation = _config.LeftAnimation;
						_characterDirectionTarget = Quaternion.AngleAxis(90, Vector3.up) * _rollDirectionWorld;
						break;
				}
			}

			Time = 0;
			Duration = animation.length;

			ResetForwardMovement();

			_context.Animator.Play(animation, 0.1f, FadeMode.FromStart);
		}

		public bool CanSwitchToAttack => _context.Config.Roll.ExitToRollAttackTiming.Contains(NormalizedTime);

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			return base.CheckIsReadyToChangeState(nextCommand);
		}

		public override void Update(float deltaTime)
		{
			Time += deltaTime;

			if(_context.LockOnLogic.IsLockedOn)
			{
				_context.LockOnLogic.DisableRotationForThisFrame = true;
			}

			_context.MovementLogic.RotateCharacter(_characterDirectionTarget, deltaTime);
			UpdateForwardMovement(_config.ForwardMovement.Evaluate(Time), _rollDirectionWorld, deltaTime);

			if(TimeLeft <= 0f)
			{
				IsComplete = true;
			}

			if(_context.Config.Roll.RollInvulnerabilityTiming.Contains(NormalizedTime))
			{
				_context.InvulnerabilityLogic.SetInvulnerability(InvulnerabilityReason.Roll, true);
			}
			else
			{
				_context.InvulnerabilityLogic.SetInvulnerability(InvulnerabilityReason.Roll, false);
			}
		}
	}
}