using Animancer;
using dream_lib.src.extensions;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.logic;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public class RollState : CharacterAnimationStateBase
	{
		private RollConfig _config;
		private Vector3 _rollDirectionWorld;
		private Vector3 _rollDirectionLocal;
		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		public RollState(CharacterContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			base.OnEnter();
			IsComplete = false;

			_config = _context.Config.Roll;

			var animation = _config.ForwardAnimation;

			if(_context.LockOnLogic.IsLockedOn && _context.InputData.HasDirectionInput)
			{
				var directionType = _context.InputData.DirectionLocal.GetDirectionHor();

				_rollDirectionLocal = directionType.ToVector();

				switch(directionType)
				{
					case Direction.Forward:
						_rollDirectionWorld = _context.Transform.Forward;
						animation = _config.ForwardAnimation;
						break;
					case Direction.Right:
						_rollDirectionWorld = _context.Transform.Right;
						animation = _config.RightAnimation;
						break;
					case Direction.Back:
						_rollDirectionWorld = -_context.Transform.Forward;
						animation = _config.BackwardAnimation;
						break;
					case Direction.Left:
						_rollDirectionWorld = -_context.Transform.Right;
						animation = _config.LeftAnimation;

						break;
				}
			}
			else
			{
				_rollDirectionWorld = _context.InputData.HasDirectionInput ? _context.InputData.DirectionWorld : -_context.Transform.Forward;
				_rollDirectionLocal = _context.InputData.DirectionLocal;
			}

			Time = 0;
			Duration = animation.length;

			ResetForwardMovement();

			_context.Animator.Play(animation, 0.1f, FadeMode.FromStart);
		}

		public override void OnExit()
		{
			base.OnExit();
		}

		public override void Update(float deltaTime)
		{
			Time += deltaTime;

			if(_context.LockOnLogic.IsLockedOn)
			{
				_context.LockOnLogic.DisableRotationForThisFrame = true;
			}
			else
			{
				_context.MovementLogic.RotateCharacter(_rollDirectionWorld, deltaTime);
			}

			IsReadyToRememberNextCommand = NormalizedTime > 0.3f;

			if(TimeLeft <= 0f)
			{
				IsComplete = true;
			}

			UpdateForwardMovement(_config.ForwardMovement.Evaluate(Time), _rollDirectionWorld);

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
