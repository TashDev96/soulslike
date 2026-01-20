using game.gameplay_core.characters;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class ParryReceiver : MonoBehaviour
	{
		private CharacterContext _context;

		public Team OwnerTeam => _context.Team.Value;
		public string CharacterId => _context.CharacterId.Value;

		public void Initialize(CharacterContext context)
		{
			_context = context;
			gameObject.SetActive(false);
		}

		public void SetActive(bool active)
		{
			gameObject.SetActive(active);
		}

		public bool TryResolveParry(CharacterDomain damageDealer)
		{
			damageDealer.CharacterStateMachine.TriggerParryStun();
			_context.OnParryTriggered.Execute(damageDealer);
			return true;
		}
	}
}
