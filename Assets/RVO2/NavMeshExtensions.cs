using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Core.Client.Helpers
{
	public static class NavMeshExtensions
	{
		public static float CalculateLength(this NavMeshPath path)
		{
			var result = 0f;
			var corners = path.corners;
			for(var i = 1; i < corners.Length; i++)
			{
				result += (corners[i - 1] - corners[i]).magnitude;
			}
			return result;
		}

		public static Vector3 GetPositionInsideNavmesh(Vector3 position, float maxDisplacement = 10f)
		{
			if(NavMesh.SamplePosition(position, out var hit, maxDisplacement, NavMesh.AllAreas))
			{
				return hit.position;
			}

			return position;
		}

		public static Vector3[] CalculatePathWithControlPoints(List<Vector3> controlPoints)
		{
			if(controlPoints.Count < 2)
			{
				throw new Exception("not enough points to calculate path");
			}

			var results = new List<Vector3>();

			var nextIndex = 1;
			var startPoint = GetPositionInsideNavmesh(controlPoints[0]);
			var path = new NavMeshPath();

			while(nextIndex < controlPoints.Count)
			{
				var endPoint = GetPositionInsideNavmesh(controlPoints[nextIndex]);
				var pathFound = NavMesh.CalculatePath(startPoint, endPoint, NavMesh.AllAreas, path);
				if(pathFound)
				{
					for(var i = 0; i < path.corners.Length - 1; i++)
					{
						var corner = path.corners[i];
						results.Add(corner);
					}
					startPoint = endPoint;
				}
				nextIndex++;
			}

			results.Add(startPoint);
			return results.ToArray();
		}

		 
	}
}
