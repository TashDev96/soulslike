using game.gameplay_core.characters.ai.utility.considerations.value_sources;
using game.gameplay_core.characters.ai.world_reflection;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility
{
	public class SubUtilityPursue : SubUtilityBase
	{
		private CharacterObservation _lastTarget;
		private Vector3 _lastVectorToTarget;

		public override void Think(float deltaTime)
		{
			_lastTarget = GetOptimalTarget();
			if(_lastTarget != null)
			{
				_context.BlackboardValues[BlackboardValues.DistanceToTarget] = _lastVectorToTarget.magnitude;
				base.Think(deltaTime);
			}
		}

		public override float GetExecutionWorthWeight()
		{
			if(GetOptimalTarget() != null)
			{
				return 0.5f;
			}

			return 0;
		}

		protected override bool TryPerformAction(UtilityAction action)
		{
			if(base.TryPerformAction(action))
			{
				return true;
			}
			switch(action.Type)
			{
				case UtilityAction.ActionType.GetIntoAttackDistance:
					MoveTo(_lastTarget.Position);
					return true;
			}

			return false;
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

				if(observation.Character.Context.IsDead.Value)
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
