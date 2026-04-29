using dream_lib.src.extensions;
using dream_lib.src.utils.drawers;
using game.gameplay_core.characters.view;
using game.gameplay_core.damage_system;
using game.gameplay_core.worldspace_ui;
using UnityEngine;

namespace game.gameplay_core.characters.bosses
{
	public class BossArmorDamageReceiver : DamageReceiver
	{
		[SerializeField]
		private DamageReceiver _unarmoredReceiver;
		[SerializeField]
		private ParticleSystem _armorBreakParticles;
		[SerializeField]
		private CharacterBodyView _blinkView;

		[SerializeField]
		private float _armorAmount;

		[SerializeField]
		private bool _listenPlungeDamage;
		private int _selfLayerMask;

		public bool IsBroken => _armorAmount <= 0;

		public override void Initialize(DamageReceiverContext damageReceiverContext)
		{
			base.Initialize(damageReceiverContext);
			_unarmoredReceiver.gameObject.SetActive(false);
			_selfLayerMask = LayerMask.GetMask("DamageReceivers");

			if(_listenPlungeDamage)
			{
				damageReceiverContext.ApplyDamage.OnExecute += HandlePlungeAttack;
			}
		}

		public override void ApplyDamage(DamageInfo damageInfo)
		{
			var damageToArmor = damageInfo.DamageAmount * _damageMultiplier;
			if(damageToArmor <= 0)
			{
				return;
			}

			_armorAmount -= damageToArmor;
			if(_armorAmount > 0)
			{
				GameStaticContext.Instance.FloatingTextsManager.ShowFloatingText(damageToArmor.RoundFormat(), FloatingTextView.TextColorVariant.Grey, transform.position);
				_blinkView.PlayDamageBlink();
				return;
			}

			base.ApplyDamage(damageInfo);

			if(_armorAmount <= 0)
			{
				DestroyArmor();
			}
		}

		private void HandlePlungeAttack(DamageInfo data)
		{
			if(_armorAmount <= 0)
			{
				return;
			}

			if(data.IsPlunge)
			{
				var colliders = Physics.OverlapSphere(data.WorldPos, 1.5f, _selfLayerMask);
				DebugDrawUtils.DrawHandlesSphere(data.WorldPos, 1.5f, Color.red, 10f);
				foreach(var collider in colliders)
				{
					if(collider.transform == transform)
					{
						_armorAmount -= data.DamageAmount * _damageMultiplier;

						if(_armorAmount <= 0)
						{
							DestroyArmor();
						}

						return;
					}
				}
			}
		}

		private void DestroyArmor()
		{
			_unarmoredReceiver.gameObject.SetActive(true);
			gameObject.SetActive(false);
			_armorBreakParticles.gameObject.SetActive(true);
			_armorBreakParticles.Play();
		}
	}
}
