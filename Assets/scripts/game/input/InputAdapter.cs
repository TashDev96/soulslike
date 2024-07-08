using ControlFreak2;

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

		public static bool GetButtonDown(InputAxesNames axis)
		{
			return CF2Input.GetButtonDown(axis.ToString());
		}

		public static bool GetButton(InputAxesNames axis)
		{
			return CF2Input.GetButton(axis.ToString());
		}
	}
}
