using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace experiments
{
	public class OptimizedCapsuleCollider
	{
		public Vector3 point1;
		public Vector3 point2;
		public bool SkipCollisions;
		public List<OptimizedCapsuleCollider> DontCollideWith = new List<OptimizedCapsuleCollider>();

		public Vector3 Center { get; private set; }

		public Vector3 Direction => Vector3.up;
		public float Height { get; private set; }

		public float Radius { get; private set; }

		public IReadOnlyList<OptimizedCapsuleCollider> AllColliders { get; private set; }

		public OptimizedCapsuleCollider(Vector3 center, float height, float radius)
		{
			Center = center;
			Height = height;
			Radius = radius;

			CalculatePoints();
		}

		public bool ContainsPoint(Vector3 point)
		{
			var closestPoint = GetClosestPointOnLine(point);
			return Vector3.Distance(point, closestPoint) <= Radius;
		}

		public Vector3 GetClosestPointOnLine(Vector3 point)
		{
			var lineDirection = point2 - point1;
			var lineLength = lineDirection.magnitude;

			if(lineLength < 0.0001f)
			{
				return point1;
			}

			lineDirection /= lineLength;
			var pointToStart = point - point1;
			var projection = Vector3.Dot(pointToStart, lineDirection);

			projection = Mathf.Clamp(projection, 0f, lineLength);
			return point1 + lineDirection * projection;
		}

		public Vector3 GetClosestPointOnSurface(Vector3 point)
		{
			var closestOnLine = GetClosestPointOnLine(point);
			var directionToPoint = (point - closestOnLine).normalized;
			return closestOnLine + directionToPoint * Radius;
		}

		public bool IntersectsCapsule(OptimizedCapsuleCollider other)
		{
			var distance = GetDistanceBetweenLines(point1, point2, other.point1, other.point2);
			return distance <= Radius + other.Radius;
		}

		public bool IntersectsSphere(Vector3 sphereCenter, float sphereRadius)
		{
			var closestPoint = GetClosestPointOnLine(sphereCenter);
			var distance = Vector3.Distance(sphereCenter, closestPoint);
			return distance <= Radius + sphereRadius;
		}

		public Bounds GetBounds()
		{
			var min = Vector3.Min(point1, point2) - Vector3.one * Radius;
			var max = Vector3.Max(point1, point2) + Vector3.one * Radius;

			var bounds = new Bounds();
			bounds.SetMinMax(min, max);
			return bounds;
		}

		public void Translate(Vector3 translation)
		{
			Center += translation;
			CalculatePoints();
		}

		public void Scale(float scale)
		{
			Height *= scale;
			Radius *= scale;
			CalculatePoints();
		}

		public void SetCenter(Vector3 newCenter)
		{
			Center = newCenter;
			CalculatePoints();
		}

		public void SetHeight(float newHeight)
		{
			Height = newHeight;
			CalculatePoints();
		}

		public void SetRadius(float newRadius)
		{
			Radius = newRadius;
		}

		public OptimizedCapsuleCollider Scaled(float scale)
		{
			var result = this;
			result.Scale(scale);
			return result;
		}

		public OptimizedCapsuleCollider Translated(Vector3 translation)
		{
			var result = this;
			result.Translate(translation);
			return result;
		}

		public bool CapsuleCast(Vector3 direction, float maxDistance, out RaycastHit hitInfo, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			return Physics.CapsuleCast(point1, point2, Radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
		}

		public bool CapsuleCast(Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			return Physics.CapsuleCast(point1, point2, Radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
		}

		public int CapsuleCastNonAlloc(Vector3 direction, RaycastHit[] results, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			return Physics.CapsuleCastNonAlloc(point1, point2, Radius, direction, results, maxDistance, layerMask, queryTriggerInteraction);
		}

		public Collider[] OverlapCapsule(int layerMask = Physics.AllLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			return Physics.OverlapCapsule(point1, point2, Radius, layerMask, queryTriggerInteraction);
		}

		public int OverlapCapsuleNonAlloc(Collider[] results, int layerMask = Physics.AllLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			return Physics.OverlapCapsuleNonAlloc(point1, point2, Radius, results, layerMask, queryTriggerInteraction);
		}

		public bool CheckCapsule(int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			return Physics.CheckCapsule(point1, point2, Radius, layerMask, queryTriggerInteraction);
		}

		public CollisionInfo CollideWith(OptimizedCapsuleCollider other)
		{
			var distance = GetDistanceBetweenLines(point1, point2, other.point1, other.point2);
			var combinedRadius = Radius + other.Radius;

			if(distance >= combinedRadius)
			{
				return new CollisionInfo { IsColliding = false };
			}

			var penetrationDepth = combinedRadius - distance;
			var (thisClosest, otherClosest) = GetClosestPointsBetweenLines(point1, point2, other.point1, other.point2);

			var separationDirection = (thisClosest - otherClosest).normalized;
			if(separationDirection == Vector3.zero)
			{
				separationDirection = Vector3.up;
			}

			var contactPoint = (thisClosest + otherClosest) * 0.5f;

			return new CollisionInfo
			{
				IsColliding = true,
				PenetrationDepth = penetrationDepth,
				SeparationDirection = separationDirection,
				ContactPoint = contactPoint,
				ThisClosestPoint = thisClosest,
				OtherClosestPoint = otherClosest
			};
		}

		public void SetAllColliders(IReadOnlyList<OptimizedCapsuleCollider> allColliders)
		{
			AllColliders = allColliders;
		}

		public List<OptimizedCapsuleCollider> GetIntersectingColliders()
		{
			if(AllColliders == null)
			{
				return new List<OptimizedCapsuleCollider>();
			}

			return AllColliders.Where(other => other != this && IntersectsCapsule(other)).ToList();
		}

		public List<OptimizedCapsuleCollider> GetCollidersInRange(float range)
		{
			if(AllColliders == null)
			{
				return new List<OptimizedCapsuleCollider>();
			}

			var expandedBounds = GetBounds();
			expandedBounds.Expand(range * 2f);

			return AllColliders.Where(other =>
				other != this &&
				expandedBounds.Intersects(other.GetBounds()) &&
				GetDistanceBetweenLines(point1, point2, other.point1, other.point2) <= range
			).ToList();
		}

		public OptimizedCapsuleCollider GetNearestCollider()
		{
			if(AllColliders == null || AllColliders.Count <= 1)
			{
				return null;
			}

			OptimizedCapsuleCollider nearest = null;
			var nearestDistance = float.MaxValue;

			foreach(var other in AllColliders)
			{
				if(other == this)
				{
					continue;
				}

				var distance = GetDistanceBetweenLines(point1, point2, other.point1, other.point2);
				if(distance < nearestDistance)
				{
					nearestDistance = distance;
					nearest = other;
				}
			}

			return nearest;
		}

		public List<CollisionInfo> GetAllCollisions()
		{
			var collisions = new List<CollisionInfo>();
			if(AllColliders == null)
			{
				return collisions;
			}

			foreach(var other in AllColliders)
			{
				if(other == this)
				{
					continue;
				}

				var collision = CollideWith(other);
				if(collision.IsColliding)
				{
					collisions.Add(collision);
				}
			}

			return collisions;
		}

		public bool OptimizedCapsuleCast(Vector3 direction, float maxDistance, out CapsuleCastHit hitInfo)
		{
			hitInfo = CapsuleCastHit.NoHit;

			if(AllColliders == null || direction == Vector3.zero)
			{
				return false;
			}

			direction = direction.normalized;
			var closestHit = CapsuleCastHit.NoHit;
			var closestDistance = float.MaxValue;

			foreach(var other in AllColliders)
			{
				if(other == this || other.SkipCollisions || DontCollideWith.Contains(other))
				{
					continue;
				}

				if(CollideWith(other).IsColliding)
				{
					continue;
				}

				var hit = CastAgainstCollider(other, direction, maxDistance);
				if(hit.hasHit && hit.distance < closestDistance)
				{
					closestDistance = hit.distance;
					closestHit = hit;
				}
			}

			if(closestHit.hasHit)
			{
				hitInfo = closestHit;
				return true;
			}

			return false;
		}

		public int OptimizedCapsuleCastNonAlloc(Vector3 direction, float maxDistance, CapsuleCastHit[] results)
		{
			if(AllColliders == null || direction == Vector3.zero || results == null || results.Length == 0)
			{
				return 0;
			}

			direction = direction.normalized;
			var hitCount = 0;
			var tempHits = new List<CapsuleCastHit>();

			foreach(var other in AllColliders)
			{
				if(other == this)
				{
					continue;
				}

				var hit = CastAgainstCollider(other, direction, maxDistance);
				if(hit.hasHit)
				{
					tempHits.Add(hit);
				}
			}

			tempHits.Sort((a, b) => a.distance.CompareTo(b.distance));

			var maxResults = Mathf.Min(tempHits.Count, results.Length);
			for(var i = 0; i < maxResults; i++)
			{
				results[i] = tempHits[i];
				hitCount++;
			}

			return hitCount;
		}

		public CapsuleCastHit[] OptimizedCapsuleCastAll(Vector3 direction, float maxDistance)
		{
			if(AllColliders == null || direction == Vector3.zero)
			{
				return new CapsuleCastHit[0];
			}

			direction = direction.normalized;
			var hits = new List<CapsuleCastHit>();

			foreach(var other in AllColliders)
			{
				if(other == this)
				{
					continue;
				}

				var hit = CastAgainstCollider(other, direction, maxDistance);
				if(hit.hasHit)
				{
					hits.Add(hit);
				}
			}

			hits.Sort((a, b) => a.distance.CompareTo(b.distance));
			return hits.ToArray();
		}

		private void CalculatePoints()
		{
			var halfHeight = (Height - Radius * 2f) * 0.5f;
			halfHeight = Mathf.Max(0f, halfHeight);

			point1 = Center + Vector3.up * halfHeight;
			point2 = Center - Vector3.up * halfHeight;
		}

		private static float GetDistanceBetweenLines(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
		{
			var u = line1End - line1Start;
			var v = line2End - line2Start;
			var w = line1Start - line2Start;

			var a = Vector3.Dot(u, u);
			var b = Vector3.Dot(u, v);
			var c = Vector3.Dot(v, v);
			var d = Vector3.Dot(u, w);
			var e = Vector3.Dot(v, w);

			var denominator = a * c - b * b;
			float s, t;

			if(denominator < 0.0001f)
			{
				s = 0f;
				t = b > c ? d / b : e / c;
			}
			else
			{
				s = (b * e - c * d) / denominator;
				t = (a * e - b * d) / denominator;
			}

			s = Mathf.Clamp01(s);
			t = Mathf.Clamp01(t);

			var closestPoint1 = line1Start + s * u;
			var closestPoint2 = line2Start + t * v;

			return Vector3.Distance(closestPoint1, closestPoint2);
		}

		private static (Vector3 point1, Vector3 point2) GetClosestPointsBetweenLines(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
		{
			var u = line1End - line1Start;
			var v = line2End - line2Start;
			var w = line1Start - line2Start;

			var a = Vector3.Dot(u, u);
			var b = Vector3.Dot(u, v);
			var c = Vector3.Dot(v, v);
			var d = Vector3.Dot(u, w);
			var e = Vector3.Dot(v, w);

			var denominator = a * c - b * b;
			float s, t;

			if(denominator < 0.0001f)
			{
				s = 0f;
				t = b > c ? d / b : e / c;
			}
			else
			{
				s = (b * e - c * d) / denominator;
				t = (a * e - b * d) / denominator;
			}

			s = Mathf.Clamp01(s);
			t = Mathf.Clamp01(t);

			var closestPoint1 = line1Start + s * u;
			var closestPoint2 = line2Start + t * v;

			return (closestPoint1, closestPoint2);
		}

		private CapsuleCastHit CastAgainstCollider(OptimizedCapsuleCollider target, Vector3 direction, float maxDistance)
		{
			var hit = CapsuleCastHit.NoHit;

			var sweepSteps = Mathf.Max(1, Mathf.CeilToInt(maxDistance / (Radius * 0.5f)));
			var stepDistance = maxDistance / sweepSteps;

			for(var step = 0; step <= sweepSteps; step++)
			{
				var currentDistance = step * stepDistance;
				var testPosition = Center + direction * currentDistance;

				var testCapsule = new OptimizedCapsuleCollider(testPosition, Height, Radius);

				if(testCapsule.IntersectsCapsule(target))
				{
					var collision = testCapsule.CollideWith(target);
					if(collision.IsColliding)
					{
						hit.hasHit = true;
						hit.collider = target;
						hit.distance = currentDistance;
						hit.point = collision.ContactPoint;
						hit.contactPoint = collision.ContactPoint;
						hit.normal = -collision.SeparationDirection;

						if(step > 0)
						{
							var refinedHit = RefineCastHit(target, direction, (step - 1) * stepDistance, currentDistance);
							if(refinedHit.hasHit)
							{
								hit = refinedHit;
							}
						}

						break;
					}
				}
			}

			return hit;
		}

		private CapsuleCastHit RefineCastHit(OptimizedCapsuleCollider target, Vector3 direction, float minDistance, float maxDistance)
		{
			const int refinementSteps = 8;
			var stepSize = (maxDistance - minDistance) / refinementSteps;

			for(var i = 0; i <= refinementSteps; i++)
			{
				var testDistance = minDistance + i * stepSize;
				var testPosition = Center + direction * testDistance;
				var testCapsule = new OptimizedCapsuleCollider(testPosition, Height, Radius);

				if(testCapsule.IntersectsCapsule(target))
				{
					var collision = testCapsule.CollideWith(target);
					return new CapsuleCastHit
					{
						hasHit = true,
						collider = target,
						distance = testDistance,
						point = collision.ContactPoint,
						contactPoint = collision.ContactPoint,
						normal = -collision.SeparationDirection
					};
				}
			}

			return CapsuleCastHit.NoHit;
		}
	}

	public struct CollisionInfo
	{
		public bool IsColliding;
		public float PenetrationDepth;
		public Vector3 SeparationDirection;
		public Vector3 ContactPoint;
		public Vector3 ThisClosestPoint;
		public Vector3 OtherClosestPoint;
	}

	public struct CapsuleCastHit
	{
		public OptimizedCapsuleCollider collider;
		public Vector3 point;
		public Vector3 normal;
		public float distance;
		public Vector3 contactPoint;
		public bool hasHit;

		public static CapsuleCastHit NoHit => new() { hasHit = false };
	}
}
