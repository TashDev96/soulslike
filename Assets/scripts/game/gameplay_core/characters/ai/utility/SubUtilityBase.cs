using System;
using System.Collections.Generic;
using dream_lib.src.extensions;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.ai.utility.blackbox;
using game.gameplay_core.characters.ai.utility.considerations;
using game.gameplay_core.characters.ai.utility.considerations.value_sources;
using game.gameplay_core.characters.ai.utility.goals;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.state_machine.states;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility
{
	public class WeightUpdateEventArgs : EventArgs
	{
		public List<UtilityAction> Actions { get; }
		public float DeltaTime { get; }

		public WeightUpdateEventArgs(List<UtilityAction> actions, float deltaTime)
		{
			Actions = actions;
			DeltaTime = deltaTime;
		}
	}

	public class SubUtilityBase : MonoBehaviour
	{
		[SerializeField]
		private float _noGoalsWeight = 5f;
		[ValidateInput(nameof(ValidateGoals))]
		public List<GoalsChain> GoalChains;
		[ValidateInput(nameof(ValidateActions))]
		public List<UtilityAction> Actions;

		[HideInInspector]
		public string DebugString = "asdasd\nasdasd";

		protected UtilityBrainContext _context;
		private ReadOnlyTransform _transform;

		private bool _hasMovedByPathThisFrame;
		private bool _needRecalculatePath;
		private GoalsChain _currentGoalChain;
		private int _currentGoalIndex;
		private float _currentGoalExecutionTime;
		private UtilityAction _lastAction;

		private CharacterInputData InputData => _context.CharacterContext.InputData;

		public static event EventHandler<WeightUpdateEventArgs> OnWeightUpdate;

		public void Initialize(UtilityBrainContext context)
		{
			_context = context;
			_transform = _context.CharacterContext.Transform;
			_context.CharacterContext.OnStateChanged.OnExecute += HandleCharacterStateChanged;
		}

		public virtual void Think(float deltaTime)
		{
			var maxWeight = float.MinValue;
			var minWeight = float.MaxValue;
			var selectedAction = Actions[0];

			UpdateBlackboardValues();

			DebugString = "\ngoal:";

			UpdateGoals(deltaTime);

			if(_currentGoalChain != null)
			{
				DebugString += $"{_currentGoalChain.Id} {_currentGoalChain.Goals[_currentGoalIndex].Action} {_currentGoalExecutionTime.RoundFormat()}\n";
			}

			foreach(var utilityAction in Actions)
			{
				var weight = EvaluateConsiderations(utilityAction.Considerations, deltaTime);

				if(_currentGoalChain != null)
				{
					var currentGoalElement = _currentGoalChain.Goals[_currentGoalIndex];
					if(utilityAction.Id == currentGoalElement.Action)
					{
						weight += currentGoalElement.WeightAdd;
					}
				}

				if(utilityAction.HasInertia && utilityAction.InertiaTimer > 0)
				{
					weight += utilityAction.InertiaCurve.Evaluate(1 - utilityAction.InertiaTimer / utilityAction.InertiaDuration);
					utilityAction.InertiaTimer -= deltaTime;
				}

				DebugString += $"{utilityAction.Id} {weight} \n";

				if(weight > maxWeight)
				{
					maxWeight = weight;
					selectedAction = utilityAction;
				}

				if(weight < minWeight)
				{
					minWeight = weight;
				}

				utilityAction.DebugWeightCache = weight;
			}

			OnWeightUpdate?.Invoke(this, new WeightUpdateEventArgs(Actions, deltaTime));

			if(_currentGoalChain != null)
			{
				var currentGoalElement = _currentGoalChain.Goals[_currentGoalIndex];
				if(selectedAction.Id == currentGoalElement.Action)
				{
					_currentGoalExecutionTime += deltaTime;
					if(_currentGoalExecutionTime > currentGoalElement.Duration)
					{
						_currentGoalExecutionTime = 0;
						_currentGoalIndex++;
					}

					if(_currentGoalIndex >= _currentGoalChain.Goals.Count)
					{
						_currentGoalChain = null;
					}
				}
			}

			PerformAction(selectedAction);
		}

		private void UpdateBlackboardValues()
		{
			_context.BlackboardValues[BlackboardValues.BasicAttackRange] = _context.CharacterContext.InventoryLogic.RightWeapon.Config.RegularAttacks[0].Range;
			var leftWeapon = _context.CharacterContext.InventoryLogic.LeftWeapon;
			if(leftWeapon != null && leftWeapon.Config.BlockDeflectionRating > 0)
			{
				_context.BlackboardValues[BlackboardValues.HasShield] = 1;
			}
			else
			{
				_context.BlackboardValues[BlackboardValues.HasShield] = 0;
			}
		}

		private void HandleCharacterStateChanged(CharacterStateBase oldState, CharacterStateBase newState)
		{
		}

		private void UpdateGoals(float deltaTime)
		{
			var maxWeight = float.MinValue;
			var result = _currentGoalChain;

			var currentGoalWeight = 0f;
			if(_currentGoalChain != null)
			{
				currentGoalWeight += EvaluateConsiderations(_currentGoalChain.Considerations, deltaTime);
				currentGoalWeight += _currentGoalChain.InertiaWeight;
			}

			foreach(var goalChain in GoalChains)
			{
				var weight = EvaluateConsiderations(goalChain.Considerations, deltaTime);
				goalChain.LastWeight = weight;

				if(weight >= maxWeight && weight > currentGoalWeight && weight > _noGoalsWeight)
				{
					maxWeight = weight;
					result = goalChain;
				}
			}

			if(result != _currentGoalChain && result.LastWeight > 0)
			{
				_currentGoalChain = result;
				_currentGoalIndex = 0;
			}
		}

		private float EvaluateConsiderations(ICollection<Consideration> considerations, float deltaTime)
		{
			var result = 0f;
			foreach(var consideration in considerations)
			{
				result += consideration.Evaluate(_context);
			}
			return result;
		}

		private void PerformAction(UtilityAction action)
		{
			_hasMovedByPathThisFrame = false;

			if(_lastAction != action)
			{
				action.InertiaTimer = action.InertiaDuration;
				_lastAction = action;
			}

			var vectorToTarget = _transform.Forward;
			var mainAttackDistance = _context.BlackboardValues[BlackboardValues.BasicAttackRange];

			if(_context.TargetTransform != null)
			{
				vectorToTarget = _context.TargetTransform.Position - _transform.Position;
			}

			_context.CharacterContext.LockOnLogic.LockOnTarget.Value = _context.Target;

			switch(action.Type)
			{
				case UtilityAction.ActionType.LightAttack:
					InputData.Command = CharacterCommand.RegularAttack;
					InputData.DirectionWorld = vectorToTarget;
					break;
				case UtilityAction.ActionType.StrongAttack:
					InputData.Command = CharacterCommand.RegularAttack;
					InputData.DirectionWorld = vectorToTarget;
					break;
				case UtilityAction.ActionType.SpecialAttack:
					InputData.Command = CharacterCommand.RegularAttack;
					InputData.DirectionWorld = vectorToTarget;
					break;
				case UtilityAction.ActionType.Roll:
					InputData.Command = CharacterCommand.Roll;
					InputData.DirectionWorld = _transform.TransformDirection(action.Direction.ToVector());
					break;
				case UtilityAction.ActionType.WalkToTransform:
					break;
				case UtilityAction.ActionType.KeepSafeDistance:

					InputData.Command = _context.BlackboardValues[BlackboardValues.HasShield] > 0 ? CharacterCommand.WalkBlock : CharacterCommand.Walk;

					if(vectorToTarget.sqrMagnitude < action.Distance * action.Distance)
					{
						InputData.DirectionWorld = -vectorToTarget;
					}
					else
					{
						InputData.DirectionWorld = Mathf.Sin(Time.time) > 0 ? _transform.Right : -_transform.Right;
					}
					break;
				case UtilityAction.ActionType.GetIntoAttackDistance:
					MoveTo(_context.TargetTransform.Position - vectorToTarget.normalized * mainAttackDistance);
					break;
				case UtilityAction.ActionType.Strafe:
					break;
				case UtilityAction.ActionType.Heal:
					InputData.Command = CharacterCommand.UseItem;
					break;
				case UtilityAction.ActionType.Block:
					InputData.Command = _context.BlackboardValues[BlackboardValues.HasShield] > 0 ? CharacterCommand.StayBlock : CharacterCommand.None;

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if(InputData.Command is CharacterCommand.Walk or CharacterCommand.WalkBlock or CharacterCommand.Run or CharacterCommand.Roll)
			{
				if(_context.CharacterContext.CharacterCollider.CheckForFallOff(InputData.DirectionWorld, 1f, 1.3f))
				{
					InputData.Command = CharacterCommand.None;
					InputData.DirectionWorld = Vector3.zero;
				}
			}

			if(!_hasMovedByPathThisFrame)
			{
				_needRecalculatePath = true;
			}
		}

		private void MoveTo(Vector3 worldPos)
		{
			const float navMeshDistance = 4f;
			var moveVector = worldPos - _transform.Position;
			InputData.Command = CharacterCommand.Walk;
			if(moveVector.sqrMagnitude < navMeshDistance * navMeshDistance)
			{
				InputData.DirectionWorld = moveVector;
				if(moveVector.sqrMagnitude < 0.03f)
				{
					InputData.Command = CharacterCommand.None;
				}
			}
			else
			{
				_needRecalculatePath |= _context.NavigationModule.CheckTargetPositionChangedSignificantly(worldPos, 1f);
				if(_needRecalculatePath)
				{
					_context.NavigationModule.BuildPath(worldPos);
					_context.NavigationModule.DrawDebug(Color.green, 2f);
					_needRecalculatePath = false;
				}
				_context.CharacterContext.LockOnLogic.LockOnTarget.Value = null;
				InputData.DirectionWorld = _context.NavigationModule.CalculateMoveDirection(_transform.Position);
				_hasMovedByPathThisFrame = true;
			}
		}

#if UNITY_EDITOR
		private bool ValidateActions(List<UtilityAction> actions, ref string errorMessage)
		{
			if(actions.HasDuplicates(a => a.Id, out var duplicateId))
			{
				errorMessage = $"duplicate id {duplicateId}";
				return false;
			}

			return true;
		}

		private bool ValidateGoals(List<GoalsChain> goals, ref string errorMessage)
		{
			if(goals.HasDuplicates(a => a.Id, out var duplicateId))
			{
				errorMessage = $"duplicate id {duplicateId}";
				return false;
			}

			return true;
		}

		[Button]
		private void SetDebugColors()
		{
			for(var i = 0; i < Actions.Count; i++)
			{
				Actions[i].DebugColor = Color.HSVToRGB((float)i / Actions.Count, 0.8f, 1f);
			}
		}

		[OnInspectorGUI]
		private void DrawGui()
		{
			EditorGUILayout.LabelField(DebugString, EditorStyles.wordWrappedLabel);
		}
#endif
	}
}
