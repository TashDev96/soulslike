using System.Collections.Generic;
using System.Linq;
using Core.Client.Helpers;
using dream_lib.src.extensions;
using Game.GameView.InstancedDrawer;
using Game.GameView.InstancedDrawer.Drawables;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;
using UnityEngine;

namespace SkinnedMeshInstancing.AnimationBaker.AnimationController
{
	public class BakedAnimationController : IAnimatedInstancedDrawable
	{
		private readonly BakedAnimationData _bakedAnimationData;
		private readonly Dictionary<int, BakedAnimationClip> _animationClips = new();
		private BakedAnimationClip _currentClip;
		private AnimationClipPlayInfo _currentClipInfo;

		private readonly Queue<AnimationClipPlayInfo> _clipsQueue = new();

		public string DrawInstanceId { get; }
		public bool Enabled { get; private set; }
		public MeshMaterial MeshMaterial { get; }
		public Matrix4x4 Matrix { get; private set; }
		public float AnimationTime { get; private set; }
		public Vector3 Position => Matrix.GetPosition();

		public BakedAnimationController(string drawInstanceId, BakedAnimationData bakedAnimationData, MeshMaterial meshMaterial)
		{
			DrawInstanceId = drawInstanceId;
			_bakedAnimationData = bakedAnimationData;
			MeshMaterial = meshMaterial;

			foreach(var animationClip in _bakedAnimationData.AnimationClips)
			{
				_animationClips[Animator.StringToHash(animationClip.Id)] = animationClip;
			}
			PlayAnimation(Animator.StringToHash(_animationClips.Values.First().Id));

			Enabled = true;
		}

		public void PlayAnimation(int animationHash, int loopsCount = -1)
		{
			_currentClipInfo = new AnimationClipPlayInfo(animationHash, loopsCount);
			_currentClip = _animationClips[animationHash];
			AnimationTime = _currentClip.Start;
		}

		public void AddAnimationToQueue(int animationHash, int loopsCount)
		{
			_currentClipInfo.Loops = 0;
			var clipPLayInfo = new AnimationClipPlayInfo(animationHash, loopsCount);
			_clipsQueue.Enqueue(clipPLayInfo);
		}

		public void PlayRandomAnimation()
		{
			_currentClip = _animationClips.Values.GetRandomElement();
			PlayAnimation(Animator.StringToHash(_currentClip.Id));
		}

		public void TickAnimation(float deltaTime)
		{
			AnimationTime += deltaTime * _currentClip.Speed * _bakedAnimationData.SpeedMultiplier;
			if(AnimationTime >= _currentClip.End)
			{
				switch(_currentClipInfo.Loops)
				{
					case 1:
					case 0:
					{
						if(_clipsQueue.TryDequeue(out var newClipInfo))
						{
							PlayAnimation(newClipInfo.ClipNameHash, newClipInfo.Loops);
						}
						else
						{
							_currentClipInfo.Loops = -2;
						}
					}
						break;
					case -1:
					{
						AnimationTime = _currentClip.Start;
					}
						break;
					case > 1:
					{
						AnimationTime = _currentClip.Start;
						_currentClipInfo.Loops--;
					}
						break;
					case -2:
					{
						AnimationTime = _currentClip.End;
					}
						break;
				}
			}
		}

		public void SetActive(bool active)
		{
			Enabled = active;
		}

		public void SetTrsMatrix(Matrix4x4 matrix)
		{
			Matrix = matrix;
		}
	}
}
