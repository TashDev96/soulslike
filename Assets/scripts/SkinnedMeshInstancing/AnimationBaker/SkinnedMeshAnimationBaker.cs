#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace SkinnedMeshInstancing.AnimationBaker.Editor
{
	public class SkinnedMeshAnimationBaker : MonoBehaviour
	{
		[Header("Paths and names")]
		[SerializeField]
		private string _name;
		[SerializeField]
		private bool _createBakedAnimationDataAsset;
		[SerializeField]
		private string _assetPath;
		[SerializeField]
		private string _texturesPath;

		[Header("Properties")]
		[SerializeField]
		private TextureColorDepth _positionTextureColorDepth;
		[SerializeField]
		private TextureSaveType _textureSaveType;
		[SerializeField]
		private float _boundingBoxSize;
		[SerializeField]
		private Vector3 _boundingBoxCenter;
		[SerializeField]
		private Vector3 _rotationOffset;
		[SerializeField]
		private int _framesPerSecond;

		[Header("Refs")]
		[SerializeField]
		private AnimationClip[] _animationClips;
		[SerializeField]
		private SkinnedMeshRenderer _skinnedMeshRenderer;
		[SerializeField]
		private Animator _animator;

		[Button]
		private void Bake()
		{
			EditorCoroutineUtility.StartCoroutine(ProcessBake(), this);
		}

		[Button]
		private void BakeToSeparateMeshes()
		{
			EditorCoroutineUtility.StartCoroutine(ProcessBakeToSeparateMeshes(), this);
		}

		[Button]
		private void DrawRays()
		{
			const int res = 24;
			for(var x = 0; x < res; x++)
			{
				for(var y = 0; y < res; y++)
				{
					for(var z = 0; z < res; z++)
					{
						var posX = -_boundingBoxSize + _boundingBoxSize * 2 * x / (res - 1f);
						var posY = -_boundingBoxSize + _boundingBoxSize * 2 * y / (res - 1f);
						var posZ = -_boundingBoxSize + _boundingBoxSize * 2 * z / (res - 1f);
						var pos = new Vector3(posX, posY, posZ);
						Debug.DrawRay(pos, Vector3.up * 0.01f, VertexPositionToColor(pos, _boundingBoxSize), 10);
					}
				}
			}
		}

		private IEnumerator ProcessBake()
		{
			var vertexCount = _skinnedMeshRenderer.sharedMesh.vertexCount;
			var mesh = new Mesh();

			var timePerFrame = 1f / _framesPerSecond;
			var textureWidth = vertexCount;
			var textureHeight = _animationClips.Sum(clip => Mathf.CeilToInt(clip.length / timePerFrame));

			var normalTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
			Texture2D positionTexture;
			if(_positionTextureColorDepth == TextureColorDepth.BitPerChannel8)
			{
				positionTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
			}
			else
			{
				positionTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA64, false);
			}

			var bakedAnimationData = ScriptableObject.CreateInstance<BakedAnimationData>();
			bakedAnimationData.AnimationClips = new List<BakedAnimationClip>();
			bakedAnimationData.BoundCenter = _boundingBoxCenter;
			bakedAnimationData.BoundSize = _boundingBoxSize;
			bakedAnimationData.SpeedMultiplier = _framesPerSecond / (float)textureHeight;
			bakedAnimationData.TextureWidth = textureWidth;
			bakedAnimationData.Mesh = _skinnedMeshRenderer.sharedMesh;

			var rotationOffsetQuaternion = Quaternion.Euler(_rotationOffset);
			var textureIndexY = 0;
			for(var clipIndex = 0; clipIndex < _animationClips.Length; clipIndex++)
			{
				var clip = _animationClips[clipIndex];
				var framesCount = Mathf.CeilToInt(clip.length / timePerFrame);

				var positionColors = new Color[framesCount * vertexCount];
				var normalColors = new Color[framesCount * vertexCount];

				for(var frameIndex = 0; frameIndex < framesCount; frameIndex++)
				{
					clip.SampleAnimation(_animator.gameObject, timePerFrame * frameIndex);
					_skinnedMeshRenderer.BakeMesh(mesh);

					for(var vId = 0; vId < vertexCount; vId++)
					{
						var colorIndex = frameIndex * vertexCount + vId;
						var vertexPos = rotationOffsetQuaternion * _skinnedMeshRenderer.transform.TransformPoint(mesh.vertices[vId]);
						var vertexNormal = rotationOffsetQuaternion * _skinnedMeshRenderer.transform.TransformDirection(mesh.normals[vId]);

						positionColors[colorIndex] = VertexPositionToColor(vertexPos, _boundingBoxSize);
						normalColors[colorIndex] = VertexNormalToColor(vertexNormal);

						Debug.DrawRay(mesh.vertices[vId], Vector3.up * 0.1f, positionColors[colorIndex], 0.1f);
					}

					yield return null;
				}

				positionTexture.SetPixels(0, textureIndexY, vertexCount, framesCount, positionColors.ToArray());
				normalTexture.SetPixels(0, textureIndexY, vertexCount, framesCount, normalColors.ToArray());

				var clipData = new BakedAnimationClip();

				clipData.Id = clip.name;
				clipData.Start = (float)(textureIndexY + 1) / textureHeight;
				clipData.End = (float)(textureIndexY + framesCount) / textureHeight - 1f / textureHeight;
				clipData.Speed = 1f;

				bakedAnimationData.AnimationClips.Add(clipData);

				textureIndexY += framesCount;
			}

			var textureFolderPath = Path.Combine("Assets", _texturesPath);
			var assetFolderPath = Path.Combine("Assets", _assetPath);
			if(!Directory.Exists(textureFolderPath))
			{
				Directory.CreateDirectory(textureFolderPath);
			}
			if(!Directory.Exists(assetFolderPath))
			{
				Directory.CreateDirectory(assetFolderPath);
			}
			if(_textureSaveType == TextureSaveType.Asset)
			{
				SaveAsAsset(positionTexture, Path.Combine(textureFolderPath, $"AnimTex_{_name}_Pos"));
				SaveAsAsset(normalTexture, Path.Combine(textureFolderPath, $"AnimTex_{_name}_Normal"));
			}
			else
			{
				SaveAsPng(positionTexture, Path.Combine(textureFolderPath, $"AnimTex_{_name}_Pos"));
				SaveAsPng(normalTexture, Path.Combine(textureFolderPath, $"AnimTex_{_name}_Normal"));
			}

			if(_createBakedAnimationDataAsset)
			{
				AssetDatabase.CreateAsset(bakedAnimationData, Path.Combine(assetFolderPath, $"BakedAnimationData_{_name}.asset"));
			}

			AssetDatabase.Refresh();
		}

		private IEnumerator ProcessBakeToSeparateMeshes()
		{
			var timePerFrame = 1f / _framesPerSecond;
			var rotationOffsetQuaternion = Quaternion.Euler(_rotationOffset);
			var skinnedMeshLocalRotation = _skinnedMeshRenderer.transform.localRotation;
			var combinedRotation = rotationOffsetQuaternion * skinnedMeshLocalRotation;

			var assetFolderPath = Path.Combine("Assets", _assetPath);
			if(!Directory.Exists(assetFolderPath))
			{
				Directory.CreateDirectory(assetFolderPath);
			}

			var meshFolderPath = Path.Combine(assetFolderPath, $"Meshes_{_name}");
			if(!Directory.Exists(meshFolderPath))
			{
				Directory.CreateDirectory(meshFolderPath);
			}

			var allMeshes = new List<Mesh>();
			var sequenceAssetPath = Path.Combine(assetFolderPath, $"BakedMeshSequence_{_name}.asset");
			var bakedMeshSequence = AssetDatabase.LoadAssetAtPath<BakedMeshSequence>(sequenceAssetPath);
			
			if (bakedMeshSequence == null)
			{
				bakedMeshSequence = ScriptableObject.CreateInstance<BakedMeshSequence>();
			}
			
			bakedMeshSequence.AnimationClips = new List<MeshSequenceClip>();
			bakedMeshSequence.BoundCenter = _boundingBoxCenter;
			bakedMeshSequence.BoundSize = _boundingBoxSize;
			bakedMeshSequence.FramesPerSecond = _framesPerSecond;

			var currentFrameIndex = 0;

			for(var clipIndex = 0; clipIndex < _animationClips.Length; clipIndex++)
			{
				var clip = _animationClips[clipIndex];
				var framesCount = Mathf.CeilToInt(clip.length / timePerFrame);

				var clipFolderPath = Path.Combine(meshFolderPath, clip.name);
				if(!Directory.Exists(clipFolderPath))
				{
					Directory.CreateDirectory(clipFolderPath);
				}

				var meshSequenceClip = new MeshSequenceClip
				{
					Id = clip.name,
					StartFrame = currentFrameIndex,
					EndFrame = currentFrameIndex + framesCount - 1,
					Speed = 1f
				};

				for(var frameIndex = 0; frameIndex < framesCount; frameIndex++)
				{
					clip.SampleAnimation(_animator.gameObject, timePerFrame * frameIndex);

					var bakedMesh = new Mesh();
					_skinnedMeshRenderer.BakeMesh(bakedMesh);

					var vertices = bakedMesh.vertices;
					var normals = bakedMesh.normals;

					for(var vId = 0; vId < vertices.Length; vId++)
					{
						vertices[vId] = combinedRotation * vertices[vId];
						normals[vId] = combinedRotation * normals[vId];
					}

					bakedMesh.vertices = vertices;
					bakedMesh.normals = normals;
					bakedMesh.RecalculateBounds();

					bakedMesh.name = $"{clip.name}_Frame_{frameIndex:D3}";

					var meshAssetPath = Path.Combine(clipFolderPath, $"{bakedMesh.name}.asset");
					AssetDatabase.CreateAsset(bakedMesh, meshAssetPath);

					allMeshes.Add(bakedMesh);

					yield return null;
				}

				bakedMeshSequence.AnimationClips.Add(meshSequenceClip);
				currentFrameIndex += framesCount;
			}

			bakedMeshSequence.Meshes = allMeshes.ToArray();

			var existingAsset = AssetDatabase.LoadAssetAtPath<BakedMeshSequence>(sequenceAssetPath);
			if (existingAsset == null)
			{
				AssetDatabase.CreateAsset(bakedMeshSequence, sequenceAssetPath);
			}
			else
			{
				EditorUtility.SetDirty(bakedMeshSequence);
				AssetDatabase.SaveAssets();
			}

			AssetDatabase.Refresh();
		}

		private void SaveAsPng(Texture2D tex, string path)
		{
			var pngData = tex.EncodeToPNG();
			File.WriteAllBytes(path + ".png", pngData);
		}

		private void SaveAsAsset(Texture2D tex, string path)
		{
			AssetDatabase.CreateAsset(tex, path + ".asset");
		}

		private void OnDrawGizmos()
		{
			Handles.DrawWireCube(_boundingBoxCenter, Vector3.one * _boundingBoxSize * 2);
		}

		private Color VertexPositionToColor(Vector3 vertexPos, float boundingBoxSize)
		{
			vertexPos -= _boundingBoxCenter;
			var clampedPos = (vertexPos + Vector3.one * boundingBoxSize) / 2f / boundingBoxSize;
			return new Color(clampedPos.x, clampedPos.y, clampedPos.z, 1);
		}

		private Color VertexNormalToColor(Vector3 vertexNormal)
		{
			var clampedNormal = (vertexNormal + Vector3.one) / 2f;
			return new Color(clampedNormal.x, clampedNormal.y, clampedNormal.z, 1);
		}

		private enum TextureColorDepth
		{
			BitPerChannel8,
			BitPerChannel16
		}

		private enum TextureSaveType
		{
			Png,
			Asset
		}
	}
}
#endif
