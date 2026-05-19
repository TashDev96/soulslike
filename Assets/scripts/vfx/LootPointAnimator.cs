using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = System.Random;

namespace VFX
{
	/// <summary>
	///     Animates a mesh with the LootPointShader, providing deterministic random parameters based on an item ID string.
	/// </summary>
	[RequireComponent(typeof(MeshRenderer))]
	public class LootPointAnimator : MonoBehaviour
	{
		// Shader Property IDs
		private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
		private static readonly int CoreSharpnessId = Shader.PropertyToID("_CoreSharpness");
		private static readonly int RayLengthId = Shader.PropertyToID("_RayLength");
		private static readonly int RayWidthId = Shader.PropertyToID("_RayWidth");
		private static readonly int SecondaryLengthId = Shader.PropertyToID("_SecondaryLength");
		private static readonly int SecondaryWidthId = Shader.PropertyToID("_SecondaryWidth");
		private static readonly int SecondaryStrengthId = Shader.PropertyToID("_SecondaryStrength");
		private static readonly int SecondaryAngleId = Shader.PropertyToID("_SecondaryAngle");
		private static readonly int GlowRadiusId = Shader.PropertyToID("_GlowRadius");
		private static readonly int PulseSpeedId = Shader.PropertyToID("_PulseSpeed");
		[Header("Animation Timings")]
		[Tooltip("Duration for the mesh to scale up and appear.")]
		public float appearDuration = 0.4f;
		[Tooltip("Duration for the secondary rays to fade in after the initial appearance.")]
		public float secondaryRaysShowDuration = 0.8f;
		[Tooltip("Duration for the mesh to scale down and disappear.")]
		public float hideDuration = 0.25f;

		[Header("Randomization Ranges (Min/Max)")]
		public Color colorMin = new(1f, 0.8f, 0.3f);
		public Color colorMax = new(1f, 1f, 0.7f);

		public Vector2 coreSharpnessRange = new(80, 150);
		public Vector2 rayLengthRange = new(25, 45);
		public Vector2 rayWidthRange = new(300, 600);

		public Vector2 secondaryLengthRange = new(15, 30);
		public Vector2 secondaryWidthRange = new(400, 700);
		public Vector2 secondaryStrengthRange = new(0.2f, 0.5f);
		public Vector2 secondaryAngleRange = new(0, 360);
		[Tooltip("Range for rotation speed (degrees per second). Can be negative for clockwise rotation.")]
		public Vector2 secondaryRotationSpeedRange = new(10, 30);
		[Range(0, 1)]
		[Tooltip("Chance (0-1) that the secondary rays will be static instead of rotating.")]
		public float secondaryStaticChance = 0.3f;

		public Vector2 glowRadiusRange = new(10, 15);
		public Vector2 pulseSpeedRange = new(2.5f, 4f);

		[Header("Debug / Testing")]
		public string testItemId = "common_sword_01";
		public bool appearOnStart;

		private MeshRenderer _renderer;
		private MaterialPropertyBlock _propBlock;
		private Coroutine _animationCoroutine;

		// Deterministic target values
		private Color _targetColor;
		private float _targetCoreSharpness;
		private float _targetRayLength;
		private float _targetRayWidth;
		private float _targetSecondaryLength;
		private float _targetSecondaryWidth;
		private float _targetSecondaryStrength;
		private float _targetSecondaryAngle;
		private float _targetRotationSpeed;
		private float _currentRotationAngle;
		private float _currentSecondaryStrength;
		private float _targetGlowRadius;
		private float _targetPulseSpeed;

		public void InitializeAndAppear(string itemId)
		{
			if(string.IsNullOrEmpty(itemId))
			{
				itemId = "default";
			}

			GenerateDeterministicParameters(itemId);

			if(_animationCoroutine != null)
			{
				StopCoroutine(_animationCoroutine);
			}
			_animationCoroutine = StartCoroutine(AppearRoutine());
		}

		private void Awake()
		{
			_renderer = GetComponent<MeshRenderer>();
			_propBlock = new MaterialPropertyBlock();

			// Start hidden if not appearing immediately
			if(!appearOnStart)
			{
				transform.localScale = Vector3.zero;
			}
		}

		private void Start()
		{
			if(appearOnStart)
			{
				InitializeAndAppear(testItemId);
			}
		}

		/// <summary>
		///     Generates unique parameters for the given string ID and triggers the appear animation.
		/// </summary>
		[Button]
		[ContextMenu("Trigger Appear")]
		public void TriggerAppearTest()
		{
			InitializeAndAppear(testItemId);
		}

