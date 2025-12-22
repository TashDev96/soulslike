using UnityEngine;

namespace game.gameplay_core.camera
{
	public interface ICameraController
	{
		Camera Camera { get; }
		void Update(float deltaTime);
	}
}
