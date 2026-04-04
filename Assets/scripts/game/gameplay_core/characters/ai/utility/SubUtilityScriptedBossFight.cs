using System.Collections.Generic;
using dream_lib.src.utils.data_types;
using game.enums;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.state_machine.states.attack;
using game.gameplay_core.damage_system;
using game.gameplay_core.inventory.item_configs;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility
{
	public class SubUtilityScriptedBossFight : SubUtilityBase
	{
		[SerializeField]
		private SerializableDictionary<int, List<int>> _attackSequencesFirstPhase;

		[SerializeField]
		private SerializableDictionary<int, List<int>> _attackSequencesSecondPhase;


		private struct RuntimeData
		{
			public int Phase;
			public int AttackSequenceKey;
			public int AttackIndex;
			public CharacterDomain Target;
			public bool IsAttackInProcess;
		}

		private RuntimeData _data;

		public override void Think(float deltaTime)
		{
			base.Think(deltaTime);

			switch(_data.Phase)
			{
				case 0:

					ThinkFirstPhase();
					break;
				case 1:

					ThinkSecondPhase();
					break;
			}
		}

		public override float GetExecutionWorthWeight()
		{
			if(_data.Target != null)
			{
				return 1;
			}

			foreach(var observation in CharacterObservations)
			{
				if(observation.Character == null)
				{
					continue;
				}

				if(observation.Character.Context.Team.Value == Team.Player)
				{
					_data.Target = observation.Character;
					_context.CharacterContext.LockOnLogic.LockOnTarget.Value = _data.Target;
					return 1;
				}
			}

			return 0;
		}

		private void ThinkFirstPhase()
		{
			if(_data.IsAttackInProcess)
			{
				var currentState = _context.CharacterContext.CurrentState.Value;
				if(currentState.CheckIsReadyToChangeState(CharacterCommand.RegularAttack) || currentState is not AttackState)
				{
					_data.IsAttackInProcess = false;
				}
				return;
			}

			var weaponConfig = _context.CharacterContext.InventoryLogic.GetEquipment(EquipmentSlotType.RightHand).BaseConfig as WeaponItemConfig;
			var nextAttackIndex = _attackSequencesFirstPhase[_data.AttackSequenceKey][_data.AttackIndex];
			var nextAttackConfig = weaponConfig.SpecialAttacks[nextAttackIndex];

			var range = nextAttackConfig.Range;
			
			
			var vecToTarget = _data.Target.transform.position - _context.CharacterContext.Transform.Position;
			var distanceToTarget = vecToTarget.magnitude;

			if(distanceToTarget > range)
			{
				_context.CharacterContext.InputData.Command = CharacterCommand.Walk;
				_context.CharacterContext.InputData.DirectionWorld = vecToTarget;
				return;
			}

			_context.CharacterContext.InputData.Command = CharacterCommand.AttackByIndex;
			_context.CharacterContext.InputData.SpecialAttackIndex = nextAttackIndex;
			_data.AttackIndex++;
			if(_data.AttackIndex >= _attackSequencesFirstPhase[_data.AttackSequenceKey].Count)
			{
				_data.AttackIndex = 0;
			}
			_data.IsAttackInProcess = true;
		}

		private void ThinkSecondPhase()
		{
		}

		public override void Reset()
		{
			base.Reset();
			_data = default;
		}
	}
}
