using System.Collections.Generic;
using UnityEngine;

namespace SkinnedMeshInstancing.AnimationBaker.AnimationData
{
	[System.Serializable]
	public class MeshSequenceClip
	{
		public string Id;
		public int StartFrame;
		public int EndFrame;
		public float Speed = 1f;
		
		public int FrameCount => EndFrame - StartFrame + 1;
	}

	[CreateAssetMenu(fileName = "BakedMeshSequence", menuName = "Animation Baker/Baked Mesh Sequence")]
	public class BakedMeshSequence : ScriptableObject
	{
		[Header("Sequence Data")]
		public Mesh[] Meshes;
		public float FramesPerSecond = 30f;
		public List<MeshSequenceClip> AnimationClips;
		
		[Header("Bounds")]
		public Vector3 BoundCenter;
		public float BoundSize;
		
		public Mesh GetMeshAtFrame(int frameIndex)
		{
			if (Meshes == null || Meshes.Length == 0)
				return null;
				
			frameIndex = Mathf.Clamp(frameIndex, 0, Meshes.Length - 1);
			return Meshes[frameIndex];
		}
		
		public Mesh GetMeshAtTime(float time, string clipId = null)
		{
			if (string.IsNullOrEmpty(clipId))
			{
				var frameIndex = Mathf.FloorToInt(time * FramesPerSecond);
				return GetMeshAtFrame(frameIndex);
			}
			
			var clip = GetClip(clipId);
			if (clip == null)
				return null;
				
			var normalizedTime = time * clip.Speed;
			var frameInClip = Mathf.FloorToInt(normalizedTime * FramesPerSecond);
			var actualFrame = clip.StartFrame + (frameInClip % clip.FrameCount);
			
			return GetMeshAtFrame(actualFrame);
		}
		
		public MeshSequenceClip GetClip(string clipId)
		{
			if (AnimationClips == null)
				return null;
				
			return AnimationClips.Find(clip => clip.Id == clipId);
		}
		
		public float GetClipDuration(string clipId)
		{
			var clip = GetClip(clipId);
			if (clip == null)
				return 0f;
				
			return clip.FrameCount / FramesPerSecond / clip.Speed;
		}
	}
}
