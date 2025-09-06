using Animancer;
using game.gameplay_core.characters.commands;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public class FallState : CharacterAnimationStateBase
	{
		private const float LANDING_WINDOW_DURATION = 1.0f;
		public bool HasValidRollInput;

		private float _fallDuration;
		private bool _hasPlayedFallAnimation;
		private float _initialFallY;
		private float _lastRollInputTime = -10f;

		public override float Time { get; protected set; }
		protected override float Duration { get; set; } = float.MaxValue;

		public bool ShouldRollOnLanding => HasValidRollInput;

		public FallState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			_hasPlayedFallAnimation = false;
			_fallDuration = 0f;
			_initialFallY = _context.Transform.Position.y;
			HasValidRollInput = false;
			_lastRollInputTime = -10f;

			PlayFallingAnimation();

			_context.IsFalling.OnChangedFromTo += HandleFallingChanged;
		}

		public override void OnExit()
		{
			base.OnExit();

			_context.IsFalling.OnChangedFromTo -= HandleFallingChanged;
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			_fallDuration += deltaTime;

			var currentHeight = _context.Transform.Position.y;
			var fallDistance = _initialFallY - currentHeight;

			if(!_hasPlayedFallAnimation && _fallDuration > 0.5f && fallDistance > 1.0f)
			{
				PlayFallingAnimation();
				_hasPlayedFallAnimation = true;
			}

			if(_context.InputData.Command == CharacterCommand.Roll)
			{
				var currentTime = UnityEngine.Time.realtimeSinceStartup;

				_lastRollInputTime = currentTime;

				if(_context.FallDamageLogic != null && _context.IsFalling.Value)
				{
					_context.FallDamageLogic.TryActivateFallDamageProtection();
				}
			}
		}

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			return IsComplete;
		}

		public override bool TryContinueWithCommand(CharacterCommand command)
		{
			if(command == CharacterCommand.Roll)
			{
				return true;
			}

			return base.TryContinueWithCommand(command);
		}

		private void HandleFallingChanged(bool wasFalling, bool isFalling)
		{
			if(!isFalling && wasFalling)
			{
				CheckRollOnLanding();
				IsComplete = true;
			}
		}

		private void CheckRollOnLanding()
		{
			var currentTime = UnityEngine.Time.realtimeSinceStartup;
			var timeSinceRollInput = currentTime - _lastRollInputTime;

			if(timeSinceRollInput <= LANDING_WINDOW_DURATION)
			{
				HasValidRollInput = true;

				if(_context.FallDamageLogic != null)
				{
					var success = _context.FallDamageLogic.TryActivateFallDamageProtection();
					if(success)
					{
						Debug.Log("Perfectly timed roll will prevent fall damage!");
					}
				}
			}
		}

		private void PlayFallingAnimation()
		{
			if(_context.Config.FallAnimation != null)
			{
				_context.Animator.Play(_context.Config.FallAnimation, 0.2f, FadeMode.FromStart);
			}
			else
			{
				_context.Animator.Play(_context.Config.IdleAnimation, 0.2f, FadeMode.FromStart);
			}
		}
	}
}
