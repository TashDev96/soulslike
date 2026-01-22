using System;
using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
using game.gameplay_core.camera;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace game.gameplay_core.worldspace_ui
{
	public class FloatingTextView : MonoBehaviour
	{
		public enum TextColorVariant
		{
			Red,
			Green,
			Grey
		}

		public struct Context
		{
			public ICameraController CameraController;
			public ReactiveCommand<float> LocationUpdate;
		}

		private const string GlowColorPropertyName = "_GlowColor";
		private const float FloatSpeed = 0.5f;
		private const float Duration = 1.0f;

		[SerializeField]
		private TMP_Text _text;
		[SerializeField]
		private TMP_Text _textForBevelEffect;
		[SerializeField]
		private SpriteRenderer _glowSprite;

		[SerializeField]
		private SerializableDictionary<TextColorVariant, ColorVariantConfig> _colorVariants;

		private ICameraController _cameraController;
		private IDisposable _updateSubscription;
		private float _timer;
		private ColorVariantConfig _currentConfig;
		private Material _textMaterial;
		private Material _bevelTextMaterial;

		public void Initialize(string text, TextColorVariant color, Vector3 worldPosition, Context context)
		{
			_text.text = text;
			_textForBevelEffect.text = text;

			if(context.CameraController is IsometricCameraController)
			{
				worldPosition -= context.CameraController.Camera.transform.forward * 4f;
			}
			transform.position = worldPosition;

			_cameraController = context.CameraController;
			_currentConfig = _colorVariants[color];

			if(_textMaterial == null)
			{
				_textMaterial = _text.material;
				_bevelTextMaterial = _textForBevelEffect.material;
			}

			SetColor(1f);
			UpdateSpriteWidth();

			_timer = 0;
			_updateSubscription?.Dispose();
			_updateSubscription = context.LocationUpdate.Subscribe(CustomUpdate);
		}

		private void UpdateSpriteWidth()
		{
			_text.ForceMeshUpdate();
			var textWidth = _text.textBounds.size.x;

			if(_glowSprite.sprite != null)
			{
				var spriteWidth = _glowSprite.sprite.bounds.size.x;
				if(spriteWidth > 0)
				{
					var targetScale = textWidth / spriteWidth * 0.5f;
					var localScale = _glowSprite.transform.localScale;
					localScale.x = targetScale;
					_glowSprite.transform.localScale = localScale;
				}
			}
		}

		private void OnDestroy()
		{
			_updateSubscription?.Dispose();
		}

		private void CustomUpdate(float deltaTime)
		{
			_timer += deltaTime;
			var progress = _timer / Duration;

			if(progress >= 1.0f)
			{
				Destroy(gameObject);
				return;
			}

			// Float up in camera space
			transform.position += _cameraController.Camera.transform.up * (FloatSpeed * deltaTime);

			// Fade out
			var alpha = 1.0f - progress;
			SetColor(alpha);
		}

		private void SetColor(float alpha)
		{
			_glowSprite.color = WithAlpha(_currentConfig.SpriteColor, alpha);
			_text.color = WithAlpha(_currentConfig.Color, alpha);
			_textMaterial.SetColor(GlowColorPropertyName, WithAlpha(_currentConfig.GlowColor, alpha));
			_textForBevelEffect.color = WithAlpha(_currentConfig.BevelTextColor, alpha);
			_bevelTextMaterial.SetColor(GlowColorPropertyName, WithAlpha(_currentConfig.BevelTextGlowColor, alpha));
		}

		private Color WithAlpha(Color color, float alpha)
		{
			color.a *= alpha;
			return color;
		}

		private void SetColor(TextColorVariant color)
		{
			_currentConfig = _colorVariants[color];
			SetColor(1f);
		}

#if UNITY_EDITOR
		[Button]
		private void SetColorDebug(TextColorVariant variant)
		{
			if(_textMaterial == null)
			{
				_textMaterial = _text.material;
				_bevelTextMaterial = _textForBevelEffect.material;
			}
			SetColor(variant);
		}
#endif
		[Serializable]
		private struct ColorVariantConfig
		{
			public Color Color;
			public Color GlowColor;

			public Color SpriteColor;
			public Color BevelTextColor;
			public Color BevelTextGlowColor;
		}
	}
}
