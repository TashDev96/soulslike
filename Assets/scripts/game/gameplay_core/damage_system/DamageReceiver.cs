using dream_lib.src.reactive;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class DamageReceiver : MonoBehaviour
	{
		public struct DamageReceiverContext
		{
			public IReadOnlyReactiveProperty<Team> Team { get; set; }
			public IReadOnlyReactiveProperty<string> CharacterId { get; set; }
			public ReactiveCommand<DamageInfo> ApplyDamage { get; set; }
		}

		[SerializeField]
		private float _damageMultiplier = 1f;

		private DamageReceiverContext _context;

		public Team OwnerTeam => _context.Team.Value;
		public string CharacterId => _context.CharacterId.Value;

		public void Initialize(DamageReceiverContext damageReceiverContext)
		{
			_context = damageReceiverContext;
		}

		public void ApplyDamage(DamageInfo damageInfo)
		{
			damageInfo.DamageAmount *= _damageMultiplier;
			_context.ApplyDamage.Execute(damageInfo);
		}
	}
}
