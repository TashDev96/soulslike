using Animancer;
using game.enums;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.stats.config;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public class FallState : CharacterAnimationStateBase
	{
		private const float PerfectLandingWindowSeconds = 0.7f;
		public bool HasValidRollInput;

		private bool _isAttacking;

		private float _fallDuration;
		private bool _hasPlayedFallAnimation;
		private float _initialFallY;
		private float _lastRollInputTime = -10f;
		private HitData _hitData;

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
			_isAttacking = false;
			_hasPlayedFallAnimation = false;

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

			if(_hasPlayedFallAnimation && !_isAttacking && _context.InputData.Command == CharacterCommand.RegularAttack)
			{
				_isAttacking = true;

				var weaponConfig = _context.Logic.InventoryLogic.RightWeapon.Config;
				_hitData = new HitData
				{
					BlockDamageMultiplier = 1,
					Config = weaponConfig.FallHitConfig
				};
				_context.Views.Animator.Play(weaponConfig.FallAttackAnimation, 0.2f, FadeMode.FromStart);
				_context.Views.EquippedWeaponViews[EquipmentSlotType.RightHand].StorePreviousTransform();
			}

			if(_isAttacking)
			{
				if(_context.Views.BodyAttackView.CheckPlungeAttackLanding(out var target, out var pivot))
				{
					_isAttacking = false;
					_context.Events.TriggerPlungeAttack.Execute(target, pivot);
					return;
				}

				var weaponView = _context.Views.EquippedWeaponViews[EquipmentSlotType.RightHand];
				var interpolatedCaster = weaponView.StartInterpolatedCast(WeaponColliderType.Attack, _hitData.Config.InvolvedColliders);
				while(interpolatedCaster.MoveNext())
				{
					foreach(var caster in interpolatedCaster.GetActiveColliders())
					{
						var damage = _context.CharacterStats.GetValue(StatKey.AttackDamage) * _context.Logic.FallDamageLogic.GetBonusDamageMultiplierForPlunge();
						AttackHelpers.CastAttack(damage, _hitData, caster, _context, 999, true);
					}
				}
				weaponView.StorePreviousTransform();

				return;
			}

			if(_context.InputData.Command == CharacterCommand.Roll)
			{
				var currentTime = UnityEngine.Time.realtimeSinceStartup;

				_lastRollInputTime = currentTime;

				if(_context.Logic.FallDamageLogic != null && _context.IsFalling.Value)
				{
					_context.Logic.FallDamageLogic.TryActivateFallDamageProtection();
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
			if(_isAttacking)
			{
				var anim = _context.Logic.InventoryLogic.RightWeapon.Config.FallAttackLandingAnim;
				_context.SelfLink.CharacterStateMachine.LockInAnimation(anim);
				return;
			}

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

			if(timeSinceRollInput <= PerfectLandingWindowSeconds)
			{
				HasValidRollInput = true;

				if(_context.Logic.FallDamageLogic != null)
				{
					var success = _context.Logic.FallDamageLogic.TryActivateFallDamageProtection();
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
				_context.Views.Animator.Play(_context.Config.FallAnimation, 0.2f, FadeMode.FromStart);
			}
			else
			{
				_context.Views.Animator.Play(_context.Config.IdleAnimation, 0.2f, FadeMode.FromStart);
			}
		}
	}
}
