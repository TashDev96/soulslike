using Cysharp.Threading.Tasks;

namespace game.gameplay_core.characters.logic
{
	public class DeathLogic
	{
		private CharacterContext _context;

		public void SetContext(CharacterContext context)
		{
			_context = context;
			_context.IsDead.OnChanged += HandleIsDead;
		}

		private void HandleIsDead(bool isDead)
		{
			_context.RigidBody.IsKinematic = isDead;
			_context.CharacterCollider.SetColliderEnabled(!isDead);
			_context.Views.BodyView.SetDeadState(isDead);

			if(isDead)
			{
				if(_context.IsPlayer.Value)
				{
					ProcessPlayerDeathSequence().Forget();
				}
				else
				{
					Shortcuts.PlayerSoftCurrency.AddDelayedValue(_context.Config.SoftCurrencyDrop, out var delayId);
					ViewsOrchestra.ShowSoftCurrencyDrop(_context.Transform.Position, delayId);
				}
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
