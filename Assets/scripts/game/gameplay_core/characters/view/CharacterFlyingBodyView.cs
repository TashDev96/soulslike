using System;
using game.gameplay_core.characters.config.animation;
using game.gameplay_core.damage_system;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.view
{
	public class CharacterFlyingBodyView : MonoBehaviour
	{
		[SerializeField]
		private Transform _wingRPivot;
		[SerializeField]
		private Transform _wingLPivot;

		[SerializeField]
		private Animations _animations;

		[SerializeField]
		private BlinkView _blinkView;

		private IDisposable _damageSub;
		private CharacterContext _context;

		public void Initialize(CharacterContext context)
		{
			_context = context;
			_damageSub = _context.ApplyDamage.Subscribe(HandleDamageApplied);
			_blinkView.Initialize();
		}

		private void Awake()
		{
		}

		public void PlayDamageBlink()
		{
			_blinkView.PlayDamageBlink();
		}

		public Vector3 GetTopPos()
		{
			return transform.position + Vector3.up * 3f;
		}

		public void PlaySitAnimation()
		{
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

		[Serializable]
		private struct Animations
		{
			public AnimationConfig Sit;
			public AnimationConfig Walk;
			public AnimationConfig TakeOff;
			public AnimationConfig Plane;
			public AnimationConfig Flap;
		}
	}
}
