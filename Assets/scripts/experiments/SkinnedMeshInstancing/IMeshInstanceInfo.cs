using UnityEngine;

namespace SkinnedMeshInstancing
{
	public interface IMeshInstanceInfo
	{
		Matrix4x4 GetTransformMatrix();
		Mesh GetCurrentMesh();
		Material GetMaterial();
		int GetLayer();
		bool IsVisible();
		void UpdateInstance(float deltaTime);
	}
}
