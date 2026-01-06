using System;
using dream_lib.src.reactive;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.extensions;
using game.gameplay_core.characters.state_machine.states;
using game.gameplay_core.characters.state_machine.states.attack;
using game.gameplay_core.characters.state_machine.states.attack.critical;
using game.gameplay_core.characters.state_machine.states.stagger;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine
{
	public class CharacterStateMachine
	{
		private readonly CharacterContext _context;

		private readonly IdleState _idleState;
		private readonly WalkState _walkState;
		private readonly RunState _runState;
		private readonly RollState _rollState;
		private readonly AttackState _attackState;
		private readonly FallState _fallState;
		private readonly StayBlockState _stayBlockState;
		private readonly WalkBlockState _walkBlockState;
		private readonly ParryState _parryState;

		private CharacterCommand _nextCommand;
		private readonly ReactiveProperty<CharacterStateBase> _currentState = new();

		private CharacterCommand NextCommand
		{
			get => _nextCommand;
			set
			{
				if(value != _nextCommand)
				{
				}
				_nextCommand = value;
			}
		}

		public IReadOnlyReactiveProperty<CharacterStateBase> CurrentState => _currentState;

		public CharacterStateMachine(CharacterContext characterContext)
		{
			_context = characterContext;

			_idleState = new IdleState(_context);
			_walkState = new WalkState(_context);
			_attackState = new AttackState(_context);
			_rollState = new RollState(_context);
			_fallState = new FallState(_context);
			_runState = new RunState(_context);
			_stayBlockState = new StayBlockState(_context);
			_walkBlockState = new WalkBlockState(_context);
			_parryState = new ParryState(_context);

			_context.IsDead.OnChanged += HandleIsDeadChanged;
			_context.TriggerStagger.OnExecute += HandleTriggerStagger;
			_context.DeflectCurrentAttack.OnExecute += HnadleAttackDeflected;
			_context.BlockLogic.OnParryFail.OnExecute += HandleParryFail;

			_context.IsFalling.OnChangedFromTo += HandleIsFallingChanged;

			SetState(_idleState);
		}

		public void TriggerParryStun()
		{
			_currentState.Value.OnInterrupt();
			SetState(new ParryStunState(_context));
		}

		public void LockInAnimation(AnimationClip animationClip, bool canInterruptByStagger = false)
		{
			_currentState.Value.OnInterrupt();
			SetState(new LockedInAnimationState(_context, animationClip, canInterruptByStagger));
		}

		public void Update(float deltaTime, bool calculateInputLogic)
		{
			_currentState.Value.Update(deltaTime);

			calculateInputLogic &= !_context.IsDead.Value;

			if(calculateInputLogic)
			{
				TryRememberNextCommand();
				CalculateChangeState();
			}
		}

		public string GetDebugString()
		{
			var str = "";
			str += $"state:   {_currentState.Value.GetType().Name}  complete: {_currentState.Value.IsComplete}\n";
			str += $"{_currentState.Value.GetDebugString()}\n";
			str += $"command: {_context.InputData.Command}\n";
			str += $"next command: {NextCommand}\n";
			return str;
		}

		private void HandleParryFail()
		{
			if(_currentState.Value is ParryState parryState)
			{
				parryState.OnParryFailed();
			}
		}

		private void HnadleAttackDeflected()
		{
			if(_currentState.Value is AttackState attackState)
			{
				SetState(new DeflectedAttackState(_context, attackState.CurrentAttackAnimation, attackState.CurrentAttackConfig));
			}
		}

		private void HandleIsFallingChanged(bool wasFalling, bool isFalling)
		{
			if(isFalling && !(_currentState.Value is FallState) && !_context.IsDead.Value)
			{
				if(!(_currentState.Value is AttackState) && !(_currentState.Value is RollState) && !(_currentState.Value is StaggerState))
				{
					_currentState.Value.OnInterrupt();
					SetState(_fallState);
				}
			}

			if(!isFalling && wasFalling && _currentState.Value is FallState fallState)
			{
				if(fallState.ShouldRollOnLanding)
				{
					SetState(_rollState);
				}
			}
		}

		private void HandleTriggerStagger(StaggerReason staggerReason)
		{
			if(_currentState.Value.CanInterruptByStagger && !_context.IsDead.Value)
			{
				_currentState.Value.OnInterrupt();
				SetState(new StaggerState(_context, _currentState.Value, staggerReason));
			}
		}

		private void HandleIsDeadChanged(bool isDead)
		{
			if(isDead)
			{
				SetState(new DeadState(_context));
			}
		}

		private void TryRememberNextCommand()
		{
			var inputCommand = _context.InputData.Command;

			var overrideMovement = NextCommand.IsMovementCommand() && !inputCommand.IsMovementCommand() && inputCommand != CharacterCommand.None;

			if(NextCommand == CharacterCommand.None || overrideMovement)
			{
				if(_currentState.Value.IsReadyToRememberNextCommand)
				{
					NextCommand = inputCommand;
				}
			}
		}

		private void CalculateChangeState()
		{
			if(TryEnterFall())
			{
				return;
			}

			if(CheckRunExhaustedEnd())
			{
				return;
			}

			if(TryContinueCurrentState())
			{
				return;
			}

			if(TryEnterRollAttack())
			{
				return;
			}

			if(TryEnterRollAfterFall())
			{
				return;
			}

			if(_currentState.Value.CheckIsReadyToChangeState(NextCommand))
			{
				switch(NextCommand)
				{
					case CharacterCommand.None:
						if(_currentState.Value.IsComplete && _currentState.Value != _idleState)
						{
							SetState(_idleState);
						}
						break;
					case CharacterCommand.Walk:
						SetState(_walkState);
						break;
					case CharacterCommand.Run:
						SetState(_runState);
						break;
					case CharacterCommand.Roll:
						SetState(_rollState);
						break;
					case CharacterCommand.RegularAttack:

						var weaponConfig = _context.InventoryLogic.RightWeapon.Config;

						var canDoBackStabs = weaponConfig.CanBackstab && _context.Config.CanDoBackstabs;
						var canRiposte = weaponConfig.CanRiposte;

						var riposteableEnemy = canRiposte ? FindRiposteableEnemy() : null;
						var backstabbableEnemy = canDoBackStabs ? FindBackstabbableEnemy() : null;

						if(canRiposte && riposteableEnemy != null)
						{
							SetState(new RiposteAttackState(_context, riposteableEnemy));
						}
						else if(canDoBackStabs && backstabbableEnemy != null)
						{
							SetState(new BackstabAttackState(_context, backstabbableEnemy));
						}
						else
						{
							_attackState.SetEnterParams(_currentState.Value is RunState ? AttackType.RunAttackRegular : AttackType.Regular);
							SetState(_attackState);
						}
						break;
					case CharacterCommand.StrongAttack:
						_attackState.SetEnterParams(_currentState.Value is RunState ? AttackType.RunAttackStrong : AttackType.Strong);
						SetState(_attackState);
						break;
					case CharacterCommand.StayBlock:
						SetState(_stayBlockState);
						break;
					case CharacterCommand.WalkBlock:
						SetState(_walkBlockState);
						break;
					case CharacterCommand.Parry:
						if(_context.InventoryLogic.CheckHasParryWeapon())
						{
							SetState(_parryState);
						}
						break;
					case CharacterCommand.UseItem:

						if(_context.CurrentConsumableItem.HasValue && _context.CurrentConsumableItem.Value.CheckCanStartConsumption())
						{
							SetState(new ConsumeState(_context, _context.CurrentConsumableItem.Value));
						}
						break;
					case CharacterCommand.Interact:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				NextCommand = CharacterCommand.None;
			}
		}

		private bool TryEnterRollAfterFall()
		{
			if(_currentState.Value == _fallState && _fallState.IsComplete && _fallState.HasValidRollInput)
			{
				SetState(_rollState);
				return true;
			}
			return false;
		}

		private bool CheckRunExhaustedEnd()
		{
			if(_currentState.Value is RunState && _context.CharacterStats.Stamina.Value <= 0)
			{
				if(NextCommand == CharacterCommand.Run)
				{
					SetState(_walkState);
					return true;
				}
			}
			return false;
		}

		private bool TryContinueCurrentState()
		{
			if(_currentState.Value.TryContinueWithCommand(NextCommand))
			{
				NextCommand = CharacterCommand.None;
				_context.InputData.Command = CharacterCommand.None;
				return true;
			}
			return false;
		}

		private bool TryEnterFall()
		{
			if(_context.IsFalling.Value && _currentState.Value is not FallState)
			{
				if(_currentState.Value.IsComplete || _currentState.Value.CheckIsReadyToChangeState(CharacterCommand.Walk))
				{
					SetState(_fallState);
					return true;
				}
			}
			return false;
		}

		private bool TryEnterRollAttack()
		{
			if(NextCommand.IsAttackCommand() && _currentState.Value is RollState { CanSwitchToAttack: true })
			{
				var rollAttackType = NextCommand is CharacterCommand.StrongAttack ? AttackType.RollAttackStrong : AttackType.RollAttackRegular;
				_attackState.SetEnterParams(rollAttackType);
				SetState(_attackState);
				NextCommand = CharacterCommand.None;
				return true;
			}
			return false;
		}

		private void SetState(CharacterStateBase newState)
		{
			_currentState.Value?.OnExit();
			var oldState = _currentState.Value;

			if(!_context.StaminaLogic.CheckCanEnterState(newState))
			{
				if(newState is RunState)
				{
					if(_currentState.Value is WalkState)
					{
						return;
					}
					newState = _walkState;
				}
				else
				{
					newState = _idleState;
				}
			}

			_currentState.Value = newState;
			_currentState.Value.OnEnter();
			_context.OnStateChanged.Execute(oldState, newState);
			NextCommand = CharacterCommand.None;
		}

		private CharacterDomain FindBackstabbableEnemy()
		{
			const float maxBackstabDistance = 3f;
			const float attackerBehindVictimMaxAngle = 45f;

			var selfPosition = _context.Transform.Position;
			var selfForward = _context.Transform.Forward;

			foreach(var character in _context.LockOnLogic.AllCharacters)
			{
				if(character == _context.SelfLink || character.ExternalData.IsDead)
				{
					continue;
				}

				if(character.CharacterStateMachine.CurrentState.Value is LockedInAnimationState)
				{
					continue;
				}

				var targetPosition = character.ExternalData.Transform.Position;
				var distance = (targetPosition - selfPosition).magnitude;

				if(distance > maxBackstabDistance)
				{
					continue;
				}

				if(!CheckAngleForBackstab(selfPosition, selfForward, targetPosition, character.ExternalData.Transform.Forward, attackerBehindVictimMaxAngle))
				{
					continue;
				}

				return character;
			}

			return null;
		}

		private CharacterDomain FindRiposteableEnemy()
		{
			const float maxRiposteDistance = 3f;
			const float attackerLookingAtVictimMaxAngle = 10f;
			const float victimLookingAtAttackerMaxAngle = 20f;

			var selfPosition = _context.Transform.Position;
			var selfForward = _context.Transform.Forward;

			foreach(var character in _context.LockOnLogic.AllCharacters)
			{
				if(character == _context.SelfLink || character.ExternalData.IsDead)
				{
					continue;
				}

				if(character.CharacterStateMachine.CurrentState.Value is LockedInAnimationState)
				{
					continue;
				}

				if(!(character.CharacterStateMachine.CurrentState.Value is ParryStunState parryStunState) ||
				   !parryStunState.CanReceiveRiposte)
				{
					continue;
				}

				var targetPosition = character.ExternalData.Transform.Position;
				var distance = (targetPosition - selfPosition).magnitude;

				if(distance > maxRiposteDistance)
				{
					Debug.DrawLine(_context.Transform.Position, targetPosition, Color.red, 1f);
					;
					continue;
				}

				if(!CheckAngleForRiposte(selfPosition, selfForward, targetPosition, character.ExternalData.Transform.Forward, attackerLookingAtVictimMaxAngle, victimLookingAtAttackerMaxAngle))
				{
					continue;
				}

				return character;
			}

			return null;
		}

		private bool CheckAngleForBackstab(Vector3 attackerPos, Vector3 attackerForward, Vector3 victimPos, Vector3 victimForward, float attackerBehindVictimMaxAngle)
		{
			var attackerToVictim = (victimPos - attackerPos).normalized;
			var victimToAttacker = (attackerPos - victimPos).normalized;

			var attackerLookingAtVictimAngle = Vector3.Angle(attackerForward, attackerToVictim);
			var victimLookingAwayFromAttackerAngle = Vector3.Angle(victimForward, -victimToAttacker);

			var attackerInsideCone = attackerLookingAtVictimAngle <= attackerBehindVictimMaxAngle;
			var victimLookingAway = victimLookingAwayFromAttackerAngle <= attackerBehindVictimMaxAngle;

			DrawAngleDebugLines(attackerPos, attackerForward, attackerToVictim, victimPos, victimForward, -victimToAttacker,
				attackerBehindVictimMaxAngle, attackerBehindVictimMaxAngle, attackerInsideCone && victimLookingAway, Color.blue);

			return attackerInsideCone && victimLookingAway;
		}

		private bool CheckAngleForRiposte(Vector3 attackerPos, Vector3 attackerForward, Vector3 victimPos, Vector3 victimForward, float attackerLookingAtVictimMaxAngle, float victimLookingAtAttackerMaxAngle)
		{
			var attackerToVictim = (victimPos - attackerPos).normalized;
			var victimToAttacker = (attackerPos - victimPos).normalized;

			var attackerLookingAtVictimAngle = Vector3.Angle(attackerForward, attackerToVictim);
			var victimLookingAtAttackerAngle = Vector3.Angle(victimForward, victimToAttacker);

			var attackerInsideCone = attackerLookingAtVictimAngle <= attackerLookingAtVictimMaxAngle;
			var victimInsideCone = victimLookingAtAttackerAngle <= victimLookingAtAttackerMaxAngle;

			DrawAngleDebugLines(attackerPos, attackerForward, attackerToVictim, victimPos, victimForward, victimToAttacker,
				attackerLookingAtVictimMaxAngle, victimLookingAtAttackerMaxAngle, attackerInsideCone && victimInsideCone, Color.yellow);

			return attackerInsideCone && victimInsideCone;
		}

		private void DrawAngleDebugLines(Vector3 attackerPos, Vector3 attackerForward, Vector3 attackerToVictim, Vector3 victimPos, Vector3 victimForward, Vector3 victimToAttacker, float attackerMaxAngle, float victimMaxAngle, bool success, Color debugColor)
		{
			const float debugDistance = 2f;
			const float debugDuration = 2f;

			Debug.DrawLine(attackerPos, attackerPos + attackerForward * debugDistance, debugColor, debugDuration);
			Debug.DrawLine(attackerPos, attackerPos + attackerToVictim * debugDistance, success ? Color.green : Color.red, debugDuration);

			var attackerConeLeft = Quaternion.AngleAxis(-attackerMaxAngle, Vector3.up) * attackerForward;
			var attackerConeRight = Quaternion.AngleAxis(attackerMaxAngle, Vector3.up) * attackerForward;
			Debug.DrawLine(attackerPos, attackerPos + attackerConeLeft * debugDistance, debugColor, debugDuration);
			Debug.DrawLine(attackerPos, attackerPos + attackerConeRight * debugDistance, debugColor, debugDuration);

			Debug.DrawLine(victimPos, victimPos + victimForward * debugDistance, debugColor, debugDuration);
			Debug.DrawLine(victimPos, victimPos + victimToAttacker * debugDistance, success ? Color.green : Color.red, debugDuration);

			var victimConeLeft = Quaternion.AngleAxis(-victimMaxAngle, Vector3.up) * victimForward;
			var victimConeRight = Quaternion.AngleAxis(victimMaxAngle, Vector3.up) * victimForward;
			Debug.DrawLine(victimPos, victimPos + victimConeLeft * debugDistance, debugColor, debugDuration);
			Debug.DrawLine(victimPos, victimPos + victimConeRight * debugDistance, debugColor, debugDuration);
		}

		public void Reset()
		{
			SetState(_idleState);
		}
	}
}
