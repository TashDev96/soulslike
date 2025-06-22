using Animancer;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.state_machine.states.attack
{
	public class DeflectedAttackState:CharacterAnimationStateBase
	{
		private readonly AnimancerState _animationState;
		private readonly AttackConfig _attackConfig;

		public DeflectedAttackState(CharacterContext context, AnimancerState animationState, AttackConfig attackConfig) : base(context)
		{
			_animationState = animationState;
			_attackConfig = attackConfig;
		}

		public override void OnEnter()
		{
			_animationState.Speed = -1;
			_context.StaminaLogic.SpendStamina(_attackConfig.StaminaCost);
			base.OnEnter();
		}
		

		public override void Update(float deltaTime)
		{
			_animationState.Speed += deltaTime*2f;
			if(_animationState.Speed >= -0.1f)
			{
				IsComplete = true;
			}
		}

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }
	}
}