		/// <summary>
		///     Triggers the hide animation.
		/// </summary>
		[Button]
		[ContextMenu("Trigger Hide")]
		public void TriggerHideTest()
		{
			Hide();
		}

		public void Hide()
		{
			if(_animationCoroutine != null)
			{
				StopCoroutine(_animationCoroutine);
			}
			_animationCoroutine = StartCoroutine(HideRoutine());
		}

		private void Update()
		{
			if(_targetRotationSpeed != 0)
			{
				_currentRotationAngle += _targetRotationSpeed * Time.deltaTime;
				ApplyToRenderer();
			}
		}

		private void GenerateDeterministicParameters(string itemId)
		{
			// Use string hash for a stable seed
			var seed = itemId.GetHashCode();
			var rnd = new Random(seed);

			float GetRand(Vector2 range)
			{
				return range.x;
			}

			_targetColor = Color.Lerp(colorMin, colorMax, (float)rnd.NextDouble());
			_targetCoreSharpness = GetRand(coreSharpnessRange);
			_targetRayLength = GetRand(rayLengthRange);
			_targetRayWidth = GetRand(rayWidthRange);
			_targetSecondaryLength = GetRand(secondaryLengthRange);
			_targetSecondaryWidth = GetRand(secondaryWidthRange);
			_targetSecondaryStrength = GetRand(secondaryStrengthRange);
			_targetSecondaryAngle = GetRand(secondaryAngleRange);

			// Handle static chance and rotation speed
			if(rnd.NextDouble() < secondaryStaticChance)
			{
				_targetRotationSpeed = 0;
			}
			else
			{
				_targetRotationSpeed = GetRand(secondaryRotationSpeedRange);

				// Randomize direction (50% chance for clockwise)
				if(rnd.NextDouble() > 0.5)
				{
					_targetRotationSpeed *= -1;
				}
			}

			_currentRotationAngle = _targetSecondaryAngle;
			_targetGlowRadius = GetRand(glowRadiusRange);
			_targetPulseSpeed = GetRand(pulseSpeedRange);
		}

		private IEnumerator AppearRoutine()
		{
			float elapsed = 0;

			// Set initial state (secondary strength 0)
			ApplyToRenderer();

			// 1. Scale Up Animation
			while(elapsed < appearDuration)
			{
				elapsed += Time.deltaTime;
				var t = Mathf.Clamp01(elapsed / appearDuration);

				// Using a slight overshoot for a "pop" effect
				transform.localScale = Vector3.one * 2;

				yield return null;
			}
			transform.localScale = Vector3.one * 2f;

			// 2. Secondary Rays Fade In
			elapsed = 0;
			while(elapsed < secondaryRaysShowDuration)
			{
				elapsed += Time.deltaTime;
				var t = Mathf.Clamp01(elapsed / secondaryRaysShowDuration);

				// Smooth fade for secondary rays
				_currentSecondaryStrength = Mathf.Lerp(0, _targetSecondaryStrength, t);
				ApplyToRenderer();

				yield return null;
			}

			_currentSecondaryStrength = _targetSecondaryStrength;
			ApplyToRenderer();
			_animationCoroutine = null;
		}

		private IEnumerator HideRoutine()
		{
			float elapsed = 0;
			var startScale = transform.localScale;

			while(elapsed < hideDuration)
			{
				elapsed += Time.deltaTime;
				var t = Mathf.Clamp01(elapsed / hideDuration);

				// Ease in cubic for disappearing
				var scale = 1.0f - t * t * t;
				transform.localScale = startScale * scale;

				yield return null;
			}

			transform.localScale = Vector3.zero;
			_animationCoroutine = null;
		}

		private void ApplyToRenderer()
		{
			if(_renderer == null)
			{
				return;
			}

			_renderer.GetPropertyBlock(_propBlock);

			_propBlock.SetColor(BaseColorId, _targetColor);
			_propBlock.SetFloat(CoreSharpnessId, _targetCoreSharpness);
			_propBlock.SetFloat(RayLengthId, _targetRayLength);
			_propBlock.SetFloat(RayWidthId, _targetRayWidth);
			_propBlock.SetFloat(SecondaryLengthId, _targetSecondaryLength);
			_propBlock.SetFloat(SecondaryWidthId, _targetSecondaryWidth);
			_propBlock.SetFloat(SecondaryStrengthId, _currentSecondaryStrength);
			_propBlock.SetFloat(SecondaryAngleId, _currentRotationAngle);
			_propBlock.SetFloat(GlowRadiusId, _targetGlowRadius);
			_propBlock.SetFloat(PulseSpeedId, _targetPulseSpeed);

			_renderer.SetPropertyBlock(_propBlock);
		}
	}
}
