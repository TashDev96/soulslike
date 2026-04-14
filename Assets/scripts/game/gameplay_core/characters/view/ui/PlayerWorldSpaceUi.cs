using DG.Tweening;
using dream_lib.src.extensions;
using UnityEngine;

namespace game.gameplay_core.characters.view.ui
{
	public class PlayerWorldSpaceUi : MonoBehaviour, ICharacterWorldSpaceUi
	{
		[SerializeField]
		private Transform _lockOnMark;
		private CharacterContext _context;
		private CharacterDomain _target;

		public void Initialize(CharacterContext context)
		{
			_context = context;
			transform.parent = GameStaticContext.Instance.WorldToScreenUiParent.Value;
			_lockOnMark.gameObject.SetActive(false);

			_context.LockOnLogic.LockOnTarget.OnChanged += HandleLockOnChanged;
			LocationStaticContext.Instance.LocationUiUpdate.OnExecute += CustomUpdate;
		}

		public void Reset()
		{
			_target = null;
			_lockOnMark.gameObject.SetActive(false);
		}

		private void HandleLockOnChanged(CharacterDomain target)
		{
			_target = target;
			_lockOnMark.gameObject.SetActive(target != null);
			if(target != null)
			{
				_lockOnMark.localScale = Vector3.one * 2f;
				_lockOnMark.DOScale(Vector3.one, 0.3f);
			}
		}

		private void CustomUpdate(float deltaTime)
		{
			if(_target == null)
			{
				return;
			}
			var worldPos = _target.Context.LockOnPoints[0].transform.position;
			var screenPos = GameStaticContext.Instance.MainCamera.Value.WorldToScreenPoint(worldPos).Round();
			_lockOnMark.position = screenPos;
		}
	}
}
