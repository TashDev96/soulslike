using UnityEngine;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;

namespace SkinnedMeshInstancing
{
	[System.Serializable]
	public class MeshSequenceInstance : IMeshInstanceInfo
	{
		[SerializeField] private Transform _transform;
		[SerializeField] private BakedMeshSequence _meshSequence;
		[SerializeField] private Material _material;
		[SerializeField] private int _layer = 0;
		[SerializeField] private string _currentClipId;
		[SerializeField] private bool _isPlaying = true;
		[SerializeField] private bool _loop = true;
		[SerializeField] private float _playbackSpeed = 1f;
		[SerializeField] private bool _isVisible = true;
		
		private float _currentTime;
		private Matrix4x4 _cachedMatrix;
		private bool _matrixDirty = true;
		
		public MeshSequenceInstance(Transform transform, BakedMeshSequence meshSequence, Material material)
		{
			_transform = transform;
			_meshSequence = meshSequence;
			_material = material;
			_currentTime = 0f;
		}
		
		public Matrix4x4 GetTransformMatrix()
		{
			if (_transform == null)
				return Matrix4x4.identity;
				
			if (_matrixDirty || _transform.hasChanged)
			{
				_cachedMatrix = _transform.localToWorldMatrix;
				_matrixDirty = false;
				_transform.hasChanged = false;
			}
			
			return _cachedMatrix;
		}
		
		public Mesh GetCurrentMesh()
		{
			if (_meshSequence == null)
				return null;
				
			return _meshSequence.GetMeshAtTime(_currentTime, _currentClipId);
		}
		
		public Material GetMaterial()
		{
			return _material;
		}
		
		public int GetLayer()
		{
			return _layer;
		}
		
		public bool IsVisible()
		{
			return _isVisible && _transform != null && _transform.gameObject.activeInHierarchy;
		}
		
		public void UpdateInstance(float deltaTime)
		{
			if (!_isPlaying || _meshSequence == null)
				return;
				
			_currentTime += deltaTime * _playbackSpeed;
			
			if (!string.IsNullOrEmpty(_currentClipId))
			{
				var clipDuration = _meshSequence.GetClipDuration(_currentClipId);
				if (_currentTime >= clipDuration)
				{
					if (_loop)
					{
						_currentTime = _currentTime % clipDuration;
					}
					else
					{
						_currentTime = clipDuration;
						_isPlaying = false;
					}
				}
			}
		}
		
		public void Play(string clipId = null)
		{
			_currentClipId = clipId;
			_currentTime = 0f;
			_isPlaying = true;
		}
		
		public void Stop()
		{
			_isPlaying = false;
		}
		
		public void Pause()
		{
			_isPlaying = false;
		}
		
		public void Resume()
		{
			_isPlaying = true;
		}
		
		public void SetTime(float time)
		{
			_currentTime = time;
		}
		
		public void SetPlaybackSpeed(float speed)
		{
			_playbackSpeed = speed;
		}
		
		public void SetVisible(bool visible)
		{
			_isVisible = visible;
		}
		
		public void SetMaterial(Material material)
		{
			_material = material;
		}
		
		public void SetLayer(int layer)
		{
			_layer = layer;
		}
		
		public void SetMeshSequence(BakedMeshSequence meshSequence)
		{
			_meshSequence = meshSequence;
		}
		
		public float GetCurrentTime() => _currentTime;
		public bool IsPlaying() => _isPlaying;
		public string GetCurrentClip() => _currentClipId;
		public float GetPlaybackSpeed() => _playbackSpeed;
		public BakedMeshSequence GetMeshSequence() => _meshSequence;
		public Transform GetTransform() => _transform;
	}
}
