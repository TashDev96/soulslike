using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.GameView.InstancedDrawer.Drawables;
using UnityEngine;

namespace Game.GameView.InstancedDrawer
{
	public class InstancedDrawManager : MonoBehaviour
	{
		private readonly Dictionary<string, List<IAnimatedInstancedDrawable>> _animatedDrawables = new();
		private readonly Dictionary<string, MeshMaterial> _meshes = new();

		private readonly float[] _tempAnimationArray32 = new float[32];
		private readonly float[] _tempAnimationArray16 = new float[16];
		private readonly float[] _tempAnimationArray8 = new float[8];
		private readonly float[] _tempAnimationArray4 = new float[4];

		private readonly Matrix4x4[] _tempMatrixArray32 = new Matrix4x4[32];
		private readonly Matrix4x4[] _tempMatrixArray16 = new Matrix4x4[16];
		private readonly Matrix4x4[] _tempMatrixArray8 = new Matrix4x4[8];
		private readonly Matrix4x4[] _tempMatrixArray4 = new Matrix4x4[4];

		private readonly MaterialPropertyBlock _materialPropertyBlock = new();
		private readonly int AnimationProperty = Shader.PropertyToID("_Animation");

		public void AddAnimatedDrawable(IAnimatedInstancedDrawable animatedInstancedDrawable)
		{
			if(!_animatedDrawables.ContainsKey(animatedInstancedDrawable.DrawInstanceId))
			{
				_animatedDrawables.Add(animatedInstancedDrawable.DrawInstanceId, new List<IAnimatedInstancedDrawable>());
				_meshes.Add(animatedInstancedDrawable.DrawInstanceId, animatedInstancedDrawable.MeshMaterial);
			}
			_animatedDrawables[animatedInstancedDrawable.DrawInstanceId].Add(animatedInstancedDrawable);
		}

		public void RemoveAnimatedDrawable(IAnimatedInstancedDrawable animatedInstancedDrawable)
		{
			_animatedDrawables[animatedInstancedDrawable.DrawInstanceId].Remove(animatedInstancedDrawable);
			if(_animatedDrawables[animatedInstancedDrawable.DrawInstanceId].Count == 0)
			{
				_animatedDrawables.Remove(animatedInstancedDrawable.DrawInstanceId);
				_meshes.Remove(animatedInstancedDrawable.DrawInstanceId);
			}
		}

		public void ClearAll()
		{
			_meshes.Clear();
			_animatedDrawables.Clear();
		}

		private void Update()
		{
			CollectAndDraw();
		}

		private void CollectAndDraw()
		{
			foreach(var (drawableId, drawablesList) in _animatedDrawables)
			{
				var meshMaterial = _meshes[drawableId];
				var count = drawablesList.Count;
				var drawId = 0;
				while(drawId < count)
				{
					var currentDrawsLeft = count - drawId;

					var array = GetTrsArray(currentDrawsLeft);
					var animationArray = GetAnimationArray(currentDrawsLeft);
					var currentArrayLength = array.Length;
					var currentDrawIndex = 0;

					var fallbackMatrix = drawablesList[drawId].Matrix;
					fallbackMatrix.SetTRS(fallbackMatrix.GetPosition(), Quaternion.identity, Vector3.zero);

					while(currentDrawIndex < currentArrayLength)
					{
						if(drawId < count && drawablesList[drawId].Enabled)
						{
							var drawable = drawablesList[drawId];
							array[currentDrawIndex] = drawable.Matrix;
							animationArray[currentDrawIndex] = drawable.AnimationTime;
						}
						else
						{
							array[currentDrawIndex] = fallbackMatrix;
							animationArray[currentDrawIndex] = 0;
						}
						drawId++;
						currentDrawIndex++;
					}
					_materialPropertyBlock.Clear();
					_materialPropertyBlock.SetFloatArray(AnimationProperty, animationArray);
					var drawsCount = Mathf.Min(currentDrawsLeft, currentArrayLength);
					Graphics.DrawMeshInstanced(meshMaterial.Mesh, 0, meshMaterial.Material, array, drawsCount, _materialPropertyBlock);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Matrix4x4[] GetTrsArray(int count)
		{
			if(count < 5)
			{
				return _tempMatrixArray4;
			}
			if(count < 9)
			{
				return _tempMatrixArray8;
			}
			if(count < 17)
			{
				return _tempMatrixArray16;
			}
			return _tempMatrixArray32;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float[] GetAnimationArray(int count)
		{
			if(count < 5)
			{
				return _tempAnimationArray4;
			}
			if(count < 9)
			{
				return _tempAnimationArray8;
			}
			if(count < 17)
			{
				return _tempAnimationArray16;
			}
			return _tempAnimationArray32;
		}
	}
}
