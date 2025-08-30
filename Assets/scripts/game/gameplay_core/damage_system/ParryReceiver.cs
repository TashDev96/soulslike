using dream_lib.src.reactive;
using game.gameplay_core.characters;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class ParryReceiver : MonoBehaviour
	{
		public struct Context
		{
			public IReadOnlyReactiveProperty<Team> Team { get; set; }
			public IReadOnlyReactiveProperty<string> CharacterId { get; set; }
			public ReactiveCommand<CharacterDomain> OnParryTriggered { get; set; }
		}

		private Context _context;

		public Team OwnerTeam => _context.Team.Value;
		public string CharacterId => _context.CharacterId.Value;

		public void Initialize(Context context)
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
