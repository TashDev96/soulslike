using System.Collections.Generic;
using Animancer;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.state_machine.states
{
	public class ParryState : CharacterAnimationStateBase
	{
		private WeaponView _parryWeapon;
		private AttackConfig _parryConfig;
		private readonly List<HitData> _hitsData = new();

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		public bool IsParrySuccessful { get; private set; }

		public bool IsInActiveFrames { get; private set; }

		public bool IsInRecoveryFrames { get; private set; }

		public override bool CanInterruptByStagger => !IsInRecoveryFrames;

		public ParryState(CharacterContext context) : base(context)
		{
			IsReadyToRememberNextCommand = true;
		}

		public override void OnEnter()
		{
			base.OnEnter();

			_parryWeapon = _context.LeftWeapon.HasValue ? _context.LeftWeapon.Value : _context.RightWeapon.Value;
			_parryConfig = _parryWeapon?.Config.Parry;

			if(_parryConfig != null && _parryWeapon.Config.CanParry)
			{
				Duration = _parryConfig.Duration;

				_hitsData.Clear();
				for(var i = 0; i < _parryConfig.HitConfigs.Count; i++)
				{
					_hitsData.Add(new HitData
					{
						Config = _parryConfig.HitConfigs[i]
					});
				}

				_context.Animator.Play(_parryConfig.Animation, 0.1f, FadeMode.FromStart);
				_context.StaminaLogic.SpendStamina(_parryConfig.StaminaCost);
			}
			else
			{
				IsComplete = true;
			}
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if(_parryConfig == null)
			{
				IsComplete = true;
				return;
			}

			var wasInActiveFrames = IsInActiveFrames;
			IsInActiveFrames = false;
			IsInRecoveryFrames = false;

			var hasActiveHit = false;
			var allHitsComplete = true;

			foreach(var hitData in _hitsData)
			{
				var hitTiming = hitData.Config.Timing;

				if(!hitData.IsStarted && NormalizedTime >= hitTiming.x)
				{
					hitData.IsStarted = true;
				}

				if(hitData.IsActive)
				{
					IsInActiveFrames = true;
					hasActiveHit = true;

					if(NormalizedTime >= hitTiming.y)
					{
						hitData.IsEnded = true;
					}
				}

				allHitsComplete &= hitData.IsEnded;
			}

			if(allHitsComplete && !hasActiveHit)
			{
				IsInRecoveryFrames = true;
			}

			if(wasInActiveFrames != IsInActiveFrames && _context.ParryReceiver != null)
			{
				_context.ParryReceiver.SetActive(IsInActiveFrames);
			}

			if(Time >= _parryConfig.Duration)
			{
				IsComplete = true;
			}
		}

		public void OnParryFailed()
		{
			IsParrySuccessful = false;

			if(_parryConfig != null)
			{
				IsComplete = true;
			}
		}

		public override void OnExit()
		{
			if(_context.ParryReceiver != null)
			{
				_context.ParryReceiver.gameObject.SetActive(false);
			}

			base.OnExit();
		}

		public override float GetEnterStaminaCost()
		{
			var parryWeapon = _context.LeftWeapon.HasValue ? _context.LeftWeapon.Value : _context.RightWeapon.Value;
			return parryWeapon?.Config.Parry?.StaminaCost ?? 15f;
		}

		public override string GetDebugString()
		{
			return $"Active: {IsInActiveFrames}, Recovery: {IsInRecoveryFrames}, Success: {IsParrySuccessful}";
		}
	}
}
