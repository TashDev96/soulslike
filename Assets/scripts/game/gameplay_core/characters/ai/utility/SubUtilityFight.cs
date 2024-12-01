using System.Collections.Generic;
using dream_lib.src.extensions;
using game.gameplay_core.characters.ai.editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace game.gameplay_core.characters.ai
{
	[OdinDontRegister]
	public class SubUtilityFight : MonoBehaviour, ISerializationCallbackReceiver
	{
		[ValidateInput(nameof(ValidateGoals))]
		public List<GoalsChain> GoalChains;
		[ValidateInput(nameof(ValidateActions))]
		public List<UtilityAction> Actions;

		//chain of goals
		//chain examples:
		//multiple attacks
		//jump back then heal
		//jump attack then roll back

		//input: enemy
		//input: self stats
		//input: inventory

		//input: fight history data

		//list of attacks
		//list of defences
		//list of movement
		//list of stupidities

		public void HandleCharacterUpdate(float deltaTime)
		{
		}

		public void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			UtilityEditorHelper.CurrentContextActions = Actions;
#endif
		}

		public void OnAfterDeserialize()
		{
		}

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
	}
}
