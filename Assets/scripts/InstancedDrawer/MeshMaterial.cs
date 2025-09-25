using UnityEngine;

namespace Game.GameView.InstancedDrawer
{
	public class MeshMaterial
	{
		public Mesh Mesh;
		public Material Material;

		public MeshMaterial()
		{
		}

		public MeshMaterial(Mesh mesh, Material material)
		{
			Material = material;
			Mesh = mesh;
		}
	}
}
