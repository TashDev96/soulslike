using Animancer;
using game.gameplay_core.characters.commands;
using game.gameplay_core.inventory.item_configs;
using game.gameplay_core.inventory.items_logic;

namespace game.gameplay_core.characters.state_machine.states
{
	public class ConsumeState : CharacterAnimationStateBase
	{
		private readonly IConsumableItemLogic _logic;
		private readonly ItemAnimationConfig _animationConfig;
		private AnimancerState _animation;
		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		public ConsumeState(CharacterContext context, IConsumableItemLogic itemLogic) : base(context)
		{
			IsComplete = false;
			_logic = itemLogic;
			_animationConfig = _logic.AnimationConfig;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			_animation = _context.Animator.Play(_animationConfig.Animation, 0.1f, FadeMode.FromStart);
			Duration = _animationConfig.Animation.Length;
			if(_animationConfig.DisableRightHandWeapon && _context.RightWeapon.HasValue)
			{
				_context.RightWeapon.Value.gameObject.SetActive(false);
			}
			_logic.HandleAnimationBegin();
		}

		public override void OnExit()
		{
			base.OnExit();
			if(_animationConfig.DisableRightHandWeapon && _context.RightWeapon.HasValue)
			{
				_context.RightWeapon.Value.gameObject.SetActive(true);
			}
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			_logic.HandleAnimationProgress(NormalizedTime);

			if(NormalizedTime > 0 && !CheckTiming(_animationConfig.LockedStateTime))
			{
				IsComplete = true;
			}
		}

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			return !CheckTiming(_animationConfig.LockedStateTime);
		}
	}
}
