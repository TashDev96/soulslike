using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace game.gameplay_core.characters.view
{
	[Serializable]
	public class BlinkView
	{
		private static readonly int BlinkIntensityId = Shader.PropertyToID("_BlinkIntensity");
		private static readonly int BlinkColorId = Shader.PropertyToID("_BlinkColor");

		[SerializeField]
		private Color _blinkColor = Color.white;

		[SerializeField]
		private int _blinkCount = 3;

		[SerializeField]
		private float _blinkDuration = 0.3f;

		[SerializeField]
		private MeshRenderer[] _blinkMeshes;

		private UniTask _blinkTask;

		private CancellationTokenSource _cancellationToken;

		private MaterialPropertyBlock _propertyBlock;

		public void Initialize()
		{
			_propertyBlock = new MaterialPropertyBlock();
			_cancellationToken = new CancellationTokenSource();
		}

		public void PlayDamageBlink()
		{
			StopBlink();
			_blinkTask = BlinkCoroutine();
		}

		public void Dispose()
		{
			StopBlink();
		}

		private void StopBlink()
		{
			if(_blinkTask.Status == UniTaskStatus.Pending)
			{
				_cancellationToken.Cancel();
			}
		}

		private async UniTask BlinkCoroutine()
		{
			var singleBlinkDuration = _blinkDuration / _blinkCount;
			var halfBlinkDuration = singleBlinkDuration * 0.5f;

			_propertyBlock.SetColor(BlinkColorId, _blinkColor);

			for(var i = 0; i < _blinkCount; i++)
			{
				_propertyBlock.SetFloat(BlinkIntensityId, 1.2f);
				WritePropertyToMeshes();
				await UniTask.Delay(TimeSpan.FromSeconds(halfBlinkDuration),
					cancellationToken: _cancellationToken.Token);

				_propertyBlock.SetFloat(BlinkIntensityId, 0f);
				WritePropertyToMeshes();
				await UniTask.Delay(TimeSpan.FromSeconds(halfBlinkDuration),
					cancellationToken: _cancellationToken.Token);
			}

			_propertyBlock.SetFloat(BlinkIntensityId, 0f);
			WritePropertyToMeshes();
		}
		
		private void WritePropertyToMeshes()
		{
			foreach(var meshRenderer in _blinkMeshes)
			{
				meshRenderer.SetPropertyBlock(_propertyBlock);
			}
		}
	}
}
