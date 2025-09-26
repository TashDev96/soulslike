using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkinnedMeshInstancing.AnimationBaker.Editor
{
	public class AnimationMeshBakerTest : MonoBehaviour
	{
		[SerializeField]
		private Mesh _originalMesh;
		[SerializeField]
		private MeshFilter _meshFilter;
		[SerializeField]
		private float _boundSize;
		[SerializeField]
		private int _frame;
		[SerializeField]
		private Texture2D _posTexture;
		[SerializeField]
		private Texture2D _normalTexture;
		[SerializeField]
		private int _vertexIndexOffset;

		[ContextMenu("Render")]
		private void Render()
		{
			var mesh = new Mesh();
			mesh.SetVertices(_originalMesh.vertices);
			mesh.SetTriangles(_originalMesh.triangles, 0);

			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			for(var i = 0; i < mesh.vertexCount; i++)
			{
				var color = _posTexture.GetPixel(i, _frame);
				var x = color.r * 2f * _boundSize - _boundSize;
				var y = color.g * 2f * _boundSize - _boundSize;
				var z = color.b * 2f * _boundSize - _boundSize;
				vertices.Add(new Vector3(x, y, z));

				var normalColor = _normalTexture.GetPixel(i + _vertexIndexOffset, _frame);
				var nx = normalColor.r * 2f - 1;
				var ny = normalColor.g * 2f - 1;
				var nz = normalColor.b * 2f - 1;
				normals.Add(new Vector3(nx, ny, nz));

				Debug.DrawRay(vertices.Last(), Vector3.up * 0.2f, color, 0.5f);
			}
			mesh.SetVertices(vertices.ToArray());
			mesh.SetNormals(normals);
			_meshFilter.sharedMesh = mesh;
		}
	}
}
