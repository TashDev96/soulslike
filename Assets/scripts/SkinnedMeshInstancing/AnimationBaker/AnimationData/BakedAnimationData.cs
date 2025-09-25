using System.Collections.Generic;
using UnityEngine;

namespace SkinnedMeshInstancing.AnimationBaker.AnimationData
{
	public class BakedAnimationData : ScriptableObject
	{
		public int TextureWidth;
		public float SpeedMultiplier;
		public float BoundSize;
		public float Scale;
		public Vector3 BoundCenter;
		public List<BakedAnimationClip> AnimationClips;
		public Mesh Mesh;
		public Material Material;
	}
}
