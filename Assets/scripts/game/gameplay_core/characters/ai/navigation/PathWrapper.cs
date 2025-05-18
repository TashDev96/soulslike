using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace game.gameplay_core.characters.ai.navigation
{
	public class PathWrapper
	{
		private const float CatmullRomTension = 0.1f;
		private readonly List<Vector3> _positions = new();
		private readonly List<Vector3> _directions = new();
		private readonly List<float> _distances = new();
		public bool IsEmpty => _positions.Count == 0;

		public float Length { get; private set; }

		public IReadOnlyList<Vector3> Positions => _positions;
		public IReadOnlyList<Vector3> Directions => _directions;
		public IReadOnlyList<float> Distances => _distances;

		public void SetPath(NavMeshPath path, bool smooth = true)
		{
			_positions.Clear();
			_directions.Clear();
			_distances.Clear();
			var corners = path.corners;
			if(smooth)
			{
				corners = GenerateSpline(corners, 5).ToArray();
			}

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

		public static List<Vector3> GenerateSpline(Vector3[] points, int resolution)
		{
			if(points.Length < 2)
			{
				Debug.LogWarning("At least two points are required to generate a spline.");
				return new List<Vector3>();
			}

			var splinePoints = new List<Vector3>();

			var extendedPoints = new Vector3[points.Length + 2];
			extendedPoints[0] = points[0];
			extendedPoints[extendedPoints.Length - 1] = points[points.Length - 1];
			for(var i = 0; i < points.Length; i++)
			{
				extendedPoints[i + 1] = points[i];
			}

			for(var i = 0; i < extendedPoints.Length - 3; i++)
			{
				var p0 = extendedPoints[i];
				var p1 = extendedPoints[i + 1];
				var p2 = extendedPoints[i + 2];
				var p3 = extendedPoints[i + 3];

				for(var j = 0; j < resolution; j++)
				{
					var t = j / (float)resolution;
					splinePoints.Add(CalculateCatmullRomPoint(p0, p1, p2, p3, t));
				}
			}

			splinePoints.Add(extendedPoints[extendedPoints.Length - 2]);

			return splinePoints;
		}

		private static Vector3 CalculateCatmullRomPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			var t2 = t * t;
			var t3 = t2 * t;

			var b0 = -CatmullRomTension * t3 + 2 * CatmullRomTension * t2 - CatmullRomTension * t;
			var b1 = (2 - CatmullRomTension) * t3 + (CatmullRomTension - 3) * t2 + 1;
			var b2 = (CatmullRomTension - 2) * t3 + (3 - 2 * CatmullRomTension) * t2 + CatmullRomTension * t;
			var b3 = CatmullRomTension * t3 - CatmullRomTension * t2;

			return b0 * p0 + b1 * p1 + b2 * p2 + b3 * p3;
		}
	}
}
