using Cysharp.Threading.Tasks;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class DeathLogic
	{
		private CharacterContext _context;

		public void SetContext(CharacterContext context)
		{
			_context = context;
			_context.IsDead.OnChanged += HandlePlayerIsDead;
		}

		private void HandlePlayerIsDead(bool isDead)
		{
			_context.RigidBody.IsKinematic = isDead;
			_context.CharacterCollider.SetColliderEnabled(!isDead);
			_context.Views.BodyView.SetDeadState(isDead);

			if(isDead && _context.IsPlayer.Value)
			{
				ProcessPlayerDeathSequence().Forget();
			}
		}

		private async UniTask ProcessPlayerDeathSequence()
		{
			await UniTask.WaitForSeconds(1);
			GameStaticContext.Instance.ReloadLocation.Execute();
			_context.Logic.MovementLogic.Teleport(GameStaticContext.Instance.PlayerSave.RespawnTransform);
			_context.IsDead.Value = false;
		}
	}
}
