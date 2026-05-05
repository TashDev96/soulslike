using UnityEngine;

namespace game.gameplay_core.inventory.item_configs
{
	[CreateAssetMenu(menuName = "Configs/Items/Resource")]
	public class ResourceItemConfig : BaseItemConfig
	{
		[field:SerializeField]
		public bool CanStack { get; private set; }
	}
}
