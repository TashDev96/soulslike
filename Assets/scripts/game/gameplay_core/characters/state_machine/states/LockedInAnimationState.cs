using Animancer;
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

		public LockedInAnimationState(CharacterContext context, AnimationClip animationClip, bool canInterruptByStagger = false) : base(context)
		{
			_animationClip = animationClip;
			_animationDuration = animationClip.length;
			_canInterruptByStagger = canInterruptByStagger;
			IsReadyToRememberNextCommand = false;
		}

		public override void OnEnter()
		{
			base.OnEnter();

			_animation = _context.Animator.Play(_animationClip, 0.1f, FadeMode.FromStart);
			Duration = _animationDuration;
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if(Time >= Duration)
			{
				IsComplete = true;
			}
		}

		public override string GetDebugString()
		{
			return $"Locked animation: {Time:F2}/{Duration:F2}  ({NormalizedTime}), Animation: {_animationClip?.name ?? "None"}";
		}
	}
}
