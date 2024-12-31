using System;
using System.Collections.Generic;
using dream_lib.src.extensions;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.ai.blackbox;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.runtime_data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.ai
{
	public class SubUtilityBase : MonoBehaviour, ISerializationCallbackReceiver
	{
		[ValidateInput(nameof(ValidateGoals))]
		public List<GoalsChain> GoalChains;
		[ValidateInput(nameof(ValidateActions))]
		public List<UtilityAction> Actions;
		
		private UtilityBrainContext _context;

		public void Initialize(UtilityBrainContext context)
		{
			_context = context;
		}

		public void Think(float deltaTime)
		{

			var maxWeight = float.MinValue;
			var selectedAction = Actions[0];

			DebugString = "";
			
			foreach(var utilityAction in Actions)
			{
				var weight = 0f;
				foreach(var consideration in utilityAction.Considerations)
				{
					weight += consideration.Evaluate(_context);
				}

				DebugString += $"{utilityAction.Id} {weight} \n";

				if(weight > maxWeight)
				{
					maxWeight = weight;
					selectedAction = utilityAction;
				}
			}

			PerformAction(selectedAction);

		}

		public string DebugString;

		private CharacterInputData InputData => _context.CharacterContext.InputData;
		private ReadOnlyTransform Transform => _context.CharacterContext.Transform;
		
		private void PerformAction(UtilityAction action)
		{
			var vectorToTarget = Transform.Forward;
			if(_context.TargetTransform != null)
			{
				vectorToTarget = _context.TargetTransform.Position - Transform.Position;
			}
			
			switch(action.Type)
			{
				case UtilityAction.ActionType.LightAttack:
					InputData.Command = CharacterCommand.Attack;
					InputData.DirectionWorld = vectorToTarget;
					break;
				case UtilityAction.ActionType.StrongAttack:
					InputData.Command = CharacterCommand.Attack;
					InputData.DirectionWorld = vectorToTarget;
					break;
				case UtilityAction.ActionType.SpecialAttack:
					InputData.Command = CharacterCommand.Attack;
					InputData.DirectionWorld = vectorToTarget;
					break;
				case UtilityAction.ActionType.Roll:
					InputData.Command = CharacterCommand.Roll;
					InputData.DirectionWorld = Transform.TransformDirection(action.Direction.ToVector());
					break;
				case UtilityAction.ActionType.WalkToTransform:
					break;
				case UtilityAction.ActionType.KeepSafeDistance:
					InputData.Command = CharacterCommand.Walk;
					InputData.DirectionWorld = -vectorToTarget;
					break;
				case UtilityAction.ActionType.GetIntoAttackDistance:
					InputData.Command = CharacterCommand.Walk;
					InputData.DirectionWorld = vectorToTarget;
					break;
				case UtilityAction.ActionType.Strafe:
					break;
				case UtilityAction.ActionType.Heal:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void PerformRelativeToTargetMovement(Vector3 localVector)
		{
			
		}

		public void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			foreach(var goalChain in GoalChains)
			{
				goalChain.PropagateEditorData(this);
			}
#endif
		}

		public void OnAfterDeserialize()
		{
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
#endif
	}
}
