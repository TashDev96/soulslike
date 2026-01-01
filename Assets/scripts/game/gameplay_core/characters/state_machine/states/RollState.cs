using Animancer;
using dream_lib.src.extensions;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.config;
using game.gameplay_core.characters.logic;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	using game.gameplay_core.characters.config.animation;

	public class RollState : CharacterAnimationStateBase
	{
		private const string StaminaRegenLockKey = nameof(RollState);
		private Vector3 _characterDirectionTarget;
		private RollConfig _config;
		private Vector3 _rollDirectionWorld;

		private bool _staminaSpent;

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		public bool CanSwitchToAttack => _context.Config.Roll.AnimationConfig.HasFlag(AnimationFlagEvent.AnimationFlags.TimingExitToAttack, NormalizedTime);

		public RollState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			IsComplete = false;

			_config = _context.Config.Roll;
			_staminaSpent = false;

			_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenLockKey, true);

			_context.BodyAttackView.PrepareRollBodyAttack();

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

		public override void OnExit()
		{
			base.OnExit();
			_context.InvulnerabilityLogic.SetInvulnerability(InvulnerabilityReason.Roll, false);
		}

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			return base.CheckIsReadyToChangeState(nextCommand);
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

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

			var isInvulnerable = _config.AnimationConfig.HasFlag(AnimationFlagEvent.AnimationFlags.Invulnerability, NormalizedTime);

			if(isInvulnerable)
			{
				if(!_staminaSpent)
				{
					_context.StaminaLogic.SpendStamina(CalculateStaminaCost());
					_staminaSpent = true;
					_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenLockKey, false);
				}
				_context.InvulnerabilityLogic.SetInvulnerability(InvulnerabilityReason.Roll, true);
			}
			else
			{
				_context.InvulnerabilityLogic.SetInvulnerability(InvulnerabilityReason.Roll, false);
			}

			if(_config.AnimationConfig.HasFlag(AnimationFlagEvent.AnimationFlags.BodyAttack, NormalizedTime))
			{
				_context.BodyAttackView.CastRollAttack();
			}
		}

		public override float GetEnterStaminaCost()
		{
			return 1;
		}

		private float CalculateStaminaCost()
		{
			var staminaSpendAmount = _context.Config.Roll.BaseStaminaCost; //todo rpg calculate based on armor weight
			return staminaSpendAmount;
		}
	}
}
