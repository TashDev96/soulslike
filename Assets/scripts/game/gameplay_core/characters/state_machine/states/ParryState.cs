using System.Collections.Generic;
using Animancer;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.damage_system;
using game.gameplay_core.inventory.items_logic;

namespace game.gameplay_core.characters.state_machine.states
{
	public class ParryState : CharacterAnimationStateBase
	{
		private WeaponView _parryWeaponView;
		private AttackConfig _parryConfig;
		private readonly List<HitData> _hitsData = new();
		private WeaponItemLogic _parryWeaponLogic;

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

			_context.InventoryLogic.TryGetParryWeapon(out _parryWeaponLogic, out var slot);
			_parryWeaponView = _context.EquippedWeaponViews[slot];
			_parryConfig = _parryWeaponView?.Config.Parry;

			if(_parryConfig != null && _parryWeaponView.Config.CanParry)
			{
				Duration = _parryConfig.Duration;

				_hitsData.Clear();
				foreach(var hitEvent in _parryConfig.AnimationConfig.GetHitEvents())
				{
					_hitsData.Add(new HitData
					{
						Config = hitEvent
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
				if(!hitData.IsStarted && NormalizedTime >= hitData.Config.StartTime)
				{
					hitData.IsStarted = true;
				}

				if(hitData.IsActive)
				{
					IsInActiveFrames = true;
					hasActiveHit = true;

					if(NormalizedTime >= hitData.Config.EndTime)
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
			return _parryWeaponLogic.Config.Parry.StaminaCost;
		}

		public override string GetDebugString()
		{
			return $"Active: {IsInActiveFrames}, Recovery: {IsInRecoveryFrames}, Success: {IsParrySuccessful}";
		}
	}
}
