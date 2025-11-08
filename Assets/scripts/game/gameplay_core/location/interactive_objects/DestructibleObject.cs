using System;
using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.damage_system;
using game.gameplay_core.location.location_save_system;
using UnityEngine;

namespace game.gameplay_core.location.interactive_objects
{
	public enum DestructibleVisualState
	{
		Normal,
		DestroyedWithAnimation,
		DestroyedInstantly
	}

	public class DestructibleObject : SavableSceneObjectGeneric<DestructibleObjectSaveData>
	{
		[SerializeField]
		private float _maxHp = 100f;

		[SerializeField]
		private SerializableDictionary<DestructibleVisualState, GameObject> _visualStates = new();

		[SerializeField]
		private DamageReceiver _damageReceiver;
		private ApplyDamageCommand _applyDamageCommand;
		private float _currentHp;

		public override void InitializeFirstTime()
		{
			SaveData = new DestructibleObjectSaveData
			{
				Destroyed = false,
				Hp = _maxHp
			};
			InitializeAfterSaveLoaded();
		}

		protected override void InitializeAfterSaveLoaded()
		{
			_currentHp = SaveData.Hp;

			if(SaveData.Destroyed)
			{
				SetVisualState(DestructibleVisualState.DestroyedInstantly);
			}
			else
			{
				SetVisualState(DestructibleVisualState.Normal);
				InitializeDamageReceiver();
			}
		}

		private void InitializeDamageReceiver()
		{
			_applyDamageCommand = new ApplyDamageCommand();
			_applyDamageCommand.OnExecute += HandleDamageReceived;

			var characterId = new ReactiveProperty<string>(UniqueId);
			var team = new ReactiveProperty<Team>(Team.HostileNPC);

			_damageReceiver.Initialize(new DamageReceiver.DamageReceiverContext
			{
				Team = team,
				CharacterId = characterId,
				ApplyDamage = _applyDamageCommand
			});
		}

		private void HandleDamageReceived(DamageInfo damageInfo)
		{
			if(SaveData.Destroyed)
			{
				return;
			}

			_currentHp -= damageInfo.DamageAmount;
			SaveData.Hp = _currentHp;

			if(_currentHp <= 0)
			{
				SaveData.Destroyed = true;
				SetVisualState(DestructibleVisualState.DestroyedWithAnimation);
			}
		}

		private void SetVisualState(DestructibleVisualState state)
		{
			foreach(var kvp in _visualStates)
			{
				if(kvp.Value != null)
				{
					kvp.Value.SetActive(kvp.Key == state);
				}
			}
		}
	}

	[Serializable]
	public class DestructibleObjectSaveData : BaseSaveData
	{
		[field: SerializeField]
		public bool Destroyed { get; set; }

		[field: SerializeField]
		public float Hp { get; set; }
	}
}
