using dream_lib.src.extensions;
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

		public override void Initialize(DamageReceiverContext damageReceiverContext)
		{
			base.Initialize(damageReceiverContext);
			_unarmoredReceiver.gameObject.SetActive(false);
		}

		public override void ApplyDamage(DamageInfo damageInfo)
		{
			var damageToArmor = damageInfo.DamageAmount * _damageMultiplier;
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
				_unarmoredReceiver.gameObject.SetActive(true);
				gameObject.SetActive(false);
				_armorBreakParticles.gameObject.SetActive(true);
				_armorBreakParticles.Play();
			}
		}
	}
}
