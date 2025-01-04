using System.Collections.Generic;
using dream_lib.src.extensions;
using dream_lib.src.utils.components;
using game.gameplay_core.characters.ai.blackbox;
using game.gameplay_core.characters.ai.navigation;
using UnityEngine;

namespace game.gameplay_core.characters.ai
{
	public class UtilityBrain : MonoBehaviour, ICharacterBrain
	{
		[SerializeField]
		private List<SubUtilityBase> _subUtilities;
		
		private UtilityBrainContext _context;
		
		
		[SerializeField]
		private TriggerEventsListener[] _aggroZones;
		[SerializeField]
		private float _attackDistance = 2f;
		[SerializeField]
		private float _fightModeDistance = 4f;
		
		private bool HasTarget => _context.Target != null;


		public void Initialize(CharacterContext context)
		{
			_context = new UtilityBrainContext()
			{
				CharacterContext = context,
				PerformedActionsHistory = new List<ActionHistoryNode>(),
				NavigationModule = new AiNavigationModule(context.Transform)
			};
			
			foreach(var subUtility in _subUtilities)
			{
				subUtility.Initialize(_context);
			}
			
			
			foreach(var triggerListener in _aggroZones)
			{
				triggerListener.OnTriggerEnterEvent += HandleAggroTriggerEnter;
			}
		}
		
		private void HandleAggroTriggerEnter(Collider enteredObject)
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

		public float GetTimeSinceActionPerformed(string actionId)
		{
			return 0f;
		}

		public void Think(float deltaTime)
		{
			if(_context.Target != null)
			{
				_subUtilities[0].Think(deltaTime);
			}
			else
			{
				
			}
		}

		public string GetDebugSting()
		{
			return $"brain: {_subUtilities[0].DebugString}\n";
		}
	}
}
