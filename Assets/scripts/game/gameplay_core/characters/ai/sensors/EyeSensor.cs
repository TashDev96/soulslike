using System;
using System.Collections.Generic;
using System.Linq;
using dream_lib.src.utils;
using dream_lib.src.utils.drawers;
using game.gameplay_core.characters.ai.world_reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace game.gameplay_core.characters.ai.sensors
{
	[Serializable]
	public class EyeSensor
	{
		[SerializeField]
		private string _name;
		[SerializeField]
		private Transform _rootTransform;
		[SerializeField]
		private Vector3 _localPosition;
		[SerializeField]
		private Vector3 _localEuler = Vector3.forward;

		[SerializeField]
		private Cone[] _viewCones;

		[SerializeField]
		private bool _disableDebugDraw;

		private List<CharacterObservation> _characterVisualObservations;
		private int _layerMask;
		private CharacterDomain _self;

		private static List<VisualInfoPoint> _validVisualPointsCache = new(1000);
		public Vector3 Position => _rootTransform.TransformPoint(_localPosition);
		public Vector3 Forward => _rootTransform.TransformDirection(Quaternion.Euler(_localEuler) * Vector3.forward);
		public Vector3 Right => _rootTransform.TransformDirection(Quaternion.Euler(_localEuler) * Vector3.right);
		public Vector3 Up => _rootTransform.TransformDirection(Quaternion.Euler(_localEuler) * Vector3.up);

		public void Initialize(List<CharacterObservation> characterVisualObservations, CharacterDomain self)
		{
			_characterVisualObservations = characterVisualObservations;
			_self = self;

			foreach(var cone in _viewCones)
			{
				cone.LastUpdateTime = Time.time + Random.value * cone.UpdatePeriodSec;
				cone.HalfAngleCosSqrCache = Mathf.Pow(Mathf.Cos(cone.Angle * 0.5f * Mathf.Deg2Rad), 2);
			}

			_layerMask = LayerMask.GetMask("Default", "LevelGeometry", "Doors");
		}

		public void Observe()
		{
			foreach(var viewCone in _viewCones)
			{
				UpdateCone(viewCone);
			}
		}

		public void UpdateExistingObservations(float deltaTime)
		{
			var eyeForward = Forward;
			var position = Position;

			foreach(var observation in _characterVisualObservations)
			{
				observation.TimePassed += deltaTime;
				var currentCharPos = observation.Character.Context.Transform.Position;

				foreach(var cone in _viewCones)
				{
					if(observation.TimePassed > cone.UpdatePeriodSec)
					{
						continue;
					}

					if(!MathUtils.CheckPointInCone(position, eyeForward, cone.HalfAngleCosSqrCache, cone.Distance, currentCharPos))
					{
						continue;
					}

					//we don't raycast walls here, it's hack for character prediction in which direction previously observed character will go
					observation.Position = currentCharPos;
					break;
				}
			}
		}

		private void UpdateCone(Cone cone)
		{
			if(cone.LastUpdateTime + cone.UpdatePeriodSec > Time.time)
			{
				return;
			}

			var allVisualPoints = LocationStaticContext.Instance.WorldInfo.VisualInfoPoints;
			_validVisualPointsCache.Clear();

			cone.LastUpdateTime = Time.time;

			var eyeForward = Forward;
			var position = Position;
			var maxDistanceSqr = cone.Distance * cone.Distance;

			foreach(var visualPoint in allVisualPoints)
			{
				if(visualPoint.Character == _self)
				{
					continue;
				}

				var vecToPoint = visualPoint.Position - position;
				var distanceToPointSqr = vecToPoint.sqrMagnitude;

				if(distanceToPointSqr > maxDistanceSqr)
				{
					continue;
				}

				if(!MathUtils.CheckPointInCone(position, eyeForward, cone.HalfAngleCosSqrCache, visualPoint.Position))
				{
					continue;
				}

				if(Physics.Linecast(position, visualPoint.Position, _layerMask))
				{
					continue;
				}

				if(visualPoint.Character != null)
				{
					var lastObservation = _characterVisualObservations.FirstOrDefault(o => o.Character == visualPoint.Character);
					if(lastObservation == null)
					{
						lastObservation = new CharacterObservation
						{
							Character = visualPoint.Character
						};
						_characterVisualObservations.Add(lastObservation);
					}

					lastObservation.Position = lastObservation.Character.Context.Transform.Position;
					lastObservation.TimePassed = 0;
				}

				//Debug.DrawLine(position, visualPoint.Position, cone.Color, 0.1f);
			}

			//проходиться по массивам объектов? или делать overlap? 

			//если overlap, тогда на всём должны быть коллайдеры, минус
			//если по массивам, тогда нужно менеджерить массивы и чанки

			//хочу возможность делать толпу, поэтому нужен максимально оптимизированный подход
		}

		[Serializable]
		private class Cone
		{
			public string Name;
			public float Distance;
			public float Angle;
			public float UpdatePeriodSec;

			public Color Color;

			public bool EnableDebugLog;

			[NonSerialized]
			public float LastUpdateTime;
			[NonSerialized]
			public float HalfAngleCosSqrCache;
		}

#if UNITY_EDITOR

		public void OnValidate()
		{
			foreach(var cone in _viewCones)
			{
				cone.HalfAngleCosSqrCache = Mathf.Pow(Mathf.Cos(cone.Angle * 0.5f * Mathf.Deg2Rad), 2);
			}
		}

		public void DrawGizmosSelected()
		{
			if(_disableDebugDraw)
			{
				return;
			}

			DrawCone(_viewCones[0], Color.blue);
			DrawCone(_viewCones[1], Color.lightBlue);

			void DrawCone(Cone cone, Color color)
			{
				var endPoint = Position + Forward * cone.Distance;
				DebugDrawUtils.DrawCross(Position, 0.1f, color);
				DebugDrawUtils.DrawCross(endPoint, 0.1f, color);
				Gizmos.color = color;
				var endRadius = Mathf.Tan(cone.Angle / 2 * Mathf.Deg2Rad) * cone.Distance;
				DebugDrawUtils.DrawWireCircle(endPoint, endRadius, Forward, color, 0, false);
				Debug.DrawLine(Position, endPoint + Up * endRadius, color);
				Debug.DrawLine(Position, endPoint - Up * endRadius, color);
				Debug.DrawLine(Position, endPoint + Right * endRadius, color);
				Debug.DrawLine(Position, endPoint - Right * endRadius, color);
			}
		}

#endif
	}
}
