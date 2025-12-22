using System;
using System.Collections.Generic;
using Animancer;
using dream_lib.src.extensions;
using game.gameplay_core.characters.commands;
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
			_context.RightWeapon.Value.StorePreviousTransform();
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if(_context.InputData.HasDirectionInput && !_currentAttackConfig.RotationDisabledTime.Contains(NormalizedTime))
			{
				_context.MovementLogic.RotateCharacter(_context.InputData.DirectionWorld, deltaTime);
			}

			UpdateStaminaRegenLock();

			UpdateForwardMovement(_currentAttackConfig.ForwardMovement.Evaluate(Time), deltaTime);

			var hasActiveHit = false;
			var allHitsComplete = true;

			if(_currentAttackConfig.IsRangedAttack)
			{
				foreach(var hitData in _hitsData)
				{
					var hitTiming = hitData.Config.Timing;

					if(!hitData.IsStarted && NormalizedTime >= hitTiming.x)
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
					var hitTiming = hitData.Config.Timing;

					if(!hitData.IsStarted && NormalizedTime >= hitTiming.x)
					{
						hitData.IsStarted = true;
						if(!_staminaSpent)
						{
							_context.StaminaLogic.SpendStamina(_currentAttackConfig.StaminaCost);
						}
					}

					if(hitData.IsActive)
					{
						var interpolatedCaster = _context.RightWeapon.Value.StartInterpolatedCast(WeaponColliderType.Attack, hitData.Config.InvolvedColliders);
						while(interpolatedCaster.MoveNext())
						{
							foreach(var caster in interpolatedCaster.GetActiveColliders())
							{
								var deflectionRating = _context.RightWeapon.Value.Config.AttackDeflectionRating + _currentAttackConfig.AttackDeflectionRatingBonus;
								AttackHelpers.CastAttack(_currentAttackConfig.BaseDamage, hitData, caster, _context, deflectionRating, true);
							}
						}

						if(NormalizedTime >= hitTiming.y)
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
			if(_stage == AttackStage.Windup && NormalizedTime > _currentAttackConfig.StartHandleObstacleCastTime)
			{
				var interpolatedHandleCaster = _context.RightWeapon.Value.StartInterpolatedCast(WeaponColliderType.Handle);
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

			if(_currentAttackConfig.LockedStateTime.Contains(NormalizedTime))
			{
				_framesToUnlockWalk = FramesToUnlockWalkAfterStateUnlocked;
			}

			IsReadyToRememberNextCommand = TimeLeft < 3f;
			_context.RightWeapon.Value.StorePreviousTransform();

			void UpdateStaminaRegenLock()
			{
				if(!_staminaRegenDisabled)
				{
					if(_currentAttackConfig.StaminaRegenDisabledTime.Contains(NormalizedTime))
					{
						_staminaRegenDisabled = true;
						_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenDisableKey, true);
					}
				}
				else if(!_currentAttackConfig.StaminaRegenDisabledTime.Contains(NormalizedTime))
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

			if(_currentAttackConfig.ExitToComboTime.Contains(NormalizedTime))
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
			GetCurrentAttackConfig(out var config, out _);
			return config.StaminaCost;
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

			return !_currentAttackConfig.LockedStateTime.Contains(NormalizedTime);
		}

		public void SetEnterParams(AttackType attackType)
		{
			_attackType = attackType;
		}

		private void LaunchAttack()
		{
			GetCurrentAttackConfig(out _currentAttackConfig, out _currentAttackIndex);
			Duration = _currentAttackConfig.Duration;

			_stage = AttackStage.Windup;
			_staminaSpent = false;
			_projectileSpawned = false;
			_hitsData.Clear();
			for(var i = 0; i < _currentAttackConfig.HitConfigs.Count; i++)
			{
				_hitsData.Add(new HitData
				{
					Config = _currentAttackConfig.HitConfigs[i]
				});
			}

			CurrentAttackAnimation = _context.Animator.Play(_currentAttackConfig.Animation, 0.1f, FadeMode.FromStart);

			if(_attackType.IsRollAttack())
			{
				SetAttackInitialTime(_currentAttackConfig.EnterFromRollTime);
			}
			else
			{
				if(_comboCounter > 0)
				{
					SetAttackInitialTime(_currentAttackConfig.EnterComboTime);
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

		private void SpawnProjectile(HitConfig hitConfig)
		{
			var weapon = _context.RightWeapon.Value;
			var weaponConfig = weapon.Config;
			var prefab = AddressableManager.LoadAssetImmediately<GameObject>(_currentAttackConfig.ProjectilePrefabNames, AssetOwner.Game);
			var projectileInstance = Object.Instantiate(prefab);
			var projectileView = projectileInstance.GetComponent<ProjectileView>();

			var spawnPosition = weapon.ProjectileSpawnPosition;
			var direction = _context.Transform.Forward;

			if(_context.LockOnLogic.LockOnTarget.HasValue)
			{
				var targetPosition = _context.LockOnLogic.LockOnTarget.Value.transform.position + Vector3.up * 1.2f;
				direction = (targetPosition - spawnPosition).normalized;
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
			var weaponConfig = _context.RightWeapon.Value.Config;
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
					attackConfig = _context.RightWeapon.Value.Config.RollAttack;
					return;
				case AttackType.RollAttackStrong:
					newAttackIndex = 0;
					attackConfig = _context.RightWeapon.Value.Config.RollAttackStrong;
					return;
				case AttackType.RunAttackRegular:
					newAttackIndex = 0;
					attackConfig = _context.RightWeapon.Value.Config.RunAttack;
					return;
				case AttackType.RunAttackStrong:
					newAttackIndex = 0;
					attackConfig = _context.RightWeapon.Value.Config.RunAttackStrong;
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
