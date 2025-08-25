using Animancer;
using game.gameplay_core.characters.commands;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public class LockedInAnimationState : CharacterAnimationStateBase
	{
		private AnimancerState _animation;
		private readonly AnimationClip _animationClip;
		private readonly float _animationDuration;
		private readonly bool _canInterruptByStagger;
		private DamageReceiver[] _damageReceivers;

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }
		public override bool CanInterruptByStagger => _canInterruptByStagger;

		public LockedInAnimationState(CharacterContext context, AnimationClip animationClip, float duration = 0f, bool canInterruptByStagger = false) : base(context)
		{
			_animationClip = animationClip;
			_animationDuration = duration > 0 ? duration : animationClip.length;
			_canInterruptByStagger = canInterruptByStagger;
			IsReadyToRememberNextCommand = false;
		}

		public override void OnEnter()
		{
			base.OnEnter();

			_damageReceivers = _context.SelfLink.GetComponentsInChildren<DamageReceiver>();
			foreach(var damageReceiver in _damageReceivers)
			{
				damageReceiver.enabled = false;
			}

			if(_animationClip != null)
			{
				_animation = _context.Animator.Play(_animationClip, 0.1f, FadeMode.FromStart);
				Duration = _animationDuration;
			}
			else
			{
				Duration = _animationDuration;
			}
		}

		public override void Update(float deltaTime)
		{
			Time += deltaTime;

			if(_animation != null && _animation.NormalizedTime >= 0.9999f)
			{
				IsComplete = true;
			}
			else if(Time >= Duration)
			{
				IsComplete = true;
			}
		}

		public override void OnExit()
		{
			if(_damageReceivers != null)
			{
				foreach(var damageReceiver in _damageReceivers)
				{
					if(damageReceiver != null)
					{
						damageReceiver.enabled = true;
					}
				}
			}

			base.OnExit();
		}

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			return false;
		}

		public override string GetDebugString()
		{
			return $"Locked animation: {Time:F2}/{Duration:F2}, Animation: {_animationClip?.name ?? "None"}";
		}
	}
}
