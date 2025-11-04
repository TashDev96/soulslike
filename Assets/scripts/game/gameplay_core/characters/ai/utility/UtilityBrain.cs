using System;
using System.Collections.Generic;
using dream_lib.src.utils.components;
using game.gameplay_core.characters.ai.navigation;
using game.gameplay_core.characters.ai.utility.blackbox;
using game.gameplay_core.characters.ai.utility.considerations.value_sources;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility
{
	public class UtilityBrain : MonoBehaviour, ICharacterBrain
	{
		[SerializeField]
		private List<SubUtilityBase> _subUtilities;

		[SerializeField]
		private TriggerEventsListener[] _aggroZones;
		[SerializeField]
		private float _attackDistance = 2f;
		[SerializeField]
		private float _fightModeDistance = 4f;

		private UtilityBrainContext _context;

		[ShowInInspector]
		private bool HasTarget => _context?.Target != null;

		public void Initialize(CharacterContext context)
		{
			_context = new UtilityBrainContext
			{
				CharacterContext = context,
				PerformedActionsHistory = new List<ActionHistoryNode>(),
				NavigationModule = new AiNavigationModule(context.Transform),
				BlackboardValues = new Dictionary<BlackboardValues, float>()
			};

			foreach(BlackboardValues enumKey in Enum.GetValues(typeof(BlackboardValues)))
			{
				_context.BlackboardValues.Add(enumKey, 0f);
			}
			foreach(var subUtility in _subUtilities)
			{
				subUtility.Initialize(_context);
			}

			foreach(var triggerListener in _aggroZones)
			{
				triggerListener.OnTriggerEnterEvent += HandleAggroTriggerEnter;
			}
		}

		public float GetTimeSinceActionPerformed(string actionId)
		{
			return 0f;
		}

		public void Think(float deltaTime)
		{
			_context.BrainTime += deltaTime;
			if(_context.Target != null)
			{
				_subUtilities[0].Think(deltaTime);
			}
		}

		public string GetDebugSting()
		{
			return $"brain: {_subUtilities[0].DebugString}\n";
		}

		private void HandleAggroTriggerEnter(GameObject enteredObject)
		{
			if(HasTarget)
			{
				return;
			}

			if(enteredObject.gameObject.TryGetComponent<CharacterDomain>(out var otherCharacter))
			{
				if(otherCharacter.ExternalData.Team != _context.CharacterContext.Team.Value)
				{
					_context.Target = otherCharacter;
					_context.CharacterContext.LockOnLogic.LockOnTarget.Value = otherCharacter;
				}
			}
		}
	}
}
