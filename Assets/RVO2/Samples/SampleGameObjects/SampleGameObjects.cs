// -----------------------------------------------------------------------
// <copyright file="SampleGameObjects.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace RVO
{
	public class SampleGameObjects : MonoBehaviour
	{
		[SerializeField]
		private Transform goalIndicator;
		private static SampleGameObjects instance;

		private static bool gameQuiting;

		private Simulator simulator;

		private Camera mainCamera;
		private float2 currentGoal;
		private Plane ground = new(Vector3.up, 0);

		public static SampleGameObjects Instance
		{
			get
			{
				if(instance == null)
				{
					if(gameQuiting)
					{
						return null;
					}

					instance = FindAnyObjectByType<SampleGameObjects>();
					Assert.IsNotNull(instance);
				}

				return instance;
			}
		}

		private void Awake()
		{
			SetGoalPosFromIndicator();
		}

		private void OnEnable()
		{
			mainCamera = Camera.main;
		}

		public static Simulator GetSimulator()
		{
			if(gameQuiting)
			{
				return null;
			}

			if(Instance.simulator == null)
			{
				var simulator = new Simulator();

				simulator.SetTimeStep(1 / 60f);
				simulator.SetAgentDefaults(10f, 10, 4f, 4f, 0.5f, 0.2f, new float2(0f, 0f));

				Instance.simulator = simulator;
			}

			return Instance.simulator;
		}

		public static float2 GetGoal()
		{
			if(gameQuiting)
			{
				return default;
			}

			return instance.currentGoal;
		}

	 

		private void OnDestroy()
		{
			simulator.Clear();

			simulator.Dispose();
		}

		private void OnGUI()
		{
			GUILayout.Label($"Agents:{simulator.GetNumAgents()}");
			GUILayout.Label($"FPS:{1f / Time.deltaTime}");
		}

		private void Update()
		{
			simulator.SetTimeStep(Time.deltaTime);
			simulator.DoStep();
			return;
			if(Input.GetMouseButton(0))
			{
				if(mainCamera == null)
				{
					return;
				}

				var position = Input.mousePosition;
				var ray = mainCamera.ScreenPointToRay(position);
				if(ground.Raycast(ray, out var enter))
				{
					var worldPosition = ray.GetPoint(enter);
					currentGoal = new float2(worldPosition.x, worldPosition.z);
					var goalPosition = goalIndicator.transform.position;
					goalIndicator.transform.position = new Vector3(currentGoal.x, goalPosition.y, currentGoal.y);
				}
			}
		}

		private void SetGoalPosFromIndicator()
		{
			currentGoal = new float2(goalIndicator.position.x, goalIndicator.position.z);
		}

		private void LateUpdate()
		{
			simulator.EnsureCompleted();
		}

		private void OnApplicationQuit()
		{
			gameQuiting = true;
		}
	}
}
