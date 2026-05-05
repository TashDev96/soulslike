using ControlFreak2;
using UnityEngine;

namespace game.input
{
	public class InputAdapter
	{
		public static float GetAxis(InputAxesNames axis)
		{
			return CF2Input.GetAxis(axis.ToString());
		}

		public static float GetAxisRaw(InputAxesNames axis)
		{
			return CF2Input.GetAxisRaw(axis.ToString());
		}

		public static Vector2 GetDirectionInputRaw()
		{
			return new Vector2(CF2Input.GetAxisRaw(nameof(InputAxesNames.Horizontal)), CF2Input.GetAxisRaw(nameof(InputAxesNames.Vertical)));
		}

		public static bool GetButtonDown(InputAxesNames axis)
		{
			return CF2Input.GetButtonDown(axis.ToString());
		}

		public static bool GetButtonUp(InputAxesNames axis)
		{
			return CF2Input.GetButtonUp(axis.ToString());
		}

		public static bool GetButton(InputAxesNames axis)
		{
			return CF2Input.GetButton(axis.ToString());
		}
	}
}
