using System;
using game.gameplay_core.damage_system;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.view
{
	public class CharacterBodyView : MonoBehaviour
	{
		[SerializeField]
		private BlinkView _blinkView;

		private IDisposable _damageSub;

		[field: SerializeField]
		public CharacterFlyingBodyView FlyingBodyView { get; private set; }

		public void Initialize(CharacterContext context)
		{
			_blinkView.Initialize();
			_damageSub = context.ApplyDamage.Subscribe(HandleDamageApplied);
			FlyingBodyView?.Initialize(context);
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
