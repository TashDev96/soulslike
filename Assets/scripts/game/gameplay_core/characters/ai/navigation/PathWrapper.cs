using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace game.gameplay_core.characters.ai
{
	public class PathWrapper
	{
		public bool IsEmpty => _positions.Count == 0;
		
		public float Length { get; private set; }

		private List<Vector3> _positions = new();
		private List<Vector3> _directions = new();
		private List<float> _distances = new();

		public IReadOnlyList<Vector3> Positions => _positions;
		public IReadOnlyList<Vector3> Directions => _directions;
		public IReadOnlyList<float> Distances => _distances;

		public void SetPath(NavMeshPath path)
		{
			_positions.Clear();
			_directions.Clear();
			_distances.Clear();
			var corners = path.corners;

			for(var i = 0; i < corners.Length; i++)
			{
				_positions.Add(corners[i]);
				if(i < corners.Length - 1)
				{
					_directions.Add(corners[i + 1] - corners[i]);
					_distances.Add(_directions[^1].magnitude);
					Length += _distances[^1];
				}
			}
		}

		public bool TryGetDirection(int index, out Vector3 direction)
		{
			if(index >= _directions.Count - 1)
			{
				direction = default;
				return false;
			}

			direction = _directions[index];
			return true;
		}
	}
}
