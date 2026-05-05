using game.gameplay_core.characters;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class DamageReceiver : MonoBehaviour
	{
		[SerializeField]
		protected float _damageMultiplier = 1f;

		private CharacterContext _context;

		public Team OwnerTeam => _context.Team.Value;
		public string CharacterId => _context.CharacterId.Value;

		public bool IsInvulnerable => _context.Logic.InvulnerabilityLogic.IsInvulnerable;

		public virtual void Initialize(CharacterContext context)
		{
			_context = context;
		}

		public virtual void ApplyDamage(DamageInfo damageInfo)
		{
			if(_context.Logic.InvulnerabilityLogic is { IsInvulnerable: true })
			{
				return;
			}

			damageInfo.DamageAmount *= _damageMultiplier;
			_context.Events.ApplyDamage.Execute(damageInfo);
		}
	}
}
