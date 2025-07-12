using Animancer;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.damage_system;
using game.gameplay_core.utils;

namespace game.gameplay_core.characters.state_machine.states.attack
{
	public class DeflectedAttackState : CharacterAnimationStateBase
	{
		private readonly AnimancerState _animationState;
		private readonly AttackConfig _attackConfig;

		private bool _reverseTriggered;
		private float _reverseTriggerTimer;
		private float _waitAfterHitTimer;

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		public DeflectedAttackState(CharacterContext context, AnimancerState animationState, AttackConfig attackConfig) : base(context)
		{
			_animationState = animationState;
			_attackConfig = attackConfig;
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			_reverseTriggered = false;
			_reverseTriggerTimer = 1.60f;
			_context.MaxDeltaTime.Value = CharacterConstants.MaxDeltaTimeAttacking;

			base.OnEnter();
		}

		public override void Update(float deltaTime)
		{
			if(_reverseTriggered)
			{
				if(_waitAfterHitTimer > 0)
				{
					_animationState.Speed = 0;
					_waitAfterHitTimer -= deltaTime;
					if(_waitAfterHitTimer <= 0)
					{
						_animationState.Speed = -1;
					}
					return;
				}

				_animationState.Speed += deltaTime * 2f;
				if(_animationState.Speed >= -0.1f)
				{
					IsComplete = true;
				}
			}
			else
			{
				_context.RightWeapon.Value.CastCollidersInterpolated(WeaponColliderType.PreciseContact, null, CastPreciseHit);

				_reverseTriggerTimer -= deltaTime;
				if(_reverseTriggerTimer <= 0)
				{
					TriggerReverse();
				}
			}
		}

		public override void OnExit()
		{
			_context.MaxDeltaTime.Value = CharacterConstants.MaxDeltaTimeNormal;
			base.OnExit();
		}

		private void CastPreciseHit(HitData _, CapsuleCaster caster)
		{
			if(_reverseTriggered)
			{
				return;
			}

			if(AttackHelpers.CastAttackObstacles(caster, true,true))
			{
				_waitAfterHitTimer = 1 / 120f;
				TriggerReverse();
			}
		}

		private void TriggerReverse()
		{
			if(_reverseTriggered)
			{
				return;
			}
			_reverseTriggered = true;
			_animationState.Speed = -1;
			_context.StaminaLogic.SpendStamina(_attackConfig.StaminaCost);
		}
	}
}
