using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using game.gameplay_core.characters.ai.navigation;
using game.gameplay_core.characters.ai.utility.blackbox;
using game.gameplay_core.characters.ai.utility.considerations.value_sources;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility
{
	public class UtilityBrain : MonoBehaviour, ICharacterBrain
	{
		[SerializeField]
		private List<SubUtilityBase> _subUtilities;

		private UtilityBrainContext _context;

		public void Initialize(CharacterContext context)
		{
			_context = new UtilityBrainContext
			{
				CharacterContext = context,
				PerformedActionsHistory = new List<ActionHistoryNode>(),
				NavigationModule = new AiNavigationModule(context.Transform),
				BlackboardValues = new Dictionary<BlackboardValues, float>(),
				Sensors = context.SensorsDomain
			};

			foreach(BlackboardValues enumKey in Enum.GetValues(typeof(BlackboardValues)))
			{
				_context.BlackboardValues.Add(enumKey, 0f);
			}
			foreach(var subUtility in _subUtilities)
			{
				subUtility.Initialize(_context);
			}
		}

		public float GetTimeSinceActionPerformed(string actionId)
		{
			return 0f;
		}

		public void Think(float deltaTime)
		{
			_context.BrainTime += deltaTime;

			SubUtilityBase utilityToExecute = null;
			var maxWeight = 0f;

			foreach(var subUtilityBase in _subUtilities)
			{
				var weight = subUtilityBase.GetExecutionWorthWeight();
				if(weight > maxWeight)
				{
					maxWeight = weight;
					utilityToExecute = subUtilityBase;
				}
			}

			utilityToExecute?.Think(deltaTime);
		}

		public void GetDebugString(StringBuilder sb)
		{
			foreach(var subUtilityBase in _subUtilities)
			{
				var weight = subUtilityBase.GetExecutionWorthWeight();
				sb.Append(subUtilityBase.GetType().Name).Append(" ").Append(weight).AppendLine();
			}
		}

		public void Reset()
		{
			foreach(var key in _context.BlackboardValues.Keys.ToList())
			{
				_context.BlackboardValues[key] = 0f;
			}
			_context.BrainTime = 0;
		}
	}
}
