using UnityEngine;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;

namespace SkinnedMeshInstancing.AnimationBaker
{
	public class MeshSequencePlayer : MonoBehaviour
	{
		[SerializeField] private BakedMeshSequence _meshSequence;
		[SerializeField] private MeshFilter _meshFilter;
		[SerializeField] private string _currentClipId;
		[SerializeField] private bool _autoPlay = true;
		[SerializeField] private bool _loop = true;
		
		private float _currentTime;
		private bool _isPlaying;
		
		private void Start()
		{
			if (_autoPlay)
				Play(_currentClipId);
		}
		
		private void Update()
		{
			if (!_isPlaying || _meshSequence == null)
				return;
				
			_currentTime += Time.deltaTime;
			
			if (!string.IsNullOrEmpty(_currentClipId))
			{
				var clipDuration = _meshSequence.GetClipDuration(_currentClipId);
				if (_currentTime >= clipDuration)
				{
					if (_loop)
					{
						_currentTime = 0f;
					}
					else
					{
						_isPlaying = false;
						return;
					}
				}
			}
			
			UpdateMesh();
		}
		
		public void Play(string clipId = null)
		{
			if (_meshSequence == null)
				return;
				
			_currentClipId = clipId;
			_currentTime = 0f;
			_isPlaying = true;
			UpdateMesh();
		}
		
		public void Stop()
		{
			_isPlaying = false;
		}
		
		public void SetTime(float time)
		{
			_currentTime = time;
			UpdateMesh();
		}
		
		public void SetFrame(int frameIndex)
		{
			if (_meshSequence == null)
				return;
				
			var mesh = _meshSequence.GetMeshAtFrame(frameIndex);
			if (_meshFilter != null && mesh != null)
				_meshFilter.mesh = mesh;
		}
		
		private void UpdateMesh()
		{
			if (_meshSequence == null || _meshFilter == null)
				return;
				
			var mesh = _meshSequence.GetMeshAtTime(_currentTime, _currentClipId);
			if (mesh != null)
				_meshFilter.mesh = mesh;
		}
		
		public float GetCurrentTime() => _currentTime;
		public bool IsPlaying() => _isPlaying;
		public string GetCurrentClip() => _currentClipId;
	}
}

