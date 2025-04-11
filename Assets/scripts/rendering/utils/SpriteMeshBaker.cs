using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SpriteMeshBaker : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;
	public Collider collider;
	public MeshFilter targetMeshFilter;
	public float raycastDistance = 10f;
	public float rayOriginOffset = 0.1f;

	[Header("Subdivision Settings")]
	public int subdivisionLevel = 1;
	public bool useSubdivision = true;

	[Header("Correction Settings")]
	public float maxCorrectionRadius = 0.5f;
	public float correctionRadiusStep = 0.1f;
	public float linearStep = 0.05f;

	[Header("Debug")]
	public bool drawDebugRays;
	public float debugDrawDuration = 2f;

	[Button]
	public void Bake()
	{
		if(spriteRenderer == null || collider == null)
		{
			Debug.LogError("SpriteRenderer or Collider not assigned!");
			return;
		}

		var sprite = spriteRenderer.sprite;
		if(sprite == null)
		{
			Debug.LogError("No sprite found in SpriteRenderer!");
			return;
		}

		var spriteVertices = sprite.vertices;
		var spriteUVs = sprite.uv;
		var spriteTriangles = sprite.triangles;

		if(useSubdivision && subdivisionLevel > 0)
		{
			SubdivideMesh(ref spriteVertices, ref spriteUVs, ref spriteTriangles, subdivisionLevel);
		}

		var worldVertices = new Vector3[spriteVertices.Length];
		var finalVertices = new Vector3[spriteVertices.Length];

		var spriteTransform = spriteRenderer.transform;
		var rayDirection = spriteTransform.forward;

		for(var i = 0; i < spriteVertices.Length; i++)
		{
			var vertLocal = new Vector3(spriteVertices[i].x, spriteVertices[i].y, 0);
			worldVertices[i] = spriteTransform.TransformPoint(vertLocal);

			var rayOrigin = worldVertices[i] - rayDirection * rayOriginOffset;

			var hitFound = false;
			var closestHitPoint = worldVertices[i];
			var closestHitDistance = float.MaxValue;

			RaycastHit hit;
			var ray = new Ray(rayOrigin, rayDirection);

			if(drawDebugRays)
			{
				Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.yellow, debugDrawDuration);
			}

			if(collider.Raycast(ray, out hit, raycastDistance))
			{
				hitFound = true;

				// Project hit point onto the original ray direction
				var rayOriginToHit = hit.point - rayOrigin;
				var distanceAlongRay = Vector3.Dot(rayOriginToHit, rayDirection);
				var projectedHitPoint = rayOrigin + rayDirection * distanceAlongRay;

				closestHitPoint = projectedHitPoint;
				closestHitDistance = distanceAlongRay;

				if(drawDebugRays)
				{
					Debug.DrawLine(rayOrigin, hit.point, Color.green, debugDrawDuration);
					Debug.DrawLine(hit.point, projectedHitPoint, Color.magenta, debugDrawDuration);
				}
			}

			if(!hitFound)
			{
				for(var radius = correctionRadiusStep; radius <= maxCorrectionRadius; radius += correctionRadiusStep)
				{
					var circumference = 2f * Mathf.PI * radius;
					var numPoints = Mathf.Max(6, Mathf.FloorToInt(circumference / linearStep));
					var angularStep = 360f / numPoints;

					for(float angle = 0; angle < 360; angle += angularStep)
					{
						var radians = angle * Mathf.Deg2Rad;
						var offset = new Vector3(Mathf.Cos(radians) * radius, Mathf.Sin(radians) * radius, 0);
						offset = spriteTransform.TransformDirection(offset);

						var correctedOrigin = rayOrigin + offset;
						var correctedRay = new Ray(correctedOrigin, rayDirection);

						if(drawDebugRays)
						{
							Debug.DrawRay(correctedOrigin, rayDirection * raycastDistance, Color.cyan, debugDrawDuration);
						}

						if(collider.Raycast(correctedRay, out hit, raycastDistance))
						{
							// Project hit point onto the original ray direction
							var rayOriginToHit = hit.point - rayOrigin;
							var distanceAlongRay = Vector3.Dot(rayOriginToHit, rayDirection);
							var projectedHitPoint = rayOrigin + rayDirection * distanceAlongRay;

							var distance = distanceAlongRay;

							if(distance < closestHitDistance)
							{
								hitFound = true;
								closestHitPoint = projectedHitPoint;
								closestHitDistance = distance;

								if(drawDebugRays)
								{
									Debug.DrawLine(correctedOrigin, hit.point, Color.green, debugDrawDuration);
									Debug.DrawLine(hit.point, projectedHitPoint, Color.magenta, debugDrawDuration);
								}
							}
						}
					}

					if(hitFound)
					{
						break;
					}
				}
			}

			finalVertices[i] = hitFound ? closestHitPoint : worldVertices[i];

			if(!hitFound && drawDebugRays)
			{
				Debug.DrawLine(rayOrigin, worldVertices[i], Color.red, debugDrawDuration);
			}
		}

		var mesh = new Mesh();
		mesh.name = "SpriteMesh";
		mesh.vertices = finalVertices;
		mesh.uv = spriteUVs;
		mesh.triangles = Array.ConvertAll(spriteTriangles, item => (int)item);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		if(targetMeshFilter != null)
		{
			targetMeshFilter.mesh = mesh;

			// Make the mesh transform match the sprite transform's position, rotation, and scale
			targetMeshFilter.transform.position = spriteTransform.position;
			targetMeshFilter.transform.rotation = spriteTransform.rotation;
			targetMeshFilter.transform.localScale = spriteTransform.localScale;

			// Convert vertices from world space to local space relative to the new transform
			var localVertices = new Vector3[finalVertices.Length];
			for(var i = 0; i < finalVertices.Length; i++)
			{
				localVertices[i] = targetMeshFilter.transform.InverseTransformPoint(finalVertices[i]);
			}

			mesh.vertices = localVertices;
			mesh.RecalculateBounds();
		}
		else
		{
			Debug.LogWarning("No target MeshFilter assigned. Mesh was created but not assigned.");
		}
	}

	private void SubdivideMesh(ref Vector2[] vertices, ref Vector2[] uvs, ref ushort[] triangles, int level)
	{
		for(var i = 0; i < level; i++)
		{
			SubdivideOnce(ref vertices, ref uvs, ref triangles);
		}
	}

	private void SubdivideOnce(ref Vector2[] vertices, ref Vector2[] uvs, ref ushort[] triangles)
	{
		var edgeVertexMap = new Dictionary<long, int>();
		var newVertices = new List<Vector2>(vertices);
		var newUvs = new List<Vector2>(uvs);
		var newTriangles = new List<ushort>();

		var numTriangles = triangles.Length / 3;

		for(var t = 0; t < numTriangles; t++)
		{
			var i1 = triangles[t * 3];
			var i2 = triangles[t * 3 + 1];
			var i3 = triangles[t * 3 + 2];

			var m12 = GetMidpointIndex(edgeVertexMap, newVertices, newUvs, i1, i2, vertices, uvs);
			var m23 = GetMidpointIndex(edgeVertexMap, newVertices, newUvs, i2, i3, vertices, uvs);
			var m31 = GetMidpointIndex(edgeVertexMap, newVertices, newUvs, i3, i1, vertices, uvs);

			newTriangles.Add(i1);
			newTriangles.Add(m12);
			newTriangles.Add(m31);

			newTriangles.Add(m12);
			newTriangles.Add(i2);
			newTriangles.Add(m23);

			newTriangles.Add(m31);
			newTriangles.Add(m23);
			newTriangles.Add(i3);

			newTriangles.Add(m12);
			newTriangles.Add(m23);
			newTriangles.Add(m31);
		}

		vertices = newVertices.ToArray();
		uvs = newUvs.ToArray();
		triangles = newTriangles.ToArray();
	}

	private ushort GetMidpointIndex(Dictionary<long, int> edgeVertexMap, List<Vector2> newVertices,
		List<Vector2> newUvs, ushort i1, ushort i2, Vector2[] originalVertices, Vector2[] originalUvs)
	{
		var edgeKey = ((long)Mathf.Min(i1, i2) << 32) | Mathf.Max(i1, i2);

		if(edgeVertexMap.TryGetValue(edgeKey, out var existingIndex))
		{
			return (ushort)existingIndex;
		}

		var midVertex = (originalVertices[i1] + originalVertices[i2]) * 0.5f;
		var midUv = (originalUvs[i1] + originalUvs[i2]) * 0.5f;

		var newIndex = newVertices.Count;
		newVertices.Add(midVertex);
		newUvs.Add(midUv);
		edgeVertexMap[edgeKey] = newIndex;

		return (ushort)newIndex;
	}
}
