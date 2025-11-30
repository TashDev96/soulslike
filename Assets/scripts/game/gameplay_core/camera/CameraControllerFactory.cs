using System;
using dream_lib.src.reactive;
using game.gameplay_core.characters;
using UnityEngine;

namespace game.gameplay_core.camera
{
	public static class CameraControllerFactory
	{
		public static ICameraController Create(CameraSettings settings, IReadOnlyReactiveProperty<Camera> camera, ReactiveProperty<CharacterDomain> player)
		{
			return settings switch
			{
				IsometricCameraSettings isometric => new IsometricCameraController(new IsometricCameraController.Context
				{
					Camera = camera,
					Player = player,
					CameraSettings = isometric
				}),
				ThirdPersonCameraSettings thirdPerson => new ThirdPersonCameraController(new ThirdPersonCameraController.Context
				{
					Camera = camera,
					Player = player,
					CameraSettings = thirdPerson
				}),
				FixedCameraSettings fixedCamera => new FixedCameraController(new FixedCameraController.Context
				{
					Camera = camera,
					Player = player,
					CameraSettings = fixedCamera
				}),
				_ => throw new ArgumentException($"Unknown camera settings type: {settings.GetType()}")
			};
		}
	}
}
