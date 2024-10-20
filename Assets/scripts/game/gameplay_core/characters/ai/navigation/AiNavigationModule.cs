using dream_lib.src.utils.data_types;
using UnityEngine;
using UnityEngine.AI;

namespace game.gameplay_core.characters.ai
{
	public class AiNavigationModule
	{
		private readonly ReadOnlyTransform _characterTransform;

		private readonly NavMeshPath _navMeshPath;

		private int _currentIndex;
		private float _currentDistanceInsideSegment;

		public PathWrapper Path { get; }
		public Vector3 ReferencePos { get; private set; }

		public AiNavigationModule(ReadOnlyTransform characterTransform)
		{
			Path = new PathWrapper();
			_navMeshPath = new NavMeshPath();
			_characterTransform = characterTransform;
		}

		public void BuildPath(Vector3 targetPosition)
		{
			NavMesh.CalculatePath(_characterTransform.Position, targetPosition, NavMesh.AllAreas, _navMeshPath);
			if(_navMeshPath.status != NavMeshPathStatus.PathInvalid)
			{
				Path.SetPath(_navMeshPath);
			}
			_currentDistanceInsideSegment = 0.5f;
			_currentIndex = 0;
		}

		public Vector3 CalculateMoveDirection(Vector3 currentPosition, float speed)
		{
			var speedOffset = speed / 30f;

			if((currentPosition - ReferencePos).sqrMagnitude < 0.2f + speedOffset)
			{
				_currentDistanceInsideSegment += 0.5f + speedOffset;
			}

			if(_currentIndex >= Path.Positions.Count - 1)
			{
				return (Path.Positions[^1] - currentPosition).normalized;
			}

			var point = Path.Positions[_currentIndex];
			var nextPoint = Path.Positions[_currentIndex + 1];
			var normalizedDistance = Mathf.Clamp01(_currentDistanceInsideSegment / Path.Distances[_currentIndex]);

			ReferencePos = Vector3.MoveTowards(point, nextPoint, normalizedDistance);

			if(normalizedDistance >= 1)
			{
				_currentIndex++;
				_currentDistanceInsideSegment = 0;
			}

			return (ReferencePos - currentPosition).normalized;
		}
	}
}