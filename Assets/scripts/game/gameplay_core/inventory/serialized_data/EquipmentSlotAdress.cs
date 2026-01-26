using System;
using game.enums;

namespace game.gameplay_core.inventory.serialized_data
{
	[Serializable]
	public struct EquipmentSlotAdress : IEquatable<EquipmentSlotAdress>
	{
		public EquipmentSlotType SlotType;
		public int Index;

		public EquipmentSlotAdress(EquipmentSlotType slotType, int index)
		{
			SlotType = slotType;
			Index = index;
		}

		public bool Equals(EquipmentSlotAdress other)
		{
			return SlotType == other.SlotType && Index == other.Index;
		}

		public override bool Equals(object obj)
		{
			return obj is EquipmentSlotAdress other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine((int)SlotType, Index);
		}

		public override string ToString()
		{
			return $"{SlotType}_{Index}";
		}

		public static bool operator ==(EquipmentSlotAdress left, EquipmentSlotAdress right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(EquipmentSlotAdress left, EquipmentSlotAdress right)
		{
			return !left.Equals(right);
		}
	}
}
