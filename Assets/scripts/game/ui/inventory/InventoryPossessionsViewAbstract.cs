using System;
using game.gameplay_core.inventory.items_logic;
using UnityEngine;

namespace game.ui.inventory
{
	public abstract class InventoryPossessionsViewAbstract : MonoBehaviour
	{
		public abstract void Initialize(Action<BaseItemLogic> autoEquipItem);
	}
}
