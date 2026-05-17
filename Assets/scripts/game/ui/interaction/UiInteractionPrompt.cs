using System.Collections.Generic;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.ui.animations;
using game.gameplay_core.characters.logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace game.ui
{
	public class UiInteractionPrompt : MonoBehaviour
	{
		public class Context
		{
			public InteractionLogic Logic;
			public ReactiveCommand<float> UiUpdate;
		}

		[SerializeField]
		private TextMeshProUGUI _text;

		[SerializeField]
		private RectTransform _messagesContainer;
		[SerializeField]
		private UiMessageView _messagePrefab;

		[SerializeReference]
		[SerializeField]
		private List<UiAnimationBase> _appearAnims;

		private Context _context;

		public void SetContext(Context context)
		{
			_context = context;

			_context.Logic.InteractionText.OnChanged += HandleInteractionChanged;
			_context.Logic.OnNotificationAdded.OnExecute += HandleNewNotification;
			_context.Logic.OnNotificationsCleared.OnExecute += HandleNotificationsCleared;
			_context.UiUpdate.OnExecute += HandleUiUpdate;

			_messagesContainer.DestroyAllChildren();

			foreach(var anim in _appearAnims)
			{
				anim.Initialize(this);
				anim.Clear(true);
			}
		}

		private void HandleNotificationsCleared()
		{
			_messagesContainer.DestroyAllChildren();
		}

		private void HandleNewNotification(NotificationInfo info)
		{
			var newNotification = Instantiate(_messagePrefab, _messagesContainer);
			newNotification.Show(info.Message, info.Icon);
			LayoutRebuilder.ForceRebuildLayoutImmediate(_messagesContainer);
			LayoutRebuilder.ForceRebuildLayoutImmediate(_messagesContainer);
		}

		private void HandleInteractionChanged(string interactionTextHint)
		{
			if(interactionTextHint == null)
			{
				foreach(var anim in _appearAnims)
				{
					anim.Clear(false);
				}
			}
			else
			{
				_text.text = interactionTextHint;
				foreach(var anim in _appearAnims)
				{
					anim.Play(false);
				}
			}
		}

		private void HandleUiUpdate(float deltaTime)
		{
			if(_context.Logic.InteractionText.Value == null)
			{
				foreach(var anim in _appearAnims)
				{
					if(anim.InProgress)
					{
						return;
					}
				}
			}
		}
	}
}
