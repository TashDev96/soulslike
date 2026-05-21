using System;
using System.Collections;
using game.gameplay_core.damage_system;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.view
{
	public class CharacterBodyView : MonoBehaviour
	{
		private static readonly int BlinkIntensityId = Shader.PropertyToID("_BlinkIntensity");
		private static readonly int BlinkColorId = Shader.PropertyToID("_BlinkColor");
		[SerializeField]
		private MeshRenderer _bodyMesh;

		[SerializeField]
		private Collider _aliveBodyCollider;
		[SerializeField]
		private CapsuleCollider _deadBodyCollider;

		[SerializeField]
		private Color _blinkColor = Color.white;

		[SerializeField]
		private float _blinkDuration = 0.5f;

		[SerializeField]
		private int _blinkCount = 3;

		private MaterialPropertyBlock _propertyBlock;
		private Coroutine _blinkCoroutine;
		private IDisposable _damageSub;
		private Coroutine _deadStateCoroutine;
		private float _defaultDeadRadius;

		public void Initialize(CharacterContext context)
		{
			_damageSub = context.Events.ApplyDamage.Subscribe(HandleDamageApplied);
		}

		private void Awake()
		{
			_propertyBlock = new MaterialPropertyBlock();
			if(_deadBodyCollider != null)
			{
				_defaultDeadRadius = _deadBodyCollider.radius;
			}
		}

		public void SetDeadState(bool isDead)
		{
			if(_deadStateCoroutine != null)
			{
				StopCoroutine(_deadStateCoroutine);
			}

			
			_aliveBodyCollider.gameObject.SetActive(!isDead);
			_deadBodyCollider.gameObject.SetActive(false);

			if(isDead)
			{
				_deadStateCoroutine = StartCoroutine(ActivateDeadState());
			}
		}

		public void PlayDamageBlink()
		{
			if(_blinkCoroutine != null)
			{
				StopCoroutine(_blinkCoroutine);
			}
			_blinkCoroutine = StartCoroutine(BlinkCoroutine());
		}

		public Vector3 GetTopPos()
		{
			return transform.position + Vector3.up * 3f;
		}

		private IEnumerator ActivateDeadState()
		{
			yield return new WaitForSeconds(2f);
			
			_deadBodyCollider.gameObject.SetActive(true);
			_deadBodyCollider.radius = 0.01f;
			
			
			while(_deadBodyCollider.radius < _defaultDeadRadius)
			{
				var deltaTime = Mathf.Min(1 / 60f, Time.deltaTime);
				_deadBodyCollider.radius += 0.3f * deltaTime;
				yield return null;
			}
			_deadBodyCollider.radius = _defaultDeadRadius;
		}

		private IEnumerator BlinkCoroutine()
		{
			var singleBlinkDuration = _blinkDuration / _blinkCount;
			var halfBlinkDuration = singleBlinkDuration * 0.5f;

			_bodyMesh.GetPropertyBlock(_propertyBlock);
			_propertyBlock.SetColor(BlinkColorId, _blinkColor);

			for(var i = 0; i < _blinkCount; i++)
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
