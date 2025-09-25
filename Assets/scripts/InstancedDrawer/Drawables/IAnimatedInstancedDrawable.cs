using UnityEngine;

namespace Game.GameView.InstancedDrawer.Drawables
{
	public interface IAnimatedInstancedDrawable
	{
		string DrawInstanceId { get; }
		bool Enabled { get; }
		MeshMaterial MeshMaterial { get; }
		Matrix4x4 Matrix { get; }
		float AnimationTime { get; }
	}
}
