using System;
using System.Collections;
using game.gameplay_core.characters.logic;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.damage_system;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.view
{
	public class CharacterBodyView : MonoBehaviour
	{
		[SerializeField]
		private MeshRenderer _bodyMesh;
		
		[SerializeField]
		private Color _blinkColor = Color.white;
		
		[SerializeField]
		private float _blinkDuration = 0.5f;
		
		[SerializeField]
		private int _blinkCount = 3;
		
		private MaterialPropertyBlock _propertyBlock;
		private Coroutine _blinkCoroutine;
		private IDisposable _damageSub;
		private static readonly int BlinkIntensityId = Shader.PropertyToID("_BlinkIntensity");
		private static readonly int BlinkColorId = Shader.PropertyToID("_BlinkColor");
		
		private void Awake()
		{
			_propertyBlock = new MaterialPropertyBlock();
		}

		public void Initizlie()
		{
			
		}
		
		public void PlayDamageBlink()
		{
			if (_blinkCoroutine != null)
			{
				StopCoroutine(_blinkCoroutine);
			}
			_blinkCoroutine = StartCoroutine(BlinkCoroutine());
		}
		
		private IEnumerator BlinkCoroutine()
		{
			float singleBlinkDuration = _blinkDuration / _blinkCount;
			float halfBlinkDuration = singleBlinkDuration * 0.5f;
			
			_bodyMesh.GetPropertyBlock(_propertyBlock);
			_propertyBlock.SetColor(BlinkColorId, _blinkColor);
			
			for (int i = 0; i < _blinkCount; i++)
			{
				_propertyBlock.SetFloat(BlinkIntensityId, 1.2f);
				_bodyMesh.SetPropertyBlock(_propertyBlock);
				yield return new WaitForSeconds(halfBlinkDuration);
				
				_propertyBlock.SetFloat(BlinkIntensityId, 0f);
				_bodyMesh.SetPropertyBlock(_propertyBlock);
				yield return new WaitForSeconds(halfBlinkDuration);
			}
			
			_propertyBlock.SetFloat(BlinkIntensityId, 0f);
			_bodyMesh.SetPropertyBlock(_propertyBlock);
			_blinkCoroutine = null;
		}

		public void Initialize(ApplyDamageCommand applyDamage)
		{
			_damageSub = applyDamage.Subscribe(HandleDamageApplied);
		}

		private void HandleDamageApplied(DamageInfo damageInfo)
		{
			if(damageInfo.DamageAmount > 0)
			{
				PlayDamageBlink();
			}
		}

		private void OnDestroy()
		{
			_damageSub?.Dispose();
		}
		
		#if UNITY_EDITOR
		[Button]
		private void TestBlink()
		{
			PlayDamageBlink();
		}
		#endif
	}
}
