using System;
using System.Collections;
using game.gameplay_core.damage_system;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.view
{
	public class CharacterBodyView : MonoBehaviour
	{
		 
		[SerializeField]
		private Collider _aliveBodyCollider;
		[SerializeField]
		private CapsuleCollider _deadBodyCollider;

		[SerializeField]
		private BlinkView _blinkView;


		private IDisposable _damageSub;
		private Coroutine _deadStateCoroutine;
		private float _defaultDeadRadius;
		
		[field: SerializeField]
		public CharacterFlyingBodyView FlyingBodyView { get; private set; }


		public void Initialize(CharacterContext context)
		{
			_damageSub = context.Events.ApplyDamage.Subscribe(HandleDamageApplied);
			_blinkView.Initialize();
			FlyingBodyView?.Initialize(context);
		}

		private void Awake()
		{
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
			_blinkView.PlayDamageBlink();
		}

		public Vector3 GetTopPos()
		{
			return transform.position + Vector3.up * 3f;
		}
		
		public void SetFlyingMode(bool flying)
		{
			gameObject.SetActive(!flying);
			FlyingBodyView.gameObject.SetActive(flying);
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
			_blinkView.Dispose();
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
