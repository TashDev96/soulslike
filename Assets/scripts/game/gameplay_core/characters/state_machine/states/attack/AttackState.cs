using System;
using System.Collections.Generic;
using Animancer;
using game.enums;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.config.animation;
using game.gameplay_core.characters.extensions;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.damage_system;
using UnityEngine;
using Object = UnityEngine.Object;

namespace game.gameplay_core.characters.state_machine.states.attack
{
	public class AttackState : CharacterAnimationStateBase
	{
		private const string StaminaRegenDisableKey = "AttackState";

		private const int FramesToUnlockWalkAfterStateUnlocked = 5;
		private int _currentAttackIndex;
		private AttackType _attackType;
		private AttackConfig _currentAttackConfig;
		private readonly List<HitData> _hitsData = new();
		private int _comboCounter;
		private bool _staminaSpent;
		private bool _staminaRegenDisabled;
		private bool _projectileSpawned;

		private int _framesToUnlockWalk;
		private AttackStage _stage;
		private WeaponView _weaponView;
		public AnimancerState CurrentAttackAnimation { get; private set; }
		public AttackConfig CurrentAttackConfig => _currentAttackConfig;

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		public AttackState(CharacterContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			_comboCounter = 0;
			LaunchAttack();
			_weaponView.StorePreviousTransform();
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			var rotationDisabled = _currentAttackConfig.AnimationConfig.HasFlag(AnimationFlagEvent.AnimationFlags.RotationLocked, NormalizedTime);
			if(_context.InputData.HasDirectionInput && !rotationDisabled)
			{
				_context.MovementLogic.RotateCharacter(_context.InputData.DirectionWorld, _context.Config.Locomotion.HalfTurnDurationSecondsLockOn, deltaTime);
			}

			UpdateStaminaRegenLock();

			UpdateForwardMovement(_currentAttackConfig.ForwardMovement.Evaluate(Time), deltaTime);

			var hasActiveHit = false;
			var allHitsComplete = true;

			if(_currentAttackConfig.IsRangedAttack)
			{
				foreach(var hitData in _hitsData)
				{
					var hitTimingStart = hitData.Config.StartTime;

					if(!hitData.IsStarted && NormalizedTime >= hitTimingStart)
					{
						hitData.IsStarted = true;
						hitData.IsEnded = true;
						if(!_staminaSpent)
						{
							_context.StaminaLogic.SpendStamina(_currentAttackConfig.StaminaCost);
							_staminaSpent = true;
						}

						if(!_projectileSpawned)
						{
							_projectileSpawned = true;
							SpawnProjectile(hitData.Config);
						}
					}

					hasActiveHit |= hitData.IsActive;
					allHitsComplete &= hitData.IsEnded;
				}
			}
			else
			{
				foreach(var hitData in _hitsData)
				{
					var hitTimingStart = hitData.Config.StartTime;

					if(!hitData.IsStarted && NormalizedTime >= hitTimingStart)
					{
						hitData.IsStarted = true;
						if(!_staminaSpent)
						{
							_context.StaminaLogic.SpendStamina(_currentAttackConfig.StaminaCost);
						}
					}

					if(hitData.IsActive)
					{
						var interpolatedCaster = _weaponView.StartInterpolatedCast(WeaponColliderType.Attack, hitData.Config.InvolvedColliders);
						while(interpolatedCaster.MoveNext())
						{
							foreach(var caster in interpolatedCaster.GetActiveColliders())
							{
								var deflectionRating = _weaponView.Config.AttackDeflectionRating + _currentAttackConfig.AttackDeflectionRatingBonus;
								AttackHelpers.CastAttack(_currentAttackConfig.BaseDamage, hitData, caster, _context, deflectionRating, true);
							}
						}

						if(NormalizedTime >= hitData.Config.EndTime)
						{
							hitData.IsEnded = true;
						}
					}

					hasActiveHit |= hitData.IsActive;
					allHitsComplete &= hitData.IsEnded;
				}
			}

			if(hasActiveHit)
			{
				_stage = AttackStage.Impact;
			}
			if(allHitsComplete)
			{
				_stage = AttackStage.Recovery;
			}

			var deflectedByHandleCast = false;
			var handleCastTime = _currentAttackConfig.AnimationConfig.GetMarkerTime(AnimationFlagEvent.AnimationFlags.StartHandleObstacleCast) ?? 0;
			if(_stage == AttackStage.Windup && NormalizedTime > handleCastTime)
			{
				var interpolatedHandleCaster = _weaponView.StartInterpolatedCast(WeaponColliderType.Handle);
				while(interpolatedHandleCaster.MoveNext() && !deflectedByHandleCast)
				{
					foreach(var caster in interpolatedHandleCaster.GetActiveColliders())
					{
						if(AttackHelpers.CastAttackObstacles(caster, false, true))
						{
							deflectedByHandleCast = true;
							_context.DeflectCurrentAttack.Execute();
							interpolatedHandleCaster.ResetOnInterrupted();
							break;
						}
					}
				}
			}

			_context.MaxDeltaTime.Value = hasActiveHit ? CharacterConstants.MaxDeltaTimeAttacking : CharacterConstants.MaxDeltaTimeNormal;

			if(Time >= _currentAttackConfig.Duration)
			{
				IsComplete = true;
			}

			if(_currentAttackConfig.AnimationConfig.HasFlag(AnimationFlagEvent.AnimationFlags.StateLocked, NormalizedTime))
			{
				_framesToUnlockWalk = FramesToUnlockWalkAfterStateUnlocked;
			}

			IsReadyToRememberNextCommand = TimeLeft < 3f;
			_weaponView.StorePreviousTransform();

			void UpdateStaminaRegenLock()
			{
				var disableRegen = _currentAttackConfig.AnimationConfig.HasFlag(AnimationFlagEvent.AnimationFlags.StaminaRegenDisabled, NormalizedTime);

				if(!_staminaRegenDisabled)
				{
					if(disableRegen)
					{
						_staminaRegenDisabled = true;
						_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenDisableKey, true);
					}
				}
				else if(!disableRegen)
				{
					_staminaRegenDisabled = false;
					_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenDisableKey, false);
				}
			}
		}

		public override void OnExit()
		{
			_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenDisableKey, false);
			base.OnExit();
		}

		public override bool TryContinueWithCommand(CharacterCommand nextCommand)
		{
			if(nextCommand is not CharacterCommand.RegularAttack and not CharacterCommand.StrongAttack)
			{
				return false;
			}

			if(_context.CharacterStats.Stamina.Value < 1)
			{
				return false;
			}

			//_context.DebugDrawer.Value.AddAttackComboAttempt(Time);

			if(_currentAttackConfig.AnimationConfig.HasFlag(AnimationFlagEvent.AnimationFlags.TimingExitToNextCombo, NormalizedTime))
			{
				_comboCounter++;
				SetEnterParams(nextCommand is CharacterCommand.StrongAttack ? AttackType.Strong : AttackType.Regular);
				LaunchAttack();
				return true;
			}
			return false;
		}

		public override float GetEnterStaminaCost()
		{
			//TODO: make correct calculation
			return _context.InventoryLogic.RightWeapon.Config.RegularAttacks[0].StaminaCost;
		}

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			if(nextCommand.IsMovementCommand())
			{
				if(_framesToUnlockWalk > 0)
				{
					_framesToUnlockWalk--;
					return false;
				}
			}

			return !_currentAttackConfig.AnimationConfig.HasFlag(AnimationFlagEvent.AnimationFlags.StateLocked, NormalizedTime);
		}

		public void SetEnterParams(AttackType attackType)
		{
			_attackType = attackType;
		}

		private void LaunchAttack()
		{
			GetCurrentAttackConfig(out _currentAttackConfig, out _currentAttackIndex);

			_weaponView = _context.EquippedWeaponViews[ArmamentSlot.Right];
			Duration = _currentAttackConfig.Duration;

			_stage = AttackStage.Windup;
			_staminaSpent = false;
			_projectileSpawned = false;
			_hitsData.Clear();

			foreach(var hitEvent in _currentAttackConfig.AnimationConfig.GetHitEvents())
			{
				_hitsData.Add(new HitData
				{
					Config = hitEvent
				});
			}

			CurrentAttackAnimation = _context.Animator.Play(_currentAttackConfig.Animation, 0.1f, FadeMode.FromStart);

			if(_attackType.IsRollAttack())
			{
				var startTime = _currentAttackConfig.AnimationConfig.GetMarkerTime(AnimationFlagEvent.AnimationFlags.TimingEnterFromRoll) ?? 0;
				SetAttackInitialTime(startTime);
			}
			else
			{
				if(_comboCounter > 0)
				{
					var startTime = _currentAttackConfig.AnimationConfig.GetMarkerTime(AnimationFlagEvent.AnimationFlags.TimingEnterFromCombo) ?? 0;
					SetAttackInitialTime(startTime);
				}
				else
				{
					Time = 0f;
					ResetForwardMovement();
				}
			}

			//_context.DebugDrawer.Value.AddAttackGraph(_currentAttackConfig);

			IsComplete = false;

			void SetAttackInitialTime(float time)
			{
				Time = time * CurrentAttackAnimation.Duration;
				CurrentAttackAnimation.Time = time * CurrentAttackAnimation.Duration;
				ResetForwardMovement(_currentAttackConfig.ForwardMovement.Evaluate(Time));
			}
		}

		private void SpawnProjectile(IHitConfig hitConfig)
		{
			var weapon = _weaponView;
			var weaponConfig = weapon.Config;
			var prefab = AddressableManager.GetPreloadedAsset<GameObject>(_currentAttackConfig.ProjectilePrefabNames);
			var projectileInstance = Object.Instantiate(prefab);
			var projectileView = projectileInstance.GetComponent<ProjectileView>();

			var spawnPosition = weapon.ProjectileSpawnPosition;
			var direction = _context.Transform.Forward;

			if(_context.LockOnLogic.LockOnTarget.HasValue)
			{
				var maxAngleCorrection = _currentAttackConfig.MaxProjectileHorizontalAngleCorrection;
				var targetPosition = _context.LockOnLogic.LockOnTarget.Value.transform.position + Vector3.up * 1.2f;
				var targetDirection = (targetPosition - spawnPosition).normalized;

				var forwardHorizontal = _context.Transform.Forward;
				forwardHorizontal.y = 0;
				forwardHorizontal = forwardHorizontal.normalized;

				var targetDirectionHorizontal = targetDirection;
				targetDirectionHorizontal.y = 0;
				targetDirectionHorizontal = targetDirectionHorizontal.normalized;

				var angleDifference = Vector3.SignedAngle(forwardHorizontal, targetDirectionHorizontal, Vector3.up);
				var clampedAngle = Mathf.Clamp(angleDifference, -maxAngleCorrection, maxAngleCorrection);

				var horizontalMagnitude = new Vector3(targetDirection.x, 0, targetDirection.z).magnitude;
				var correctedDirection = Quaternion.AngleAxis(clampedAngle, Vector3.up) * forwardHorizontal * horizontalMagnitude;
				correctedDirection.y = targetDirection.y;
				direction = correctedDirection;
			}

			projectileView.Initialize(new ProjectileData
			{
				Speed = weaponConfig.ProjectileSpeed,
				BaseDamage = _currentAttackConfig.BaseDamage,
				HitConfig = hitConfig,
				CasterContext = _context,
				DeflectionRating = weaponConfig.AttackDeflectionRating + _currentAttackConfig.AttackDeflectionRatingBonus,
				SpawnPosition = spawnPosition,
				Direction = direction
			});
		}

		private void GetCurrentAttackConfig(out AttackConfig attackConfig, out int newAttackIndex)
		{
			var weaponConfig = _context.InventoryLogic.RightWeapon.Config;
			if(_context.InputData.ForcedAttackConfig != null)
			{
				newAttackIndex = _currentAttackIndex;
				attackConfig = _context.InputData.ForcedAttackConfig;
			}

			switch(_attackType)
			{
				case AttackType.Regular:
				case AttackType.Strong:
					var attacksList = weaponConfig.GetAttacksSequence(_attackType);

					if(_comboCounter > 0)
					{
						newAttackIndex = _comboCounter % attacksList.Length;
					}
					else
					{
						newAttackIndex = 0;
					}

					attackConfig = attacksList[newAttackIndex];
					return;
				case AttackType.RollAttackRegular:
					newAttackIndex = 0;
					attackConfig = _weaponView.Config.RollAttack;
					return;
				case AttackType.RollAttackStrong:
					newAttackIndex = 0;
					attackConfig = _weaponView.Config.RollAttackStrong;
					return;
				case AttackType.RunAttackRegular:
					newAttackIndex = 0;
					try
					{
						attackConfig = _weaponView.Config.RunAttack;
						if(attackConfig == null)
						{
							attackConfig = weaponConfig.GetAttacksSequence(AttackType.Regular)[0];
						}
					}
					catch(Exception e)
					{
						Debug.LogError(_weaponView);
						Debug.LogError(_weaponView.Config);
						Debug.LogError(_weaponView.Config.RunAttack);
						throw;
					}
					
					return;
				case AttackType.RunAttackStrong:
					newAttackIndex = 0;
					attackConfig = _weaponView.Config.RunAttackStrong;
					return;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private enum AttackStage
		{
			Windup,
			Impact,
			Recovery
		}
	}
}
