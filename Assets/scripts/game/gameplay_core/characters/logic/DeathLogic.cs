using System.Collections;
using dream_lib.src.utils.components;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class DeathLogic
	{
		private CharacterContext _context;
		private readonly UnityEventsListener _unityEventsListener;

		public DeathLogic(CharacterContext context)
		{
			_context = context;
			if(_context.IsPlayer.Value)
			{
				_context.IsDead.OnChanged += HandlePlayerIsDead;
				_unityEventsListener = UnityEventsListener.Create("PlayerDeathLogic");
			}
		}

		private void HandlePlayerIsDead(bool isDead)
		{
			if(isDead)
			{
				_unityEventsListener.StartCoroutine(ProcessPlayerDeathSequence());
			}
		}

		private IEnumerator ProcessPlayerDeathSequence()
		{
			yield return new WaitForSeconds(1);
			GameStaticContext.Instance.ReloadLocation.Execute();
			_context.MovementLogic.Teleport(GameStaticContext.Instance.PlayerSave.RespawnTransform);
			_context.IsDead.Value = false;
		}
	}
}
