using System.Collections.Generic;
using dream_lib.src.utils.data_types;
using UnityEngine;
using UnityEngine.AI;

namespace game.gameplay_core.characters.ai.navigation
{
	public class AiNavigationModule
	{
		private readonly ReadOnlyTransform _characterTransform;

		private readonly NavMeshPath _navMeshPath;

		private int _currentIndex;

		private float _currentLength;

		public AiNavigationModule(ReadOnlyTransform characterTransform)
		{
			Path = new PathWrapper();
			_navMeshPath = new NavMeshPath();
			_characterTransform = characterTransform;
		}

		public PathWrapper Path { get; }

		public void BuildPath(Vector3 targetPosition)
		{
			NavMesh.CalculatePath(_characterTransform.Position, targetPosition, NavMesh.AllAreas, _navMeshPath);
			if(_navMeshPath.status != NavMeshPathStatus.PathInvalid)
			{
				Path.SetPath(_navMeshPath);
			}

			_currentLength = 0;
			_currentIndex = 0;
		}

		private void SampleByLength(float length, out Vector3 position, out Vector3 direction)
		{
			var iLength = 0f;
			for(int i = 0; i < Path.Positions.Count-1; i++)
			{
				var cornerDistance = Path.Distances[i]; 
				if(iLength+cornerDistance >= length)
				{
					position = Vector3.Lerp(Path.Positions[i], Path.Positions[i + 1], (length - iLength) / cornerDistance);
					direction = Path.Directions[i];
					return;
				}
				
				iLength += Path.Distances[i];
			}

			position = Path.Positions[^1];
			direction = Path.Directions[^1];
		}

		public Vector3 CalculateMoveDirection(Vector3 currentPosition)
		{
			SampleByLength(_currentLength, out var targetPos, out _);
			if((targetPos - currentPosition).sqrMagnitude < 0.1f*0.1f)
			{
				_currentLength += 0.2f;
				SampleByLength(_currentLength, out targetPos, out _);
			}

			return (targetPos - currentPosition).normalized;
		}
		

	}
}