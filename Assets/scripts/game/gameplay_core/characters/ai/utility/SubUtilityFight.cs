using dream_lib.src.extensions;
using game.gameplay_core.characters.ai.utility.considerations.utils;
using game.gameplay_core.characters.ai.utility.considerations.value_sources;
using game.gameplay_core.characters.ai.world_reflection;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace game.gameplay_core.characters.ai.utility
{
#if UNITY_EDITOR
	[OdinDontRegister]
#endif
	public class SubUtilityFight : SubUtilityBase
	{
		[SerializeField]
		private PerlinConfig _noAttackWeight;
		private Vector3 _lastVectorToTarget;
		private CharacterObservation _lastTarget;

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

		public override void Think(float deltaTime)
		{
			_context.BlackboardValues[BlackboardValues.NoAttacksWeight] = _noAttackWeight.Evaluate(_context.BrainTime);

			_lastTarget = GetOptimalTarget();
			if(_lastTarget != null)
			{
				_lastVectorToTarget = _context.CharacterContext.Transform.Position - _lastTarget.Position;
				_context.BlackboardValues[BlackboardValues.DistanceToTarget] = _lastVectorToTarget.magnitude;

				var attackRange = _context.BlackboardValues[BlackboardValues.BasicAttackRange];
				var doLockOn = _lastVectorToTarget.sqrMagnitude < attackRange * attackRange + 1f;
				doLockOn |= _lastAction?.Type == UtilityAction.ActionType.KeepSafeDistance;

				_context.CharacterContext.LockOnLogic.HandleLockOnSelectedByAI(doLockOn ? _lastTarget.Character : null);
				base.Think(deltaTime);
			}
		}

		public override float GetExecutionWorthWeight()
		{
			UpdateBlackboardValues();
			if(_lastTarget == null)
			{
				_lastTarget = GetOptimalTarget();
				if(_lastTarget == null)
				{
					return 0;
				}
			}

			const float keepFightDisappearedCharacterTime = 5f;
			const float keepFightOutOfRangeMeters = 5f;

			if(_lastTarget.TimePassed < keepFightDisappearedCharacterTime)
			{
				var vector = _context.CharacterContext.Transform.Position - _lastTarget.Position;
				var range = _context.BlackboardValues[BlackboardValues.BasicAttackRange];

				var result =Mathf.Clamp01(range * range - vector.sqrMagnitude + keepFightOutOfRangeMeters * keepFightOutOfRangeMeters);
				 
				//Debug.DrawLine(_context.CharacterContext.Transform.Position, _lastTarget.Position, Color.red);
				return result;
			}

			return 0;
		}

		public override void Reset()
		{
			base.Reset();
			_lastTarget = null;
			_lastVectorToTarget = default;
		}

		protected override bool TryPerformAction(UtilityAction action)
		{
			if(base.TryPerformAction(action))
			{
				return true;
			}

			var mainAttackDistance = _context.BlackboardValues[BlackboardValues.BasicAttackRange];

			switch(action.Type)
			{
				case UtilityAction.ActionType.GetIntoAttackDistance:
					MoveTo(_lastTarget.Position - _lastVectorToTarget.normalized * mainAttackDistance);
					break;
			}

			return true;
		}

		private CharacterObservation GetOptimalTarget()
		{
			var minDistance = float.MaxValue;
			CharacterObservation result = null;

			foreach(var observation in CharacterObservations)
			{
				if(observation.Character.Context.Team.Value == _context.CharacterContext.Team.Value)
				{
					continue;
				}

				var distanceSq = (_context.CharacterContext.Transform.Position - observation.Position).sqrMagnitude;
				if(distanceSq < minDistance)
				{
					minDistance = distanceSq;
					result = observation;
				}
			}

			return result;
		}
	}
}
