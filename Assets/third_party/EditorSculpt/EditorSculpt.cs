/*
  Copyright <2025> <h.tomioka>

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the software)
, to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense
, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED as is, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EditorSculptPreference;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


namespace EditorSculptEditor
{
	public class EditorSculpt : EditorWindow
	{
		private enum SculptStatus
		{
			Inactive,
			Active
		}

		private enum BrushModeRemesh
		{
			Move,
			Draw,
			Lower,
			Extrude,
			Dig,
			Inflat,
			Pinch,
			Flatten,
			Smooth,
			Erase,
			VertexColor,
			DrawMask,
			EraseMask,
			VertexWeight,
			EraseWeight,
			ReducePoly,
			IncreasePoly,
			AnimationMove,
			AnimationTip
		}

		private enum BrushMode
		{
			Move,
			Draw,
			Lower,
			Extrude,
			Dig,
			Inflat,
			Pinch,
			Flatten,
			Smooth,
			TexturePaint,
			VertexColor,
			DrawMask,
			EraseMask,
			VertexWeight,
			EraseWeight,
			Move2D,
			AnimationMove,
			AnimationTip
		}

		private enum BrushModeBeta
		{
			Move,
			Draw,
			Lower,
			Extrude,
			Dig,
			Inflat,
			Pinch,
			Flatten,
			Smooth,
			TexturePaint,
			VertexColor,
			DrawMask,
			EraseMask,
			VertexWeight,
			EraseWeight,
			Move2D,
			AnimationMove,
			AnimationTip,

			//BETA_PaitDecal,
			BETA_Decal,
			BETA_DecalSpline,
			BETA_Cut,
			BETA_Spline

			//BETA_PathPaint
		}

		private enum BrushModeRemeshBeta
		{
			Move,
			Draw,
			Lower,
			Extrude,
			Dig,
			Inflat,
			Pinch,
			Flatten,
			Smooth,
			Erase,
			VertexColor,
			DrawMask,
			EraseMask,
			VertexWeight,
			EraseWeight,
			ReducePoly,
			IncreasePoly,
			Move2D,
			AnimationMove,
			AnimationTip,
			BETA_Texture,
			BETA_Decal,
			BETA_DecalSpline,
			BETA_Repair,
			BETA_Cut,
			BETA_Spline,
			BETA_BoneSpike

			//BETA_PathPaint
		}

		private enum EditorSculptMode
		{
			Sculpt,
			RemeshSculpt,
			Beta,
			RemeshBeta
		}

		private enum SplineAction
		{
			DrawSpline,
			EndDraw,
			InsertPoint,
			DeletePoint,
			MovePoint

			//PaintTexture
		}

		private enum SplinePlane
		{
			XY_Near,
			XY_Far,
			YZ_Near,
			YZ_Far,
			ZX_Near,
			ZX_far,
			FREE_3D
		}

		private enum CurveMode
		{
			LineRenderer,
			Mesh,
			Spike
		}

		private enum ReferenceImagePlane
		{
			XY_Plane_Back,
			XY_Plane_Forward,
			YZ_Plane_Left,
			YZ_Plane_Right,
			ZX_Plane_Down,
			ZX_Plane_Up
		}

		private enum SymmetryMode
		{
			None,
			X_axis,
			Y_axis,
			Z_axis
		}

		private enum VColorDisplay
		{
			Standard,
			Rendered,
			VertexColor,
			VertexWeight,
			Texture,
			Bones
		}

		private enum BrushShape
		{
			Normal,
			SoftSolid,
			HardSolid,
			SoftSpike,
			HardSpike
		}

		private enum BoneAction
		{
			None,
			Add,
			Delete

			//Insert
			//EditHuman,
			//HumanWizard,
			//Move
		}

		private enum AnimMoveMode
		{
			Rotate,
			Toranslate,
			Scale,
			Revert,
			Paste
		}

		private enum BoneSelectFlags
		{
			None = 0,
			Humanoid = 1 << 0,
			Generic = 1 << 1,
			Tips = 1 << 2,
			IK = 1 << 3,
			All = ~0
		}

		private enum RefImgDeform
		{
			None = 0,
			InvertU = 1 << 0,
			InvertV = 1 << 1,
			RotateU = 1 << 2,
			RotateV = 1 << 3,
			All = ~0
		}

		private enum Move2DAxis
		{
			XYPlane,
			YZPlane,
			ZXPLane,
			XDirection,
			YDirection,
			ZDirection
		}

		 

		private struct Ikinfo
		{
			public Quaternion rotate;
			public Vector3 position;
		}

		private static SculptStatus currentStatus = SculptStatus.Inactive;
		private static float BrushRadius = 0.5f;
		private static float BrushStrength = 0.5f;
		private static GameObject currentObject;
		private static Mesh currentMesh;
		private static MeshFilter currentMeshFilter;
		private static SkinnedMeshRenderer currentSkinned;
		private static SplineAction splinetype = SplineAction.DrawSpline;
		private static SplinePlane splinepln = SplinePlane.FREE_3D;
		private static CurveMode curveMode = CurveMode.LineRenderer;
		private static ReferenceImagePlane refplane = ReferenceImagePlane.XY_Plane_Forward;
		private static Vector2 refImgOffset = Vector2.zero;
		private static Vector2 refImgScale = Vector2.one;
		private static Vector3 refImgPos = Vector3.zero;
		private static float refImgSize = 1.0f;
		private static bool refimgCenter;
		private static bool IsShortcut;
		private static bool IsDoneSmooth;
		private static SymmetryMode smode = SymmetryMode.None;
		private static VColorDisplay vcdisp = VColorDisplay.Standard;
		private static VColorDisplay oldvcdisp = VColorDisplay.Standard;
		private static string DispString = "Standard";
		private static bool ShowWireframe;
		private static bool AutoMergeVerts;
		private static bool AccurateBrush = true;
		private static bool AutoFixSymmetry = true;
		private static bool AutoSmooth;
		private static bool GlobalBrushRad = true;
		private static bool ShowAdvancedOption;
		private static bool ShowShortcut;
		private static bool ShowEditButtons;
		private static bool ShowAnimationButtons;
		private static bool ShowSaveButtons;
		private static Color BrushColor = Color.black;
		private static readonly int ThreadNum = 4;
		private static bool EnableUndo = true;
		private static BrushModeRemesh btyper = BrushModeRemesh.Move;
		private static BrushMode btype = BrushMode.Move;
		private static BrushModeBeta btypeb = BrushModeBeta.BETA_Decal;
		private static BrushModeRemeshBeta btyperb = BrushModeRemeshBeta.BETA_Decal;
		private static string BrushString = "Move";
		private static string BrushStringOld = "Move";
		private static string DecalString = "";
		private static Mesh blendBaseMesh;
		private static readonly string SaveDir = "";
		private static BoneAction boneAct = BoneAction.None;
		private static Move2DAxis moveAx = Move2DAxis.XYPlane;

		private static EditorSculpt window;
		private static int windowHash;

		//static Texture2D	BrushTex = new Texture2D(64,64);
		private static Texture2D BrushTex;
		private static Texture2D uvtexture;
		private static List<List<int>> uvindex = new();
		private static List<List<Vector3>> uvpos = new();
		private static int[] uvSameIdxs;
		private static Vector2Int[][] uvSameIdxArrArr;
		private static int texwidth = 512;
		private static int texheight = 512;

		//static float textrans = 1.0f;
		private static BrushShape bshape = BrushShape.Normal;
		private static BrushShape bstex = BrushShape.SoftSolid;
		private static float maskWeight = 0.8f;
		private static float retopoWeight = 0.5f;
		private static float autoSmoothDeg = 0.5f;
		private static Color maskColor = Color.black;
		private static Color boneColor = Color.green;
		private static float bonetransp = 0.2f;
		private static float splineOffset = 0.5f;
		private static readonly float decaloffset = 2.0f;
		private static RefImgDeform refImgDeform = RefImgDeform.None;
		private static int refimgInt;
		private static int refimgCnt;

		//static bool refimgLoad = false;
		private static bool ISReferenceTrnsp;
		private static float refTransparentf = 0.5f;
		private static bool ShowReferenceEdit;
		private static bool ShowReferenceButtons;
		private static bool OldShowReference;
		private static bool IsRefereceUpdate;
		private static List<Material> oldMaterilList = new();
		private static Material transparentMat;
		private static string oldrefname = "";
		private static Color guiLineColor = Color.green;
		private static bool AutoSplineProjection;
		private static bool SplineAsMesh;
		private static bool SplineSubdivide;
		private static bool AutoCloseHole = true;
		private static bool AutoFixBlackPoly = true;
		private static EditorSculptMode esmode = EditorSculptMode.Sculpt;
		private static bool IsExportColor = true;
		private static bool IsExportMaterial = true;
		private static bool IsExportESMat;
		private static bool IsExportMerged;
		private static bool IsExportAlpha;
		private static bool IsPaintBrush;
		private static bool IsTexturePaint;
		private static bool IsEditBrush;
		private static bool IsAnimationBrush;
		private static bool IsOldVertList;
		private static bool IsOldNormList;
		private static bool DebugMode;
		private static bool SmoothWithAutoRemesh = true;
		private static bool IsRealTimeAutoRemesh = true;
		private static bool IsDoneRealtimeRemesh;
		private static int primitiveres = 8;
		private static float avgPointDist;
		private static bool IsModelImporter = true;
		private static bool IsCreatePrimitive;
		private static bool IsStartTexture;
		private static bool IsAutoSave = true;
		private static string ImportMatPath = "";
		private static List<string> ImportMatPathList = new();
		private static bool UseModelImporter;
		private static string importMetaPath = "";
		private static string metaContent = "";
		private static float importSize = 1.0f;

		private static Vector2 scrollPosition = Vector2.zero;
		private static Vector2 strokepos = Vector2.zero;
		private static Plane strokePlane;
		private static Vector2 strokePrePos;
		private static Vector2 strokePostPos;
		private static Vector3 strokePreVec;
		private static Vector2 CloseLinePos;
		private static bool[] IsBrushedArray;
		private static int matint;
		private static int startidx;
		private static int paintmatidx;
		private static int oldpaintmatidx;
		private static Vector3 BrushHitPos = Vector3.zero;
		private static Vector3 BrushHitNorm = Vector3.zero;
		private static readonly int BrushHitInt = 0;
		private static bool IsBrushHitPos = true;

		//static bool IsBrushBonePos = true;
		private static Vector3 BrushOldHitPos = Vector3.zero;
		private static Vector3 BrushBoneHitPos;
		private static int decalidx;
		private static float CameraSize;
		private static int DecalInt;

		//static int DecalLayerInt = 0;
		private static int[] mergedtris = { 0 };
		private static int[] tris = { 0 };
		private static int[] BoneIdxArr = { };
		private static int boneidxint;
		private static int oldboneidx;
		private static int delblend;
		private static bool EnableDelBlend;
		private static int delboneidx;

		//static bool EnableDelBone = false;
		private static int parentidx = -1;
		private static int BoneMinIdx = -1;
		private static bool IsBoneTips;
		private static bool IsShowBones;
		private static int aclipidx;
		private static int oldaclipidx;
		private static float animeslider;
		private static float animeKeySlider;
		private static float animeKeyPostSlider;
		private static float oldanimsli;
		private static float AnimePTime;
		private static bool IsHumanLimit = true;
		private static Mesh UnloadMesh;
		private static bool IsBoneMove;
		private static readonly bool IsLimitTrunk = true;
		private static float animeOverrideMin = -1.0f;
		private static float animeOverrideMax = -1.0f;
		private static float animeAclipLength;

		//static Vector3 currentRayDir = Vector3.zero;
		//static Camera currentCam = new Camera();
		private static int BrushSamples = 5;
		private static int LoadTexCnt;
		private static bool isSmoothStroke = true;

		private static List<Vector3> oldVertList = new();
		private static List<Vector3> oldNormalList = new();

		private static List<Vector2> CutLineList = new();
		private static List<List<Vector3>> Spline3DListList = new();
		private static List<List<Vector3>> Spline2DListList = new();
		private static List<List<Vector3>> SplineSubDListList = new();
		private static List<List<Vector3>> SplineVertListList = new();
		private static List<List<Vector3>> Spline3DVertListList = new();
		private static List<List<int>> SplineTriListList = new();
		private static List<List<Vector3>> SplineDirListList = new();
		private static List<List<Vector3>> SplineSubDDirListList = new();
		private static bool IsSplineUpdate;
		private static List<List<Vector3>> Decal2DListList = new();
		private static List<List<Vector3>> Decal3DListList = new();
		private static List<List<int>> DecalTriListList = new();
		private static List<List<Vector2>> DecalUVListList = new();
		private static List<List<Vector3>> DecalDirListList = new();
		private static List<List<Vector3>> DecalBaseListList = new();
		private static readonly Ray[] decalrays = Enumerable.Repeat(new Ray(), 4).ToArray();
		private static readonly bool IsDecalUpdate = false;
		private static bool IsPreviewAnimation;
		private static Mesh startmesh;
		private static int pickerid = -1;
		private static AnimationClip impanim;
		private static readonly bool ShowAllbones = false;
		private static bool IsOptimizeTriangles;
		private static bool IsPreserveTriangles = true;
		private static bool IsMonoSubmesh;
		private static bool IsCalcNormal;
		private static readonly Vector2 aclipminmax = Vector2.up;
		private static Vector3[] extrabones = { Vector3.zero };
		private static BoneSelectFlags boneSelectFlags = BoneSelectFlags.Humanoid | BoneSelectFlags.Generic | BoneSelectFlags.Tips;
		private static bool NeedFrameSelected;
		private static AnimationClip aniclip;
		private static bool IsSkipOverrideAnim = true;

		//static Transform LatestAddedBone;
		//static int LatestBoneIdx = -1;
		private static Dictionary<string, Transform> humanDict = new();
		private static Transform humantrans;
		private static int humanWizardIdx = -1;
		private static bool IsSaved;
		private static AnimMoveMode moveMode = AnimMoveMode.Rotate;

		//static Vector3 spikeRoot = Vector3.zero;
		private static List<Vector3> BrushHitPosList = new();
		private static List<int> BrushHitIntList = new();
		private static bool IsInStroke;
		private static bool IsLazy = true;
		private static int LazyDelayInt = 5;
		private static bool IsBoostPaint = true;
		private static bool IsAccurateSample;
		private static bool IsFixMissing;
		private static string blendShapeName = "BlendShape";
		private static string blendShapeNewName = "BlendShape";
		private static bool ShowRenameBlendShape;
		private static int renameBlend;
		private static bool IsImportDependencies = true;
		private static bool IsPrefabDialog;
		private static GameObject startGO;
		private static bool ShowImportPref;
		private static bool IsLoadMesh;
		private static bool IsHideAutoRemeshMessage;
		private static bool IsOverridePrefab;
		private static bool IsLoadingNow;
		private static bool IsUsePopup;
		private static bool IsUncompressTexture = true;
		private static bool IsDontInstantiate;
		private static bool IsMoveOverlap = true;
		private static readonly Renderer loadRen = null;

		//#if UNITY_2019
		private static bool IsSetupAnimation;
		private static bool IsNeedChangeLightmap;

		//static Lightmapping.GIWorkflowMode oldGIWork = Lightmapping.GIWorkflowMode.Iterative;
		private static bool IsSetupUV;
		private static int oldGIWork;
		private static Mesh memoryMesh;
		private static byte[] memoryTexBytes;
#if MemoryAnimation
		static EditorCurveBinding[] memoryBindings = null;
		static AnimationCurve[] memoryCurves = null;
#endif
		private static bool IsMeshSaved = true;
		private static bool IsTexSaved = true;
#if MemoryAnimation
		static bool IsAnimationSaved = true;
#endif
		private static bool IsSkipDialog;
		private static bool IsAnimationPaste;

		//static float animePasteMin = 0.0f;
		//static float animePasteMax = 0.0f;
		private static float animePasteMin = -1.0f;
		private static float animePasteMax = -1.0f;
		private static bool IsLegacyAnime;
		private static bool IsAnimateMaterial;

		//#endif
		private static List<Quaternion> AnimPoseRotateList = new();
		private static List<Vector3> AnimPoseTraList = new();

		//static HashSet<Transform> BoneMoveHash = new HashSet<Transform>();
		private static bool IsUseKeyFrame = true;

		//static bool IsEditKeyFrame = false;
		private static string SaveFolderPath = "Assets/";
		private static bool IsSaveInImporterFolder;

		//static SaveFolderOptions saveFolder = SaveFolderOptions.SAME_AS_IMPORTER;
		//static Matrix4x4[] oldbind = null;
		private static string ModelImporterPath = "";

		public static bool IsEnable;

		//EditorSculptImportFix importFix;
		//To remember include when compile.

		[MenuItem("Tools/EditorSculpt/Standard Sculpt")]
		private static void EditorSculptWindow()
		{
			if(btype == BrushMode.TexturePaint)
			{
				btype = BrushMode.Move;
			}
			IsTexturePaint = false;
			esmode = EditorSculptMode.Sculpt;
			AnimationQuit();
			ShowWindow();
		}

		[MenuItem("Tools/EditorSculpt/AutoRemesh Sculpt")]
		private static void EditorSculptAdvWindow()
		{
			if(IsHideAutoRemeshMessage)
			{
				IsTexturePaint = false;
				esmode = EditorSculptMode.RemeshSculpt;
				AnimationQuit();
				ShowWindow();
				return;
			}
			if(EditorUtility.DisplayDialog("Warning!", "AutoRemsh Sculpt destroys mesh UVs and BlendShapes." +
			                                           "So, if you want to sculpt textured mesh or mesh with BlendSahpes,"
			                                           + "You should Use Standard Sculpt insted. Do you want to continue AutoRemesh Sculpt?", "Yes", "No"))
			{
				IsTexturePaint = false;
				esmode = EditorSculptMode.RemeshSculpt;
				AnimationQuit();
				ShowWindow();
			}
		}

		[MenuItem("Tools/EditorSculpt/Texture Paint")]
		private static void EditorSculptTextPaint()
		{
			esmode = EditorSculptMode.Sculpt;
			btype = BrushMode.TexturePaint;
			IsPaintBrush = true;
			IsTexturePaint = true;
			IsStartTexture = true;
			try
			{
				LoadTexture(true);
			}
			catch
			{
			}
			AnimationQuit();
			ShowWindow();
		}

		/* [MenuItem("Tools/EditorSculpt/Animation Sculpt")]
		 static void EditorSculptAnimationSculpt()

		 {
		     IsTexturePaint = false;
		     IsAnimationBrush = true;
		     esmode = EditorSculptMode.Sculpt;
		     btype = BrushMode.AnimationMove;
		     AnimationQuit();
		     ShowWindow();
		 }*/

		[MenuItem("Tools/EditorSculpt/Create Primitive/Plane")]
		private static void CreatePlaneMenu()
		{
			CreatePlane();

			SceneView.duringSceneGui -= OnSceneGUI;
			Tools.hidden = false;
			IsEnable = false;
			ShowAnimationButtons = false;
			IsSaved = true;
		}

		//[MenuItem("EditorSculpt/Create Primitive/Sphere", false, 101)]
		[MenuItem("Tools/EditorSculpt/Create Primitive/Sphere")]
		private static void CreateSphereMenu()
		{
			CreateSphere();

			SceneView.duringSceneGui -= OnSceneGUI;
			Tools.hidden = false;
			IsEnable = false;
			ShowAnimationButtons = false;
			IsSaved = true;
		}

		//[MenuItem("EditorSculpt/Create Primitive/Cube", false, 102)]
		[MenuItem("Tools/EditorSculpt/Create Primitive/Cube")]
		private static void CreateCubeMenu()
		{
			CreateCube();

			SceneView.duringSceneGui -= OnSceneGUI;
			Tools.hidden = false;
			IsEnable = false;
			ShowAnimationButtons = false;
			IsSaved = true;
		}

		//[MenuItem("EditorSculpt/Save Mesh/Save", false, 200)]
		[MenuItem("Tools/EditorSculpt/Save Mesh/Save")]
		private static void EditorSculptSaveMenu()
		{
			AnimationQuit();
			GetCurrentMesh(false);
			FixAutoRemeshFinsh();

			//EditorSculptSave();
			EditorSculptPrefab(false, false);
		}

		[MenuItem("Tools/EditorSculpt/Save Mesh/Export Unitypackage")]
		private static void EditorSculptUnitypackageMenu()
		{
			AnimationQuit();
			GetCurrentMesh(false);
			LoadTexture(true);

			//SaveTexture(false);
			FixAutoRemeshFinsh();
			EditorSculptPrefab(false, true);
			var exportpath = EditorUtility.SaveFilePanel("Export Package", "", currentObject.name + ".unitypackage", "unitypackage");
			var PackageStringList = new List<string>();
			PackageStringList.Add(AssetDatabase.GetAssetPath(currentMesh));
			var MaterialList = currentObject.GetComponent<Renderer>().sharedMaterials.ToList();
			for(var i = 0; i < MaterialList.Count; i++)
			{
				PackageStringList.Add(AssetDatabase.GetAssetPath(MaterialList[i]));
			}
			PackageStringList.Add(GetPrefabPath(currentMesh, true));
			AssetDatabase.ExportPackage(PackageStringList.ToArray(), exportpath, ExportPackageOptions.IncludeDependencies);
		}

		//[MenuItem("EditorSculpt/Save Mesh/Export", false, 201)]
		[MenuItem("Tools/EditorSculpt/Save Mesh/Export")]
		private static void EditorSculptExportMenu()
		{
			AnimationQuit();
			GetCurrentMesh(false);
			LoadTexture(true);
			FixAutoRemeshFinsh();
			EditorSculptPrefab(false, true);
			EditorSculptExport();
		}

		//[MenuItem("EditorSculpt/Extra/Try Beta Brushes", false, 300)]
		[MenuItem("Tools/EditorSculpt/Extra/Try Beta Brushes")]
		private static void EditorSculptBetaWindow()
		{
			//if (btypeb == BrushModeBeta.TexturePaint) btypeb = BrushModeBeta.Move;
			IsTexturePaint = false;
			esmode = EditorSculptMode.Beta;
			AnimationQuit();
			ShowWindow();
		}

		[MenuItem("Tools/EditorSculpt/Extra/Try Beta Brushes(AutoReMesh)")]
		private static void EditorSculptRemeshBetaWindow()
		{
			if(IsHideAutoRemeshMessage)
			{
				IsTexturePaint = false;
				esmode = EditorSculptMode.RemeshBeta;
				AnimationQuit();
				ShowWindow();
				return;
			}
			if(EditorUtility.DisplayDialog("Warning!", "AutoRemsh Sculpt destroys mesh UVs and BlendShapes." +
			                                           "So, if you want to sculpt textured mesh or mesh with BlendSahpes,"
			                                           + "You should Use Standard Sculpt insted. Do you want to continue AutoRemesh Sculpt?", "Yes", "No"))
			{
				//if (btyperb == BrushModeRemeshBeta.BETA_Texture) btyperb = BrushModeRemeshBeta.Move;
				IsTexturePaint = false;
				esmode = EditorSculptMode.RemeshBeta;
				AnimationQuit();
				ShowWindow();
			}
		}

		[MenuItem("Tools/EditorSculpt/Extra/AutoRemesh Sculpt NoDialog")]
		private static void EditorSculptAdvWindowNoDialg()
		{
			IsTexturePaint = false;
			esmode = EditorSculptMode.RemeshSculpt;
			AnimationQuit();
			ShowWindow();
		}

		private static void GetCurrentMesh(bool isFrame)
		{
			currentObject = Selection.activeGameObject;

			if(currentObject != null && currentObject.GetComponent<Renderer>() != null &&
			   (currentObject.GetComponent<Renderer>() == null || currentObject.GetComponent<Renderer>().sharedMaterial == null))
			{
				currentObject.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Standard"));
				if(DebugMode)
				{
					Debug.Log("Standard Material is assigned.");
				}
			}
			var rend = currentObject == null ? null : currentObject.GetComponent<Renderer>();
			if(currentObject != null && rend != null)
			{
				var MaterialList = rend.sharedMaterials.ToList();
				var NewMatList = new List<Material>();
				for(var i = 0; i < MaterialList.Count; i++)
				{
					var mat = MaterialList[i];
					if(!mat)
					{
						break;
					}
					if(mat.shader.name == "Custom/EditorSculpt" && !IsExportESMat)
					{
						if(!AssetDatabase.Contains(mat))
						{
							break;
						}
					}
					NewMatList.Add(MaterialList[i]);
				}
				currentObject.GetComponent<Renderer>().sharedMaterials = NewMatList.ToArray();
				matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			}
			if(currentObject != null)
			{
				currentMeshFilter = currentObject.GetComponent<MeshFilter>();
			}
			if(currentObject != null)
			{
				if(currentObject.GetComponent<SkinnedMeshRenderer>())
				{
					currentSkinned = currentObject.GetComponent<SkinnedMeshRenderer>();
					currentMesh = currentSkinned.sharedMesh;
				}
				else if(currentMeshFilter != null)
				{
					currentMesh = currentMeshFilter.sharedMesh;
				}
			}
			if(currentObject == null)
			{
				currentMesh = null;
				currentMeshFilter = null;
			}
			else
			{
				//if (isFrame) SceneView.lastActiveSceneView.FrameSelected();
				if(isFrame)
				{
					if(currentMesh != null)
					{
						SceneView.lastActiveSceneView.Frame(currentMesh.bounds);
					}
				}
			}
		}

		private static string GetSaveFolderPath()
		{
			var retstr = "Assets/";
			if(IsSaveInImporterFolder)
			{
				if(currentObject != null && currentMesh != null)
				{
					var meshPath = AssetDatabase.GetAssetPath(currentMesh);
					if(meshPath.Length > 0)
					{
						try
						{
							if(meshPath.StartsWith("Library/unity default resources"))
							{
								return retstr;
							}
							retstr = Path.GetDirectoryName(meshPath) + "/";
						}
						catch
						{
							return retstr;
						}
					}
				}
			}

			//switch(saveFolder)
			//{
			//    case SaveFolderOptions.ROOT:
			//        retstr = "Assets/";
			//        break;
			//    case SaveFolderOptions.SAME_AS_IMPORTER:
			//        if((currentObject!=null) && (currentMesh!=null))
			//        {
			//            string meshPath = AssetDatabase.GetAssetPath(currentMesh);
			//            retstr = Path.GetDirectoryName(meshPath) + "/";
			//        }
			//        break;
			//    case SaveFolderOptions.NAME_FOLDER:
			//        retstr = EditorUtility.OpenFolderPanel("Folder to save", "", "");
			//        break;

			//}
			return retstr;
		}

		private static Bounds GetBoundsOfAll(GameObject go)
		{
			var allbounds = new Bounds();
			var minvec = Vector3.zero;
			var maxvec = Vector3.zero;
			var meshfs = go.GetComponentsInChildren<MeshFilter>();
			foreach(var meshf in meshfs)
			{
				var bounds = meshf.sharedMesh.bounds;
				if(minvec == Vector3.zero && maxvec == Vector3.zero)
				{
					minvec = bounds.min;
					maxvec = bounds.max;
					continue;
				}
				if(bounds.min.x < minvec.x)
				{
					minvec.x = bounds.min.x;
				}
				if(bounds.min.y < minvec.y)
				{
					minvec.y = bounds.min.y;
				}
				if(bounds.min.z < minvec.z)
				{
					minvec.z = bounds.min.z;
				}
				if(bounds.max.x < maxvec.x)
				{
					maxvec.x = bounds.max.x;
				}
				if(bounds.max.y < maxvec.y)
				{
					maxvec.y = bounds.max.y;
				}
				if(bounds.max.x < maxvec.z)
				{
					maxvec.z = bounds.max.z;
				}
			}
			var skinneds = go.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach(var skinned in skinneds)
			{
				var bounds = skinned.sharedMesh.bounds;
				if(minvec == Vector3.zero && maxvec == Vector3.zero)
				{
					minvec = bounds.min;
					maxvec = bounds.max;
					continue;
				}
				if(bounds.min.x < minvec.x)
				{
					minvec.x = bounds.min.x;
				}
				if(bounds.min.y < minvec.y)
				{
					minvec.y = bounds.min.y;
				}
				if(bounds.min.z < minvec.z)
				{
					minvec.z = bounds.min.z;
				}
				if(bounds.max.x < maxvec.x)
				{
					maxvec.x = bounds.max.x;
				}
				if(bounds.max.y < maxvec.y)
				{
					maxvec.y = bounds.max.y;
				}
				if(bounds.max.x < maxvec.z)
				{
					maxvec.z = bounds.max.z;
				}
			}
			allbounds.min = new Vector3(minvec.x, minvec.y, minvec.z);
			allbounds.max = new Vector3(maxvec.x, maxvec.y, maxvec.z);
			return allbounds;
		}

		//private static Bounds GetBoundsInScene()
		//{
		//    GameObject[] gos = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
		//    Bounds allbounds = new Bounds();
		//    Vector3 minvec = Vector3.zero;
		//    Vector3 maxvec = Vector3.zero;
		//    List<MeshFilter> meshfList = new List<MeshFilter>();
		//    List<SkinnedMeshRenderer> skinnedList = new List<SkinnedMeshRenderer>();
		//    foreach (GameObject go in gos)
		//    {
		//        meshfList.AddRange(go.GetComponentsInChildren<MeshFilter>());
		//        skinnedList.AddRange(go.GetComponentsInChildren<SkinnedMeshRenderer>());
		//    }
		//    foreach (MeshFilter meshf in meshfList)
		//    {
		//        Bounds bounds = meshf.sharedMesh.bounds;
		//        if (minvec == Vector3.zero && maxvec == Vector3.zero)
		//        {
		//            minvec = bounds.min;
		//            maxvec = bounds.max;
		//            continue;
		//        }
		//        if (bounds.min.x < minvec.x) minvec.x = bounds.min.x;
		//        if (bounds.min.y < minvec.y) minvec.y = bounds.min.y;
		//        if (bounds.min.z < minvec.z) minvec.z = bounds.min.z;
		//        if (bounds.max.x < maxvec.x) maxvec.x = bounds.max.x;
		//        if (bounds.max.y < maxvec.y) maxvec.y = bounds.max.y;
		//        if (bounds.max.x < maxvec.z) maxvec.z = bounds.max.z;
		//    }
		//    foreach (SkinnedMeshRenderer skinned in skinnedList)
		//    {
		//        Bounds bounds = skinned.sharedMesh.bounds;
		//        if (minvec == Vector3.zero && maxvec == Vector3.zero)
		//        {
		//            minvec = bounds.min;
		//            maxvec = bounds.max;
		//            continue;
		//        }
		//        if (bounds.min.x < minvec.x) minvec.x = bounds.min.x;
		//        if (bounds.min.y < minvec.y) minvec.y = bounds.min.y;
		//        if (bounds.min.z < minvec.z) minvec.z = bounds.min.z;
		//        if (bounds.max.x < maxvec.x) maxvec.x = bounds.max.x;
		//        if (bounds.max.y < maxvec.y) maxvec.y = bounds.max.y;
		//        if (bounds.max.x < maxvec.z) maxvec.z = bounds.max.z;
		//    }
		//    allbounds.min = new Vector3(minvec.x, minvec.y, minvec.z);
		//    allbounds.max = new Vector3(maxvec.x, maxvec.y, maxvec.z);
		//    return allbounds;
		//}

		//private static void MoveInstanceGameobj(GameObject sourceGO, GameObject go)
		//{
		//    Bounds bounds = GetBoundsOfAll(sourceGO);
		//    //Bounds bounds = GetBoundsInScene();
		//    go.transform.root.position = bounds.max + (bounds.max-bounds.min);
		//    //go.transform.root.position = bounds.max;
		//}

		private static void MoveOverlapGameObject(GameObject gameObj)
		{
			if(!IsMoveOverlap)
			{
				return;
			}
			var gos = SceneManager.GetActiveScene().GetRootGameObjects();
			var poshash = new HashSet<Vector3>();
			foreach(var go in gos)
			{
				poshash.Add(go.transform.position);
			}
			var inspos = gameObj.transform.root.position;
			var bounds = GetBoundsOfAll(gameObj);

			for(var i = 0; i < gos.Length; i++)
			{
				if(!poshash.Contains(inspos))
				{
					break;
				}
				inspos += (bounds.max - bounds.min) * 1.2f;
			}
			gameObj.transform.root.position = inspos;
		}

		private static Mesh GetMeshFromGameObject(GameObject go)
		{
			if(go == null)
			{
				return null;
			}
			SkinnedMeshRenderer skinned = null;
			MeshFilter meshf = null;
			skinned = go.GetComponent<SkinnedMeshRenderer>();

			//go.TryGetComponent<SkinnedMeshRenderer>(out skinned);
			if(skinned != null)
			{
				return skinned.sharedMesh;
			}
			meshf = go.GetComponent<MeshFilter>();

			//go.TryGetComponent<MeshFilter>(out meshf);
			if(meshf != null)
			{
				return meshf.sharedMesh;
			}
			return null;
		}

		public static void ShowWindow()
		{
			IsEnable = true;
			switch(esmode)
			{
				case EditorSculptMode.Sculpt:
					BrushString = Enum.ToObject(typeof(BrushMode), btype).ToString();
					break;
				case EditorSculptMode.RemeshSculpt:
					BrushString = Enum.ToObject(typeof(BrushModeRemesh), btyper).ToString();
					break;
				case EditorSculptMode.Beta:
					BrushString = Enum.ToObject(typeof(BrushModeBeta), btypeb).ToString();
					break;
				case EditorSculptMode.RemeshBeta:
					BrushString = Enum.ToObject(typeof(BrushModeRemeshBeta), btyperb).ToString();
					break;
			}
			BrushStringOld = BrushString;
			IsAnimationBrush = CheckAnimationBrush(BrushString);
			if(IsAnimationBrush && AnimationMode.InAnimationMode())
			{
				AnimationMode.StopAnimationMode();
			}

			LoadPreference();

			Undo.undoRedoPerformed += UndoCallbackFunc;

			if(currentObject != null && currentObject.GetComponent<LineRenderer>())
			{
				var linren = currentObject.GetComponent<LineRenderer>();
				if(linren.name.StartsWith("Decal") || linren.name.StartsWith("Spline") || linren.name.StartsWith("EditorSculptRef"))
				{
					var parento = linren.gameObject.transform.parent.gameObject;
					var meshf = parento.GetComponent<MeshFilter>();
					var IsLinren = false;
					if(meshf != null && meshf.sharedMesh != null)
					{
						var labels = AssetDatabase.GetLabels(meshf.sharedMesh);
						foreach(var str in labels)
						{
							if(str.StartsWith("EditorSculpt"))
							{
								IsLinren = true;
								break;
							}
						}
					}
					if(IsLinren || meshf == null || meshf.sharedMesh == null)
					{
						currentObject = null;
						currentMesh = null;
						return;
					}
				}
			}

			if(Selection.activeGameObject != null)
			{
				startGO = Selection.activeGameObject;
				Selection.activeGameObject = null;
				EditorApplication.delayCall -= DelaySelectGameObject;
				EditorApplication.delayCall += DelaySelectGameObject;
			}

			CutLineList = new List<Vector2>();

			GetWindow(typeof(EditorSculpt));

			//SceneView.onSceneGUIDelegate -= OnSceneGUI;
			//SceneView.onSceneGUIDelegate += OnSceneGUI;
			//SceneView.onSceneGUIDelegate = OnSceneGUI;
			SceneView.duringSceneGui -= OnSceneGUI;
			SceneView.duringSceneGui += OnSceneGUI;

			window = (EditorSculpt)GetWindow(typeof(EditorSculpt));
			window.position = new Rect(10, 200, 280, 320);
			window.titleContent.text = "EditorSculpt";

			//Vector2 wmax = Vector2.zero;
			//wmax.x = 500;
			//wmax.y = 600;
			//window.maxSize = wmax;
			var wmin = new Vector2(280, 320);
			window.minSize = wmin;
			windowHash = window.GetHashCode();

			if(ShowReferenceButtons)
			{
				ReferenceImgShow();
			}

			//OldMaterial = null;
		}

		private void OnEnable()
		{
			//uvtexture = new Texture2D(0, 0);

			//EditorWindow.GetWindow(typeof(EditorSculpt));
			//SceneView.onSceneGUIDelegate = OnSceneGUI;
			//SceneView.onSceneGUIDelegate -= OnSceneGUI;
			//SceneView.onSceneGUIDelegate += OnSceneGUI;
			SceneView.duringSceneGui -= OnSceneGUI;
			SceneView.duringSceneGui += OnSceneGUI;

			//window = (EditorSculpt)EditorWindow.GetWindow(typeof(EditorSculpt));
		}

		private void OnDestroy()
		{
			//Added 2021/02/01
			//if (!IsSaved)
			//{
			//    if (EditorUtility.DisplayDialog("Caution","Mesh hasn't saved yet. Do you want to save?", "OK", "Cancel"))EditorSculptPrefab(false);
			//}
			//Added 2021/09/15
			//if(BrushString== "BETA_Spline")
			AnimationSaveBoneMove(animeslider, 0.0f, false);
			SavePreference();
			if(BrushString == "BETA_Spline" || BrushString == "BETA_CurveMove")
			{
				EditorSculptPrefab(false, true);
				IsSaved = true;
			}

			//End Added 2021/09/15
			if(currentObject != null && currentMesh != null && !IsSaved)
			{
				if(currentObject.GetComponent<SkinnedMeshRenderer>() != null || currentObject.GetComponent<MeshFilter>() != null)
				{
					if(EditorUtility.DisplayDialog("Caution", "Mesh hasn't saved yet. Do you want to save?", "OK", "Cancel"))
					{
						EditorSculptPrefab(false, false);
						IsSaved = true;
						IsMeshSaved = true;
					}
				}
			}
			if(currentObject != null && currentMesh != null && ((memoryMesh != null && !IsMeshSaved) || (memoryTexBytes != null && !IsTexSaved))
			   && !IsSkipDialog)
			{
				if(EditorUtility.DisplayDialog("Caution", "Mesh hasn't saved yet. Do you want to save?", "OK", "Cancel"))
				{
				}
				else
				{
					RestoreMemoryMesh();
				}
				IsMeshSaved = true;
				IsTexSaved = true;
			}

			//if(IsDisabledAnimationImport==true)ModelImporterChangeAnimeImport(true);

			//RevertPreMetaFile();

			//if ((uvtexture !=null) && (!IsTexSaved))
			//{
			//    if (EditorUtility.DisplayDialog("Caution", "The texture hasn't saved yet. Do you want to save?", "OK", "Cancel"))
			//    {
			//    }
			//    else
			//    {
			//        RestoreMemoryTex();
			//    }
			//    IsTexSaved = true;
			//}
			//End Added 2021/02/01
			LineRendererMeshDelete("EditorSculptRef");
			currentStatus = SculptStatus.Inactive;
			IsTexturePaint = false;
			if(IsAnimationBrush)
			{
				RestoreBrush();
			}
			boneAct = BoneAction.None;
			if(bshape == bstex && bshape != BrushShape.Normal)
			{
				bstex = BrushShape.Normal;
			}
			if(bshape == bstex && bshape == BrushShape.Normal)
			{
				bstex = BrushShape.SoftSolid;
			}
			if(!uvtexture)
			{
				uvtexture = new Texture2D(0, 0);
			}
			uvindex = new List<List<int>>();
			uvpos = new List<List<Vector3>>();
			IsSetupUV = false;
			LoadTexCnt = 0;
			ReferenceImageRevertMat();
			ShowReferenceEdit = false;
			FixMaterial();

			//if (currentObject != null && currentMesh != null && currentMeshFilter != null)
			if(currentObject != null && currentMesh != null)
			{
				SubMeshGenerate(currentMesh);
				CheckMonoSubMesh(currentMesh);
			}
			Spline2DListList = new List<List<Vector3>>();
			Spline3DListList = new List<List<Vector3>>();
			SplineDirListList = new List<List<Vector3>>();
			paintmatidx = 0;

			Decal2DListList = new List<List<Vector3>>();
			Decal3DListList = new List<List<Vector3>>();
			DecalTriListList = new List<List<int>>();
			DecalUVListList = new List<List<Vector2>>();
			DecalDirListList = new List<List<Vector3>>();
			DecalBaseListList = new List<List<Vector3>>();
			DecalLoad();
			decalidx = 0;
			DecalInt = 0;
			mergedtris = new int[0];
			uvindex = new List<List<int>>();
			uvpos = new List<List<Vector3>>();
			IsSetupUV = false;
			uvtexture = new Texture2D(0, 0);
			IsPaintBrush = false;
			IsEditBrush = false;
			IsAnimationBrush = false;

			//IsOldImporter = false;
			ImportMatPath = "";
			ImportMatPathList = new List<string>();
			BoneMinIdx = -1;
			aniclip = null;

			//New in 2020/08/17
			aclipidx = 0;
			oldaclipidx = 0;

			//End new in 2020/08/17
			extrabones = new Vector3[] { };
			humanDict = new Dictionary<string, Transform>();

			//LatestBoneIdx = -1;

			//New in 2020/10/22
			var isneedframe = AnimationMode.InAnimationMode();

			//End New in 2020/10/22

			if(currentMesh != null && blendBaseMesh != null)
			{
				BlendShapeCreate();
			}
			AnimationQuit();

			//New in 2020/11/25
			AnimePTime = 0.0f;
			animeslider = 0.0f;
			oldanimsli = 0.0f;

			//oldbind = null;
			//End New in 2020/11/25

			////Added 2025/09/13
			////Fix error of animation
			//if ((currentObject != null) && (currentMesh != null) && (oldbind != null))
			//{
			//    //currentMesh.bindposes = oldbind;
			//    //if (oldbind.Length == currentMesh.bindposeCount) currentMesh.bindposes = oldbind;
			//}
			////End added 2025/09/13

			// //New in 2020/10/22
			//if (isneedframe) SceneView.lastActiveSceneView.FrameSelected();
			// //End New in 2020/10/22
			//OldIsReferenceTransp = false;

			Undo.undoRedoPerformed -= UndoCallbackFunc;

			currentObject = null;
			currentMesh = null;
			currentMeshFilter = null;

			//Resources.UnloadUnusedAssets();
			Tools.hidden = false;
			IsSaved = false;
			IsLoadMesh = false;
			IsLoadingNow = false;

			//StageUtility.GoToMainStage();

			IsEnable = false;
		}

		/*void Awake()
		{
		    if (currentStatus == SculptStatus.Active) this.Close();
		}*/

		private void OnGUI()
		{
			//if (!IsLoadPreference) LoadPreference();
			var flag0 = false;
			if(currentObject != null && currentObject.GetComponent<LineRenderer>())
			{
				var linename = currentObject.GetComponent<LineRenderer>().name;
				if(linename.StartsWith("Decal") || linename.StartsWith("Spline") || linename.StartsWith("EditorSculptRef"))
				{
					flag0 = true;
				}
			}
			if(!window)
			{
				window = (EditorSculpt)GetWindow(typeof(EditorSculpt));
			}

			//#if UNITY_2019
			//if(window.position.width>window.maxSize.x || window.position.height>window.maxSize.y)
			if(EditorGUIUtility.currentViewWidth > window.position.width)
			{
				if(IsEnable)
				{
					var style = new GUIStyle();
					style.normal.textColor = Color.green;
					GUILayout.Label("Now EditorSculpt working", style);
					if(GUILayout.Button("Disable"))
					{
						SceneView.duringSceneGui -= OnSceneGUI;
						Tools.hidden = false;
						IsEnable = false;
						ShowAnimationButtons = false;

						if(currentMesh != null && blendBaseMesh != null)
						{
							BlendShapeCreate();
						}
						AnimationQuit();
						AnimePTime = 0.0f;
						animeslider = 0.0f;
						oldanimsli = 0.0f;
						extrabones = new Vector3[] { };
						humanDict = new Dictionary<string, Transform>();
						humantrans = null;

						//LatestBoneIdx = -1;

						currentStatus = SculptStatus.Inactive;
						Tools.hidden = false;
						IsTexturePaint = false;
						if(IsAnimationBrush)
						{
							RestoreBrush();
						}
						boneAct = BoneAction.None;
					}
				}
				else
				{
					GUILayout.Label("Now EditorSculpt stopping");
					if(GUILayout.Button("Enable"))
					{
						SceneView.duringSceneGui += OnSceneGUI;
						Tools.hidden = true;
						IsEnable = true;
					}
				}
			}

			//#else
			//        if (window.docked)
			//        {
			//            //IsEnable = GUILayout.Toggle(IsEnable, "Enable");
			//            if (Tools.hidden)
			//            {
			//                GUILayout.Label("Now EditorSculpt working");
			//                if (GUILayout.Button("Disable"))
			//                {
			//                    SceneView.duringSceneGui -= OnSceneGUI;
			//                    Tools.hidden = false;
			//                }
			//            }
			//            else
			//            {
			//                GUILayout.Label("Now EditorSculpt stopping");
			//                if (GUILayout.Button("Enable"))
			//                {
			//                    SceneView.duringSceneGui += OnSceneGUI;
			//                    Tools.hidden = true;
			//                }
			//            }
			//        }
			//#endif
			if(flag0 || currentObject == null || currentMesh == null)
			{
				using(var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
				{
					scrollPosition = scroll.scrollPosition;
					if(IsTexturePaint)
					{
						ShowLoadMesh(true);
						GUILayout.Label("");

						//GUILayout.Label("or");
						if(GUILayout.Button("Paint Plane"))
						{
							CreatePlane();
							IsPaintBrush = true;
							IsCreatePrimitive = true;
							LoadTexture(true);
						}
						GUILayout.Label("");
						if(GUILayout.Button("Paint Sphere"))
						{
							CreateSphere();
							IsPaintBrush = true;
							IsCreatePrimitive = true;
							LoadTexture(true);
						}
						GUILayout.Label("");
						if(GUILayout.Button("Paint Cube"))
						{
							CreateCube();
							IsPaintBrush = true;
							IsCreatePrimitive = true;
							LoadTexture(true);
						}
						primitiveres = EditorGUILayout.IntSlider("Primitive Resolution", primitiveres, 1, 12);
						ShowAdvancedOption = EditorGUILayout.Foldout(ShowAdvancedOption, "Show Advanced Options");
						if(ShowAdvancedOption)
						{
							GUIAdvaceOpt();
						}
						GUIChangeWindow(false);
						return;
					}
					ShowLoadMesh(false);
					GUILayout.Label("");

					//GUILayout.Label("or");
					if(GUILayout.Button("Sculpt Plane"))
					{
						CreatePlane();

						//uvtexture = new Texture2D(texwidth, texheight);
						IsPaintBrush = false;
					}
					GUILayout.Label("");
					if(GUILayout.Button("Sculpt Sphere"))
					{
						CreateSphere();

						//uvtexture = new Texture2D(texwidth, texheight);
						IsPaintBrush = false;
					}
					GUILayout.Label("");
					if(GUILayout.Button("Sculpt Cube"))
					{
						CreateCube();

						//uvtexture = new Texture2D(texwidth, texheight);
						IsPaintBrush = false;
					}

					//GUILayout.Label("");
					primitiveres = EditorGUILayout.IntSlider("Primitive Resolution", primitiveres, 1, 12);
					ShowAdvancedOption = EditorGUILayout.Foldout(ShowAdvancedOption, "Show Advanced Options");
					if(ShowAdvancedOption)
					{
						GUIAdvaceOpt();
					}
					GUIChangeWindow(false);
					return;
				}
			}
			if(currentMesh == null || currentMesh.GetTriangles(0).Length <= 0)
			{
				currentStatus = SculptStatus.Inactive;
			}
			if(currentMesh == null || currentMesh.GetTriangles(0).Length <= 0)
			{
				return;
			}

			var oldrefImgSize = refImgSize;
			var oldrefImgScale = refImgScale;
			var oldrefImgOffset = refImgOffset;
			var oldrefImgPos = refImgPos;
			var oldrefplane = refplane;
			var oldrefImgTrans = refImgDeform;
			var oldShowReferenceEdit = ShowReferenceEdit;

			var ImgNumStr = "";
			using(var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
			{
				scrollPosition = scroll.scrollPosition;
				switch(esmode)
				{
					case EditorSculptMode.Sculpt:
						GUILayout.Label("Editor Sculpt(Standard)", EditorStyles.boldLabel);
						btype = (BrushMode)EditorGUILayout.EnumPopup("BrushType", btype);
						break;
					case EditorSculptMode.RemeshSculpt:
						GUILayout.Label("Editor Sculpt(AutoRemesh)", EditorStyles.boldLabel);
						btyper = (BrushModeRemesh)EditorGUILayout.EnumPopup("BrushType", btyper);
						break;

					case EditorSculptMode.Beta:
						GUILayout.Label("Editor Sculpt(StandardBeta)", EditorStyles.boldLabel);
						btypeb = (BrushModeBeta)EditorGUILayout.EnumPopup("BrushType", btypeb);
						break;

					case EditorSculptMode.RemeshBeta:
						GUILayout.Label("Editor Sculpt(AutoRemeshBeta)", EditorStyles.boldLabel);
						btyperb = (BrushModeRemeshBeta)EditorGUILayout.EnumPopup("BrushType", btyperb);
						break;
				}
				BrushRadius = EditorGUILayout.Slider("Brush Radius", BrushRadius, 0, 3);
				BrushStrength = EditorGUILayout.Slider("Brush Strength", BrushStrength, 0, 1);
				vcdisp = (VColorDisplay)EditorGUILayout.EnumPopup("DisplayMode", vcdisp);
				if(oldvcdisp != vcdisp)
				{
					oldvcdisp = vcdisp;
					LoadTexCnt = 0;
				}
				smode = (SymmetryMode)EditorGUILayout.EnumPopup("Symmetry Mode:", smode);
				if(IsPaintBrush || BrushString == "Draw" || BrushString == "Lower")
				{
					LazyDelayInt = EditorGUILayout.IntSlider("Stroke Delay", LazyDelayInt, 1, 5);
				}
				GUI.enabled = true;
				if(currentObject != null && !ShowWireframe)
				{
					EditorUtility.SetSelectedRenderState(currentObject.GetComponent<Renderer>(), EditorSelectedRenderState.Wireframe);
				}
				else if(currentObject != null)
				{
					EditorUtility.SetSelectedRenderState
						(currentObject.GetComponent<Renderer>(), EditorSelectedRenderState.Hidden);
				}
				if(BrushString == "Move2D")
				{
					moveAx = (Move2DAxis)EditorGUILayout.EnumPopup("Move2D Direction:", moveAx);
				}
				if(BrushString == "DrawMask" || BrushString == "EraseMask")
				{
					DrawGUIMask();
				}
				if(BrushString == "VertexColor")
				{
					GUILayout.Label("");
					GUILayout.Label("Vertex Color Brush Options");
					BrushColor = EditorGUILayout.ColorField("Brush Color", BrushColor);
					if(GUILayout.Button("BakeVertexColor"))
					{
						var startt = (float)EditorApplication.timeSinceStartup;
						LoadTexture(true);
						BakeVertexColor(currentMesh);
						var texbytes = uvtexture.EncodeToPNG();

						//File.WriteAllBytes(Application.dataPath + "/../" + "Assets/" + currentObject.name + "_EditorSculpt.png", texbytes);
						File.WriteAllBytes(Application.dataPath + "/../" + SaveFolderPath + currentObject.name + "_EditorSculpt.png", texbytes);
						LoadTexCnt = 0;
						if(DebugMode)
						{
							Debug.Log("Bake Vertex Color Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
							Debug.Log("Texture Width" + uvtexture.width + " " + "Texture Height" + uvtexture.height);
						}
					}
				}
				if(BrushString == "TexturePaint" || BrushString == "BETA_Texture")
				{
					GUILayout.Label("");
					GUILayout.Label("Texture Paint Brush Options");
					var MatNameList = new List<string>();
					var MaterialList = currentObject.GetComponent<Renderer>().sharedMaterials.ToList();
					for(var i = 0; i < MaterialList.Count; i++)
					{
						//if (MaterialList[i].HasProperty("_MainTex") && MaterialList[i].shader.name != "Custom/EditorSculpt")
						//if ((MaterialList[i].mainTexture!=null) && MaterialList[i].shader.name != "Custom/EditorSculpt")
						if(MaterialList[i].GetTexturePropertyNames().Length > 0 && MaterialList[i].shader.name != "Custom/EditorSculpt")
						{
							MatNameList.Add(MaterialList[i].name);
						}
					}
					BrushColor = EditorGUILayout.ColorField("Brush Color", BrushColor);
					paintmatidx = EditorGUILayout.Popup("Material: ", paintmatidx, MatNameList.ToArray());
					if(paintmatidx != oldpaintmatidx)
					{
						oldpaintmatidx = paintmatidx;
						LoadTexCnt = 0;
						if(currentObject != null && currentMesh != null && ((memoryMesh != null &&
						                                                     !IsMeshSaved) || (memoryTexBytes != null && !IsTexSaved)) && !IsSkipDialog)
						{
							if(EditorUtility.DisplayDialog("Caution", "The texture of the previous material " +
							                                          "hasn't saved yet. Do you want to save?", "OK", "Cancel"))
							{
							}
							else
							{
								RestoreMemoryMesh();
							}
							IsMeshSaved = true;
							IsTexSaved = true;
						}

						//if ((currentObject != null) && (currentMesh != null) && (memoryTex != null) && (uvtexture != null) && (!IsTexSaved) && (!IsSkipDialog))
						//{
						//    if (EditorUtility.DisplayDialog("Caution", "The Texture hasn't saved yet. Do you want to save?", "OK", "Cancel"))
						//    {
						//        IsTexSaved = true;
						//    }
						//    else
						//    {
						//        EditorUtility.CopySerialized(memoryTex, uvtexture);
						//        uvtexture.Apply();
						//        LoadTexture(true);
						//        SaveTexture();
						//        ChangeMaterial();
						//    }
						//}

						//uvtexture.Apply();
						//LoadTexture(true);
						//SaveTexture();
						//ChangeMaterial();
					}

					//textrans = EditorGUILayout.Slider("Texture Transparent", textrans, 0, 1.0f);
					if(GUILayout.Button("Create New Material"))
					{
						//Material mat = new Material(Shader.Find("Standard"));
						var mat = new Material(Shader.Find("Custom/EditorSculptTextureMix"));

						//mat.SetFloat("_Mode", 3);
						//mat.SetOverrideTag("RenderType", "Transparent");
						////mat.SetOverrideTag("Queue", "Geometry+1");
						//mat.EnableKeyword("_ALPHATEST_ON");
						//mat.renderQueue = 3000;
						////mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
						////mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
						////mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
						////mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstAlpha);
						////mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);

						//mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
						//mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);

						var matlist = currentObject.GetComponent<Renderer>().sharedMaterials.ToList();
						var maxque = 0;
						for(var i = 0; i < matlist.Count; i++)
						{
							if(matlist[i].renderQueue > maxque)
							{
								maxque = matlist[i].renderQueue;
							}
						}
						mat.renderQueue = maxque + 1;
						var newmatlist = new List<Material>();
						Material endmat = null;
						for(var i = 0; i < matlist.Count; i++)
						{
							if(matlist[i].shader.name == "Custom/EditorSculpt")
							{
								endmat = matlist[i];
							}
							else
							{
								newmatlist.Add(matlist[i]);
							}
						}
						matlist = newmatlist;
						matlist.Add(mat);
						matlist.Add(endmat);

						//matlist.Add(mat);
						currentObject.GetComponent<Renderer>().sharedMaterials = matlist.ToArray();
						currentMesh.subMeshCount++;
						currentMesh.SetTriangles(currentMesh.triangles, currentMesh.subMeshCount - 1);

						paintmatidx = Math.Max(0, matlist.Count - 2);
						var savetex = new Texture2D(texwidth, texheight);
						var y = 0;
						while(y < savetex.height)
						{
							var x = 0;
							while(x < savetex.width)
							{
								var col = Color.white;

								//col.a = textrans;
								col.a = 0.0f;
								savetex.SetPixel(x, y, col);
								++x;
							}
							++y;
						}
						savetex.Apply();
						var matpath = AssetDatabase.GenerateUniqueAssetPath(SaveFolderPath + SaveDir + currentObject.name + "_" +
						                                                    mat.shader.name.Replace("/", "") + ".mat");
						try
						{
							try
							{
								AssetDatabase.StartAssetEditing();
								AssetDatabase.CreateAsset(mat, matpath);
								AssetDatabase.AddObjectToAsset(savetex, matpath);
								AssetDatabase.ImportAsset(matpath);
							}
							finally
							{
								AssetDatabase.StopAssetEditing();
							}

							//SetMainTexture(mat, savetex);
							//CreateAssetAndAddObjFast(mat, matpath, savetex);
							mat.mainTexture = savetex;
						}
						catch
						{
						}
					}
					texwidth = EditorGUILayout.IntField("Texture Width", texwidth);
					texheight = EditorGUILayout.IntField("Texture Height", texheight);
					if(GUILayout.Button("Resize Texture"))
					{
						var resizetex = new Texture2D(texwidth, texheight);
						var y = 0;
						while(y < resizetex.height)
						{
							var x = 0;
							while(x < resizetex.width)
							{
								var cx = Mathf.RoundToInt(x / (float)resizetex.width * uvtexture.width);
								var cy = Mathf.RoundToInt(y / (float)resizetex.height * uvtexture.height);
								var col = uvtexture.GetPixel(cx, cy);
								resizetex.SetPixel(x, y, col);
								++x;
							}
							++y;
						}
						resizetex.Apply();
						for(var i = 0; i < resizetex.height; i++)
						{
							for(var j = 0; j < resizetex.width; j++)
							{
								Color c0 = Color.white,
									c1 = Color.white,
									c2 = Color.white,
									c3 = Color.white,
									c4 = Color.white,
									c5 = Color.white,
									c6 = Color.white,
									c7 = Color.white,
									c8 = Color.white;
								c0 = resizetex.GetPixel(i, j);
								c1 = resizetex.GetPixel(i - 1, j);
								c2 = resizetex.GetPixel(i + 1, j);
								c3 = resizetex.GetPixel(i, j - 1);
								c4 = resizetex.GetPixel(i, j + 1);
								c5 = resizetex.GetPixel(i - 1, j - 1);
								c6 = resizetex.GetPixel(i + 1, j - 1);
								c7 = resizetex.GetPixel(i - 1, j + 1);
								c8 = resizetex.GetPixel(i + 1, j + 1);
								c0 = (c0 * 8.0f + (c1 + c2 + c3 + c4) * 2.0f + c5 + c6 + c7 + c8) * 0.05f;
								resizetex.SetPixel(i, j, c0);
							}
						}
						resizetex.Apply();
						var texbytes = resizetex.EncodeToPNG();
						uvtexture.LoadImage(texbytes);

						//SaveTexture();
						SaveTexture();
						LoadTexture(true);
						LoadTexCnt = 0;
					}
					if(GUILayout.Button("LoadTexture"))
					{
						var loadpath = EditorUtility.OpenFilePanel("Select Texture Image", "", "png");
						var bytes = File.ReadAllBytes(loadpath);
						uvtexture.LoadImage(bytes);
						LoadTexCnt = 0;
					}
					if(GUILayout.Button("ExportTexture"))
					{
						var texbytes = uvtexture.EncodeToPNG();
						var savepath = EditorUtility.SaveFilePanel("Save Texture", "", currentObject.name + "_EditorSculpt.png", "png");
						File.WriteAllBytes(savepath, texbytes);
						AssetDatabase.Refresh();
						LoadTexCnt = 0;
					}
					if(GUILayout.Button("BakeVertexColor"))
					{
						var startt = (float)EditorApplication.timeSinceStartup;
						BakeVertexColor(currentMesh);
						var texbytes = uvtexture.EncodeToPNG();
						LoadTexCnt = 0;

						//File.WriteAllBytes(Application.dataPath + "/../" + "Assets/" + currentObject.name + "_EditorSculpt.png", texbytes);
						File.WriteAllBytes(Application.dataPath + "/../" + SaveFolderPath + currentObject.name + "_EditorSculpt.png", texbytes);
						if(DebugMode)
						{
							Debug.Log("Bake Vertex Color Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
							Debug.Log("Texture Width" + uvtexture.width + " " + "Texture Height" + uvtexture.height);
						}
					}
				}
				else if(BrushString == "VertexWeight" || BrushString == "EraseWeight")
				{
					GUILayout.Label("");
					GUILayout.Label("Vertex Weight Brush Options");
					var BoneIdxStrList = new List<string>();
					for(var i = 0; i < BoneIdxArr.Length; i++)
					{
						BoneIdxStrList.Add("Bone" + BoneIdxArr[i]);
					}
					boneidxint = EditorGUILayout.Popup("Select Weight", boneidxint, BoneIdxStrList.ToArray());
					if(boneidxint != oldboneidx)
					{
						oldboneidx = boneidxint;
						BoneWeight1s2UV4(currentMesh);
					}
				}
				else if(BrushString == "BETA_Spline")
				{
					GUILayout.Label("");
					GUILayout.Label("Spline Brush Options");
					splinetype = (SplineAction)EditorGUILayout.EnumPopup("Spline Action:", splinetype);
					splinepln = (SplinePlane)EditorGUILayout.EnumPopup("Spline Plane:", splinepln);
					curveMode = (CurveMode)EditorGUILayout.EnumPopup("Spline Mode:", curveMode);
					SplineAsMesh = EditorGUILayout.Toggle("Spline as Mesh", SplineAsMesh);
					AutoSplineProjection = EditorGUILayout.Toggle("Auto Spline Projection", AutoSplineProjection);
					SplineSubdivide = EditorGUILayout.Toggle("Subdivide Supline", SplineSubdivide);
					splineOffset = EditorGUILayout.FloatField("Spline Offset", splineOffset);
					guiLineColor = EditorGUILayout.ColorField("Line Color", guiLineColor);
				}
				else if(BrushString == "BETA_Cut")
				{
					GUILayout.Label("");
					GUILayout.Label("Cut Brush Options");
					GUILayout.Label(" Cut Action:");
					if(GUILayout.Button("Reset Cut line"))
					{
						CutLineList = new List<Vector2>();
					}
					if(GUILayout.Button("Cut Now"))
					{
					}
					guiLineColor = EditorGUILayout.ColorField("Line Color", guiLineColor);
				}
				else if(BrushString == "BETA_Decal")
				{
					GUILayout.Label("");
					GUILayout.Label("Decal Brush Options");
					Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
					var DecalStrList = new List<string>();
					foreach(LineRenderer linren in components)
					{
						if(!linren.name.StartsWith("Decal"))
						{
							continue;
						}
						DecalStrList.Add(linren.gameObject.name);
					}
					DecalStrList.Add("Create New Decal");
					DecalInt = EditorGUILayout.Popup("Select Decal:", DecalInt, DecalStrList.ToArray());
					DecalString = DecalStrList[DecalInt];
					if(DecalInt <= DecalStrList.Count - 1)
					{
						LineRendererRenderRefresh("Decal", DecalString, false, false);
					}
					else
					{
						LineRendererRenderRefresh("Decal", "", true, false);
					}
					if(GUILayout.Button("Delete Decal"))
					{
						if(DecalString == "Create New Decal")
						{
							EditorUtility.DisplayDialog("Caution", "Select a decal for delete", "OK");
						}
						else
						{
							if(EditorUtility.DisplayDialog("Caution", "Do you want to delete" + DecalString + "?", "OK", "Cancel"))
							{
								LineRendererDelete(DecalString, true);
							}
						}
					}
					BrushColor = EditorGUILayout.ColorField("Brush Color", BrushColor);
				}
				else if(IsAnimationBrush || BrushString == "BETA_BoneSpike")
				{
					var aclips = AnimationUtility.GetAnimationClips(currentObject.transform.root.gameObject);
					if(aclips.Length < 1)
					{
						BuildSkinnedMeshRenderer();
						if(GUILayout.Button("Setup Animator"))
						{
							AnimatorSetup(true);
						}
					}
					else
					{
						var animt = currentObject.transform.root.gameObject.GetComponent<Animator>();
						var aclipstrlist = new List<string>();
						foreach(var acl in aclips)
						{
							aclipstrlist.Add(acl.name);
						}
						if(aniclip != null)
						{
							aclipstrlist.Add(aniclip.name);
						}
						aclipstrlist.Add("[Import a Animation]");
						aclipstrlist.Add("[Create a new Animation]");
						var aclipstrs = aclipstrlist.ToArray();
						var aclints = Enumerable.Range(0, aclipstrs.Length).ToArray();
						GUILayout.Label("");
						GUILayout.Label("Animation Options");
						if(BrushString != "BETA_BoneSpike")
						{
							moveMode = (AnimMoveMode)EditorGUILayout.EnumPopup("Animation Move Mode", moveMode);
						}
						aclipidx = EditorGUILayout.IntPopup("Select Animation: ", aclipidx, aclipstrs, aclints);

						//if (aclipidx < aclips.Length)
						//{
						//    importclip = aclips[aclipidx];
						//}
						//else if (aclipidx == aclips.Length)
						if(aclipidx == aclips.Length)
						{
							EditorGUIUtility.ShowObjectPicker<AnimationClip>(impanim, true, "", 1);
							aclipidx = 0;
						}
						else if(aclipidx == aclips.Length + 1)
						{
							AnimatorSetup(true);
							aclips = AnimationUtility.GetAnimationClips(currentObject.transform.root.gameObject);
							aclipidx = aclips.Length - 1;

							// //New in 2020/10/06
							//SceneView.lastActiveSceneView.FrameSelected();
							// //End New in 2020/10/06
						}
						var aclip = aclips[aclipidx];

						/*if (blendBaseMesh == null)
						 {
						     if (GUILayout.Button("Start Record Animation"))
						     {
						         if (BuildSkinnedMeshRenderer())
						         {
						             blendBaseMesh = new Mesh();
						             EditorUtility.CopySerialized(currentMesh, blendBaseMesh);
						         }
						     }
						 }
						 else if (currentObject.GetComponent<SkinnedMeshRenderer>())
						 {
						     blendBaseMesh.name = EditorGUILayout.TextField("Blend Shape Name", blendBaseMesh.name);
						     if (GUILayout.Button("Stop Record Animation"))
						     {
						         AnimationSave(animeslider);
						         if (blendBaseMesh != null) AnimationBlendShapeSave2(animeslider);
						         AssetDatabase.Refresh();
						         if (currentMesh == null) return;
						     }
						 }*/
						if(GUI.changed)
						{
							AnimeSliderReset();
						}

						if(GUILayout.Button("Import Animation"))
						{
							EditorGUIUtility.ShowObjectPicker<AnimationClip>(impanim, true, "", 1);
						}
						boneSelectFlags = (BoneSelectFlags)EditorGUILayout.EnumFlagsField("Sculpt Bone Filter", boneSelectFlags);
						IsHumanLimit = EditorGUILayout.Toggle("Limit Humanoid Animation", IsHumanLimit);
						if(GUILayout.Button("Save Animation"))
						{
							AnimationSave(animeslider, 0.0f, false);
						}
						if(GUILayout.Button("Copy Pose"))
						{
							AnimationPoseCopy();
						}
						if(GUILayout.Button("Paste Pose"))
						{
							AnimationPosePaste();
							AnimationSave(animeslider, 0.0f, false);
						}
						if(GUILayout.Button("Bake Animation to a mesh"))
						{
							var bakepath = EditorUtility.SaveFilePanelInProject("Save a baked mesh", currentMesh.name + "_baked", "asset", "Save a baked mesh");

							//if (bakepath != "")BakeAnimationToMesh(bakepath);
							if(bakepath != "")
							{
								BakeAnimationToMesh2(bakepath);
							}
						}
						if(AnimePTime > 0.0f)
						{
							var animef = (float)EditorApplication.timeSinceStartup - AnimePTime;
							if(animef > aclip.length)
							{
								animeslider = 0.0f;
								AnimePTime = 0.0f;
								AnimationPoseLoad(0.0f);

								//Added 2020/11/26
								IsPreviewAnimation = false;

								//End Added 2020/11/26
							}
							else
							{
								animeslider = animef;
								AnimationPoseLoad(animeslider);
							}
							if(GUILayout.Button("Stop Animation"))
							{
								AnimePTime = 0.0f;
							}
						}
						else if(GUILayout.Button("Preview Animation"))
						{
							//if (IsBoneMove) AnimationSave(animeslider);
							//Changed 2020/11/25
							if(IsBoneMove)
							{
								AnimationSaveBoneMove(animeslider, 0.0f, false);

								//AnimationSave(animeslider, 0.0f, false);
								//IsBoneMove = false;
								//BoneMoveHash.Clear();
							}
							else
							{
								AnimationPoseLoad(0.0f);
							}

							//End Changed 2020/11/25

							AnimePTime = (float)EditorApplication.timeSinceStartup;
							animeslider = 0.0f;
							AnimatorSetup(false);
							IsPreviewAnimation = true;
						}
						if(boneAct != BoneAction.Add)
						{
							if(EditorGUILayout.DropdownButton(new GUIContent("Add a new Bone"), FocusType.Keyboard))
							{
								if(BuildSkinnedMeshRenderer())
								{
									var bones = GetAnimatorBones();
									var BoneList = new List<string>();
									for(var i = 0; i < bones.Length; i++)
									{
										BoneList.Add(bones[i].name);
									}
									var menu = new GenericMenu();
									menu.AddItem(new GUIContent("AutoAddBone"), false, OnSelectBoneCallback, BoneList.Count - 1);
									for(var i = 0; i < bones.Length; i++)
									{
										menu.AddItem(new GUIContent("Child of" + "/" + BoneList[i]), false, OnSelectBoneCallback, i);
									}
									menu.ShowAsContext();
								}
							}
						}
						if(boneAct == BoneAction.Add)
						{
							if(GUILayout.Button("Cancel add a new bone"))
							{
								boneAct = BoneAction.None;
								if(IsAnimationBrush)
								{
									AnimatorSetup(false);
								}
							}
						}
						IsSkipOverrideAnim = EditorGUILayout.Toggle("Override without dialog", IsSkipOverrideAnim);
						if(currentSkinned.bones.Length > 0)
						{
							if(GUILayout.Button("Delete a bone"))
							{
								boneAct = BoneAction.Delete;
							}

							//if (GUILayout.Button("Inset bone")) boneAct = BoneAction.Insert;
						}
						GUILayout.Label("Animation Length: " + aclip.length + " sec");
						if(GUILayout.Button("Extend Animation Clip"))
						{
							AnimationExtend(aclip.length * 1.1f);
						}
						if(GUILayout.Button("Shorten Animation Clip"))
						{
							AnimationShorten(aclip.length * 0.9f);
						}
						GUILayout.Label("Animation Start Time: " + aclipminmax.x + " sec");
						GUILayout.Label("Animation End Time: " + aclipminmax.y + " sec");
					}
					if(Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorClosed")
					{
						if(EditorGUIUtility.GetObjectPickerControlID() == 1)
						{
							impanim = (AnimationClip)EditorGUIUtility.GetObjectPickerObject();
							var ishuman = false;
							try
							{
								if(currentObject.transform.root.gameObject.GetComponent<Animator>().avatar.isHuman)
								{
									ishuman = true;
								}
							}
							catch
							{
							}
							if(!ShowAnimationButtons)
							{
								if(impanim == null)
								{
									EditorUtility.DisplayDialog("Caution", "You canceled select the AnimationClip", "OK");
								}
								else if(ishuman && impanim.humanMotion == false)
								{
									EditorUtility.DisplayDialog("Caution",
										"This Animation Clip is a generic clip, that does'nt match to the your mesh's avatar.", "OK");
								}
								else if(!ishuman && impanim.humanMotion)
								{
									EditorUtility.DisplayDialog("Caution",
										"This Animation Clip is a human clip, that does'nt match to the your mesh's avatar.", "OK");
								}
								else
								{
									pickerid = 1;
								}
							}
						}
					}
				}
				GUILayout.Label("");
				ShowEditButtons = EditorGUILayout.Foldout(ShowEditButtons, "Edit Mesh");
				if(ShowEditButtons)
				{
					if(GUILayout.Button("Merge Doubled Vertex"))
					{
						currentStatus = SculptStatus.Inactive;
						if(EnableUndo)
						{
							Undo.RegisterCompleteObjectUndo(currentMesh, "Merge Doubled Vertex" + Undo.GetCurrentGroup());
						}

						//Undo.undoRedoPerformed -= UndoCallbackFunc;
						//Undo.undoRedoPerformed += UndoCallbackFunc;
						FixAutoRemeshFinsh();
						var startt = (float)EditorApplication.timeSinceStartup;
						MergeVertsFast(currentMesh);
						if(DebugMode)
						{
							Debug.Log("Merge Vertex Main:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
						}
						MergetriGenerate(currentMesh);
						ChangeMaterial();
						CalcMeshNormals(currentMesh);
						IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
						LoadTexCnt = 0;
						if(DebugMode)
						{
							Debug.Log("Merge Vertex Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
							Debug.Log("vertices" + currentMesh.vertexCount + " " + "triangles" + currentMesh.triangles.Length);
						}
						avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));
					}
					if(GUILayout.Button("Smooth Mesh"))
					{
						currentStatus = SculptStatus.Inactive;
						if(EnableUndo)
						{
							Undo.RegisterCompleteObjectUndo(currentMesh, "Smooth Mesh" + Undo.GetCurrentGroup());
						}

						//Undo.undoRedoPerformed -= UndoCallbackFunc;
						//Undo.undoRedoPerformed += UndoCallbackFunc;
						var startt = (float)EditorApplication.timeSinceStartup;
						FixAutoRemeshFinsh();
						IsBrushedArray = Enumerable.Repeat(true, currentMesh.vertexCount).ToArray();
						CatmullClarkMerged(currentMesh, 1.0f);
						if(DebugMode)
						{
							Debug.Log("Smooth Mesh Main:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
						}
						ChangeMaterial();
						IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
						LoadTexCnt = 0;
						if(DebugMode)
						{
							Debug.Log("Smooth Mesh Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
							Debug.Log("vertices" + currentMesh.vertexCount + " " + "triangles" + currentMesh.triangles.Length);
						}
						avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));
					}
					DrawGUIMask();
					if(GUILayout.Button("Clear Undo"))
					{
						Undo.ClearUndo(currentMesh);
					}
					if(esmode == EditorSculptMode.RemeshSculpt || esmode == EditorSculptMode.RemeshBeta)
					{
						var polycnt = currentMesh.vertexCount / 500;
						var polycnts = currentMesh.vertexCount / 50 - polycnt * 10;
						EditorGUILayout.LabelField("Mesh Resolution", polycnt + "." + polycnts + "K");
						if(GUILayout.Button("Remesh"))
						{
							currentStatus = SculptStatus.Inactive;
							if(EnableUndo)
							{
								Undo.RegisterCompleteObjectUndo(currentMesh, "Remesh" + Undo.GetCurrentGroup());
							}

							//Undo.undoRedoPerformed -= UndoCallbackFunc;
							//Undo.undoRedoPerformed += UndoCallbackFunc;
							FixAutoRemeshFinsh();
							IsBrushedArray = Enumerable.Repeat(true, currentMesh.vertexCount).ToArray();
							var startt = (float)EditorApplication.timeSinceStartup;
							RemeshPolyWithoutHoles(currentMesh);
							FixBlackPoly(currentMesh);
							if(DebugMode)
							{
								Debug.Log("Remesh Main:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
							}
							IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
							LoadTexCnt = 0;
							CloseHoleFast(currentMesh);
							MergeVertsFast(currentMesh);
							MergetriGenerate(currentMesh);
							CalcMeshNormals(currentMesh);
							if(DebugMode)
							{
								Debug.Log("Remesh Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
								Debug.Log("vertices" + currentMesh.vertexCount + " " + "triangles" + currentMesh.triangles.Length);
							}
							avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));
						}
						if(GUILayout.Button("Subdivide Mesh"))
						{
							currentStatus = SculptStatus.Inactive;
							if(EnableUndo)
							{
								Undo.RegisterCompleteObjectUndo(currentMesh, "Subdivide Mesh" + Undo.GetCurrentGroup());
							}

							//Undo.undoRedoPerformed -= UndoCallbackFunc;
							//Undo.undoRedoPerformed += UndoCallbackFunc;
							FixAutoRemeshFinsh();
							var startt = (float)EditorApplication.timeSinceStartup;
							SubdivideMesh(currentMesh);
							if(DebugMode)
							{
								Debug.Log("Subdivide Mesh Main:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
							}
							CloseHoleFast(currentMesh);
							MergeVertsFast(currentMesh);
							FixBlackPoly(currentMesh);
							MergeVertsFast(currentMesh);
							IsBrushedArray = Enumerable.Repeat(true, currentMesh.vertexCount).ToArray();
							MergetriGenerate(currentMesh);
							CatmullClarkMerged(currentMesh, 0.5f);
							IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
							LoadTexCnt = 0;
							if(DebugMode)
							{
								Debug.Log("Subdivide Mesh Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
								Debug.Log("vertices" + currentMesh.vertexCount + " " + "triangles" + currentMesh.triangles.Length);
							}
							avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));
						}
						if(GUILayout.Button("Solid Subdivide"))
						{
							currentStatus = SculptStatus.Inactive;
							if(EnableUndo)
							{
								Undo.RegisterCompleteObjectUndo(currentMesh, "Solid Subdivide" + Undo.GetCurrentGroup());
							}

							//Undo.undoRedoPerformed -= UndoCallbackFunc;
							//Undo.undoRedoPerformed += UndoCallbackFunc;
							FixAutoRemeshFinsh();
							var startt = (float)EditorApplication.timeSinceStartup;
							SubdivideMesh(currentMesh);
							if(DebugMode)
							{
								Debug.Log("Solid Subdivide Main:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
							}
							MergeVertsFast(currentMesh);
							CloseHoleFast(currentMesh);
							MergeVertsFast(currentMesh);
							FixBlackPoly(currentMesh);
							IsBrushedArray = Enumerable.Repeat(true, currentMesh.vertexCount).ToArray();
							MergetriGenerate(currentMesh);
							IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
							LoadTexCnt = 0;
							if(DebugMode)
							{
								Debug.Log("Solid Subdivide Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
								Debug.Log("vertices" + currentMesh.vertexCount + " " + "triangles" + currentMesh.triangles.Length);
							}
							avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));
						}
						if(GUILayout.Button("Decimate Mesh"))
						{
							currentStatus = SculptStatus.Inactive;
							if(EnableUndo)
							{
								Undo.RegisterCompleteObjectUndo(currentMesh, "Decimate Mesh" + Undo.GetCurrentGroup());
							}

							//Undo.undoRedoPerformed -= UndoCallbackFunc;
							//Undo.undoRedoPerformed += UndoCallbackFunc;
							FixAutoRemeshFinsh();
							MergeVerts(currentMesh);
							var startt = (float)EditorApplication.timeSinceStartup;
							IsBrushedArray = Enumerable.Repeat(true, currentMesh.vertexCount).ToArray();
							DecimateMesh(currentMesh);
							FixBlackPoly(currentMesh);
							if(DebugMode)
							{
								Debug.Log("Decimate Mesh Main:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
							}
							MergeVertsFast(currentMesh);
							CloseHoleFast(currentMesh);
							MergeVerts(currentMesh);
							IsBrushedArray = Enumerable.Repeat(true, currentMesh.vertexCount).ToArray();
							MergetriGenerate(currentMesh);
							CatmullClarkMerged(currentMesh, 0.5f);
							IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
							LoadTexCnt = 0;
							if(DebugMode)
							{
								Debug.Log("Decimate Mesh Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
								Debug.Log("vertices" + currentMesh.vertexCount + " " + "triangles" + currentMesh.triangles.Length);
							}
							avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));
						}
						/*if(GUILayout.Button("Solid Decimate"))
						{
						    currentStatus = SculptStatus.Inactive;
						    if (EnableUndo) Undo.RegisterCompleteObjectUndo(currentMesh, "Solid Decimate" + Undo.GetCurrentGroup());
						    Undo.undoRedoPerformed += UndoCallbackFunc;
						    DecimateMesh(currentMesh);
						    MergeVerts(currentMesh);
						    //CloseHole(currentMesh);
						    //MergeVerts(currentMesh);
						    IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
						    avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));
						}*/
						if(GUILayout.Button("Close Holes"))
						{
							currentStatus = SculptStatus.Inactive;
							if(EnableUndo)
							{
								Undo.RegisterCompleteObjectUndo(currentMesh, "Close Holes" + Undo.GetCurrentGroup());
							}

							//Undo.undoRedoPerformed -= UndoCallbackFunc;
							//Undo.undoRedoPerformed += UndoCallbackFunc;
							FixAutoRemeshFinsh();
							var startt = (float)EditorApplication.timeSinceStartup;
							CloseHoleFast(currentMesh);
							if(DebugMode)
							{
								Debug.Log("Close Holes Main:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
							}
							MergeVertsFast(currentMesh);
							MergetriGenerate(currentMesh);
							CalcMeshNormals(currentMesh);
							if(DebugMode)
							{
								Debug.Log("Close Holes Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
								Debug.Log("vertices" + currentMesh.vertexCount + " " + "triangles" + currentMesh.triangles.Length);
							}
							avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));
							IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
							LoadTexCnt = 0;
						}
						if(GUILayout.Button("Reverse Polygon"))
						{
							currentStatus = SculptStatus.Inactive;
							if(EnableUndo)
							{
								Undo.RegisterCompleteObjectUndo(currentMesh, "Reverse Polygon" + Undo.GetCurrentGroup());
							}

							//Undo.undoRedoPerformed -= UndoCallbackFunc;
							//Undo.undoRedoPerformed += UndoCallbackFunc;
							FixAutoRemeshFinsh();
							var startt = (float)EditorApplication.timeSinceStartup;
							ReversePolygon(currentMesh);
							if(DebugMode)
							{
								Debug.Log("Reverse Polygon Main:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
							}
							MergeVertsFast(currentMesh);

							//mergedtris = currentMesh.GetTriangles(0);
							MergetriGenerate(currentMesh);
							CalcMeshNormals(currentMesh);
							IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
							LoadTexCnt = 0;
							if(DebugMode)
							{
								Debug.Log("Reverse Polygon Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
								Debug.Log("vertices" + currentMesh.vertexCount + " " + "triangles" + currentMesh.triangles.Length);
							}
						}
						if(GUILayout.Button("Fix Symmetry"))
						{
							currentStatus = SculptStatus.Inactive;
							if(EnableUndo)
							{
								Undo.RegisterCompleteObjectUndo(currentMesh, "Fix Symmertry" + Undo.GetCurrentGroup());
							}

							//Undo.undoRedoPerformed -= UndoCallbackFunc;
							//Undo.undoRedoPerformed += UndoCallbackFunc;
							FixAutoRemeshFinsh();
							var startt = (float)EditorApplication.timeSinceStartup;
							SymmetryMeshFast(currentMesh);
							CloseHoleFast(currentMesh);
							MergeVertsFast(currentMesh);
							FixBlackPoly(currentMesh);
							MergetriGenerate(currentMesh);
							CalcMeshNormals(currentMesh);
							IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
							LoadTexCnt = 0;
							if(DebugMode)
							{
								Debug.Log("Fix Symmetry Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
								Debug.Log("vertices" + currentMesh.vertexCount + " " + "triangles" + currentMesh.triangles.Length);
							}
							avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));
						}
						/*if(GUILayout.Button("SpeedTest0"))
						{
						    float startt = (float)EditorApplication.timeSinceStartup;
						    Vector3[] vecarr = currentMesh.vertices;
						    vecarr.ToList().Distinct();
						    List<Vector3> veclist = vecarr.ToList();
						    Debug.Log("Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
						}
						if (GUILayout.Button("SpeedTest1"))
						{
						    float startt = (float)EditorApplication.timeSinceStartup;
						    Vector3[] vecarr = currentMesh.vertices;
						    HashSet<Vector3> vhash = new HashSet<Vector3>();
						    int vcnt0 = vecarr.Length;
						    for(int i=0;i<vcnt0;i++)
						    {
						        vhash.Add(vecarr[i]);
						    }
						    List<Vector3> veclist = vhash.ToList();
						    Debug.Log("Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
						}
						if (GUILayout.Button("SpeedTest2"))
						{
						    float startt = (float)EditorApplication.timeSinceStartup;
						    Vector3[] vecarr = currentMesh.vertices;
						    vecarr.ToList().Distinct();
						    List<Vector3> veclist = vecarr.ToList();
						    Dictionary<Vector3, int> NewVDict = Enumerable.Range(0, vecarr.Length).ToDictionary(i => vecarr[i], i => i);
						    Debug.Log("Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
						}*/
					}
					else
					{
						var polycnt = currentMesh.vertexCount / 500;
						var polycnts = currentMesh.vertexCount / 50 - polycnt * 10;
						EditorGUILayout.LabelField("Mesh Resolution", polycnt + "." + polycnts + "K");
						if(GUILayout.Button("Subdivide Mesh"))
						{
							currentStatus = SculptStatus.Inactive;
							if(EnableUndo)
							{
								Undo.RegisterCompleteObjectUndo(currentMesh, "Subdivide Mesh" + Undo.GetCurrentGroup());
							}

							//Undo.undoRedoPerformed -= UndoCallbackFunc;
							//Undo.undoRedoPerformed += UndoCallbackFunc;
							var startt = (float)EditorApplication.timeSinceStartup;
							SubdivideStandard(currentMesh);
							if(DebugMode)
							{
								Debug.Log("Subdivide Mesh Main:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
							}
							IsBrushedArray = Enumerable.Repeat(true, currentMesh.vertexCount).ToArray();

							//Changed 2020/02/03
							MergeVertsFast(currentMesh);

							//End Changed 2020/02/03
							MergetriGenerate(currentMesh);
							CatmullClarkMerged(currentMesh, 0.5f);
							IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
							LoadTexCnt = 0;
							if(DebugMode)
							{
								Debug.Log("Subdivide Mesh Complete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
								Debug.Log("vertices" + currentMesh.vertexCount + " " + "triangles" + currentMesh.triangles.Length);
							}
							avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));
						}
						if(GUILayout.Button("Solid Subdivide"))
						{
							currentStatus = SculptStatus.Inactive;
							if(EnableUndo)
							{
								Undo.RegisterCompleteObjectUndo(currentMesh, "Solid Subdivide" + Undo.GetCurrentGroup());
							}

							//Undo.undoRedoPerformed -= UndoCallbackFunc;
							//Undo.undoRedoPerformed += UndoCallbackFunc;
							var startt = (float)EditorApplication.timeSinceStartup;
							SubdivideStandard(currentMesh);
							if(DebugMode)
							{
								Debug.Log("Solid Subdivide Main:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
							}
							MergeVertsFast(currentMesh);
							MergetriGenerate(currentMesh);
							CalcMeshNormals(currentMesh);
							IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
							LoadTexCnt = 0;
							if(DebugMode)
							{
								Debug.Log("Solid SubdivideComplete:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
								Debug.Log("vertices" + currentMesh.vertexCount + " " + "triangles" + currentMesh.triangles.Length);
							}
							avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));
						}
					}
				}
				GUILayout.Label("");

				ShowReferenceButtons = EditorGUILayout.Foldout(ShowReferenceButtons, "Reference Image");

				if(ShowReferenceButtons)
				{
					if(EditorGUILayout.DropdownButton(new GUIContent("Create New Reference Image"), FocusType.Keyboard))
					{
						var menu = new GenericMenu();
						menu.AddItem(new GUIContent("XY_Back"), false, OnSelectReferenceCallback, ReferenceImagePlane.XY_Plane_Back);
						menu.AddItem(new GUIContent("XY_Forward"), false, OnSelectReferenceCallback, ReferenceImagePlane.XY_Plane_Forward);
						menu.AddItem(new GUIContent("YZ_Left"), false, OnSelectReferenceCallback, ReferenceImagePlane.YZ_Plane_Left);
						menu.AddItem(new GUIContent("YZ_Right"), false, OnSelectReferenceCallback, ReferenceImagePlane.YZ_Plane_Right);
						menu.AddItem(new GUIContent("ZX_Down"), false, OnSelectReferenceCallback, ReferenceImagePlane.ZX_Plane_Down);
						menu.AddItem(new GUIContent("ZX_Up"), false, OnSelectReferenceCallback, ReferenceImagePlane.ZX_Plane_Up);
						menu.ShowAsContext();
					}
					if(GUILayout.Button("Hide Reference Image"))
					{
						ShowReferenceButtons = false;
					}
					Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
					var ReferenceStrList = new List<string>();
					ReferenceStrList.Add("Select Reference to edit.");
					foreach(LineRenderer linren in components)
					{
						if(!linren.name.StartsWith("EditorSculptRef"))
						{
							continue;
						}
						ReferenceStrList.Add(linren.gameObject.name);
					}

					refimgCnt = ReferenceStrList.Count;
					ISReferenceTrnsp = EditorGUILayout.Toggle("Transparent Mesh", ISReferenceTrnsp);
					if(ISReferenceTrnsp)
					{
						refTransparentf = EditorGUILayout.Slider("Transparent Value", refTransparentf, 0.0f, 1.0f);
					}
					if(ReferenceStrList.Count > 0)
					{
						ShowReferenceEdit = EditorGUILayout.Foldout(ShowReferenceEdit, "Edit Reference Image");
						if(ShowReferenceEdit)
						{
							refimgInt = EditorGUILayout.Popup("Select Reference", refimgInt, ReferenceStrList.ToArray());
							try
							{
								ImgNumStr = ReferenceStrList[refimgInt];
							}
							catch
							{
							}
							LineRendererRenderRefresh("EditorSculptRef", ImgNumStr, true, true);
							if(ImgNumStr != "Select Reference to edit.")
							{
								refplane = (ReferenceImagePlane)EditorGUILayout.EnumPopup("Referernce Image Axis:", refplane);
								if(oldrefplane != refplane)
								{
									oldrefplane = refplane;
									ReferenceImgUpdate(ImgNumStr);
								}
								refImgDeform = (RefImgDeform)EditorGUILayout.EnumFlagsField("Reference Image Deform", refImgDeform);
								if(GUILayout.Button("Delete Reference Image"))
								{
									if(EditorUtility.DisplayDialog("Caution", "Do you want to delete" + ImgNumStr + "?", "OK", "Cancel"))
									{
										LineRendererDelete(ImgNumStr, true);
										refimgInt = 0;
									}
									else
									{
										LineRendererRenderRefresh("EditorSculptRef", "", true, true);
									}
								}
								if(GUILayout.Button("Change Texture"))
								{
									IsBrushHitPos = false;
									BrushHitPos = Vector3.zero;
									var loadpath = EditorUtility.OpenFilePanelWithFilters("Select Texture Image", ""
										, new[] { "Image files", "png,jpg,jpeg", "All files", "*" });
									if(loadpath != null)
									{
										if(loadpath.Length > 0)
										{
											EditorUtility.DisplayDialog("Caution", "Rerference Image Changed.", "OK");
											ReferenceTextureLoad(loadpath, ImgNumStr);
											refImgOffset = Vector2.zero;
											refImgScale = Vector2.one;
											refImgPos = Vector3.zero;
											ReferenceImgUpdate(ImgNumStr);
										}
									}
									IsBrushHitPos = true;
								}
								refimgCenter = EditorGUILayout.Toggle("Center Reference Image", refimgCenter);
								refImgSize = EditorGUILayout.FloatField("Reference Image Size", refImgSize);
								refImgPos = EditorGUILayout.Vector3Field("Reference Image Position", refImgPos);
								refImgOffset = EditorGUILayout.Vector2Field("Reference Image Offset", refImgOffset);
								refImgScale = EditorGUILayout.Vector2Field("Reference Image Scale", refImgScale);
							}
						}
					}
				}
				GUILayout.Label("");

				ShowAnimationButtons = EditorGUILayout.Foldout(ShowAnimationButtons, "Animation");
				if(ShowAnimationButtons && !IsEnable)
				{
					EditorUtility.DisplayDialog("Caution", "You cann't use Animation with EditorSculpt disabled. Press Enable button on the top of the window and try again.", "OK");
					ShowAnimationButtons = false;
				}
				if(ShowAnimationButtons)
				{
					blendShapeName = EditorGUILayout.TextField("Blend Shape Name", blendShapeName);
					if(blendBaseMesh == null)
					{
						if(esmode == EditorSculptMode.RemeshBeta || esmode == EditorSculptMode.RemeshSculpt)
						{
						}
						else if(GUILayout.Button("Start Record BlendShape"))
						{
							if(BuildSkinnedMeshRenderer())
							{
								if(PlayerSettings.gpuSkinning)
								{
									if(!BlendShapeUnityVer())
									{
										if(EditorUtility.DisplayDialog("Caution", "Recording Blend Shape with GPU Skinning is buggy." +
										                                          "Do you want to turn GPU SKinning off?", "OK", "No"))
										{
											PlayerSettings.gpuSkinning = false;
										}
									}
								}
								blendBaseMesh = new Mesh();
								EditorUtility.CopySerialized(currentMesh, blendBaseMesh);
							}
						}
					}
					else if(currentObject.GetComponent<SkinnedMeshRenderer>())
					{
						//blendBaseMesh.name = EditorGUILayout.TextField("Blend Shape Name", blendBaseMesh.name);
						if(GUILayout.Button("Stop Record BlendShape"))
						{
							blendBaseMesh.name = blendShapeName;
							BlendShapeCreate();
							AssetDatabase.Refresh();
							NeedFrameSelected = true;
							if(currentMesh == null)
							{
								return;
							}
						}
					}
					if(currentMesh.blendShapeCount > 0)
					{
						if(EnableDelBlend)
						{
							var BlendList = new List<string>();
							for(var i = 0; i < currentMesh.blendShapeCount; i++)
							{
								BlendList.Add(currentMesh.GetBlendShapeName(i));
							}
							delblend = EditorGUILayout.Popup("Delete BlendShape:", delblend, BlendList.ToArray());
							if(GUILayout.Button("Delete"))
							{
								if(EditorUtility.DisplayDialog("Confirm Delete",
									   "Delete BlendShape " + currentMesh.GetBlendShapeName(delblend) + "?", "OK", "No"))
								{
									if(EnableUndo)
									{
										Undo.RegisterCompleteObjectUndo(currentMesh, "Delete BlendShape" + Undo.GetCurrentGroup());
									}

									//Undo.undoRedoPerformed -= UndoCallbackFunc;
									//Undo.undoRedoPerformed += UndoCallbackFunc;
									BlendShapeDelete(delblend, currentMesh);
									AssetDatabase.Refresh();
								}
							}
						}
						EnableDelBlend = EditorGUILayout.Toggle("Enable Delete BlendShapes", EnableDelBlend);
						ShowRenameBlendShape = EditorGUILayout.Foldout(ShowRenameBlendShape, "RenameBlendShape");
						if(ShowRenameBlendShape)
						{
							blendShapeNewName = EditorGUILayout.TextField("New Blend Shape Name", blendShapeNewName);
							var BlendList = new List<string>();
							for(var i = 0; i < currentMesh.blendShapeCount; i++)
							{
								BlendList.Add(currentMesh.GetBlendShapeName(i));
							}
							renameBlend = EditorGUILayout.Popup("Rename BlendShape:", renameBlend, BlendList.ToArray());
							if(GUILayout.Button("Rename"))
							{
								if(BlendList.Contains(blendShapeNewName))
								{
									EditorUtility.DisplayDialog("Caution", "The mesh has BlendShape with same name. " +
									                                       "Use another new Blend Shape name.", "OK");
								}
								else
								{
									BlendShapeRename(renameBlend, blendShapeNewName, currentMesh);
								}
							}
						}
						GUILayout.Label("");
						if(GUILayout.Button("Bake Blend Shape"))
						{
							BakeBlendShape(renameBlend, currentMesh);
						}
					}
					IsShowBones = EditorGUILayout.Toggle("Show the Bones", IsShowBones);
					if(boneAct != BoneAction.Add)
					{
						if(EditorGUILayout.DropdownButton(new GUIContent("Add a new Bone"), FocusType.Keyboard))
						{
							if(BuildSkinnedMeshRenderer())
							{
								//Transform[] bones = currentSkinned.bones;
								var bones = GetAnimatorBones();
								var BoneList = new List<string>();
								for(var i = 0; i < bones.Length; i++)
								{
									BoneList.Add(bones[i].name);
								}
								var menu = new GenericMenu();
								menu.AddItem(new GUIContent("AutoAddBone"), false, OnSelectBoneCallback, BoneList.Count - 1);
								for(var i = 0; i < bones.Length; i++)
								{
									menu.AddItem(new GUIContent("Child of" + "/" + BoneList[i]), false, OnSelectBoneCallback, i);
								}
								menu.ShowAsContext();
							}
						}
					}
					if(boneAct == BoneAction.Add)
					{
						if(GUILayout.Button("Cancel Add a new Bone"))
						{
							boneAct = BoneAction.None;

							//if (BrushString == "AnimationMove") AnimatorSetup(false);
							if(IsAnimationBrush)
							{
								AnimatorSetup(false);
							}
						}
					}
					if(GUILayout.Button("Setup Animator"))
					{
						AnimatorSetup(true);
					}
					if(currentObject.GetComponent<SkinnedMeshRenderer>())
					{
						if(currentSkinned.bones.Length > 0)
						{
							if(GUILayout.Button("Delete Bone"))
							{
								boneAct = BoneAction.Delete;
							}
						}
					}
					else if(currentObject.GetComponent<MeshFilter>())
					{
						if(GUILayout.Button("Build SkinnedMeshRenderer"))
						{
							currentMesh.RecalculateBounds();
							var skinned = new SkinnedMeshRenderer();

							//skinned = ObjectFactory.AddComponent<SkinnedMeshRenderer>(currentObject);
							skinned = Undo.AddComponent<SkinnedMeshRenderer>(currentObject);
							skinned.sharedMesh = currentMesh;
							skinned.localBounds = currentMesh.bounds;
							EditorSculptPrefab(false, true);
						}
					}
					if(GUILayout.Button("Recalculate Bounds"))
					{
						currentMesh.RecalculateBounds();
						try
						{
							currentSkinned.localBounds = currentMesh.bounds;
							EditorSculptPrefab(false, true);
						}
						catch
						{
						}
					}
					if(GUILayout.Button("Reset Bindpose"))
					{
						ResetBindPose2();
					}
					if(GUILayout.Button("Import Animation"))
					{
						EditorGUIUtility.ShowObjectPicker<AnimationClip>(impanim, true, "", 1);
					}
					if(Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorClosed")
					{
						if(EditorGUIUtility.GetObjectPickerControlID() == 1)
						{
							impanim = (AnimationClip)EditorGUIUtility.GetObjectPickerObject();
							var ishuman = false;
							try
							{
								if(currentObject.transform.root.gameObject.GetComponent<Animator>().avatar.isHuman)
								{
									ishuman = true;
								}
							}
							catch
							{
							}
							if(impanim == null)
							{
								EditorUtility.DisplayDialog("Caution", "You canceled select the AnimationClip", "OK");
							}
							else if(ishuman && impanim.humanMotion == false)
							{
								EditorUtility.DisplayDialog("Caution",
									"This Animation Clip is a generic clip, that does'nt match to the your mesh's avatar.", "OK");
							}
							else if(!ishuman && impanim.humanMotion)
							{
								EditorUtility.DisplayDialog("Caution",
									"This Animation Clip is a human clip, that does'nt match to the your mesh's avatar.", "OK");
							}
							else
							{
								pickerid = 1;
							}
						}
					}
					var isnoanim = false;
					try
					{
						var animt = currentObject.transform.root.gameObject.GetComponent<Animator>();
						var aclips = animt.runtimeAnimatorController.animationClips;
						var aclipstrlist = new List<string>();
						foreach(var acl in aclips)
						{
							aclipstrlist.Add(acl.name);
						}
						if(aniclip != null)
						{
							aclipstrlist.Add(aniclip.name);
						}
						var aclipstrs = aclipstrlist.ToArray();
						aclipidx = EditorGUILayout.Popup("Select Animation: ", aclipidx, aclipstrs);
						var aclip = aclips[aclipidx];

						//animeslider = GUILayout.HorizontalSlider(animeslider, 0.0f, aclip.length, GUILayout.Width(150));
						if(AnimePTime > 0.0f)
						{
							var animef = (float)EditorApplication.timeSinceStartup - AnimePTime;

							//if (animef > aclip.length) { AnimePTime = 0.0f; }
							//if (animef > aclip.length) { AnimePTime = (float)EditorApplication.timeSinceStartup; }
							if(animef > aclip.length)
							{
								AnimePTime = (float)EditorApplication.timeSinceStartup;

								//New in 2020/11/26
								IsPreviewAnimation = false;

								//End New in 2020/11/26

								//New in 2021/11/23
								animeslider = 0.0f;
								AnimePTime = 0.0f;
								AnimationPoseLoad(0.0f);
								AnimationStop();

								//End New in 2021/11/23
							}

							//Changed 2021/11/20
							//else { AnimationPoseLoad(animef); }
							else
							{
								animeslider = animef;
								AnimationPoseLoad(animeslider);
								if(GUILayout.Button("Stop Animation"))
								{
									AnimationStop();
								}
							}

							//End Changed 2021/11/20
							//if (GUILayout.Button("Stop Animation")) AnimationStop();
						}
					}
					catch
					{
						isnoanim = true;
					}
					if(AnimePTime <= 0.0f)
					{
						if(GUILayout.Button("Preview Animation"))
						{
							//Added 2020/11/25
							if(IsBoneMove)
							{
								//AnimationSave(animeslider, 0.0f, false);
								//IsBoneMove = false;
								AnimationSaveBoneMove(animeslider, 0.0f, false);

								//BoneMoveHash.Clear();
							}
							else
							{
								AnimationPoseLoad(0.0f);
							}

							//End Addded 2020/11/25

							if(isnoanim)
							{
								AnimatorSetup(false);
							}
							IsPreviewAnimation = true;
							AnimePTime = (float)EditorApplication.timeSinceStartup;
							animeslider = 0.0f;
						}
					}
				}
				GUILayout.Label("");

				ShowSaveButtons = EditorGUILayout.Foldout(ShowSaveButtons, "Save / Export");
				if(ShowSaveButtons)
				{
					if(GUILayout.Button("Save"))
					{
						FixAutoRemeshFinsh();
						EditorSculptPrefab(false, false);
						DestroyImmediate(memoryMesh);
						memoryMesh = new Mesh();
						EditorUtility.CopySerialized(currentMesh, memoryMesh);
						memoryTexBytes = uvtexture.GetRawTextureData();
						IsMeshSaved = true;
						IsTexSaved = true;
						GUIUtility.ExitGUI();
					}
					if(GUILayout.Button("RestoreChanges"))
					{
						if(EditorUtility.DisplayDialog("Caution", "Restore change of mesh and start over?", "OK", "NO"))
						{
							RestoreMemoryMesh();
							IsMeshSaved = true;
							IsTexSaved = true;
						}
					}
					if(GUILayout.Button("Export unitypackage"))
					{
						LoadTexture(true);
						FixAutoRemeshFinsh();
						EditorSculptPrefab(false, true);
						var exportpath = EditorUtility.SaveFilePanel("Export Package", "", currentObject.name + ".unitypackage", "unitypackage");
						var PackageStringList = new List<string>();
						PackageStringList.Add(AssetDatabase.GetAssetPath(currentMesh));
						var MaterialList = currentObject.GetComponent<Renderer>().sharedMaterials.ToList();
						for(var i = 0; i < MaterialList.Count; i++)
						{
							PackageStringList.Add(AssetDatabase.GetAssetPath(MaterialList[i]));
						}
						PackageStringList.Add(GetPrefabPath(currentMesh, true));
						AssetDatabase.ExportPackage(PackageStringList.ToArray(), exportpath, ExportPackageOptions.IncludeDependencies);
					}
					if(GUILayout.Button("Export OBJ"))
					{
						LoadTexture(true);
						FixAutoRemeshFinsh();
						EditorSculptPrefab(false, true);
						EditorSculptExport();
					}
					IsExportColor = EditorGUILayout.Toggle("Export Vertex Color", IsExportColor);
					IsExportMaterial = EditorGUILayout.Toggle("Export / Save Materials", IsExportMaterial);
					IsExportESMat = EditorGUILayout.Toggle("Export EditorSculpt Mat", IsExportESMat);
					IsExportMerged = EditorGUILayout.Toggle("Merge Doubled Vertex", IsExportMerged);
					IsExportAlpha = EditorGUILayout.Toggle("Export Texture Alpha", IsExportAlpha);
				}
				GUILayout.Label("");

				if(currentMesh != null)
				{
					currentStatus = SculptStatus.Active;
				}
				Repaint();

				//Texture2D BrushTex = new Texture2D(64,64);
				if(!BrushTex && ShowAdvancedOption)
				{
					BrushTex = new Texture2D(64, 64);
				}
				if(bstex != bshape && BrushTex && ShowAdvancedOption)
				{
					var y = 0;
					while(y < BrushTex.height)
					{
						var x = 0;
						while(x < BrushTex.width)
						{
							var centpos = new Vector3(32.0f, 0, 0);
							var dpos = new Vector3(x, y, 0);
							var dist = GetStroke(dpos, centpos);
							Color color;
							if(dist / 48.0f < 0.9f)
							{
								color = Color.gray;
							}
							else
							{
								color = Color.white;
							}
							BrushTex.SetPixel(x, y, color);
							++x;
						}
						++y;
					}
					BrushTex.Apply();
					bstex = bshape;
				}
				GUILayout.Label("");
				ShowAdvancedOption = EditorGUILayout.Foldout(ShowAdvancedOption, "Show Advanced Options");
				if(ShowAdvancedOption)
				{
					GUIAdvaceOpt();
				}
				GUILayout.Label("");
				ShowShortcut = EditorGUILayout.Foldout(ShowShortcut, "Show keyboard shortcuts");
				if(ShowShortcut)
				{
					GUILayout.Label("Shift - Smooth");
					GUILayout.Label("Alt -Inverse Brush");
					GUILayout.Label("Ctrl - Draw Mask");
					GUILayout.Label("Ctrl+Alt - Erase Mask");
					GUILayout.Label("Ctrl+Shift - Smooth Mask");
					GUILayout.Label("Ctrl+Z - Undo Sculpt");
					GUILayout.Label("Ctrl+Y - Redo Sculpt");
				}
			}
			if(GUI.changed || IsCreatePrimitive || IsStartTexture || IsRefereceUpdate)
			{
				var flag1 = false;
				var IsAnimationBrushRevert = false;
				IsRefereceUpdate = false;
				if(IsCreatePrimitive || IsStartTexture)
				{
					var verintlist = UnityVersionToIntList(Application.unityVersion);
					if(verintlist[0] == 2019 && verintlist[1] <= 2)
					{
						AssetDatabase.Refresh();
					}
					flag1 = true;
				}
				IsCreatePrimitive = false;
				IsStartTexture = false;
				DispString = Enum.ToObject(typeof(VColorDisplay), vcdisp).ToString();
				switch(esmode)
				{
					case EditorSculptMode.Sculpt:
						BrushString = Enum.ToObject(typeof(BrushMode), btype).ToString();
						break;
					case EditorSculptMode.RemeshSculpt:
						BrushString = Enum.ToObject(typeof(BrushModeRemesh), btyper).ToString();
						break;

					case EditorSculptMode.Beta:
						BrushString = Enum.ToObject(typeof(BrushModeBeta), btypeb).ToString();
						break;
					case EditorSculptMode.RemeshBeta:
						BrushString = Enum.ToObject(typeof(BrushModeRemeshBeta), btyperb).ToString();
						break;
				}

				//IsOldTextureBrush = ((BrushStringOld == "TexturePaint") || (BrushStringOld == "BETA_Texture")) ? true : false;
				if(BrushString != BrushStringOld)
				{
					if(DebugMode)
					{
						Debug.Log("Brush Type Changed to :" + BrushString);
					}
					LoadTexCnt = 0;
					if(boneAct != BoneAction.None)
					{
						EditorUtility.DisplayDialog("Caution", "You canceled add bone", "OK");
						boneAct = BoneAction.None;
					}
					if(CheckAnimationBrush(BrushStringOld))
					{
#if MemoryAnimation
						if((currentObject != null) && (currentMesh != null) && (memoryBindings != null) && (memoryCurves != null)
						   && (!IsAnimationSaved) && (!IsSkipDialog) && (!CheckAnimationBrush(BrushString)))
						{
							if(EditorUtility.DisplayDialog("Caution", "The Animation Clip hasn't saved yet. Do you want to save?", "OK", "Cancel"))
							{
							}
							else
							{
								AnimationClip[] aclips = AnimationUtility.GetAnimationClips(currentObject.transform.root.gameObject);
								if(aclips.Length > 0 && aclipidx < aclips.Length)
								{
									AnimationClip aclip = aclips[aclipidx];
									AnimationUtility.SetEditorCurves(aclip, memoryBindings, memoryCurves);

									//foreach (AnimationCurve memoryCurve in memoryCurves)
									//{
									//    foreach (EditorCurveBinding memoryBinding in memoryBindings)
									//    {
									//        AnimationUtility.SetEditorCurve(aclip, memoryBinding, memoryCurve);
									//    }
									//}
								}
								AnimationPoseLoad(animeslider);
							}
						}
#endif
						IsAnimationBrushRevert = true;

						AnimeSliderReset();

						//if (IsBoneMove) AnimationSave(animeslider, 0.0f, false);
						AnimationSaveBoneMove(animeslider, 0.0f, false);
						AnimationStop();

						//SceneView.lastActiveSceneView.Frame(currentMesh.bounds);
					}
					if(CheckAnimationBrush(BrushString))
					{
						IsAnimationBrushRevert = false;
						if(!IsEnable)
						{
							EditorUtility.DisplayDialog("Caution", "You cann't use Animation with EditorSculpt disabled. Press Enable button on the top of the window and try again.", "OK");

							//Debug.Log("BrushString:" + BrushString + "   BrushOld:" + BrushStringOld);
							if(!CheckAnimationBrush(BrushStringOld))
							{
								IsAnimationBrush = true;
							}
							else
							{
								BrushString = "Move";
								BrushStringOld = "Move";
							}
							RestoreBrush();
						}
						else
						{
							//#if UNITY_2019
							var IsOldUnity = Application.unityVersion.StartsWith("2019");
							if(IsOldUnity)
							{
								startGO = currentObject;
								IsSetupAnimation = true;
							}

							//#endif
							//NeedFrameSelected = true;
							AnimatorSetup(false);

							//#if UNITY_2019
							if(IsOldUnity)
							{
								EditorApplication.delayCall -= DelaySelectGameObjectAnim;
								EditorApplication.delayCall += DelaySelectGameObjectAnim;
							}

							//#endif
						}

						//bool hasAnimator = AnimatorSetup(false);
						//if(hasAnimator)EditorUtility.DisplayDialog("Caution", "Setup Animator finished.Chnage the Brush to Animation Brush again to start Animation Sculpt", "OK");
					}
					else if(BrushString == "BETA_BoneSpike")
					{
						//NeedFrameSelected = true;
						AnimatorSetup(false);
					}
					else if(BrushStringOld == "BETA_Spline" || BrushStringOld == "BETA_Decal")
					{
						ExitSplineBrush();
						EditorSculptPrefab(false, true);
					}
				}

				//IsOldTextureBrush = (BrushStringOld == "TexturePaint" BrushString);
				if(BrushString == "VertexColor")
				{
					IsPaintBrush = true;
				}
				else if(BrushString == "DrawMask" || BrushString == "EraseMask")
				{
					IsPaintBrush = true;
				}
				else if(BrushString == "VertexWeight" || BrushString == "EraseWeight")
				{
					IsPaintBrush = true;
				}
				else if(BrushString == "TexturePaint" || BrushString == "BETA_Texture")
				{
					if(!CheckHasTexture())
					{
						EditorUtility.DisplayDialog("Caution", "The mesh has no texture. Change material in the Prefab Mode and try again.", "OK");
						RestoreBrush();
						IsPaintBrush = false;
					}
					else
					{
						IsPaintBrush = true;
					}
					if(IsAnimationBrushRevert)
					{
						EditorUtility.DisplayDialog("Caution", "Change animation brush to texture paint not allowed. try again.", "OK");
						BrushString = "Move";
						BrushStringOld = "Move";
						RestoreBrush();
						IsPaintBrush = false;
					}

					//if ((!IsOldTextureBrush) && (!IsAnimationBrushRevert) && currentObject != null &&
					//     currentMesh != null && memoryMesh != null && (!IsMeshSaved) && (!IsSkipDialog))
					//{
					//    if (EditorUtility.DisplayDialog("Caution", "Mesh hasn't saved yet. Do you want to save?", "OK", "Cancel"))
					//    {
					//        EditorUtility.CopySerialized(currentMesh, memoryMesh);
					//    }
					//    else
					//    {
					//        RestoreMemoryMesh();
					//    }
					//    IsMeshSaved = true;
					//}
				}
				else if(BrushString == "BETA_Decal")
				{
					IsPaintBrush = true;
				}
				else
				{
					IsPaintBrush = false;
					LoadTexture(false);
				}
				if(BrushString == "IncreasePoly" || BrushString == "ReducePoly" || BrushString == "BETA_Repair" || BrushString == "Erase")
				{
					IsEditBrush = true;
				}
				else
				{
					IsEditBrush = false;
				}
				IsAnimationBrush = CheckAnimationBrush(BrushString);
				if(BrushString == "Draw" || BrushString == "Lower" || BrushString == "Extrude" || BrushString == "Dig")
				{
					IsOldVertList = true;
				}
				else
				{
					IsOldVertList = false;
				}
				if(BrushString == "Extrude" || BrushString == "Dig" || BrushString == "Inflat" || BrushString == "Pinch")
				{
					IsOldNormList = true;
				}
				else
				{
					IsOldNormList = false;
				}
				if(BrushString == "VertexWeight" || BrushString == "EraseWeight")
				{
					BoneWeight1s2UV4(currentMesh);
					var weight1s = currentMesh.GetAllBoneWeights().ToArray();
					var idxsets = new HashSet<int>();
					foreach(var weight1 in weight1s)
					{
						idxsets.Add(weight1.boneIndex);
					}
					BoneIdxArr = idxsets.ToArray();
				}
				if(!IsPreviewAnimation && !IsAnimationBrush && AnimationMode.InAnimationMode())
				{
					//Added 2025/09/12 to backup bindpose to fix bug that animtion not work.
					//oldbind = currentMesh.bindposes;
					ResetBindPose2();

					//End added 2025/09/12

					AnimationStop();
				}
				else if(boneAct != BoneAction.None && AnimationMode.InAnimationMode())
				{
					//Added 2025/09/12 to backup bindpose to fix bug that animtion not work.
					//oldbind = currentMesh.bindposes;
					ResetBindPose2();

					//End added 2025/09/12

					AnimationStop();
				}
				if(IsAnimationBrushRevert)
				{
					SceneView.lastActiveSceneView.Frame(currentMesh.bounds);
				}

				//if((IsOldTextureBrush) && (BrushString != "TexturePaint") && (BrushString != "BETA_Texture") && (!IsTexSaved))
				//{
				//    if (EditorUtility.DisplayDialog("Caution", "The texture hasn't saved yet. Do you want to save?", "OK", "Cancel"))
				//    {
				//    }
				//    else
				//    {
				//        RestoreMemoryTex();
				//    }
				//    IsTexSaved = true;
				//}

				if(LoadTexCnt < 2)
				{
					//AssetDatabase.Refresh();
					LoadTexture(true);
					SaveTexture();
					ChangeMaterial();
					if(IsPaintBrush)
					{
						SetupUVIndex();
					}

					//Added 2025/09/22 for fix bug that memory texture lost when you change material
					memoryTexBytes = uvtexture.GetRawTextureData();

					//End added 2025/09/22
					LoadTexCnt++;
				}
				if(flag1)
				{
					ImportMatPath = "";
					ImportMatPathList = new List<string>();

					//Chnaged 2020/01/29
					//AssetDatabase.Refresh();
					//End Changed 2020/01/29
				}
				if(oldrefImgScale == refImgScale && oldrefImgOffset == refImgOffset && oldrefImgPos == refImgPos
				   && oldrefplane == refplane && oldrefImgTrans == refImgDeform && oldrefImgSize == refImgSize)
				{
					if(ShowReferenceButtons)
					{
						ReferenceImgShow();
					}
					else
					{
						LineRendererMeshDelete("EditorSculptRef");
						ReferenceImageRevertMat();
						ShowReferenceEdit = false;
						refimgInt = 0;
					}
				}
				if(ShowReferenceButtons != OldShowReference)
				{
					IsRefereceUpdate = true;
					OldShowReference = ShowReferenceButtons;
				}

				if(ImgNumStr == "Select Reference to edit.")
				{
					refplane = ReferenceImagePlane.XY_Plane_Back;
					refImgOffset = Vector2.zero;
					refImgScale = Vector2.one;
					refImgPos = Vector3.zero;
					oldrefname = "Select Reference to edit.";
				}
				else if(oldrefname != ImgNumStr)
				{
					ReferenceImgLoad(ImgNumStr);
					ReferenceImgUpdate(ImgNumStr);
					oldrefname = ImgNumStr;

					LineRendererRenderRefresh("EditorSculptRef", ImgNumStr, false, true);
				}
				else
				{
					ReferenceImgUpdate(ImgNumStr);
				}
				if(ISReferenceTrnsp && ShowReferenceButtons)
				{
					ReferenceImgTransparentMat();
				}
				else
				{
					ReferenceImageRevertMat();
				}
			}
			if(ShowReferenceButtons != OldShowReference)
			{
				IsRefereceUpdate = true;
				OldShowReference = ShowReferenceButtons;
			}
		}

		private static void GUIAdvaceOpt()
		{
			if(GUILayout.Button("Reset All Advanced Option setting"))
			{
				if(EditorUtility.DisplayDialog("Caution", "Do you want to reset all Advanced Option settings?", "OK", "Cancel"))
				{
					IsSkipDialog = false;
					IsHideAutoRemeshMessage = false;
					IsAutoSave = true;
					IsCalcNormal = false;
					IsOptimizeTriangles = false;
					IsPreserveTriangles = true;
					ShowWireframe = false;
					AccurateBrush = true;
					GlobalBrushRad = true;
					IsLazy = true;
					IsAccurateSample = false;
					BrushSamples = 5;
					IsBoostPaint = true;
					maskWeight = 0.8f;
					maskColor = Color.black;
					boneColor = Color.green;
					bonetransp = 0.2f;
					retopoWeight = 0.5f;
					AutoSmooth = false;
					autoSmoothDeg = 0.5f;
					AutoFixSymmetry = true;
					AutoCloseHole = true;
					AutoFixBlackPoly = true;
					AutoMergeVerts = false;
					SmoothWithAutoRemesh = true;
					IsRealTimeAutoRemesh = true;
					EnableUndo = true;
					DebugMode = false;
					bshape = BrushShape.Normal;
				}
			}
			IsSkipDialog = EditorGUILayout.Toggle("Skip the mesh save Dialog(Unsafe)", IsSkipDialog);
			IsHideAutoRemeshMessage = EditorGUILayout.Toggle("Hide AutoRemesh Warning", IsHideAutoRemeshMessage);
			IsAutoSave = EditorGUILayout.Toggle("Enable Auto Save", IsAutoSave);
			IsCalcNormal = EditorGUILayout.Toggle("Calc Mesh Normals", IsCalcNormal);
			IsOptimizeTriangles = EditorGUILayout.Toggle("Optimize Triangles", IsOptimizeTriangles);
			IsPreserveTriangles = EditorGUILayout.Toggle("Preserve Triangles", IsPreserveTriangles);
			ShowWireframe = EditorGUILayout.Toggle("Show Wireframe", ShowWireframe);
			AccurateBrush = EditorGUILayout.Toggle("Accurate Brush", AccurateBrush);
			GlobalBrushRad = EditorGUILayout.Toggle("Global Brush Radius", GlobalBrushRad);
			IsLazy = EditorGUILayout.Toggle("Improve brush stroke", IsLazy);
			isSmoothStroke = EditorGUILayout.Toggle("Smooth Stroke", isSmoothStroke);
			IsAccurateSample = EditorGUILayout.Toggle("Accurate Brush Sample", IsAccurateSample);
			BrushSamples = EditorGUILayout.IntSlider("Brush Stroke Samples", BrushSamples, 1, 20);
			IsBoostPaint = EditorGUILayout.Toggle("Boost Texture Paint", IsBoostPaint);
			maskWeight = EditorGUILayout.Slider("Mask Weight", maskWeight, 0, 1);
			maskColor = EditorGUILayout.ColorField("Mask Color", maskColor);
			boneColor = EditorGUILayout.ColorField("Bone Color", boneColor);
			bonetransp = EditorGUILayout.Slider("Bone Transparent", bonetransp, 0.0f, 1.0f);
			retopoWeight = EditorGUILayout.Slider("Autoretopo Strngth", retopoWeight, 0, 1.0f);
			AutoSmooth = EditorGUILayout.Toggle("Auto Smooth Polygons", AutoSmooth);
			autoSmoothDeg = EditorGUILayout.Slider("Auto Smooth Degree", autoSmoothDeg, 0, 1);
			AutoFixSymmetry = EditorGUILayout.Toggle("Auto Fix Symmetry", AutoFixSymmetry);
			AutoCloseHole = EditorGUILayout.Toggle("Auto Close Hole", AutoCloseHole);
			AutoFixBlackPoly = EditorGUILayout.Toggle("Auto Fix BlackPoly", AutoFixBlackPoly);
			AutoMergeVerts = EditorGUILayout.Toggle("Fix Doubled Vertex", AutoMergeVerts);
			SmoothWithAutoRemesh = EditorGUILayout.Toggle("Smooth With AutoRemesh", SmoothWithAutoRemesh);
			IsRealTimeAutoRemesh = EditorGUILayout.Toggle("RealTime AutoRemesh", IsRealTimeAutoRemesh);

			//AutoRemeshInterval = EditorGUILayout.IntSlider("AutoRemesh Interval", AutoRemeshInterval, 0, 10);
			//ThreadNum = EditorGUILayout.IntSlider("Multithread Number", ThreadNum, 1, 16);
			//MaxUndo = EditorGUILayout.IntSlider("Max Undo Number", MaxUndo, 0, 20);
			EnableUndo = EditorGUILayout.Toggle("Enable Sculpt Undo", EnableUndo);
			DebugMode = EditorGUILayout.Toggle("Enable Debg Mode", DebugMode);
			bshape = (BrushShape)EditorGUILayout.EnumPopup("Brush Shape:", bshape);
			GUILayout.BeginHorizontal();
			GUILayout.Space(160);
			GUILayout.Box(BrushTex, GUILayout.Height(50), GUILayout.Width(50));
			GUILayout.EndHorizontal();
		}

		private static void ShowSaveTextureDialog()
		{
			if(currentObject != null && currentMesh != null && ((memoryMesh != null &&
			                                                     !IsMeshSaved) || (memoryTexBytes != null && !IsTexSaved)) && !IsSkipDialog)
			{
				if(EditorUtility.DisplayDialog("Caution", "The texture of the previous material " +
				                                          "hasn't saved yet. Do you want to save?", "OK", "Cancel"))
				{
				}
				else
				{
					RestoreMemoryMesh();
				}
				IsMeshSaved = true;
				IsTexSaved = true;
			}
		}

		private static void ShowLoadMesh(bool IsTexture)
		{
			GUILayout.Label(IsTexture ? "Select Mesh for Paint" : "Select Mesh for Sculpt");
			if(IsUsePopup)
			{
				ShowPopupSelect(false);
			}
			else
			{
				if(GUILayout.Button("Select Scene Mesh"))
				{
					EditorGUIUtility.ShowObjectPicker<Renderer>(loadRen, true, "", 2);
				}
			}
			IsUsePopup = EditorGUILayout.Toggle("Use Popup window", IsUsePopup);
			GUILayout.Label("");
			if(GUILayout.Button("Select Asset Mesh"))
			{
				EditorGUIUtility.ShowObjectPicker<Mesh>(startmesh, true, "", 0);
			}
			if(GUILayout.Button("Load Your mesh"))
			{
				EditorGUIUtility.ShowObjectPicker<Mesh>(startmesh, true, "l:EditorSculpt", 0);
			}
			if(Event.current.commandName == "ObjectSelectorClosed")
			{
				if(EditorGUIUtility.GetObjectPickerControlID() == 0)
				{
					pickerid = 0;
					startmesh = (Mesh)EditorGUIUtility.GetObjectPickerObject();
				}
				else if(EditorGUIUtility.GetObjectPickerControlID() == 2)
				{
					Selection.activeGameObject = (GameObject)EditorGUIUtility.GetObjectPickerObject();
				}
			}
			ShowImportPref = EditorGUILayout.Foldout(ShowImportPref, "Import Settings");
			if(ShowImportPref)
			{
				if(GUILayout.Button("Reset Import Settings"))
				{
					if(EditorUtility.DisplayDialog("Caution", "Do you want to revert all import settings to default?", "OK", "Cancel"))
					{
						IsSaveInImporterFolder = false;
						IsFixMissing = false;
						IsDontInstantiate = false;
						IsImportDependencies = true;
						IsPrefabDialog = false;
						IsOverridePrefab = false;
						IsMoveOverlap = true;
						IsUsePopup = false;
						IsUncompressTexture = true;

						//IsSkipDialog = false;
					}
				}

				//IsSkipDialog = EditorGUILayout.Toggle("Skip the mesh save Dialog(Unsafe)", IsSkipDialog);
				IsSaveInImporterFolder = EditorGUILayout.Toggle("Save in importer folder", IsSaveInImporterFolder);

				//saveFolder = (SaveFolderOptions)EditorGUILayout.EnumPopup("Save Folder Directory", saveFolder);
				IsFixMissing = EditorGUILayout.Toggle("Auto Fix Missing Asset", IsFixMissing);
				IsDontInstantiate = EditorGUILayout.Toggle("Don't Instantiate(Unsafe)", IsDontInstantiate);
				IsOverridePrefab = EditorGUILayout.Toggle("Override Prefab(Unsafe)", IsOverridePrefab);
				IsMoveOverlap = EditorGUILayout.Toggle("Move overlap model(safe)", IsMoveOverlap);
				IsImportDependencies = EditorGUILayout.Toggle("Import dependencies", IsImportDependencies);
				IsPrefabDialog = EditorGUILayout.Toggle("Show the Save Prefab Dialog", IsPrefabDialog);
				IsUsePopup = EditorGUILayout.Toggle("Use Popup window", IsUsePopup);
				IsUncompressTexture = EditorGUILayout.Toggle("Uncompress Texture(fast)", IsUncompressTexture);
				IsAnimateMaterial = EditorGUILayout.Toggle("Import Material Animation", IsAnimateMaterial);
			}
		}

		private static void ShowPopupSelect(bool IsTexture)
		{
			var gos = SceneManager.GetActiveScene().GetRootGameObjects();
			var newgolist = new List<GameObject>();
			var gonames = new List<string>();
			foreach(var go in gos)
			{
				var skinneds = go.GetComponentsInChildren<SkinnedMeshRenderer>();
				var meshfs = go.GetComponentsInChildren<MeshFilter>();
				foreach(var skinned in skinneds)
				{
					var tempgo = skinned.gameObject;
					if(tempgo.GetComponent<LineRenderer>())
					{
						var linename = tempgo.GetComponent<LineRenderer>().name;
						if(linename.StartsWith("Decal") || linename.StartsWith("Spline") || linename.StartsWith("EditorSculptRef"))
						{
							continue;
						}
					}
					gonames.Add(ObjectNames.GetUniqueName(gonames.ToArray(), tempgo.name));
					newgolist.Add(tempgo);
				}
				foreach(var meshf in meshfs)
				{
					var tempgo = meshf.gameObject;
					if(tempgo.GetComponent<LineRenderer>())
					{
						var linename = tempgo.GetComponent<LineRenderer>().name;
						if(linename.StartsWith("Decal") || linename.StartsWith("Spline") || linename.StartsWith("EditorSculptRef"))
						{
							continue;
						}
					}
					gonames.Add(ObjectNames.GetUniqueName(gonames.ToArray(), meshf.gameObject.name));
					newgolist.Add(meshf.gameObject);
				}
			}
			startidx = EditorGUILayout.Popup("", startidx, gonames.ToArray());
			if(GUILayout.Button(IsTexture ? "Texture Paint" : "Sculpt"))
			{
				if(startidx >= 0 && newgolist.Count > 0)
				{
					pickerid = 0;
					startmesh = GetMeshFromGameObject(newgolist[startidx]);
					if(IsTexture)
					{
						IsPaintBrush = true;
					}
				}
			}
		}

		private void Update()
		{
			if(pickerid == 0)
			{
				pickerid = -1;
				if(startmesh == null)
				{
					return;
				}
				var starttime = (float)EditorApplication.timeSinceStartup;
				IsLoadMesh = true;
				currentObject = LoadGameObjectFromMesh(startmesh, true);
				if(currentObject != null && currentObject.activeInHierarchy == false && UnloadMesh == null)
				{
					var newgo = InstantiateGameObject(currentObject);

					//Added 2023/05/08
					//MoveInstanceGameobj(currentObject, newgo);
					MoveOverlapGameObject(newgo);

					//End added 2023/05/08
					Selection.activeGameObject = newgo;
					currentObject = newgo;
				}
				else
				{
					Selection.activeGameObject = currentObject;
				}
				GetCurrentMesh(true);

				//Added 2025/09/13
				//Fix animation error
				if(currentMesh != null)
				{
					//oldbind = currentMesh.bindposes;
					ResetBindPose2();
				}

				//End added 2025/09/13

				startmesh = null;
				IsLoadMesh = false;
				if(DebugMode)
				{
					Debug.Log("Check Mesh complete:" + ((float)EditorApplication.timeSinceStartup - starttime) + "sec");
				}
			}
		}

		private static void GUIChangeWindow(bool flag0)
		{
			//Vector2 minvec = new Vector2(280, 320);
			var minvec = new Vector2(280, 340);
			if(flag0)
			{
				if(ShowAdvancedOption)
				{
					minvec.y += 250;
				}
				if(ShowShortcut)
				{
					minvec.y += 120;
				}
				if(ShowEditButtons)
				{
					minvec.y += 180;
				}
				if(ShowSaveButtons)
				{
					minvec.y += 50;
				}
			}
			else if(ShowAdvancedOption)
			{
				minvec.y += 250;
			}
			window.minSize = minvec;
			window.maxSize = new Vector2(window.maxSize.x, minvec.y + 20);
		}

		private void OnSelectBoneCallback(object obj)
		{
			delboneidx = (int)obj;
			parentidx = (int)obj;

			//New in 2019/12/18
			boneAct = BoneAction.Add;

			//End New in 2019/12/18

			if(CheckAnimationBrush(BrushString))
			{
				AnimeSliderReset();

				//if (IsBoneMove) AnimationSave(animeslider, 0.0f, false);
				AnimationSaveBoneMove(animeslider, 0.0f, false);
				AnimationStop();
			}
		}

		private static void SavePreference()
		{
			var epref = CreateInstance<EditorSculptPref>();

			//epref.IsImportDependencies = IsImportDependencies;
			//epref.IsLoadAssetMesh = IsLoadAssetMesh;
			var boolList = new List<bool>();
			boolList.Add(IsImportDependencies);
			boolList.Add(IsDontInstantiate);
			boolList.Add(IsFixMissing);
			boolList.Add(IsHideAutoRemeshMessage);
			boolList.Add(IsPrefabDialog);
			boolList.Add(IsOverridePrefab);
			boolList.Add(IsUsePopup);
			boolList.Add(IsUncompressTexture);
			boolList.Add(IsMoveOverlap);
			boolList.Add(IsSkipDialog);
			boolList.Add(IsAnimateMaterial);
			boolList.Add(IsUseKeyFrame);
			boolList.Add(IsSaveInImporterFolder);
			epref.boolList = boolList;
			AssetDatabase.CreateAsset(epref, "Assets/EditorSculpt.asset");
			AssetDatabase.SaveAssets();
		}

		private static void LoadPreference()
		{
			EditorSculptPref epref = null;
			try
			{
				AssetDatabase.StartAssetEditing();
				epref = (EditorSculptPref)AssetDatabase.LoadAssetAtPath("Assets/EditorSculpt.asset", typeof(EditorSculptPref));
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
			if(epref == null)
			{
				return;
			}

			//IsLoadPreference = true;
			var boolList = epref.boolList;
			if(boolList.Count > 0)
			{
				IsImportDependencies = boolList[0];
			}
			if(boolList.Count > 1)
			{
				IsDontInstantiate = boolList[1];
			}
			if(boolList.Count > 2)
			{
				IsFixMissing = boolList[2];
			}
			if(boolList.Count > 3)
			{
				IsHideAutoRemeshMessage = boolList[3];
			}
			if(boolList.Count > 4)
			{
				IsPrefabDialog = boolList[4];
			}
			if(boolList.Count > 5)
			{
				IsOverridePrefab = boolList[5];
			}
			if(boolList.Count > 6)
			{
				IsUsePopup = boolList[6];
			}
			if(boolList.Count > 7)
			{
				IsUncompressTexture = boolList[7];
			}
			if(boolList.Count > 8)
			{
				IsMoveOverlap = boolList[8];
			}
			if(boolList.Count > 9)
			{
				IsSkipDialog = boolList[9];
			}
			if(boolList.Count > 10)
			{
				IsAnimateMaterial = boolList[10];
			}
			if(boolList.Count > 11)
			{
				IsUseKeyFrame = boolList[11];
			}
			if(boolList.Count > 12)
			{
				IsSaveInImporterFolder = boolList[12];
			}
		}

		private static GameObject GetGameObjFromMesh(Mesh mesh, bool IsLabel)
		{
			if(IsLabel)
			{
				var gos = SceneManager.GetActiveScene().GetRootGameObjects();
				foreach(var go in gos)
				{
					if(!go.name.Contains("EditorSculpt"))
					{
						continue;
					}
					var skinneds = go.GetComponentsInChildren<SkinnedMeshRenderer>();
					foreach(var skinned in skinneds)
					{
						if(skinned.sharedMesh == mesh)
						{
							return skinned.gameObject;
						}
					}
					var meshfs = go.GetComponentsInChildren<MeshFilter>();
					foreach(var meshf in meshfs)
					{
						if(meshf.sharedMesh == mesh)
						{
							return meshf.gameObject;
						}
					}
				}
			}

			var meshpath = AssetDatabase.GetAssetPath(mesh);

			//Added 2025/09/05 for Copy Model Importer and disable Import Animation
			//This fixes many bugs.
			if(!meshpath.Contains("EditorSculpt"))
			{
				try
				{
					var importer = (ModelImporter)AssetImporter.GetAtPath(meshpath);

					//if (importer.importAnimation)
					//{
					var newMeshPath = Path.GetDirectoryName(meshpath) + "/" + Path.GetFileNameWithoutExtension(meshpath)
					                  + "_EditorSculpt" + Path.GetExtension(meshpath);

					//newMeshPath = AssetDatabase.GenerateUniqueAssetPath(newMeshPath);
					//AssetDatabase.CopyAsset(meshpath, newMeshPath);

					//ModelImporter assetImp = (ModelImporter)AssetImporter.GetAtPath(newMeshPath);
					//EditorUtility.CopySerialized(importer, assetImp);

					var assetImp = new ModelImporter();
					var uniquePath = AssetDatabase.GenerateUniqueAssetPath(newMeshPath);
					if(newMeshPath == uniquePath)
					{
						AssetDatabase.CopyAsset(meshpath, newMeshPath);
						assetImp = (ModelImporter)AssetImporter.GetAtPath(newMeshPath);
					}
					else
					{
						assetImp = (ModelImporter)AssetImporter.GetAtPath(newMeshPath);
						if(assetImp == null)
						{
							newMeshPath = uniquePath;
							AssetDatabase.CopyAsset(meshpath, uniquePath);
							assetImp = (ModelImporter)AssetImporter.GetAtPath(uniquePath);
						}
						else if(assetImp.userData != meshpath)
						{
							newMeshPath = uniquePath;
							AssetDatabase.CopyAsset(meshpath, uniquePath);
							assetImp = (ModelImporter)AssetImporter.GetAtPath(uniquePath);
						}
					}

					EditorUtility.CopySerialized(importer, assetImp);
					assetImp.isReadable = true;
					assetImp.importAnimation = false;
					assetImp.userData = meshpath;
					assetImp.SaveAndReimport();

					//mesh = (Mesh)AssetDatabase.LoadAssetAtPath(newMeshPath, typeof(Mesh));

					meshpath = newMeshPath;

					//}
				}
				catch
				{
				}
			}

			//}
			//End Added 2025/09/05 for Copy Model Importer and disable Import Animation

			var objarr = AssetDatabase.LoadAllAssetsAtPath(meshpath);
			foreach(var obj in objarr)
			{
				try
				{
					if(obj.GetType() == typeof(GameObject))
					{
						var go = (GameObject)obj;
						Mesh tempmesh = null;
						try
						{
							tempmesh = go.GetComponent<SkinnedMeshRenderer>().sharedMesh;
						}
						catch
						{
						}
						if(tempmesh == null)
						{
							try
							{
								tempmesh = go.GetComponent<MeshFilter>().sharedMesh;
							}
							catch
							{
							}
						}
						if(tempmesh != null)
						{
							//if (tempmesh == mesh)
							if(CheckSameMeshes(tempmesh, mesh))
							{
								if(!IsLabel)
								{
									//if (go.transform.root.gameObject.name.Contains("EditorSculpt")) continue;

									if(!go.activeInHierarchy && go.transform.root.gameObject.name.Contains("EditorSculpt"))
									{
										continue;
									}
								}
								return go;
							}
						}
					}
				}
				catch
				{
				}
			}
			var assetgos = AssetDatabase.FindAssets("t:GameObject");
			var pathhash = new HashSet<string>();
			for(var i = 0; i < assetgos.Length; i++)
			{
				pathhash.Add(AssetDatabase.GUIDToAssetPath(assetgos[i]));
			}
			foreach(var str in pathhash)
			{
				objarr = AssetDatabase.LoadAllAssetsAtPath(str);
				foreach(var obj in objarr)
				{
					try
					{
						if(obj.GetType() == typeof(GameObject))
						{
							var go = (GameObject)obj;
							Mesh tempmesh = null;
							try
							{
								tempmesh = go.GetComponent<SkinnedMeshRenderer>().sharedMesh;
							}
							catch
							{
							}
							if(tempmesh == null)
							{
								try
								{
									tempmesh = go.GetComponent<MeshFilter>().sharedMesh;
								}
								catch
								{
								}
							}
							if(tempmesh != null)
							{
								//if (tempmesh == mesh) return go;
								if(CheckSameMeshes(tempmesh, mesh))
								{
									return go;
								}
							}
						}
					}
					catch
					{
					}
				}
			}
			return null;
		}

		private static GameObject GeneratreGameObjectFromMesh(Mesh mesh)
		{
			var newgo = new GameObject();
			newgo.name = mesh.name == null ? "EditorSculptObject" : mesh.name;
			var meshf = newgo.AddComponent<MeshFilter>();
			var meshren = newgo.AddComponent<MeshRenderer>();
			meshf.sharedMesh = mesh;
			var tempgo = GetGameObjFromMesh(mesh, false);
			var tempren = tempgo.GetComponent<Renderer>();
			var matarr = tempren.sharedMaterials;
			var matlist = new List<Material>();
			for(var i = 0; i < matarr.Length; i++)
			{
				var mat = new Material(matarr[i]);
				matlist.Add(mat);
			}
			meshren.sharedMaterials = matlist.ToArray();
			newgo.transform.position = tempgo.transform.position;
			newgo.transform.rotation = tempgo.transform.rotation;
			newgo.name += "_EditorSculpt";
			return newgo;
		}

		private static GameObject LoadGameObjectFromMesh(Mesh mesh, bool islabel)
		{
			if(AssetDatabase.GetAssetPath(mesh).StartsWith("Library/unity default resources"))
			{
				GameObject primobj = null;
				if(mesh.name == "Capsule")
				{
					primobj = ObjectFactory.CreatePrimitive(PrimitiveType.Capsule);
				}
				else if(mesh.name == "Cube")
				{
					primobj = ObjectFactory.CreatePrimitive(PrimitiveType.Cube);
				}
				else if(mesh.name == "Cylinder")
				{
					primobj = ObjectFactory.CreatePrimitive(PrimitiveType.Cylinder);
				}
				else if(mesh.name == "Plane")
				{
					primobj = ObjectFactory.CreatePrimitive(PrimitiveType.Plane);
				}
				else if(mesh.name == "Quad")
				{
					primobj = ObjectFactory.CreatePrimitive(PrimitiveType.Quad);
				}
				else if(mesh.name == "Sphere")
				{
					primobj = ObjectFactory.CreatePrimitive(PrimitiveType.Sphere);
				}
				if(primobj != null)
				{
					primobj.name += "_EditorSculpt";
					return primobj;
				}
			}
			return IsImportDependencies ? GetGameObjFromMesh(mesh, islabel) : GeneratreGameObjectFromMesh(mesh);
		}

		private static bool CheckEditorSculptObj(Object chkobj)
		{
			var strings = AssetDatabase.GetLabels(chkobj);
			foreach(var tempstr in strings)
			{
				if(tempstr == "EditorSculpt" || tempstr == "EditorSculptImported")
				{
					return true;
				}
			}

			var tempgo = GetGameObjFromMesh((Mesh)chkobj, false);

			if(tempgo == null)
			{
				return false;
			}
			var meshfs = tempgo.transform.root.gameObject.GetComponentsInChildren<MeshFilter>();
			var skinneds = tempgo.transform.root.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			if(meshfs.Length > 0)
			{
				foreach(var meshf in meshfs)
				{
					var labels = AssetDatabase.GetLabels(meshf.sharedMesh);
					foreach(var label in labels)
					{
						if(label == "EditorSculpt" || label == "EditorSculptImported")
						{
							return true;
						}
					}
				}
			}
			if(skinneds.Length > 0)
			{
				foreach(var skinned in skinneds)
				{
					var labels = AssetDatabase.GetLabels(skinned.sharedMesh);
					foreach(var label in labels)
					{
						if(label == "EditorSculpt" || label == "EditorSculptImported")
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private static bool CheckEditorSculptGameObj(GameObject go)
		{
			if(go == null)
			{
				return false;
			}
			var rootgo = go.transform.root.gameObject;
			var meshfs = rootgo.transform.root.gameObject.GetComponentsInChildren<MeshFilter>();
			var skinneds = rootgo.transform.root.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			if(meshfs.Length > 0)
			{
				foreach(var meshf in meshfs)
				{
					var labels = AssetDatabase.GetLabels(meshf.sharedMesh);
					foreach(var label in labels)
					{
						if(label == "EditorSculpt" || label == "EditorSculptImported")
						{
							return true;
						}
					}
				}
			}
			if(skinneds.Length > 0)
			{
				foreach(var skinned in skinneds)
				{
					var labels = AssetDatabase.GetLabels(skinned.sharedMesh);
					foreach(var label in labels)
					{
						if(label == "EditorSculpt" || label == "EditorSculptImported")
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private static bool CheckSameMeshes(Mesh mesh1, Mesh mesh2)
		{
			if(mesh1.name != mesh2.name)
			{
				return false;
			}

			if(mesh1.vertexCount != mesh2.vertexCount)
			{
				return false;
			}

			var vertices1 = mesh1.vertices;
			var vertices2 = mesh2.vertices;
			var vcnt = mesh1.vertexCount;
			for(var i = 0; i < vcnt; i++)
			{
				if(vertices1[i] != vertices2[i])
				{
					return false;
				}
			}

			return true;
		}

		private static string GetEditorSculptGOPath(GameObject go)
		{
			var rootgo = go.transform.root.gameObject;
			var meshfs = rootgo.transform.root.gameObject.GetComponentsInChildren<MeshFilter>();
			var skinneds = rootgo.transform.root.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			if(meshfs.Length > 0)
			{
				foreach(var meshf in meshfs)
				{
					var labels = AssetDatabase.GetLabels(meshf.sharedMesh);
					foreach(var label in labels)
					{
						if(label == "EditorSculpt" || label == "EditorSculptImported")
						{
							return AssetDatabase.GetAssetPath(rootgo);
						}
					}
				}
			}
			if(skinneds.Length > 0)
			{
				foreach(var skinned in skinneds)
				{
					var labels = AssetDatabase.GetLabels(skinned.sharedMesh);
					foreach(var label in labels)
					{
						if(label == "EditorSculpt" || label == "EditorSculptImported")
						{
							return AssetDatabase.GetAssetPath(rootgo);
						}
					}
				}
			}
			return "";
		}

		private static GameObject InstantiateGameObject(GameObject go)
		{
			if(go.transform.root.gameObject.name.StartsWith("PreviewMaterials") || go.transform.root.gameObject.name.StartsWith("HandlesGO"))
			{
				var insobj = Instantiate(go);
				insobj.name += "_EditorSculpt";
				return insobj;
			}
			GameObject rootobj = null;
			if(go.transform.root.gameObject.activeInHierarchy)
			{
				rootobj = go.transform.root.gameObject;
			}
			else
			{
				var oldstr = go.transform.root.gameObject.name;
				rootobj = PrefabUtility.IsPartOfPrefabAsset(go)
					? (GameObject)PrefabUtility.InstantiatePrefab(go.transform.root.gameObject)
					: Instantiate(go.transform.root.gameObject);
				rootobj.name = oldstr;
				if(!rootobj.name.Contains("EditorSculpt"))
				{
					rootobj.name += "_EditorSculpt";
				}
			}

			var gos = SceneManager.GetActiveScene().GetRootGameObjects();
			foreach(var tempgo in gos)
			{
				if(tempgo == rootobj)
				{
					var renderers = tempgo.GetComponentsInChildren<Renderer>();
					foreach(var rend in renderers)
					{
						if(rend.gameObject.name.StartsWith(go.name))
						{
							return rend.gameObject;
						}
					}
				}
			}
			return null;
		}

		private static GameObject[] GetChildObjects(GameObject go)
		{
			var golist = new List<GameObject>();
			var childset = new HashSet<GameObject>();
			var chdcnt = go.transform.childCount;
			for(var i = 0; i < chdcnt; i++)
			{
				golist.Add(go.transform.GetChild(i).gameObject);
				childset.Add(go.transform.GetChild(i).gameObject);
			}
			while(childset.Count > 0)
			{
				var childlist = childset.ToList();
				for(var i = 0; i < childlist.Count; i++)
				{
					var cgo = childlist[i];
					var cchdcnt = cgo.transform.childCount;
					for(var j = 0; j < cchdcnt; j++)
					{
						golist.Add(cgo.transform.GetChild(j).gameObject);
						childset.Add(cgo.transform.GetChild(j).gameObject);
					}
					childset.Remove(cgo);
				}
			}
			return golist.ToArray();
		}

		private static void PostLoadMesh()
		{
			var starttime = (float)EditorApplication.timeSinceStartup;
			currentObject = Selection.activeGameObject;
			GetCurrentMesh(true);

			if(currentObject != null && currentObject.GetComponent<LineRenderer>())
			{
				var linren = currentObject.GetComponent<LineRenderer>();
				if(linren.name.StartsWith("Decal") || linren.name.StartsWith("Spline") || linren.name.StartsWith("EditorSculptRef"))
				{
					var parento = linren.gameObject.transform.parent.gameObject;
					var meshf = parento.GetComponent<MeshFilter>();
					var IsLinren = false;
					if(meshf != null && meshf.sharedMesh != null)
					{
						var labels = AssetDatabase.GetLabels(meshf.sharedMesh);
						foreach(var str in labels)
						{
							if(str.StartsWith("EditorSculpt"))
							{
								IsLinren = true;
								break;
							}
						}
					}
					if(IsLinren || meshf == null || meshf.sharedMesh == null)
					{
						currentObject = null;
						currentMesh = null;
						return;
					}
				}
			}
			if(!currentObject || !currentMesh)
			{
				return;
			}
			var trilen = currentMesh.GetTriangles(0).Length;
			if(trilen == 0)
			{
				return;
			}

			IsLoadMesh = true;

			//if(!CheckEditorSculptObj4(currentMesh)) EditorApplication.ExecuteMenuItem("Edit/Duplicate");

			importMetaPath = "";
			metaContent = "";
			UseModelImporter = false;

			//FixAndCopyImportedMesh();
			FixImportedMesh();
			MeshScale(currentMesh);

			////Added 2025/09/12 to backup bindpose to fix bug that animtion not work.
			//oldbind = currentMesh.bindposes;
			////End Added 2025/09/12
			ResetBindPose2();
			AnimationStop();
			if(IsAnimationBrush)
			{
				AnimationModeStart();
			}

			SaveFolderPath = GetSaveFolderPath();

			FixMissingImport();
			if(DebugMode)
			{
				Debug.Log("Load Mesh step1:" + ((float)EditorApplication.timeSinceStartup - starttime) + "sec");
			}

			//if (currentMeshFilter != null && trilen > 0 && currentMesh.vertexCount > 0)
			if(currentMesh != null && trilen > 0 && currentMesh.vertexCount > 0)
			{
				avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));
			}
			if(EnableUndo)
			{
				Undo.RegisterCompleteObjectUndo(currentMesh, "Setup Mesh" + Undo.GetCurrentGroup());
			}
			LoadTexture(false);
			if(esmode == EditorSculptMode.Sculpt || esmode == EditorSculptMode.Beta)
			{
				if(AutoMergeVerts)
				{
					if(trilen > 0 && (currentMesh.vertexCount - 2) * 5 > trilen)
					{
						if(EditorUtility.DisplayDialog("Mesh has too many doubed vertices", "do you want to fix that?", "fix", "don't fix"))
						{
							MergeVertsFast(currentMesh);
						}
					}
				}
				if(currentObject != null && currentMesh != null)
				{
					SubMeshGenerate(currentMesh);
					CheckMonoSubMesh(currentMesh);
				}
			}
			else
			{
				SubMeshGenerate(currentMesh);
				CheckMonoSubMesh(currentMesh);
				IsBrushedArray = Enumerable.Repeat(true, currentMesh.vertexCount).ToArray();
				MergeVertsFast(currentMesh);
				CloseHole(currentMesh);
				MergeVertsFast(currentMesh);

				//mergedtris = currentMesh.GetTriangles(0);
				MergetriGenerate(currentMesh);
				CalcMeshNormals(currentMesh);
			}
			MergetriGenerate(currentMesh);

			if(currentMesh.uv4 != null)
			{
				var vcount = currentMesh.vertexCount;
				var mcnt = 0;
				var uv4s = currentMesh.uv4;
				if(uv4s.Length != vcount)
				{
					uv4s = Enumerable.Repeat(Vector2.one, vcount).ToArray();
					currentMesh.uv4 = uv4s;
				}
				var startt = (float)EditorApplication.timeSinceStartup;
				for(var i = 0; i < vcount; i++)
				{
					if(uv4s[i].x == 0.0f)
					{
						mcnt++;
					}
				}
				if(DebugMode)
				{
					Debug.Log("Check mesh UVs:" + ((float)EditorApplication.timeSinceStartup - startt) + "sec");
				}

				//if(mcnt>vcount*0.999F)
				if(mcnt == vcount)
				{
					if(EditorUtility.DisplayDialog("Warning!", "Mask seems get error.Do you want to Clear that?", "Clear", "No"))
					{
						uv4s = Enumerable.Repeat(Vector2.one, vcount).ToArray();
						currentMesh.uv4 = uv4s;
						if(DebugMode)
						{
							Debug.Log("Editor Sculpt clears mesh masks.");
						}
					}
				}
			}
			if(currentMesh.vertexCount != currentMesh.colors.Length)
			{
				currentMesh.colors = Enumerable.Repeat(Color.white, currentMesh.vertexCount).ToArray();
			}
			if(currentMesh.uv.Length < 1)
			{
				currentMesh.uv = Enumerable.Repeat(Vector2.one, currentMesh.vertexCount).ToArray();
			}
			if(currentMesh.uv2.Length < 1)
			{
				currentMesh.uv2 = Enumerable.Repeat(Vector2.one, currentMesh.vertexCount).ToArray();
			}
			if(currentMesh.uv3.Length < 1)
			{
				currentMesh.uv3 = Enumerable.Repeat(Vector2.one, currentMesh.vertexCount).ToArray();
			}
			if(currentMesh.uv4.Length < 1)
			{
				currentMesh.uv4 = Enumerable.Repeat(Vector2.one, currentMesh.vertexCount).ToArray();
			}

			//Disabled 2023/1/26
			//ChangeMaterial2();
			//End Disabled 2023/1/26

			if(currentMesh != null && currentMesh.GetTriangles(0).Length > 0 && currentMesh.vertexCount > 0)
			{
				avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));
			}
			IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();

			if(bshape == bstex && bshape != BrushShape.Normal)
			{
				bstex = BrushShape.Normal;
			}
			if(bshape == bstex && bshape == BrushShape.Normal)
			{
				bstex = BrushShape.SoftSolid;
			}

			paintmatidx = 0;
			ImportMatPath = "";
			ImportMatPathList = new List<string>();

			Spline2DListList = new List<List<Vector3>>();
			Spline3DListList = new List<List<Vector3>>();
			SplineDirListList = new List<List<Vector3>>();

			Decal2DListList = new List<List<Vector3>>();
			Decal3DListList = new List<List<Vector3>>();
			DecalTriListList = new List<List<int>>();
			DecalUVListList = new List<List<Vector2>>();
			DecalDirListList = new List<List<Vector3>>();
			DecalBaseListList = new List<List<Vector3>>();
			DecalLoad();
			decalidx = 0;
			BoneMinIdx = -1;
			aniclip = null;
			IsPreviewAnimation = false;

			//refimgLoad = true;
			OldShowReference = false;
			oldrefname = "";
			oldMaterilList = new List<Material>();
			transparentMat = null;
			ShowReferenceEdit = false;
			if(DebugMode)
			{
				Debug.Log("Load Mesh step2:" + ((float)EditorApplication.timeSinceStartup - starttime) + "sec");
			}

			if(currentMesh != null)
			{
				if(currentMesh.uv4.Length < 1)
				{
					currentMesh.uv4 = Enumerable.Repeat(Vector2.one, currentMesh.vertexCount).ToArray();
				}
				IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
				ChangeMaterial();

				Tools.hidden = true;
				IsEnable = true;

				Spline3DLoad();
			}
			if(IsAutoSave)
			{
				EditorSculptPrefab(true, !IsPrefabDialog);
			}
			RevertMetaFile(importMetaPath, metaContent);
			SaveTexture();

			//SaveTexture();
			//IsOldImporter = false;
			ChangeMaterial();
			LoadTexture(true);
			window.Repaint();

			//Save Model Importer Path to the AssetImporter(Mesh) userdata.
			var meshPath = AssetDatabase.GetAssetPath(currentMesh);
			var aimp = AssetImporter.GetAtPath(meshPath);
			try
			{
				if(ModelImporterPath.Length > 1)
				{
					aimp.userData = ModelImporterPath;
				}
			}
			catch
			{
			}

			//if(IsOldAnimationImport==true)ModelImporterChangeAnimeImport(false);

			////Added 2023/05/08
			//Bounds movebounds = GetBoundsInScene();
			//currentObject.transform.root.position = movebounds.max;
			////End added 2023/05/08

			memoryMesh = new Mesh();
			EditorUtility.CopySerialized(currentMesh, memoryMesh);
			memoryTexBytes = uvtexture.GetRawTextureData();
			IsMeshSaved = true;
			IsTexSaved = true;

			var sibname = GameObjectUtility.GetUniqueNameForSibling(null, currentObject.transform.root.name);
			if(sibname != currentObject.transform.root.name)
			{
				currentObject.transform.root.name = sibname;
			}
			MoveOverlapGameObject(currentObject);

			//MoveInstanceGameobj(GameObject.Find(currentObject.transform.root.name), currentObject);

			IsAnimationBrush = CheckAnimationBrush(BrushString);
			if(IsAnimationBrush)
			{
				AnimatorSetup(false);
			}

			if(ShowReferenceButtons)
			{
				ReferenceImgShow();
			}
			IsLoadMesh = false;
			if(DebugMode)
			{
				Debug.Log("Load Mesh complete:" + ((float)EditorApplication.timeSinceStartup - starttime) + "sec");
			}

			//#if UNITY_2019
			if(Application.unityVersion.StartsWith("2019"))
			{
				//oldGIWork = Lightmapping.giWorkflowMode;
				oldGIWork = (int)Lightmapping.giWorkflowMode;
				Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.Iterative;
				IsNeedChangeLightmap = true;
			}

			//#endif
		}

		private static bool BuildSkinnedMeshRenderer()
		{
			if(currentObject.GetComponent<SkinnedMeshRenderer>())
			{
				return true;
			}
			if(EditorUtility.DisplayDialog("No Skinned MeshRenderer"
				   , "Mesh has no Skinned Meshrenderer,Do you want to build taht?", "No", "OK"))
			{
				RestoreBrush();
				return false;
			}
			currentMesh.RecalculateBounds();
			var skinned = new SkinnedMeshRenderer();
			skinned = Undo.AddComponent<SkinnedMeshRenderer>(currentObject);
			skinned.sharedMesh = currentMesh;
			skinned.localBounds = currentMesh.bounds;
			EditorSculptPrefab(false, true);
			return true;
		}

		private static void RestoreBrush()
		{
			if(BrushString == BrushStringOld)
			{
				switch(esmode)
				{
					case EditorSculptMode.RemeshSculpt:
						btyper = BrushModeRemesh.Move;
						break;
					case EditorSculptMode.Sculpt:
						btype = BrushMode.Move;
						break;
					case EditorSculptMode.Beta:
						btypeb = BrushModeBeta.Move;
						break;
					case EditorSculptMode.RemeshBeta:
						btyperb = BrushModeRemeshBeta.Move;
						break;
				}
				BrushString = "Move";
				BrushStringOld = "Move";
				if(IsAnimationBrush)
				{
					IsAnimationBrush = false;
					AnimationStop();
				}

				//Debug.Log("Brush==Oldbrush  Bush:" + BrushString);
			}
			else
			{
				switch(esmode)
				{
					case EditorSculptMode.RemeshSculpt:
						btyper = (BrushModeRemesh)Enum.Parse(typeof(BrushModeRemesh), BrushStringOld);
						break;
					case EditorSculptMode.Sculpt:
						btype = (BrushMode)Enum.Parse(typeof(BrushMode), BrushStringOld);
						break;
					case EditorSculptMode.Beta:
						btypeb = (BrushModeBeta)Enum.Parse(typeof(BrushModeBeta), BrushStringOld);
						break;
					case EditorSculptMode.RemeshBeta:
						btyperb = (BrushModeRemeshBeta)Enum.Parse(typeof(BrushModeRemeshBeta), BrushStringOld);
						break;
				}
				if(IsAnimationBrush)
				{
					if(CheckAnimationBrush(BrushStringOld) == false)
					{
						AnimationStop();

						//Added 2025/09/12 to backup bindpose to fix bug that animtion not work.
						//oldbind = currentMesh.bindposes;
						ResetBindPose2();

						//End Added 2025/09/12
					}
				}

				//New in 2020/10/28
				else
				{
					if(CheckAnimationBrush(BrushString))
					{
						//Added 2025/09/12 to backup bindpose to fix bug that animtion not work.
						//currentMesh.bindposes = oldbind;
						//End Added 2025/09/12

						AnimationModeStart();
					}
				}

				//End New in 2020/10/28
				//Debug.Log("Brush:" + BrushString + "  OldBrush:" + BrushStringOld);
				BrushString = BrushStringOld;
				IsAnimationBrush = CheckAnimationBrush(BrushString);
			}
		}

		private static void BlendShapeCreate()
		{
			//Add New in 2020/03/09
			UnloadMesh = currentMesh;

			//End New in 2020/03/09
			var currname = currentMesh.name;
			var blendnames = new List<string>();
			for(var i = 0; i < currentMesh.blendShapeCount; i++)
			{
				blendnames.Add(currentMesh.GetBlendShapeName(i));
			}
			var tempmesh = new Mesh();
			EditorUtility.CopySerialized(currentMesh, tempmesh);
			var weight1s = new NativeArray<BoneWeight1>(currentMesh.GetAllBoneWeights().ToArray(), Allocator.Temp);
			var PerVerts = new NativeArray<byte>(currentMesh.GetBonesPerVertex().ToArray(), Allocator.Temp);
			tempmesh.SetBoneWeights(PerVerts, weight1s);
			tempmesh.bindposes = currentMesh.bindposes;
			EditorUtility.CopySerialized(blendBaseMesh, currentMesh);
			weight1s = new NativeArray<BoneWeight1>(blendBaseMesh.GetAllBoneWeights().ToArray(), Allocator.Temp);
			PerVerts = new NativeArray<byte>(blendBaseMesh.GetBonesPerVertex().ToArray(), Allocator.Temp);
			currentMesh.SetBoneWeights(PerVerts, weight1s);
			currentMesh.bindposes = blendBaseMesh.bindposes;

			var vcount = tempmesh.vertexCount;
			var deltavec = Enumerable.Repeat(Vector3.zero, vcount).ToArray();
			var deltanorm = Enumerable.Repeat(Vector3.zero, vcount).ToArray();
			var deltatan = Enumerable.Repeat(Vector3.zero, vcount).ToArray();
			var vertices0 = blendBaseMesh.vertices;
			var vertices1 = tempmesh.vertices;
			var normals0 = blendBaseMesh.normals;
			var normals1 = tempmesh.normals;
			var tangents0 = blendBaseMesh.tangents;
			var tangents1 = tempmesh.tangents;
			for(var i = 0; i < vcount; i++)
			{
				deltavec[i] = vertices1[i] - vertices0[i];
				deltanorm[i] = normals1[i] - normals0[i];
				deltatan[i] = tangents1[i] - tangents0[i];
			}
			var cnt0 = 0;
			for(var i = 0; i < 256; i++)
			{
				var flag1 = false;

				//if (blendnames.Contains(blendBaseMesh.name + i.ToString())) flag1 = true;
				if(blendnames.Contains(blendShapeName + i))
				{
					flag1 = true;
				}
				if(!flag1)
				{
					cnt0 = i;
					break;
				}
			}

			//currentMesh.AddBlendShapeFrame(blendBaseMesh.name + cnt0.ToString(), 100.0f, deltavec, deltanorm, deltatan);
			currentMesh.AddBlendShapeFrame(blendShapeName + cnt0, 100.0f, deltavec, deltanorm, deltatan);
			DestroyImmediate(blendBaseMesh);

			//currentMesh.RecalculateNormals();
			CalcMeshNormals(currentMesh);
			currentMesh.RecalculateBounds();
			currentMesh.RecalculateTangents();
			currentMesh.name = currname;
			blendShapeName = "BlendShape";

			//String temppath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".unitypackage");
			//String assetpath = AssetDatabase.GetAssetPath(currentMesh);
			//AssetDatabase.ExportPackage(assetpath, temppath);
			//AssetDatabase.DeleteAsset(assetpath);
			//AssetDatabase.ImportPackage(temppath, false);
			//AssetDatabase.Refresh();
			//currentMesh = (Mesh)AssetDatabase.LoadAssetAtPath(assetpath, typeof(Mesh));
			//AssetDatabase.DeleteAsset(temppath);
			//AssetDatabase.Refresh();
		}

		private static void BlendShapeCreateAnime()
		{
			//Add New in 2020/03/09
			UnloadMesh = currentMesh;

			//End New in 2020/03/09
			var blendnames = new List<string>();
			for(var i = 0; i < currentMesh.blendShapeCount; i++)
			{
				blendnames.Add(currentMesh.GetBlendShapeName(i));
			}
			var tempmesh = new Mesh();
			EditorUtility.CopySerialized(currentMesh, tempmesh);
			var weight1s = new NativeArray<BoneWeight1>(currentMesh.GetAllBoneWeights().ToArray(), Allocator.Temp);
			var PerVerts = new NativeArray<byte>(currentMesh.GetBonesPerVertex().ToArray(), Allocator.Temp);
			tempmesh.SetBoneWeights(PerVerts, weight1s);
			tempmesh.bindposes = currentMesh.bindposes;
			EditorUtility.CopySerialized(blendBaseMesh, currentMesh);
			weight1s = new NativeArray<BoneWeight1>(blendBaseMesh.GetAllBoneWeights().ToArray(), Allocator.Temp);
			PerVerts = new NativeArray<byte>(blendBaseMesh.GetBonesPerVertex().ToArray(), Allocator.Temp);
			currentMesh.SetBoneWeights(PerVerts, weight1s);
			currentMesh.bindposes = blendBaseMesh.bindposes;

			var vcount = tempmesh.vertexCount;
			var deltavec = Enumerable.Repeat(Vector3.zero, vcount).ToArray();
			var deltanorm = Enumerable.Repeat(Vector3.zero, vcount).ToArray();
			var deltatan = Enumerable.Repeat(Vector3.zero, vcount).ToArray();
			var vertices0 = blendBaseMesh.vertices;
			var vertices1 = tempmesh.vertices;
			var normals0 = blendBaseMesh.normals;
			var normals1 = tempmesh.normals;
			var tangents0 = blendBaseMesh.tangents;
			var tangents1 = tempmesh.tangents;
			for(var i = 0; i < vcount; i++)
			{
				deltavec[i] = vertices1[i] - vertices0[i];
				deltanorm[i] = normals1[i] - normals0[i];
				deltatan[i] = tangents1[i] - tangents0[i];
			}
			var cnt0 = 0;
			for(var i = 0; i < 256; i++)
			{
				var flag1 = false;
				if(blendnames.Contains(blendBaseMesh.name + i))
				{
					flag1 = true;
				}
				if(!flag1)
				{
					cnt0 = i;
					break;
				}
			}
			currentMesh.AddBlendShapeFrame(blendBaseMesh.name + cnt0, 100.0f, deltavec, deltanorm, deltatan);
			DestroyImmediate(blendBaseMesh);

			//currentMesh.RecalculateNormals();
			CalcMeshNormals(currentMesh);
			currentMesh.RecalculateBounds();
			currentMesh.RecalculateTangents();
		}

		private static void BlendShapeDelete(int delint, Mesh mesh)
		{
			var vertices = currentMesh.vertices;
			var blendnames = new List<string>();
			var bweights = new List<float>();
			var bvertices = new List<Vector3[]>();
			var bnormals = new List<Vector3[]>();
			var btangents = new List<Vector3[]>();
			var bcnt = mesh.blendShapeCount;
			for(var i = 0; i < bcnt; i++)
			{
				if(i == delint)
				{
					continue;
				}
				var fcnt = mesh.GetBlendShapeFrameCount(i);
				for(var j = 0; j < fcnt; j++)
				{
					blendnames.Add(mesh.GetBlendShapeName(i));
					bweights.Add(mesh.GetBlendShapeFrameWeight(i, j));
					var dverts = Enumerable.Repeat(Vector3.zero, vertices.Length).ToArray();
					var dnormals = Enumerable.Repeat(Vector3.zero, vertices.Length).ToArray();
					var dtangents = Enumerable.Repeat(Vector3.zero, vertices.Length).ToArray();
					mesh.GetBlendShapeFrameVertices(i, j, dverts, dnormals, dtangents);
					bvertices.Add(dverts);
					bnormals.Add(dnormals);
					btangents.Add(dtangents);
				}
			}

			mesh.ClearBlendShapes();
			for(var i = 0; i < blendnames.Count; i++)
			{
				mesh.AddBlendShapeFrame(blendnames[i], bweights[i], bvertices[i], bnormals[i], btangents[i]);
			}
		}

		private static void BlendShapeRename(int renameint, string newname, Mesh mesh)
		{
			var vertices = currentMesh.vertices;
			var blendnames = new List<string>();
			var bweights = new List<float>();
			var bvertices = new List<Vector3[]>();
			var bnormals = new List<Vector3[]>();
			var btangents = new List<Vector3[]>();
			var bcnt = mesh.blendShapeCount;
			for(var i = 0; i < bcnt; i++)
			{
				var fcnt = mesh.GetBlendShapeFrameCount(i);

				//Changed 2025/02/21
				if(i == renameint)
				{
					blendnames.Add(newname);
				}
				else
				{
					blendnames.Add(mesh.GetBlendShapeName(i));
				}

				//End changed 2025/02/21
				for(var j = 0; j < fcnt; j++)
				{
					//Changed 2025/02/21
					//if (j == renameint) blendnames.Add(newname);
					//else blendnames.Add(mesh.GetBlendShapeName(i));
					//End changed 2025/02/21
					bweights.Add(mesh.GetBlendShapeFrameWeight(i, j));
					var dverts = Enumerable.Repeat(Vector3.zero, vertices.Length).ToArray();
					var dnormals = Enumerable.Repeat(Vector3.zero, vertices.Length).ToArray();
					var dtangents = Enumerable.Repeat(Vector3.zero, vertices.Length).ToArray();
					mesh.GetBlendShapeFrameVertices(i, j, dverts, dnormals, dtangents);
					bvertices.Add(dverts);
					bnormals.Add(dnormals);
					btangents.Add(dtangents);
				}
			}

			mesh.ClearBlendShapes();
			for(var i = 0; i < blendnames.Count; i++)
			{
				mesh.AddBlendShapeFrame(blendnames[i], bweights[i], bvertices[i], bnormals[i], btangents[i]);
			}
		}

		private static void BakeBlendShape(int idx, Mesh mesh)
		{
			var vcnt = mesh.vertexCount;
			var bvertices = new List<Vector3[]>();
			var bnormals = new List<Vector3[]>();
			var btangents = new List<Vector3[]>();
			var bcnt = mesh.blendShapeCount;
			var dverts = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
			var dnormals = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
			var dtangents = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
			mesh.GetBlendShapeFrameVertices(idx, 0, dverts, dnormals, dtangents);
			var vertices = mesh.vertices;
			for(var i = 0; i < vcnt; i++)
			{
				vertices[i] += currentObject.transform.TransformPoint(dverts[i]);
			}
			mesh.vertices = vertices;
			BlendShapeDelete(idx, mesh);
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
			try
			{
				currentSkinned.localBounds = currentMesh.bounds;
				EditorSculptPrefab(false, true);
			}
			catch
			{
			}
		}

		private static bool BlendShapeUnityVer()
		{
			try
			{
				var unityver = Application.unityVersion;
				var verintlist = UnityVersionToIntList(unityver);

				//for (int i = 0; i < verintlist.Count; i++)Debug.Log(verintlist[i]);
				if(unityver.Contains("f"))
				{
					if(verintlist[0] >= 2020)
					{
						return true;
					}
					if(verintlist[0] == 2019)
					{
						if(verintlist[1] >= 3)
						{
							return true;
						}
						if(verintlist[1] == 2)
						{
							if(verintlist[2] >= 10)
							{
								return true;
							}
						}
						if(verintlist[1] == 1)
						{
							return true;
						}
					}
				}
				else if(unityver.Contains("b"))
				{
					if(verintlist[0] >= 2020)
					{
						return true;
					}
					if(verintlist[0] == 2019)
					{
						if(verintlist[1] >= 4)
						{
							return true;
						}
						if(verintlist[1] == 3)
						{
							if(verintlist[3] >= 9)
							{
								return true;
							}
						}
					}
				}
				else if(unityver.Contains("a"))
				{
					if(verintlist[0] >= 2021)
					{
						return true;
					}
					if(verintlist[0] == 2020)
					{
						if(verintlist[1] >= 2)
						{
							return true;
						}
						if(verintlist[1] == 1)
						{
							if(verintlist[3] >= 11)
							{
								return true;
							}
						}
					}
				}
			}
			catch
			{
			}
			return false;
		}

		private static List<int> UnityVersionToIntList(string str)
		{
			var vintlist = new List<int>();
			var strint = 0;
			var tempsb = new StringBuilder(str);
			var tempstr = tempsb.ToString();
			for(var i = 0; i < 256; i++)
			{
				for(var j = 0; j < str.Length; j++)
				{
					tempstr = tempsb.ToString();
					if(tempstr.StartsWith("0"))
					{
						strint += 0;
					}
					else if(tempstr.StartsWith("1"))
					{
						strint += 1;
					}
					else if(tempstr.StartsWith("2"))
					{
						strint += 2;
					}
					else if(tempstr.StartsWith("3"))
					{
						strint += 3;
					}
					else if(tempstr.StartsWith("4"))
					{
						strint += 4;
					}
					else if(tempstr.StartsWith("5"))
					{
						strint += 5;
					}
					else if(tempstr.StartsWith("6"))
					{
						strint += 6;
					}
					else if(tempstr.StartsWith("7"))
					{
						strint += 7;
					}
					else if(tempstr.StartsWith("8"))
					{
						strint += 8;
					}
					else if(tempstr.StartsWith("9"))
					{
						strint += 9;
					}
					else
					{
						try
						{
							tempsb.Remove(0, 1);
						}
						catch
						{
						}
						tempstr = tempsb.ToString();
						break;
					}
					try
					{
						tempsb.Remove(0, 1);
						tempstr = tempsb.ToString();
					}
					catch
					{
						break;
					}
					tempstr = tempsb.ToString();
					if(tempsb.Length < 1)
					{
						break;
					}
					if(tempstr.StartsWith("0") || tempstr.StartsWith("1") || tempstr.StartsWith("2") || tempstr.StartsWith("3")
					   || tempstr.StartsWith("4") || tempstr.StartsWith("5") || tempstr.StartsWith("6")
					   || tempstr.StartsWith("7") || tempstr.StartsWith("8") || tempstr.StartsWith("9"))
					{
						strint *= 10;
					}
				}
				vintlist.Add(strint);
				if(tempstr.Length > 0)
				{
					strint = 0;
				}
				else
				{
					try
					{
						tempsb.Remove(0, 1);
					}
					catch
					{
					}
					break;
				}
			}
			return vintlist;
		}

		/*static void DoSubwindow(int windowID)
		{
		    bool hogehoge = true;
		    GUILayout.Label("hogehoge");
		    //bint = EditorGUILayout.Popup("", bint, BrushStrings);
		    if (BrushStrings[bint] == "BETA_Spline")
		    {
		        splinetype = (SplineAction)EditorGUILayout.EnumPopup("Spline Mode:", splinetype);
		        splinepln = (SplinePlane)EditorGUILayout.EnumPopup("Spline Plane:", splinepln);
		        SplineAsMesh = EditorGUILayout.Toggle("Spline as Mesh", SplineAsMesh);
		        SplineSubdivide = EditorGUILayout.Toggle("Subdivide Supline", SplineSubdivide);
		        splineOffset = EditorGUILayout.FloatField("Spline Offset", splineOffset);
		        AutoSplineProjection = EditorGUILayout.Toggle("Auto Projection", AutoSplineProjection);
		        guiLineColor = EditorGUILayout.ColorField("Line Color", guiLineColor);

		        if (splinetype == SplineAction.DrawSpline)
		        {
		            GUILayout.Label(" Spline Action:");
		            if (GUILayout.Button("Save Spline")) Spline3DSave();
		            if (GUILayout.Button("Reset Spline"))
		            {
		                Spline2DList = new List<Vector3>();
		                Spline3DList = new List<Vector3>();
		            }
		        }
		    }
		    //if (BrushStrings[bint] == "BETA_Spline") splinetype = (SplineAction)EditorGUILayout.EnumPopup(splinetype);
		    if (splinetype != splineoldtype)
		    {
		        splineoldtype = splinetype;
		        Spline2DList = new List<Vector3>();
		        Spline3DList = new List<Vector3>();
		        //SplineDirList = new List<Vector3>();
		    }
		}*/

		private static void SculptDraw()
		{
			if(!IsEnable)
			{
				return;
			}
			if(boneAct != BoneAction.None)
			{
				return;
			}
			var r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

			//Vector3 rv = currentObject.transform.InverseTransformPoint(r.origin);
			var rd = currentObject.transform.InverseTransformDirection(r.direction);
			rd.x /= currentObject.transform.localScale.x;
			rd.y /= currentObject.transform.localScale.y;
			rd.z /= currentObject.transform.localScale.z;
			var delta = HandleUtility.GUIPointToWorldRay(Event.current.delta + Event.current.mousePosition);

			//Vector3 delv = currentObject.transform.InverseTransformPoint(delta.origin);
			var pdist = Vector3.Distance(r.origin, currentObject.transform.TransformPoint(BrushHitPos));
			var rp = r.GetPoint(pdist);
			rp = currentObject.transform.InverseTransformPoint(rp);
			var delp = delta.GetPoint(pdist);
			delp = currentObject.transform.InverseTransformPoint(delp);

			var vertices = currentMesh.vertices;
			var normals = currentMesh.normals;
			var triangles = currentMesh.GetTriangles(0);
			var maskv2 = currentMesh.uv4;
			var colors = currentMesh.colors;
			var hitpos = Vector3.zero;
			if(IsBrushedArray == null)
			{
				IsBrushedArray = Enumerable.Repeat(false, vertices.Length).ToArray();
			}
			else if(vertices.Length != IsBrushedArray.Length)
			{
				IsBrushedArray = Enumerable.Repeat(false, vertices.Length).ToArray();
			}

			//if (mergedtris.Length != triangles.Length) MergetriGenerate(currentMesh);
			if(mergedtris.Length < 3)
			{
				MergetriGenerate(currentMesh);
			}
			if(IsOldVertList && vertices.Length != oldVertList.Count)
			{
				oldVertList = vertices.ToList();
			}
			if(IsOldNormList && normals.Length != oldNormalList.Count)
			{
				oldNormalList = normals.ToList();
			}
			var BrushRadFix = GlobalBrushRad
				? BrushRadius
				  * HandleUtility.GetHandleSize(currentObject.transform.TransformPoint(currentMesh.bounds.center)) * 2.0f
				: BrushRadius;

			if(BrushString == "BETA_Spline" && splinetype == SplineAction.MovePoint)
			{
				if(splinepln == SplinePlane.FREE_3D)
				{
					for(var i = 0; i < Spline3DListList.Count; i++)
					{
						for(var j = 0; j < Spline3DListList[i].Count; j++)
						{
							var v0 = Spline3DListList[i][j];
							v0 = currentObject.transform.TransformPoint(v0);
							var d3 = GetStroke(v0, currentObject.transform.TransformPoint(BrushHitPos));
							if(d3 <= BrushRadFix)
							{
								var spdirpos = (delp - rp) * (1 - d3 / BrushRadFix);
								Spline3DListList[i][j] += spdirpos;
								var spdirp = Event.current.delta * (1 - d3 / BrushRadFix);
								var currp = HandleUtility.WorldToGUIPoint(v0);
								var curray = HandleUtility.GUIPointToWorldRay(currp + spdirp);
								Spline2DListList[i][j] = curray.origin;
								SplineDirListList[i][j] = curray.direction;
							}
						}
					}
					Spline3DUpdate();
					return;
				}

				var splineplanes = Get2DSplinePlane(false);
				var n0 = Vector3.Cross(splineplanes[2] - splineplanes[1], splineplanes[1] - splineplanes[0]);
				var sv0 = splineplanes[0];
				var d0 = Mathf.Abs(Vector3.Dot(r.origin - sv0, n0)) / Mathf.Abs(Vector3.Dot(r.direction, n0));
				var rhitpos = r.GetPoint(d0);
				var d1 = Mathf.Abs(Vector3.Dot(delta.origin - sv0, n0)) / Mathf.Abs(Vector3.Dot(r.direction, n0));
				var dhitpos = delta.GetPoint(d1);
				for(var i = 0; i < Spline3DListList.Count; i++)
				{
					for(var j = 0; j < Spline3DListList[i].Count; j++)
					{
						var v0 = Spline3DListList[i][j];
						v0 = currentObject.transform.TransformPoint(v0);
						var d2 = GetStroke(v0, currentObject.transform.TransformPoint(BrushHitPos));

						//Vector3 v0 = Spline2DListList[i][j];
						//float d2 = GetStroke(Spline2DListList[i][j], BrushHitPos);
						//v0 = currentObject.transform.InverseTransformPoint(v0);
						//float d2 = GetStroke(Spline2DListList[i][j], currentObject.transform.TransformPoint(BrushHitPos));
						if(d2 <= BrushRadFix)
						{
							var spdirpos = (dhitpos - rhitpos) * (1 - d2 / BrushRadFix);
							Spline3DListList[i][j] += spdirpos;
							var spdirp = Event.current.delta * (1 - d2 / BrushRadFix);

							//Vector2 currp = HandleUtility.WorldToGUIPoint(currentObject.transform.TransformPoint(v0));
							var currp = HandleUtility.WorldToGUIPoint(v0);
							var curray = HandleUtility.GUIPointToWorldRay(currp + spdirp);

							//float d3 = Mathf.Abs(Vector3.Dot((curray.origin - sv0), n0) / Mathf.Abs(Vector3.Dot(curray.direction, n0)));
							//Spline2DListList[i][j] = currentObject.transform.InverseTransformPoint(curray.GetPoint(d3));
							//SplineDirListList[i][j] = currentObject.transform.InverseTransformDirection(curray.GetPoint(d3));
							//SplineDirListList[i][j] = curray.direction;
							Spline2DListList[i][j] = curray.origin;
							SplineDirListList[i][j] = curray.direction;
						}
					}
				}
				Spline3DUpdate();
				return;
			}
			if(IsAnimationBrush)
			{
				var bones = GetAnimatorBones();
				if(bones.Length < 2)
				{
					EditorUtility.DisplayDialog("Caution", "This Mesh has a few bones. So,AnimationMove brush will not work." +
					                                       "Add bone with Animation/Add a new Bone first", "OK");
					GUIUtility.ExitGUI();
					RestoreBrush();
					return;
				}
				if(BoneMinIdx < 0)
				{
					return;
				}
				var aclips = AnimationUtility.GetAnimationClips(currentObject.transform.root.gameObject);
				if(aclips.Length < 1)
				{
					if(EditorUtility.DisplayDialog("Caution", "This Mesh has no AnimationClips. Do you Setup Animator?", "OK", "Cancel"))
					{
						AnimatorSetup(false);
					}
					GUIUtility.ExitGUI();
					return;
				}
				if(!AnimationMode.InAnimationMode())
				{
					return;
				}
				Vector3 bv0 = Vector3.zero, bv1 = Vector3.zero;
				var bonet = bones[BoneMinIdx];

				//if (BrushString == "AnimationTip")
				if(BrushString == "AnimationTip" || IsBoneTips)
				{
					bv0 = bones[BoneMinIdx].transform.position;
					bv1 = extrabones[BoneMinIdx];
				}
				else
				{
					try
					{
						bv0 = bones[BoneMinIdx].transform.parent.position;
					}
					catch
					{
						return;
					}
					bv1 = bones[BoneMinIdx].transform.position;
				}

				//Ray rbt = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(bonet.position));
				var rbt = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(bv1));
				var movepos = bonet.transform.position + (currentObject.transform.TransformPoint(delp) - currentObject.transform.TransformPoint(rp))
					* 0.1f;
				var br1 = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(bv1));
				var rdist = Vector3.Distance(br1.origin, bv1);
				var delr = delta.GetPoint(rdist);
				var rr = r.GetPoint(rdist);

				var bodylimit = 1.0f;
				var anim = currentObject.transform.root.gameObject.GetComponent<Animator>();
				Quaternion q0;
				if(anim && IsLimitTrunk)
				{
					//Added 2022/11/22
					if(anim.isHuman)

						//End Added 2022/11/22
					{
						if(bonet.parent == anim.GetBoneTransform(HumanBodyBones.Hips)
						   || bonet == anim.GetBoneTransform(HumanBodyBones.Hips))
						{
							bodylimit = 0.05f;
						}
						else if(bonet.parent == anim.GetBoneTransform(HumanBodyBones.Spine)
						        || bonet == anim.GetBoneTransform(HumanBodyBones.Spine))
						{
							bodylimit = 0.05f;
						}
						else if(bonet.parent == anim.GetBoneTransform(HumanBodyBones.Neck)
						        || bonet == anim.GetBoneTransform(HumanBodyBones.Neck))
						{
							bodylimit = 0.05f;
						}
						else if(bonet.parent == anim.GetBoneTransform(HumanBodyBones.Head)
						        || bonet == anim.GetBoneTransform(HumanBodyBones.Head))
						{
							bodylimit = 0.05f;
						}
						else if(bonet.parent == anim.GetBoneTransform(HumanBodyBones.UpperChest)
						        || bonet == anim.GetBoneTransform(HumanBodyBones.UpperChest))
						{
							bodylimit = 0.05f;
						}
						else if(bonet.parent == anim.GetBoneTransform(HumanBodyBones.Chest)
						        || bonet == anim.GetBoneTransform(HumanBodyBones.Chest))
						{
							bodylimit = 0.05f;
						}
					}
				}
				var dist = Vector3.Distance(r.GetPoint(pdist), rbt.GetPoint(pdist));
				if(dist > BrushRadFix)
				{
					return;
				}
				IsBoneMove = true;
#if MemoryAnimation
				IsAnimationSaved = false;
#endif
				if(!anim.isHuman)
				{
					//if (BrushString == "AnimationMove")
					if(BrushString == "AnimationMove" && !IsBoneTips)
					{
						movepos = bonet.transform.position + (delr - rr) * bodylimit * (1 - dist / BrushRadFix);
						switch(moveMode)
						{
							case AnimMoveMode.Rotate:
								q0 = Quaternion.FromToRotation(bv1 - bv0, movepos - bv0);
								bonet.parent.rotation = q0 * bonet.parent.rotation;
								break;
							case AnimMoveMode.Toranslate:
								bonet.position = movepos;
								break;
							case AnimMoveMode.Scale:
								bonet.position = bv1 + (bv0 - bv1) * (1.0f - Vector3.Distance(bv0, movepos) / Vector3.Distance(bv0, bv1));
								break;
							case AnimMoveMode.Revert:
								bonet.parent.rotation = Quaternion.Lerp(bonet.parent.rotation, Quaternion.identity,
									1.0f - Vector3.Distance(bv0, movepos) / Vector3.Distance(bv0, bv1));
								break;
						}

						//movepos = bonet.transform.position + (delr - rr) * bodylimit * (1 - (dist / BrushRadFix));
						//q0 = Quaternion.FromToRotation(bv1 - bv0, movepos - bv0);
						//bonet.parent.rotation = q0 * bonet.parent.rotation;
					}

					//else if (BrushString == "AnimationTip")
					else
					{
						movepos = bv1 + (delr - rr) * bodylimit * (1 - dist / BrushRadFix);
						q0 = Quaternion.FromToRotation(bv1 - bv0, movepos - bv0);
						bonet.rotation = q0 * bonet.rotation;
					}
				}
				else
				{
					var qp0 = bonet.parent.rotation;

					var posehand = new HumanPoseHandler(anim.avatar, currentObject.transform.root);
					var humanPose = new HumanPose();
					var oldhand = new HumanPoseHandler(anim.avatar, currentObject.transform.root);
					var oldpose = new HumanPose();
					oldhand.GetHumanPose(ref oldpose);
					var oldmusclefs = oldpose.muscles;

					//if (BrushString == "AnimationMove")
					if(BrushString == "AnimationMove" && !IsBoneTips)
					{
						movepos = bonet.transform.position + (delr - rr) * bodylimit * (1 - dist / BrushRadFix);
						switch(moveMode)
						{
							case AnimMoveMode.Rotate:
								q0 = Quaternion.FromToRotation(bv1 - bv0, movepos - bv0);
								bonet.parent.rotation = q0 * bonet.parent.rotation;
								break;
							case AnimMoveMode.Toranslate:
								bonet.position = movepos;
								break;
							case AnimMoveMode.Scale:
								bonet.position = bv1 + (bv0 - bv1) * (1.0f - Vector3.Distance(bv0, movepos) / Vector3.Distance(bv0, bv1));
								break;
							case AnimMoveMode.Revert:
								bonet.parent.localRotation = Quaternion.Lerp(bonet.parent.localRotation, Quaternion.identity,
									1.0f - Vector3.Distance(bv0, movepos) / Vector3.Distance(bv0, bv1));
								break;
							case AnimMoveMode.Paste:
								var boneminp = -1;
								for(var i = 0; i < bones.Length; i++)
								{
									if(bones[i] == bonet.transform.parent)
									{
										boneminp = i;
										break;
									}
								}

								//bonet.parent.rotation = Quaternion.Lerp(bonet.parent.rotation, AnimPoseRotateList[boneminp],
								//    1.0f - Vector3.Distance(bv0, movepos) / Vector3.Distance(bv0, bv1));

								if(BoneMinIdx >= 0 && AnimPoseRotateList.Count > BoneMinIdx)
								{
									//bonet.rotation = Quaternion.Lerp(bonet.rotation, AnimPoseRotateList[BoneMinIdx],(1 - (dist / BrushRadFix)));
									bonet.parent.rotation = Quaternion.Lerp(bonet.parent.rotation, AnimPoseRotateList[boneminp],
										1 - dist / BrushRadFix);

									//bonet.position = bonet.position + (AnimPoseTraList[BoneMinIdx] - bonet.position) * (1 - (dist / BrushRadFix));
								}

								// (1 - (dist / BrushRadFix) (1 - (dist / BrushRadFix)
								break;
						}

						//movepos = bonet.transform.position + (delr - rr) * bodylimit * (1 - (dist / BrushRadFix));
						//q0 = Quaternion.FromToRotation(bv1 - bv0, movepos - bv0);
						//bonet.parent.rotation = q0 * bonet.parent.rotation;
					}

					//else if (BrushString == "AnimationTip")
					else
					{
						movepos = bv1 + (delr - rr) * bodylimit * (1 - dist / BrushRadFix);
						q0 = Quaternion.FromToRotation(bv1 - bv0, movepos - bv0);
						bonet.rotation = q0 * bonet.rotation;
					}
					posehand.GetHumanPose(ref humanPose);
					var musclefs = humanPose.muscles;
					if(IsHumanLimit)
					{
						var aclip = aclips[aclipidx];
						var bindings = AnimationUtility.GetCurveBindings(aclip);
						var musclenames = HumanTrait.MuscleName;
						var islimit = false;
						for(var i = 0; i < bones.Length; i++)
						{
							var muscleids = TransformToMuscleIds(bones[i], anim);
							for(var j = 0; j < muscleids.Length; j++)
							{
								var mid = muscleids[j];
								if(musclefs[mid] < -1.0f && oldmusclefs[mid] >= -1.0f)
								{
									musclefs[muscleids[j]] = -0.99f;
									islimit = true;
								}
								if(musclefs[mid] > 1.0f && oldmusclefs[mid] <= 1.0f)
								{
									musclefs[muscleids[j]] = 0.99f;
									islimit = true;
								}
							}
						}
						if(islimit)
						{
							anim.applyRootMotion = false;
							humanPose.muscles = musclefs;
							var iki = GetRootFromAnimationClip(aclip);
							humanPose.bodyPosition = iki.position;
							humanPose.bodyRotation = iki.rotate;
							posehand.SetHumanPose(ref humanPose);
						}

						//else if(BrushString == "AnimationTip")
						//{
						//    Matrix4x4 extmat = Matrix4x4.Rotate(bonet.rotation);
						//    extrabones[BoneMinIdx] = extmat.MultiplyPoint(Vector3.left).normalized * Vector3.Distance(bv1, bv0) + bv0;
						//}
					}
				}

				//if (BrushString == "AnimationTip")
				if(BrushString == "AnimationTip" || IsBoneTips)
				{
					var extmat = Matrix4x4.Rotate(bonet.rotation);
					extrabones[BoneMinIdx] = extmat.MultiplyPoint(Vector3.left).normalized * Vector3.Distance(bv1, bv0) + bv0;
				}
			}
			else if(AnimationMode.InAnimationMode())
			{
				return;
			}
			BrushOldHitPos = BrushHitPos;
			if(BrushString == "Draw" || BrushString == "Lower")
			{
				hitpos = BrushOldHitPos;
			}
			else
			{
				hitpos = BrushHitPos;
			}
			if(hitpos == Vector3.zero)
			{
				return;
			}
			var thitp = currentObject.transform.TransformPoint(BrushHitPos);
			var ithitp = currentObject.transform.TransformPoint(GetSymmetry(BrushHitPos));

			//bool IsPainted = false;
			//if (!BrushHitFlag) return;
			switch(BrushString)
			{
				/*case "BETA_BoneSpike":
				    Transform[] bones = GetAnimatorBones();
				    if (bones.Length<2 || BoneMinIdx < 0) return;
				    Vector3 bv0 = Vector3.zero, bv1 = Vector3.zero;
				    try
				    {
				        bv0 = bones[BoneMinIdx].transform.parent.position;
				    }
				    catch { return; }
				    bv1 = bones[BoneMinIdx].transform.position;
				    for (int i=0;i<vertices.Length;i++)
				    {
				        float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
				        if (dist > BrushRadFix) continue;
				        if (maskv2[i].x < maskWeight) continue;
				        //Vector3 dirpos = (bv1 - bv0).normalized*BrushStrength*0.1f * (1 - (dist / BrushRadFix));
				        Vector3 dirpos = (currentObject.transform.TransformPoint(hitpos)-bv0).normalized * BrushStrength * 0.1f * (1 - (dist / BrushRadFix));
				        vertices[i] += dirpos;
				        IsBrushedArray[i] = true;
				    }
				    break;*/
				case "Move":
					//Vector2 hitd = HandleUtility.WorldToGUIPoint(currentObject.transform.TransformPoint(hitpos));
					var hitd = HandleUtility.WorldToGUIPoint(thitp);
					var oldrv = HandleUtility.GUIPointToWorldRay(hitd).origin;
					var rg = r.GetPoint(Vector3.Distance(oldrv, thitp));
					if(GetStroke(rg, thitp) > BrushRadFix)
					{
						return;
					}

					//Vector3 rg = r.GetPoint(Vector3.Distance(oldrv, currentObject.transform.TransformPoint(hitpos)));
					//if (GetStroke(rg, currentObject.transform.TransformPoint(hitpos)) > BrushRadFix) return;
					for(var i = 0; i < vertices.Length; i++)
					{
						if(smode != SymmetryMode.None)
						{
							var ihitpos = GetSymmetry(hitpos);

							//float idist = (vertices[i]-ihitpos).magnitude;
							//float idist = GetStroke(vertices[i], ihitpos);
							//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
							var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
							if(idist <= BrushRadFix && maskv2[i].x >= maskWeight)
							{
								//Vector3 idirpos = Vector3.zero;
								//if (!Camera.current.orthographic) idirpos = (delv - rv).normalized * (1 - (idist / BrushRadFix))*0.1f;
								//else idirpos = (delv - rv) * (1 - (idist / BrushRadFix));
								//Vector3 idirpos = (delv - rv) * (1 - (idist / BrushRadFix));
								//if (!Camera.current.orthographic) idirpos = (delp - rp) * (1 - (idist / BrushRadFix));
								var idirpos = (delp - rp) * (1 - idist / BrushRadFix);
								idirpos = GetSymmetry(idirpos);

								// if (!Camera.current.orthographic) idirpos *= Camera.current.fieldOfView;
								vertices[i] += idirpos;

								//newverts[i] += idirpos;
								IsBrushedArray[i] = true;
							}
						}

						//Vector3 dirpos = (delta.origin-r.origin)*(1-(dist/BrushRadius));
						//Vector3 dirpos = (delv-rv).normalized*0.1f*(1-(dist/BrushRadius));
						//float dist = (vertices[i] - hitpos).magnitude;
						//float dist = GetStroke(vertices[i], hitpos);
						//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
						var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
						if(dist > BrushRadFix)
						{
							continue;
						}
						if(maskv2[i].x < maskWeight)
						{
							continue;
						}

						//Vector3 dirpos = (currentObject.transform.TransformPoint(delv) - currentObject.transform.TransformPoint(rv))*(1 - (dist / BrushRadFix));
						//Vector3 dirpos = Vector3.zero;
						// if (!Camera.current.orthographic)dirpos = (delv - rv).normalized * (1 - (dist / BrushRadFix))*0.1f;
						//   else dirpos = (delv - rv) * (1 - (dist / BrushRadFix));
						//if (!Camera.current.orthographic) dirpos *= Camera.current.fieldOfView;
						//Vector3 dirpos = (delv - rv) * (1 - (dist / BrushRadFix));
						//if (!Camera.current.orthographic)dirpos = (delp - rp) * (1 - (dist / BrushRadFix));
						var dirpos = (delp - rp) * (1 - dist / BrushRadFix);

						//Vector3 dirpos = (delp - rp).normalized*0.03f*BrushStrength * (1 - (dist / BrushRadFix));
						//if (!Camera.current.orthographic) dirpos *= HandleUtility.GetHandleSize(currentObject.transform.TransformPoint(currentMesh.bounds.center));
						vertices[i] += dirpos;

						//newverts[i] += dirpos;
						IsBrushedArray[i] = true;
					}
					break;
				case "Move2D":
					var hitd2 = HandleUtility.WorldToGUIPoint(thitp);
					var oldrv2 = HandleUtility.GUIPointToWorldRay(hitd2).origin;
					var rg2 = r.GetPoint(Vector3.Distance(oldrv2, thitp));
					if(GetStroke(rg2, thitp) > BrushRadFix)
					{
						return;
					}
					for(var i = 0; i < vertices.Length; i++)
					{
						if(smode != SymmetryMode.None)
						{
							var ihitpos = GetSymmetry(hitpos);
							var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
							if(idist <= BrushRadFix && maskv2[i].x >= maskWeight)
							{
								var idirpos = (delp - rp) * (1 - idist / BrushRadFix);
								idirpos = GetSymmetry(idirpos);
								switch(moveAx)
								{
									case Move2DAxis.XYPlane:
										idirpos.z = 0.0f;
										break;
									case Move2DAxis.YZPlane:
										idirpos.x = 0.0f;
										break;
									case Move2DAxis.ZXPLane:
										idirpos.y = 0.0f;
										break;
									case Move2DAxis.XDirection:
										idirpos.y = 0.0f;
										idirpos.z = 0.0f;
										break;
									case Move2DAxis.YDirection:
										idirpos.x = 0.0f;
										idirpos.z = 0.0f;
										break;
									case Move2DAxis.ZDirection:
										idirpos.x = 0.0f;
										idirpos.y = 0.0f;
										break;
								}
								vertices[i] += idirpos;
								IsBrushedArray[i] = true;
							}
						}
						var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
						if(dist > BrushRadFix)
						{
							continue;
						}
						if(maskv2[i].x < maskWeight)
						{
							continue;
						}
						var dirpos = (delp - rp) * (1 - dist / BrushRadFix);
						switch(moveAx)
						{
							case Move2DAxis.XYPlane:
								dirpos.z = 0.0f;
								break;
							case Move2DAxis.YZPlane:
								dirpos.x = 0.0f;
								break;
							case Move2DAxis.ZXPLane:
								dirpos.y = 0.0f;
								break;
							case Move2DAxis.XDirection:
								dirpos.y = 0.0f;
								dirpos.z = 0.0f;
								break;
							case Move2DAxis.YDirection:
								dirpos.x = 0.0f;
								dirpos.z = 0.0f;
								break;
							case Move2DAxis.ZDirection:
								dirpos.x = 0.0f;
								dirpos.y = 0.0f;
								break;
						}
						vertices[i] += dirpos;
						IsBrushedArray[i] = true;
					}
					break;
				case "Draw":
					if(IsLazy)
					{
						DoOldBrushHitPosList(BrushRadFix);
					}
					else
					{
						DoDraw(thitp, thitp, ithitp, ithitp, BrushRadFix, false);
					}

					//else DoDraw(thitp, ithitp, BrushRadFix, false);
					break;

				case "Lower":
					if(IsLazy)
					{
						DoOldBrushHitPosList(BrushRadFix);
					}
					else
					{
						DoDraw(thitp, thitp, ithitp, ithitp, BrushRadFix, true);
					}
					break;

				case "Extrude":
					for(var i = 0; i < vertices.Length; i++)
					{
						if(smode != SymmetryMode.None)
						{
							//Vector3 ihitpos = GetSymmetry(hitpos);
							// //float idist = GetStroke(vertices[i], ihitpos);
							//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
							var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
							if(idist <= BrushRadFix && maskv2[i].x >= maskWeight)
							{
								//Vector3 idirpos = normals[i] * BrushStrength * 0.01f * (1 - (idist / BrushRadFix));
								var idirpos = oldNormalList[i] * BrushStrength * 0.1f;

								//vertices[i] += idirpos;t
								vertices[i] = oldVertList[i] + idirpos;

								//newverts[i] = oldVertList[i] + idirpos;
								IsBrushedArray[i] = true;
							}
						}

						//float dist = GetStroke(vertices[i], hitpos);
						//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
						var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
						if(dist > BrushRadFix)
						{
							continue;
						}
						if(maskv2[i].x < maskWeight)
						{
							continue;
						}
						var dirpos = oldNormalList[i] * BrushStrength * 0.1f;

						//vertices[i] += dirpos;
						vertices[i] = oldVertList[i] + dirpos;

						//newverts[i] = oldVertList[i] + dirpos;
						IsBrushedArray[i] = true;
					}
					break;

				case "Dig":
					for(var i = 0; i < vertices.Length; i++)
					{
						if(smode != SymmetryMode.None)
						{
							//Vector3 ihitpos = GetSymmetry(hitpos);
							//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
							var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
							if(idist <= BrushRadFix && maskv2[i].x >= maskWeight)
							{
								var idirpos = oldNormalList[i] * BrushStrength * 0.1f;
								vertices[i] = oldVertList[i] - idirpos;

								//newverts[i] = oldVertList[i] - idirpos;
								IsBrushedArray[i] = true;
							}
						}

						//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
						var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
						if(dist > BrushRadFix)
						{
							continue;
						}
						if(maskv2[i].x < maskWeight)
						{
							continue;
						}
						var dirpos = oldNormalList[i] * BrushStrength * 0.1f;
						vertices[i] = oldVertList[i] - dirpos;

						//newverts[i] = oldVertList[i] - dirpos;
						IsBrushedArray[i] = true;
					}
					break;

				case "Inflat":
					for(var i = 0; i < vertices.Length; i++)
					{
						if(smode != SymmetryMode.None)
						{
							//Vector3 ihitpos = GetSymmetry(hitpos);
							// //float idist = (vertices[i]-ihitpos).magnitude;
							// //float idist = GetStroke(vertices[i], ihitpos);
							//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
							var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
							if(idist <= BrushRadFix && maskv2[i].x >= maskWeight)
							{
								//Vector3 idirpos = normals[i] * BrushStrength * 0.01f * (1 - (idist / BrushRadFix));
								var idirpos = IsRealTimeAutoRemesh ? oldNormalList[i] * BrushStrength * 0.02f * (1 - idist / BrushRadFix) : normals[i] * BrushStrength * 0.02f * (1 - idist / BrushRadFix);
								vertices[i] += idirpos;

								//newverts[i] += idirpos;
								IsBrushedArray[i] = true;
							}
						}

						//float dist = (vertices[i] - hitpos).magnitude;
						//float dist = GetStroke(vertices[i], hitpos);
						//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
						var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
						if(dist > BrushRadFix)
						{
							continue;
						}
						if(maskv2[i].x < maskWeight)
						{
							continue;
						}

						//Vector3 dirpos = normals[i] * BrushStrength * 0.01f * (1 - (dist / BrushRadFix));
						//Vector3 dirpos = Vector3.zero;
						var dirpos = IsRealTimeAutoRemesh ? oldNormalList[i] * BrushStrength * 0.02f * (1 - dist / BrushRadFix) : normals[i] * BrushStrength * 0.02f * (1 - dist / BrushRadFix);
						vertices[i] += dirpos;

						//newverts[i] += dirpos;
						IsBrushedArray[i] = true;
					}
					break;

				case "Pinch":
					for(var i = 0; i < vertices.Length; i++)
					{
						if(smode != SymmetryMode.None)
						{
							//Vector3 ihitpos = GetSymmetry(hitpos);
							// //float idist = (vertices[i]-ihitpos).magnitude;
							// //float idist = GetStroke(vertices[i], ihitpos);
							//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
							var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
							if(idist <= BrushRadFix && maskv2[i].x >= maskWeight)
							{
								//Vector3 idirpos = -normals[i] * BrushStrength * 0.01f * (1 - (idist / BrushRadFix));
								var idirpos = IsRealTimeAutoRemesh ? -oldNormalList[i] * BrushStrength * 0.02f * (1 - idist / BrushRadFix) : -normals[i] * BrushStrength * 0.02f * (1 - idist / BrushRadFix);
								vertices[i] += idirpos;

								//newverts[i] += idirpos;
								IsBrushedArray[i] = true;
							}
						}

						//float dist = (vertices[i] - hitpos).magnitude;
						//float dist = GetStroke(vertices[i], hitpos);
						//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
						var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
						if(dist > BrushRadFix)
						{
							continue;
						}
						if(maskv2[i].x < maskWeight)
						{
							continue;
						}

						//Vector3 dirpos = -normals[i] * BrushStrength * 0.01f * (1 - (dist / BrushRadFix));
						var dirpos = IsRealTimeAutoRemesh ? -oldNormalList[i] * BrushStrength * 0.02f * (1 - dist / BrushRadFix) : -normals[i] * BrushStrength * 0.02f * (1 - dist / BrushRadFix);
						vertices[i] += dirpos;

						//newverts[i] += dirpos;
						IsBrushedArray[i] = true;
					}
					break;

				/*case BrushType.Smooth:
				    Vector3 ihitpos = GetSymmetry(hitpos);
				    int[] BrushTri = GetBrushTri(vertices,triangles,hitpos).ToArray();
				    int[] iBrushTri = BrushTri;
				    List<List<int>> iadjlistlist = new List<List<int>>();
				    if(smode!=SymmetryMode.None)
				    {
				        iBrushTri = GetBrushTri(vertices,triangles,ihitpos).ToArray();
				        iadjlistlist = GetAdjListList(vertices,iBrushTri);
				    }
				    List<List<int>> adjlistlist = GetAdjListList(vertices,BrushTri);
				    for(int i=0;i<vertices.Length;i++)
				    {
				        if(smode!=SymmetryMode.None)
				        {
				            //float idist = (vertices[i]-ihitpos).magnitude;
				            float idist = GetStroke(vertices[i],ihitpos);
				            if(idist<=BrushRadius && MaskArray[i].r>=maskWeight)
				            {
				                Vector3 iavpoint = Vector3.zero;
				                for(int j=0;j<iadjlistlist[i].Count;j++)
				                {
				                iavpoint += vertices[iadjlistlist[i][j]];
				                }
				                iavpoint /= iadjlistlist[i].Count;
				                Vector3 idirpos = (iavpoint-vertices[i]).normalized*BrushStrength*0.01f*(1-(idist/BrushRadius));
				                vertices[i] += idirpos;
				            }
				        }
				        //float dist = (vertices[i]-hitpos).magnitude;
				        float dist = GetStroke(vertices[i],hitpos);
				        if(dist>BrushRadius)continue;
				        if(MaskArray[i].r<maskWeight)continue;
				        Vector3 avpoint = Vector3.zero;
				        for(int j=0;j<adjlistlist[i].Count;j++)
				        {
				        avpoint += vertices[adjlistlist[i][j]];
				        }
				        avpoint /= adjlistlist[i].Count;
				        Vector3 dirpos = (avpoint-vertices[i]).normalized*BrushStrength*0.01f*(1-(dist/BrushRadius));
				        vertices[i] += dirpos;
				    }
				    break;*/

				case "Smooth":
					//int[] adjarr = Enumerable.Repeat(0,triangles.Length).ToArray();
					//int[] adjarr = new int[triangles.Length];
					var adjarr = new int[vertices.Length];

					//Vector3[] avarr = Enumerable.Repeat(Vector3.zero,vertices.Length).ToArray();
					var avarr = new Vector3[vertices.Length];
					try
					{
						for(var i = 0; i < mergedtris.Length; i += 3)
						{
							if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
							{
								continue;
							}
							adjarr[mergedtris[i]] += 2;
							adjarr[mergedtris[i + 1]] += 2;
							adjarr[mergedtris[i + 2]] += 2;
						}
					}
					catch
					{
						MergetriGenerate(currentMesh);
						for(var i = 0; i < mergedtris.Length; i += 3)
						{
							if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
							{
								continue;
							}
							adjarr[mergedtris[i]] += 2;
							adjarr[mergedtris[i + 1]] += 2;
							adjarr[mergedtris[i + 2]] += 2;
						}
						if(DebugMode)
						{
							Debug.Log("Mesh get error.EditorSculpt fix that automatically");
						}
					}

					//for (int i = 0; i < triangles.Length; i += 3)
					//{
					//    Vector3 v0 = vertices[triangles[i]];
					//    Vector3 v1 = vertices[triangles[i + 1]];
					//    Vector3 v2 = vertices[triangles[i + 2]];
					//    int a0 = adjarr[triangles[i]];
					//    int a1 = adjarr[triangles[i + 1]];
					//    int a2 = adjarr[triangles[i + 2]];
					//    avarr[triangles[i]] = avarr[triangles[i]] + v1 / a0 + v2 / a0;
					//    avarr[triangles[i + 1]] = avarr[triangles[i + 1]] + v0 / a1 + v2 / a1;
					//    avarr[triangles[i + 2]] = avarr[triangles[i + 2]] + v0 / a2 + v1 / a2;
					//}
					for(var i = 0; i < mergedtris.Length; i += 3)
					{
						if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
						{
							continue;
						}
						var v0 = vertices[mergedtris[i]];
						var v1 = vertices[mergedtris[i + 1]];
						var v2 = vertices[mergedtris[i + 2]];
						var a0 = adjarr[mergedtris[i]] == 0 ? 1 : adjarr[mergedtris[i]];
						var a1 = adjarr[mergedtris[i + 1]] == 0 ? 1 : adjarr[mergedtris[i + 1]];
						var a2 = adjarr[mergedtris[i + 2]] == 0 ? 1 : adjarr[mergedtris[i + 2]];
						avarr[mergedtris[i]] = avarr[mergedtris[i]] + v1 / a0 + v2 / a0;
						avarr[mergedtris[i + 1]] = avarr[mergedtris[i + 1]] + v0 / a1 + v2 / a1;
						avarr[mergedtris[i + 2]] = avarr[mergedtris[i + 2]] + v0 / a2 + v1 / a2;
					}

					//Changed 2020/06/08
					//for (int i = 0; i < mergedtris.Length; i += 3)
					//{
					//    if (mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0) continue;
					//    if (avarr[mergedtris[i]] != avarr[triangles[i]]) avarr[triangles[i]] = avarr[mergedtris[i]];
					//    if (avarr[mergedtris[i + 1]] != avarr[triangles[i + 1]]) avarr[triangles[i + 1]] = avarr[mergedtris[i + 1]];
					//    if (avarr[mergedtris[i + 2]] != avarr[triangles[i + 2]]) avarr[triangles[i + 2]] = avarr[mergedtris[i + 2]];
					//}
					//End Changed 2020/06/08
					if(IsShortcut && BrushStringOld == "Move2D")
					{
						for(var i = 0; i < avarr.Length; i++)
						{
							switch(moveAx)
							{
								case Move2DAxis.XYPlane:
									avarr[i].z = vertices[i].z;
									break;
								case Move2DAxis.YZPlane:
									avarr[i].x = vertices[i].x;
									break;
								case Move2DAxis.ZXPLane:
									avarr[i].y = vertices[i].y;
									break;
								case Move2DAxis.XDirection:
									avarr[i].y = vertices[i].y;
									avarr[i].z = vertices[i].z;
									break;
								case Move2DAxis.YDirection:
									avarr[i].x = vertices[i].x;
									avarr[i].z = vertices[i].z;
									break;
								case Move2DAxis.ZDirection:
									avarr[i].x = vertices[i].x;
									avarr[i].y = vertices[i].y;
									break;
							}
						}
					}
					for(var i = 0; i < mergedtris.Length; i += 3)
					{
						if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
						{
							continue;
						}
						if(avarr[mergedtris[i]] != avarr[tris[i]])
						{
							avarr[tris[i]] = avarr[mergedtris[i]];
						}
						if(avarr[mergedtris[i + 1]] != avarr[tris[i + 1]])
						{
							avarr[tris[i + 1]] = avarr[mergedtris[i + 1]];
						}
						if(avarr[mergedtris[i + 2]] != avarr[tris[i + 2]])
						{
							avarr[tris[i + 2]] = avarr[mergedtris[i + 2]];
						}
					}
					for(var i = 0; i < vertices.Length; i++)
					{
						if(smode != SymmetryMode.None)
						{
							//Vector3 ihitpos = GetSymmetry(hitpos);
							// //float idist = GetStroke(vertices[i], ihitpos);
							//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
							var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
							if(idist <= BrushRadFix && maskv2[i].x >= maskWeight)
							{
								var ismoothdeg = Mathf.Clamp01((1.0f - idist / BrushRadFix) * BrushStrength * 2.0f);

								//Vector3 idirpos = (avarr[i] - vertices[i]).normalized * BrushStrength * 0.002f * (1 - (idist / BrushRadFix));
								//vertices[i] += idirpos;
								//vertices[i] = avarr[i] * (1.0f - (idist / BrushRadFix)) + vertices[i] * (idist / BrushRadFix);
								vertices[i] = avarr[i] * ismoothdeg + vertices[i] * (1.0f - ismoothdeg);

								//newverts[i] += idirpos;
								IsBrushedArray[i] = true;
							}
						}

						//float dist = GetStroke(vertices[i], hitpos);
						//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
						var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
						if(dist > BrushRadFix)
						{
							continue;
						}
						if(maskv2[i].x < maskWeight)
						{
							continue;
						}
						var smoothdeg = Mathf.Clamp01((1.0f - dist / BrushRadFix) * BrushStrength * 2.0f);

						//Vector3 dirpos = (avarr[i] - vertices[i]).normalized * BrushStrength * 0.002f * (1 - (dist / BrushRadFix));
						//vertices[i] += dirpos;
						//vertices[i] = avarr[i]* (1.0f - (dist / BrushRadFix)) + vertices[i]*(dist / BrushRadFix);
						vertices[i] = avarr[i] * smoothdeg + vertices[i] * (1.0f - smoothdeg);

						//newverts[i] += dirpos;
						IsBrushedArray[i] = true;
					}
					/*for (int i = 0; i < triangles.Length; i++)
					{
					    float dist = GetStroke(currentObject.transform.TransformPoint(vertices[triangles[i]]), currentObject.transform.TransformPoint(hitpos));
					    if (dist > BrushRadFix) continue;
					    if (maskv2[triangles[i]].x < maskWeight) continue;
					    Vector3 dirpos = (avarr[triangles[i]] - vertices[triangles[i]]).normalized * BrushStrength * 0.01f * (1 - (dist / BrushRadFix));
					    //vertices[i] += dirpos;
					    newverts[triangles[i]] += dirpos;
					    IsBrushedArray[triangles[i]] = true;
					}*/
					break;

				case "Flatten":
					float flatdist = 0;
					var flatpos = Vector3.zero;
					if(strokePlane.Raycast(r, out flatdist))
					{
						flatpos = r.GetPoint(flatdist);
					}
					strokePlane.normal = -r.direction;
					flatpos = currentObject.transform.InverseTransformPoint(flatpos);
					for(var i = 0; i < vertices.Length; i++)
					{
						if(smode != SymmetryMode.None)
						{
							//Vector3 ihitpos = GetSymmetry(hitpos);
							// //float idist = GetStroke(vertices[i], ihitpos);
							//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
							var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
							if(idist <= BrushRadFix && maskv2[i].x >= maskWeight)
							{
								Vector3 ivr0 = HandleUtility.WorldToGUIPoint(currentObject.transform.TransformPoint(GetSymmetry(vertices[i])));
								var ir0 = HandleUtility.GUIPointToWorldRay(ivr0);
								var ifltpos = Vector3.zero;
								float ifltdist;
								if(strokePlane.Raycast(ir0, out ifltdist))
								{
									ifltpos = ir0.GetPoint(ifltdist);
									ifltpos = currentObject.transform.InverseTransformPoint(ifltpos);
									ifltpos = GetSymmetry(ifltpos);
								}

								if(ifltdist != 0)
								{
									var iflatdeg = Mathf.Clamp01((1.0f - idist / BrushRadFix) * BrushStrength * 0.5f);
									vertices[i] = ifltpos * iflatdeg + vertices[i] * (1.0f - iflatdeg);

									//newverts[i] += dirpos;
									IsBrushedArray[i] = true;
								}

								//Vector3 idirpos = -(vertices[i] - iflatpos).normalized * BrushStrength * 0.01f * (1 - (idist / BrushRadFix));
								//vertices[i] += idirpos;
								//IsBrushedArray[i] = true;
							}
						}

						//float dist = GetStroke(vertices[i], hitpos);
						//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
						var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
						if(dist > BrushRadFix)
						{
							continue;
						}
						if(maskv2[i].x < maskWeight)
						{
							continue;
						}
						/*if (flatdist != 0)
						{
						    Vector3 dirpos = -(vertices[i] - flatpos).normalized * BrushStrength * 0.01f * (1 - (dist / BrushRadFix));
						    vertices[i] += dirpos;
						    //newverts[i] += dirpos;
						    IsBrushedArray[i] = true;
						}*/
						Vector3 vr0 = HandleUtility.WorldToGUIPoint(currentObject.transform.TransformPoint(vertices[i]));
						var r0 = HandleUtility.GUIPointToWorldRay(vr0);

						//Ray r0 = new Ray(vertices[i]+strokePlane.normal.normalized*BrushRadFix*10.0f, -strokePlane.normal);
						var fltpos = Vector3.zero;
						float fltdist;
						if(strokePlane.Raycast(r0, out fltdist))
						{
							fltpos = r0.GetPoint(fltdist);
						}
						fltpos = currentObject.transform.InverseTransformPoint(fltpos);
						if(fltdist != 0)
						{
							//Vector3 dirpos = -(vertices[i] - fltpos).normalized * BrushStrength * 0.01f * (1 - (dist / BrushRadFix));
							//vertices[i] += dirpos;v
							var flatdeg = Mathf.Clamp01((1.0f - dist / BrushRadFix) * BrushStrength * 0.5f);
							vertices[i] = fltpos * flatdeg + vertices[i] * (1.0f - flatdeg);

							//newverts[i] += dirpos;
							IsBrushedArray[i] = true;
						}
					}
					break;

				case "Erase":
					var adjarr1 = new int[vertices.Length];
					var avarr1 = new Vector3[vertices.Length];
					try
					{
						for(var i = 0; i < mergedtris.Length; i += 3)
						{
							if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
							{
								continue;
							}
							adjarr1[mergedtris[i]] += 2;
							adjarr1[mergedtris[i + 1]] += 2;
							adjarr1[mergedtris[i + 2]] += 2;
						}
					}
					catch
					{
						MergetriGenerate(currentMesh);
						for(var i = 0; i < mergedtris.Length; i += 3)
						{
							if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
							{
								continue;
							}
							adjarr1[mergedtris[i]] += 2;
							adjarr1[mergedtris[i + 1]] += 2;
							adjarr1[mergedtris[i + 2]] += 2;
						}
						if(DebugMode)
						{
							Debug.Log("Mesh get error.EditorSculpt fix that automatically");
						}
					}
					for(var i = 0; i < mergedtris.Length; i += 3)
					{
						var v0 = vertices[mergedtris[i]];
						var v1 = vertices[mergedtris[i + 1]];
						var v2 = vertices[mergedtris[i + 2]];
						var a0 = adjarr1[mergedtris[i]] == 0 ? 1 : adjarr1[mergedtris[i]];
						var a1 = adjarr1[mergedtris[i + 1]] == 0 ? 1 : adjarr1[mergedtris[i + 1]];
						var a2 = adjarr1[mergedtris[i + 2]] == 0 ? 1 : adjarr1[mergedtris[i + 2]];
						avarr1[mergedtris[i]] = avarr1[mergedtris[i]] + v1 / a0 + v2 / a0;
						avarr1[mergedtris[i + 1]] = avarr1[mergedtris[i + 1]] + v0 / a1 + v2 / a1;
						avarr1[mergedtris[i + 2]] = avarr1[mergedtris[i + 2]] + v0 / a2 + v1 / a2;
					}
					for(var i = 0; i < vertices.Length; i++)
					{
						if(smode != SymmetryMode.None)
						{
							//Vector3 ihitpos = GetSymmetry(hitpos);
							// //float idist = GetStroke(vertices[i], ihitpos);
							//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
							var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
							if(idist <= BrushRadFix && maskv2[i].x >= maskWeight)
							{
								var ismoothdeg = Mathf.Clamp01((1.0f - idist / BrushRadFix) * BrushStrength * 2.0f);

								//Vector3 idirpos = (avarr1[i] - vertices[i]).normalized * BrushStrength * 0.01f * (1 - (idist / BrushRadFix));
								//vertices[i] += idirpos;
								//vertices[i] = avarr1[i] * (1.0f - (idist / BrushRadFix)) + vertices[i] * (idist / BrushRadFix);
								vertices[i] = avarr1[i] * ismoothdeg + vertices[i] * (1.0f - ismoothdeg);
								IsBrushedArray[i] = true;
							}
						}

						//float dist = GetStroke(vertices[i], hitpos);
						//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
						var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
						if(dist > BrushRadFix)
						{
							continue;
						}
						if(maskv2[i].x < maskWeight)
						{
							continue;
						}
						var smoothdeg = Mathf.Clamp01((1.0f - dist / BrushRadFix) * BrushStrength * 2.0f);

						//Vector3 dirpos = (avarr1[i] - vertices[i]).normalized * BrushStrength * 0.01f * (1 - (dist / BrushRadFix));
						//vertices[i] += dirpos;
						//vertices[i] = avarr1[i] * (1.0f - (dist / BrushRadFix)) + vertices[i] * (dist / BrushRadFix);
						vertices[i] = avarr1[i] * smoothdeg + vertices[i] * (1.0f - smoothdeg);
						IsBrushedArray[i] = true;
					}
					break;

				case "VertexColor":
					if(!Event.current.shift)
					{
						if(IsLazy)
						{
							DoBrushHitPosList(BrushRadFix);
						}
						else
						{
							DoVertexColor(thitp, thitp, ithitp, ithitp, BrushRadFix);
							return;
						}
					}
					else
					{
						var adjcarr = new int[vertices.Length];
						var avcarr = new Color[vertices.Length];
						try
						{
							for(var i = 0; i < mergedtris.Length; i += 3)
							{
								if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
								{
									continue;
								}
								adjcarr[mergedtris[i]] += 2;
								adjcarr[mergedtris[i + 1]] += 2;
								adjcarr[mergedtris[i + 2]] += 2;
							}
						}
						catch
						{
							MergetriGenerate(currentMesh);
							for(var i = 0; i < mergedtris.Length; i += 3)
							{
								if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
								{
									continue;
								}
								adjcarr[mergedtris[i]] += 2;
								adjcarr[mergedtris[i + 1]] += 2;
								adjcarr[mergedtris[i + 2]] += 2;
							}
							if(DebugMode)
							{
								Debug.Log("Mesh get error.EditorSculpt fix that automatically");
							}
						}
						for(var i = 0; i < mergedtris.Length; i += 3)
						{
							if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
							{
								continue;
							}
							var c0 = colors[mergedtris[i]];
							var c1 = colors[mergedtris[i + 1]];
							var c2 = colors[mergedtris[i + 2]];
							var a0 = adjcarr[mergedtris[i]] == 0 ? 1 : adjcarr[mergedtris[i]];
							var a1 = adjcarr[mergedtris[i + 1]] == 0 ? 1 : adjcarr[mergedtris[i + 1]];
							var a2 = adjcarr[mergedtris[i + 2]] == 0 ? 1 : adjcarr[mergedtris[i + 2]];
							avcarr[mergedtris[i]] = avcarr[mergedtris[i]] + c1 / a0 + c2 / a0;
							avcarr[mergedtris[i + 1]] = avcarr[mergedtris[i + 1]] + c0 / a1 + c2 / a1;
							avcarr[mergedtris[i + 2]] = avcarr[mergedtris[i + 2]] + c0 / a2 + c1 / a2;
						}

						//for (int i = 0; i < mergedtris.Length; i += 3)
						//{
						//    if (mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0) continue;
						//    if (avcarr[mergedtris[i]] != avcarr[triangles[i]]) avcarr[triangles[i]] = avcarr[mergedtris[i]];
						//    if (avcarr[mergedtris[i + 1]] != avcarr[triangles[i + 1]]) avcarr[triangles[i + 1]] = avcarr[mergedtris[i + 1]];
						//    if (avcarr[mergedtris[i + 2]] != avcarr[triangles[i + 2]]) avcarr[triangles[i + 2]] = avcarr[mergedtris[i + 2]];
						//}
						for(var i = 0; i < mergedtris.Length; i += 3)
						{
							if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
							{
								continue;
							}
							if(avcarr[mergedtris[i]] != avcarr[tris[i]])
							{
								avcarr[tris[i]] = avcarr[mergedtris[i]];
							}
							if(avcarr[mergedtris[i + 1]] != avcarr[tris[i + 1]])
							{
								avcarr[tris[i + 1]] = avcarr[mergedtris[i + 1]];
							}
							if(avcarr[mergedtris[i + 2]] != avcarr[tris[i + 2]])
							{
								avcarr[tris[i + 2]] = avcarr[mergedtris[i + 2]];
							}
						}
						for(var i = 0; i < vertices.Length; i++)
						{
							if(smode != SymmetryMode.None)
							{
								var ihitpos = GetSymmetry(hitpos);

								//float idist = GetStroke(vertices[i], ihitpos);
								//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
								var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
								if(idist <= BrushRadFix && maskv2[i].x >= maskWeight)
								{
									colors[i] = Color.Lerp(colors[i], avcarr[i], BrushStrength * (1 - idist / BrushRadFix));
									IsBrushedArray[i] = true;
								}
							}

							//float dist = GetStroke(vertices[i], hitpos);
							//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
							var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
							if(dist > BrushRadFix)
							{
								continue;
							}
							if(maskv2[i].x < maskWeight)
							{
								continue;
							}
							colors[i] = Color.Lerp(colors[i], avcarr[i], BrushStrength * (1 - dist / BrushRadFix));
							IsBrushedArray[i] = true;
						}
					}
					break;

				case "DrawMask":
				case "EraseMask":
					if(!Event.current.shift)
					{
						if(IsLazy)
						{
							DoBrushHitPosList(BrushRadFix);
						}
						else
						{
							DoDrawMask(thitp, thitp, ithitp, ithitp, BrushRadFix, BrushString == "EraseMask");
							return;
						}
					}
					else
					{
						var adjmarr = new int[vertices.Length];
						var avfarr = new float[vertices.Length];
						try
						{
							for(var i = 0; i < mergedtris.Length; i += 3)
							{
								if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
								{
									continue;
								}
								adjmarr[mergedtris[i]] += 2;
								adjmarr[mergedtris[i + 1]] += 2;
								adjmarr[mergedtris[i + 2]] += 2;
							}
						}
						catch
						{
							MergetriGenerate(currentMesh);
							for(var i = 0; i < mergedtris.Length; i += 3)
							{
								if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
								{
									continue;
								}
								adjmarr[mergedtris[i]] += 2;
								adjmarr[mergedtris[i + 1]] += 2;
								adjmarr[mergedtris[i + 2]] += 2;
							}
							if(DebugMode)
							{
								Debug.Log("Mesh get error.EditorSculpt fix that automatically");
							}
						}
						for(var i = 0; i < mergedtris.Length; i += 3)
						{
							if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
							{
								continue;
							}

							//float f0 = masks[triangles[i]];
							//float f1 = masks[triangles[i + 1]];
							//float f2 = masks[triangles[i + 2]];
							var f0 = maskv2[mergedtris[i]].x;
							var f1 = maskv2[mergedtris[i + 1]].x;
							var f2 = maskv2[mergedtris[i + 2]].x;

							var a0 = adjmarr[mergedtris[i]] == 0 ? 1 : adjmarr[mergedtris[i]];
							var a1 = adjmarr[mergedtris[i + 1]] == 0 ? 1 : adjmarr[mergedtris[i + 1]];
							var a2 = adjmarr[mergedtris[i + 2]] == 0 ? 1 : adjmarr[mergedtris[i + 2]];

							avfarr[mergedtris[i]] = avfarr[mergedtris[i]] + f1 / a0 + f2 / a0;
							avfarr[mergedtris[i + 1]] = avfarr[mergedtris[i + 1]] + f0 / a1 + f2 / a1;
							avfarr[mergedtris[i + 2]] = avfarr[mergedtris[i + 2]] + f0 / a2 + f1 / a2;
						}

						//for (int i = 0; i < mergedtris.Length; i += 3)
						//{
						//    if (mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0) continue;
						//    if (avfarr[mergedtris[i]] != avfarr[triangles[i]]) avfarr[triangles[i]] = avfarr[mergedtris[i]];
						//    if (avfarr[mergedtris[i + 1]] != avfarr[triangles[i + 1]]) avfarr[triangles[i + 1]] = avfarr[mergedtris[i + 1]];
						//    if (avfarr[mergedtris[i + 2]] != avfarr[triangles[i + 2]]) avfarr[triangles[i + 2]] = avfarr[mergedtris[i + 2]];
						//}
						for(var i = 0; i < mergedtris.Length; i += 3)
						{
							if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
							{
								continue;
							}
							if(avfarr[mergedtris[i]] != avfarr[tris[i]])
							{
								avfarr[tris[i]] = avfarr[mergedtris[i]];
							}
							if(avfarr[mergedtris[i + 1]] != avfarr[tris[i + 1]])
							{
								avfarr[tris[i + 1]] = avfarr[mergedtris[i + 1]];
							}
							if(avfarr[mergedtris[i + 2]] != avfarr[tris[i + 2]])
							{
								avfarr[tris[i + 2]] = avfarr[mergedtris[i + 2]];
							}
						}
						for(var i = 0; i < vertices.Length; i++)
						{
							if(smode != SymmetryMode.None)
							{
								var ihitpos = GetSymmetry(hitpos);

								//float idist = GetStroke(vertices[i], ihitpos);
								//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
								var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);

								//if(idist<=BrushRadFix && MaskArray[i].r>=maskWeight)
								if(idist <= BrushRadFix)
								{
									//masks[i] = Mathf.Lerp(masks[i], avfarr[i], BrushStrength * (1 - (idist / BrushRadFix)));
									var imaskf = Mathf.Lerp(maskv2[i].x, avfarr[i], BrushStrength * (1 - idist / BrushRadFix));

									//maskv2[i].x = masks[i];
									maskv2[i].x = imaskf;
									IsBrushedArray[i] = true;
								}
							}

							//float dist = GetStroke(vertices[i], hitpos);
							//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
							var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
							if(dist > BrushRadFix)
							{
								continue;
							}

							//masks[i] = Mathf.Lerp(masks[i], avfarr[i], BrushStrength * (1 - (dist / BrushRadFix)));
							var maskf = Mathf.Lerp(maskv2[i].x, avfarr[i], BrushStrength * (1 - dist / BrushRadFix));

							//maskv2[i].x = masks[i];
							maskv2[i].x = maskf;
							IsBrushedArray[i] = true;
						}
					}
					break;

				case "VertexWeight":
				case "EraseWeight":
					if(!Event.current.shift)
					{
						if(IsLazy)
						{
							DoBrushHitPosList(BrushRadFix);
						}
						else
						{
							DoVertexWeight(thitp, thitp, ithitp, ithitp, BrushRadFix, BrushString == "EraseWeight");
							return;
						}
					}
					else
					{
						var adjmarr = new int[vertices.Length];
						var avfarr = new float[vertices.Length];
						try
						{
							for(var i = 0; i < mergedtris.Length; i += 3)
							{
								if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
								{
									continue;
								}
								adjmarr[mergedtris[i]] += 2;
								adjmarr[mergedtris[i + 1]] += 2;
								adjmarr[mergedtris[i + 2]] += 2;
							}
						}
						catch
						{
							MergetriGenerate(currentMesh);
							for(var i = 0; i < mergedtris.Length; i += 3)
							{
								if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
								{
									continue;
								}
								adjmarr[mergedtris[i]] += 2;
								adjmarr[mergedtris[i + 1]] += 2;
								adjmarr[mergedtris[i + 2]] += 2;
							}
							if(DebugMode)
							{
								Debug.Log("Mesh get error.EditorSculpt fix that automatically");
							}
						}
						for(var i = 0; i < mergedtris.Length; i += 3)
						{
							var f0 = maskv2[mergedtris[i]].y;
							var f1 = maskv2[mergedtris[i + 1]].y;
							var f2 = maskv2[mergedtris[i + 2]].y;

							var a0 = adjmarr[mergedtris[i]] == 0 ? 1 : adjmarr[mergedtris[i]];
							var a1 = adjmarr[mergedtris[i + 1]] == 0 ? 1 : adjmarr[mergedtris[i + 1]];
							var a2 = adjmarr[mergedtris[i + 2]] == 0 ? 1 : adjmarr[mergedtris[i + 2]];

							avfarr[mergedtris[i]] = avfarr[mergedtris[i]] + f1 / a0 + f2 / a0;
							avfarr[mergedtris[i + 1]] = avfarr[mergedtris[i + 1]] + f0 / a1 + f2 / a1;
							avfarr[mergedtris[i + 2]] = avfarr[mergedtris[i + 2]] + f0 / a2 + f1 / a2;
						}

						//for (int i = 0; i < mergedtris.Length; i += 3)
						//{
						//    if (mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0) continue;
						//    if (avfarr[mergedtris[i]] != avfarr[triangles[i]]) avfarr[triangles[i]] = avfarr[mergedtris[i]];
						//    if (avfarr[mergedtris[i + 1]] != avfarr[triangles[i + 1]]) avfarr[triangles[i + 1]] = avfarr[mergedtris[i + 1]];
						//    if (avfarr[mergedtris[i + 2]] != avfarr[triangles[i + 2]]) avfarr[triangles[i + 2]] = avfarr[mergedtris[i + 2]];
						//}
						for(var i = 0; i < mergedtris.Length; i += 3)
						{
							if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
							{
								continue;
							}
							if(avfarr[mergedtris[i]] != avfarr[tris[i]])
							{
								avfarr[tris[i]] = avfarr[mergedtris[i]];
							}
							if(avfarr[mergedtris[i + 1]] != avfarr[tris[i + 1]])
							{
								avfarr[tris[i + 1]] = avfarr[mergedtris[i + 1]];
							}
							if(avfarr[mergedtris[i + 2]] != avfarr[tris[i + 2]])
							{
								avfarr[tris[i + 2]] = avfarr[mergedtris[i + 2]];
							}
						}
						for(var i = 0; i < vertices.Length; i++)
						{
							if(smode != SymmetryMode.None)
							{
								var ihitpos = GetSymmetry(hitpos);

								//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
								var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
								if(idist <= BrushRadFix)
								{
									var iweightf = Mathf.Lerp(maskv2[i].y, avfarr[i], BrushStrength * (1 - idist / BrushRadFix));
									maskv2[i].y = iweightf;
									IsBrushedArray[i] = true;
								}
							}

							//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
							var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
							if(dist > BrushRadFix)
							{
								continue;
							}
							var weightf = Mathf.Lerp(maskv2[i].y, avfarr[i], BrushStrength * (1 - dist / BrushRadFix));
							maskv2[i].y = weightf;
							IsBrushedArray[i] = true;
						}
					}
					break;

				case "ReducePoly":
					for(var i = 0; i < vertices.Length; i++)
					{
						if(smode != SymmetryMode.None)
						{
							var ihitpos = GetSymmetry(hitpos);

							//float idist = GetStroke(vertices[i], ihitpos);
							//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
							var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
							if(idist <= BrushRadFix)
							{
								IsBrushedArray[i] = true;
							}
						}

						//float dist = GetStroke(vertices[i], hitpos);
						//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
						var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
						if(dist > BrushRadFix)
						{
							continue;
						}
						IsBrushedArray[i] = true;
					}
					break;

				case "IncreasePoly":
					for(var i = 0; i < vertices.Length; i++)
					{
						if(smode != SymmetryMode.None)
						{
							var ihitpos = GetSymmetry(hitpos);

							//float idist = GetStroke(vertices[i], ihitpos);
							//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
							var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
							if(idist <= BrushRadFix)
							{
								IsBrushedArray[i] = true;
							}
						}

						//float dist = GetStroke(vertices[i], hitpos);
						//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
						var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
						if(dist > BrushRadFix)
						{
							continue;
						}
						IsBrushedArray[i] = true;
					}
					break;

				/*case BrushType.BETA_Repair:
				    for(int i=0;i<vertices.Length;i++)
				    {
				        if(smode!=SymmetryMode.None)
				        {
				            Vector3 ihitpos = GetSymmetry(hitpos);
				            float idist = GetStroke(vertices[i],ihitpos);
				            if(idist<=BrushRadFix)
				            {
				                IsBrushedArray[i] = true;
				            }
				        }
				        float dist = GetStroke(vertices[i],hitpos);
				        if(dist>BrushRadFix)continue;
				        IsBrushedArray[i] = true;
				    }
				    break;*/

				case "BETA_Repair":

					for(var i = 0; i < vertices.Length; i++)
					{
						if(smode != SymmetryMode.None)
						{
							var ihitpos = GetSymmetry(hitpos);

							//float idist = GetStroke(vertices[i], ihitpos);
							//float idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(ihitpos));
							var idist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
							if(idist <= BrushRadFix)
							{
								IsBrushedArray[i] = true;
							}
						}

						//float dist = GetStroke(vertices[i], hitpos);
						//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), currentObject.transform.TransformPoint(hitpos));
						var dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
						if(dist > BrushRadFix)
						{
							continue;
						}
						IsBrushedArray[i] = true;
					}
					break;
				case "BETA_Decal":
					if(DecalString != "Create New Decal")
					{
						if(IsLazy)
						{
							DoBrushHitPosList(BrushRadFix);
						}
						else
						{
							DoDecal(thitp, thitp, ithitp, ithitp, BrushRadFix);
							return;
						}
					}
					break;
				case "TexturePaint":
					if(IsLazy)
					{
						DoBrushHitPosList(BrushRadFix);
					}
					else
					{
						DoTexturePaint(thitp, thitp, ithitp, ithitp, BrushRadFix);
						return;
					}
					break;
				case "BETA_Texture":
					if(IsLazy)
					{
						DoBrushHitPosList(BrushRadFix);
					}
					else
					{
						DoBETA_Texture(thitp, thitp, ithitp, ithitp, BrushHitInt, BrushHitInt, BrushRadFix);
						return;
					}
					break;
			}

			//Changed 2019/03/20
			currentMesh.vertices = vertices;
			currentMesh.SetTriangles(triangles, 0);

			//End Changed 2019/03/20
			/*for (int i = 0; i < matint; i++)
			{
			    currentMesh.SetTriangles(triangles, i);
			}*/
			//Changed 2019/03/22
			//currentMesh.triangles = triangles;
			//End Changed 2019/03/22
			currentMesh.uv4 = maskv2;
			currentMesh.colors = colors;
			IsMeshSaved = false;
		}

		private static void DoTexturePaint(Vector3 thitp0, Vector3 thitp1, Vector3 ithitp0, Vector3 ithitp1, float BrushRadFix)
		{
			if(!IsInStroke)
			{
				return;
			}
			if(!uvtexture)
			{
				uvtexture = new Texture2D(texwidth, texheight);
			}
			var uvwidth = uvtexture.width;
			var uvheight = uvtexture.height;
			var ChkArrArr = Enumerable.Repeat(0, uvwidth).Select(n => Enumerable.Repeat(0, uvheight).ToArray()).ToArray();
			var DoSmoothStroke = isSmoothStroke && thitp0 != thitp1 ? true : false;
			int i, j;
			Color cx0 = Color.white, cx1 = Color.white;
			Vector3 px0;
			float dist0, idist0;
			var brushstack = new Stack<Vector2Int>();
			var isDraw = false;
			for(i = 0; i < uvwidth; i += 8)
			{
				for(j = 0; j < uvheight; j += 8)
				{
					px0 = uvpos[i][j];
					dist0 = DoSmoothStroke ? HandleUtility.DistancePointLine(px0, thitp0, thitp1) : GetStroke(px0, thitp0);
					if(dist0 <= BrushRadFix)
					{
						brushstack.Push(new Vector2Int(i, j));
					}
					if(smode != SymmetryMode.None)
					{
						idist0 = DoSmoothStroke ? HandleUtility.DistancePointLine(px0, ithitp0, ithitp1) : GetStroke(px0, ithitp0);
						if(idist0 <= BrushRadFix)
						{
							brushstack.Push(new Vector2Int(i, j));
						}
					}
				}
			}
			if(brushstack.Count < 1)
			{
				for(i = 0; i < uvwidth; i++)
				{
					for(j = 0; j < uvheight; j++)
					{
						px0 = uvpos[i][j];
						dist0 = DoSmoothStroke ? HandleUtility.DistancePointLine(px0, thitp0, thitp1) : GetStroke(px0, thitp0);
						if(dist0 <= BrushRadFix)
						{
							brushstack.Push(new Vector2Int(i, j));
						}
						if(smode != SymmetryMode.None)
						{
							idist0 = DoSmoothStroke ? HandleUtility.DistancePointLine(px0, ithitp0, ithitp1) : GetStroke(px0, ithitp0);
							if(idist0 <= BrushRadFix)
							{
								brushstack.Push(new Vector2Int(i, j));
							}
						}
					}
				}
			}

			try
			{
				// if (uvindex[hituvi.x][hituvi.y] == uvindex[hituvi2.x][hituvi2.y])
				//{
				var bstack = Vector2Int.zero;
				int bx, by;
				ChkArrArr = Enumerable.Repeat(0, uvwidth).Select(n => Enumerable.Repeat(0, uvheight).ToArray()).ToArray();

				for(;;)
				{
					//if (brushstack.TryPop(out bstack) == false) break;
					try
					{
						bstack = brushstack.Pop();
					}
					catch
					{
						break;
					}
					bx = bstack.x;
					by = bstack.y;
					if(bx < 0 || by < 0 || bx >= uvwidth || by >= uvheight)
					{
						continue;
					}
					if(ChkArrArr[bx][by] != 0)
					{
						continue;
					}
					ChkArrArr[bx][by] = 1;
					px0 = uvpos[bx][by];

					dist0 = DoSmoothStroke ? HandleUtility.DistancePointLine(px0, thitp0, thitp1) : GetStroke(px0, thitp0);
					cx0 = uvtexture.GetPixel(bx, by);
					isDraw = false;
					if(dist0 <= BrushRadFix)
					{
						isDraw = true;
						cx1 = Color.Lerp(cx0, BrushColor, BrushStrength * (1 - dist0 / BrushRadFix));
						uvtexture.SetPixel(bx, by, cx1);
						IsTexSaved = false;

						//uvtexture.SetPixel(bstack.x, bstack.y, cx1);

						if(bx + 1 < uvwidth)
						{
							if(ChkArrArr[bx + 1][by] == 0)
							{
								brushstack.Push(new Vector2Int(bx + 1, by));
							}
						}
						if(by + 1 < uvheight)
						{
							if(ChkArrArr[bx][by + 1] == 0)
							{
								brushstack.Push(new Vector2Int(bx, by + 1));
							}
						}
						if(bx + 1 < uvwidth && by + 1 < uvheight)
						{
							if(ChkArrArr[bx + 1][by + 1] == 0)
							{
								brushstack.Push(new Vector2Int(bx + 1, by + 1));
							}
						}
						if(bx - 1 >= 0)
						{
							if(ChkArrArr[bx - 1][by] == 0)
							{
								brushstack.Push(new Vector2Int(bx - 1, by));
							}
						}
						if(by - 1 >= 0)
						{
							if(ChkArrArr[bx][by - 1] == 0)
							{
								brushstack.Push(new Vector2Int(bx, by - 1));
							}
						}
						if(bx - 1 >= 0 && by - 1 >= 0)
						{
							if(ChkArrArr[bx - 1][by - 1] == 0)
							{
								brushstack.Push(new Vector2Int(bx - 1, by - 1));
							}
						}
						if(bx + 1 < uvwidth && by - 1 >= 0)
						{
							if(ChkArrArr[bx + 1][by - 1] == 0)
							{
								brushstack.Push(new Vector2Int(bx + 1, by - 1));
							}
						}
						if(bx - 1 >= 0 && by + 1 < uvheight)
						{
							if(ChkArrArr[bx - 1][by + 1] == 0)
							{
								brushstack.Push(new Vector2Int(bx - 1, by + 1));
							}
						}
					}
					if(smode != SymmetryMode.None)
					{
						idist0 = DoSmoothStroke ? HandleUtility.DistancePointLine(px0, ithitp0, ithitp1) : GetStroke(px0, ithitp0);
						if(idist0 <= BrushRadFix)
						{
							cx1 = Color.Lerp(cx0, BrushColor, BrushStrength * (1 - idist0 / BrushRadFix));
							uvtexture.SetPixel(bx, by, cx1);
							IsTexSaved = false;
							if(!isDraw)
							{
								if(bx + 1 < uvwidth)
								{
									if(ChkArrArr[bx + 1][by] == 0)
									{
										brushstack.Push(new Vector2Int(bx + 1, by));
									}
								}
								if(by + 1 < uvheight)
								{
									if(ChkArrArr[bx][by + 1] == 0)
									{
										brushstack.Push(new Vector2Int(bx, by + 1));
									}
								}
								if(bx + 1 < uvwidth && by + 1 < uvheight)
								{
									if(ChkArrArr[bx + 1][by + 1] == 0)
									{
										brushstack.Push(new Vector2Int(bx + 1, by + 1));
									}
								}
								if(bx - 1 >= 0)
								{
									if(ChkArrArr[bx - 1][by] == 0)
									{
										brushstack.Push(new Vector2Int(bx - 1, by));
									}
								}
								if(by - 1 >= 0)
								{
									if(ChkArrArr[bx][by - 1] == 0)
									{
										brushstack.Push(new Vector2Int(bx, by - 1));
									}
								}
								if(bx - 1 >= 0 && by - 1 >= 0)
								{
									if(ChkArrArr[bx - 1][by - 1] == 0)
									{
										brushstack.Push(new Vector2Int(bx - 1, by - 1));
									}
								}
								if(bx + 1 < uvwidth && by - 1 >= 0)
								{
									if(ChkArrArr[bx + 1][by - 1] == 0)
									{
										brushstack.Push(new Vector2Int(bx + 1, by - 1));
									}
								}
								if(bx - 1 >= 0 && by + 1 < uvheight)
								{
									if(ChkArrArr[bx - 1][by + 1] == 0)
									{
										brushstack.Push(new Vector2Int(bx - 1, by + 1));
									}
								}
							}
						}
					}
				}
				uvtexture.Apply();

				//}
			}
			catch
			{
			}
		}

		private static void SetupUVIndex()
		{
			if(IsSetupUV)
			{
				return;
			}
			IsSetupUV = true;

			var uvs = currentMesh.uv;
			var vertices = currentMesh.vertices;
			int[] triangles = null;
			if(paintmatidx < currentMesh.subMeshCount)
			{
				triangles = currentMesh.GetTriangles(paintmatidx);
			}
			else
			{
				triangles = currentMesh.triangles;
			}
			var uvwidth = uvtexture.width;
			var uvheight = uvtexture.height;
			uvSameIdxs = Enumerable.Repeat(0, triangles.Length).ToArray();
			var idxHashArrArr = Enumerable.Repeat(new HashSet<int>(), uvwidth).Select(n => Enumerable.Repeat(new HashSet<int>(), uvheight).ToArray()).ToArray();
			for(var i = 0; i < triangles.Length; i += 3)
			{
				var t0 = triangles[i];
				var t1 = triangles[i + 1];
				var t2 = triangles[i + 2];
				var v0 = vertices[t0];
				var v1 = vertices[t1];
				var v2 = vertices[t2];

				var uv0 = uvs[t0];
				uv0.x *= uvtexture.width;
				uv0.y *= uvtexture.height;
				var uv1 = uvs[t1];
				uv1.x *= uvtexture.width;
				uv1.y *= uvtexture.height;
				var uv2 = uvs[t2];
				uv2.x *= uvtexture.width;
				uv2.y *= uvtexture.height;
				var uxmin = (int)Mathf.Min(uv0.x, uv1.x, uv2.x);
				var uxmax = (int)Mathf.Max(uv0.x, uv1.x, uv2.x);
				var uymin = (int)Mathf.Min(uv0.y, uv1.y, uv2.y);
				var uymax = (int)Mathf.Max(uv0.y, uv1.y, uv2.y);
				if(uxmax >= uvwidth + 1 || uymax >= uvheight + 1)
				{
					continue;
				}
				var uvc = (uv0 + uv1 + uv2) / 3;
				var tu0 = (uv0.y - uv1.y) / (uv0.x - uv1.x);
				var tu1 = (uv1.y - uv2.y) / (uv1.x - uv2.x);
				var tu2 = (uv0.y - uv2.y) / (uv0.x - uv2.x);
				bool uflag0 = false, uflag1 = false, uflag2 = false;
				if(uvc.y > (uvc.x - uv1.x) * tu0 + uv1.y)
				{
					uflag0 = true;
				}
				if(uvc.y > (uvc.x - uv2.x) * tu1 + uv2.y)
				{
					uflag1 = true;
				}
				if(uvc.y > (uvc.x - uv2.x) * tu2 + uv2.y)
				{
					uflag2 = true;
				}
				for(var j = uymin; j < uymax + 1; j++)
				{
					for(var k = uxmin; k < uxmax + 1; k++)
					{
						var up0 = new Vector2(k, j);

						if(uflag0 && up0.y < (up0.x - uv1.x) * tu0 + uv1.y)
						{
							continue;
						}
						if(!uflag0 && up0.y > (up0.x - uv1.x) * tu0 + uv1.y)
						{
							continue;
						}
						if(uflag1 && up0.y < (up0.x - uv2.x) * tu1 + uv2.y)
						{
							continue;
						}
						if(!uflag1 && up0.y > (up0.x - uv2.x) * tu1 + uv2.y)
						{
							continue;
						}
						if(uflag2 && up0.y < (up0.x - uv2.x) * tu2 + uv2.y)
						{
							continue;
						}
						if(!uflag2 && up0.y > (up0.x - uv2.x) * tu2 + uv2.y)
						{
							continue;
						}

						Vector2 uc0, uc1, u2u1, c1c0, c0u1, ua0, c0c1, u0u1, c1u1, ub0;
						float ta0, tb0, tc0;
						Vector3 va, vb;
						var p0 = Vector3.zero;
						uc0 = up0 + (uv2 - uv0);
						uc1 = up0 + (uv0 - uv2);
						u2u1 = uv2 - uv1;
						c1c0 = uc1 - uc0;
						c0u1 = uc0 - uv1;
						var fc0 = c1c0.x * c0u1.y - c1c0.y * c0u1.x;
						var fc1 = c1c0.x * u2u1.y - c1c0.y * u2u1.x;
						ua0 = uv1 + u2u1 * fc0 / fc1;
						u0u1 = uv0 - uv1;
						c0c1 = uc0 - uc1;
						c1u1 = uc1 - uv1;
						var fc2 = c0c1.x * c1u1.y - c0c1.y * c1u1.x;
						var fc3 = c0c1.x * u0u1.y - c0c1.y * u0u1.x;
						ub0 = uv1 + u0u1 * fc2 / fc3;
						if(fc0 == 0.0f || fc2 == 0.0f)
						{
							continue;
						}
						ta0 = Vector2.Distance(uv1, ua0) / Vector2.Distance(uv1, uv2);
						tb0 = Vector2.Distance(uv1, ub0) / Vector2.Distance(uv1, uv0);
						tc0 = Vector2.Distance(ua0, up0) / Vector2.Distance(ua0, ub0);
						va = v1 + ta0 * (v2 - v1);
						vb = v1 + tb0 * (v0 - v1);
						p0 = va + tc0 * (vb - va);
						p0 = currentObject.transform.TransformPoint(p0);

						if(uvindex[k][j] < 0)
						{
							uvindex[k][j] = i;
							uvpos[k][j] = p0;
							uvSameIdxs[i] = i;
						}
						else
						{
							uvSameIdxs[i] = uvindex[k][j];
						}

						//HashSet<int> idxhash = idxHashArrArr[k][j];
						//idxhash.Add(i);
						//idxHashArrArr[k][j] = idxhash;
					}
				}
			}
		}

		private static void SetupUVIndex2()
		{
			if(IsSetupUV)
			{
				return;
			}
			IsSetupUV = true;
			if(uvtexture.width < 1 || uvtexture.height < 1)
			{
				return;
			}
			var uvs = currentMesh.uv;
			var vertices = currentMesh.vertices;
			var triangles = currentMesh.GetTriangles(paintmatidx);
			var uvwidth = uvtexture.width;
			var uvheight = uvtexture.height;
			uvSameIdxs = Enumerable.Repeat(0, triangles.Length).ToArray();
			uvSameIdxArrArr = Enumerable.Repeat(Vector2Int.zero, uvwidth + 1).Select(n => Enumerable.Repeat(Vector2Int.zero, uvheight + 1).ToArray()).ToArray();
			var vposUVDict = new Dictionary<Vector3, Vector2Int>();
			var adjIdxDict = new Dictionary<Vector3, int>();
			var triIdxDict = new Dictionary<Vector3, int>();
			Vector3 v0, v1, v2, v3, e0, e1, e2, vqa0, vqa1, vqa2;
			int t0, t1, t2, t3, t4, t5, idx0, at0, av0, av1, av2;
			var vcnt = currentMesh.vertexCount;
			for(var i = 0; i < triangles.Length; i += 3)
			{
				t0 = triangles[i];
				t1 = triangles[i + 1];
				t2 = triangles[i + 2];
				v0 = vertices[t0];
				v1 = vertices[t1];
				v2 = vertices[t2];
				e0 = v0 * 0.5f + v1 * 0.5f;
				e1 = v1 * 0.5f + v2 * 0.5f;
				e2 = v2 * 0.5f + v0 * 0.5f;
				if(adjIdxDict.TryGetValue(e0, out idx0))
				{
					//try
					//{
					//    if(vertices[idx0]!=vertices[t2])
					//    {
					//        adjIdxDict[e0] = idx0 + t2;
					//    }
					//}
					//catch { }
					//if(idx0 != t2)
					//{
					//    adjIdxDict[e0] = idx0 + t2;
					//}
				}
				else
				{
					adjIdxDict[e0] = t2;
				}
				if(adjIdxDict.TryGetValue(e1, out idx0))
				{
					//try
					//{
					//    if(vertices[idx0]!=vertices[t0])
					//    {
					//        adjIdxDict[e1] = idx0 + t0;
					//    }
					//}
					//catch { }
					//if(idx0 != t0)
					//{
					//    adjIdxDict[e1] = idx0 + t0;
					//}
				}
				else
				{
					adjIdxDict[e1] = t0;
				}
				if(adjIdxDict.TryGetValue(e2, out idx0))
				{
					//try
					//{
					//    if(vertices[idx0]!=vertices[t1])
					//    {
					//        adjIdxDict[e2] = idx0 + t1;
					//    }
					//}
					//catch { }
					//if(idx0!=t1)
					//{
					//    adjIdxDict[e2] = idx0 + t1;
					//}
				}
				else
				{
					adjIdxDict[e2] = t1;
				}
			}
			for(var i = 0; i < triangles.Length; i += 3)
			{
				t0 = triangles[i];
				t1 = triangles[i + 1];
				t2 = triangles[i + 2];
				v0 = vertices[t0];
				v1 = vertices[t1];
				v2 = vertices[t2];
				triIdxDict[v0 * 0.333f + v1 * 0.333f + v2 * 0.333f] = i;
			}

			// /* Debug only value */ 
			//int setupcnt = 0, tricnt = 0;
			for(var i = 0; i < triangles.Length; i += 3)
			{
				t0 = triangles[i];
				t1 = triangles[i + 1];
				t2 = triangles[i + 2];
				v0 = vertices[t0];
				v1 = vertices[t1];
				v2 = vertices[t2];
				e0 = v0 * 0.5f + v1 * 0.5f;
				e1 = v1 * 0.5f + v2 * 0.5f;
				e2 = v2 * 0.5f + v0 * 0.5f;

				var uv0 = uvs[t0];
				uv0.x *= uvtexture.width;
				uv0.y *= uvtexture.height;
				var uv1 = uvs[t1];
				uv1.x *= uvtexture.width;
				uv1.y *= uvtexture.height;
				var uv2 = uvs[t2];
				uv2.x *= uvtexture.width;
				uv2.y *= uvtexture.height;
				var uxmin = (int)Mathf.Min(uv0.x, uv1.x, uv2.x);
				var uxmax = (int)Mathf.Max(uv0.x, uv1.x, uv2.x);
				var uymin = (int)Mathf.Min(uv0.y, uv1.y, uv2.y);
				var uymax = (int)Mathf.Max(uv0.y, uv1.y, uv2.y);

				t3 = t4 = t5 = -1;
				if(adjIdxDict.TryGetValue(e0, out idx0))
				{
					//t3 = idx0 - t0;
					if(idx0 != t0)
					{
						t3 = idx0;
					}
				}
				if(adjIdxDict.TryGetValue(e1, out idx0))
				{
					//t4 = idx0 - t1;
					if(idx0 != t1)
					{
						t4 = idx0;
					}
				}
				if(adjIdxDict.TryGetValue(e2, out idx0))
				{
					//t5 = idx0 - t2;
					if(idx0 != t3)
					{
						t5 = idx0;
					}
				}

				//if (t3 >= 0 && t3 < vcnt && t4 >= 0 && t4 < vcnt && t5 >= 0 && t5 < vcnt)
				if(t3 >= 0 && t3 < vcnt)
				{
					v3 = vertices[t3];

					//v4 = vertices[t4];
					//v5 = vertices[t5];

					av0 = -1;
					av1 = -1;
					av2 = -1;
					at0 = -1;
					if(triIdxDict.TryGetValue(v0 * 0.333f + v1 * 0.333f + v3 * 0.333f, out at0))
					{
						//tricnt++;
						if(at0 >= 0 && at0 != i)
						{
							vqa0 = vertices[triangles[at0]];
							vqa1 = vertices[triangles[at0 + 1]];
							vqa2 = vertices[triangles[at0 + 2]];
							if(vqa0 == v0)
							{
								if(triangles[at0] != t0)
								{
									av0 = triangles[at0];
								}
							}
							else if(vqa0 == v1)
							{
								if(triangles[at0] != t1)
								{
									av1 = triangles[at0];
								}
							}
							else if(vqa0 == v2)
							{
								if(triangles[at0] != t2)
								{
									av2 = triangles[at0];
								}
							}

							if(vqa1 == v0)
							{
								if(triangles[at0 + 1] != t0 && av0 < 0)
								{
									av0 = triangles[at0 + 1];
								}
							}
							else if(vqa1 == v1)
							{
								if(triangles[at0 + 1] != t1 && av1 < 0)
								{
									av1 = triangles[at0 + 1];
								}
							}
							else if(vqa1 == v2)
							{
								if(triangles[at0 + 1] != t2 && av2 < 0)
								{
									av2 = triangles[at0 + 1];
								}
							}

							if(vqa2 == v0)
							{
								if(triangles[at0 + 2] != t0 && av0 < 0)
								{
									av0 = triangles[at0 + 2];
								}
							}
							else if(vqa2 == v1)
							{
								if(triangles[at0 + 2] != t1 && av1 < 0)
								{
									av1 = triangles[at0 + 2];
								}
							}
							else if(vqa2 == v2)
							{
								if(triangles[at0 + 2] != t2 && av2 < 0)
								{
									av2 = triangles[at0 + 2];
								}
							}
							if(av0 >= 0 && av1 >= 0 && av2 < 0)
							{
								SetupSameUVIdx(uvs[av0], uvs[av1], uvs[t0], uvs[t1]);

								//setupcnt++;
							}
							else if(av0 < 0 && av1 >= 0 && av2 >= 0)
							{
								SetupSameUVIdx(uvs[av1], uvs[av2], uvs[t1], uvs[t2]);

								//setupcnt++;
							}
							else if(av0 >= 0 && av1 < 0 && av2 >= 0)
							{
								SetupSameUVIdx(uvs[av2], uvs[av0], uvs[t2], uvs[t0]);

								//setupcnt++;
							}
						}
					}
				}

				if(uxmax >= uvwidth + 1 || uymax >= uvheight + 1)
				{
					continue;
				}
				var uvc = (uv0 + uv1 + uv2) / 3;
				var tu0 = (uv0.y - uv1.y) / (uv0.x - uv1.x);
				var tu1 = (uv1.y - uv2.y) / (uv1.x - uv2.x);
				var tu2 = (uv0.y - uv2.y) / (uv0.x - uv2.x);
				bool uflag0 = false, uflag1 = false, uflag2 = false;
				if(uvc.y > (uvc.x - uv1.x) * tu0 + uv1.y)
				{
					uflag0 = true;
				}
				if(uvc.y > (uvc.x - uv2.x) * tu1 + uv2.y)
				{
					uflag1 = true;
				}
				if(uvc.y > (uvc.x - uv2.x) * tu2 + uv2.y)
				{
					uflag2 = true;
				}
				for(var j = uymin; j < uymax + 1; j++)
				{
					for(var k = uxmin; k < uxmax + 1; k++)
					{
						var up0 = new Vector2(k, j);

						if(uflag0 && up0.y < (up0.x - uv1.x) * tu0 + uv1.y)
						{
							continue;
						}
						if(!uflag0 && up0.y > (up0.x - uv1.x) * tu0 + uv1.y)
						{
							continue;
						}
						if(uflag1 && up0.y < (up0.x - uv2.x) * tu1 + uv2.y)
						{
							continue;
						}
						if(!uflag1 && up0.y > (up0.x - uv2.x) * tu1 + uv2.y)
						{
							continue;
						}
						if(uflag2 && up0.y < (up0.x - uv2.x) * tu2 + uv2.y)
						{
							continue;
						}
						if(!uflag2 && up0.y > (up0.x - uv2.x) * tu2 + uv2.y)
						{
							continue;
						}

						Vector2 uc0, uc1, u2u1, c1c0, c0u1, ua0, c0c1, u0u1, c1u1, ub0;
						float ta0, tb0, tc0;
						Vector3 va, vb;
						var p0 = Vector3.zero;
						uc0 = up0 + (uv2 - uv0);
						uc1 = up0 + (uv0 - uv2);
						u2u1 = uv2 - uv1;
						c1c0 = uc1 - uc0;
						c0u1 = uc0 - uv1;
						var fc0 = c1c0.x * c0u1.y - c1c0.y * c0u1.x;
						var fc1 = c1c0.x * u2u1.y - c1c0.y * u2u1.x;
						ua0 = uv1 + u2u1 * fc0 / fc1;
						u0u1 = uv0 - uv1;
						c0c1 = uc0 - uc1;
						c1u1 = uc1 - uv1;
						var fc2 = c0c1.x * c1u1.y - c0c1.y * c1u1.x;
						var fc3 = c0c1.x * u0u1.y - c0c1.y * u0u1.x;
						ub0 = uv1 + u0u1 * fc2 / fc3;
						if(fc0 == 0.0f || fc2 == 0.0f)
						{
							continue;
						}
						ta0 = Vector2.Distance(uv1, ua0) / Vector2.Distance(uv1, uv2);
						tb0 = Vector2.Distance(uv1, ub0) / Vector2.Distance(uv1, uv0);
						tc0 = Vector2.Distance(ua0, up0) / Vector2.Distance(ua0, ub0);
						va = v1 + ta0 * (v2 - v1);
						vb = v1 + tb0 * (v0 - v1);
						p0 = va + tc0 * (vb - va);
						p0 = currentObject.transform.TransformPoint(p0);

						//if (uvSameIdxArrArr[k][j] != Vector2Int.zero)
						//{

						//}

						//Vector2Int tempuv = Vector2Int.zero;
						//if (vposUVDict.TryGetValue(p0, out tempuv))
						//{
						//    uvSameIdxArrArr[k][j] = tempuv;
						//}
						//else
						//{
						//    vposUVDict.TryAdd(p0, new Vector2Int(k, j));
						//}

						if(uvindex[k][j] < 0)
						{
							uvindex[k][j] = i;
							uvpos[k][j] = p0;
						}

						//if (uvindex[k][j] < 0)
						//{
						//    uvindex[k][j] = i;
						//    uvpos[k][j] = p0;
						//    uvSameIdxs[i] = i;
						//    uvSameIdxArrArr[k][j] = new Vector2Int(k, j);
						//}
						//else
						//{
						//    uvSameIdxs[i] = uvindex[k][j];

						//}
					}
				}
			}

			//Debug.Log("adidxDict:" + adjIdxDict.Count);
			//Debug.Log("tricnt:" + tricnt);
			//Debug.Log("sameid:" + setupcnt);
		}

		private static void SetupSameUVIdx(Vector2 duv0, Vector2 duv1, Vector2 suv0, Vector2 suv1)
		{
			var uvwidth = uvtexture.width;
			var uvheight = uvtexture.height;
			if(uvwidth < 1 || uvheight < 1)
			{
				return;
			}

			var du0 = new Vector2(duv0.x * uvwidth, duv0.y * uvheight);
			var du1 = new Vector2(duv1.x * uvwidth, duv1.y * uvheight);
			var duxmin = (int)Mathf.Min(du0.x, du1.x);
			var duxmax = (int)Mathf.Max(du0.x, du1.x);
			var duymin = (int)Mathf.Min(du0.y, du1.y);
			var duymax = (int)Mathf.Max(du0.y, du1.y);
			var dtu0 = (du0.y - du1.y) / (du0.x - du1.x);

			var su0 = new Vector2(suv0.x * uvwidth, suv0.y * uvheight);
			var su1 = new Vector2(suv1.x * uvwidth, suv1.y * uvheight);
			var suxmin = (int)Mathf.Min(su0.x, su1.x);
			var suxmax = (int)Mathf.Max(su0.x, su1.x);
			var suymin = (int)Mathf.Min(su0.y, su1.y);
			var suymax = (int)Mathf.Max(su0.y, su1.y);
			var stu0 = (su0.y - su1.y) / (su0.x - su1.x);

			for(var i = suxmin; i < suxmax; i++)
			{
				var ps0y = (int)((i - su1.x) * stu0 + su1.y);
				var ds0x = i - suxmin + duxmin;
				var ds0y = (int)((ds0x - du1.x) * dtu0 + du1.y);
				uvSameIdxArrArr[ds0x][ds0y] = new Vector2Int(i, ps0y);
			}
		}

		private static void DoDecal(Vector3 thitp0, Vector3 thitp1, Vector3 ithitp, Vector3 ithitp1, float BrushRadFix)
		{
			//Added 2024/05/28
			if(!IsInStroke)
			{
				return;
			}

			//End added 2024/05/28
			var DoSmoothStroke = isSmoothStroke && thitp0 != thitp1 ? true : false;
			Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
			var h = 0;
			foreach(LineRenderer linren in components)
			{
				if(!linren.name.StartsWith("Decal"))
				{
					continue;
				}
				var decalobj = linren.gameObject;
				var decalmeshf = decalobj.GetComponent<MeshFilter>();
				if(decalmeshf == null)
				{
					continue;
				}
				if(decalmeshf && h == DecalInt)
				{
					var decalmesh = decalmeshf.sharedMesh;
					var decalvarr = decalmesh.vertices;
					var decaltarr = decalmesh.triangles;
					var decaluvs = decalmesh.uv;
					var decalren = decalobj.GetComponent<MeshRenderer>();
					var DecalTexture = (Texture2D)decalren.sharedMaterial.mainTexture;
					var IsPaintedArr = Enumerable.Repeat(false, decalvarr.Length).ToArray();
					var bhitp = decalobj.transform.InverseTransformPoint(thitp0);
					var ibhitp = decalobj.transform.InverseTransformPoint(ithitp);
					for(var i = 0; i < decaltarr.Length; i += 3)
					{
						var t0 = decaltarr[i];
						var t1 = decaltarr[i + 1];
						var t2 = decaltarr[i + 2];
						var v0 = decalvarr[t0];
						var v1 = decalvarr[t1];
						var v2 = decalvarr[t2];

						var dx0 = Vector3.Distance(v0, v1);
						var dx1 = Vector3.Distance(v1, v2);
						var dx2 = Vector3.Distance(v2, v0);

						//if (IsBoostPaint && 1!=hitint)
						//if (IsBoostPaint && i != hitint)
						//{
						//    if (smode == SymmetryMode.None)
						//    {
						//        if (CalcDotLineDist2(bhitp, v0, v1) > dx0 && CalcDotLineDist2(bhitp, v1, v2) > dx1 && CalcDotLineDist2(bhitp, v2, v0) > dx2) continue;
						//    }
						//    else
						//    {
						//        if (CalcDotLineDist2(bhitp, v0, v1) > dx0 && CalcDotLineDist2(bhitp, v1, v2) > dx1 && CalcDotLineDist2(bhitp, v2, v0) > dx2
						//            && CalcDotLineDist2(ibhitp, v0, v1) > dx0 && CalcDotLineDist2(ibhitp, v1, v2) > dx1 && CalcDotLineDist2(ibhitp, v2, v0) > dx2
						//            ) continue;
						//    }
						//}

						var uv0 = decaluvs[t0];
						uv0.x *= DecalTexture.width;
						uv0.y *= DecalTexture.height;
						var uv1 = decaluvs[t1];
						uv1.x *= DecalTexture.width;
						uv1.y *= DecalTexture.height;
						var uv2 = decaluvs[t2];
						uv2.x *= DecalTexture.width;
						uv2.y *= DecalTexture.height;
						var uxmin = (int)Mathf.Min(uv0.x, uv1.x, uv2.x);
						var uxmax = (int)Mathf.Max(uv0.x, uv1.x, uv2.x);
						var uymin = (int)Mathf.Min(uv0.y, uv1.y, uv2.y);
						var uymax = (int)Mathf.Max(uv0.y, uv1.y, uv2.y);
						var uvc = (uv0 + uv1 + uv2) / 3;
						var tu0 = (uv0.y - uv1.y) / (uv0.x - uv1.x);
						var tu1 = (uv1.y - uv2.y) / (uv1.x - uv2.x);
						var tu2 = (uv0.y - uv2.y) / (uv0.x - uv2.x);
						bool uflag0 = false, uflag1 = false, uflag2 = false;
						if(uvc.y > (uvc.x - uv1.x) * tu0 + uv1.y)
						{
							uflag0 = true;
						}
						if(uvc.y > (uvc.x - uv2.x) * tu1 + uv2.y)
						{
							uflag1 = true;
						}
						if(uvc.y > (uvc.x - uv2.x) * tu2 + uv2.y)
						{
							uflag2 = true;
						}
						for(var j = uymin; j < uymax + 1; j++)
						{
							for(var k = uxmin; k < uxmax + 1; k++)
							{
								var up0 = new Vector2(k, j);
								if(uflag0 && up0.y < (up0.x - uv1.x) * tu0 + uv1.y)
								{
									continue;
								}
								if(!uflag0 && up0.y > (up0.x - uv1.x) * tu0 + uv1.y)
								{
									continue;
								}
								if(uflag1 && up0.y < (up0.x - uv2.x) * tu1 + uv2.y)
								{
									continue;
								}
								if(!uflag1 && up0.y > (up0.x - uv2.x) * tu1 + uv2.y)
								{
									continue;
								}
								if(uflag2 && up0.y < (up0.x - uv2.x) * tu2 + uv2.y)
								{
									continue;
								}
								if(!uflag2 && up0.y > (up0.x - uv2.x) * tu2 + uv2.y)
								{
									continue;
								}
								var uc0 = up0 + (uv2 - uv0);
								var uc1 = up0 + (uv0 - uv2);
								var u2u1 = uv2 - uv1;
								var c1c0 = uc1 - uc0;
								var c0u1 = uc0 - uv1;
								var fc0 = c1c0.x * c0u1.y - c1c0.y * c0u1.x;
								var fc1 = c1c0.x * u2u1.y - c1c0.y * u2u1.x;

								var ua0 = uv1 + u2u1 * fc0 / fc1;
								var u0u1 = uv0 - uv1;
								var c0c1 = uc0 - uc1;
								var c1u1 = uc1 - uv1;
								var fc2 = c0c1.x * c1u1.y - c0c1.y * c1u1.x;

								//if (fc2 == 0.0f) continue;
								var fc3 = c0c1.x * u0u1.y - c0c1.y * u0u1.x;
								float ta0, tb0, tc0, dist;
								Vector3 va, vb, p0;
								var ub0 = uv1 + u0u1 * fc2 / fc3;
								if(fc0 == 0.0f || fc2 == 0.0f)
								{
									continue;
								}
								ta0 = Vector2.Distance(uv1, ua0) / Vector2.Distance(uv1, uv2);
								tb0 = Vector2.Distance(uv1, ub0) / Vector2.Distance(uv1, uv0);
								tc0 = Vector2.Distance(ua0, up0) / Vector2.Distance(ua0, ub0);
								va = v1 + ta0 * (v2 - v1);
								vb = v1 + tb0 * (v0 - v1);
								p0 = va + tc0 * (vb - va);
								p0 = currentObject.transform.TransformPoint(p0);
								dist = DoSmoothStroke ? HandleUtility.DistancePointLine(p0, thitp0, thitp1) : GetStroke(p0, thitp0);

								//dist = GetStroke(p0, thitp0);
								var c0 = DecalTexture.GetPixel(k, j);
								var c1 = Color.Lerp(c0, BrushColor, BrushStrength * (1 - dist / BrushRadFix));
								DecalTexture.SetPixel(k, j, c1);
							}
						}
					}
					DecalTexture.Apply();
					h++;
				}
				h++;
			}
		}

		private static void DoBETA_Texture(Vector3 thitp0, Vector3 thitp1, Vector3 ithitp0, Vector3 ithitp1, int hitint0, int hitint1, float BrushRadFix)
		{
			//Added 2024/05/28
			if(!IsInStroke)
			{
				return;
			}

			//End added 2024/05/28
			var DoSmoothStroke = isSmoothStroke && thitp0 != thitp1 ? true : false;
			var vertices = currentMesh.vertices;
			var uvs0 = currentMesh.uv;
			var triangles = currentMesh.GetTriangles(paintmatidx);
			var IsPaintedArr = Enumerable.Repeat(false, vertices.Length).ToArray();
			var bhitp = currentObject.transform.InverseTransformPoint(thitp0);
			var ibhitp = Vector3.zero;
			if(smode != SymmetryMode.None)
			{
				ibhitp = currentObject.transform.InverseTransformPoint(ithitp0);
			}
			for(var i = 0; i < vertices.Length; i++)
			{
				var v0 = currentObject.transform.TransformPoint(vertices[i]);
				if(DoSmoothStroke)
				{
					if(HandleUtility.DistancePointLine(v0, thitp0, thitp1) < BrushRadFix)
					{
						IsPaintedArr[i] = true;
					}
					else
					{
						if(smode != SymmetryMode.None)
						{
							if(HandleUtility.DistancePointLine(v0, ithitp0, ithitp1) < BrushRadFix)
							{
								IsPaintedArr[i] = true;
							}
						}
					}
				}
				else
				{
					if(Vector3.Distance(v0, thitp0) < BrushRadFix)
					{
						IsPaintedArr[i] = true;
					}
					else
					{
						if(smode != SymmetryMode.None)
						{
							if(Vector3.Distance(v0, ithitp0) < BrushRadFix)
							{
								IsPaintedArr[i] = true;
							}
						}
					}
				}
			}
			try
			{
				for(var i = 0; i < triangles.Length; i += 3)
				{
					var t0 = triangles[i];
					var t1 = triangles[i + 1];
					var t2 = triangles[i + 2];
					var v0 = vertices[t0];
					var v1 = vertices[t1];
					var v2 = vertices[t2];
					if(!IsPaintedArr[t0] && !IsPaintedArr[t1] && !IsPaintedArr[t2] && i != hitint0)
					{
						if(avgPointDist < BrushRadFix)
						{
							continue;
						}

						var dx0 = Vector3.Distance(v0, v1);
						var dx1 = Vector3.Distance(v1, v2);
						var dx2 = Vector3.Distance(v2, v0);
						if(smode == SymmetryMode.None)
						{
							if(CalcDotLineDist2(bhitp, v0, v1) > dx0 && CalcDotLineDist2(bhitp, v1, v2) > dx1 && CalcDotLineDist2(bhitp, v2, v0) > dx2)
							{
								continue;
							}
						}
						else
						{
							if(CalcDotLineDist2(bhitp, v0, v1) > dx0 && CalcDotLineDist2(bhitp, v1, v2) > dx1 && CalcDotLineDist2(bhitp, v2, v0) > dx2
							   && CalcDotLineDist2(ibhitp, v0, v1) > dx0 && CalcDotLineDist2(ibhitp, v1, v2) > dx1 && CalcDotLineDist2(ibhitp, v2, v0) > dx2
							  )
							{
								continue;
							}
						}
					}

					var uv0 = uvs0[t0];
					uv0.x *= uvtexture.width;
					uv0.y *= uvtexture.height;
					var uv1 = uvs0[t1];
					uv1.x *= uvtexture.width;
					uv1.y *= uvtexture.height;
					var uv2 = uvs0[t2];
					uv2.x *= uvtexture.width;
					uv2.y *= uvtexture.height;
					var uxmin = (int)Mathf.Min(uv0.x, uv1.x, uv2.x);
					var uxmax = (int)Mathf.Max(uv0.x, uv1.x, uv2.x);
					var uymin = (int)Mathf.Min(uv0.y, uv1.y, uv2.y);
					var uymax = (int)Mathf.Max(uv0.y, uv1.y, uv2.y);
					var uvc = (uv0 + uv1 + uv2) / 3;
					var tu0 = (uv0.y - uv1.y) / (uv0.x - uv1.x);
					var tu1 = (uv1.y - uv2.y) / (uv1.x - uv2.x);
					var tu2 = (uv0.y - uv2.y) / (uv0.x - uv2.x);
					bool uflag0 = false, uflag1 = false, uflag2 = false;
					if(uvc.y > (uvc.x - uv1.x) * tu0 + uv1.y)
					{
						uflag0 = true;
					}
					if(uvc.y > (uvc.x - uv2.x) * tu1 + uv2.y)
					{
						uflag1 = true;
					}
					if(uvc.y > (uvc.x - uv2.x) * tu2 + uv2.y)
					{
						uflag2 = true;
					}
					for(var j = uymin; j < uymax + 1; j++)
					{
						for(var k = uxmin; k < uxmax + 1; k++)
						{
							if(uvindex[k][j] != -1 && uvindex[k][j] != i)
							{
								continue;
							}
							if(uvindex[k][j] == i)
							{
								var px0 = uvpos[k][j];

								//float dist0 = GetStroke(px0, thitp0);
								var dist0 = DoSmoothStroke ? HandleUtility.DistancePointLine(px0, thitp0, thitp1) : GetStroke(px0, thitp0);
								var cx0 = uvtexture.GetPixel(k, j);
								Color cx1;
								if(dist0 <= BrushRadFix)
								{
									cx1 = Color.Lerp(cx0, BrushColor, BrushStrength * (1 - dist0 / BrushRadFix));
									uvtexture.SetPixel(k, j, cx1);
								}
								if(smode != SymmetryMode.None)
								{
									//float idist = GetStroke(px0, ithitp0);
									var idist = DoSmoothStroke ? HandleUtility.DistancePointLine(px0, ithitp0, ithitp1) : GetStroke(px0, ithitp0);
									if(idist <= BrushRadFix)
									{
										cx1 = Color.Lerp(cx0, BrushColor, BrushStrength * (1 - idist / BrushRadFix));
										uvtexture.SetPixel(k, j, cx1);
									}
								}
								continue;
							}
							var up0 = new Vector2(k, j);

							if(uflag0 && up0.y < (up0.x - uv1.x) * tu0 + uv1.y)
							{
								continue;
							}
							if(!uflag0 && up0.y > (up0.x - uv1.x) * tu0 + uv1.y)
							{
								continue;
							}
							if(uflag1 && up0.y < (up0.x - uv2.x) * tu1 + uv2.y)
							{
								continue;
							}
							if(!uflag1 && up0.y > (up0.x - uv2.x) * tu1 + uv2.y)
							{
								continue;
							}
							if(uflag2 && up0.y < (up0.x - uv2.x) * tu2 + uv2.y)
							{
								continue;
							}
							if(!uflag2 && up0.y > (up0.x - uv2.x) * tu2 + uv2.y)
							{
								continue;
							}

							Vector2 uc0, uc1, u2u1, c1c0, c0u1, ua0, c0c1, u0u1, c1u1, ub0;
							float ta0, tb0, tc0;
							var dist = -1.0f;
							Vector3 va, vb;
							var p0 = Vector3.zero;
							uc0 = up0 + (uv2 - uv0);
							uc1 = up0 + (uv0 - uv2);
							u2u1 = uv2 - uv1;
							c1c0 = uc1 - uc0;
							c0u1 = uc0 - uv1;
							var fc0 = c1c0.x * c0u1.y - c1c0.y * c0u1.x;
							var fc1 = c1c0.x * u2u1.y - c1c0.y * u2u1.x;
							ua0 = uv1 + u2u1 * fc0 / fc1;
							u0u1 = uv0 - uv1;
							c0c1 = uc0 - uc1;
							c1u1 = uc1 - uv1;
							var fc2 = c0c1.x * c1u1.y - c0c1.y * c1u1.x;
							var fc3 = c0c1.x * u0u1.y - c0c1.y * u0u1.x;
							ub0 = uv1 + u0u1 * fc2 / fc3;
							if(fc0 == 0.0f || fc2 == 0.0f)
							{
								continue;
							}
							ta0 = Vector2.Distance(uv1, ua0) / Vector2.Distance(uv1, uv2);
							tb0 = Vector2.Distance(uv1, ub0) / Vector2.Distance(uv1, uv0);
							tc0 = Vector2.Distance(ua0, up0) / Vector2.Distance(ua0, ub0);
							va = v1 + ta0 * (v2 - v1);
							vb = v1 + tb0 * (v0 - v1);
							p0 = va + tc0 * (vb - va);
							p0 = currentObject.transform.TransformPoint(p0);
							dist = DoSmoothStroke ? HandleUtility.DistancePointLine(p0, thitp0, thitp1) : GetStroke(p0, thitp0);

							//dist = GetStroke(p0, thitp0);

							uvindex[k][j] = i;
							uvpos[k][j] = p0;
							var c0 = uvtexture.GetPixel(k, j);
							Color c1;
							if(dist <= BrushRadFix)
							{
								c1 = Color.Lerp(c0, BrushColor, BrushStrength * (1 - dist / BrushRadFix));
								uvtexture.SetPixel(k, j, c1);
							}
							if(smode != SymmetryMode.None)
							{
								//float idist = GetStroke(p0, ithitp0);
								var idist = DoSmoothStroke ? HandleUtility.DistancePointLine(p0, ithitp0, ithitp1) : GetStroke(p0, ithitp0);
								if(idist <= BrushRadFix)
								{
									c1 = Color.Lerp(c0, BrushColor, BrushStrength * (1 - idist / BrushRadFix));
									uvtexture.SetPixel(k, j, c1);
								}
							}
						}
					}
					IsBrushedArray[triangles[i]] = true;
				}
			}
			catch
			{
			}
			uvtexture.Apply();
		}

		private static void DoBrushHitPosList(float BrushRadFix)
		{
			//Added 2021/08/31
			if(!IsInStroke)
			{
				return;
			}
			if(BrushHitNorm == Vector3.zero)
			{
				return;
			}

			//End Added 2021/08/31
			//Added 2021/08/28
			// //if (!IsBrushHitPos)return;
			// //if (!IsBrushHitPos && BrushHitPosList.Count<2) return;
			if(!IsBrushHitPos && BrushHitPosList.Count < 1)
			{
				return;
			}

			//End Added 2021/08/28
			if(BrushHitPosList.Count < 1)
			{
				BrushHitPosList.Add(BrushHitPos);
				BrushHitIntList.Add(BrushHitInt);
			}
			else if(BrushHitPosList.Count < 1000)
			{
				var prevpos = BrushHitPosList[BrushHitPosList.Count - 1];
				var hdist = Vector3.Distance(currentObject.transform.TransformPoint(prevpos)
					, currentObject.transform.TransformPoint(BrushHitPos));
				if(hdist > BrushRadFix * 0.5f)
				{
					//int bsamp = Mathf.CeilToInt(BrushRadFix / (hdist * 0.5f));
					var bsamp = IsAccurateSample
						? Math.Max(1, (int)(Vector3.Distance(BrushHitPos, prevpos) / avgPointDist)) * ((BrushSamples + 1) / 2)
						: BrushSamples;

					//int bsamp = Math.Max(1, (int)(Vector3.Distance(BrushHitPos, prevpos) / avgPointDist)) * 3;
					//int bsamp = BrushSamples;
					for(var i = 0; i < bsamp; i++)
					{
						BrushHitPosList.Add(prevpos + (BrushHitPos - prevpos) * i / bsamp);
						BrushHitIntList.Add(BrushHitInt);
					}
				}
				else
				{
					BrushHitPosList.Add(BrushHitPos);
					BrushHitIntList.Add(BrushHitInt);
				}
			}
		}

		private static void DoOldBrushHitPosList(float BrushRadFix)
		{
			//Added 2021/09/01
			if(!IsInStroke)
			{
				return;
			}
			if(!IsBrushHitPos && BrushHitPosList.Count < 1)
			{
				return;
			}

			//End Added 2021/09/01
			if(BrushHitPosList.Count < 1)
			{
				BrushHitPosList.Add(BrushOldHitPos);
			}
			else if(BrushHitPosList.Count < 1000)
			{
				var prevpos = BrushHitPosList[BrushHitPosList.Count - 1];
				var hdist = Vector3.Distance(currentObject.transform.TransformPoint(prevpos)
					, currentObject.transform.TransformPoint(BrushOldHitPos));
				if(hdist > BrushRadFix * 0.5f)
				{
					//int bsamp = Mathf.CeilToInt(BrushRadFix / (hdist * 0.5f));
					//int bsamp = BrushSamples;
					var bsamp = IsAccurateSample
						? Math.Max(1, (int)(Vector3.Distance(BrushHitPos, prevpos) / avgPointDist)) * ((BrushSamples + 1) / 2)
						: BrushSamples;
					for(var i = 0; i < bsamp; i++)
					{
						BrushHitPosList.Add(prevpos + (BrushOldHitPos - prevpos) * i / bsamp);
					}
				}
				else
				{
					BrushHitPosList.Add(BrushOldHitPos);
				}
			}
		}

		private static void DoLazyStroke()
		{
			var BrushRadFix = GlobalBrushRad
				? BrushRadius
				  * HandleUtility.GetHandleSize(currentObject.transform.TransformPoint(currentMesh.bounds.center)) * 2.0f
				: BrushRadius;

			//int lazyInt = 1;
			var lazyInt = 6 - LazyDelayInt;
			for(var i = 0; i < lazyInt; i++)
			{
				if(BrushHitPosList.Count < 2)
				{
					break;
				}

				//if (BrushHitPosList.Count > 1)
				//while(BrushHitPosList.Count>1)
				//{
				var thitp = currentObject.transform.TransformPoint(BrushHitPosList[0]);
				var ithitp = currentObject.transform.TransformPoint(GetSymmetry(BrushHitPosList[0]));
				switch(BrushString)
				{
					case "TexturePaint":
						DoTexturePaint(thitp, currentObject.transform.TransformPoint(BrushHitPosList[1]), ithitp, currentObject.transform.TransformPoint
							(GetSymmetry(BrushHitPosList[1])), BrushRadFix);
						break;
					case "Draw":
						DoDraw(thitp, currentObject.transform.TransformPoint(BrushHitPosList[1]), ithitp, currentObject.transform.TransformPoint
							(GetSymmetry(BrushHitPosList[1])), BrushRadFix, false);
						break;
					case "Lower":
						DoDraw(thitp, currentObject.transform.TransformPoint(BrushHitPosList[1]), ithitp, currentObject.transform.TransformPoint
							(GetSymmetry(BrushHitPosList[1])), BrushRadFix, true);
						break;
					case "VertexColor":
						DoVertexColor(thitp, currentObject.transform.TransformPoint(BrushHitPosList[1]), ithitp, currentObject.transform.TransformPoint
							(GetSymmetry(BrushHitPosList[1])), BrushRadFix);
						break;
					case "VertexWeight":
						DoVertexWeight(thitp, currentObject.transform.TransformPoint(BrushHitPosList[1]), ithitp, currentObject.transform.TransformPoint
							(GetSymmetry(BrushHitPosList[1])), BrushRadFix, false);
						break;
					case "EraseWeight":
						DoVertexWeight(thitp, currentObject.transform.TransformPoint(BrushHitPosList[1]), ithitp, currentObject.transform.TransformPoint
							(GetSymmetry(BrushHitPosList[1])), BrushRadFix, true);
						break;
					case "DrawMask":
						DoDrawMask(thitp, currentObject.transform.TransformPoint(BrushHitPosList[1]), ithitp, currentObject.transform.TransformPoint
							(GetSymmetry(BrushHitPosList[1])), BrushRadFix, false);
						break;
					case "EraseMask":
						DoDrawMask(thitp, currentObject.transform.TransformPoint(BrushHitPosList[1]), ithitp, currentObject.transform.TransformPoint
							(GetSymmetry(BrushHitPosList[1])), BrushRadFix, true);
						break;
					case "BETA_Decal":
						if(DecalString != "Create New Decal")
						{
							DoDecal(thitp, currentObject.transform.TransformPoint(BrushHitPosList[1]), ithitp,
								currentObject.transform.TransformPoint(GetSymmetry(BrushHitPosList[1])), BrushRadFix);
						}
						break;
					case "BETA_Texture":
						DoBETA_Texture(thitp, currentObject.transform.TransformPoint(BrushHitPosList[1]), ithitp,
							currentObject.transform.TransformPoint(GetSymmetry(BrushHitPosList[1])), BrushHitIntList[0], BrushHitIntList[1], BrushRadFix);
						break;
				}
				BrushHitPosList.RemoveAt(0);
				if(BrushHitIntList.Count > 0)
				{
					BrushHitIntList.RemoveAt(0);
				}
			}
		}

		private static void DoDraw(Vector3 thitp0, Vector3 thitp1, Vector3 ithitp0, Vector3 ithitp1, float BrushRadFix, bool IsInvert)
		{
			//Added 2024/05/28
			if(!IsInStroke)
			{
				return;
			}

			//End added 2024/05/28
			var DoSmoothStroke = isSmoothStroke && thitp0 != thitp1 ? true : false;
			var r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			var rd = currentObject.transform.InverseTransformDirection(r.direction);
			rd.x /= currentObject.transform.localScale.x;
			rd.y /= currentObject.transform.localScale.y;
			rd.z /= currentObject.transform.localScale.z;
			var vertices = currentMesh.vertices;
			var maskv2 = currentMesh.uv4;
			for(var i = 0; i < vertices.Length; i++)
			{
				var v0 = vertices[i];
				if(smode != SymmetryMode.None)
				{
					var idist = DoSmoothStroke
						? HandleUtility.DistancePointLine(currentObject.transform.TransformPoint(v0), ithitp0, ithitp1)
						: GetStroke(currentObject.transform.TransformPoint(v0), ithitp0);
					if(idist <= BrushRadFix && maskv2[i].x >= maskWeight)
					{
						//Vector3 idirpos = rd.normalized * BrushStrength * 0.1f * (1 - (idist / BrushRadFix)) * (IsInvert ? 1.0f : -1.0f);
						var idirpos = GetSymmetry(rd.normalized * BrushStrength * 0.1f * (1 - idist / BrushRadFix) * (IsInvert ? 1.0f : -1.0f));
						vertices[i] += idirpos;
						IsBrushedArray[i] = true;
					}
				}

				//float dist = GetStroke(currentObject.transform.TransformPoint(v0), thitp);
				var dist = DoSmoothStroke
					? HandleUtility.DistancePointLine(currentObject.transform.TransformPoint(v0), thitp0, thitp1)
					: GetStroke(currentObject.transform.TransformPoint(v0), thitp0);
				if(dist > BrushRadFix || maskv2[i].x < maskWeight)
				{
					continue;
				}
				var dirpos = rd.normalized * BrushStrength * 0.1f * (1 - dist / BrushRadFix) * (IsInvert ? 1.0f : -1.0f);
				vertices[i] += dirpos;
				IsBrushedArray[i] = true;
			}
			currentMesh.vertices = vertices;
		}

		private static void DoVertexColor(Vector3 thitp0, Vector3 thitp1, Vector3 ithitp, Vector3 ithitp1, float BrushRadFix)
		{
			//Added 2024/05/28
			if(!IsInStroke)
			{
				return;
			}

			//End added 2024/05/28
			var vertices = currentMesh.vertices;
			var colors = currentMesh.colors;
			var maskv2 = currentMesh.uv4;
			var DoSmoothStroke = isSmoothStroke && thitp0 != thitp1 ? true : false;
			for(var i = 0; i < vertices.Length; i++)
			{
				if(smode != SymmetryMode.None)
				{
					var idist = DoSmoothStroke
						? HandleUtility.DistancePointLine(vertices[i], ithitp, ithitp1)
						: GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
					if(idist <= BrushRadFix && maskv2[i].x >= maskWeight)
					{
						colors[i] = Color.Lerp(colors[i], BrushColor, BrushStrength * (1 - idist / BrushRadFix));
						IsBrushedArray[i] = true;
					}
				}

				//float dist = GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp);
				var dist = DoSmoothStroke
					? HandleUtility.DistancePointLine(currentObject.transform.TransformPoint(vertices[i]), thitp0, thitp1)
					: GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp0);
				if(dist > BrushRadFix || maskv2[i].x < maskWeight)
				{
					continue;
				}
				colors[i] = Color.Lerp(colors[i], BrushColor, BrushStrength * (1 - dist / BrushRadFix));
				IsBrushedArray[i] = true;
			}
			currentMesh.colors = colors;
		}

		private static void DoDrawMask(Vector3 thitp0, Vector3 thitp1, Vector3 ithitp0, Vector3 ithitp1, float BrushRadFix, bool IsInvert)
		{
			//Added 2024/05/28
			if(!IsInStroke)
			{
				return;
			}

			//End added 2024/05/28
			var vertices = currentMesh.vertices;
			var maskv2 = currentMesh.uv4;
			var DoSmoothStroke = isSmoothStroke && thitp0 != thitp1 ? true : false;
			for(var i = 0; i < vertices.Length; i++)
			{
				if(smode != SymmetryMode.None)
				{
					var idist = DoSmoothStroke
						? HandleUtility.DistancePointLine(currentObject.transform.TransformPoint(vertices[i]), ithitp0, ithitp1)
						: GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp0);
					if(idist <= BrushRadFix)
					{
						var imaskf = IsInvert
							? Mathf.Lerp(maskv2[i].x, 1.0f, BrushStrength * (1 - idist / BrushRadFix))
							: Mathf.Lerp(maskv2[i].x, 0.0f, BrushStrength * (1 - idist / BrushRadFix));
						maskv2[i].x = imaskf;
						IsBrushedArray[i] = true;
					}
				}
				var dist = DoSmoothStroke
					? HandleUtility.DistancePointLine(currentObject.transform.TransformPoint(vertices[i]), thitp0, thitp1)
					: GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp0);
				if(dist > BrushRadFix)
				{
					continue;
				}
				var maskf = IsInvert
					? Mathf.Lerp(maskv2[i].x, 1.0f, BrushStrength * (1 - dist / BrushRadFix))
					: Mathf.Lerp(maskv2[i].x, 0.0f, BrushStrength * (1 - dist / BrushRadFix));
				maskv2[i].x = maskf;
				IsBrushedArray[i] = true;
			}
			currentMesh.uv4 = maskv2;
		}

		private static void DoVertexWeight(Vector3 thitp0, Vector3 thitp1, Vector3 ithitp, Vector3 ithitp1, float BrushRadFix, bool IsInvert)
		{
			//Added 2024/05/28
			if(!IsInStroke)
			{
				return;
			}

			//End added 2024/05/28
			var vertices = currentMesh.vertices;
			var maskv2 = currentMesh.uv4;
			var DoSmoothStroke = isSmoothStroke && thitp0 != thitp1 ? true : false;
			for(var i = 0; i < vertices.Length; i++)
			{
				if(smode != SymmetryMode.None)
				{
					var idist = DoSmoothStroke
						? HandleUtility.DistancePointLine(currentObject.transform.TransformPoint(vertices[i]), ithitp, ithitp1)
						: GetStroke(currentObject.transform.TransformPoint(vertices[i]), ithitp);
					if(idist <= BrushRadFix)
					{
						var iweightf = IsInvert
							? Mathf.Lerp(maskv2[i].y, 1.0f, BrushStrength * 0.1f * (1 - idist / BrushRadFix))
							: Mathf.Lerp(maskv2[i].y, 0.0f, BrushStrength * 0.1f * (1 - idist / BrushRadFix));
						maskv2[i].y = iweightf;
						IsBrushedArray[i] = true;
					}
				}
				var dist = DoSmoothStroke
					? HandleUtility.DistancePointLine(currentObject.transform.TransformPoint(vertices[i]), thitp0, thitp1)
					: GetStroke(currentObject.transform.TransformPoint(vertices[i]), thitp0);
				if(dist > BrushRadFix)
				{
					continue;
				}
				var weightf = IsInvert
					? Mathf.Lerp(maskv2[i].y, 1.0f, BrushStrength * 0.1f * (1 - dist / BrushRadFix))
					: Mathf.Lerp(maskv2[i].y, 0.0f, BrushStrength * 0.1f * (1 - dist / BrushRadFix));
				maskv2[i].y = weightf;
				IsBrushedArray[i] = true;
			}
			currentMesh.uv4 = maskv2;
		}

		private static void ExitSplineBrush()
		{
			var matlist = new List<Material>(currentObject.GetComponent<Renderer>().sharedMaterials);
			var newmatlist = new List<Material>();
			for(var i = 0; i < matlist.Count; i++)
			{
				if(!matlist[i].name.StartsWith(currentObject.name + "_EditorSculptCurve"))
				{
					newmatlist.Add(matlist[i]);
				}
			}
			currentObject.GetComponent<Renderer>().sharedMaterials = newmatlist.ToArray();
		}

		private static Ikinfo GetRootFromAnimationClip(AnimationClip aclip)
		{
			var bindings0 = AnimationUtility.GetCurveBindings(aclip);
			var bpos0 = Vector3.zero;
			var brot0 = Quaternion.identity;
			for(var i = 0; i < bindings0.Length; i++)
			{
				var binding = bindings0[i];
				if(binding.type != typeof(Animator))
				{
					continue;
				}
				var bindstr = binding.propertyName;
				if(bindstr.StartsWith("Motion"))
				{
				}
				else if(bindstr.StartsWith("RootT.x"))
				{
					var curve = AnimationUtility.GetEditorCurve(aclip, binding);
					bpos0.x = curve.Evaluate(animeslider);
				}
				else if(bindstr.StartsWith("RootT.y"))
				{
					var curve = AnimationUtility.GetEditorCurve(aclip, binding);
					bpos0.y = curve.Evaluate(animeslider);
				}
				else if(bindstr.StartsWith("RootT.z"))
				{
					var curve = AnimationUtility.GetEditorCurve(aclip, binding);
					bpos0.z = curve.Evaluate(animeslider);
				}
				else if(bindstr.StartsWith("RootQ.x"))
				{
					var curve = AnimationUtility.GetEditorCurve(aclip, binding);
					brot0.x = curve.Evaluate(animeslider);
				}
				else if(bindstr.StartsWith("RootQ.y"))
				{
					var curve = AnimationUtility.GetEditorCurve(aclip, binding);
					brot0.y = curve.Evaluate(animeslider);
				}
				else if(bindstr.StartsWith("RootQ.z"))
				{
					var curve = AnimationUtility.GetEditorCurve(aclip, binding);
					brot0.z = curve.Evaluate(animeslider);
				}
				else if(bindstr.StartsWith("RootQ.w"))
				{
					var curve = AnimationUtility.GetEditorCurve(aclip, binding);
					brot0.w = curve.Evaluate(animeslider);
				}
			}
			var iki = new Ikinfo();
			iki.position = bpos0;
			iki.rotate = brot0;
			return iki;
		}

		private static void DrawHandle()
		{
			var hitpos = Vector3.zero;
			var hitp = currentObject.transform.TransformPoint(hitpos);

			//Added 2021/11/22  This fixes brush disapear in Unity2021.2.
			//Fixed in Unity2021.2.4?
			//Handles.BeginGUI();
			//Handles.EndGUI();
			//End Added 2021/11/22
			if(BrushString == "BETA_Decal")
			{
				if(strokePrePos != Vector2.zero && strokePostPos == Vector2.zero)
				{
					var mousepos = Event.current.mousePosition;
					Handles.BeginGUI();
					GUI.color = Color.cyan;
					GUI.Box(new Rect(strokePrePos.x, strokePrePos.y, mousepos.x - strokePrePos.x, mousepos.y - strokePrePos.y), "");
					Handles.EndGUI();
				}
				else if(strokePrePos != Vector2.zero && strokepos != Vector2.zero)
				{
					Handles.BeginGUI();
					GUI.color = Color.cyan;
					GUI.Box(new Rect(strokePrePos.x, strokePrePos.y, strokePostPos.x - strokePrePos.x, strokePostPos.y - strokePrePos.y), "");
					Handles.EndGUI();
				}

				//Disabled 2021/06/30
				/*else
				{
				    Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
				    int h = 0;
				    foreach(LineRenderer linren in components)
				    {
				        if (!linren.name.StartsWith("Decal")) continue;
				        if(h!=DecalInt)
				        {
				            MeshFilter decalmf = linren.gameObject.GetComponent<MeshFilter>();
				            if (decalmf)
				            {
				                Mesh decalmesh = decalmf.sharedMesh;
				                Handles.DrawAAConvexPolygon(decalmesh.vertices);
				            }
				        }
				        h++;
				    }
				}*/
				//End Disabled 2021/06/30
				var brushhitpos = currentObject.transform.TransformPoint(BrushHitPos);
				if(BrushHitPos != Vector3.zero)
				{
					Handles.color = Color.red;
					var dotradfix = HandleUtility.GetHandleSize(brushhitpos) * 0.1f;
					Handles.SphereHandleCap(0, brushhitpos, Quaternion.identity, dotradfix, EventType.Repaint);
					if(strokePreVec != Vector3.zero)
					{
						var strokeprev = currentObject.transform.TransformPoint(strokePreVec);
						Handles.SphereHandleCap(0, strokeprev, Quaternion.identity, dotradfix, EventType.Repaint);
					}
				}
			}

			if(BrushString == "BETA_Cut" && CutLineList.Count > 0)
			{
				var col = guiLineColor;
				col.a = 1.0f;
				Handles.color = col;
				for(var i = 0; i < CutLineList.Count - 1; i++)
				{
					var mr0 = HandleUtility.GUIPointToWorldRay(CutLineList[i]);
					var mr1 = HandleUtility.GUIPointToWorldRay(CutLineList[i + 1]);
					var p0 = mr0.GetPoint(Camera.current.nearClipPlane);
					var p1 = mr1.GetPoint(Camera.current.nearClipPlane);
					Handles.DrawLine(p0, p1);
					if(i == 0)
					{
						Handles.SphereHandleCap(0, p0, Quaternion.identity, HandleUtility.GetHandleSize(p0) * 0.1f, EventType.Repaint);
					}
					Handles.SphereHandleCap(0, p1, Quaternion.identity, HandleUtility.GetHandleSize(p1) * 0.1f, EventType.Repaint);

					var p2 = mr0.GetPoint(Camera.current.farClipPlane);
					var q0 = Vector3.Cross(p0 - p1, p0 - p2);
					Handles.Slider((p0 + p1) * 0.5f, q0);
					float ssize = Camera.current.pixelWidth + Camera.current.pixelHeight;

					var bcent = currentMesh.bounds.center;
					bcent = currentObject.transform.TransformPoint(bcent);
					var bcent2 = HandleUtility.WorldToGUIPoint(bcent);
					var bcentr = HandleUtility.GUIPointToWorldRay(bcent2);
					var pc0 = bcentr.GetPoint(Camera.current.nearClipPlane);
					var closeptr = HandleUtility.GUIPointToWorldRay(CloseLinePos);
					var pa0 = closeptr.GetPoint(Camera.current.nearClipPlane);
					Vector3 cx0, cx1;
					var cq0 = (pa0 - pc0).normalized;
					var cq1 = Vector3.Cross(cq0, p0 - p2);
					cx0 = p0 + cq1 * ssize;
					cx1 = p1 + cq1 * ssize;

					var polys = new[] { p0, p1, cx0, cx1 };
					var col0 = guiLineColor;
					col0.a = 0.25f;
					Handles.color = col0;
					Handles.DrawAAConvexPolygon(polys);
					Handles.color = guiLineColor;
				}
				if(CutLineList.Count > 1)
				{
					var sr0 = HandleUtility.GUIPointToWorldRay(strokePrePos);
					var sr1 = HandleUtility.GUIPointToWorldRay(CutLineList[0]);
					var p0 = sr0.GetPoint(Camera.current.nearClipPlane);
					var p1 = sr1.GetPoint(Camera.current.nearClipPlane);
					Handles.DrawLine(p0, p1);

					var sr2 = HandleUtility.GUIPointToWorldRay(CutLineList[CutLineList.Count - 1]);
					var sr3 = HandleUtility.GUIPointToWorldRay(strokePostPos);
					var p2 = sr2.GetPoint(Camera.current.nearClipPlane);
					var p3 = sr3.GetPoint(Camera.current.nearClipPlane);
					Handles.DrawLine(p2, p3);
				}
			}

			else if(BrushString == "BETA_Spline" || BrushString == "BETA_BoneSpike")
			{
				var splineplanes = Get2DSplinePlane(true);
				Handles.DrawSolidRectangleWithOutline(splineplanes, new Color(1.0f, 1.0f, 1.0f, 0.2f), new Color(0.0f, 0.0f, 0.0f, 0.2f));
				Spline3DLoad();
				var col = guiLineColor;
				col.a = 1.0f;
				Handles.color = col;
				for(var i = 0; i < Spline3DListList.Count; i++)
				{
					var SplineLineList = new List<Vector3>();
					for(var j = 0; j < Spline3DListList[i].Count; j++)
					{
						var splinept = currentObject.transform.TransformPoint(Spline3DListList[i][j]);
						SplineLineList.Add(splinept);
						Handles.SphereHandleCap(0, splinept, Quaternion.identity, HandleUtility.GetHandleSize(hitp) * 0.1f, EventType.Repaint);
					}
					Handles.DrawPolyLine(SplineLineList.ToArray());
				}
			}

			else if(BrushString == "BETA_DecalSpline")
			{
				var col = guiLineColor;
				col.a = 1.0f;
				Handles.color = col;
				Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
				foreach(LineRenderer linren in components)
				{
					if(linren.name.StartsWith("DecSpline"))
					{
						var decalsp = new Vector3[linren.positionCount];
						linren.GetPositions(decalsp);
						Handles.DrawPolyLine(decalsp);
						for(var i = 0; i < decalsp.Length; i++)
						{
							Handles.SphereHandleCap(0, decalsp[i], Quaternion.identity, HandleUtility.GetHandleSize(hitp) * 0.1f, EventType.Repaint);
						}
					}
				}
			}

			if(currentObject.GetComponent<SkinnedMeshRenderer>() && AnimePTime == 0.0f &&
			   (DispString == "Bones" || IsShowBones
			                          || IsAnimationBrush || boneAct == BoneAction.Add))

				//    || (IsAnimationBrush && BrushString != "AnimationTip") || boneAct == BoneAction.Add))
			{
				var bones = GetAnimatorBones();
				var anim = currentObject.transform.root.gameObject.GetComponent<Animator>()
				           == true
					? currentObject.transform.root.gameObject.GetComponent<Animator>()
					: null;
				var boneb = Enumerable.Repeat(false, bones.Length).ToArray();
				var mu = Matrix4x4.Rotate(Quaternion.AngleAxis(90.0f, Vector3.right));
				var mr = Matrix4x4.Rotate(Quaternion.AngleAxis(90.0f, Vector3.forward));
				for(var i = 0; i < bones.Length; i++)
				{
					var trans0 = bones[i];
					if(boneAct == BoneAction.Add)
					{
						if(i == parentidx || (parentidx < 0 && i == bones.Length - 1))
						{
							Handles.color = Color.red;
							var dotradfix = HandleUtility.GetHandleSize(trans0.position) * 0.1f;
							var h0 = trans0.position;
							var bhit0 = currentObject.transform.TransformPoint(BrushBoneHitPos);
							var bhit1 = currentObject.transform.TransformPoint(BrushHitPos);
							var h1 = bhit0 * 0.5f + bhit1 * 0.5f;
							Handles.SphereHandleCap(0, h0, Quaternion.identity, dotradfix, EventType.Repaint);
							Handles.DrawLine(h0, h1);
							var hm = h0 * 0.5f + h1 * 0.5f;
							var hv0 = hm + mu.MultiplyPoint3x4(h1 - h0) * 0.1f;
							var hv1 = hm - mu.MultiplyPoint3x4(h1 - h0) * 0.1f;
							var hv2 = hm + mr.MultiplyPoint3x4(h1 - h0) * 0.1f;
							var hv3 = hm - mr.MultiplyPoint3x4(h1 - h0) * 0.1f;
							Handles.DrawAAConvexPolygon(h0, hv0, hv2);
							Handles.DrawAAConvexPolygon(h0, hv0, hv3);
							Handles.DrawAAConvexPolygon(h1, hv0, hv2);
							Handles.DrawAAConvexPolygon(h1, hv0, hv3);
							Handles.DrawAAConvexPolygon(h0, hv1, hv2);
							Handles.DrawAAConvexPolygon(h0, hv1, hv3);
							Handles.DrawAAConvexPolygon(h1, hv1, hv2);
							Handles.DrawAAConvexPolygon(h1, hv1, hv3);
						}
					}
					if(!ShowAllbones && IsAnimationBrush && boneAct == BoneAction.None)
					{
						if(BoneMinIdx != i && bones[i].parent != trans0 && trans0.parent != bones[i])
						{
							continue;
						}
					}

					//if(IsAnimationBrush && BrushString != "AnimationTip")
					if(IsAnimationBrush && BrushString != "AnimationTip" && ((boneSelectFlags | BoneSelectFlags.Humanoid) == boneSelectFlags
					                                                         || (boneSelectFlags | BoneSelectFlags.Generic) == boneSelectFlags))
					{
						for(var j = 0; j < bones.Length; j++)
						{
							if(i == j)
							{
								continue;
							}
							var trans1 = bones[j];
							if(trans0.parent == trans1 || trans1.parent == trans0
							                           || trans0.parent == trans1.parent || trans1.parent == trans0.parent)
							{
								if(boneb[i] && boneb[j])
								{
									continue;
								}
								boneb[j] = true;
								if(!ShowAllbones && IsAnimationBrush && boneAct == BoneAction.None)
								{
									if(BoneMinIdx != i && BoneMinIdx != j)
									{
										continue;
									}
								}
								var h0 = trans0.position;
								var h1 = trans1.position;
								Handles.DrawLine(h0, h1);
								var hm = h0 * 0.5f + h1 * 0.5f;
								var hv0 = hm + mu.MultiplyPoint3x4(h1 - h0) * 0.1f;
								var hv1 = hm - mu.MultiplyPoint3x4(h1 - h0) * 0.1f;
								var hv2 = hm + mr.MultiplyPoint3x4(h1 - h0) * 0.1f;
								var hv3 = hm - mr.MultiplyPoint3x4(h1 - h0) * 0.1f;
								var col = boneColor;
								col.a = bonetransp;
								Handles.color = col;
								Handles.DrawAAConvexPolygon(h0, hv0, hv2);
								Handles.DrawAAConvexPolygon(h0, hv0, hv3);
								Handles.DrawAAConvexPolygon(h1, hv0, hv2);
								Handles.DrawAAConvexPolygon(h1, hv0, hv3);
								Handles.DrawAAConvexPolygon(h0, hv1, hv2);
								Handles.DrawAAConvexPolygon(h0, hv1, hv3);
								Handles.DrawAAConvexPolygon(h1, hv1, hv2);
								Handles.DrawAAConvexPolygon(h1, hv1, hv3);
							}
						}
					}
					boneb[i] = true;
				}

				//if ((boneSelectFlags | BoneSelectFlags.Tips) == boneSelectFlags)
				//if (BrushString == "AnimationTip" || ((boneSelectFlags | BoneSelectFlags.Tips) == boneSelectFlags))
				if(BrushString == "AnimationTip" || ((boneSelectFlags | BoneSelectFlags.Tips) == boneSelectFlags && boneAct != BoneAction.Add))
				{
					//Added 2022/11/24
					if(bones.Length != extrabones.Length)
					{
						extrabones = GetExtraBoneVec(bones);
					}
					for(var i = 0; i < bones.Length; i++)
					{
						var bone = bones[i];

						//if (bone.childCount != 0) continue;
						if(extrabones[i] == Vector3.zero)
						{
							continue;
						}
						if(!ShowAllbones && IsAnimationBrush && boneAct == BoneAction.None)
						{
							if(BoneMinIdx != i)
							{
								continue;
							}
						}
						var h0 = bone.position;

						//Vector3 h1 = h0 + (h0 - bone.parent.position);
						var h1 = extrabones[i];
						Handles.DrawLine(h0, h1);
						var hm = h0 * 0.5f + h1 * 0.5f;
						var hv0 = hm + mu.MultiplyPoint3x4(h1 - h0) * 0.1f;
						var hv1 = hm - mu.MultiplyPoint3x4(h1 - h0) * 0.1f;
						var hv2 = hm + mr.MultiplyPoint3x4(h1 - h0) * 0.1f;
						var hv3 = hm - mr.MultiplyPoint3x4(h1 - h0) * 0.1f;
						var col = boneColor;
						col.a = bonetransp;
						Handles.color = col;
						Handles.DrawAAConvexPolygon(h0, hv0, hv2);
						Handles.DrawAAConvexPolygon(h0, hv0, hv3);
						Handles.DrawAAConvexPolygon(h1, hv0, hv2);
						Handles.DrawAAConvexPolygon(h1, hv0, hv3);
						Handles.DrawAAConvexPolygon(h0, hv1, hv2);
						Handles.DrawAAConvexPolygon(h0, hv1, hv3);
						Handles.DrawAAConvexPolygon(h1, hv1, hv2);
						Handles.DrawAAConvexPolygon(h1, hv1, hv3);
					}
				}

				//End Added 2022/11/24
			}
			if(NeedFrameSelected)
			{
				NeedFrameSelected = false;
				SceneView.lastActiveSceneView.FrameSelected();
			}
			if(IsSplineUpdate)
			{
				if(Spline3DVertListList.Count > 0)
				{
					Spline3DUpdate();
				}
			}
			if(IsDecalUpdate)
			{
				if(Decal3DListList.Count > 0)
				{
					DecalUpdate();
				}
			}
			if(boneAct == BoneAction.Add)
			{
				if(BrushBoneHitPos != Vector3.zero)
				{
					var bhit0 = currentObject.transform.TransformPoint(BrushBoneHitPos);
					var bhit1 = currentObject.transform.TransformPoint(BrushHitPos);
					var bhite = bhit0 * 0.5f + bhit1 * 0.5f;
					var dotradbone = HandleUtility.GetHandleSize(bhite) * 0.5f;
					var col = Color.green;
					col.a = 0.5f;
					Handles.color = col;
					Handles.SphereHandleCap(0, bhite, Quaternion.identity, dotradbone, EventType.Repaint);
				}
			}

			//if (IsAnimationBrush || boneAct == BoneAction.Delete)
			//Changed 2025/05/31 to fix Add bone
			else if(IsAnimationBrush || boneAct == BoneAction.Delete)
			{
				if(!currentObject.GetComponent<SkinnedMeshRenderer>())
				{
					return;
				}
				var bones = GetAnimatorBones();
				if(bones.Length < 1)
				{
					return;
				}
				var anim = currentObject.transform.root.gameObject.GetComponent<Animator>()
				           == true
					? currentObject.transform.root.gameObject.GetComponent<Animator>()
					: null;
				var r0 = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				var BrushRadmFix = GlobalBrushRad ? BrushRadius * HandleUtility.GetHandleSize(currentSkinned.bounds.center) * 2.0f : BrushRadius;
				var minf0 = -1.0f;
				var minint = -1;
				var mousep = Event.current.mousePosition;

				if(BrushString == "AnimationTip")
				{
					for(var i = 0; i < extrabones.Length; i++)
					{
						if(extrabones[i] == Vector3.zero)
						{
							continue;
						}
						var bpos = Vector2.zero;
						try
						{
							bpos = HandleUtility.WorldToGUIPoint(extrabones[i]);
						}
						catch
						{
							continue;
						}
						var dist = Vector2.Distance(bpos, mousep);
						if(minint < 0 || dist < minf0)
						{
							var r1 = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(extrabones[i]));
							var rdist = Vector3.Distance(r1.origin, extrabones[i]);
							var brdist = Vector3.Distance(r0.GetPoint(rdist), r1.GetPoint(rdist));
							if(brdist <= BrushRadmFix)
							{
								minint = i;
								minf0 = dist;
							}
						}
					}

					//if (minint >= 0) IsBoneTips = true;
				}
				else
				{
					for(var i = 0; i < bones.Length; i++)
					{
						try
						{
							if(!AnimationValidateHuman(bones[i]))
							{
								if(boneSelectFlags != (boneSelectFlags | BoneSelectFlags.Generic))
								{
									continue;
								}
							}
							else
							{
								if(boneSelectFlags != (boneSelectFlags | BoneSelectFlags.Humanoid))
								{
									continue;
								}
							}
						}
						catch
						{
							continue;
						}
						var bpos = Vector2.zero;
						try
						{
							bpos = HandleUtility.WorldToGUIPoint(bones[i].position);
						}
						catch
						{
							continue;
						}
						var dist = Vector2.Distance(bpos, mousep);
						if((minint < 0 || dist < minf0) && boneSelectFlags != BoneSelectFlags.IK)
						{
							var r1 = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(bones[i].position));
							var rdist = Vector3.Distance(r1.origin, bones[i].position);
							var brdist = Vector3.Distance(r0.GetPoint(rdist), r1.GetPoint(rdist));
							if(brdist <= BrushRadmFix)
							{
								minint = i;
								minf0 = dist;
							}
						}
					}

					//if (minint >= 0) IsBoneTips = false;

					//Added 2022/11/24
					var minint1 = -1;
					var minf1 = 0.0f;

					//if (((boneSelectFlags | BoneSelectFlags.Tips) == boneSelectFlags) && boneAct!=BoneAction.Delete)
					if((boneSelectFlags | BoneSelectFlags.Tips) == boneSelectFlags && boneAct != BoneAction.Delete && boneAct != BoneAction.Add)
					{
						for(var i = 0; i < extrabones.Length; i++)
						{
							if(extrabones[i] == Vector3.zero)
							{
								continue;
							}
							var bpos = Vector2.zero;
							try
							{
								bpos = HandleUtility.WorldToGUIPoint(extrabones[i]);
							}
							catch
							{
								continue;
							}
							var dist = Vector2.Distance(bpos, mousep);
							if(minint1 < 0 || dist < minf1)
							{
								var r1 = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(extrabones[i]));
								var rdist = Vector3.Distance(r1.origin, extrabones[i]);
								var brdist = Vector3.Distance(r0.GetPoint(rdist), r1.GetPoint(rdist));
								if(brdist <= BrushRadmFix)
								{
									minint1 = i;
									minf1 = dist;
								}
							}
						}
						IsBoneTips = false;
						if((minf1 < minf0 && minint1 >= 0) || (minint < 0 && minint1 >= 0))
						{
							minf0 = minf1;
							minint = minint1;
							IsBoneTips = true;
						}
					}

					//End Added 2022/11/24
				}
				BoneMinIdx = minint;
				if(BoneMinIdx >= 0)
				{
					//Vector3 bv0 = BrushString == "AnimationTip" ? extrabones[BoneMinIdx] : bones[BoneMinIdx].position;
					var bv0 = BrushString == "AnimationTip" || IsBoneTips ? extrabones[BoneMinIdx] : bones[BoneMinIdx].position;
					var dotradbone = HandleUtility.GetHandleSize(bv0) * 0.5f;
					var col = Color.green;
					col.a = 0.5f;
					Handles.color = col;
					Handles.SphereHandleCap(0, bv0, Quaternion.identity, dotradbone, EventType.Repaint);
				}

				if(blendBaseMesh == null)
				{
					Handles.color = Color.white;
				}
				else
				{
					Handles.color = Color.red;
				}
				var mouser = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				var mousevec = mouser.GetPoint(Vector3.Distance(mouser.origin, currentSkinned.bounds.center));
				Handles.DrawWireDisc(mousevec, -mouser.direction, BrushRadmFix);
				Handles.color = Color.red;
				var dotdist = Vector3.Distance(r0.origin, currentObject.transform.TransformPoint(BrushHitPos));
				var dotradfix = HandleUtility.GetHandleSize(r0.GetPoint(dotdist)) * 0.1f;
				Handles.SphereHandleCap(0, r0.GetPoint(dotdist), Quaternion.identity, dotradfix, EventType.Repaint);

				/*if(boneAct==BoneAction.Insert && minint>=0)
				{
				    float mindist = 0.0f;
				    int minidx = -1;
				    Vector3 vm0 = bones[minint].position;
				    for(int i=0;i<bones.Length;i++)
				    {
				        if (bones[i].parent != bones[minint] && bones[minint].parent != bones[i]) continue;
				        Vector3 v0 = bones[i].position;
				        float dist = Vector3.Dot((vm0-v0).normalized,currentObject.transform.TransformPoint(BrushHitPos) -vm0);
				        if (dist < 0) continue;
				        if(dist<mindist || minidx<0)
				        {
				            mindist = dist;
				            minidx = i;
				        }
				    }
				    if (minidx >= 0)
				    {
				        Vector3 inspos = vm0 + (vm0 - bones[minidx].position).normalized * mindist;
				        BrushBoneHitPos = currentObject.transform.TransformPoint(inspos);
				        Handles.color = Color.red;
				        Handles.SphereHandleCap(0, inspos, UnityEngine.Quaternion.identity, dotradfix, EventType.Repaint);
				    }
				}*/
				return;
			}

			if(blendBaseMesh == null)
			{
				Handles.color = Color.white;
			}
			else
			{
				Handles.color = Color.red;
			}

			//    private static readonly MethodInfo intersectRayMeshMethod = typeof(HandleUtility).GetMethod("IntersectRayMesh",
			//BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

			//if ((bool)intersectRayMeshMethod.Invoke(null, rayMeshParameters))
			//{
			//    hit = (RaycastHit)rayMeshParameters[3];
			//    return true;
			//}

			var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			var rayhit = new RaycastHit();
			var rayMeshParameters = new object[]
				{ ray, currentMesh, currentObject.transform.localToWorldMatrix, null };
			var rayCastMethod = typeof(HandleUtility).GetMethod("IntersectRayMesh", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if((bool)rayCastMethod.Invoke(null, rayMeshParameters))
			{
				rayhit = (RaycastHit)rayMeshParameters[3];
			}
			var brushhitp = rayhit.point;
			BrushHitPos = currentObject.transform.InverseTransformPoint(brushhitp);
			BrushHitNorm = rayhit.normal;

			// //Vector3 brushhitp = Vector3.zero;
			// //HandleUtility.FindNearestVertex(Event.current.mousePosition, new Transform[] {currentObject.transform},out brushhitp);
			// //BrushHitPos = currentObject.transform.InverseTransformPoint(brushhitp);
			//Vector3[] vertices = currentMesh.vertices;
			//int vcount = vertices.Length;
			//int[] triangles = currentMesh.triangles;
			//for(int i=0;i<vcount;i++)
			//{
			//    if (vertices[i]==BrushHitPos)
			//    {
			//        BrushHitNorm = currentMesh.normals[i];
			//        break;
			//    }
			//}
			var BrushRadFix = GlobalBrushRad
				? BrushRadius
				  * HandleUtility.GetHandleSize(brushhitp) * 2.0f
				: BrushRadius;

			//float BrushRadFix = GlobalBrushRad == true ? BrushRadius
			//    * HandleUtility.GetHandleSize(currentObject.transform.TransformPoint(BrushHitPos)) * 2.0f : BrushRadius;
			//HandleUtility.FindNearestVertex(Event.current.mousePosition, out BrushHitPos);
			//Vector3 brushhitp = currentObject.transform.TransformPoint(BrushHitPos);
			//Handles.DrawWireDisc(brushhitp, currentObject.transform.TransformDirection(BrushHitNorm), BrushRadFix);
			//Handles.DrawWireDisc(brushhitp, ray.direction, BrushRadFix);
			//if (BrushHitPos != Vector3.zero)
			if(BrushHitNorm != Vector3.zero)
			{
				Handles.DrawWireDisc(brushhitp, BrushHitNorm, BrushRadFix);
				Handles.color = Color.red;
				var dotradfix = HandleUtility.GetHandleSize(brushhitp) * 0.1f;
				Handles.SphereHandleCap(0, brushhitp, Quaternion.identity, dotradfix, EventType.Repaint);

				//if (IsInStroke)
				if(IsInStroke && LazyDelayInt > 3)
				{
					if(BrushHitPosList.Count > 0)
					{
						var col = guiLineColor;
						col.a = 1.0f;
						Handles.color = col;
						Handles.DrawAAPolyLine(brushhitp, currentObject.transform.TransformPoint(BrushHitPosList[0]));
						Handles.SphereHandleCap(0, currentObject.transform.TransformPoint(BrushHitPosList[0])
							, Quaternion.identity, dotradfix, EventType.Repaint);
					}
				}
			}
			else
			{
				//Handles.DrawWireDisc(ray.origin, ray.direction, BrushRadFix);

				var centpos = HandleUtility.WorldToGUIPoint(currentObject.transform.TransformPoint(currentMesh.bounds.center));
				var rayc = HandleUtility.GUIPointToWorldRay(centpos);
				var getpos = ray.GetPoint(Vector3.Distance(rayc.origin, currentObject.transform.TransformPoint(currentMesh.bounds.center)));
				BrushRadFix = GlobalBrushRad ? BrushRadius * HandleUtility.GetHandleSize(getpos) * 2.0f : BrushRadius;
				Handles.DrawWireDisc(ray.GetPoint(Vector3.Distance(rayc.origin, currentObject.transform.TransformPoint(currentMesh.bounds.center))), ray.direction, BrushRadFix);
			}
		}

		public static void OnSceneGUI(SceneView sceneview)
		{
			if(window != null)
			{
				//if (IsEnable && !window.hasFocus && currentObject != Selection.activeGameObject)
				if(IsEnable && focusedWindow != window && currentObject != Selection.activeGameObject)
				{
					//Selection.activeGameObject = null;
					//IsEnable = false;
					//Tools.hidden = false;
					//currentObject = null;
					if(EditorGUIUtility.currentViewWidth > window.position.width)
					{
						window.OnSelectionChange();
					}
				}
			}
			if(IsLoadMesh)
			{
				return;
			}

			//if (IsInStroke && BrushHitPosList.Count > 0) DoLazyStroke();
			//if (!IsLoadPreference) LoadPreference();
			if(BrushHitPosList.Count > 0 && currentMesh != null)
			{
				DoLazyStroke();
			}

			if(pickerid == 0)
			{
				pickerid = -1;
				if(startmesh == null)
				{
					return;
				}
				var starttime = (float)EditorApplication.timeSinceStartup;
				IsLoadMesh = true;
				currentObject = LoadGameObjectFromMesh(startmesh, true);
				if(currentObject != null && currentObject.activeInHierarchy == false && UnloadMesh == null)
				{
					var newgo = InstantiateGameObject(currentObject);

					//Added 2023/05/08
					//MoveInstanceGameobj(currentObject, newgo);
					MoveOverlapGameObject(newgo);

					//End added 2023/05/08
					Selection.activeGameObject = newgo;
					currentObject = newgo;
				}
				else
				{
					Selection.activeGameObject = currentObject;
				}

				GetCurrentMesh(true);
				startmesh = null;
				IsLoadMesh = false;
				IsEnable = true;
				Tools.hidden = false;
				if(DebugMode)
				{
					Debug.Log("Check Mesh complete:" + ((float)EditorApplication.timeSinceStartup - starttime) + "sec");
				}
			}
			else if(pickerid == 1)
			{
				//New in 2020/10/14
				AnimationSave(animeslider, 0.0f, false);

				//End New in 2020/10/14

				AnimationImport(impanim);
				impanim = null;

				//Resources.UnloadUnusedAssets();
				pickerid = -1;
				aclipidx = currentObject.transform.root.gameObject.GetComponent<Animator>()
					.runtimeAnimatorController.animationClips.Length - 1;
				currentObject.transform.root.gameObject.GetComponent<Animator>().applyRootMotion = false;
				oldaclipidx = aclipidx;
				animeslider = 0.0f;
				oldanimsli = 0.0f;
				AnimationPoseLoad(animeslider);

				//SceneView.lastActiveSceneView.FrameSelected();
				SceneView.RepaintAll();
				ChangeMaterial();
			}

			if(UnloadMesh != null)
			{
				Selection.activeGameObject = LoadGameObjectFromMesh(UnloadMesh, CheckEditorSculptObj(UnloadMesh));
				GetCurrentMesh(true);
				UnloadMesh = null;
				window.Repaint();
			}

			//#if UNITY_2019
			if(IsNeedChangeLightmap)
			{
				if(!Lightmapping.isRunning)
				{
					IsNeedChangeLightmap = false;
					Lightmapping.giWorkflowMode = (Lightmapping.GIWorkflowMode)oldGIWork;

					//Lightmapping.giWorkflowMode = oldGIWork;
				}
			}

			//#endif

			//if (!Tools.hidden) return;
			//if (currentStatus == SculptStatus.Inactive) return;
			var ctrlID = GUIUtility.GetControlID(windowHash, FocusType.Passive);

			if(currentObject == null || currentMesh == null)
			{
				return;
			}

			if(decalidx >= Decal3DListList.Count)
			{
				DecalUpdate();
				decalidx = 0;
			}

			if(blendBaseMesh != null)
			{
				Handles.BeginGUI();
				GUILayout.BeginArea(new Rect(100, 0, 200, 200));
				var style = new GUIStyle();
				style.normal.textColor = Color.red;
				GUILayout.Label("Recording BlendShape", style);
				if(GUILayout.Button("Stop Record", GUILayout.Width(100)))
				{
					BlendShapeCreate();
					AssetDatabase.Refresh();
					NeedFrameSelected = true;
				}
				GUILayout.EndArea();
				Handles.EndGUI();
			}
			if(boneAct == BoneAction.Add)
			{
				Handles.BeginGUI();
				GUILayout.BeginArea(new Rect(100, 0, 200, 200));
				var style = new GUIStyle();
				style.normal.textColor = Color.green;
				GUILayout.Label("Click mouse to add a bone", style);
				if(GUILayout.Button("Cancel add a bone", GUILayout.Width(150)))
				{
					boneAct = BoneAction.None;
					if(IsAnimationBrush)
					{
						AnimatorSetup(false);
					}
				}
				GUILayout.EndArea();
				Handles.EndGUI();
			}
			else if(boneAct == BoneAction.Delete)
			{
				Handles.BeginGUI();
				GUILayout.BeginArea(new Rect(100, 0, 200, 200));
				var style = new GUIStyle();
				style.normal.textColor = Color.green;
				GUILayout.Label("Click mouse on a bone to delete that bone", style);
				if(GUILayout.Button("Cancel delete a bone", GUILayout.Width(150)))
				{
					boneAct = BoneAction.None;
					if(IsAnimationBrush)
					{
						AnimatorSetup(false);
					}
				}
				GUILayout.EndArea();
				Handles.EndGUI();
			}
			else if(IsAnimationBrush)

				//if (IsAnimationBrush)
			{
				Handles.BeginGUI();
				GUILayout.BeginArea(new Rect(100, 0, 200, 350));

				//GUILayout.BeginArea(new Rect(100, 0, 200, 250));
				try
				{
					var animt = currentObject.transform.root.gameObject.GetComponent<Animator>();
					var aclips = animt.runtimeAnimatorController.animationClips;
					var aclip = aclips[aclipidx];
					IsUseKeyFrame = EditorGUILayout.Toggle("Use KeyFrame Animation", IsUseKeyFrame);
					EditorGUILayout.LabelField("AnimationTime(sec)");
					animeslider = EditorGUILayout.Slider(animeslider, 0.0f, aclip.length, GUILayout.Width(150));
					if(IsUseKeyFrame)
					{
						EditorGUILayout.LabelField("Previous Key Frame");
						animeKeySlider = EditorGUILayout.Slider(animeKeySlider, 0.0f, aclip.length, GUILayout.Width(150));
						EditorGUILayout.LabelField("Next Key Frame");
						animeKeyPostSlider = EditorGUILayout.Slider(animeKeyPostSlider, 0.0f, aclip.length, GUILayout.Width(150));

						//animeKeySlider = GetNearKeyTimeMulti(aclip, animeslider);
						animeKeySlider = GetNearKeyTime2(aclip, animeslider, true);
						animeKeyPostSlider = GetNearKeyTime2(aclip, animeslider, false);
					}
					if(!IsLegacyAnime)
					{
						if(IsUseKeyFrame)
						{
							if(GUILayout.Button("Save to a new KeyFrame"))
							{
								AnimationSave(animeslider, animeslider, true);
								IsBoneMove = false;
							}
							if(GUILayout.Button("Save to Previous KeyFrame"))
							{
								var keytime = GetNearKeyTime2(aclip, animeslider, true);
								animeslider = keytime;
								AnimationSave(animeslider, animeslider, true);
								IsBoneMove = false;
							}
							if(GUILayout.Button("Save to Next KeyFrame"))
							{
								var keytime = GetNearKeyTime2(aclip, animeslider, false);
								animeslider = keytime;
								AnimationSave(animeslider, animeslider, true);
								IsBoneMove = false;
							}

							//if (GUILayout.Button("Save to nearest KeyFrame"))
							//{
							//    float keytime = GetNearKeyTimeMulti(aclip, animeslider);
							//    animeslider = keytime;
							//    AnimationSave(animeslider, animeslider, true);
							//    IsBoneMove = false;
							//}
							EditorGUILayout.LabelField("");

							//if (GUILayout.Button("Goto previous KeyFrame"))
							//{
							//    float keytime = GetNearKeyTime2(aclip, animeslider, true);
							//    animeslider = keytime;
							//    //Debug.Log(keytime);
							//    AnimationPoseLoad(keytime);
							//    IsEditKeyFrame = true;
							//}
							//if (GUILayout.Button("Goto next KeyFrame"))
							//{
							//    float keytime = GetNearKeyTime2(aclip, animeslider, false);
							//    animeslider = keytime;
							//    AnimationPoseLoad(keytime);
							//    IsEditKeyFrame = true;
							//}
							//if ((IsEditKeyFrame==true) && (IsBoneMove==true))
							//{
							//    if (GUILayout.Button("Save KeyFrame"))
							//    {
							//        AnimationSave(animeslider, animeslider, true);
							//        IsBoneMove = false;
							//    }
							//}
							//if(IsEditKeyFrame)
							//{
							//if (GUILayout.Button("Insert New KeyFrame"))
							//{
							//    float keytime = (GetNearKeyTime2(aclip, animeslider, false) + GetNearKeyTime2(aclip, animeslider, true)) / 2;
							//    animeslider = keytime;
							//    AnimationPoseLoad(keytime);
							//    AnimationSave(animeslider, animeslider, true);
							//}
							//if (GUILayout.Button("Remove Nearest KeyFrame"))
							//{
							//    float keytime = GetNearKeyTimeMulti(aclip, animeslider);
							//    animeslider = keytime;
							//    AnimationClipRemoveKey(aclip, animeslider);
							//    keytime = GetNearKeyTime2(aclip, animeslider, true);
							//    animeslider = keytime;
							//    AnimationPoseLoad(keytime);
							//}
							if(GUILayout.Button("Remove Previous KeyFrame"))
							{
								var keytime = GetNearKeyTime2(aclip, animeslider, true);
								animeslider = keytime;
								if(animeslider > 0.0f && animeslider < 1.0f)
								{
									AnimationClipRemoveKey(aclip, animeslider);
									keytime = GetNearKeyTime2(aclip, animeslider, true);
									animeslider = keytime;
									AnimationPoseLoad(keytime);
								}
								else
								{
									EditorUtility.DisplayDialog("caution", "You cann't remove the first KeyFrame.", "OK");
								}
							}
							if(GUILayout.Button("Remove Next KeyFrame"))
							{
								var keytime = GetNearKeyTime2(aclip, animeslider, false);
								animeslider = keytime;
								if(animeslider > 0.0f && animeslider < 1.0f)
								{
									AnimationClipRemoveKey(aclip, animeslider);
									keytime = GetNearKeyTime2(aclip, animeslider, true);
									animeslider = keytime;
									AnimationPoseLoad(keytime);
								}
								else
								{
									EditorUtility.DisplayDialog("caution", "You cann't remove the last KeyFrame.", "OK");
								}
							}
						}

						//if (GUILayout.Button("Remove KeyFrame"))
						//{
						//    AnimationClipRemoveKey(aclip, animeslider);
						//    float keytime = GetNearKeyTime2(aclip, animeslider, true);
						//    animeslider = keytime;
						//    AnimationPoseLoad(keytime);
						//}
						//}

						//if (IsBoneMove)
						else if(IsBoneMove)
						{
							animeAclipLength = aclip.length;
							if(animeOverrideMin < 0)
							{
								animeOverrideMin = animeslider;
							}
							if(animeOverrideMax < 0)
							{
								animeOverrideMax = animeslider + (animeAclipLength - animeslider) * 0.1f;
							}
							EditorGUILayout.LabelField("Animation save start time(sec)");
							animeOverrideMin = EditorGUILayout.Slider(animeOverrideMin, 0.0f, animeAclipLength, GUILayout.Width(150));
							EditorGUILayout.LabelField("Animation save end time(sec)");
							animeOverrideMax = EditorGUILayout.Slider(animeOverrideMax, 0.0f, animeAclipLength, GUILayout.Width(150));
							if(GUILayout.Button("Save Animation"))
							{
								AnimationSave(animeOverrideMin, animeOverrideMax, true);
								IsAnimationPaste = false;
								animeOverrideMin = -1.0f;
								animeOverrideMax = -1.0f;
								IsBoneMove = false;

								//BoneMoveHash.Clear();
							}
							if(GUILayout.Button("Cancel"))
							{
								IsAnimationPaste = false;
								IsBoneMove = false;
							}
						}
					}
					if(IsAnimationPaste)
					{
						if(!IsUseKeyFrame)
						{
							if(animePasteMin < 0)
							{
								animePasteMin = animeslider;
							}
							if(animePasteMax < 0)
							{
								animePasteMax = animeslider + (aclip.length - animeslider) * 0.1f;
							}

							//EditorGUILayout.MinMaxSlider(ref animePasteMIn, ref animePasteMax, 0.0f, aclip.length, GUILayout.Width(120));
							EditorGUILayout.LabelField("Animation paste start time(sec)");
							animePasteMin = EditorGUILayout.Slider(animePasteMin, 0.0f, aclip.length, GUILayout.Width(150));
							EditorGUILayout.LabelField("Animation paste end time(sec)");
							animePasteMax = EditorGUILayout.Slider(animePasteMax, 0.0f, aclip.length, GUILayout.Width(150));
							if(GUILayout.Button("Paste Animation"))
							{
								AnimationSave(animePasteMin, animePasteMax, true);
								IsAnimationPaste = false;
								animePasteMin = -1.0f;
								animePasteMax = -1.0f;
							}
							if(GUILayout.Button("Cancel"))
							{
								IsAnimationPaste = false;
								IsBoneMove = false;
							}
						}
						else
						{
							IsBoneMove = true;
						}
					}
					if(IsBoneMove)
					{
						var style = new GUIStyle();
						style.normal.textColor = Color.green;
						if(IsLegacyAnime)
						{
							EditorGUILayout.LabelField("Move slider to save the animation.", style);
						}
					}
					IsLegacyAnime = EditorGUILayout.Toggle("Legacy Animation", IsLegacyAnime);
				}
				catch
				{
				}
				GUILayout.EndArea();
				Handles.EndGUI();
				if(GUI.changed)
				{
					if(oldanimsli != animeslider)
					{
						//if (IsBoneMove)
						//if ((IsBoneMove) && (!IsUseKeyFrame))
						//{
						//    //if(!IsManualOverrideAnime)AnimationSave(oldanimsli, 0.0f, false);
						//    AnimationSave(oldanimsli, 0.0f, !IsLegacyAnime);
						//    //BoneMoveHash.Clear();
						//}

						AnimationSaveBoneMove(oldanimsli, 0.0f, !IsLegacyAnime);
						AnimationPoseLoad(animeslider);
						IsBoneMove = false;

						//IsEditKeyFrame = false;
						//BoneMoveHash.Clear();
						oldanimsli = animeslider;
					}
				}
			}
			else if(IsPreviewAnimation)
			{
				Handles.BeginGUI();
				GUILayout.BeginArea(new Rect(100, 0, 200, 200));
				var style = new GUIStyle();
				style.normal.textColor = Color.green;

				//GUILayout.Label("Previewing the Animation", style);
				//GUILayout.Label("Click on the screen to stop.", style);
				EditorGUILayout.LabelField("Previewing the Animation", style);
				EditorGUILayout.LabelField("Click on the screen to stop.", style);
				GUILayout.EndArea();
				Handles.EndGUI();
			}

			if(Event.current.shift && !Event.current.control && !IsPaintBrush && !IsAnimationBrush)
			{
				if(esmode == EditorSculptMode.RemeshSculpt || esmode == EditorSculptMode.RemeshBeta)
				{
					if(IsDoneRealtimeRemesh && !IsDoneSmooth)
					{
						RemeshPolyFinish(currentMesh);
						IsDoneRealtimeRemesh = false;
					}
					else if(IsRealTimeAutoRemesh && IsDoneRealtimeRemesh)
					{
						RemeshPolyFinish(currentMesh);
						IsDoneRealtimeRemesh = false;
					}
				}
				switch(esmode)
				{
					case EditorSculptMode.RemeshSculpt:
						btyper = BrushModeRemesh.Smooth;
						break;
					case EditorSculptMode.Sculpt:
						btype = BrushMode.Smooth;
						break;
					case EditorSculptMode.Beta:
						btypeb = BrushModeBeta.Smooth;
						break;
					case EditorSculptMode.RemeshBeta:
						btyperb = BrushModeRemeshBeta.Smooth;
						break;
				}
				BrushString = "Smooth";
				IsShortcut = true;
				IsDoneSmooth = true;
			}

			else if(Event.current.control && !Event.current.alt)

				//else if (Event.current.control && !Event.current.alt && !Event.current.Equals(Event.KeyboardEvent(KeyCode.Z.ToString())))
			{
				switch(esmode)
				{
					case EditorSculptMode.RemeshSculpt:
						btyper = BrushModeRemesh.DrawMask;
						break;
					case EditorSculptMode.Sculpt:
						btype = BrushMode.DrawMask;
						break;
					case EditorSculptMode.Beta:
						btypeb = BrushModeBeta.DrawMask;
						break;
					case EditorSculptMode.RemeshBeta:
						btyperb = BrushModeRemeshBeta.DrawMask;
						break;
				}
				BrushString = "DrawMask";
				IsShortcut = true;
			}

			else if(Event.current.control && Event.current.alt)
			{
				switch(esmode)
				{
					case EditorSculptMode.RemeshSculpt:
						btyper = BrushModeRemesh.EraseMask;
						break;
					case EditorSculptMode.Sculpt:
						btype = BrushMode.EraseMask;
						break;
					case EditorSculptMode.Beta:
						btypeb = BrushModeBeta.EraseMask;
						break;
					case EditorSculptMode.RemeshBeta:
						btyperb = BrushModeRemeshBeta.EraseMask;
						break;
				}
				BrushString = "EraseMask";
				IsShortcut = true;
			}

			else if(Event.current.alt)
			{
				switch(esmode)
				{
					case EditorSculptMode.RemeshSculpt:
						switch(BrushStringOld)
						{
							case "Draw":
								btyper = BrushModeRemesh.Lower;
								break;
							case "Lower":
								btyper = BrushModeRemesh.Draw;
								break;
							case "Extrude":
								btyper = BrushModeRemesh.Dig;
								break;
							case "Dig":
								btyper = BrushModeRemesh.Extrude;
								break;
							case "Inflat":
								btyper = BrushModeRemesh.Pinch;
								break;
							case "Pinch":
								btyper = BrushModeRemesh.Inflat;
								break;
							case "DrawMask":
								btyper = BrushModeRemesh.EraseMask;
								break;
							case "EraseMask":
								btyper = BrushModeRemesh.DrawMask;
								break;
							case "IncreasePoly":
								btyper = BrushModeRemesh.ReducePoly;
								break;
							case "ReducePoly":
								btyper = BrushModeRemesh.IncreasePoly;
								break;
							case "VertexWeight":
								btyper = BrushModeRemesh.EraseWeight;
								break;
							case "EraseWeight":
								btyper = BrushModeRemesh.VertexWeight;
								break;
						}
						BrushString = Enum.ToObject(typeof(BrushModeRemesh), btyper).ToString();
						break;
					case EditorSculptMode.Sculpt:
						switch(BrushStringOld)
						{
							case "Draw":
								btype = BrushMode.Lower;
								break;
							case "Lower":
								btype = BrushMode.Draw;
								break;
							case "Extrude":
								btype = BrushMode.Dig;
								break;
							case "Dig":
								btype = BrushMode.Extrude;
								break;
							case "Inflat":
								btype = BrushMode.Pinch;
								break;
							case "Pinch":
								btype = BrushMode.Inflat;
								break;
							case "DrawMask":
								btype = BrushMode.EraseMask;
								break;
							case "EraseMask":
								btype = BrushMode.DrawMask;
								break;
							case "VertexWeight":
								btype = BrushMode.EraseWeight;
								break;
							case "EraseWeight":
								btype = BrushMode.VertexWeight;
								break;
						}
						BrushString = Enum.ToObject(typeof(BrushMode), btype).ToString();
						break;
					case EditorSculptMode.Beta:
						switch(BrushStringOld)
						{
							case "Draw":
								btypeb = BrushModeBeta.Lower;
								break;
							case "Lower":
								btypeb = BrushModeBeta.Draw;
								break;
							case "Extrude":
								btypeb = BrushModeBeta.Dig;
								break;
							case "Dig":
								btypeb = BrushModeBeta.Extrude;
								break;
							case "Inflat":
								btypeb = BrushModeBeta.Pinch;
								break;
							case "Pinch":
								btypeb = BrushModeBeta.Inflat;
								break;
							case "DrawMask":
								btypeb = BrushModeBeta.EraseMask;
								break;
							case "EraseMask":
								btypeb = BrushModeBeta.DrawMask;
								break;
							case "VertexWeight":
								btypeb = BrushModeBeta.EraseWeight;
								break;
							case "EraseWeight":
								btypeb = BrushModeBeta.VertexWeight;
								break;
						}
						BrushString = Enum.ToObject(typeof(BrushModeBeta), btypeb).ToString();
						break;
					case EditorSculptMode.RemeshBeta:
						switch(BrushStringOld)
						{
							case "Draw":
								btyperb = BrushModeRemeshBeta.Lower;
								break;
							case "Lower":
								btyperb = BrushModeRemeshBeta.Draw;
								break;
							case "Extrude":
								btyperb = BrushModeRemeshBeta.Dig;
								break;
							case "Dig":
								btyperb = BrushModeRemeshBeta.Extrude;
								break;
							case "Inflat":
								btyperb = BrushModeRemeshBeta.Pinch;
								break;
							case "Pinch":
								btyperb = BrushModeRemeshBeta.Inflat;
								break;
							case "DrawMask":
								btyperb = BrushModeRemeshBeta.EraseMask;
								break;
							case "EraseMask":
								btyperb = BrushModeRemeshBeta.DrawMask;
								break;
							case "IncreasePoly":
								btyperb = BrushModeRemeshBeta.ReducePoly;
								break;
							case "ReducePoly":
								btyperb = BrushModeRemeshBeta.IncreasePoly;
								break;
							case "VertexWeight":
								btyperb = BrushModeRemeshBeta.EraseWeight;
								break;
							case "EraseWeight":
								btyperb = BrushModeRemeshBeta.VertexWeight;
								break;
						}
						BrushString = Enum.ToObject(typeof(BrushModeBeta), btyperb).ToString();
						break;
				}
				IsShortcut = true;
			}

			else if(IsShortcut)
			{
				IsShortcut = false;
				switch(esmode)
				{
					case EditorSculptMode.RemeshSculpt:
						btyper = (BrushModeRemesh)Enum.Parse(typeof(BrushModeRemesh), BrushStringOld);
						break;
					case EditorSculptMode.Sculpt:
						btype = (BrushMode)Enum.Parse(typeof(BrushMode), BrushStringOld);
						break;
					case EditorSculptMode.Beta:
						btypeb = (BrushModeBeta)Enum.Parse(typeof(BrushModeBeta), BrushStringOld);
						break;
					case EditorSculptMode.RemeshBeta:
						btyperb = (BrushModeRemeshBeta)Enum.Parse(typeof(BrushModeRemeshBeta), BrushStringOld);
						break;
				}
				BrushString = BrushStringOld;
			}

			//Disabled in 2021/10/30
			if(BrushString == "BETA_Spline" && splinetype == SplineAction.DrawSpline && Event.current.button == 1 && Event.current.type == EventType.MouseDown)
			{
				if(Spline2DListList.Count > 0)
				{
					if(Spline2DListList[Spline2DListList.Count - 1].Count > 0)
					{
						Spline2DListList.Add(new List<Vector3>());
					}
				}
				if(Spline3DListList.Count > 0)
				{
					if(Spline3DListList[Spline3DListList.Count - 1].Count > 0)
					{
						Spline3DListList.Add(new List<Vector3>());
					}
				}
				if(SplineDirListList.Count > 0)
				{
					if(SplineDirListList[SplineDirListList.Count - 1].Count > 0)
					{
						SplineDirListList.Add(new List<Vector3>());
					}
				}
			}

			//End Disabled 2021/10/30

			if(Event.current.button == 1 || Event.current.button == 2)
			{
				return;
			}

			switch(Event.current.type)
			{
				case EventType.MouseDown:
					BrushHitPosList = new List<Vector3>();
					BrushHitIntList = new List<int>();
					IsInStroke = true;

					strokepos = Event.current.mousePosition;
					IsDoneSmooth = false;
					if(IsOldVertList)
					{
						oldVertList = currentMesh.vertices.ToList();
					}
					if(IsOldNormList)
					{
						CalcMeshNormals(currentMesh);
						oldNormalList = currentMesh.normals.ToList();
					}
					if(EnableUndo && BrushString == "TexturePaint")
					{
						Undo.RegisterCompleteObjectUndo(uvtexture, "TexturePaint" + Undo.GetCurrentGroup());
					}
					if(BrushString == "BETA_Spline" && EnableUndo)
					{
						Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
						foreach(LineRenderer linren in components)
						{
							if(linren.name.StartsWith("Spline"))
							{
								Undo.RegisterCompleteObjectUndo(linren, "EditorSculptCurve" + Undo.GetCurrentGroup());
							}
						}
					}
					if(BrushString == "BETA_Decal" && EnableUndo)
					{
						Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
						foreach(LineRenderer linren in components)
						{
							if(linren.name.StartsWith("Decal"))
							{
								var decalobj = linren.gameObject;
								var mat = decalobj.GetComponent<MeshRenderer>().sharedMaterial;
								var tex = mat.mainTexture;
								Undo.RegisterCompleteObjectUndo(tex, "DecalPaint" + Undo.GetCurrentGroup());
							}
						}
					}
					else if(IsAnimationBrush)
					{
						try
						{
							Undo.RegisterCompleteObjectUndo(currentObject.GetComponent<SkinnedMeshRenderer>().bones[BoneMinIdx].parent
								, "BoneMove" + Undo.GetCurrentGroup());
						}
						catch
						{
						}
					}
					else if(EnableUndo && BrushString != "TexturePaint")
					{
						Undo.RegisterCompleteObjectUndo(currentMesh, "Sculpt" + Undo.GetCurrentGroup());
					}

					//Undo.undoRedoPerformed -= UndoCallbackFunc;
					//Undo.undoRedoPerformed += UndoCallbackFunc;
					Event.current.Use();

					if(BrushString == "BETA_Spline" && splinetype == SplineAction.DrawSpline && splinepln == SplinePlane.FREE_3D)
					{
						CameraSize = Camera.current.farClipPlane;

						//Vector3 hitpos = Vector3.zero;
						var r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
						if(BrushHitPos != Vector3.zero)
						{
							//hitpos = currentObject.transform.TransformPoint(BrushHitPos);
							if(Spline3DListList.Count == 0)
							{
								Spline2DListList.Add(new List<Vector3> { r.origin });
								Spline3DListList.Add(new List<Vector3> { BrushHitPos });
								SplineDirListList.Add(new List<Vector3> { r.direction });
								Spline3DSave();
							}
							else
							{
								Spline2DListList[Spline2DListList.Count - 1].Add(r.origin);
								Spline3DListList[Spline3DListList.Count - 1].Add(BrushHitPos);
								SplineDirListList[SplineDirListList.Count - 1].Add(r.direction);
								if(Spline3DListList[Spline3DListList.Count - 1].Count == 1)
								{
									Spline3DSave();
								}
							}
							Spline3DUpdate();
						}
					}
					else if(BrushString == "BETA_Spline" && splinetype == SplineAction.DrawSpline)
					{
						CameraSize = Camera.current.farClipPlane;
						var hitpos = Vector3.zero;
						var r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
						if(BrushHitPos != Vector3.zero)
						{
							hitpos = currentObject.transform.TransformPoint(BrushHitPos);
						}
						var rv = currentObject.transform.InverseTransformPoint(r.origin);
						var rd = currentObject.transform.InverseTransformDirection(r.direction);
						rd.x /= currentObject.transform.localScale.x;
						rd.y /= currentObject.transform.localScale.y;
						rd.z /= currentObject.transform.localScale.z;
						var splineplanes = Get2DSplinePlane(false);
						var n0 = Vector3.Cross(splineplanes[2] - splineplanes[1], splineplanes[1] - splineplanes[0]).normalized;
						var v0 = splineplanes[0];
						var d0 = Mathf.Abs(Vector3.Dot(rv - v0, n0)) / Mathf.Abs(Vector3.Dot(rd, n0));
						hitpos = r.GetPoint(d0);
						if(BrushHitPos != Vector3.zero)
						{
							if(Spline3DListList.Count == 0)
							{
								Spline2DListList.Add(new List<Vector3> { hitpos });
								Spline3DListList.Add(new List<Vector3> { BrushHitPos });
								SplineDirListList.Add(new List<Vector3> { r.direction });
								Spline3DSave();
							}
							else
							{
								if(curveMode == CurveMode.Spike && Spline3DListList[Spline3DListList.Count - 1].Count < 2)
								{
									Spline2DListList[Spline2DListList.Count - 1].Add(Spline2DListList[0][0]);
									Spline3DListList[Spline3DListList.Count - 1].Add(Spline3DListList[0][0]);
									SplineDirListList[SplineDirListList.Count - 1].Add(SplineDirListList[0][0]);
									Spline3DSave();
								}
								Spline2DListList[Spline2DListList.Count - 1].Add(hitpos);
								Spline3DListList[Spline3DListList.Count - 1].Add(BrushHitPos);
								SplineDirListList[SplineDirListList.Count - 1].Add(r.direction);
								if(Spline3DListList[Spline3DListList.Count - 1].Count == 1)
								{
									Spline3DSave();
								}
							}
							Spline3DUpdate();
						}
					}

					else if(BrushString == "BETA_Spline" && splinetype == SplineAction.InsertPoint)
					{
						var mousepos = Event.current.mousePosition;
						float mindist = 0;
						var mincntcnt = 0;
						var mincnt = 0;
						var minpos = Vector2.zero;
						var minvec = Vector3.zero;
						for(var i = 0; i < Spline3DListList.Count; i++)
						{
							for(var j = 0; j < Spline3DListList[i].Count - 1; j++)
							{
								var v0 = Spline3DListList[i][j];
								v0 = currentObject.transform.TransformPoint(v0);
								var v1 = Spline3DListList[i][j + 1];
								v1 = currentObject.transform.TransformPoint(v1);
								var p0 = HandleUtility.WorldToGUIPoint(v0);
								var p1 = HandleUtility.WorldToGUIPoint(v1);
								var dp0 = Vector2.Distance(mousepos, p0);
								var dp1 = Vector2.Distance(mousepos, p1);
								var mpos = dp0 / (dp0 + dp1) * (p0 - mousepos) + dp1 / (dp0 + dp1) * (p1 - mousepos) + mousepos;
								var d0 = Vector2.Distance(mousepos, mpos);
								var dm0 = Vector2.Distance(p0, mpos);
								var dm1 = Vector2.Distance(p1, mpos);
								var mvec = (dm0 * v0 + dm1 * v1) / (dm0 + dm1);
								if(mindist == 0)
								{
									mindist = d0;
									mincntcnt = i;
									mincnt = j;
									minpos = mpos;
									minvec = mvec;
								}
								else if(d0 < mindist)
								{
									mindist = d0;
									mincntcnt = i;
									mincnt = j;
									minpos = mpos;
									minvec = mvec;
								}
							}
						}
						var r0 = HandleUtility.GUIPointToWorldRay(minpos).origin;
						var r1 = HandleUtility.GUIPointToWorldRay(mousepos).origin;
						var dr0 = Vector3.Distance(r0, r1);
						var BrushRadFix = GlobalBrushRad ? BrushRadius * HandleUtility.GetHandleSize(minvec) * 2.0f : BrushRadius;
						if(mindist != 0 && dr0 < BrushRadFix)
						{
							var SplinePList = Spline3DListList[mincntcnt];
							SplinePList.Insert(mincnt + 1, minvec);
							Spline3DListList[mincntcnt] = SplinePList;
							var Spline2DPlist = Spline2DListList[mincntcnt];
							Spline2DPlist.Insert(mincnt + 1, minvec);
							Spline2DListList[mincntcnt] = Spline2DPlist;
							var SplinePDlist = SplineDirListList[mincntcnt];
							SplinePDlist.Insert(mincnt + 1, HandleUtility.GUIPointToWorldRay(minpos).direction);
							SplineDirListList[mincntcnt] = SplinePDlist;
							Spline3DUpdate();
						}
					}

					else if(BrushString == "BETA_Spline" && splinetype == SplineAction.DeletePoint)
					{
						var mousepos = Event.current.mousePosition;
						float mindist = 0;
						var mincntcnt = 0;
						var mincnt = 0;
						var minpos = Vector2.zero;
						var minvec = Vector3.zero;
						for(var i = 0; i < Spline3DListList.Count; i++)
						{
							for(var j = 0; j < Spline3DListList[i].Count; j++)
							{
								var v0 = Spline3DListList[i][j];
								v0 = currentObject.transform.TransformPoint(v0);
								var p0 = HandleUtility.WorldToGUIPoint(v0);
								var dist = Vector3.Distance(mousepos, p0);
								if(mindist == 0)
								{
									mindist = dist;
									mincntcnt = i;
									mincnt = j;
									minpos = p0;
									minvec = v0;
								}
								else if(dist < mindist)
								{
									mindist = dist;
									mincntcnt = i;
									mincnt = j;
									minpos = p0;
									minvec = v0;
								}
							}
						}
						var r0 = HandleUtility.GUIPointToWorldRay(minpos).origin;
						var r1 = HandleUtility.GUIPointToWorldRay(mousepos).origin;
						var dr0 = Vector3.Distance(r0, r1);
						var BrushRadFix = GlobalBrushRad ? BrushRadius * HandleUtility.GetHandleSize(minvec) * 2.0f : BrushRadius;

						if(mindist != 0 && dr0 < BrushRadFix)
						{
							Spline3DListList[mincntcnt].RemoveAt(mincnt);
							if(Spline3DListList[mincntcnt].Count < 1)
							{
								Spline3DListList.RemoveAt(mincntcnt);
								Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
								var h = 0;
								foreach(LineRenderer linren in components)
								{
									if(!linren.name.StartsWith("Spline"))
									{
										continue;
									}
									var splineobj = linren.gameObject;
									if(h == mincntcnt)
									{
										DestroyImmediate(splineobj);
									}
									h++;
								}
							}
							Spline2DListList[mincntcnt].RemoveAt(mincnt);
							if(Spline2DListList[mincntcnt].Count < 1)
							{
								Spline2DListList.RemoveAt(mincntcnt);
							}
							SplineDirListList[mincntcnt].RemoveAt(mincnt);
							if(SplineDirListList[mincntcnt].Count < 1)
							{
								SplineDirListList.RemoveAt(mincntcnt);
							}
							Spline3DUpdate();
						}
					}

					else if(BrushString == "BETA_DecalSpline")
					{
						Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
						foreach(LineRenderer linren in components)
						{
							if(!linren.name.StartsWith("Decal"))
							{
								continue;
							}
							var decalobj = linren.gameObject;

							var decalmeshf = decalobj.GetComponent<MeshFilter>();
							var decalmesh = new Mesh();
							if(decalmeshf)
							{
								decalmesh = decalmeshf.sharedMesh;
							}

							var r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
							var rv = r.origin;
							var rd = currentObject.transform.InverseTransformDirection(r.direction);
							rd.x /= currentObject.transform.localScale.x;
							rd.y /= currentObject.transform.localScale.y;
							rd.z /= currentObject.transform.localScale.z;
							var minpos = Vector2.zero;
							var decalren = decalobj.GetComponent<MeshRenderer>();
							var decalsptex = (Texture2D)decalren.sharedMaterial.mainTexture;

							//Texture2D decalsptex = buildd.TextureList[DecalLayerInt];
							if(!decalsptex)
							{
								continue;
							}
							if(decalmesh)
							{
								var decalvarr = decalmesh.vertices;
								var decaltarr = decalmesh.triangles;
								var decaluvs = decalmesh.uv;
								var hitpos = Vector3.zero;
								var hitint = 0;
								for(var i = 0; i < decaltarr.Length; i += 3)
								{
									var p0 = decalvarr[decaltarr[i]];
									var p1 = decalvarr[decaltarr[i + 1]];
									var p2 = decalvarr[decaltarr[i + 2]];
									var norm = Vector3.Cross(p1 - p0, p2 - p0).normalized;
									var dot0 = Vector3.Dot(p0 - rv, norm);
									var dot1 = Vector3.Dot(rv + rd * CameraSize * 10.0f, norm);
									var tlen = dot0 / dot1;
									if(dot1 >= -0.0001f)
									{
										continue;
									}
									var hitp = rv + tlen * (rv + rd * CameraSize * 10.0f);
									var norm0 = Vector3.Cross(p1 - p0, hitp - p0);
									if(Vector3.Dot(norm, norm0) < 0)
									{
										continue;
									}
									var norm1 = Vector3.Cross(p2 - p1, hitp - p1);
									if(Vector3.Dot(norm, norm1) < 0)
									{
										continue;
									}
									var norm2 = Vector3.Cross(p0 - p2, hitp - p2);
									if(Vector3.Dot(norm, norm2) < 0)
									{
										continue;
									}
									hitpos = hitp;
									hitint = i;
								}
								var v0 = decalvarr[decaltarr[hitint]];
								var v1 = decalvarr[decaltarr[hitint + 1]];
								var v2 = decalvarr[decaltarr[hitint + 2]];
								var uv0 = decaluvs[decaltarr[hitint]];
								uv0.x *= decalsptex.width;
								uv0.y *= decalsptex.height;
								var uv1 = decaluvs[decaltarr[hitint + 1]];
								uv1.x *= decalsptex.width;
								uv1.y *= decalsptex.height;
								var uv2 = decaluvs[decaltarr[hitint + 2]];
								uv2.x *= decalsptex.width;
								uv2.y *= decalsptex.height;
								var uxmin = (int)Mathf.Min(uv0.x, uv1.x, uv2.x);
								var uxmax = (int)Mathf.Max(uv0.x, uv1.x, uv2.x);
								var uymin = (int)Mathf.Min(uv0.y, uv1.y, uv2.y);
								var uymax = (int)Mathf.Max(uv0.y, uv1.y, uv2.y);
								var uvc = (uv0 + uv1 + uv2) / 3;
								var tu0 = (uv0.y - uv1.y) / (uv0.x - uv1.x);
								var tu1 = (uv1.y - uv2.y) / (uv1.x - uv2.x);
								var tu2 = (uv0.y - uv2.y) / (uv0.x - uv2.x);
								bool uflag0 = false, uflag1 = false, uflag2 = false;
								if(uvc.y > (uvc.x - uv1.x) * tu0 + uv1.y)
								{
									uflag0 = true;
								}
								if(uvc.y > (uvc.x - uv2.x) * tu1 + uv2.y)
								{
									uflag1 = true;
								}
								if(uvc.y > (uvc.x - uv2.x) * tu2 + uv2.y)
								{
									uflag2 = true;
								}
								var mindist = -1.0f;
								for(var i = uymin; i < uymax + 1; i++)
								{
									for(var j = uxmin; j < uxmax + 1; j++)
									{
										var up0 = new Vector2(j, i);

										if(uflag0 && up0.y < (up0.x - uv1.x) * tu0 + uv1.y)
										{
											continue;
										}
										if(!uflag0 && up0.y > (up0.x - uv1.x) * tu0 + uv1.y)
										{
											continue;
										}
										if(uflag1 && up0.y < (up0.x - uv2.x) * tu1 + uv2.y)
										{
											continue;
										}
										if(!uflag1 && up0.y > (up0.x - uv2.x) * tu1 + uv2.y)
										{
											continue;
										}
										if(uflag2 && up0.y < (up0.x - uv2.x) * tu2 + uv2.y)
										{
											continue;
										}
										if(!uflag2 && up0.y > (up0.x - uv2.x) * tu2 + uv2.y)
										{
											continue;
										}

										Vector2 uc0, uc1, u2u1, c1c0, c0u1, ua0, c0c1, u0u1, c1u1, ub0;
										float ta0, tb0, tc0;
										var dist = -1.0f;
										Vector3 va, vb;
										var p0 = Vector3.zero;
										uc0 = up0 + (uv2 - uv0);
										uc1 = up0 + (uv0 - uv2);
										u2u1 = uv2 - uv1;
										c1c0 = uc1 - uc0;
										c0u1 = uc0 - uv1;
										var fc0 = c1c0.x * c0u1.y - c1c0.y * c0u1.x;
										var fc1 = c1c0.x * u2u1.y - c1c0.y * u2u1.x;
										ua0 = uv1 + u2u1 * fc0 / fc1;
										u0u1 = uv0 - uv1;
										c0c1 = uc0 - uc1;
										c1u1 = uc1 - uv1;
										var fc2 = c0c1.x * c1u1.y - c0c1.y * c1u1.x;
										var fc3 = c0c1.x * u0u1.y - c0c1.y * u0u1.x;
										ub0 = uv1 + u0u1 * fc2 / fc3;
										if(fc0 == 0.0f || fc2 == 0.0f)
										{
											continue;
										}
										ta0 = Vector2.Distance(uv1, ua0) / Vector2.Distance(uv1, uv2);
										tb0 = Vector2.Distance(uv1, ub0) / Vector2.Distance(uv1, uv0);
										tc0 = Vector2.Distance(ua0, up0) / Vector2.Distance(ua0, ub0);
										va = v1 + ta0 * (v2 - v1);
										vb = v1 + tb0 * (v0 - v1);
										p0 = va + tc0 * (vb - va);
										p0 = currentObject.transform.TransformPoint(p0);
										dist = GetStroke(p0, currentObject.transform.TransformPoint(hitpos));
										if(mindist == -1.0f || dist < mindist)
										{
											mindist = dist;
											minpos = up0;
										}
									}
								}
							}
							if(minpos != Vector2.zero)
							{
								minpos.x /= decalsptex.width;
								minpos.y /= decalsptex.height;
							}
							Component[] linecomp = decalobj.GetComponentsInChildren<LineRenderer>();
							var IsDecalSpline = false;
							foreach(LineRenderer linr in linecomp)
							{
								if(linr.name == "DecSpline")
								{
									IsDecalSpline = true;
								}
							}

							//if (linecomp.Length < 1)
							if(!IsDecalSpline)
							{
								var decalchd = new GameObject { name = "DecSpline" };
								GameObjectUtility.SetParentAndAlign(decalchd, decalobj);
								var linerd = ObjectFactory.AddComponent<LineRenderer>(decalchd);
								linerd.positionCount = 1;
								linerd.SetPosition(0, BrushHitPos);
								linerd.enabled = false;

								var decalchu = new GameObject { name = "UVDecSpline" };
								GameObjectUtility.SetParentAndAlign(decalchu, decalobj);
								var lineru = ObjectFactory.AddComponent<LineRenderer>(decalchu);
								lineru.positionCount = 1;
								lineru.SetPosition(0, minpos);
								lineru.enabled = false;
							}
							else
							{
								foreach(LineRenderer liner in linecomp)
								{
									var lineobj = liner.gameObject;
									if(lineobj.name == "DecSpline")
									{
										var lineponts = new Vector3[liner.positionCount];
										liner.positionCount = liner.GetPositions(lineponts);
										var lineplist = new List<Vector3>();
										for(var i = 0; i < liner.positionCount; i++)
										{
											lineplist.Add(liner.GetPosition(i));
										}
										lineplist.Add(BrushHitPos);
										liner.positionCount = lineplist.Count;
										liner.SetPositions(lineplist.ToArray());

										//break;
									}
									else if(lineobj.name == "UVDecSpline")
									{
										var lineuvs = new Vector3[liner.positionCount];
										liner.positionCount = liner.GetPositions(lineuvs);
										var lineuvlist = new List<Vector3>();
										for(var i = 0; i < liner.positionCount; i++)
										{
											lineuvlist.Add(liner.GetPosition(i));
										}
										lineuvlist.Add(minpos);
										liner.positionCount = lineuvlist.Count;
										liner.SetPositions(lineuvlist.ToArray());

										//break;

										for(var i = 0; i < decalsptex.height; i++)
										{
											for(var j = 0; j < decalsptex.width; j++)
											{
												decalsptex.SetPixel(j, i, Color.white);
											}
										}

										for(var i = 0; i < lineuvlist.Count; i++)
										{
											Vector2 uvp0 = lineuvlist[i];
											var uvp1 = Vector2.zero;
											if(i >= lineuvlist.Count - 1)
											{
												uvp1 = lineuvlist[0];
											}
											else
											{
												uvp1 = lineuvlist[i + 1];
											}
											var ip0x = (int)(uvp0.x * decalsptex.width);
											var ip0y = (int)(uvp0.y * decalsptex.height);
											var ip1x = (int)(uvp1.x * decalsptex.width);
											var ip1y = (int)(uvp1.y * decalsptex.height);

											//float f0 = (uvp1.y - uvp0.y) / (uvp1.x - uvp0.x);
											if(ip0x == ip1x && ip0y == ip1y)
											{
												decalsptex.SetPixel(ip0x, ip0y, BrushColor);
											}
											else if(ip0x == ip1x && ip0y > ip1y)
											{
												for(var j = ip1y; j < ip0y; j++)
												{
													decalsptex.SetPixel(ip0x, j, BrushColor);
												}
											}
											else if(ip0x == ip1x && ip0y < ip1y)
											{
												for(var j = ip0y; j < ip1y; j++)
												{
													decalsptex.SetPixel(ip0x, j, BrushColor);
												}
											}
											else if(ip0x > ip1x && ip0y == ip1y)
											{
												for(var k = ip1x; k < ip0x; k++)
												{
													decalsptex.SetPixel(k, ip0y, BrushColor);
												}
											}
											else if(ip0x < ip1x && ip0y == ip1y)
											{
												for(var k = ip0x; k < ip1x; k++)
												{
													decalsptex.SetPixel(k, ip0y, BrushColor);
												}
											}

											else if(ip0x > ip1x && ip0y > ip1y)
											{
												var f0 = (uvp0.y - uvp1.y) / (uvp0.x - uvp1.x);
												for(var j = ip1y; j < ip0y; j++)
												{
													for(var k = ip1x; k < ip0x; k++)
													{
														var ic0 = (int)(f0 * (k - (float)ip1x) + ip1y);
														var ic1 = (int)((k - (float)ip1y) / f0 + ip1x);
														if(Vector2.Distance(new Vector2(k, ic0), new Vector2(k, j)) < 2.0f)

															//if (j == (int)((f0 * ((float)k - (float)ip1x)) + (float)ip1y))
														{
															if(Vector2.Distance(new Vector2(ic1, j), new Vector2(k, j)) < 2.0f)
															{
																decalsptex.SetPixel(k, j, BrushColor);
															}
														}
													}
												}
											}
											else if(ip0x < ip1x && ip0y > ip1y)
											{
												var f0 = (uvp1.y - uvp0.y) / (uvp1.x - uvp0.x);
												for(var j = ip0y; j > ip1y; j--)
												{
													for(var k = ip0x; k < ip1x; k++)
													{
														var ic0 = (int)(f0 * (k - (float)ip0x) + ip0y);
														if(Vector2.Distance(new Vector2(k, ic0), new Vector2(k, j)) < 2.0f)

															//if (j == (int)((f0 * ((float)k - (float)ip0x)) + (float)ip0y))
														{
															decalsptex.SetPixel(k, j, BrushColor);
														}
													}
												}
											}
											else if(ip0x > ip1x && ip0y < ip1y)
											{
												var f0 = (uvp0.y - uvp1.y) / (uvp0.x - uvp1.x);
												for(var j = ip1y; j > ip0y; j--)
												{
													for(var k = ip1x; k < ip0x; k++)
													{
														var ic0 = (int)(f0 * (k - (float)ip1x) + ip1y);
														if(Vector2.Distance(new Vector2(k, ic0), new Vector2(k, j)) < 2.0f)

															//if (j == (int)((f0 * ((float)k - (float)ip1x)) + (float)ip1y))
														{
															decalsptex.SetPixel(k, j, BrushColor);
														}
													}
												}
											}
											else if(ip0x < ip1x && ip0y < ip1y)
											{
												var f0 = (uvp1.y - uvp0.y) / (uvp1.x - uvp0.x);
												for(var j = ip0y; j < ip1y; j++)
												{
													for(var k = ip0x; k < ip1x; k++)
													{
														var ic0 = (int)(f0 * (k - (float)ip0x) + ip0y);
														if(Vector2.Distance(new Vector2(k, ic0), new Vector2(k, j)) < 2.0f)

															//if (j == (int)((f0 * ((float)k - (float)ip0x)) + (float)ip0y))
														{
															decalsptex.SetPixel(k, j, BrushColor);
														}
													}
												}
											}
										}
										decalsptex.Apply();
									}
								}
							}

							//break;
						}
					}
					else if(BrushString == "Flatten")
					{
						var hitp = currentObject.transform.TransformPoint(BrushHitPos);
						if(hitp != Vector3.zero)
						{
							strokePlane.SetNormalAndPosition(-HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).direction.normalized, hitp);
						}
					}

					else if(BrushString == "BETA_Cut")
					{
						CutLineList.Add(Event.current.mousePosition);
						if(CutLineList.Count > 1)
						{
							float width = Camera.current.pixelWidth;
							float height = Camera.current.pixelHeight;
							var p0 = CutLineList[0];
							var p1 = CutLineList[1];
							strokePrePos = (p0 - p1).normalized * (width + height) + p0;

							var p2 = CutLineList[CutLineList.Count - 1];
							var p3 = CutLineList[CutLineList.Count - 2];
							strokePostPos = (p2 - p3).normalized * (width + height) + p2;

							var bcent = currentMesh.bounds.center;
							bcent = currentObject.transform.TransformPoint(bcent);
							var bcent2 = HandleUtility.WorldToGUIPoint(bcent);
							var closepos = Vector2.zero;
							for(var i = 0; i < CutLineList.Count - 1; i++)
							{
								var d0 = Vector2.Distance(bcent2, CutLineList[i]);
								var d1 = Vector2.Distance(bcent2, closepos);
								if(d0 < d1 || closepos == Vector2.zero)
								{
									closepos = CutLineList[i];
								}
							}
							CloseLinePos = closepos;
						}
					}

					else if(BrushString == "BETA_Decal" && DecalString == "Create New Decal")
					{
						CameraSize = Camera.current.farClipPlane;
						if(BrushHitPos != Vector3.zero)
						{
							strokePrePos = Event.current.mousePosition;
							strokePostPos = Vector2.zero;
							strokePreVec = BrushHitPos;
						}
						else
						{
							strokePrePos = Vector2.zero;
							strokePostPos = Vector2.zero;
							strokePreVec = BrushHitPos;
						}
					}
					if(IsPreviewAnimation && !IsAnimationBrush)
					{
						AnimationStop();
					}

					break;

				case EventType.MouseUp:

					IsInStroke = false;

					if(BrushString == "ReducePoly")
					{
						currentStatus = SculptStatus.Active;
						DecimateMesh(currentMesh);
						if(AutoCloseHole)
						{
							CloseHole(currentMesh);
						}
						if(AutoFixBlackPoly)
						{
							FixBlackPoly(currentMesh);
						}
						MergeVerts(currentMesh);

						//mergedtris = currentMesh.GetTriangles(0);
						MergetriGenerate(currentMesh);
						CatmullClarkMerged(currentMesh, 0.5f);
					}

					else if(BrushString == "IncreasePoly")
					{
						currentStatus = SculptStatus.Active;
						SubdivideMesh(currentMesh);
						if(AutoCloseHole)
						{
							CloseHole(currentMesh);
						}
						if(AutoFixBlackPoly)
						{
							FixBlackPoly(currentMesh);
						}
						MergeVerts(currentMesh);

						//mergedtris = currentMesh.GetTriangles(0);
						MergetriGenerate(currentMesh);
						CatmullClarkMerged(currentMesh, 0.5f);
					}

					else if(BrushString == "Erase")
					{
						currentStatus = SculptStatus.Active;
						DecimateMesh(currentMesh);
						if(AutoCloseHole)
						{
							CloseHole(currentMesh);
						}
						if(AutoFixBlackPoly)
						{
							FixBlackPoly(currentMesh);
						}
						MergeVerts(currentMesh);

						//mergedtris = currentMesh.GetTriangles(0);
						MergetriGenerate(currentMesh);
					}

					else if(BrushString == "BETA_Repair")
					{
						currentStatus = SculptStatus.Active;
						CloseHole(currentMesh);
						FixBlackPoly(currentMesh);
						MergeVertsFast(currentMesh);

						//mergedtris = currentMesh.GetTriangles(0);
						MergetriGenerate(currentMesh);
					}

					else if(BrushString == "BETA_Decal" && DecalString == "Create New Decal")
					{
						DecalCreate();
					}
					else if((esmode == EditorSculptMode.RemeshSculpt || esmode == EditorSculptMode.RemeshBeta) && !IsPaintBrush && !IsAnimationBrush)
					{
						if(IsDoneRealtimeRemesh && BrushString != "Smooth" && !IsDoneSmooth)
						{
							RemeshPolyFinish(currentMesh);
							IsDoneRealtimeRemesh = false;
						}
						else if(IsRealTimeAutoRemesh && BrushString != "Smooth" && !IsDoneSmooth)
						{
							RemeshPolyFinish(currentMesh);
							IsDoneRealtimeRemesh = false;
						}
						else if(SmoothWithAutoRemesh)
						{
							RemeshPolyWithoutHoles(currentMesh);
						}
					}
					if(currentMesh != null && boneAct != BoneAction.None)
					{
						var bones = currentSkinned.bones;
						var bonepos = currentObject.transform.TransformPoint(BrushBoneHitPos * 0.5f + BrushHitPos * 0.5f);
						var BrushRadFix = GlobalBrushRad
							? BrushRadius *
							  HandleUtility.GetHandleSize(currentObject.transform.TransformPoint(BrushBoneHitPos)) * 2.0f
							: BrushRadius;
						var flag0 = true;
						for(var i = 0; i < bones.Length; i++)
						{
							var trans = bones[i];
							if(Vector3.Distance(trans.position, bonepos) < BrushRadFix * 0.5f)
							{
								flag0 = false;
								break;
							}
						}
						if(bones.Length < 1)
						{
							flag0 = true;
						}
						if(boneAct == BoneAction.Add)
						{
							if(flag0)
							{
								boneAct = BoneAction.None;
								AddBone();

								//AddBone2(currentObject.transform.TransformPoint(BrushBoneHitPos * 0.5f + BrushHitPos * 0.5f));
								GUIUtility.ExitGUI();
							}
							else
							{
								EditorUtility.DisplayDialog("Caution", "You cann't add a bone arround the another bone", "OK");
								GUIUtility.ExitGUI();
								if(IsAnimationBrush)
								{
									AnimatorSetup(false);
								}
								boneAct = BoneAction.None;
							}
						}
						if(boneAct == BoneAction.Delete)
						{
							boneAct = BoneAction.None;
							delboneidx = BoneMinIdx;

							//BoneDelete();
							if(EditorUtility.DisplayDialog("Caution", "Do you want to delete this bone?", "OK", "Cancel"))
							{
								BoneDelete();
								GUIUtility.ExitGUI();
							}
							else
							{
								boneAct = BoneAction.None;
								if(IsAnimationBrush)
								{
									AnimatorSetup(false);
								}
								EditorUtility.DisplayDialog("Caution", "You cannceled delete a bone", "OK");
								GUIUtility.ExitGUI();
							}
						}

						//if (boneAct == BoneAction.Insert)
						//{
						//    boneAct = BoneAction.None;
						//    AddBone2(currentObject.transform.TransformPoint(BrushBoneHitPos));
						//    GUIUtility.ExitGUI();
						//}
					}
					if(IsAnimationBrush)
					{
						oldanimsli = animeslider;
						if(smode != SymmetryMode.None)
						{
							AnimationHumanSymmetry();
						}
					}

					if(BrushString == "BETA_BoneSpike")
					{
						BoneSpikeSplineAdd();
					}
					if(BrushString == "VertexWeight" || BrushString == "EraseWeight")
					{
						UV42BoneWeight1(currentMesh);
					}

					if(AutoSmooth && !IsPaintBrush && !IsAnimationBrush)
					{
						if(BrushString != "Smooth")
						{
							CatmullClarkMerged(currentMesh, autoSmoothDeg);
						}
					}

					if(Spline3DListList.Count > 0)
					{
						SplineToVertList();
						Spline3DUpdate();
					}
					if(Spline3DListList.Count > 0 && (AutoSplineProjection || (BrushString == "BETA_Spline" && splinetype == SplineAction.MovePoint)))
					{
						var rv = Vector3.zero;
						var rd = Vector3.zero;
						var vertices = currentMesh.vertices;
						var triangles = currentMesh.GetTriangles(0);
						var hitspMethod = new Func<Vector3[], int[], Vector3[], int[], bool>(SplineMethod);
						var callback = new AsyncCallback(SplineCallback);
						var hitmeshMethod = new Func<Vector3[], int[], Vector3[], int[], bool>(SplineMeshMethod);
						var callback0 = new AsyncCallback(SplineMeshCallback);
						CameraSize = Camera.current.farClipPlane;

						for(var i = 0; i < Spline3DListList.Count; i++)
						{
							for(var j = 0; j < Spline3DListList[i].Count; j++)
							{
								rd = SplineDirListList[i][j];
								rd = currentObject.transform.InverseTransformDirection(rd);
								rd.x /= currentObject.transform.localScale.x;
								rd.y /= currentObject.transform.localScale.y;
								rd.z /= currentObject.transform.localScale.z;
								rv = Spline2DListList[i][j];
								rv = currentObject.transform.InverseTransformPoint(rv);
								Vector3[] rvd = { rv, rd };
								int[] ij = { i, j };
								hitspMethod.BeginInvoke(vertices, triangles, rvd, ij, callback, null);
							}
						}
						Spline3DVertListList = SplineVertListList;
						for(var i = 0; i < SplineVertListList.Count; i++)
						{
							for(var j = 0; j < SplineVertListList[i].Count; j++)
							{
								if(SplineSubdivide)
								{
									rd = SplineSubDDirListList[i][j];
								}
								else
								{
									rd = SplineDirListList[i][j];
								}
								rd = currentObject.transform.InverseTransformDirection(rd);
								rd.x /= currentObject.transform.localScale.x;
								rd.y /= currentObject.transform.localScale.y;
								rd.z /= currentObject.transform.localScale.z;
								rv = SplineVertListList[i][j];
								rv = currentObject.transform.InverseTransformPoint(rv);
								Vector3[] rvd = { rv, rd };
								int[] ij = { i, j };
								hitmeshMethod.BeginInvoke(vertices, triangles, rvd, ij, callback0, null);
							}
						}

						Spline3DUpdate();
					}
					if(Decal3DListList.Count > 0)
					{
						var decalmeshMethod = new Func<Vector3[], int[], Vector3[], int[], bool>(DecalMeshMethod);
						var callback0 = new AsyncCallback(DecalMeshCallback);
						var rv = Vector3.zero;
						var rd = Vector3.zero;
						var vertices = currentMesh.vertices;
						var triangles = currentMesh.GetTriangles(0);
						for(var i = 0; i < Decal3DListList.Count; i++)
						{
							//Vector3[] orgins = DecalOrgListList[i].ToArray();
							for(var j = 0; j < Decal3DListList[i].Count; j++)
							{
								rd = DecalDirListList[i][j];

								// //rd = Vector3.Cross(orgins[0] - orgins[1], orgins[2] - orgins[1]).normalized;
								//rd = currentObject.transform.InverseTransformDirection(rd);
								//rd.x /= currentObject.transform.localScale.x;
								//rd.y /= currentObject.transform.localScale.y;
								//rd.z /= currentObject.transform.localScale.z;
								rv = Decal2DListList[i][j];

								//rv = currentObject.transform.InverseTransformPoint(rv);
								Vector3[] rvd = { rv, rd };
								int[] ij = { i, j };
								decalmeshMethod.BeginInvoke(vertices, triangles, rvd, ij, callback0, null);
							}
						}
						DecalUpdate();
					}
					if(AutoFixSymmetry && smode != SymmetryMode.None)
					{
						if(esmode == EditorSculptMode.RemeshSculpt || esmode == EditorSculptMode.RemeshBeta)
						{
							//if (!IsPaintBrush)
							if(!IsPaintBrush && !IsAnimationBrush)
							{
								SymmetryMeshFast(currentMesh);
								MergetriGenerate(currentMesh);
							}
						}
					}
					IsDoneSmooth = false;
					if(currentMesh == null)
					{
						return;
					}
					IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
					CalcMeshNormals(currentMesh);

					//currentMesh.RecalculateNormals();
					currentMesh.RecalculateTangents();
					break;

				case EventType.MouseDrag:

					SculptDraw();
					if(BrushString != "Smooth" && !IsDoneSmooth && boneAct == BoneAction.None)
					{
						if(esmode == EditorSculptMode.RemeshBeta || esmode == EditorSculptMode.RemeshSculpt)
						{
							if(!IsPaintBrush && !IsEditBrush && !IsAnimationBrush && IsRealTimeAutoRemesh)
							{
								RemeshPolyPrev(currentMesh);
								IsDoneRealtimeRemesh = true;

								//mergedtris = currentMesh.GetTriangles(0);
								//MergetriGenerate(currentMesh);
							}
						}
					}
					if(AccurateBrush)
					{
						CalcMeshNormals(currentMesh);
					}
					HandleUtility.Repaint();

					//EditorUtility.SetDirty(currentMesh);
					break;

				case EventType.MouseMove:
					HandleUtility.Repaint();
					break;

				case EventType.Repaint:
					DrawHandle();
					CameraSize = Camera.current.farClipPlane;
					if(BrushString == "BETA_Spline" && splinepln != SplinePlane.FREE_3D)
					{
						var r2 = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
						var rv2 = currentObject.transform.InverseTransformPoint(r2.origin);
						var rd2 = currentObject.transform.InverseTransformDirection(r2.direction);
						rd2.x /= currentObject.transform.localScale.x;
						rd2.y /= currentObject.transform.localScale.y;
						rd2.z /= currentObject.transform.localScale.z;
						var splineplanes = Get2DSplinePlane(false);
						var n0 = Vector3.Cross(splineplanes[2] - splineplanes[1], splineplanes[1] - splineplanes[0]).normalized;
						var v0 = splineplanes[0];
						var d0 = Mathf.Abs(Vector3.Dot(rv2 - v0, n0)) / Mathf.Abs(Vector3.Dot(rd2, n0));
						BrushHitPos = r2.GetPoint(d0);
						BrushHitPos = currentObject.transform.InverseTransformPoint(BrushHitPos);
						BrushHitNorm = n0;
						DrawHandle();
						return;
					}

					//Changed 2022/11/23
					//if ((boneAct == BoneAction.Add || boneAct==BoneAction.Insert) && IsBrushBonePos)
					//End Changed 2022/11/23
					//if (boneAct == BoneAction.Add && IsBrushBonePos)
					//{
					//    Vector3[] vertices1 = currentMesh.vertices;
					//    List<int[]> triarrlist1 = new List<int[]>();
					//    int subcnt = currentMesh.subMeshCount;
					//    for (int i = 0; i < subcnt; i++)
					//    {
					//        triarrlist1.Add(currentMesh.GetTriangles(i));
					//    }
					//    Func<Vector3[], List<int[]>, Vector3[], bool> HitpBoneMethod2 = new Func<Vector3[], List<int[]>, Vector3[], bool>(BrushBoneHitPosMethod);
					//    AsyncCallback callbackbone = new AsyncCallback(BrushBoneHitPosCallback);
					//    HitpBoneMethod2.BeginInvoke(vertices1, triarrlist1, new Vector3[] { BrushHitPos, BrushHitNorm * 10.0f * CameraSize }, callbackbone, null);
					//}
					if(boneAct == BoneAction.Add)
					{
						var ray = new Ray();
						ray.origin = currentObject.transform.TransformPoint(BrushHitPos);
						ray.direction = BrushHitNorm * 10.0f * CameraSize;
						var rayhit = new RaycastHit();
						var rayMeshParameters = new object[]
							{ ray, currentMesh, currentObject.transform.localToWorldMatrix, null };
						var rayCastMethod = typeof(HandleUtility).GetMethod("IntersectRayMesh", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
						if((bool)rayCastMethod.Invoke(null, rayMeshParameters))
						{
							rayhit = (RaycastHit)rayMeshParameters[3];
						}
						var brushhitp = rayhit.point;
						if(brushhitp != Vector3.zero)
						{
							BrushBoneHitPos = currentObject.transform.InverseTransformPoint(brushhitp);
						}
					}
					break;

				case EventType.Layout:
					HandleUtility.AddDefaultControl(ctrlID);
					break;
			}
		}

		private void OnSelectionChange()
		{
			AnimationSaveBoneMove(animeslider, 0.0f, false);
			if(BrushString == "BETA_Spline" || BrushString == "BETA_CurveMove")
			{
				EditorSculptPrefab(false, true);
				IsSaved = true;
			}
			if(currentObject != null && currentMesh != null && !IsSaved)
			{
				if(currentObject != Selection.activeGameObject)
				{
					if(currentObject.GetComponent<SkinnedMeshRenderer>() != null || currentObject.GetComponent<MeshFilter>() != null)
					{
						if(EditorUtility.DisplayDialog("Caution", "Mesh hasn't saved yet. Do you want to save?", "OK", "Cancel"))
						{
							EditorSculptPrefab(false, false);
							IsSaved = true;
							IsMeshSaved = true;
						}
					}
				}
			}

			//if (currentObject != null && currentMesh != null && memoryMesh != null && (!IsMeshSaved) && (!IsSkipDialog))
			if(currentObject != null && currentMesh != null && ((memoryMesh != null && !IsMeshSaved) || (memoryTexBytes != null && !IsTexSaved))
			   && !IsSkipDialog)
			{
				if(EditorUtility.DisplayDialog("Caution", "Mesh hasn't saved yet. Do you want to save?", "OK", "Cancel"))
				{
				}
				else
				{
					RestoreMemoryMesh();
				}
				IsMeshSaved = true;
				IsTexSaved = true;
			}

			//if (IsDisabledAnimationImport == true) ModelImporterChangeAnimeImport(true);

			//RevertPreMetaFile();

			LineRendererMeshDelete("EditorSculptRef");

			//IsEnable = true;
			//SaveTexture(true);
			uvindex = new List<List<int>>();
			uvpos = new List<List<Vector3>>();
			uvindex = new List<List<int>>();
			uvpos = new List<List<Vector3>>();
			IsSetupUV = false;
			ReferenceImageRevertMat();
			FixMaterial();

			if(currentMesh != null && blendBaseMesh != null)
			{
				BlendShapeCreate();
			}
			AnimationQuit();
			AnimePTime = 0.0f;
			animeslider = 0.0f;
			oldanimsli = 0.0f;
			currentObject = null;
			currentMesh = null;
			currentMeshFilter = null;
			extrabones = new Vector3[] { };
			humanDict = new Dictionary<string, Transform>();
			humantrans = null;

			//oldbind = null;
			//LatestBoneIdx = -1;

			currentStatus = SculptStatus.Inactive;
			Tools.hidden = false;
			IsTexturePaint = false;
			LoadTexCnt = 0;

			if(IsAnimationBrush)
			{
				if(Application.unityVersion.StartsWith("2019"))
				{
					if(!IsSetupAnimation)
					{
						RestoreBrush();
					}
				}
				else
				{
					RestoreBrush();
				}
			}

			////Added 2025/09/13
			////Fix error of animation
			//if ((currentObject != null) && (currentMesh != null) && (oldbind != null))
			//{
			//    //if (oldbind.Length == currentMesh.bindposeCount) currentMesh.bindposes = oldbind;
			//    //currentMesh.bindposes = oldbind;
			//}
			////End added 2025/09/13

			//#if UNITY_2019
			//        if (IsAnimationBrush && !IsSetupAnimation) RestoreBrush();
			//#else
			//        if (IsAnimationBrush) RestoreBrush();
			//#endif

			boneAct = BoneAction.None;

			if(Selection.activeGameObject == null)
			{
				return;
			}
			if(Selection.activeGameObject.GetComponent<MeshFilter>() == null && Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>() == null)
			{
				return;
			}
			if(IsLoadingNow)
			{
				return;
			}

			if(!IsDontInstantiate && !Selection.activeGameObject.transform.root.gameObject.name.Contains("EditorSculpt"))
			{
				if(CheckEditorSculptGameObj(Selection.activeGameObject))
				{
					Selection.activeGameObject.transform.root.gameObject.name += "_EditorSculpt";
					IsLoadingNow = true;
					PostLoadMesh();
					IsLoadingNow = false;
					return;
				}
				startmesh = GetMeshFromGameObject(Selection.activeGameObject);
				Selection.activeGameObject = null;
				pickerid = 0;
				return;
			}
			IsLoadingNow = true;
			PostLoadMesh();
			IsLoadingNow = false;
		}

		private static void DelaySelectGameObject()
		{
			Selection.activeGameObject = startGO;
		}

		//#if UNITY_2019
		private static void DelaySelectGameObjectAnim()
		{
			Selection.activeGameObject = startGO;
			EditorApplication.delayCall -= DelayFinishAnimationSetup;
			EditorApplication.delayCall += DelayFinishAnimationSetup;
		}

		private static void DelayFinishAnimationSetup()
		{
			IsSetupAnimation = false;
		}

		//#endif

		private static void DecalCreate()
		{
			if(strokePrePos == Vector2.zero)
			{
				strokePostPos = Vector2.zero;
				return;
			}
			strokePostPos = Event.current.mousePosition;
			var dp0 = HandleUtility.GUIPointToWorldRay(strokePrePos).origin;
			var dp1 = HandleUtility.GUIPointToWorldRay(new Vector2(strokePostPos.x, strokePrePos.y)).origin;
			var dp2 = HandleUtility.GUIPointToWorldRay(strokePostPos).origin;
			var dp3 = HandleUtility.GUIPointToWorldRay(new Vector2(strokePrePos.x, strokePostPos.y)).origin;

			//Temp change 2021/06/19
			var dr0 = HandleUtility.GUIPointToWorldRay(strokePrePos);
			var dr1 = HandleUtility.GUIPointToWorldRay(new Vector2(strokePostPos.x, strokePrePos.y));
			var dr2 = HandleUtility.GUIPointToWorldRay(strokePostPos);
			var dr3 = HandleUtility.GUIPointToWorldRay(new Vector2(strokePrePos.x, strokePostPos.y));
			var drp0 = CalcHitPosMesh(currentMesh, dr0.origin, dr0.direction);
			var drp1 = CalcHitPosMesh(currentMesh, dr1.origin, dr1.direction);
			var drp2 = CalcHitPosMesh(currentMesh, dr2.origin, dr2.direction);
			var drp3 = CalcHitPosMesh(currentMesh, dr3.origin, dr3.direction);
			DecalBaseListList.Add(new List<Vector3>
			{
				drp0 - dr0.direction.normalized, drp1 - dr1.direction.normalized,
				drp3 - dr3.direction.normalized, drp2 - dr2.direction.normalized
			});
			decalrays[0] = dr0;
			decalrays[1] = dr1;
			decalrays[2] = dr3;
			decalrays[3] = dr2;

			//End Change 2021/06/19

			Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
			var flag0 = false;
			foreach(LineRenderer linren in components)
			{
				if(linren.name.StartsWith("Base"))
				{
					var lines = new Vector3[linren.positionCount];
					linren.GetPositions(lines);
					if(lines[0] == dp0 || lines[2] == dp2)
					{
						flag0 = true;
					}
				}
			}
			if(BrushHitPos == Vector3.zero)
			{
				flag0 = true;
			}
			if(!flag0)
			{
				var sx0 = Mathf.Abs(strokePostPos.x - strokePrePos.x) * 0.25f;
				var sy0 = Mathf.Abs(strokePostPos.y - strokePrePos.y) * 0.25f;
				var xmax = 4;
				var ymax = 4;
				if(sx0 < 0.01f || sy0 < 0.01f)
				{
					return;
				}
				if(Mathf.Abs(sx0) > Mathf.Abs(sy0))
				{
					var ymaxf = Mathf.Abs((strokePostPos.y - strokePrePos.y) / sx0);
					ymax = (int)ymaxf;
					sy0 = Mathf.Abs(strokePostPos.y - strokePrePos.y) / ymax;
				}
				else
				{
					var xmaxf = Mathf.Abs((strokePostPos.x - strokePrePos.x) / sy0);
					xmax = (int)xmaxf;
					sx0 = Mathf.Abs(strokePostPos.x - strokePrePos.x) / xmax;
				}
				if(strokePostPos.x < strokePrePos.x)
				{
					sx0 = -sx0;
				}
				if(strokePostPos.y < strokePrePos.y)
				{
					sy0 = -sy0;
				}
				var decalvlist = new List<Vector3>();
				var decaltlist = new List<int>();
				var decaluvlist = new List<Vector2>();
				var decaldlist = new List<Vector3>();
				for(var i = 0; i < ymax + 1; i++)
				{
					for(var j = 0; j < xmax + 1; j++)
					{
						var decalray = HandleUtility.GUIPointToWorldRay(new Vector2(strokePrePos.x + j * sx0, strokePrePos.y + i * sy0));

						var rv = Vector3.zero;
						var rd = Vector3.zero;
						rv = currentObject.transform.InverseTransformPoint(decalray.origin);
						rd = currentObject.transform.InverseTransformDirection(decalray.direction);
						rd.x /= currentObject.transform.localScale.x;
						rd.y /= currentObject.transform.localScale.y;
						rd.z /= currentObject.transform.localScale.z;
						decalvlist.Add(rv);
						decaldlist.Add(rd);
						decaluvlist.Add(new Vector2((float)j / xmax, (float)i / ymax));
					}
				}
				for(var i = 0; i < decalvlist.Count - xmax - 2; i++)
				{
					if(i % (xmax + 1) == xmax)
					{
						continue;
					}
					decaltlist.AddRange(new[] { i, i + 1, i + xmax + 1 });
					decaltlist.AddRange(new[] { i + 1, i + xmax + 2, i + xmax + 1 });
				}
				if(decaltlist.Count < 3)
				{
					strokePrePos = Vector2.zero;
					strokePostPos = Vector2.zero;
					return;
				}
				var c0 = decalvlist[decaltlist[0]];
				var c1 = decalvlist[decaltlist[1]];
				var c2 = decalvlist[decaltlist[2]];
				var cc0 = Vector3.Cross(c1 - c0, c2 - c0);
				if(Vector3.Dot(cc0, HandleUtility.GUIPointToWorldRay(strokePostPos).direction) > 0.0f)
				{
					for(var i = 0; i < decaltlist.Count; i += 3)
					{
						var i0 = decaltlist[i];
						var i2 = decaltlist[i + 2];
						decaltlist[i] = i2;
						decaltlist[i + 2] = i0;
					}
				}

				var decaluvs = new Vector2[decaluvlist.Count];
				var gparr = new Vector2[decalvlist.Count];
				float gxmin = 0, gxmax = 0, gymin = 0, gymax = 0;
				for(var i = 0; i < decalvlist.Count; i++)
				{
					var g0 = HandleUtility.WorldToGUIPoint(decalvlist[i]);
					gparr[i] = g0;
					if(g0.x < gxmin || gxmin == 0.0f)
					{
						gxmin = g0.x;
					}
					if(g0.y < gymin || gymin == 0.0f)
					{
						gymin = g0.y;
					}
					if(g0.x > gxmax || gxmax == 0.0f)
					{
						gxmax = g0.x;
					}
					if(g0.y > gymax || gymax == 0.0f)
					{
						gymax = g0.y;
					}
				}
				var dwidth = gxmax - gxmin;
				var dheight = gymax - gymin;
				for(var i = 0; i < decaluvs.Length; i++)
				{
					var ga0 = gparr[i];
					ga0.x = (ga0.x - gxmin) / dwidth;
					ga0.y = (ga0.y - gymin) / dheight;
					if(ga0.x < 0.01f)
					{
						ga0.x = 0.0f;
					}
					if(ga0.y < 0.01f)
					{
						ga0.y = 0.0f;
					}
					decaluvs[i] = ga0;
				}
				decaluvlist = decaluvs.ToList();

				if(smode != SymmetryMode.None)
				{
					for(var i = 0; i < decalvlist.Count; i++)
					{
						var v0 = decalvlist[i];
						if(smode == SymmetryMode.X_axis && v0.x < 0.01f)
						{
							v0.x = 0.0f;
						}
						if(smode == SymmetryMode.Y_axis && v0.y < 0.01f)
						{
							v0.y = 0.0f;
						}
						if(smode == SymmetryMode.Z_axis && v0.z < 0.01f)
						{
							v0.z = 0.0f;
						}
						decalvlist[i] = v0;
					}
					var newvlist = new List<Vector3>();
					var newtrilist = new List<int>();
					var uv0list = new List<Vector2>();
					var newdlist = new List<Vector3>();
					var idx = 0;
					for(var i = 0; i < decaltlist.Count; i += 3)
					{
						idx = newtrilist.Count;
						var v0 = decalvlist[decaltlist[i]];
						var v1 = decalvlist[decaltlist[i + 1]];
						var v2 = decalvlist[decaltlist[i + 2]];
						var d0 = decaldlist[decaltlist[i]];
						var d1 = decaldlist[decaltlist[i + 1]];
						var d2 = decaldlist[decaltlist[i + 2]];
						var uv0_0 = decaluvlist[decaltlist[i]];
						var uv0_1 = decaluvlist[decaltlist[i + 1]];
						var uv0_2 = decaluvlist[decaltlist[i + 2]];
						if(smode == SymmetryMode.X_axis && (v0.x < 0.0f || v1.x < 0.0f || v2.x < 0.0f))
						{
							continue;
						}
						if(smode == SymmetryMode.X_axis && v0.x == 0.0f && v1.x == 0.0f && v2.x == 0.0f)
						{
							continue;
						}
						if(smode == SymmetryMode.Y_axis && (v0.y < 0.0f || v1.y < 0.0f || v2.y < 0.0f))
						{
							continue;
						}
						if(smode == SymmetryMode.Y_axis && v0.y == 0.0f && v1.y == 0.0f && v2.y == 0.0f)
						{
							continue;
						}
						if(smode == SymmetryMode.Z_axis && (v0.z < 0.0f || v1.z < 0.0f || v2.z < 0.0f))
						{
							continue;
						}
						if(smode == SymmetryMode.Z_axis && v0.z == 0.0f && v1.z == 0.0f && v2.z == 0.0f)
						{
							continue;
						}
						newvlist.AddRange(new[] { v0, v1, v2 });
						newtrilist.AddRange(new[] { idx, idx + 1, idx + 2 });
						uv0list.AddRange(new[] { uv0_0, uv0_1, uv0_2 });
						newdlist.AddRange(new[] { d0, d1, d2 });
						newvlist.AddRange(new[] { GetSymmetry(v0), GetSymmetry(v1), GetSymmetry(v2) });
						newtrilist.AddRange(new[] { idx + 5, idx + 4, idx + 3 });
						uv0list.AddRange(new[] { uv0_0, uv0_1, uv0_2 });
						newdlist.AddRange(new[] { GetSymmetry(d0), GetSymmetry(d1), GetSymmetry(d2) });
					}
					decalvlist = newvlist;
					decaltlist = newtrilist;
					decaluvlist = uv0list;
					decaldlist = newdlist;
				}

				var decalObject = new GameObject
					{ name = GameObjectUtility.GetUniqueNameForSibling(currentObject.transform.root, "Decal") };
				GameObjectUtility.SetParentAndAlign(decalObject, currentObject);
				Undo.RegisterCreatedObjectUndo(decalObject, "Decal");
				var lineDecal = ObjectFactory.AddComponent<LineRenderer>(decalObject);
				lineDecal.useWorldSpace = false;
				lineDecal.positionCount = decalvlist.Count;
				lineDecal.startWidth = 0.0f;
				lineDecal.SetPositions(decalvlist.ToArray());

				var orgobj = new GameObject { name = "Origin" };
				GameObjectUtility.SetParentAndAlign(orgobj, decalObject);
				Undo.RegisterCreatedObjectUndo(orgobj, "DecalOrigin");
				var linr2 = new LineRenderer();
				linr2 = ObjectFactory.AddComponent<LineRenderer>(orgobj);
				linr2.useWorldSpace = false;
				linr2.positionCount = decalvlist.Count;
				linr2.SetPositions(decalvlist.ToArray());
				linr2.startWidth = 0.0f;

				var dirobj = new GameObject { name = "DirDecal" };
				GameObjectUtility.SetParentAndAlign(dirobj, decalObject);
				Undo.RegisterCreatedObjectUndo(dirobj, "DirDecal");
				var linr3 = new LineRenderer();
				linr3 = ObjectFactory.AddComponent<LineRenderer>(dirobj);
				linr3.useWorldSpace = false;
				linr3.positionCount = decaldlist.Count;
				linr3.SetPositions(decaldlist.ToArray());

				var triobj = new GameObject { name = "Triangles" };
				GameObjectUtility.SetParentAndAlign(triobj, decalObject);
				Undo.RegisterCreatedObjectUndo(triobj, "DecalTriangle");
				var linr4 = new LineRenderer();
				linr4 = ObjectFactory.AddComponent<LineRenderer>(triobj);
				linr4.useWorldSpace = false;
				linr4.positionCount = decaltlist.Count / 3;
				linr4.startWidth = 0.0f;
				var decaltflist = new List<Vector3>();
				for(var i = 0; i < decaltlist.Count; i += 3)
				{
					decaltflist.Add(new Vector3(decaltlist[i] * 0.01f, decaltlist[i + 1] * 0.01f, decaltlist[i + 2] * 0.01f));
				}
				linr4.SetPositions(decaltflist.ToArray());

				var uvobj = new GameObject { name = "UVs" };
				GameObjectUtility.SetParentAndAlign(uvobj, decalObject);
				Undo.RegisterCreatedObjectUndo(uvobj, "DecalUVs");
				var linr5 = new LineRenderer();
				linr5 = ObjectFactory.AddComponent<LineRenderer>(uvobj);
				linr5.useWorldSpace = false;
				linr5.positionCount = decaluvlist.Count;
				linr5.startWidth = 0.0f;
				var decaluvlist0 = new List<Vector3>();
				for(var i = 0; i < decaluvlist0.Count; i++)
				{
					decaluvlist0.Add(new Vector3(decaluvlist[i].x, decaluvlist[i].y, 0));
				}
				linr5.SetPositions(decaluvlist0.ToArray());

				//Added 2021/06/23 for Decal Projection UV
				var decposobj = new GameObject { name = "Base" };
				GameObjectUtility.SetParentAndAlign(decposobj, decalObject);
				Undo.RegisterCreatedObjectUndo(decposobj, "DecalBase");
				var linr6 = new LineRenderer();
				linr6 = ObjectFactory.AddComponent<LineRenderer>(decposobj);
				linr6.useWorldSpace = false;
				linr6.positionCount = 4;
				linr6.startWidth = 0.0f;
				linr6.SetPositions(new[]
				{
					drp0 - dr0.direction.normalized, drp1 - dr1.direction.normalized,
					drp3 - dr3.direction.normalized, drp2 - dr2.direction.normalized
				});

				//linr6.SetPositions(new Vector3[] { drp0, drp1, drp2, drp3 });
				//End Added 2021/06/23

				var decalmeshf = ObjectFactory.AddComponent<MeshFilter>(decalObject);
				decalmeshf.mesh = new Mesh();
				var decalmesh = decalmeshf.sharedMesh;
				decalmesh.Clear();
				decalmesh.vertices = decalvlist.ToArray();
				decalmesh.triangles = decaltlist.ToArray();
				decalmesh.uv = decaluvlist.ToArray();

				var decalrenderer = ObjectFactory.AddComponent<MeshRenderer>(decalObject);
				var mat0 = new Material(Shader.Find("Custom/DecalTexture"));
				var NewTex = new Texture2D((int)Mathf.Abs(strokePostPos.x - strokePrePos.x), (int)Mathf.Abs(strokePostPos.y - strokePrePos.y));
				NewTex.name = "DecalTexture";
				var y = 0;
				while(y < NewTex.height)
				{
					var x = 0;
					while(x < NewTex.width)
					{
						var col = Color.white;

						//col.a = 0.0f;
						NewTex.SetPixel(x, y, col);
						++x;
					}
					++y;
				}
				NewTex.Apply();
				mat0.SetTexture("_MainTex", NewTex);

				//String matpath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + "Decal" + ".mat");
				var matpath = AssetDatabase.GenerateUniqueAssetPath(SaveFolderPath + currentObject.name + "Decal" + ".mat");
				AssetDatabase.CreateAsset(mat0, matpath);
				AssetDatabase.AddObjectToAsset(NewTex, matpath);
				AssetDatabase.SaveAssets();
				AssetDatabase.ImportAsset(matpath);
				decalrenderer.sharedMaterial = mat0;
				mat0.mainTexture = NewTex;

				Decal2DListList.Add(decalvlist);
				DecalDirListList.Add(decaldlist);
				Decal3DListList.Add(decalvlist);
				DecalTriListList.Add(decalmesh.triangles.ToList());
				DecalUVListList.Add(decalmesh.uv.ToList());
				DecalUpdate();
				EditorSculptPrefab(false, true);
			}
			strokePrePos = Vector3.zero;
			strokePostPos = Vector3.zero;
			strokePreVec = Vector3.zero;
		}

		private static Vector3[] ReferenceImgGetPos(LineRenderer linren)
		{
			Material mat = null;
			var referplane = refplane;
			if(linren != null)
			{
				var meshren = linren.gameObject.GetComponent<MeshRenderer>();
				mat = meshren.sharedMaterial;
			}
			if(mat != null)
			{
				referplane = (ReferenceImagePlane)mat.GetInt("_DecalAxis");
			}
			var reflist = new List<Vector3>();
			if(currentMesh == null || currentObject == null)
			{
				return reflist.ToArray();
			}
			Vector3 r0 = Vector3.zero, r1 = Vector3.zero, r2 = Vector3.zero, r3 = Vector3.zero;
			Vector3 centpos = Vector3.zero, minpos = Vector3.zero, maxpos = Vector3.zero;
			if(mat != null)
			{
				minpos = mat.GetVector("_DecalPos");
				maxpos = mat.GetVector("_DecalPos2");
			}
			if(minpos == Vector3.zero && maxpos == Vector3.zero)
			{
				minpos = currentMesh.bounds.min;
				maxpos = currentMesh.bounds.max;
				centpos = currentMesh.bounds.center;
				if(mat != null)
				{
					mat.SetVector("_DecalPos", minpos);
					mat.SetVector("_DecalPos2", maxpos);
				}
			}
			else
			{
				centpos = minpos * 0.5f + maxpos * 0.5f;
			}

			//float minf = Mathf.Max(Mathf.Max(minpos.x, minpos.y), minpos.z);
			var maxf = Mathf.Max(Mathf.Max(maxpos.x, maxpos.y), maxpos.z) * refImgSize;
			var minx = (minpos.x - centpos.x) * 1.2f;
			var miny = (minpos.y - centpos.y) * 1.2f;
			var minz = (minpos.z - centpos.z) * 1.2f;
			var maxx = (maxpos.x - centpos.x) * 1.2f;
			var maxy = (maxpos.y - centpos.y) * 1.2f;
			var maxz = (maxpos.z - centpos.z) * 1.2f;
			switch(referplane)
			{
				case ReferenceImagePlane.XY_Plane_Forward:
					r0.x = minx;
					r0.y = miny;
					r0.z = refimgCenter ? centpos.z : maxz;
					r1.x = minx;
					r1.y = maxy;
					r1.z = refimgCenter ? centpos.z : maxz;
					r2.x = maxx;
					r2.y = miny;
					r2.z = refimgCenter ? centpos.z : maxz;
					r3.x = maxx;
					r3.y = maxy;
					r3.z = refimgCenter ? centpos.z : maxz;
					break;
				case ReferenceImagePlane.XY_Plane_Back:
					r0.x = maxx;
					r0.y = miny;
					r0.z = refimgCenter ? centpos.z : minz;
					r1.x = maxx;
					r1.y = maxy;
					r1.z = refimgCenter ? centpos.z : minz;
					r2.x = minx;
					r2.y = miny;
					r2.z = refimgCenter ? centpos.z : minz;
					r3.x = minx;
					r3.y = maxy;
					r3.z = refimgCenter ? centpos.z : minz;
					break;
				case ReferenceImagePlane.YZ_Plane_Left:
					r0.x = refimgCenter ? centpos.x : minx;
					r0.y = miny;
					r0.z = minz;
					r1.x = refimgCenter ? centpos.x : minx;
					r1.y = maxy;
					r1.z = minz;
					r2.x = refimgCenter ? centpos.x : minx;
					r2.y = miny;
					r2.z = maxz;
					r3.x = refimgCenter ? centpos.x : minx;
					r3.y = maxy;
					r3.z = maxz;
					break;
				case ReferenceImagePlane.YZ_Plane_Right:
					r0.x = refimgCenter ? centpos.x : maxx;
					r0.y = miny;
					r0.z = maxz;
					r1.x = refimgCenter ? centpos.x : maxx;
					r1.y = maxy;
					r1.z = maxz;
					r2.x = refimgCenter ? centpos.x : maxx;
					r2.y = miny;
					r2.z = minz;
					r3.x = refimgCenter ? centpos.x : maxx;
					r3.y = maxy;
					r3.z = minz;
					break;
				case ReferenceImagePlane.ZX_Plane_Up:
					r0.x = minx;
					r0.y = refimgCenter ? centpos.y : maxy;
					r0.z = minz;
					r1.x = maxx;
					r1.y = refimgCenter ? centpos.y : maxy;
					r1.z = minz;
					r2.x = minx;
					r2.y = refimgCenter ? centpos.y : maxy;
					r2.z = maxz;
					r3.x = maxx;
					r3.y = refimgCenter ? centpos.y : maxy;
					r3.z = maxz;
					break;
				/*case ReferenceImagePlane.ZX_Plane_Down:
				    r0.x = minx; r0.y = refimgCenter ? centpos.y : miny; r0.z = minz;
				    r1.x = minx; r1.y = refimgCenter ? centpos.y : miny; r1.z = maxz;
				    r2.x = maxx; r2.y = refimgCenter ? centpos.y : miny; r2.z = minz;
				    r3.x = maxx; r3.y = refimgCenter ? centpos.y : miny; r3.z = maxz;
				    break;
				    */
				case ReferenceImagePlane.ZX_Plane_Down:
					r0.x = minx;
					r0.y = refimgCenter ? centpos.y : miny;
					r0.z = maxz;
					r1.x = maxx;
					r1.y = refimgCenter ? centpos.y : miny;
					r1.z = maxz;
					r2.x = minx;
					r2.y = refimgCenter ? centpos.y : miny;
					r2.z = minz;
					r3.x = maxx;
					r3.y = refimgCenter ? centpos.y : miny;
					r3.z = minz;
					break;
			}
			var f0 = Vector3.Distance(r0, r1) / Vector3.Distance(r0, r2);
			var rm0 = (r0 + r2) * 0.5f;
			var rm1 = (r1 + r3) * 0.5f;

			r0 = rm0 + (r0 - rm0).normalized * maxf;
			r1 = rm1 + (r1 - rm1).normalized * maxf;
			r2 = rm0 + (r2 - rm0).normalized * maxf;
			r3 = rm1 + (r3 - rm1).normalized * maxf;

			var rm2 = (r0 + r1) * 0.5f;
			var rm3 = (r2 + r3) * 0.5f;

			//r0 = rm2 + (r0 - rm2).normalized * maxf;
			//r1 = rm2 + (r1 - rm2).normalized * maxf;
			//r2 = rm3 + (r2 - rm3).normalized * maxf;
			//r3 = rm3 + (r3 - rm3).normalized * maxf;

			r0 = rm2 + (r0 - rm2).normalized * maxf + refImgPos;
			r1 = rm2 + (r1 - rm2).normalized * maxf + refImgPos;
			r2 = rm3 + (r2 - rm3).normalized * maxf + refImgPos;
			r3 = rm3 + (r3 - rm3).normalized * maxf + refImgPos;

			reflist.AddRange(new[] { r0, r1, r2, r3 });
			return reflist.ToArray();
		}

		private static string ReferenceImageCreate()
		{
			refImgOffset = Vector2.zero;
			refImgPos = Vector3.zero;
			refImgScale = Vector2.one;
			refImgSize = 1.0f;

			var ru0 = new Vector2(0.0f, 0.0f);
			var ru1 = new Vector2(0.0f, 1.0f);
			var ru2 = new Vector2(1.0f, 0.0f);
			var ru3 = new Vector2(1.0f, 1.0f);
			var rarr = ReferenceImgGetPos(null);
			if(rarr.Length < 1)
			{
				return "";
			}

			var refvlist = new List<Vector3>();
			refvlist.AddRange(new[] { rarr[0], rarr[1], rarr[2], rarr[3] });
			var reftlist = new List<int>();
			reftlist.AddRange(new[] { 0, 1, 2, 1, 3, 2 });
			var refuvlist = new List<Vector2>();
			refuvlist.AddRange(new[] { ru0, ru1, ru2, ru3 });

			var refimgobj = new GameObject
				{ name = GameObjectUtility.GetUniqueNameForSibling(currentObject.transform, "EditorSculptRef") };
			GameObjectUtility.SetParentAndAlign(refimgobj, currentObject);
			Undo.RegisterCreatedObjectUndo(refimgobj, "Reference Image");
			var lineref = ObjectFactory.AddComponent<LineRenderer>(refimgobj);
			lineref.useWorldSpace = false;
			lineref.positionCount = refvlist.Count;
			lineref.startWidth = 0.0f;
			lineref.SetPositions(refvlist.ToArray());

			var refmeshf = ObjectFactory.AddComponent<MeshFilter>(refimgobj);
			refmeshf.mesh = new Mesh();
			var refmesh = refmeshf.sharedMesh;
			refmesh.Clear();
			refmesh.vertices = refvlist.ToArray();
			refmesh.triangles = reftlist.ToArray();
			refmesh.uv = refuvlist.ToArray();

			var refrenderer = ObjectFactory.AddComponent<MeshRenderer>(refimgobj);
			var mat0 = new Material(Shader.Find("Custom/DecalTexture"));
			var NewTex = new Texture2D(512, 512);
			NewTex.name = "Reference Image Texture";
			mat0.SetTexture("_MainTex", NewTex);
			mat0.renderQueue = 2000;

			mat0.mainTextureOffset = new Vector2(refImgOffset.x, refImgOffset.y);
			mat0.SetInt("_DecalAxis", (int)refplane);
			mat0.mainTextureScale = new Vector2(refImgScale.x, refImgScale.y);
			mat0.SetVector("_DecalPos", new Vector4(refImgPos.x, refImgPos.y, refImgPos.z, refImgSize));

			//String matpath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + "Reference" + ".mat");
			var matpath = AssetDatabase.GenerateUniqueAssetPath(SaveFolderPath + currentObject.name + "Reference" + ".mat");
			AssetDatabase.CreateAsset(mat0, matpath);
			AssetDatabase.AddObjectToAsset(NewTex, matpath);
			AssetDatabase.SaveAssets();

			AssetDatabase.ImportAsset(matpath);
			refrenderer.sharedMaterial = mat0;
			mat0.mainTexture = NewTex;
			EditorSculptPrefab(false, true);

			//EditorSculptPrefab2(true);
			//Added 2022/11/26
			LoadTexture(true);

			//SaveTexture();
			SaveTexture();
			ChangeMaterial();

			//End Added 2022/11/26
			oldrefname = refimgobj.name;
			return refimgobj.name;
		}

		private static void ReferenceTextureLoad(string loadpath, string refname)
		{
			try
			{
				var bytes = File.ReadAllBytes(loadpath);
				Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
				foreach(LineRenderer linren in components)
				{
					if(linren.name != refname)
					{
						continue;
					}
					var lineobj = linren.gameObject;
					var meshren = lineobj.GetComponent<MeshRenderer>();
					var mat0 = meshren.sharedMaterial;
					var reftex = (Texture2D)mat0.mainTexture;
					reftex.LoadImage(bytes);

					//reftex = Texture2DFixAspect(reftex, Mathf.Max(reftex.width,reftex.height), Mathf.Max(reftex.width, reftex.height));
					//mat0.mainTexture = reftex;
				}
			}
			catch
			{
				Debug.Log("LoadReferenceFailed!");
			}
		}

		private static void ReferenceImgUpdate(string refname)
		{
			if(!currentObject)
			{
				return;
			}
			Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
			foreach(LineRenderer linren in components)
			{
				if(linren.name != refname)
				{
					continue;
				}
				var lineobj = linren.gameObject;
				var rarr = ReferenceImgGetPos(linren);
				if(rarr.Length < 1)
				{
					return;
				}
				var ru0 = new Vector2(0.0f, 0.0f);
				var ru1 = new Vector2(0.0f, 1.0f);
				var ru2 = new Vector2(1.0f, 0.0f);
				var ru3 = new Vector2(1.0f, 1.0f);
				Vector2 oldru0, oldru1, oldru2, oldru3;
				if(refImgDeform == (refImgDeform | RefImgDeform.RotateU))
				{
					oldru1 = ru2;
					oldru2 = ru1;
					ru1 = oldru1;
					ru2 = oldru2;
				}
				if(refImgDeform == (refImgDeform | RefImgDeform.RotateV))
				{
					oldru0 = ru3;
					oldru3 = ru0;
					ru0 = oldru0;
					ru3 = oldru3;
				}
				if(refImgDeform == (refImgDeform | RefImgDeform.InvertU))
				{
					oldru0 = ru2;
					oldru1 = ru3;
					oldru2 = ru0;
					oldru3 = ru1;
					ru0 = oldru0;
					ru1 = oldru1;
					ru2 = oldru2;
					ru3 = oldru3;
				}
				if(refImgDeform == (refImgDeform | RefImgDeform.InvertV))
				{
					oldru0 = ru3;
					oldru1 = ru2;
					oldru2 = ru1;
					oldru3 = ru0;
					ru0 = oldru0;
					ru1 = oldru1;
					ru2 = oldru2;
					ru3 = oldru3;
				}
				var refvlist = new List<Vector3>();
				refvlist.AddRange(new[] { rarr[0], rarr[1], rarr[2], rarr[3] });
				var reftlist = new List<int>();
				reftlist.AddRange(new[] { 0, 1, 2, 1, 3, 2 });
				var refuvlist = new List<Vector2>();
				refuvlist.AddRange(new[] { ru0, ru1, ru2, ru3 });

				linren.useWorldSpace = false;
				linren.positionCount = refvlist.Count;
				linren.startWidth = 0.0f;
				linren.SetPositions(refvlist.ToArray());

				Mesh rmesh = null;
				if(linren.GetComponent<MeshFilter>())
				{
					var rmeshf = linren.GetComponent<MeshFilter>();

					//rmesh = rmeshf.sharedMesh;
					if(rmeshf.sharedMesh == null)
					{
						rmesh = new Mesh();
						rmeshf.sharedMesh = rmesh;
					}
					else
					{
						rmesh = rmeshf.sharedMesh;
					}
				}
				else
				{
					var refmeshf = ObjectFactory.AddComponent<MeshFilter>(lineobj);
					rmesh = new Mesh();
					rmesh = refmeshf.sharedMesh;
				}
				if(rmesh == null)
				{
					rmesh = new Mesh();
				}
				rmesh.Clear();
				rmesh.vertices = refvlist.ToArray();
				rmesh.triangles = reftlist.ToArray();
				rmesh.uv = refuvlist.ToArray();

				var meshren = lineobj.GetComponent<MeshRenderer>();
				var mat = meshren.sharedMaterial;
				if(mat != null)
				{
					mat.mainTextureOffset = new Vector2(refImgOffset.x, refImgOffset.y);
					mat.SetInt("_DecalAxis", (int)refplane);
					mat.SetInt("_DecalDeform", (int)refImgDeform);
					var refimf = mat.mainTexture.width / (float)mat.mainTexture.height;
					mat.mainTextureScale = new Vector2(refimf > 1.0f ? refImgScale.x : refImgScale.x * refimf, refimf > 1.0f ? refImgScale.y / refimf : refImgScale.y);
					mat.SetVector("_DecalPos", new Vector4(refImgPos.x, refImgPos.y, refImgPos.z, refImgSize));
				}
			}
			if(ISReferenceTrnsp)
			{
				var mat = currentObject.GetComponent<Renderer>().sharedMaterial;
				var col = Color.white;
				col.a = refTransparentf;
				mat.color = col;
			}
		}

		private static void LineRendererRenderRefresh(string typename, string name, bool IsRender, bool IsTransParent)
		{
			Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
			foreach(LineRenderer linren in components)
			{
				if(!linren.name.StartsWith(typename))
				{
					continue;
				}
				var lineobj = linren.gameObject;
				var linmeshf = lineobj.GetComponent<MeshFilter>();
				if(lineobj == null || linmeshf == null)
				{
					continue;
				}
				var linmesh = linmeshf.sharedMesh;
				if(linmesh == null)
				{
					continue;
				}
				var linvec = linmesh.vertices;
				var lintri = linmesh.triangles;
				var linuv = linmesh.uv;
				linmesh.Clear();
				linmesh.vertices = linvec;
				linmesh.triangles = lintri;
				linmesh.uv = linuv;
				var meshren = lineobj.GetComponent<MeshRenderer>();
				var mat = meshren.sharedMaterial;
				if(mat != null)
				{
					if(typename.StartsWith("EditorSculptRef"))
					{
						if(linren.name == name)
						{
							mat.SetFloat("_Transparent", 1.0f);
						}
						else
						{
							mat.SetFloat("_Transparent", IsRender ? 0.2f : 1.0f);
						}
					}
					else if(typename.StartsWith("Decal"))
					{
						mat.SetFloat("_Transparent", -1.0f);
					}
				}
			}
			if(IsTransParent && ISReferenceTrnsp)
			{
				var mat = currentObject.GetComponent<Renderer>().sharedMaterial;
				var col = Color.white;
				col.a = refTransparentf;
				mat.color = col;
			}

			//refimgLoad = false;
		}

		private static void ReferenceImgLoad(string refname)
		{
			Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
			foreach(LineRenderer linren in components)
			{
				if(!linren.name.StartsWith("EditorSculptRef"))
				{
					continue;
				}
				if(refname != oldrefname && linren.name == refname)
				{
					Material mat = null;
					try
					{
						mat = linren.gameObject.GetComponent<MeshRenderer>().sharedMaterial;
					}
					catch
					{
					}
					if(mat != null)
					{
						var vec = mat.mainTextureOffset;
						refImgOffset = new Vector2(vec.x, vec.y);
						var refimf = mat.mainTexture.height / (float)mat.mainTexture.width;
						var vec2 = mat.mainTextureScale;
						refImgScale = new Vector2(refimf > 1.0f ? vec2.x * refimf : vec2.x, refimf > 1.0f ? vec2.y : vec2.y / refimf);
						var vec3 = mat.GetVector("_DecalPos");
						refImgPos = new Vector3(vec3.x, vec3.y, vec3.z);
						refImgSize = vec3.w;
						refplane = (ReferenceImagePlane)mat.GetInt("_DecalAxis");
						refImgDeform = (RefImgDeform)mat.GetInt("_DecalDeform");
					}

					oldrefname = refname;
				}
			}
		}

		private static void ReferenceImgShow()
		{
			try
			{
				Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
				foreach(LineRenderer linren in components)
				{
					if(!linren.name.StartsWith("EditorSculptRef"))
					{
						continue;
					}
					ReferenceImgLoad(linren.name);
					ReferenceImgUpdate(linren.name);
				}
				LineRendererRenderRefresh("EditorSculptRef", "", false, true);
			}
			catch
			{
			}
		}

		private static void ReferenceImgTransparentMat()
		{
			if(oldMaterilList.Count < 1)
			{
				oldMaterilList = new List<Material>(currentObject.GetComponent<Renderer>().sharedMaterials);
			}
			var newmatlist = new List<Material>();

			//Material mat0 = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
			if(transparentMat == null)
			{
				transparentMat = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
			}
			var mat0 = transparentMat;
			var col = Color.white;
			col.a = refTransparentf;
			mat0.color = col;
			newmatlist.Add(mat0);
			currentObject.GetComponent<Renderer>().sharedMaterials = newmatlist.ToArray();
		}

		private static void ReferenceImageRevertMat()
		{
			if(oldMaterilList.Count < 1)
			{
				return;
			}
			if(currentObject == null)
			{
				return;
			}
			var isrevert = true;
			try
			{
				var mats = currentObject.GetComponent<Renderer>().sharedMaterials;
				for(var i = 0; i < mats.Length; i++)
				{
					if(mats[i].shader.name != "Custom/EditorSculpt" && mats[i].shader.name != "Legacy Shaders/Transparent/Diffuse")

						//Fixed 2021/06/07
						//if (mats[i].name != "Custom/EditorSculpt" && mats[i].name != "Legacy Shaders/Transparent/Diffuse")
					{
						isrevert = false;
						break;
					}
				}
				if(isrevert)
				{
					currentObject.GetComponent<Renderer>().sharedMaterials = oldMaterilList.ToArray();
					EditorSculptPrefab(false, true);
				}
			}
			catch
			{
			}
		}

		private void OnSelectReferenceCallback(object obj)
		{
			//Added 2021/05/17
			IsBrushHitPos = false;
			BrushHitPos = Vector3.zero;

			//End added 2021/05/17
			refplane = (ReferenceImagePlane)obj;
			EditorUtility.DisplayDialog("caution", "Select a image file for the reference image.", "OK");
			var loadpath = EditorUtility.OpenFilePanelWithFilters("Select Texture Image", "", new[] { "Image files", "png,jpg,jpeg", "All files", "*" });
			if(loadpath.Length < 1)
			{
				IsBrushHitPos = true;
				return;
			}
			var tempobj = currentObject;
			var refname = ReferenceImageCreate();
			currentObject = tempobj;
			Selection.activeGameObject = currentObject;
			ReferenceTextureLoad(loadpath, refname);
			refimgInt = refimgCnt;

			//Added 2021/05/17
			IsBrushHitPos = true;

			//End added 2021/05/17

			ReferenceImgUpdate(refname);
		}

		private static void LineRendererDelete(string refname, bool delmat)
		{
			Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
			foreach(LineRenderer linren in components)
			{
				if(linren.name != refname)
				{
					continue;
				}
				Material mat = null;
				if(EditorUtility.DisplayDialog("Caution", "Do you want to delete materials too?", "OK", "Cancel"))
				{
					try
					{
						mat = linren.gameObject.GetComponent<MeshRenderer>().sharedMaterial;
					}
					catch
					{
					}
					if(mat != null)
					{
						var matpath = AssetDatabase.GetAssetPath(mat);
						if(matpath.Length > 0)
						{
							AssetDatabase.DeleteAsset(matpath);
							linren.gameObject.GetComponent<MeshRenderer>().sharedMaterial = null;
						}
					}
				}
				DestroyImmediate(linren.gameObject, true);
				break;
			}
		}

		private static void LineRendererMeshDelete(string linename)
		{
			if(currentObject == null)
			{
				return;
			}
			Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
			foreach(LineRenderer linren in components)
			{
				if(linren.name.StartsWith(linename))
				{
					var parento = linren.gameObject.transform.parent.gameObject;
					var meshf = parento.GetComponent<MeshFilter>();
					var IsLinren = false;
					if(meshf != null && meshf.sharedMesh != null)
					{
						var labels = AssetDatabase.GetLabels(meshf.sharedMesh);
						foreach(var str in labels)
						{
							if(str.StartsWith("EditorSculpt"))
							{
								IsLinren = true;
								break;
							}
						}
					}
					else
					{
						var skinnedMesh = parento.GetComponent<SkinnedMeshRenderer>();
						if(skinnedMesh != null && skinnedMesh.sharedMesh != null)
						{
							var labels = AssetDatabase.GetLabels(skinnedMesh.sharedMesh);
							foreach(var str in labels)
							{
								if(str.StartsWith("EditorSculpt"))
								{
									IsLinren = true;
									break;
								}
							}
						}
					}
					if(IsLinren)
					{
						var linmeshf = linren.gameObject.GetComponent<MeshFilter>();
						if(linmeshf != null)
						{
							DestroyImmediate(linmeshf, true);
							try
							{
								EditorSculptPrefab(false, true);
							}
							catch
							{
							}
						}
					}

					//EditorSculptPrefab(true);
				}
			}
		}

		private static void DrawGUIMask()
		{
			if(GUILayout.Button("Invert Mask"))
			{
				if(EnableUndo)
				{
					Undo.RegisterCompleteObjectUndo(currentMesh, "Invert Mask" + Undo.GetCurrentGroup());
				}

				//Undo.undoRedoPerformed -= UndoCallbackFunc;
				//Undo.undoRedoPerformed += UndoCallbackFunc;
				var vertices = currentMesh.vertices;
				var uv4s = currentMesh.uv4;
				var newuv4s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
				for(var i = 0; i < vertices.Length; i++)
				{
					var maskf0 = uv4s[i].x;
					var maskf1 = uv4s[i].y;
					newuv4s[i] = new Vector2(1.0f - maskf0, maskf1);
				}
				currentMesh.uv4 = newuv4s;
				ChangeMaterial();
				LoadTexCnt = 0;
			}
			if(GUILayout.Button("Clear Mask"))
			{
				if(EnableUndo)
				{
					Undo.RegisterCompleteObjectUndo(currentMesh, "Clear Mask" + Undo.GetCurrentGroup());
				}

				//Undo.undoRedoPerformed -= UndoCallbackFunc;
				//Undo.undoRedoPerformed += UndoCallbackFunc;
				var vertices = currentMesh.vertices;
				var uv4s = currentMesh.uv4;
				var newuv4s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
				for(var i = 0; i < vertices.Length; i++)
				{
					var maskf0 = uv4s[i].y;
					newuv4s[i] = new Vector2(1.0f, maskf0);
				}
				currentMesh.uv4 = newuv4s;
				ChangeMaterial();
				LoadTexCnt = 0;
			}
		}

		private static bool CheckAnimationBrush(string str)
		{
			if(str == "AnimationMove" || str == "AnimationTip")
			{
				return true;
			}
			return false;
		}

		private static float GetNearKeyTime(float f0)
		{
			var animt = currentObject.transform.root.gameObject.GetComponent<Animator>();
			var aclips = animt.runtimeAnimatorController.animationClips;
			var aclip = aclips[aclipidx];
			var curvelist = new List<AnimationCurve>();
			var keylist = new List<Keyframe>();
			var timelist = new List<float>();
			var nowclip = aclips[aclipidx];
			var bindings = AnimationUtility.GetCurveBindings(aclip);
			for(var i = 0; i < bindings.Length; i++)
			{
				var acurve = AnimationUtility.GetEditorCurve(aclip, bindings[i]);
				keylist.AddRange(acurve.keys);
			}
			for(var i = 0; i < keylist.Count; i++)
			{
				timelist.Add(keylist[i].time);
			}
			var mindist = 0.0f;
			var mintime = 0.0f;
			for(var i = 0; i < timelist.Count; i++)
			{
				var dist = Mathf.Abs(timelist[i] - f0);
				if(timelist[i] < f0)
				{
					if(dist < mindist || mindist == 0.0f)
					{
						mintime = timelist[i];
						mindist = dist;
					}
				}
			}
			return mintime;
		}

		private static float GetNearKeyTime2(AnimationClip aclip, float f0, bool isPrevious)
		{
			var curvelist = new List<AnimationCurve>();
			var keylist = new List<Keyframe>();
			var timelist = new List<float>();
			var bindings = AnimationUtility.GetCurveBindings(aclip);
			for(var i = 0; i < bindings.Length; i++)
			{
				var acurve = AnimationUtility.GetEditorCurve(aclip, bindings[i]);
				keylist.AddRange(acurve.keys);
			}
			for(var i = 0; i < keylist.Count; i++)
			{
				timelist.Add(keylist[i].time);
			}
			var mindist = 0.0f;
			var rettime = 0.0f;
			for(var i = 0; i < timelist.Count; i++)
			{
				var dist = Mathf.Abs(timelist[i] - f0);
				if(isPrevious)
				{
					if(timelist[i] < f0)
					{
						if(dist < mindist || mindist == 0.0f)
						{
							rettime = timelist[i];
							mindist = dist;
						}
					}
				}
				else
				{
					if(timelist[i] >= f0)
					{
						if(dist < mindist || mindist == 0.0f)
						{
							rettime = timelist[i];
							mindist = dist;
						}
					}
				}
			}
			return rettime;
		}

		private static float GetNearKeyTimeMulti(AnimationClip aclip, float f0)
		{
			var curvelist = new List<AnimationCurve>();
			var keylist = new List<Keyframe>();
			var timelist = new List<float>();
			var bindings = AnimationUtility.GetCurveBindings(aclip);
			for(var i = 0; i < bindings.Length; i++)
			{
				var acurve = AnimationUtility.GetEditorCurve(aclip, bindings[i]);
				keylist.AddRange(acurve.keys);
			}
			for(var i = 0; i < keylist.Count; i++)
			{
				timelist.Add(keylist[i].time);
			}
			var mindist = 0.0f;
			var rettime = 0.0f;
			for(var i = 0; i < timelist.Count; i++)
			{
				var dist = Mathf.Abs(timelist[i] - f0);
				if(dist < mindist || mindist == 0.0f)
				{
					rettime = timelist[i];
					mindist = dist;
				}
			}
			return rettime;
		}

		/*
		private static StrList GetBrushArr(Vector3[] varr, int[] tarr, Vector3 vec)
		{
		    StrList BrushList = new StrList();
		    BrushList.iarr = new List<int>();
		    BrushList.varr = new List<Vector3>();
		    for(int i=0;i<tarr.Length;i += 3)
		    {
		        float dist0 = (varr[tarr[i]]-vec).magnitude;
		        float dist1 = (varr[tarr[i]+1]-vec).magnitude;
		        float dist2 = (varr[tarr[i]+2]-vec).magnitude;
		        if(dist0>BrushRadius && dist1>BrushRadius && dist2>BrushRadius)continue;
		        BrushList.iarr.Add(i);
		        BrushList.iarr.Add(i+1);
		        BrushList.iarr.Add(i+2);
		        BrushList.varr.Add(varr[tarr[i]]);
		        BrushList.varr.Add(varr[tarr[i]+1]);
		        BrushList.varr.Add(varr[tarr[i]+2]);
		    }
		    return BrushList;
		}//private static StrList GetBrushArr
		*/

		/*static void SetMaterial()
		{
		    if(((vcdisp==VColorDisplay.OnPaint && btype == BrushType.VertexColor) || (vcdisp==VColorDisplay.Always)) && OldMaterial == null)
		    //if(ShowVertexColor && OldMaterial == null)
		    {
		        OldMaterial = currentObject.renderer.sharedMaterial;
		        currentObject.renderer.sharedMaterial = VColorShader.ShowVColor;
		    }
		    else if(OldMaterial != null)
		    {
		        currentObject.renderer.sharedMaterial = OldMaterial;
		        OldMaterial = null;
		    }
		}*/

		/*static void SetMaterial()
		{
		    if(((vcdisp==VColorDisplay.OnPaint && btype == BrushType.VertexColor) || (vcdisp==VColorDisplay.Always)) && OldMaterial == null)
		    //if(ShowVertexColor && OldMaterial == null)
		    {
		        if(currentObject==null)return;
		        OldMaterial = currentObject.renderer.sharedMaterial;
		            string shaderstr =
		            "Shader \"VertexColor\" {"+
		            "Properties {_MainTex (\"Texture\", 2D) = \"white\" {} }"+
		            "SubShader {"+
		            "	Pass {"+
		            "	BindChannels {"+
		            "	Bind \"Vertex\", vertex"+
		            "	Bind \"Texcoord\", texcoord"+
		            "	Bind \"Color\", color"+
		            "}" +
		            "SetTexture [_MainTex] {"+
		            "combine primary + primary"+
		            "	}"+
		            "}"+
		            "	Pass {"+
		            "	Blend DstColor SrcColor"+
		            "	Lighting On"+
		            "	Material{Diffuse(1,1,1,1)}"+
		            "	}" +
		            "}" +
		            "}";
		        currentObject.renderer.material = new Material(shaderstr);
		    }
		    //else if(OldMaterial != null)
		    if(((vcdisp!=VColorDisplay.OnPaint || btype != BrushType.VertexColor) && (vcdisp!=VColorDisplay.Always)) && OldMaterial != null)
		    {
		        currentObject.renderer.sharedMaterial = OldMaterial;
		        OldMaterial = null;
		    }
		}*/

		private static void ChangeMaterial()
		{
			if(currentObject == null)
			{
				return;
			}
			if(currentObject.GetComponent<Renderer>() == null)
			{
				return;
			}
			var MaterialList = currentObject.GetComponent<Renderer>().sharedMaterials.ToList();
			if(MaterialList.Count < 1)
			{
				return;
			}
			var flag0 = false;
			for(var i = 0; i < MaterialList.Count; i++)
			{
				var mat = MaterialList[i];
				if(mat == null)
				{
					continue;
				}
				if(mat.shader == null)
				{
					continue;
				}
				if(mat.shader.name == "Custom/EditorSculpt")
				{
					flag0 = true;
					mat.SetColor("_MaskColor", maskColor);
					if(BrushString == null)
					{
						mat.SetInt("_DispMode", 1);
					}
					else if(DispString == "Rendered")
					{
						mat.SetInt("_DispMode", 1);
					}
					else if(BrushString == "VertexColor")
					{
						mat.SetInt("_DispMode", 0);
					}
					else if(BrushString == "VertexWeight" || BrushString == "EraseWeight")
					{
						mat.SetInt("_DispMode", 2);
					}
					else if(BrushString == "TexturePaint" || BrushString == "BETA_Texture")
					{
						mat.SetInt("_DispMode", 3);
						if(MaterialList[0].mainTexture == null)
						{
							mat.SetTexture("_MainTex", uvtexture);
						}

						//mat.SetTexture("_MainTex", uvtexture);
						//if (!MaterialList[0].HasProperty("_MainTex")) mat.SetTexture("_MainTex", uvtexture);
					}
					else if(DispString == "Standard")
					{
						mat.SetInt("_DispMode", 1);
					}
					else if(DispString == "VertexColor")
					{
						mat.SetInt("_DispMode", 0);
					}
					else if(DispString == "VertexWeight")
					{
						mat.SetInt("_DispMode", 2);
					}
					else if(DispString == "Texture")
					{
						mat.SetInt("_DispMode", 3);
						if(MaterialList[0].mainTexture == null)
						{
							mat.SetTexture("_MainTex", uvtexture);
						}

						//mat.SetTexture("_MainTex", uvtexture);
						//if (!MaterialList[0].HasProperty("_MainTex")) mat.SetTexture("_MainTex", uvtexture);
					}
					else
					{
						mat.SetInt("_DispMode", 1);
					}

					//Added 2025/05/28
					//Fixed bug that transparent lost in some case (The Hair of the VRoid Studio imported models etc).
					mat.renderQueue = (int)RenderQueue.Transparent;

					//End Added 2025/05/28
				}
			}
			if(flag0)
			{
				return;
			}
			var mat0 = new Material(Shader.Find("Custom/EditorSculpt"));
			mat0.SetColor("_MaskColor", maskColor);
			if(BrushString == null)
			{
				mat0.SetInt("_DispMode", 1);
			}
			else if(BrushString == "VertexColor")
			{
				mat0.SetInt("_DispMode", 0);
			}
			else if(BrushString == "VertexWeight" || BrushString == "EraseWeight")
			{
				mat0.SetInt("_DispMode", 2);
			}
			else if(BrushString == "TexturePaint" || BrushString == "BETA_Texture")
			{
				mat0.SetInt("_DispMode", 3);
				if(MaterialList[0].mainTexture == null)
				{
					mat0.SetTexture("_MainTex", uvtexture);
				}

				//mat0.SetTexture("_MainTex", uvtexture);
				//if (!MaterialList[0].HasProperty("_MainTex")) mat0.SetTexture("_MainTex", uvtexture);
			}

			//else if (vcdisp == VColorDisplay.Standard) mat0.SetInt("_DispMode", 1);
			//else if (vcdisp == VColorDisplay.VertexColor) mat0.SetInt("_DispMode", 0);
			else if(DispString == "Standard")
			{
				mat0.SetInt("_DispMode", 1);
			}
			else if(DispString == "VertexColor")
			{
				mat0.SetInt("_DispMode", 0);
			}
			else if(DispString == "VertexWeight")
			{
				mat0.SetInt("_DispMode", 2);
			}
			else if(DispString == "Texture" || BrushString == "TexturePaint" || BrushString == "BETA_Texture")
			{
				mat0.SetInt("_DispMode", 3);
				if(MaterialList[0].mainTexture == null)
				{
					mat0.SetTexture("_MainTex", uvtexture);
				}

				//mat0.SetTexture("_MainTex", uvtexture);
				//if (!MaterialList[0].HasProperty("_MainTex")) mat0.SetTexture("_MainTex", uvtexture);
			}
			else
			{
				mat0.SetInt("_DispMode", 1);
			}

			//Added 2025/05/28
			//Fixed bug that transparent lost in some case(The Hair of the VRoid Studio imported models etc).
			mat0.renderQueue = (int)RenderQueue.Transparent;

			//End Added 2025/05/28
			MaterialList.Add(mat0);
			currentObject.GetComponent<Renderer>().sharedMaterials = MaterialList.ToArray();

			//if (!currentMeshFilter) return;
			if(!currentMesh)
			{
				return;
			}
			var vertices = currentMesh.vertices;
			var triangles = currentMesh.GetTriangles(0);
			var colors = currentMesh.colors;
			var uvs = currentMesh.uv;
			var uv2s = currentMesh.uv2;
			var uv3s = currentMesh.uv3;
			var uv4s = currentMesh.uv4;
			var binds = currentMesh.bindposes;
			var weight1s = currentMesh.GetAllBoneWeights().ToArray();
			var pervers = currentMesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var blendvertlistlist = new List<List<Vector3>>();
			var blendnormlistlist = new List<List<Vector3>>();
			var blendtanlistlist = new List<List<Vector3>>();
			var bnamelist = new List<string>();
			var weightlist = new List<float>();
			var bshapecnt = currentMesh.blendShapeCount;
			var vertcnt = currentMesh.vertexCount;
			if(bshapecnt > 0)
			{
				for(var i = 0; i < bshapecnt; i++)
				{
					var bshaname = currentMesh.GetBlendShapeName(i);
					var tempverts = Enumerable.Repeat(Vector3.zero, vertcnt).ToArray();
					var tempnorms = Enumerable.Repeat(Vector3.zero, vertcnt).ToArray();
					var temptans = Enumerable.Repeat(Vector3.zero, vertcnt).ToArray();
					var bsweight = currentMesh.GetBlendShapeFrameWeight(i, 0);
					currentMesh.GetBlendShapeFrameVertices(i, 0, tempverts, tempnorms, temptans);
					blendvertlistlist.Add(tempverts.ToList());
					blendnormlistlist.Add(tempnorms.ToList());
					blendtanlistlist.Add(temptans.ToList());
					bnamelist.Add(bshaname);
					weightlist.Add(bsweight);
				}
			}
			var trilistlist = new List<List<int>>();
			var topolist = new List<MeshTopology>();

			//List<List<int>> trislistlist = new List<List<int>>(); 
			var subcnt = currentMesh.subMeshCount;
			for(var i = 0; i < subcnt; i++)
			{
				//trilistlist.Add(currentMesh.GetTriangles(0).ToList());
				trilistlist.Add(currentMesh.GetIndices(i).ToList());
				topolist.Add(currentMesh.GetTopology(i));

				//trislistlist.Add(currentMesh.GetTriangles(i).ToList());
			}
			currentMesh.Clear();
			currentMesh.vertices = vertices;
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			currentMesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint + 1; i++)
			{
				try
				{
					if(trilistlist[i].Count < 1)
					{
						currentMesh.SetTriangles(triangles, i);
					}
					else
					{
						currentMesh.SetIndices(trilistlist[i].ToArray(), topolist[i], i);
					}
				}
				catch
				{
					currentMesh.SetTriangles(triangles, i);
				}
			}
			currentMesh.RecalculateBounds();
			CalcMeshNormals(currentMesh);
			currentMesh.RecalculateTangents();
			currentMesh.colors = colors;
			currentMesh.uv = uvs;
			currentMesh.uv2 = uv2s;
			currentMesh.uv3 = uv3s;
			currentMesh.uv4 = uv4s;
			currentMesh.bindposes = binds;

			//currentMesh.boneWeights = boneweights;
			if(IsWeight)
			{
				var ntweight1s = new NativeArray<BoneWeight1>(weight1s, Allocator.Temp);
				var PerVerts = new NativeArray<byte>(pervers.ToArray(), Allocator.Temp);
				currentMesh.SetBoneWeights(PerVerts, ntweight1s);
			}
			else
			{
				BoneWeight1Create(currentMesh);
			}
			if(bshapecnt > 0)
			{
				for(var i = 0; i < blendvertlistlist.Count; i++)
				{
					currentMesh.AddBlendShapeFrame(bnamelist[i], weightlist[i], blendvertlistlist[i].ToArray(),
						blendnormlistlist[i].ToArray(), blendtanlistlist[i].ToArray());
				}
			}
		}

		private static void FixMaterial()
		{
			if(currentObject == null)
			{
				return;
			}
			if(currentObject.GetComponent<Renderer>() == null)
			{
				return;
			}
			var MaterialList = currentObject.GetComponent<Renderer>().sharedMaterials.ToList();
			for(var i = 0; i < MaterialList.Count; i++)
			{
				var mat = MaterialList[i];
				try
				{
					if(mat.shader.name == "Custom/EditorSculpt")
					{
						if(BrushString == "VertexWeight" || BrushString == "EraseWeight")
						{
							mat.SetInt("_DispMode", 1);
						}
					}
				}
				catch
				{
				}
			}
		}

		private static void LoadTexture(bool flag0)
		{
			if(currentObject == null)
			{
				return;
			}
			if(currentObject.GetComponent<Renderer>() == null)
			{
				return;
			}
			var MaterialList = currentObject.GetComponent<Renderer>().sharedMaterials.ToList();
			var pid = 0;
			for(var i = 0; i < MaterialList.Count; i++)
			{
				try
				{
					//if (MaterialList[i].HasProperty("_MainTex") && MaterialList[i].shader.name != "Custom/EditorSculpt")
					//if ((MaterialList[i].mainTexture!=null) && MaterialList[i].shader.name != "Custom/EditorSculpt")
					if(MaterialList[i].GetTexturePropertyNames().Length > 0 && MaterialList[i].shader.name != "Custom/EditorSculpt")
					{
						if(pid == paintmatidx)
						{
							break;
						}
						pid++;
					}
				}
				catch
				{
				}
			}
			if(!uvtexture)
			{
				uvtexture = new Texture2D(0, 0);
			}
			if(IsPaintBrush || uvtexture.width > 1)
			{
				uvtexture = new Texture2D(texwidth, texheight);
			}
			var temptex = new Texture2D(0, 0);

			//temptex =
			//    (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(MaterialList[0].mainTexture), typeof(Texture2D));
			try
			{
				//if (MaterialList[pid].HasProperty("_MainTex"))
				//if (MaterialList[pid].mainTexture!=null)
				if(MaterialList[pid].GetTexturePropertyNames().Length > 0)
				{
					//temptex = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(GetMainTexture(MaterialList[pid])), typeof(Texture2D));
					temptex = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(MaterialList[pid].mainTexture), typeof(Texture2D));

					//temptex = (Texture2D)LoadAssetFastWithoutPath(MaterialList[pid].mainTexture, typeof(Texture2D));
				}
			}
			catch
			{
			}
			if(temptex)
			{
				uvtexture = temptex;
			}
			else if(!File.Exists(Application.dataPath + "/../" + "Assets/" + currentObject.name + "_EditorSculpt.png"))
			{
				var maintexstr = "";
				try
				{
					//maintexstr = AssetDatabase.GetAssetPath(MaterialList[0].GetTexture("_MainTex"));
					//if (MaterialList[pid].HasProperty("_MainTex"))
					//if (MaterialList[pid].mainTexture!=null)
					if(MaterialList[pid].GetTexturePropertyNames().Length > 0)
					{
						maintexstr = AssetDatabase.GetAssetPath(MaterialList[pid].mainTexture);

						//maintexstr = AssetDatabase.GetAssetPath(GetMainTexture(MaterialList[pid]));
						//maintexstr = AssetDatabase.GetAssetPath(MaterialList[pid].GetTexture("_MainTex"));
						//maintexstr = GetAssetPathFast(MaterialList[pid].GetTexture("_MainTex"));
					}
				}
				catch
				{
				}
				if(maintexstr.Length > 0)
				{
					uvtexture = (Texture2D)AssetDatabase.LoadAssetAtPath(maintexstr, typeof(Texture2D));

					//uvtexture = (Texture2D)LoadAssetFastWithPath(maintexstr, typeof(Texture2D));
				}
				else if(uvtexture.height > 1)

					//else
				{
					//uvtexture = new Texture2D(texwidth, texheight);
					var y = 0;
					while(y < uvtexture.height)
					{
						var x = 0;
						while(x < uvtexture.width)
						{
							var col = Color.white;
							col.a = 1.0f;
							uvtexture.SetPixel(x, y, col);
							++x;
						}
						++y;
					}
					uvtexture.Apply();
					memoryTexBytes = uvtexture.GetRawTextureData();
				}
				else
				{
					try
					{
						//if (MaterialList[0].HasProperty("_MainTex")) MaterialList[0].mainTexture = null;
						//if (MaterialList[pid].HasProperty("_MainTex")) MaterialList[pid].mainTexture = null;
						//if (MaterialList[pid].mainTexture != null) SetMainTexture(MaterialList[pid], null);
						//if (MaterialList[pid].mainTexture != null) MaterialList[pid].mainTexture = null;
						if(MaterialList[pid].GetTexturePropertyNames().Length > 0)
						{
							MaterialList[pid].mainTexture = null;
						}
					}
					catch
					{
					}
				}
			}
			else
			{
				var bytes = File.ReadAllBytes(Application.dataPath + "/../" + "Assets/" + currentObject.name + "_EditorSculpt.png");
				uvtexture.LoadImage(bytes);
				memoryTexBytes = uvtexture.GetRawTextureData();
			}

			//New in 2020/01/29
			if(!uvtexture)
			{
				uvtexture = new Texture2D(texwidth, texheight);
			}

			//End New in 2020/01/29
			if(flag0 && uvtexture.height > 1)
			{
				try
				{
					//if (MaterialList[0].HasProperty("_MainTex")) MaterialList[0].mainTexture = uvtexture;
					//if (MaterialList[pid].HasProperty("_MainTex")) MaterialList[pid].mainTexture = uvtexture;
					//if (MaterialList[pid].mainTexture != null) SetMainTexture(MaterialList[pid], uvtexture);
					//if (MaterialList[pid].mainTexture != null) MaterialList[pid].mainTexture = uvtexture;
					if(MaterialList[pid].GetTexturePropertyNames().Length > 0)
					{
						MaterialList[pid].mainTexture = uvtexture;
					}
				}
				catch
				{
				}
			}
			uvindex = new List<List<int>>();
			uvpos = new List<List<Vector3>>();
			IsSetupUV = false;

			//New in 2020/01/29
			if(!uvtexture)
			{
				uvtexture = new Texture2D(texwidth, texheight);
			}

			//End New in 2020/01/29
			var uwidth = uvtexture.width + 1;
			var uheight = uvtexture.height + 1;
			for(var i = 0; i < uwidth; i++)
			{
				var tempint = new List<int>();
				var tempvec = new List<Vector3>();
				for(var j = 0; j < uheight; j++)
				{
					tempint.Add(-1);
					tempvec.Add(Vector3.zero);
				}
				uvindex.Add(tempint);
				uvpos.Add(tempvec);
			}
		}

		private static void SaveTexture()
		{
			if(!currentObject)
			{
				return;
			}
			var objpath = AssetDatabase.GetAssetPath(currentMesh);

			//String objpath = GetAssetPathFast(currentMesh);
			if(objpath.Length < 1)
			{
				LoadTexture(false);
				var MaterialList = currentObject.GetComponent<Renderer>().sharedMaterials.ToList();
				var pid = 0;
				for(var i = 0; i < MaterialList.Count; i++)
				{
					//if (MaterialList[i].HasProperty("_MainTex") && MaterialList[i].shader.name != "Custom/EditorSculpt")
					//if ((MaterialList[i].mainTexture!=null) && MaterialList[i].shader.name != "Custom/EditorSculpt")
					if(MaterialList[i].GetTexturePropertyNames().Length > 0 && MaterialList[i].shader.name != "Custom/EditorSculpt")
					{
						if(pid == paintmatidx)
						{
							break;
						}
						pid++;
					}
				}
				var mat = MaterialList[pid];

				//SetMainTexture(mat, uvtexture);
				mat.mainTexture = uvtexture;
				return;
			}
			if(ImportMatPath.Length > 0)
			{
				var MaterialList = currentObject.GetComponent<Renderer>().sharedMaterials.ToList();
				var pid = 0;
				for(var i = 0; i < MaterialList.Count; i++)
				{
					var imppath = "";
					try
					{
						imppath = ImportMatPathList[i];
					}
					catch
					{
					}
					if(imppath.Length < 1)
					{
						continue;
					}
					var mat = MaterialList[i];
					var savetex = new Texture2D(0, 0);
					try
					{
						var teximp = (TextureImporter)AssetImporter.GetAtPath(imppath);

						//Added 2023/03/23
						if(IsUncompressTexture)
						{
							teximp.textureCompression = TextureImporterCompression.Uncompressed;
						}

						//End Added 2023/03/23
						var oldreadable = teximp.isReadable;
						teximp.isReadable = true;
						try
						{
							AssetDatabase.StartAssetEditing();
							AssetDatabase.WriteImportSettingsIfDirty(imppath);
							AssetDatabase.ImportAsset(imppath);
						}
						finally
						{
							AssetDatabase.StopAssetEditing();
						}
						var imptex = new Texture2D(texwidth, texheight);
						imptex = (Texture2D)AssetDatabase.LoadAssetAtPath(imppath, typeof(Texture2D));

						//Changed 2023/03/23
						EditorUtility.CopySerialized(imptex, savetex);
						////imptex = (Texture2D)LoadAssetFastWithPath(imppath, typeof(Texture2D));
						//Texture2D outtex = new Texture2D(imptex.width, imptex.height);
						//int y = 0;
						//while (y < outtex.height)
						//{
						//    int x = 0;
						//    while (x < outtex.width)
						//    {
						//        Color col = imptex.GetPixel(x, y);
						//        outtex.SetPixel(x, y, col);
						//        ++x;
						//    }
						//    ++y;
						//}
						//outtex.Apply();
						//savetex = outtex;
						//End Changed 2023/03/23
					}
					catch
					{
						if(imppath.Length > 0)
						{
							////SetTextureReadable(imppath);
							//Texture2D imptex = new Texture2D(texwidth, texheight);
							//imptex = (Texture2D)AssetDatabase.LoadAssetAtPath(imppath, typeof(Texture2D));
							//IntPtr ptrs = imptex.GetNativeTexturePtr();
							//savetex = Texture2D.CreateExternalTexture(imptex.width, imptex.height, imptex.format, imptex.mipmapCount > 0 ? true : false
							//    , true, ptrs);

							//EditorUtility.CopySerialized(imptex, savetex);

							savetex = duplicateTexture((Texture2D)mat.mainTexture);

							//if (mat.mainTexture != null)
							//{
							//    //EditorUtility.CopySerialized(mat.mainTexture, savetex);
							//    EditorUtility.CopySerialized(GetMainTexture(mat), savetex);
							//}
							//else
							//{
							//    byte[] bytes = File.ReadAllBytes(imppath);
							//    savetex.LoadImage(bytes);
							//}
						}
					}
					var savepath = AssetDatabase.GetAssetPath(mat);

					//String savepath = GetAssetPathFast(mat);
					try
					{
						if(savepath.Length > 0)
						{
							try
							{
								AssetDatabase.StartAssetEditing();
								AssetDatabase.AddObjectToAsset(savetex, savepath);
								AssetDatabase.ImportAsset(savepath);
							}
							finally
							{
								AssetDatabase.StopAssetEditing();
							}

							//SetMainTexture(mat, savetex);
							//AddObjectToAssetFast(savetex, savepath);
							mat.mainTexture = savetex;
						}
					}
					catch
					{
					}
					LoadTexture(true);
					pid++;
				}
			}
			if(BrushString == "TexturePaint" || BrushString == "BETA_Texture")
			{
				IsPaintBrush = true;
				LoadTexture(false);
				if(uvtexture.height > 1)
				{
					var MaterialList = currentObject.GetComponent<Renderer>().sharedMaterials.ToList();
					var pid = 0;
					for(var i = 0; i < MaterialList.Count; i++)
					{
						//if (MaterialList[i].HasProperty("_MainTex") && MaterialList[i].shader.name != "Custom/EditorSculpt")
						//if((MaterialList[i].mainTexture!=null) && MaterialList[i].shader.name != "Custom/EditorSculpt")
						if(MaterialList[i].GetTexturePropertyNames().Length > 0 && MaterialList[i].shader.name != "Custom/EditorSculpt")
						{
							if(pid == paintmatidx)
							{
								break;
							}
							pid++;
						}
					}
					var mat = MaterialList[pid];
					var savetex = new Texture2D(0, 0);
					savetex = uvtexture;
					var savepath = AssetDatabase.GetAssetPath(mat);

					//String savepath = GetAssetPathFast(mat);
					//Texture2D temptex = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(mat), typeof(Texture2D));
					var temptex = (Texture2D)AssetDatabase.LoadAssetAtPath(savepath, typeof(Texture2D));

					//Texture2D temptex = (Texture2D)LoadAssetFastWithPath(savepath, typeof(Texture2D));
					//if (!temptex && savepath.Length > 0)
					if(savepath.Length > 0)
					{
						//AssetDatabase.AddObjectToAsset(savetex, AssetDatabase.GetAssetPath(mat));
						//AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mat));
						//Changed 2025/09/23
						var texPath = AssetDatabase.GetAssetPath(uvtexture);
						if(texPath.Length < 2)
						{
							//End Changed 2025/09/23
							try
							{
								if(!temptex)
								{
									try
									{
										AssetDatabase.StartAssetEditing();
										AssetDatabase.AddObjectToAsset(savetex, savepath);
										AssetDatabase.ImportAsset(savepath);
									}
									finally
									{
										AssetDatabase.StopAssetEditing();
									}

									//SetMainTexture(mat, savetex);
									//AddObjectToAssetFast(savetex, savepath);
									mat.mainTexture = savetex;
								}
							}
							catch
							{
							}
						}
						else
						{
							mat.mainTexture = uvtexture;
						}
					}

					//else if (!temptex)
					else
					{
						//String matpath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + "_" + mat.shader.name.Replace("/", "") + ".mat");
						//String matpath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + SaveDir + currentObject.name + "_" +
						//    mat.shader.name.Replace("/", "") + ".mat");
						var matpath = AssetDatabase.GenerateUniqueAssetPath(SaveFolderPath + SaveDir + currentObject.name + "_" +
						                                                    mat.shader.name.Replace("/", "") + ".mat");
						try
						{
							if(!temptex)
							{
								try
								{
									AssetDatabase.StartAssetEditing();
									AssetDatabase.CreateAsset(mat, matpath);
									AssetDatabase.AddObjectToAsset(savetex, matpath);
									AssetDatabase.ImportAsset(matpath);
								}
								finally
								{
									AssetDatabase.StopAssetEditing();
								}

								//SetMainTexture(mat, savetex);
								//CreateAssetAndAddObjFast(mat, matpath, savetex);
								mat.mainTexture = savetex;
							}
						}
						catch
						{
						}
					}
				}
				LoadTexture(true);
			}
		}

		private static bool CheckHasTexture()
		{
			var MaterialList = currentObject.GetComponent<Renderer>().sharedMaterials.ToList();
			var pid = 0;
			for(var i = 0; i < MaterialList.Count; i++)
			{
				//if (MaterialList[i].HasProperty("_MainTex") && MaterialList[i].shader.name != "Custom/EditorSculpt")
				//if ((MaterialList[i].mainTexture!=null) && MaterialList[i].shader.name != "Custom/EditorSculpt")
				if(MaterialList[i].GetTexturePropertyNames().Length > 0 && MaterialList[i].shader.name != "Custom/EditorSculpt")
				{
					//Debug.Log(AssetDatabase.GetAssetPath(GetMainTexture(MaterialList[i])));
					//Debug.Log(AssetDatabase.GetAssetPath(MaterialList[i].mainTexture));
					pid++;
				}
			}
			if(pid == 0)
			{
				return false;
			}
			return true;
		}

		private static Texture GetMainTexture(Material mat)
		{
			if(mat.HasProperty("_MainTex"))
			{
				return mat.mainTexture;
			}
			var matstrs = mat.GetTexturePropertyNames();
			if(matstrs == null)
			{
				return null;
			}
			if(matstrs.Length < 1)
			{
				return null;
			}
			return mat.GetTexture(matstrs[0]);
		}

		private static void SetMainTexture(Material mat, Texture tex)
		{
			if(mat.HasProperty("_MainTex"))
			{
				mat.mainTexture = tex;
				return;
			}
			var matstrs = mat.GetTexturePropertyNames();
			if(matstrs == null)
			{
				return;
			}
			if(matstrs.Length < 1)
			{
				return;
			}
			mat.SetTexture(matstrs[0], tex);
		}

		//Taken from https://forum.unity.com/threads/easy-way-to-make-texture-isreadable-true-by-script.1141915/
		//static Texture2D duplicateTexture(Texture2D source)
		//{
		//    RenderTexture renderTex = RenderTexture.GetTemporary(
		//                source.width,
		//                source.height,
		//                0,
		//                RenderTextureFormat.Default,
		//                RenderTextureReadWrite.Linear);

		//    Graphics.Blit(source, renderTex);
		//    RenderTexture previous = RenderTexture.active;
		//    RenderTexture.active = renderTex;
		//    Texture2D readableText = new Texture2D(source.width, source.height);
		//    readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
		//    readableText.Apply();
		//    RenderTexture.active = previous;
		//    RenderTexture.ReleaseTemporary(renderTex);
		//    return readableText;
		//}

		//Taken from https://forum.unity.com/threads/easy-way-to-make-texture-isreadable-true-by-script.1141915/
		private static Texture2D duplicateTexture(Texture2D source)
		{
			var renderTex = RenderTexture.GetTemporary(
				source.width,
				source.height,
				0,
				RenderTextureFormat.Default,

				//RenderTextureReadWrite.Linear);
				RenderTextureReadWrite.sRGB);

			Graphics.Blit(source, renderTex);
			var previous = RenderTexture.active;
			RenderTexture.active = renderTex;
			var readableText = new Texture2D(source.width, source.height);
			readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
			readableText.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(renderTex);
			return readableText;
		}

		////Taken from https://forum.unity.com/threads/easy-way-to-make-texture-isreadable-true-by-script.1141915/
		//static void SetTextureReadable(string AbsoluteFilePath)
		//{
		//    string metadataPath = AbsoluteFilePath + ".meta";
		//    if (File.Exists(metadataPath))
		//    {
		//        List<string> newfile = new List<string>();

		//        string[] lines = File.ReadAllLines(metadataPath);
		//        foreach (string line in lines)
		//        {
		//            string newline = line;
		//            if (newline.Contains("isReadable: 0"))
		//            {
		//                newline = newline.Replace("isReadable: 0", "isReadable: 1");
		//            }
		//            newfile.Add(newline);
		//        }

		//        File.WriteAllLines(metadataPath, newfile.ToArray());
		//        AssetDatabase.Refresh();

		//    }
		//}

		private static void BoneWeight1s2UV4(Mesh mesh)
		{
			var vcnt = mesh.vertexCount;
			var maskv2 = mesh.uv4;
			for(var i = 0; i < vcnt; i++)
			{
				maskv2[i].y = 1.0f;
			}
			var weight1s = mesh.GetAllBoneWeights().ToArray();
			var pervers = mesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var pcnt = 0;
			if(IsWeight)
			{
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vcnt).ToArray();
				}
				for(var i = 0; i < boneidxs.Length; i++)
				{
					int pi0 = pervers[i];
					if(pi0 > 0)
					{
						for(var j = 0; j < pi0; j++)
						{
							var bw1 = weight1s[boneidxs[i] + j];
							if(bw1.boneIndex == boneidxint)
							{
								maskv2[i].y = 1.0f - bw1.weight;
								break;
							}
						}
					}
				}
			}
			mesh.uv4 = maskv2;
		}

		private static void UV42BoneWeight1(Mesh mesh)
		{
			var vcnt = mesh.vertexCount;
			var maskv2 = mesh.uv4;
			var weight1s = mesh.GetAllBoneWeights().ToArray();
			var pervers = mesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			if(IsWeight)
			{
				var pcnt = 0;
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vcnt).ToArray();
				}
				var weight1list = new List<BoneWeight1>();
				var perverlist = new List<byte>();
				var w1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
				for(var i = 0; i < boneidxs.Length; i++)
				{
					var bwlist0 = new List<BoneWeight1>();
					var pi0 = (int)pervers[i];
					var flag0 = false;
					if(pi0 > 0)
					{
						for(var j = 0; j < pi0; j++)
						{
							var bw1 = weight1s[boneidxs[i] + j];
							if(bw1.boneIndex == boneidxint)
							{
								flag0 = true;
								bw1.weight = 1.0f - maskv2[i].y;

								//break;
							}
							bwlist0.Add(bw1);
						}
						if(!flag0)
						{
							var bw2 = new BoneWeight1 { boneIndex = boneidxint, weight = 1.0f - maskv2[i].y };
							bwlist0.Add(bw2);
						}
					}
					if(bwlist0.Count < 1)
					{
						bwlist0.Add(w1);
					}
					weight1list.AddRange(bwlist0);
					perverlist.Add((byte)bwlist0.Count);
				}
				var ntweight1s = new NativeArray<BoneWeight1>(weight1list.ToArray(), Allocator.Temp);
				var PerVerts = new NativeArray<byte>(perverlist.ToArray(), Allocator.Temp);
				mesh.SetBoneWeights(PerVerts, ntweight1s);
			}
		}

		private static Vector3 GetHitpos(Vector3[] varr, int[] tarr, Vector3[] rvd, int idx)
		{
			var hitpos = Vector3.zero;
			for(var i = idx * 3; i < tarr.Length; i += ThreadNum * 3)
			{
				var p0 = varr[tarr[i]];
				var p1 = varr[tarr[i + 1]];
				var p2 = varr[tarr[i + 2]];
				var norm = Vector3.Cross(p1 - p0, p2 - p0).normalized;
				var dot0 = Vector3.Dot(p0 - rvd[0], norm);

				//float dot1 = Vector3.Dot((rvd[0] + rvd[1] * 1000.0f), norm);
				var dot1 = Vector3.Dot(rvd[0] + rvd[1] * CameraSize * 10.0f, norm);
				var tlen = dot0 / dot1;
				if(dot1 >= -0.0001f)
				{
					continue;
				}

				//Vector3 hitp = rvd[0] + tlen * (rvd[0] + rvd[1] * 1000.0f);
				var hitp = rvd[0] + tlen * (rvd[0] + rvd[1] * CameraSize * 10.0f);
				var norm0 = Vector3.Cross(p1 - p0, hitp - p0);
				if(Vector3.Dot(norm, norm0) < 0)
				{
					continue;
				}
				var norm1 = Vector3.Cross(p2 - p1, hitp - p1);
				if(Vector3.Dot(norm, norm1) < 0)
				{
					continue;
				}
				var norm2 = Vector3.Cross(p0 - p2, hitp - p2);
				if(Vector3.Dot(norm, norm2) < 0)
				{
					continue;
				}
				hitpos = hitp;
			}
			return hitpos;
		}

		private static Vector3[] GetHitNorm(Vector3[] varr, int[] tarr, Vector3[] rvd, int idx)
		{
			var hitarr = new Vector3[2];
			for(var i = idx * 3; i < tarr.Length; i += ThreadNum * 3)
			{
				var p0 = varr[tarr[i]];
				var p1 = varr[tarr[i + 1]];
				var p2 = varr[tarr[i + 2]];
				var norm = Vector3.Cross(p1 - p0, p2 - p0).normalized;
				var dot0 = Vector3.Dot(p0 - rvd[0], norm);
				var dot1 = Vector3.Dot(rvd[0] + rvd[1] * 1000.0f, norm);
				var tlen = dot0 / dot1;
				if(dot1 >= -0.0001f)
				{
					continue;
				}
				var hitp = rvd[0] + tlen * (rvd[0] + rvd[1] * 1000.0f);
				var norm0 = Vector3.Cross(p1 - p0, hitp - p0);
				if(Vector3.Dot(norm, norm0) < 0)
				{
					continue;
				}
				var norm1 = Vector3.Cross(p2 - p1, hitp - p1);
				if(Vector3.Dot(norm, norm1) < 0)
				{
					continue;
				}
				var norm2 = Vector3.Cross(p0 - p2, hitp - p2);
				if(Vector3.Dot(norm, norm2) < 0)
				{
					continue;
				}
				hitarr[0] = hitp;
				hitarr[1] = norm;
			}
			return hitarr;
		}

		/*private static List<int> GetBrushTri(Vector3[] varr, int[] tarr, Vector3 vec)
		{
		    List<int> triarr = new List<int>();
		    for(int i=0;i<tarr.Length;i += 3)
		    {
		        float dist0 = (varr[tarr[i]]-vec).magnitude;
		        float dist1 = (varr[tarr[i+1]]-vec).magnitude;
		        float dist2 = (varr[tarr[i+2]]-vec).magnitude;
		        if(dist0>BrushRadius && dist1>BrushRadius && dist2>BrushRadius)continue;
		        triarr.Add(tarr[i]);
		        triarr.Add(tarr[i+1]);
		        triarr.Add(tarr[i+2]);
		    }
		    return triarr;
		}*/

		/*private static List<int> GetBrushTri(Vector3[] varr, int[] tarr, Vector3 vec)
		{
		    List<IAsyncResult> IAList = new List<IAsyncResult>();
		    Func<Vector3[], int[], Vector3, int, List<int>> triMethod = new Func<Vector3[], int[], Vector3, int, List<int>>(getBrushTri);
		    List<int> trilist = new List<int>();

		    for(int i=0;i<ThreadNum;i++)
		    {
		        IAsyncResult ar = triMethod.BeginInvoke(varr,tarr,vec,i,null,null);
		        IAList.Add(ar);
		    }
		    for(int i=0;i<ThreadNum;i++)trilist.AddRange(triMethod.EndInvoke(IAList[i]));
		    return trilist;
		}


		private static List<int> getBrushTri(Vector3[] varr, int[] tarr, Vector3 vec, int idx)
		{
		    List<int> trilist = new List<int>();
		    for(int i=idx*3;i<tarr.Length;i +=ThreadNum*3)
		    {
		        float dist0 = (varr[tarr[i]]-vec).magnitude;
		        float dist1 = (varr[tarr[i+1]]-vec).magnitude;
		        float dist2 = (varr[tarr[i+2]]-vec).magnitude;
		        if(dist0>BrushRadius && dist1>BrushRadius && dist2>BrushRadius)continue;
		        trilist.Add(tarr[i]);
		        trilist.Add(tarr[i+1]);
		        trilist.Add(tarr[i+2]);
		    }
		    return trilist;
		}*/

		private static float GetAveragePolysize(Vector3[] varr, int[] tarr)
		{
			var IAList = new List<IAsyncResult>();
			var AvepolyMethod = new Func<Vector3[], int[], int, List<float>>(GetAveragePolysize);
			var polysizeList = new List<float>();
			for(var i = 0; i < ThreadNum; i++)
			{
				var ar = AvepolyMethod.BeginInvoke(varr, tarr, i, null, null);
				IAList.Add(ar);
			}
			for(var i = 0; i < ThreadNum; i++)
			{
				polysizeList.AddRange(AvepolyMethod.EndInvoke(IAList[i]));
			}
			return polysizeList.ToList().Average();

			//float avsize=0;
			//for(int i=0;i<polysizeList.Count;i++)avsize += polysizeList[i];
			//avsize /= polysizeList.Count;
			//return avsize;
		}

		private static List<float> GetAveragePolysize(Vector3[] varr, int[] tarr, int idx)
		{
			var polysizeList = new List<float>();
			for(var i = idx * 3; i < tarr.Length; i += ThreadNum * 3)
			{
				var v0 = varr[tarr[i]];
				var v1 = varr[tarr[i + 1]];
				var v2 = varr[tarr[i + 2]];
				var d0 = Vector3.Distance(v0, v1);
				var d1 = Vector3.Distance(v1, v2);
				var d2 = Vector3.Distance(v2, v0);
				var q0 = d0 / (d0 + d2) * (v2 - v1) + v1;
				polysizeList.Add(Vector3.Distance(v0, q0) * d1 * 0.5f);
			}
			return polysizeList;
		}

		private static float GetAveragePointDistance(Vector3[] varr, int[] tarr)
		{
			var IAList = new List<IAsyncResult>();
			var AvedistMethod = new Func<Vector3[], int[], int, List<float>>(GetAveragePointDistance);
			var pointdistList = new List<float>();
			for(var i = 0; i < ThreadNum; i++)
			{
				var ar = AvedistMethod.BeginInvoke(varr, tarr, i, null, null);
				IAList.Add(ar);
			}
			for(var i = 0; i < ThreadNum; i++)
			{
				pointdistList.AddRange(AvedistMethod.EndInvoke(IAList[i]));
			}
			return pointdistList.ToList().Average();
		}

		private static List<float> GetAveragePointDistance(Vector3[] varr, int[] tarr, int idx)
		{
			var pointdistList = new List<float>();
			for(var i = idx * 3; i < tarr.Length; i += ThreadNum * 3)
			{
				if(tarr[i] == 0 && tarr[i + 1] == 0 && tarr[i + 2] == 0)
				{
					continue;
				}
				var v0 = varr[tarr[i]];
				var v1 = varr[tarr[i + 1]];
				var v2 = varr[tarr[i + 2]];
				var d0 = Vector3.Distance(v0, v1);
				var d1 = Vector3.Distance(v1, v2);
				var d2 = Vector3.Distance(v2, v0);
				var q0 = (d0 + d1 + d2) / 3.0f;

				//if(IsBrushedArray[tarr[i]]==false)q0=0;
				//if(IsBrushedArray[tarr[i+1]]==false)q0=0;
				//if(IsBrushedArray[tarr[i+2]]==false)q0=0;
				pointdistList.Add(q0);
			}
			return pointdistList;
		}

		private static float GetAverageEdgeDist(Vector3[] varr, int[] tarr)
		{
			var IAList = new List<IAsyncResult>();
			var AveedgeMethod = new Func<Vector3[], int[], int, List<float>>(GetAverageEdgeDist);
			var edgedistList = new List<float>();
			for(var i = 0; i < ThreadNum; i++)
			{
				var ar = AveedgeMethod.BeginInvoke(varr, tarr, i, null, null);
				IAList.Add(ar);
			}
			for(var i = 0; i < ThreadNum; i++)
			{
				edgedistList.AddRange(AveedgeMethod.EndInvoke(IAList[i]));
			}
			return edgedistList.ToList().Average();
		}

		private static List<float> GetAverageEdgeDist(Vector3[] varr, int[] tarr, int idx)
		{
			var edgedistList = new List<float>();
			for(var i = idx * 3; i < tarr.Length; i += ThreadNum * 3)
			{
				if(tarr[i] == 0 && tarr[i + 1] == 0 && tarr[i + 2] == 0)
				{
					continue;
				}
				var v0 = varr[tarr[i]];
				var v1 = varr[tarr[i + 1]];
				var v2 = varr[tarr[i + 2]];
				var d0 = Vector3.Distance(v0, v1);

				//float d1 = Vector3.Distance(v1,v2);
				var d2 = Vector3.Distance(v2, v0);
				var q0 = d2 / (d2 + d0) * (v1 - v2) + v2;
				edgedistList.Add(Vector3.Cross(v2 - q0, v2 - v0).magnitude / Vector3.Distance(v0, v2));
			}
			return edgedistList;
		}

		private static List<int> GetAdjIndex(Vector3[] varr, int[] tarr, Vector3 vec)
		{
			var AdjIndex = new List<int>();
			for(var i = 0; i < tarr.Length; i += 3)
			{
				if(vec == varr[tarr[i]])
				{
					AdjIndex.Add(tarr[i + 1]);
					AdjIndex.Add(tarr[i + 2]);
				}
				if(vec == varr[tarr[i + 1]])
				{
					AdjIndex.Add(tarr[i + 2]);
					AdjIndex.Add(tarr[i]);
				}
				if(vec == varr[tarr[i + 2]])
				{
					AdjIndex.Add(tarr[i]);
					AdjIndex.Add(tarr[i + 1]);
				}
			}
			AdjIndex = AdjIndex.Distinct().ToList();
			return AdjIndex;
		}

		private static Vector3 GetSymmetry(Vector3 opos)
		{
			var sypos = opos;

			//sypos.x = (smode == SymmetryMode.X_axis ? -1 : 1) * opos.x;
			//sypos.y = (smode == SymmetryMode.Y_axis ? -1 : 1) * opos.y;
			//sypos.z = (smode == SymmetryMode.Z_axis ? -1 : 1) * opos.z;
			switch(smode)
			{
				case SymmetryMode.X_axis:
					if(sypos.x != 0.0f)
					{
						sypos.x = -opos.x;
					}
					break;
				case SymmetryMode.Y_axis:
					if(sypos.y != 0.0f)
					{
						sypos.y = -opos.y;
					}
					break;
				case SymmetryMode.Z_axis:
					if(sypos.z != 0.0f)
					{
						sypos.z = -opos.z;
					}
					break;
			}
			return sypos;
		}

		private static Vector3 GetSymmetryAnim(Vector3 opos, Vector3 cpos)
		{
			var sypos = opos;
			switch(smode)
			{
				case SymmetryMode.X_axis:
					if(sypos.x != cpos.x)
					{
						sypos.x = cpos.x + (cpos.x - opos.x);
					}
					break;
				case SymmetryMode.Y_axis:
					if(sypos.y != cpos.y)
					{
						sypos.y = cpos.y + (cpos.y - opos.y);
					}
					break;
				case SymmetryMode.Z_axis:
					if(sypos.z != cpos.z)
					{
						sypos.z = cpos.z + (cpos.z - opos.z);
					}
					break;
			}
			return sypos;
		}

		private static float GetStroke(Vector3 vpos, Vector3 hitpos)
		{
			//float stroke = (hitpos-vpos).magnitude;
			var distv = hitpos - vpos;
			var stroke = distv.magnitude;
			var dx = distv.x;
			var dy = distv.y;
			var dz = distv.z;
			switch(bshape)
			{
				case BrushShape.Normal:
					stroke = (hitpos - vpos).magnitude;
					break;

				case BrushShape.SoftSolid:
					stroke = Mathf.Pow(Mathf.Abs(dx * dx * dx) + Mathf.Abs(dy * dy * dy) + Mathf.Abs(dz * dz * dz), 0.333f);
					break;

				case BrushShape.HardSolid:
					stroke = Mathf.Pow(Mathf.Abs(dx * dx * dx * dx * dx) + Mathf.Abs(dy * dy * dy * dy * dy) + Mathf.Abs(dz * dz * dz * dz * dz), 0.2f);
					break;

				case BrushShape.SoftSpike:
					stroke = Mathf.Abs(dx) + Mathf.Abs(dy) + Mathf.Abs(dz);
					break;

				case BrushShape.HardSpike:
					stroke = Mathf.Pow(Mathf.Abs(Mathf.Pow(Mathf.Abs(dx), 0.5f)) + Mathf.Abs(Mathf.Pow(Mathf.Abs(dy), 0.5f))
					                                                             + Mathf.Abs(Mathf.Pow(Mathf.Abs(dz), 0.5f)), 2.0f);
					break;
			}
			return stroke;
		}

		private static List<List<int>> GetAdjListList(Vector3[] varr, int[] tarr)
		{
			var adjlistlist = new List<List<int>>();
			for(var i = 0; i < varr.Length; i++)
			{
				var adjlist = new List<int>();
				adjlistlist.Add(adjlist);
			}

			for(var i = 0; i < tarr.Length; i += 3)
			{
				adjlistlist[tarr[i]].Add(tarr[i + 1]);
				adjlistlist[tarr[i]].Add(tarr[i + 2]);
				adjlistlist[tarr[i + 1]].Add(tarr[i]);
				adjlistlist[tarr[i + 1]].Add(tarr[i + 2]);
				adjlistlist[tarr[i + 2]].Add(tarr[i]);
				adjlistlist[tarr[i + 2]].Add(tarr[i + 1]);
			}
			for(var i = 0; i < tarr.Length; i++)
			{
				adjlistlist[tarr[i]].Distinct().ToList();
			}
			return adjlistlist;
		}

		private static List<List<int>> GetAdjListTri(Vector3[] varr, int[] tarr)
		{
			var adjlisttri = new List<List<int>>();
			for(var i = 0; i < varr.Length; i++)
			{
				var adjlist = new List<int>();
				adjlisttri.Add(adjlist);
			}

			for(var i = 0; i < tarr.Length; i += 3)
			{
				adjlisttri[tarr[i]].Add(tarr[i]);
				adjlisttri[tarr[i]].Add(tarr[i + 1]);
				adjlisttri[tarr[i]].Add(tarr[i + 2]);
				adjlisttri[tarr[i + 1]].Add(tarr[i + 1]);
				adjlisttri[tarr[i + 1]].Add(tarr[i]);
				adjlisttri[tarr[i + 1]].Add(tarr[i + 2]);
				adjlisttri[tarr[i + 2]].Add(tarr[i + 2]);
				adjlisttri[tarr[i + 2]].Add(tarr[i]);
				adjlisttri[tarr[i + 2]].Add(tarr[i + 1]);
			}
			return adjlisttri;
		}

		/*private static bool NotDouble(List<Vector3> vlist, Vector3 v)
		{
		    bool flag0 = false;
		    foreach(Vector3 vec in vlist)
		        if(Mathf.Approximately(vec.x,v.x) && Mathf.Approximately(vec.y,v.y) && Mathf.Approximately(vec.z,v.z))
		    {
		        flag0 = true;
		        break;
		    }
		    return flag0;
		}*/

		/*private static bool NotDoubleIndex(List<int> vindex, int i)
		{
		    bool flag0 = false;
		    foreach(int ind in vindex)
		        if(ind == i)
		    {
		        flag0 = true;
		        break;
		    }
		    return flag0;
		}*/

		private static List<BoneWeight1> BoneWeight1Lerp(List<BoneWeight1> bwlist0, List<BoneWeight1> bwlist1)
		{
			var Weight1IdxDict = new Dictionary<int, float>();
			for(var i = 0; i < bwlist0.Count; i++)
			{
				//BoneWeight1 bw0 = bwlist0[i];
				//float weight0 = 0.0f;
				//Weight1IdxDict.TryGetValue(bw0.boneIndex, out weight0);
				//Weight1IdxDict[bw0.boneIndex] = weight0 + bw0.weight;
				var bw0 = bwlist0[i];
				Weight1IdxDict[bw0.boneIndex] = bw0.weight;
			}
			for(var i = 0; i < bwlist1.Count; i++)
			{
				var bw0 = bwlist1[i];
				float weight0;
				Weight1IdxDict.TryGetValue(bw0.boneIndex, out weight0);
				Weight1IdxDict[bw0.boneIndex] = weight0 + bw0.weight;
			}
			var weight1list = new List<BoneWeight1>();
			foreach(var weightkv in Weight1IdxDict)
			{
				var bw0 = new BoneWeight1 { boneIndex = weightkv.Key, weight = weightkv.Value * 0.5f };
				if(bw0.weight > 0.1f)
				{
					weight1list.Add(bw0);
				}
			}
			if(weight1list.Count == 0)
			{
				var weight1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
				weight1list.Add(weight1);
			}
			return weight1list;
		}

		private static void AddBone()
		{
			//Add 2020/03/09
			UnloadMesh = currentMesh;

			//End Add 2020/03/09
			//New in 2019/10/02
			var IsUpOffscreen = true;
			if(currentSkinned.updateWhenOffscreen == false)
			{
				IsUpOffscreen = false;
				currentSkinned.updateWhenOffscreen = true;
			}

			//End New in 2019/10/02
			//New in 2020/11/10
			try
			{
				var anim = currentObject.transform.root.gameObject.GetComponent<Animator>();
				anim.runtimeAnimatorController = null;
			}
			catch
			{
			}

			//End New in 2020/11/10
			var transobj = new GameObject();
			var bones = currentSkinned.bones;
			var bonenamelist = new List<string>();
			for(var i = 0; i < bones.Length; i++)
			{
				bonenamelist.Add(bones[i].name);
			}
			transobj.name = ObjectNames.GetUniqueName(bonenamelist.ToArray(), "Bone");
			var parentpos = Vector3.zero;
			try
			{
				var trans = currentSkinned.bones[bones.Length - 1];
				if(parentidx >= 0)
				{
					trans = bones[parentidx];
				}
				parentpos = trans.position;
				GameObjectUtility.SetParentAndAlign(transobj, trans.gameObject);
			}
			catch
			{
				GameObjectUtility.SetParentAndAlign(transobj, currentObject.transform.root.gameObject);
				parentpos = currentObject.transform.root.position;
			}
			var bonepos = currentObject.transform.TransformPoint(BrushBoneHitPos * 0.5f + BrushHitPos * 0.5f);
			transobj.transform.localPosition = transobj.transform.InverseTransformPoint(bonepos);
			transobj.transform.localRotation = Quaternion.identity;
			var bonelist = new List<Transform>(bones);
			var bindposelist = new List<Matrix4x4>(currentMesh.bindposes);
			bonelist.Add(transobj.transform);
			bindposelist.Add(transobj.transform.worldToLocalMatrix * currentObject.transform.localToWorldMatrix);
			currentSkinned.bones = bonelist.ToArray();
			currentMesh.bindposes = bindposelist.ToArray();
			var vertices = currentMesh.vertices;
			var trapos = transobj.transform.position;
			var bweight1s = currentMesh.GetAllBoneWeights();
			var pervers = currentMesh.GetBonesPerVertex();
			var weight1list = new List<BoneWeight1>();
			var perverlist = new List<byte>();
			var cnt0 = 0;
			var BrushRadFix = GlobalBrushRad
				? BrushRadius
				  * HandleUtility.GetHandleSize(currentObject.transform.TransformPoint(BrushBoneHitPos)) * 2.0f
				: BrushRadius;
			for(var i = 0; i < vertices.Length; i++)
			{
				var v0 = vertices[i];
				v0 = currentObject.transform.TransformPoint(v0);
				var flag0 = false;

				var dist0 = Vector3.Dot((bonepos - parentpos).normalized, v0 - bonepos);
				if(dist0 < 0)
				{
					flag0 = true;
				}
				var nearpos = bonepos + (bonepos - parentpos).normalized * dist0;

				var d0 = Vector3.Distance(v0, nearpos);
				for(var j = 0; j < bonelist.Count - 1; j++)
				{
					var bone0 = bonelist[j];
					var d1 = Vector3.Distance(v0, bone0.position);
					if(d1 < d0)
					{
						flag0 = true;
					}
				}
				var p0 = (int)pervers[i];
				var minidx = -1;
				var minweight = 0.0f;
				for(var j = 0; j < p0; j++)
				{
					weight1list.Add(bweight1s[cnt0 + j]);

					var idx0 = cnt0 + j;
					var weight0 = bweight1s[idx0].weight;
					if(minweight == 0.0f || weight0 < minweight)
					{
						minweight = weight0;
						minidx = idx0;
					}
				}
				cnt0 += p0;
				perverlist.Add(pervers[i]);
				if(flag0 || d0 > BrushRadFix)
				{
					continue;
				}
				if(minidx < 0)
				{
					minidx = weight1list.Count - 1;
				}
				weight1list[minidx] = new BoneWeight1 { boneIndex = currentSkinned.bones.Length - 1, weight = 1.0f - d0 / BrushRadFix };
			}
			var weight1s = new NativeArray<BoneWeight1>(weight1list.ToArray(), Allocator.Temp);
			var PerVerts = new NativeArray<byte>(perverlist.ToArray(), Allocator.Temp);
			currentMesh.SetBoneWeights(PerVerts, weight1s);
			if(!IsUpOffscreen)
			{
				currentSkinned.updateWhenOffscreen = false;
			}
			if(CheckAnimationBrush(BrushString))
			{
				//NeedFrameSelected = true;
				AnimatorSetup(false);
			}
			EditorSculptPrefab(false, true);
			ChangeMaterial();

			//String temppath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".unitypackage");
			//String assetpath = AssetDatabase.GetAssetPath(currentMesh);
			//AssetDatabase.ExportPackage(assetpath, temppath);
			//AssetDatabase.DeleteAsset(assetpath);
			//AssetDatabase.ImportPackage(temppath, false);
			//AssetDatabase.Refresh();
			//AssetDatabase.DeleteAsset(temppath);
			//AssetDatabase.Refresh();
			delboneidx = -1;
			boneAct = BoneAction.None;
			parentidx = -1;
		}

		private static void AddBone2(Vector3 bonepos)
		{
			//Add 2020/03/09
			UnloadMesh = currentMesh;

			//End Add 2020/03/09
			//New in 2019/10/02
			var IsUpOffscreen = true;
			if(currentSkinned.updateWhenOffscreen == false)
			{
				IsUpOffscreen = false;
				currentSkinned.updateWhenOffscreen = true;
			}

			//End New in 2019/10/02
			//New in 2020/11/10
			try
			{
				var anim = currentObject.transform.root.gameObject.GetComponent<Animator>();
				anim.runtimeAnimatorController = null;
			}
			catch
			{
			}

			//End New in 2020/11/10
			var transobj = new GameObject();
			var bones = currentSkinned.bones;
			var bonenamelist = new List<string>();
			for(var i = 0; i < bones.Length; i++)
			{
				bonenamelist.Add(bones[i].name);
			}
			transobj.name = ObjectNames.GetUniqueName(bonenamelist.ToArray(), "Bone");
			var parentpos = Vector3.zero;
			try
			{
				var trans = currentSkinned.bones[bones.Length - 1];
				if(parentidx >= 0)
				{
					trans = bones[parentidx];
				}
				parentpos = trans.position;
				GameObjectUtility.SetParentAndAlign(transobj, trans.gameObject);
			}
			catch
			{
				GameObjectUtility.SetParentAndAlign(transobj, currentObject.transform.root.gameObject);
				parentpos = currentObject.transform.root.position;
			}

			//Vector3 bonepos = currentObject.transform.TransformPoint(BrushBoneHitPos * 0.5f + BrushHitPos * 0.5f);
			transobj.transform.localPosition = transobj.transform.InverseTransformPoint(bonepos);
			transobj.transform.localRotation = Quaternion.identity;
			var bonelist = new List<Transform>(bones);
			var bindposelist = new List<Matrix4x4>(currentMesh.bindposes);
			bonelist.Add(transobj.transform);
			bindposelist.Add(transobj.transform.worldToLocalMatrix * currentObject.transform.localToWorldMatrix);
			currentSkinned.bones = bonelist.ToArray();
			currentMesh.bindposes = bindposelist.ToArray();
			var vertices = currentMesh.vertices;
			var trapos = transobj.transform.position;
			var bweight1s = currentMesh.GetAllBoneWeights();
			var pervers = currentMesh.GetBonesPerVertex();
			var weight1list = new List<BoneWeight1>();
			var perverlist = new List<byte>();
			var cnt0 = 0;
			var BrushRadFix = GlobalBrushRad
				? BrushRadius
				  * HandleUtility.GetHandleSize(currentObject.transform.TransformPoint(BrushBoneHitPos)) * 2.0f
				: BrushRadius;
			for(var i = 0; i < vertices.Length; i++)
			{
				var v0 = vertices[i];
				v0 = currentObject.transform.TransformPoint(v0);
				var flag0 = false;

				var dist0 = Vector3.Dot((bonepos - parentpos).normalized, v0 - bonepos);
				if(dist0 < 0)
				{
					flag0 = true;
				}

				//Vector3 nearpos = bonepos + (bonepos - parentpos).normalized * dist0;
				var nearpos = parentpos + (bonepos - parentpos).normalized * dist0;

				var d0 = Vector3.Distance(v0, nearpos);
				for(var j = 0; j < bonelist.Count - 1; j++)
				{
					var bone0 = bonelist[j];
					var bone0p = bone0.position;
					var dist1 = Vector3.Dot((bone0p - bone0.parent.position).normalized, v0 - bone0p);

					//if (d1 > 0 && d1 < d0) flag0 = true;
					if(dist1 < 0)
					{
						continue;
					}
					var nbone0p = bone0.parent.position + (bone0p - bone0.parent.position).normalized * dist1;

					//Vector3 nbone0p = bone0p + (bone0p - bone0.parent.position).normalized * dist1;
					var d1 = Vector3.Distance(v0, nbone0p);

					//float d1 = Vector3.Distance(v0, bone0.position);
					if(d1 < d0)
					{
						flag0 = true;
					}
				}
				var p0 = (int)pervers[i];
				var minidx = -1;
				var minweight = 0.0f;
				for(var j = 0; j < p0; j++)
				{
					weight1list.Add(bweight1s[cnt0 + j]);

					var idx0 = cnt0 + j;
					var weight0 = bweight1s[idx0].weight;
					if(minweight == 0.0f || weight0 < minweight)
					{
						minweight = weight0;
						minidx = idx0;
					}
				}
				cnt0 += p0;
				perverlist.Add(pervers[i]);
				if(flag0 || d0 > BrushRadFix)
				{
					continue;
				}
				if(minidx < 0)
				{
					minidx = weight1list.Count - 1;
				}
				weight1list[minidx] = new BoneWeight1 { boneIndex = currentSkinned.bones.Length - 1, weight = 1.0f - d0 / BrushRadFix };

				//perverlist[perverlist.Count - 1] = (Byte)((int)perverlist[perverlist.Count - 1] + 1);
				//BoneWeight1 w1 = new BoneWeight1() { boneIndex = currentSkinned.bones.Length - 1, weight = 1.0f - (d0 / BrushRadFix) };
				// //BoneWeight1 w1 = new BoneWeight1() { boneIndex = currentSkinned.bones.Length - 1, weight = Mathf.Clamp01(2.0f - (d0 / BrushRadFix)) };
				//weight1list.Add(w1);
				//weight1list[weight1list.Count - 1] = new BoneWeight1() { boneIndex = currentSkinned.bones.Length - 1, weight = 1.0f - (d0 / BrushRadFix) };
			}
			var weight1s = new NativeArray<BoneWeight1>(weight1list.ToArray(), Allocator.Temp);
			var PerVerts = new NativeArray<byte>(perverlist.ToArray(), Allocator.Temp);
			currentMesh.SetBoneWeights(PerVerts, weight1s);
			if(!IsUpOffscreen)
			{
				currentSkinned.updateWhenOffscreen = false;
			}

			//New in 2020/11/11
			//AnimatorSetup(true);
			//End New in 2020/11/11
			//Changed 2020/11/19
			if(CheckAnimationBrush(BrushString))
			{
				//NeedFrameSelected = true;
				AnimatorSetup(false);
			}

			//End Changed 2020/11/19
			EditorSculptPrefab(false, true);
			ChangeMaterial();

			//String temppath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".unitypackage");
			//String assetpath = AssetDatabase.GetAssetPath(currentMesh);
			//AssetDatabase.ExportPackage(assetpath, temppath);
			//AssetDatabase.DeleteAsset(assetpath);
			//AssetDatabase.ImportPackage(temppath, false);
			//AssetDatabase.Refresh();
			//AssetDatabase.DeleteAsset(temppath);
			//AssetDatabase.Refresh();
			delboneidx = -1;
			boneAct = BoneAction.None;
			parentidx = -1;
		}

		private static void InsertBone()
		{
			UnloadMesh = currentMesh;
			var IsUpOffscreen = true;
			if(currentSkinned.updateWhenOffscreen == false)
			{
				IsUpOffscreen = false;
				currentSkinned.updateWhenOffscreen = true;
			}
			try
			{
				var anim = currentObject.transform.root.gameObject.GetComponent<Animator>();
				anim.runtimeAnimatorController = null;
			}
			catch
			{
			}
			var transobj = new GameObject();
			var bones = currentSkinned.bones;
			var bonenamelist = new List<string>();
			for(var i = 0; i < bones.Length; i++)
			{
				bonenamelist.Add(bones[i].name);
			}
			transobj.name = ObjectNames.GetUniqueName(bonenamelist.ToArray(), "Bone");
			var parentpos = Vector3.zero;
			try
			{
				var trans = currentSkinned.bones[bones.Length - 1];
				if(parentidx >= 0)
				{
					trans = bones[parentidx];
				}
				parentpos = trans.position;
				GameObjectUtility.SetParentAndAlign(transobj, trans.gameObject);
			}
			catch
			{
				GameObjectUtility.SetParentAndAlign(transobj, currentObject.transform.root.gameObject);
				parentpos = currentObject.transform.root.position;
			}
			var bonepos = currentObject.transform.TransformPoint(BrushBoneHitPos);
			transobj.transform.localPosition = transobj.transform.InverseTransformPoint(bonepos);
			transobj.transform.localRotation = Quaternion.identity;
			var bonelist = new List<Transform>(bones);
			var bindposelist = new List<Matrix4x4>(currentMesh.bindposes);
			bonelist.Add(transobj.transform);
			bindposelist.Add(transobj.transform.worldToLocalMatrix * currentObject.transform.localToWorldMatrix);
			currentSkinned.bones = bonelist.ToArray();
			currentMesh.bindposes = bindposelist.ToArray();
			var vertices = currentMesh.vertices;
			var trapos = transobj.transform.position;
			var bweight1s = currentMesh.GetAllBoneWeights();
			var pervers = currentMesh.GetBonesPerVertex();
			var weight1list = new List<BoneWeight1>();
			var perverlist = new List<byte>();
			var cnt0 = 0;
			var BrushRadFix = GlobalBrushRad
				? BrushRadius
				  * HandleUtility.GetHandleSize(currentObject.transform.TransformPoint(BrushBoneHitPos)) * 2.0f
				: BrushRadius;
			for(var i = 0; i < vertices.Length; i++)
			{
				var v0 = vertices[i];
				v0 = currentObject.transform.TransformPoint(v0);
				var flag0 = false;

				var dist0 = Vector3.Dot((bonepos - parentpos).normalized, v0 - bonepos);
				if(dist0 < 0)
				{
					flag0 = true;
				}
				var nearpos = bonepos + (bonepos - parentpos).normalized * dist0;

				var d0 = Vector3.Distance(v0, nearpos);
				for(var j = 0; j < bonelist.Count - 1; j++)
				{
					var bone0 = bonelist[j];
					var d1 = Vector3.Distance(v0, bone0.position);
					if(d1 < d0)
					{
						flag0 = true;
					}
				}
				var p0 = (int)pervers[i];
				var minidx = -1;
				var minweight = 0.0f;
				for(var j = 0; j < p0; j++)
				{
					weight1list.Add(bweight1s[cnt0 + j]);

					var idx0 = cnt0 + j;
					var weight0 = bweight1s[idx0].weight;
					if(minweight == 0.0f || weight0 < minweight)
					{
						minweight = weight0;
						minidx = idx0;
					}
				}
				cnt0 += p0;
				perverlist.Add(pervers[i]);
				if(flag0 || d0 > BrushRadFix)
				{
					continue;
				}
				if(minidx < 0)
				{
					minidx = weight1list.Count - 1;
				}
				weight1list[minidx] = new BoneWeight1 { boneIndex = currentSkinned.bones.Length - 1, weight = 1.0f - d0 / BrushRadFix };
			}
			var weight1s = new NativeArray<BoneWeight1>(weight1list.ToArray(), Allocator.Temp);
			var PerVerts = new NativeArray<byte>(perverlist.ToArray(), Allocator.Temp);
			currentMesh.SetBoneWeights(PerVerts, weight1s);
			if(!IsUpOffscreen)
			{
				currentSkinned.updateWhenOffscreen = false;
			}

			//New in 2020/11/11
			//AnimatorSetup(true);
			//End New in 2020/11/11
			//Changed 2020/11/19
			if(CheckAnimationBrush(BrushString))
			{
				//NeedFrameSelected = true;
				AnimatorSetup(false);
			}

			//End Changed 2020/11/19
			EditorSculptPrefab(false, true);
			ChangeMaterial();
			var temppath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".unitypackage");
			var assetpath = AssetDatabase.GetAssetPath(currentMesh);
			AssetDatabase.ExportPackage(assetpath, temppath);
			AssetDatabase.DeleteAsset(assetpath);
			AssetDatabase.ImportPackage(temppath, false);
			AssetDatabase.Refresh();
			AssetDatabase.DeleteAsset(temppath);
			AssetDatabase.Refresh();
			delboneidx = -1;
			boneAct = BoneAction.None;
			parentidx = -1;
		}

		private static void AddBoneSpike(Vector3 pos, bool IsParent, HashSet<Vector3> hashvec)
		{
			UnloadMesh = currentMesh;
			var IsUpOffscreen = true;
			if(currentSkinned.updateWhenOffscreen == false)
			{
				IsUpOffscreen = false;
				currentSkinned.updateWhenOffscreen = true;
			}
			try
			{
				var anim = currentObject.transform.root.gameObject.GetComponent<Animator>();
				anim.runtimeAnimatorController = null;
			}
			catch
			{
			}
			var transobj = new GameObject();
			var bones = currentSkinned.bones;
			var bonenamelist = new List<string>();
			for(var i = 0; i < bones.Length; i++)
			{
				bonenamelist.Add(bones[i].name);
			}
			transobj.name = ObjectNames.GetUniqueName(bonenamelist.ToArray(), "Bone");
			var parentpos = Vector3.zero;
			if(!IsParent)
			{
				try
				{
					var trans = currentSkinned.bones[bones.Length - 1];

					//Diabled 2021/02/10
					//if (parentidx >= 0) trans = bones[parentidx];
					//End disabled 2021/02/10
					parentpos = trans.position;
					GameObjectUtility.SetParentAndAlign(transobj, trans.gameObject);
				}
				catch
				{
					GameObjectUtility.SetParentAndAlign(transobj, currentObject.transform.root.gameObject);
					parentpos = currentObject.transform.root.position;
				}
			}
			else
			{
				GameObjectUtility.SetParentAndAlign(transobj, currentObject.transform.root.gameObject);
				parentpos = currentObject.transform.root.position;
			}
			var bonepos = currentObject.transform.TransformPoint(pos);
			transobj.transform.localPosition = transobj.transform.InverseTransformPoint(bonepos);
			transobj.transform.localRotation = Quaternion.identity;
			var bonelist = new List<Transform>(bones);
			var bindposelist = new List<Matrix4x4>(currentMesh.bindposes);
			bonelist.Add(transobj.transform);
			bindposelist.Add(transobj.transform.worldToLocalMatrix * currentObject.transform.localToWorldMatrix);
			currentSkinned.bones = bonelist.ToArray();
			currentMesh.bindposes = bindposelist.ToArray();
			var vertices = currentMesh.vertices;
			var trapos = transobj.transform.position;
			var bweight1s = currentMesh.GetAllBoneWeights();
			var pervers = currentMesh.GetBonesPerVertex();
			var weight1list = new List<BoneWeight1>();
			var perverlist = new List<byte>();
			var cnt0 = 0;
			var BrushRadFix = GlobalBrushRad
				? BrushRadius
				  * HandleUtility.GetHandleSize(currentObject.transform.TransformPoint(BrushBoneHitPos)) * 2.0f
				: BrushRadius;
			for(var i = 0; i < vertices.Length; i++)
			{
				var v0 = vertices[i];
				v0 = currentObject.transform.TransformPoint(v0);
				var flag0 = false;

				var dist0 = Vector3.Dot((bonepos - parentpos).normalized, v0 - bonepos);
				if(dist0 < 0)
				{
					flag0 = true;
				}
				if(!hashvec.Contains(v0))
				{
					flag0 = true;
				}
				var nearpos = bonepos + (bonepos - parentpos).normalized * dist0;

				var d0 = Vector3.Distance(v0, nearpos);
				for(var j = 0; j < bonelist.Count - 1; j++)
				{
					var bone0 = bonelist[j];
					var d1 = Vector3.Distance(v0, bone0.position);
					if(d1 < d0)
					{
						flag0 = true;
					}
				}
				var p0 = (int)pervers[i];
				var minidx = -1;
				var minweight = 0.0f;
				var flag1 = false;
				for(var j = 0; j < p0; j++)
				{
					if(bweight1s[cnt0 + j].weight != 1.0f)
					{
						flag1 = true;
					}
					weight1list.Add(bweight1s[cnt0 + j]);

					var idx0 = cnt0 + j;
					var weight0 = bweight1s[idx0].weight;
					if(minweight == 0.0f || weight0 < minweight)
					{
						minweight = weight0;
						minidx = idx0;
					}
				}
				cnt0 += p0;
				perverlist.Add(pervers[i]);
				if(flag1)
				{
					continue;
				}
				if(flag0 || d0 > BrushRadFix)
				{
					continue;
				}
				if(minidx < 0)
				{
					minidx = weight1list.Count - 1;
				}
				weight1list[minidx] = new BoneWeight1 { boneIndex = currentSkinned.bones.Length - 1, weight = 1.0f - d0 / BrushRadFix };
			}
			var weight1s = new NativeArray<BoneWeight1>(weight1list.ToArray(), Allocator.Temp);
			var PerVerts = new NativeArray<byte>(perverlist.ToArray(), Allocator.Temp);
			currentMesh.SetBoneWeights(PerVerts, weight1s);
			if(!IsUpOffscreen)
			{
				currentSkinned.updateWhenOffscreen = false;
			}
			/*if (CheckAnimationBrush(BrushString))
			{
			    NeedFrameSelected = true;
			    AnimatorSetup(false);
			}
			EditorSculptPrefab(true);
			ChangeMaterial();
			String temppath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".unitypackage");
			String assetpath = AssetDatabase.GetAssetPath(currentMesh);
			AssetDatabase.ExportPackage(assetpath, temppath);
			AssetDatabase.DeleteAsset(assetpath);
			AssetDatabase.ImportPackage(temppath, false);
			AssetDatabase.Refresh();
			AssetDatabase.DeleteAsset(temppath);
			AssetDatabase.Refresh();
			delboneidx = -1;
			boneAct = BoneAction.None;
			parentidx = -1;*/
		}

		private static void HumanBonePlace(Vector3 pos)
		{
			try
			{
				var anim = currentObject.transform.root.gameObject.GetComponent<Animator>();
				anim.runtimeAnimatorController = null;
			}
			catch
			{
			}

			var childs = GetChildObjects(humantrans.gameObject);
			var oldposlist = new List<Vector3>();
			for(var i = 0; i < childs.Length; i++)
			{
				oldposlist.Add(childs[i].transform.position);
			}
			humantrans.position = pos;
			for(var i = 0; i < childs.Length; i++)
			{
				childs[i].transform.position = oldposlist[i];
			}

			var bones = currentSkinned.bones;
			var traidx = 0;
			for(var i = 0; i < bones.Length; i++)
			{
				if(bones[i] == humantrans)
				{
					traidx = i;
					break;
				}
			}
			var bonelist = new List<Transform>(bones);
			var bindposelist = new List<Matrix4x4>(currentMesh.bindposes);
			bonelist[traidx] = humantrans;
			bindposelist[traidx] = humantrans.worldToLocalMatrix * currentObject.transform.localToWorldMatrix;
			currentSkinned.bones = bonelist.ToArray();
			currentMesh.bindposes = bindposelist.ToArray();
			var vertices = currentMesh.vertices;
			var trapos = humantrans.position;
			var bweight1s = currentMesh.GetAllBoneWeights();
			var pervers = currentMesh.GetBonesPerVertex();
			var weight1list = new List<BoneWeight1>();
			var perverlist = new List<byte>();
			var cnt0 = 0;
			var BrushRadFix = GlobalBrushRad
				? BrushRadius
				  * HandleUtility.GetHandleSize(currentObject.transform.TransformPoint(BrushBoneHitPos)) * 2.0f
				: BrushRadius;
			var bonepos = humantrans.position;
			var parentpos = humantrans.parent.position;
			for(var i = 0; i < vertices.Length; i++)
			{
				var v0 = vertices[i];
				v0 = currentObject.transform.TransformPoint(v0);
				var flag0 = false;
				var dist0 = Vector3.Dot((bonepos - parentpos).normalized, v0 - bonepos);
				if(dist0 < 0)
				{
					flag0 = true;
				}
				var nearpos = bonepos + (bonepos - parentpos).normalized * dist0;
				var d0 = Vector3.Distance(v0, nearpos);
				for(var j = 0; j < bonelist.Count; j++)
				{
					if(j == traidx)
					{
						continue;
					}
					var bone0 = bonelist[j];
					var d1 = Vector3.Distance(v0, bone0.position);
					if(d1 < d0)
					{
						flag0 = true;
					}
				}
				var p0 = (int)pervers[i];
				var minidx = -1;
				var minweight = 0.0f;
				for(var j = 0; j < p0; j++)
				{
					weight1list.Add(bweight1s[cnt0 + j]);
					var idx0 = cnt0 + j;
					var weight0 = bweight1s[idx0].weight;
					if(minweight == 0.0f || weight0 < minweight)
					{
						minweight = weight0;
						minidx = idx0;
					}
				}
				cnt0 += p0;
				perverlist.Add(pervers[i]);
				if(flag0 || d0 > BrushRadFix)
				{
					continue;
				}
				weight1list[minidx] = new BoneWeight1 { boneIndex = traidx, weight = 1.0f - d0 / BrushRadFix };
			}
			var weight1s = new NativeArray<BoneWeight1>(weight1list.ToArray(), Allocator.Temp);
			var PerVerts = new NativeArray<byte>(perverlist.ToArray(), Allocator.Temp);
			currentMesh.SetBoneWeights(PerVerts, weight1s);

			if(CheckAnimationBrush(BrushString))
			{
				//NeedFrameSelected = true;
				AnimatorSetup(false);
			}

			EditorSculptPrefab(false, true);
			ChangeMaterial();
			var temppath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".unitypackage");
			var assetpath = AssetDatabase.GetAssetPath(currentMesh);
			AssetDatabase.ExportPackage(assetpath, temppath);
			AssetDatabase.DeleteAsset(assetpath);
			AssetDatabase.ImportPackage(temppath, false);
			AssetDatabase.Refresh();
			AssetDatabase.DeleteAsset(temppath);
			AssetDatabase.Refresh();
			delboneidx = -1;

			//if (boneAct != BoneAction.HumanWizard) boneAct = BoneAction.None;
		}

		private static void BoneSpikeSplineAdd()
		{
			currentMesh.RecalculateNormals();
			var vertices = currentMesh.vertices;
			var subcnt = IsMonoSubmesh ? 1 : currentMesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(currentMesh.GetTriangles(i));
			}
			var hitpos = BrushHitPos;

			var r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			var rv = r.origin;
			var rd = currentObject.transform.InverseTransformDirection(r.direction);
			rd.x /= currentObject.transform.localScale.x;
			rd.y /= currentObject.transform.localScale.y;
			rd.z /= currentObject.transform.localScale.z;
			var newtriarrlist = new List<int[]>();
			for(var h = 0; h < subcnt; h++)
			{
				var triarr = triarrlist[h];
				var newtrilist = new List<int>();
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					if(Vector3.Distance(v0, hitpos) < avgPointDist * 2.0f)
					{
						continue;
					}
					if(Vector3.Distance(v1, hitpos) < avgPointDist * 2.0f)
					{
						continue;
					}
					if(Vector3.Distance(v2, hitpos) < avgPointDist * 2.0f)
					{
						continue;
					}

					newtrilist.AddRange(new[] { triarr[i], triarr[i + 1], triarr[i + 2] });
				}
				newtriarrlist.Add(newtrilist.ToArray());
			}

			var adjvdict = new Dictionary<Vector3, Vector3>();
			for(var h = 0; h < subcnt; h++)
			{
				var triarr = newtriarrlist[h];
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					var q0 = (v0 + v1) * 0.5f;
					var q1 = (v1 + v2) * 0.5f;
					var q2 = (v2 + v0) * 0.5f;
					Vector3 a0;
					var adjv0 = adjvdict.TryGetValue(q0, out a0);
					if(adjv0)
					{
						if(a0 != v2)
						{
							adjvdict[q0] = adjvdict[q0] + v2;
						}
					}
					else
					{
						adjvdict[q0] = v2;
					}
					Vector3 a1;
					var adjv1 = adjvdict.TryGetValue(q1, out a1);
					if(adjv1)
					{
						if(a1 != v0)
						{
							adjvdict[q1] = adjvdict[q1] + v0;
						}
					}
					else
					{
						adjvdict[q1] = v0;
					}
					Vector3 a2;
					var adjv2 = adjvdict.TryGetValue(q2, out a2);
					if(adjv2)
					{
						if(a2 != v1)
						{
							adjvdict[q2] = adjvdict[q2] + v1;
						}
					}
					else
					{
						adjvdict[q2] = v1;
					}
				}
			}

			var holehash = new HashSet<Vector3>();
			for(var h = 0; h < subcnt; h++)
			{
				var triarr = newtriarrlist[h];
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					var q0 = (v0 + v1) * 0.5f;
					var q1 = (v1 + v2) * 0.5f;
					var q2 = (v2 + v0) * 0.5f;
					var flags = Enumerable.Repeat(false, 3).ToArray();
					var cnt0 = 0;
					try
					{
						if(adjvdict[q0] == v2)
						{
							flags[0] = true;
							flags[1] = true;
							cnt0++;
						}
					}
					catch
					{
					}
					try
					{
						if(adjvdict[q1] == v0)
						{
							flags[1] = true;
							flags[2] = true;
							cnt0++;
						}
					}
					catch
					{
					}
					try
					{
						if(adjvdict[q2] == v1)
						{
							flags[2] = true;
							flags[0] = true;
							cnt0++;
						}
					}
					catch
					{
					}
					if(flags[0])
					{
						holehash.Add(v0);
					}
					if(flags[1])
					{
						holehash.Add(v1);
					}
					if(flags[2])
					{
						holehash.Add(v2);
					}
				}
			}
			for(var i = 0; i < subcnt; i++)
			{
				currentMesh.SetTriangles(newtriarrlist[i], i);
			}
			MergeVertsFast(currentMesh);

			var avgdist = 0.0f;
			foreach(var vec in holehash)
			{
				avgdist += Vector3.Distance(vec, hitpos);
			}
			avgdist /= holehash.Count;

			vertices = currentMesh.vertices;
			for(var i = 0; i < vertices.Length; i++)
			{
				var v0 = vertices[i];
				if(holehash.Contains(v0))
				{
					var dist = Vector3.Distance(v0, hitpos);
					if(dist != avgdist)
					{
						vertices[i] = hitpos + (v0 - hitpos).normalized * avgdist * 1.0f;
					}
					holehash.Add(vertices[i]);
				}
			}
			currentMesh.vertices = vertices;
			MergeVertsFast(currentMesh);

			triarrlist = new List<int[]>();
			for(var i = 0; i < currentMesh.subMeshCount; i++)
			{
				triarrlist.Add(currentMesh.GetTriangles(i));
			}

			var AdjIdxListList = new List<List<int>>();
			for(var i = 0; i < vertices.Length; i++)
			{
				AdjIdxListList.Add(new List<int>());
			}
			for(var i = 0; i < subcnt; i++)
			{
				var triarr = triarrlist[i];
				for(var j = 0; j < triarr.Length; j += 3)
				{
					var i0 = triarr[j];
					var i1 = triarr[j + 1];
					var i2 = triarr[j + 2];
					AdjIdxListList[triarr[j]].AddRange(new[] { i1, i2 });
					AdjIdxListList[triarr[j + 1]].AddRange(new[] { i2, i0 });
					AdjIdxListList[triarr[j + 2]].AddRange(new[] { i0, i1 });
				}
			}
			var dc0 = avgPointDist * 0.5f * 1.0f;
			for(var i = 0; i < vertices.Length; i++)
			{
				var v0 = vertices[i];
				var AdjIdxList = AdjIdxListList[i];
				foreach(var t in AdjIdxList)
				{
					var vpos = vertices[t];
					var d0 = Vector3.Distance(v0, vpos);
					if(d0 < dc0 && t != 0)
					{
						if(holehash.Contains(vertices[t]))
						{
							if(!holehash.Contains(v0))
							{
								continue;
							}
						}
						vertices[t] = v0;
					}
				}
			}
			currentMesh.vertices = vertices;
			MergeVertsFast(currentMesh);

			vertices = currentMesh.vertices;
			var colors = currentMesh.colors;
			var uvs = currentMesh.uv;
			var uv2s = currentMesh.uv2;
			var uv3s = currentMesh.uv3;
			var uv4s = currentMesh.uv4;

			//triarrlist = new List<int[]>(newtriarrlist);
			triarrlist = new List<int[]>();
			for(var i = 0; i < currentMesh.subMeshCount; i++)
			{
				triarrlist.Add(currentMesh.GetTriangles(i));
			}

			var avgnorm = Vector3.zero;
			var avgcnt = 0;
			var hasharr = Enumerable.Repeat(false, vertices.Length).ToArray();
			newtriarrlist = new List<int[]>();
			for(var h = 0; h < triarrlist.Count; h++)
			{
				var triarr = triarrlist[h];
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					var cnt0 = 0;
					if(holehash.Contains(v0))
					{
						hasharr[triarr[i]] = true;
						cnt0++;
					}
					if(holehash.Contains(v1))
					{
						hasharr[triarr[i + 1]] = true;
						cnt0++;
					}
					if(holehash.Contains(v2))
					{
						hasharr[triarr[i + 2]] = true;
						cnt0++;
					}
					if(cnt0 != 2)
					{
						continue;
					}
					avgnorm += Vector3.Cross(v1 - v0, v0 - v2).normalized;
					avgcnt++;
				}
				newtriarrlist.Add(triarr);
			}
			avgnorm /= avgcnt;
			avgnorm = avgnorm.normalized;
			if(Vector3.Dot(avgnorm, rd) < 0)
			{
				avgnorm = -avgnorm;
			}

			var boneposlist = new List<Vector3>();

			var newvlist = new List<Vector3>(vertices);
			var newcollist = new List<Color>(colors);
			var newuvlist = new List<Vector2>(uvs);
			var newuv2list = new List<Vector2>(uv2s);
			var newuv3list = new List<Vector2>(uv3s);
			var newuv4list = new List<Vector2>(uv4s);
			newtriarrlist = new List<int[]>();
			var cntc = 0;
			var newhash = new HashSet<Vector3>();
			for(var h = 0; h < triarrlist.Count; h++)
			{
				if(h == 0)
				{
					var triarr = triarrlist[h];
					var newtrilist = new List<int>(triarr);

					var edgelist = new List<int>();
					for(var i = 0; i < triarr.Length; i += 3)
					{
						var t0 = triarr[i];
						var t1 = triarr[i + 1];
						var t2 = triarr[i + 2];
						if(hasharr[t0] && hasharr[t1] && !hasharr[t2])
						{
							edgelist.AddRange(new[] { t0, t1 });
						}
						if(hasharr[t0] && !hasharr[t1] && hasharr[t2])
						{
							edgelist.AddRange(new[] { t2, t0 });
						}
						if(!hasharr[t0] && hasharr[t1] && hasharr[t2])
						{
							edgelist.AddRange(new[] { t1, t2 });
						}
					}
					var newedgelist = new List<int>(edgelist);
					var loopcnt = 10;
					for(var a = 0; a < loopcnt; a++)
					{
						if(a == loopcnt - 1)
						{
							var cntcc = newvlist.Count;
							var vcc0 = hitpos - avgnorm * avgPointDist * 2.0f * (a + 2.0f);
							newvlist.Add(vcc0);
							newcollist.Add(Color.white);
							newuvlist.Add(Vector2.one);
							newuv2list.Add(Vector2.one);
							newuv3list.Add(Vector2.one);
							newuv4list.Add(Vector2.one);
							newhash.Add(vcc0);
							for(var i = 0; i < newedgelist.Count; i += 2)
							{
								var e0 = newedgelist[i];
								var e1 = newedgelist[i + 1];
								newtrilist.AddRange(new[] { e0, cntcc, e0 + 1 });
							}
							break;
						}
						var nextedgelist = new List<int>();
						var vc0 = hitpos - avgnorm * avgPointDist * 2.0f * (a + 1.0f);
						boneposlist.Add(vc0);
						for(var i = 0; i < newedgelist.Count; i += 2)
						{
							var e0 = newedgelist[i];
							var e1 = newedgelist[i + 1];
							var v0 = newvlist[e0];
							var v1 = newvlist[e1];
							cntc = newvlist.Count;
							var vp0 = v0 - avgnorm * avgPointDist * 2.0f;
							var vp1 = v1 - avgnorm * avgPointDist * 2.0f;
							var f0 = (a - 2) / (float)loopcnt;

							var calc0 = f0 * f0;

							var d0 = 1.0f - Mathf.Clamp(calc0, 0.01f, 0.99f);
							vp0 = (vp0 - vc0) * d0 + vc0;
							vp1 = (vp1 - vc0) * d0 + vc0;

							newvlist.AddRange(new[] { vp0, vp1 });
							newcollist.AddRange(new[] { Color.white, Color.white });
							newuvlist.AddRange(new[] { Vector2.one, Vector2.one });
							newuv2list.AddRange(new[] { Vector2.one, Vector2.one });
							newuv3list.AddRange(new[] { Vector2.one, Vector2.one });
							newuv4list.AddRange(new[] { Vector2.one, Vector2.one });
							newhash.Add(vp0);
							newhash.Add(vp1);
							newtrilist.AddRange(new[] { e0, cntc, cntc + 1, e0, cntc + 1, e1 });
							nextedgelist.AddRange(new[] { cntc, cntc + 1 });
						}
						newedgelist = new List<int>(nextedgelist);
					}
					newtriarrlist.Add(newtrilist.ToArray());
				}
			}
			currentMesh.vertices = newvlist.ToArray();
			currentMesh.colors = newcollist.ToArray();
			currentMesh.uv = newuvlist.ToArray();
			currentMesh.uv2 = newuv2list.ToArray();
			currentMesh.uv3 = newuv3list.ToArray();
			currentMesh.uv4 = newuv4list.ToArray();
			var cnt1 = newtriarrlist.Count;
			for(var i = 0; i < cnt1; i++)
			{
				currentMesh.SetTriangles(newtriarrlist[i], i);
			}
			MergeVertsFast(currentMesh);

			//Anmation part
			//Still buggy 2021/02/02
			cnt1 = boneposlist.Count;
			var intval = Mathf.Max((cnt1 - cnt1 % 3) / 3, 0);

			//List<Vector3> splinelist = new List<Vector3>();
			for(var i = 0; i < cnt1; i++)
			{
				if(i == 0 || i == 1 || i % intval == 0)
				{
					AddBoneSpike(boneposlist[i], i == 0, newhash);

					//splinelist.Add(boneposlist[i]);
				}
			}

			//Spline3DListList.Add(splinelist);
			//Spline3DUpdate4();
			//Spline3DSave3();

			//Add 2021/02/03
			//NeedFrameSelected = true;
			AnimatorSetup(false);

			//End Added 2021/02/03

			EditorUtility.DisplayDialog("Caution", "You add a bone spike", "OK");

			//Added 2021/02/04
			GUIUtility.ExitGUI();

			//End Added 2021/02/04

			//Added 2021/02/04
			/*EditorSculptPrefab(true);
			ChangeMaterial();
			String temppath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".unitypackage");
			String assetpath = AssetDatabase.GetAssetPath(currentMesh);
			AssetDatabase.ExportPackage(assetpath, temppath);
			AssetDatabase.DeleteAsset(assetpath);
			AssetDatabase.ImportPackage(temppath, false);
			AssetDatabase.Refresh();
			AssetDatabase.DeleteAsset(temppath);
			AssetDatabase.Refresh();
			delboneidx = -1;
			boneAct = BoneAction.None;
			parentidx = -1;*/
			//End Added 2021/02/04
		}

		private static bool AnimatorSetup(bool isaddbone)
		{
			if(!currentObject)
			{
				return false;
			}
			if(!BuildSkinnedMeshRenderer())
			{
				return false;
			}
			if(AnimationMode.InAnimationMode())
			{
				AnimationMode.StopAnimationMode();
			}
			var roott = currentObject.transform.root;
			var anim = roott.gameObject.GetComponent<Animator>() == true ? roott.gameObject.GetComponent<Animator>() : (Animator)Undo.AddComponent(roott.gameObject, typeof(Animator));

			//anim.enabled = false;
			try
			{
				var aclips = anim.runtimeAnimatorController.animationClips;
				if(aclips.Length > 0 && !isaddbone)
				{
					IsAnimationBrush = CheckAnimationBrush(BrushString);
					if(IsAnimationBrush)
					{
						AnimationModeStart();
					}
					return false;
				}
			}
			catch
			{
			}

			//Need for Unity 2019
			if(!currentObject)
			{
				return true;
			}
			anim.applyRootMotion = false;
			var aclip = new AnimationClip();
			var aclipPath = AssetDatabase.GenerateUniqueAssetPath(SaveFolderPath + currentObject.name + ".anim");

			//String aclipPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".anim");
			//Changed 2025/02/12
			// //String tempstr = new String(aclipPath);
			//String tempstr = new StringBuilder(aclipPath).ToString();
			//tempstr.Remove(tempstr.LastIndexOf("/"));
			//tempstr = tempstr.Replace(".anim", "").Replace("Assets/", "");

			//String assetPath = EditorUtility.SaveFilePanelInProject("SaveAnimationClip", tempstr, "anim", "Save AnimationClip");
			//AssetDatabase.CreateAsset(aclip, assetPath);
			AssetDatabase.CreateAsset(aclip, aclipPath);

			//End Changed 2025/02/12

			AnimationBind(anim, aclip);

			if(anim.runtimeAnimatorController == null)
			{
				//String ContrPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".controller");
				//if (ContrPath != "Assets/" + currentObject.name + ".controller")
				//{
				//    String TempPath = IsSkipOverrideAnim ? "Assets/" + currentObject.name + ".controller"
				//        : EditorUtility.SaveFilePanelInProject("SaveController", currentObject.name, "controller", "Save AnimatorController");
				//    if (TempPath != "") ContrPath = TempPath;
				//}
				var ContrPath = AssetDatabase.GenerateUniqueAssetPath(SaveFolderPath + currentObject.name + ".controller");
				if(ContrPath != SaveFolderPath + currentObject.name + ".controller")
				{
					var TempPath = IsSkipOverrideAnim
						? SaveFolderPath + currentObject.name + ".controller"
						: EditorUtility.SaveFilePanelInProject("SaveController", currentObject.name, "controller", "Save AnimatorController");
					if(TempPath != "")
					{
						ContrPath = TempPath;
					}
				}
				AnimatorController.CreateAnimatorControllerAtPathWithClip(ContrPath, aclip);
				anim.runtimeAnimatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath(ContrPath, typeof(AnimatorController));
				AssetDatabase.SetLabels(anim.runtimeAnimatorController, new[] { "EditorSculpt" });
			}
			else
			{
				var labels = AssetDatabase.GetLabels(anim.runtimeAnimatorController);
				var flag0 = false;
				for(var i = 0; i < labels.Length; i++)
				{
					if(labels[i] == "EditorSculpt")
					{
						flag0 = true;
						break;
					}
				}
				if(flag0)
				{
					var animatorCon = (AnimatorController)anim.runtimeAnimatorController;
					animatorCon.AddMotion(aclip);
				}
				else
				{
					var acliplist = anim.runtimeAnimatorController.animationClips.ToList();

					//String ContrPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".controller");
					var ContrPath = AssetDatabase.GenerateUniqueAssetPath(SaveFolderPath + currentObject.name + ".controller");
					AnimatorController.CreateAnimatorControllerAtPathWithClip(ContrPath, aclip);
					anim.runtimeAnimatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath(ContrPath, typeof(AnimatorController));
					var animatorCon = (AnimatorController)anim.runtimeAnimatorController;
					for(var i = 0; i < acliplist.Count; i++)
					{
						animatorCon.AddMotion(acliplist[i]);
					}
					var labellist = new List<string>(labels);
					labellist.Add("EditorSculpt");
					AssetDatabase.SetLabels(anim.runtimeAnimatorController, labellist.ToArray());
				}
			}
			EditorSculptPrefab(false, true);

			//if (BrushString == "AnimationMove") AnimationModeStart();
			//IsAnimationBrush = CheckAnimationBrush(BrushString);
			//if (IsAnimationBrush) AnimationModeStart();
			var curveBinsds = AnimationUtility.GetCurveBindings(aclip);
			if(curveBinsds.Length < 1)
			{
				AnimationBind(anim, aclip);

				//AssetDatabase.DeleteAsset(assetPath);
				//DestroyImmediate(aclip);

				//if (anim.avatar == null || !anim.avatar.isHuman)
				//{
				//    GameObjectRecorder gorec = new GameObjectRecorder(currentObject.transform.root.gameObject);
				//    gorec.BindComponentsOfType<Transform>(currentObject.transform.root.gameObject, true);
				//    gorec.BindComponentsOfType<SkinnedMeshRenderer>(currentObject, true);
				//    gorec.TakeSnapshot(0.0f);
				//    gorec.TakeSnapshot(1.0f);
				//    gorec.SaveToClip(aclip, 2);
				//    Debug.Log("hogehoge");
				//}
			}
			IsAnimationBrush = CheckAnimationBrush(BrushString);
			if(IsAnimationBrush)
			{
				AnimationModeStart();
			}

			return true;

			//if(anim.avatar.isHuman && !aclip.humanMotion)
			//{
			//    DestroyImmediate(aclip);
			//    AssetDatabase.DeleteAsset(assetPath);
			//}
		}

		private static void AnimationBind(Animator anim, AnimationClip aclip)
		{
			if(anim.avatar == null || !anim.avatar.isHuman)
			{
				var gorec = new GameObjectRecorder(currentObject.transform.root.gameObject);
				gorec.BindComponentsOfType<Transform>(currentObject.transform.root.gameObject, true);

				//Changed 2025/06/08 to fix bug that material transparent lost in VRoid Studio model.
				if(IsAnimateMaterial)
				{
					gorec.BindComponentsOfType<SkinnedMeshRenderer>(currentObject, true);
				}

				//End Changed 2025/06/08
				gorec.TakeSnapshot(0.0f);
				gorec.TakeSnapshot(1.0f);
				gorec.SaveToClip(aclip, 2);
			}
			else
			{
				var gorec = new GameObjectRecorder(currentObject.transform.root.gameObject);
				gorec.BindComponentsOfType<Animator>(currentObject.transform.root.gameObject, true);

				//Changed 2025/06/08 to fix bug that material transparent lost in VRoid Studio model.
				if(IsAnimateMaterial)
				{
					gorec.BindComponentsOfType<SkinnedMeshRenderer>(currentObject, true);
				}

				//End Changed 2025/06/08
				gorec.TakeSnapshot(0.0f);
				gorec.TakeSnapshot(1.0f);
				gorec.SaveToClip(aclip, 2);

				var posehand = new HumanPoseHandler(anim.avatar, currentObject.transform.root);
				var musclenames = HumanTrait.MuscleName;
				var humanPose = new HumanPose();
				posehand.GetHumanPose(ref humanPose);
				var musclefs = humanPose.muscles;
				var bindings = AnimationUtility.GetCurveBindings(aclip);
				for(var i = 0; i < bindings.Length; i++)
				{
					var binding = bindings[i];
					if(binding.type != typeof(Animator))
					{
						continue;
					}
					var bindstr = binding.propertyName;
					var curve = AnimationUtility.GetEditorCurve(aclip, binding);
					var ismuscle = false;
					var bindf = -1.0f;

					//UnityEngine.Object obj;
					//AnimationUtility.GetObjectReferenceValue(currentObject.transform.root.gameObject,binding, out obj);
					//if(obj!=null)Debug.Log(obj.name);

					for(var j = 0; j < musclenames.Length; j++)
					{
						//Changed 2025/8/18
						var compstr0 = musclenames[j].Replace(".", "").Replace(" ", "").Replace("Hand", "").Replace("Animator", "");
						var compstr1 = bindstr.Replace(".", "").Replace(" ", "").Replace("Hand", "").Replace("Animator", "");
						if(!compstr0.Equals(compstr1))
						{
							continue;
						}

						//End Changed 2025/8/18

						//if (bindstr != musclenames[j]) continue;
						bindf = musclefs[j];
						ismuscle = true;
						break;
					}
					if(!ismuscle)
					{
						var bindq = Quaternion.identity;
						var bindt = Vector3.zero;
						if(bindstr.StartsWith("Motion"))
						{
							bindq = anim.rootRotation;
							bindt = anim.rootPosition;
						}
						else if(bindstr.StartsWith("Root"))
						{
							bindq = Quaternion.identity;
							bindt = currentMesh.bounds.center;

							//bindq = currentObject.transform.root.localRotation;
							//bindt = currentObject.transform.root.localPosition;

							//bindq = Quaternion.identity;
							//bindt = Vector3.zero;

							//if (currentSkinned != null)
							//{
							//    if (currentSkinned.rootBone != null)
							//    {
							//        bindq = currentSkinned.rootBone.localRotation;
							//        bindt = currentSkinned.rootBone.localPosition;
							//    }
							//}

							//Transform rootTra = GetRootTransform();
							//if(rootTra!=null)
							//{
							//    bindq = rootTra.localRotation;
							//    bindt = rootTra.localPosition;
							//    //bindq = rootTra.rotation;
							//    //bindt = rootTra.position;
							//}
							//else
							//{
							//    bindq = Quaternion.identity;
							//    bindt = Vector3.zero;
							//}
						}
						else if(bindstr.StartsWith("LeftFoot"))
						{
							var tra = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
							var lfootpos = tra.position;
							var lfootq = tra.rotation;
							var GetPostRotate = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
							if(GetPostRotate == null)
							{
								continue;
							}
							var GetAxisLength = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
							if(GetAxisLength == null)
							{
								continue;
							}
							var postq = (Quaternion)GetPostRotate.Invoke(anim.avatar, new object[] { HumanBodyBones.LeftFoot });
							lfootq *= postq;
							var axislen = (float)GetAxisLength.Invoke(anim.avatar, new object[] { HumanBodyBones.LeftFoot });
							lfootpos += lfootq * new Vector3(axislen, 0, 0);
							bindq = lfootq;

							//bindt = skinmatrix.MultiplyPoint(( lfootpos - currentSkinned.bounds.center)/anim.humanScale);
							bindt = (lfootpos - currentSkinned.bounds.center) / anim.humanScale;
						}
						else if(bindstr.StartsWith("RightFoot"))
						{
							var tra = anim.GetBoneTransform(HumanBodyBones.RightFoot);
							var rfootpos = tra.position;
							var rfootq = tra.rotation;
							var GetPostRotate = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
							if(GetPostRotate == null)
							{
								continue;
							}
							var GetAxisLength = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
							if(GetAxisLength == null)
							{
								continue;
							}
							var postq = (Quaternion)GetPostRotate.Invoke(anim.avatar, new object[] { HumanBodyBones.RightFoot });
							rfootq *= postq;
							var axislen = (float)GetAxisLength.Invoke(anim.avatar, new object[] { HumanBodyBones.RightFoot });
							rfootpos += rfootq * new Vector3(axislen, 0, 0);
							bindq = rfootq;
							bindt = (rfootpos - currentSkinned.bounds.center) / anim.humanScale;
						}
						else if(bindstr.StartsWith("LeftHand"))
						{
							var tra = anim.GetBoneTransform(HumanBodyBones.LeftHand);
							var lfootpos = tra.position;
							var lfootq = tra.rotation;
							var GetPostRotate = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
							if(GetPostRotate == null)
							{
								continue;
							}
							var GetAxisLength = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
							if(GetAxisLength == null)
							{
								continue;
							}
							var postq = (Quaternion)GetPostRotate.Invoke(anim.avatar, new object[] { HumanBodyBones.LeftHand });
							lfootq *= postq;
							var axislen = (float)GetAxisLength.Invoke(anim.avatar, new object[] { HumanBodyBones.LeftHand });
							lfootpos += lfootq * new Vector3(axislen, 0, 0);
							bindq = lfootq;
							bindt = (lfootpos - currentSkinned.bounds.center) / anim.humanScale;
						}
						else if(bindstr.StartsWith("RightHand"))
						{
							var tra = anim.GetBoneTransform(HumanBodyBones.RightHand);
							var rfootpos = tra.position;
							var rfootq = tra.rotation;
							var GetPostRotate = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
							if(GetPostRotate == null)
							{
								continue;
							}
							var GetAxisLength = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
							if(GetAxisLength == null)
							{
								continue;
							}
							var postq = (Quaternion)GetPostRotate.Invoke(anim.avatar, new object[] { HumanBodyBones.RightHand });
							rfootq *= postq;
							var axislen = (float)GetAxisLength.Invoke(anim.avatar, new object[] { HumanBodyBones.RightHand });
							rfootpos += rfootq * new Vector3(axislen, 0, 0);
							bindq = rfootq;
							bindt = (rfootpos - currentSkinned.bounds.center) / anim.humanScale;
						}
						if(bindstr.EndsWith("Q.x"))
						{
							bindf = bindq.x;
						}
						else if(bindstr.EndsWith("Q.y"))
						{
							bindf = bindq.y;
						}
						else if(bindstr.EndsWith("Q.z"))
						{
							bindf = bindq.z;
						}
						else if(bindstr.EndsWith("Q.w"))
						{
							bindf = bindq.w;
						}
						else if(bindstr.EndsWith("T.x"))
						{
							bindf = bindt.x;
						}
						else if(bindstr.EndsWith("T.y"))
						{
							bindf = bindt.y;
						}
						else if(bindstr.EndsWith("T.z"))
						{
							bindf = bindt.z;
						}
						else if(bindstr.EndsWith("TDOF.x"))
						{
							bindf = 0.0f;
						}
						else if(bindstr.EndsWith("TODF.y"))
						{
							bindf = 0.0f;
						}
						else if(bindstr.EndsWith("TDOF.z"))
						{
							bindf = 0.0f;
						}
					}
					var keys = curve.keys;
					for(var j = 0; j < keys.Length; j++)
					{
						keys[j].value = bindf;
					}
					curve.keys = keys;
					AnimationUtility.SetEditorCurve(aclip, binding, curve);
				}
				var bones = currentSkinned.bones;
				AnimationClipBind(aclip, anim, bones);
			}
		}

		private static void AnimationModeStart()
		{
			if(!currentObject)
			{
				return;
			}
			BoneMinIdx = -1;
			var roott = currentObject.transform.root;
			var anim = roott.gameObject.GetComponent<Animator>() == true ? roott.gameObject.GetComponent<Animator>() : null;
			if(anim != null)
			{
				if(anim.avatar == null)
				{
					string contrpath = null;
					try
					{
						contrpath = AssetDatabase.GetAssetPath(anim.runtimeAnimatorController);
					}
					catch
					{
					}
					if(contrpath != null)
					{
						anim.avatar = (Avatar)AssetDatabase.LoadAssetAtPath(contrpath, typeof(Avatar));
					}
					if(anim.avatar == null)
					{
						Avatar avatar = null;
						if(anim.hasRootMotion)
						{
							avatar = AvatarBuilder.BuildGenericAvatar(currentObject, currentObject.transform.root.name);
						}
						else
						{
							avatar = AvatarBuilder.BuildGenericAvatar(currentObject, "");
						}
						avatar.name = currentObject.name;
						try
						{
							AssetDatabase.AddObjectToAsset(avatar, anim.runtimeAnimatorController);
						}
						catch
						{
							//String assetpath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".avatar");
							var assetpath = AssetDatabase.GenerateUniqueAssetPath(SaveFolderPath + currentObject.name + ".avatar");
							AssetDatabase.CreateAsset(avatar, assetpath);
						}
						AssetDatabase.SaveAssets();
						anim.avatar = avatar;
					}
				}
				else
				{
					Avatar avatar = null;
					try
					{
						avatar = (Avatar)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(anim.runtimeAnimatorController), typeof(Avatar));
					}
					catch
					{
					}
					if(avatar == null)
					{
						avatar = AvatarBuilder.BuildGenericAvatar(currentObject, "");
						EditorUtility.CopySerialized(anim.avatar, avatar);
						try
						{
							AssetDatabase.AddObjectToAsset(avatar, anim.runtimeAnimatorController);
						}
						catch
						{
							//String assetpath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".avatar");
							var assetpath = AssetDatabase.GenerateUniqueAssetPath(SaveFolderPath + currentObject.name + ".avatar");
							AssetDatabase.CreateAsset(avatar, assetpath);
						}
						AssetDatabase.SaveAssets();
						anim.avatar = avatar;
						EditorSculptPrefab(false, true);
					}
				}
			}

			//if(IsDisabledAnimationImport == true)ModelImporterChangeAnimeImport(true);
			AnimationMode.StartAnimationMode();

			////Added 2025/09/13
			//if (currentSkinned != null)
			//{
			//    Transform[] bones = currentSkinned.bones;
			//    Matrix4x4[] binds = currentMesh.bindposes;
			//    for (int i = 0; i < bones.Length; i++)
			//    {
			//        binds[i] = bones[i].worldToLocalMatrix * currentObject.transform.localToWorldMatrix;
			//    }
			//    currentMesh.bindposes = binds;
			//}
			////End added 2025/09/13

			var aclips = AnimationUtility.GetAnimationClips(roott.gameObject);
			if(anim == null || anim.avatar == null || !anim.avatar.isHuman || aclips.Length < 1)
			{
			}
			else
			{
				try
				{
					var aclip = aclips[aclipidx];
					var bindings = AnimationUtility.GetCurveBindings(aclip);
					for(var i = 0; i < bindings.Length; i++)
					{
						var binding = bindings[i];
						AnimationMode.AddEditorCurveBinding(roott.gameObject, binding);
					}
				}
				catch
				{
				}
			}

			//Added 2025/09/12 to backup bindpose to fix bug that animtion not work.
			//if(oldbind!=null) currentMesh.bindposes = oldbind;
			currentMesh.bindposes = GetBindPoseFromImporter();

			//End added 2025/09/12
			AnimationPoseLoad(animeslider);
#if MemoryAnimation
			try
			{
				AnimationClip aclip = aclips[aclipidx];
				EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(aclip);
				List<AnimationCurve> curveList = new List<AnimationCurve>();
				for(int i = 0; i < bindings.Length; i++)
				{
					EditorCurveBinding binding = bindings[i];
					curveList.Add(AnimationUtility.GetEditorCurve(aclip, binding));
				}
				memoryBindings = bindings;
				memoryCurves = curveList.ToArray();
				IsAnimationSaved = true;
			}
			catch
			{
			}

			//SceneView.lastActiveSceneView.FrameSelected();
#endif
		}

		private static void AnimationSaveBoneMove(float f0, float f1, bool isOverride)
		{
			if(!IsBoneMove)
			{
				return;
			}
			if(IsUseKeyFrame)
			{
				if(EditorUtility.DisplayDialog("Caution", "The Pose hasn't saved yet. Do you want to save?", "OK", "No"))
				{
					AnimationSave(f0, f0, true);
				}
			}
			else
			{
				AnimationSave(f0, f1, isOverride);
			}
			IsBoneMove = false;
		}

		private static void AnimationSave(float f0, float f1, bool isOverride)
		{
			if(IsPreviewAnimation)
			{
				AnimationStop();
			}

			if(currentObject == null)
			{
				return;
			}
			var roott = currentObject.transform.root;
			var aclips = AnimationUtility.GetAnimationClips(roott.gameObject);
			if(aclips.Length < 1)
			{
				return;
			}
			var aclip = aclips[oldaclipidx];
			var bindings = AnimationUtility.GetCurveBindings(aclip);

			if(AssetDatabase.IsForeignAsset(aclip))
			{
				AnimationImport(aclip);
			}

			var anim = roott.gameObject.GetComponent<Animator>() == true ? roott.gameObject.GetComponent<Animator>() : null;
			if(anim == null || anim.avatar == null || !anim.avatar.isHuman)
			{
				for(var i = 0; i < bindings.Length; i++)
				{
					var binding = bindings[i];
					if(binding.type != typeof(Transform))
					{
						continue;
					}
					var trans = (Transform)AnimationUtility.GetAnimatedObject(roott.gameObject, binding);

					//if (!BoneMoveHash.Contains(trans)) continue;
					var val0 = 0.0f;
					if(trans == null)
					{
						continue;
					}
					if(binding.propertyName == "m_LocalPosition.x")
					{
						val0 = trans.localPosition.x;
					}
					else if(binding.propertyName == "m_LocalPosition.y")
					{
						val0 = trans.localPosition.y;
					}
					else if(binding.propertyName == "m_LocalPosition.z")
					{
						val0 = trans.localPosition.z;
					}
					else if(binding.propertyName == "m_LocalRotation.x")
					{
						val0 = trans.localRotation.x;
					}
					else if(binding.propertyName == "m_LocalRotation.y")
					{
						val0 = trans.localRotation.y;
					}
					else if(binding.propertyName == "m_LocalRotation.z")
					{
						val0 = trans.localRotation.z;
					}
					else if(binding.propertyName == "m_LocalRotation.w")
					{
						val0 = trans.localRotation.w;
					}
					else if(binding.propertyName == "m_LocalScale.x")
					{
						val0 = trans.localScale.x;
					}
					else if(binding.propertyName == "m_LocalScale.y")
					{
						val0 = trans.localScale.y;
					}
					else if(binding.propertyName == "m_LocalScale.z")
					{
						val0 = trans.localScale.z;
					}
					var curve = AnimationUtility.GetEditorCurve(aclip, binding);
					if(isOverride)
					{
						AnimationCurveOverride(curve, f0, f1, val0, aclip.length);
					}
					else
					{
						AnimationCurveMix(curve, f0, val0, aclip.length);
					}
					AnimationUtility.SetEditorCurve(aclip, binding, curve);
				}
			}
			else
			{
				var posehand = new HumanPoseHandler(anim.avatar, roott);
				var musclenames = HumanTrait.MuscleName;
				var humanPose = new HumanPose();
				posehand.GetHumanPose(ref humanPose);
				var musclefs = humanPose.muscles;
				for(var i = 0; i < bindings.Length; i++)
				{
					var binding = bindings[i];
					if(binding.type == typeof(Animator))
					{
						var bindstr = binding.propertyName;
						var curve = AnimationUtility.GetEditorCurve(aclip, binding);
						var ismuscle = false;
						var val0 = -1.0f;
						for(var j = 0; j < musclenames.Length; j++)
						{
							if(bindstr != musclenames[j])
							{
								continue;
							}
							val0 = musclefs[j];
							ismuscle = true;
							break;
						}
						if(!ismuscle)
						{
							var bindq = Quaternion.identity;
							var bindt = Vector3.zero;
							/*if (bindstr.StartsWith("Motion"))
							{
							    bindq = anim.rootRotation;
							    bindt = anim.rootPosition;
							}
							else if (bindstr.StartsWith("Root"))
							{
							    bindq = currentObject.transform.root.localRotation;
							    bindt = currentObject.transform.root.localPosition;
							}
							else if (bindstr.StartsWith("LeftFoot"))*/
							if(bindstr.StartsWith("LeftFoot"))
							{
								var tra = anim.GetBoneTransform(HumanBodyBones.LeftFoot);

								//Transform prtra = tra.parent;
								if(tra != null)
								{
									var lfootpos = tra.position;
									var lfootq = tra.rotation;
									var GetPostRotate = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
									if(GetPostRotate == null)
									{
										continue;
									}
									var GetAxisLength = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
									if(GetAxisLength == null)
									{
										continue;
									}
									var postq = (Quaternion)GetPostRotate.Invoke(anim.avatar, new object[] { HumanBodyBones.LeftFoot });
									lfootq *= postq;
									var axislen = (float)GetAxisLength.Invoke(anim.avatar, new object[] { HumanBodyBones.LeftFoot });
									lfootpos += lfootq * new Vector3(axislen, 0, 0);
									bindq = lfootq;

									//bindt = skinmatrix.MultiplyPoint(( lfootpos - currentSkinned.bounds.center)/anim.humanScale);
									bindt = (lfootpos - currentSkinned.bounds.center) / anim.humanScale;
								}
							}
							else if(bindstr.StartsWith("RightFoot"))
							{
								var tra = anim.GetBoneTransform(HumanBodyBones.RightFoot);
								if(tra != null)
								{
									var rfootpos = tra.position;
									var rfootq = tra.rotation;
									var GetPostRotate = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
									if(GetPostRotate == null)
									{
										continue;
									}
									var GetAxisLength = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
									if(GetAxisLength == null)
									{
										continue;
									}
									var postq = (Quaternion)GetPostRotate.Invoke(anim.avatar, new object[] { HumanBodyBones.RightFoot });
									rfootq *= postq;
									var axislen = (float)GetAxisLength.Invoke(anim.avatar, new object[] { HumanBodyBones.RightFoot });
									rfootpos += rfootq * new Vector3(axislen, 0, 0);
									bindq = rfootq;
									bindt = (rfootpos - currentSkinned.bounds.center) / anim.humanScale;
								}
							}
							else if(bindstr.StartsWith("LeftHand"))
							{
								var tra = anim.GetBoneTransform(HumanBodyBones.LeftHand);
								if(tra != null)
								{
									var lfootpos = tra.position;
									var lfootq = tra.rotation;
									var GetPostRotate = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
									if(GetPostRotate == null)
									{
										continue;
									}
									var GetAxisLength = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
									if(GetAxisLength == null)
									{
										continue;
									}
									var postq = (Quaternion)GetPostRotate.Invoke(anim.avatar, new object[] { HumanBodyBones.LeftHand });
									lfootq *= postq;
									var axislen = (float)GetAxisLength.Invoke(anim.avatar, new object[] { HumanBodyBones.LeftHand });
									lfootpos += lfootq * new Vector3(axislen, 0, 0);
									bindq = lfootq;
									bindt = (lfootpos - currentSkinned.bounds.center) / anim.humanScale;
								}
							}
							else if(bindstr.StartsWith("RightHand"))
							{
								var tra = anim.GetBoneTransform(HumanBodyBones.RightHand);
								if(tra != null)
								{
									var rfootpos = tra.position;
									var rfootq = tra.rotation;
									var GetPostRotate = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
									if(GetPostRotate == null)
									{
										continue;
									}
									var GetAxisLength = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
									if(GetAxisLength == null)
									{
										continue;
									}
									var postq = (Quaternion)GetPostRotate.Invoke(anim.avatar, new object[] { HumanBodyBones.RightHand });
									rfootq *= postq;
									var axislen = (float)GetAxisLength.Invoke(anim.avatar, new object[] { HumanBodyBones.RightHand });
									rfootpos += rfootq * new Vector3(axislen, 0, 0);
									bindq = rfootq;
									bindt = (rfootpos - currentSkinned.bounds.center) / anim.humanScale;
								}
							}
							else
							{
								continue;
							}
							if(bindstr.EndsWith("Q.x"))
							{
								val0 = bindq.x;
							}
							else if(bindstr.EndsWith("Q.y"))
							{
								val0 = bindq.y;
							}
							else if(bindstr.EndsWith("Q.z"))
							{
								val0 = bindq.z;
							}
							else if(bindstr.EndsWith("Q.w"))
							{
								val0 = bindq.w;
							}
							else if(bindstr.EndsWith("T.x"))
							{
								val0 = bindt.x;
							}
							else if(bindstr.EndsWith("T.y"))
							{
								val0 = bindt.y;
							}
							else if(bindstr.EndsWith("T.z"))
							{
								val0 = bindt.z;
							}
							else if(bindstr.EndsWith("TDOF.x"))
							{
								val0 = 0.0f;
							}
							else if(bindstr.EndsWith("TODF.y"))
							{
								val0 = 0.0f;
							}
							else if(bindstr.EndsWith("TDOF.z"))
							{
								val0 = 0.0f;
							}
						}
						if(isOverride)
						{
							AnimationCurveOverride(curve, f0, f1, val0, aclip.length);
						}
						else
						{
							AnimationCurveMix(curve, f0, val0, aclip.length);
						}
						AnimationUtility.SetEditorCurve(aclip, binding, curve);
					}
					else if(binding.type == typeof(Transform))
					{
						var trans = (Transform)AnimationUtility.GetAnimatedObject(roott.gameObject, binding);

						//if (!BoneMoveHash.Contains(trans)) continue;
						var val0 = 0.0f;
						if(binding.propertyName == "m_LocalPosition.x")
						{
							val0 = trans.localPosition.x;
						}
						else if(binding.propertyName == "m_LocalPosition.y")
						{
							val0 = trans.localPosition.y;
						}
						else if(binding.propertyName == "m_LocalPosition.z")
						{
							val0 = trans.localPosition.z;
						}
						else if(binding.propertyName == "m_LocalRotation.x")
						{
							val0 = trans.localRotation.x;
						}
						else if(binding.propertyName == "m_LocalRotation.y")
						{
							val0 = trans.localRotation.y;
						}
						else if(binding.propertyName == "m_LocalRotation.z")
						{
							val0 = trans.localRotation.z;
						}
						else if(binding.propertyName == "m_LocalRotation.w")
						{
							val0 = trans.localRotation.w;
						}
						else if(binding.propertyName == "m_LocalScale.x")
						{
							val0 = trans.localScale.x;
						}
						else if(binding.propertyName == "m_LocalScale.y")
						{
							val0 = trans.localScale.y;
						}
						else if(binding.propertyName == "m_LocalScale.z")
						{
							val0 = trans.localScale.z;
						}
						var curve = AnimationUtility.GetEditorCurve(aclip, binding);
						if(isOverride)
						{
							AnimationCurveOverride(curve, f0, f1, val0, aclip.length);
						}
						else
						{
							AnimationCurveMix(curve, f0, val0, aclip.length);
						}
						AnimationUtility.SetEditorCurve(aclip, binding, curve);
					}
				}
			}

			//oldaclipidx = aclipidx;
		}

		private static void AnimationPoseCopy()
		{
			var bones = GetAnimatorBones();
			AnimPoseRotateList = new List<Quaternion>();
			AnimPoseTraList = new List<Vector3>();
			for(var i = 0; i < bones.Length; i++)
			{
				AnimPoseRotateList.Add(bones[i].transform.rotation);
				AnimPoseTraList.Add(bones[i].transform.position);
			}
		}

		private static void AnimationPosePaste()
		{
			var bones = GetAnimatorBones();
			if(AnimPoseRotateList.Count < bones.Length || AnimPoseTraList.Count < bones.Length)
			{
				return;
			}
			for(var i = 0; i < bones.Length; i++)
			{
				bones[i].transform.rotation = AnimPoseRotateList[i];
				bones[i].transform.position = AnimPoseTraList[i];
			}
			IsAnimationPaste = true;
		}

		private static Transform GetRootTransform()
		{
			if(currentSkinned != null)
			{
				if(currentSkinned.rootBone != null)
				{
					return currentSkinned.rootBone;
				}
			}
			var anim = currentObject.transform.root.gameObject.GetComponent<Animator>() == true
				? currentObject.transform.root.gameObject.GetComponent<Animator>()
				: null;
			if(anim != null)
			{
				Transform hiptra = null;
				try
				{
					hiptra = anim.GetBoneTransform(HumanBodyBones.Hips);
				}
				catch
				{
				}
				if(hiptra != null)
				{
					return hiptra;
				}
			}
			return currentObject.transform.root;
		}

		private static Ikinfo GetIK(Animator anim, HumanBodyBones humanBody)
		{
			var ik = new Ikinfo();
			if(anim == null)
			{
				return ik;
			}
			var tra = anim.GetBoneTransform(humanBody);
			var lfootpos = tra.position;
			var lfootq = tra.rotation;
			var GetPostRotate = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
			if(GetPostRotate == null)
			{
				return ik;
			}
			var GetAxisLength = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
			if(GetAxisLength == null)
			{
				return ik;
			}
			var postq = (Quaternion)GetPostRotate.Invoke(anim.avatar, new object[] { humanBody });
			lfootq *= postq;
			var axislen = (float)GetAxisLength.Invoke(anim.avatar, new object[] { humanBody });
			lfootpos += lfootq * new Vector3(axislen, 0, 0);
			ik.rotate = lfootq;
			ik.position = (lfootpos - currentSkinned.bounds.center) / anim.humanScale;
			return ik;
		}

		private static void AnimationBlendShapeSave(float f0)
		{
			if(blendBaseMesh == null)
			{
				return;
			}

			//BlendShapeCreate();
			BlendShapeCreateAnime();
			var blendname = "";

			//try
			//{
			//    blendname = currentMesh.GetBlendShapeName(currentMesh.blendShapeCount - 1);
			//    Debug.Log("blendName: " + blendname);
			//}
			//catch { blendname = ""; }

			//GetCurrentMesh(false);
			//GetCurrentMesh2(false, true);
			GetCurrentMesh(false);
			if(currentMesh == null)
			{
				Debug.Log("hogehoge");
			}
			for(var i = 0; i < currentMesh.blendShapeCount; i++)
			{
				var tempstr = currentMesh.GetBlendShapeName(i);
				if(tempstr != "")
				{
					blendname = tempstr;
				}
			}

			//EditorCurveBinding[] bindings = AnimationUtility.GetAnimatableBindings(currentObject, currentObject.transform.root.gameObject);
			//for (int i = 0; i < bindings.Length; i++)
			//{
			//    if (bindings[i].propertyName.Contains(blendname))
			//    {
			//        Debug.Log(bindings[i].propertyName);
			//        break;
			//    }
			//}
			var roott = currentObject.transform.root;
			var aclips = AnimationUtility.GetAnimationClips(roott.gameObject);
			var aclip = aclips[oldaclipidx];

			//AnimationClip newclip = new AnimationClip();
			//AssetDatabase.CreateAsset(newclip, AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".anim"));
			//EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(aclip);
			var bindings = AnimationUtility.GetAnimatableBindings(currentObject, currentObject.transform.root.gameObject);

			//GameObjectRecorder recobj = new GameObjectRecorder(roott.gameObject);
			for(var i = 0; i < bindings.Length; i++)
			{
				//recobj.Bind(bindings[i]);
				if(bindings[i].propertyName.Contains(blendname))
				{
					Debug.Log(bindings[i].propertyName);

					var binding = bindings[i];

					//AnimationCurve curve = AnimationUtility.GetEditorCurve(aclip, binding);
					//Debug.Log(curve.ToString());
					//AnimationCurveOverride2(curve, f0, 1.0f, aclip.length);
					//AnimationUtility.SetEditorCurve(aclip, binding, curve);

					var curve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 100.0f));

					//AnimationUtility.SetEditorCurve(newclip, binding, curve);
					AnimationUtility.SetEditorCurve(aclip, binding, curve);

					//recobj.Bind(binding);
					//recobj.TakeSnapshot(1.0f);
					//recobj.SaveToClip(aclip);
					//recobj.SaveToClip(aclip);
					//break;
				}
				AnimationUtility.SetEditorCurve(aclip, bindings[i], AnimationUtility.GetEditorCurve(aclip, bindings[i]));
			}

			//recobj.TakeSnapshot(1.0f);
			//recobj.SaveToClip(aclip);
		}

		private static void AnimationBlendShapeSave2(float f0)
		{
			if(blendBaseMesh == null)
			{
				return;
			}
			BlendShapeCreateAnime();
			var blendname = "";

			//GetCurrentMesh(false);
			//GetCurrentMesh2(false, true);
			GetCurrentMesh(false);
			for(var i = 0; i < currentMesh.blendShapeCount; i++)
			{
				var tempstr = currentMesh.GetBlendShapeName(i);
				if(tempstr != "")
				{
					blendname = tempstr;
				}
			}
			var roott = currentObject.transform.root;
			var aclips = AnimationUtility.GetAnimationClips(roott.gameObject);
			var aclip = aclips[oldaclipidx];
			var newclip = new AnimationClip();

			//AssetDatabase.CreateAsset(newclip, AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".anim"));
			AssetDatabase.CreateAsset(newclip, AssetDatabase.GenerateUniqueAssetPath(SaveFolderPath + currentObject.name + ".anim"));
			var bindings = AnimationUtility.GetCurveBindings(aclip);
			if(AssetDatabase.IsForeignAsset(aclip))
			{
				AnimationImport(aclip);
			}
			for(var i = 0; i < bindings.Length; i++)
			{
				//recobj.Bind(bindings[i]);
				if(bindings[i].propertyName.Contains(blendname))
				{
					var binding = bindings[i];
					var curve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 100.0f));
					AnimationUtility.SetEditorCurve(aclip, binding, curve);
				}
			}
		}

		private static void AnimationExtend(float f0)
		{
			var roott = currentObject.transform.root;
			var aclips = AnimationUtility.GetAnimationClips(roott.gameObject);
			var aclip = aclips[aclipidx];
			var bindings = AnimationUtility.GetCurveBindings(aclip);
			if(AssetDatabase.IsForeignAsset(aclip))
			{
				AnimationImport(aclip);
			}
			for(var i = 0; i < bindings.Length; i++)
			{
				var binding = bindings[i];
				var curve = AnimationUtility.GetEditorCurve(aclip, binding);
				curve.AddKey(new Keyframe(f0, curve.Evaluate(f0)));
				AnimationUtility.SetEditorCurve(aclip, binding, curve);
			}
		}

		private static void AnimationShorten(float f0)
		{
			var roott = currentObject.transform.root;
			var aclips = AnimationUtility.GetAnimationClips(roott.gameObject);
			var aclip = aclips[aclipidx];
			var bindings = AnimationUtility.GetCurveBindings(aclip);
			if(AssetDatabase.IsForeignAsset(aclip))
			{
				AnimationImport(aclip);
			}
			for(var i = 0; i < bindings.Length; i++)
			{
				var binding = bindings[i];
				var curve = AnimationUtility.GetEditorCurve(aclip, binding);
				if(f0 < aclip.length)
				{
					var keys = curve.keys;
					for(var j = 0; j < keys.Length; j++)
					{
						if(keys[j].time > f0)
						{
							curve.MoveKey(j, new Keyframe(f0, curve.Evaluate(f0)));
						}
					}
				}
				AnimationUtility.SetEditorCurve(aclip, binding, curve);
			}
		}

		private static void AnimationImport(AnimationClip aclip)
		{
			if(IsAnimationExist(aclip))
			{
				EditorUtility.DisplayDialog("Caution", "You have already imported this animation.", "OK");
				return;
			}
			if(currentObject == null)
			{
				return;
			}
			var roott = currentObject.transform.root;
			var bindings = AnimationUtility.GetCurveBindings(aclip);
			var anim = roott.gameObject.GetComponent<Animator>() == true ? roott.gameObject.GetComponent<Animator>() : null;
			var newclip = new AnimationClip { name = aclip.name };
			var ishuman = false;
			try
			{
				ishuman = anim.avatar.isHuman;
			}
			catch
			{
			}
			for(var i = 0; i < bindings.Length; i++)
			{
				var binding = bindings[i];
				if(ishuman && binding.type == typeof(Transform))
				{
					continue;
				}
				var curve = AnimationUtility.GetEditorCurve(aclip, binding);
				AnimationUtility.SetEditorCurve(newclip, binding, curve);
			}
			var bones = currentSkinned.bones;
			if(ishuman)
			{
				AnimationClipBind(newclip, anim, bones);
			}

			//AssetDatabase.CreateAsset(newclip, AssetDatabase.GenerateUniqueAssetPath
			//    ("Assets/" + roott.gameObject.name + "_" + aclip.name + ".anim"));
			AssetDatabase.CreateAsset(newclip, AssetDatabase.GenerateUniqueAssetPath
				(SaveFolderPath + roott.gameObject.name + "_" + aclip.name + ".anim"));
			var islabel = false;
			if(anim.runtimeAnimatorController == null)
			{
				islabel = false;
			}
			else
			{
				var lables = AssetDatabase.GetLabels(anim.runtimeAnimatorController);
				for(var i = 0; i < lables.Length; i++)
				{
					if(lables[i] == "EditorSculpt")
					{
						islabel = true;
						break;
					}
				}
			}
			if(!islabel)
			{
				//String ContrPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + ".controller");
				var ContrPath = AssetDatabase.GenerateUniqueAssetPath(SaveFolderPath + currentObject.name + ".controller");
				AnimatorController.CreateAnimatorControllerAtPathWithClip(ContrPath, newclip);
				anim.runtimeAnimatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath(ContrPath, typeof(AnimatorController));
				AssetDatabase.SetLabels(anim.runtimeAnimatorController, new[] { "EditorSculpt" });
			}
			else
			{
				var acliplist = anim.runtimeAnimatorController.animationClips.ToList();
				acliplist.Add(newclip);
				var animatorCon = (AnimatorController)anim.runtimeAnimatorController;
				animatorCon.AddMotion(newclip);
			}
			EditorSculptPrefab(false, true);
		}

		private static bool IsAnimationExist(AnimationClip chkclip)
		{
			AnimationClip[] aclips;
			try
			{
				aclips = AnimationUtility.GetAnimationClips(currentObject.transform.root.gameObject);
			}
			catch
			{
				return false;
			}
			foreach(var aclip in aclips)
			{
				if(aclip == chkclip)
				{
					return true;
				}
			}
			return false;
		}

		private static void AnimationClipBind(AnimationClip newclip, Animator anim, Transform[] bones)
		{
			//HumanDescription humand = anim.avatar.humanDescription;
			//HumanBone[] hubones = humand.human;
			//bool ishuman = false;
			//try
			//{
			//    ishuman = anim.avatar.isHuman;
			//}
			//catch { }

			var humand = new HumanDescription();
			HumanBone[] hubones = null;
			var ishuman = false;
			try
			{
				humand = anim.avatar.humanDescription;
				ishuman = anim.avatar.isHuman;
				hubones = humand.human;
			}
			catch
			{
			}

			for(var i = 0; i < bones.Length; i++)
			{
				var flag0 = false;
				for(var j = 0; j < hubones.Length; j++)
				{
					if(bones[i].name == hubones[j].boneName)
					{
						flag0 = true;
						break;
					}
				}
				if(flag0 && ishuman)
				{
					continue;
				}
				var binding0 = EditorCurveBinding.FloatCurve(
					AnimationUtility.CalculateTransformPath(bones[i], bones[i].root), typeof(Transform), "m_LocalPosition.x");
				var curve = new AnimationCurve(new Keyframe(0.0f, bones[i].transform.localPosition.x));
				curve.AddKey(new Keyframe(1.0f, bones[i].transform.localPosition.x));
				AnimationUtility.SetEditorCurve(newclip, binding0, curve);
				binding0 = EditorCurveBinding.FloatCurve(
					AnimationUtility.CalculateTransformPath(bones[i], bones[i].root), typeof(Transform), "m_LocalPosition.y");
				curve = new AnimationCurve(new Keyframe(0.0f, bones[i].transform.localPosition.y));
				curve.AddKey(new Keyframe(1.0f, bones[i].transform.localPosition.y));
				AnimationUtility.SetEditorCurve(newclip, binding0, curve);
				binding0 = EditorCurveBinding.FloatCurve(
					AnimationUtility.CalculateTransformPath(bones[i], bones[i].root), typeof(Transform), "m_LocalPosition.z");
				curve = new AnimationCurve(new Keyframe(0.0f, bones[i].transform.localPosition.z));
				curve.AddKey(new Keyframe(1.0f, bones[i].transform.localPosition.z));
				AnimationUtility.SetEditorCurve(newclip, binding0, curve);
				binding0 = EditorCurveBinding.FloatCurve(
					AnimationUtility.CalculateTransformPath(bones[i], bones[i].root), typeof(Transform), "m_LocalRotation.x");
				curve = new AnimationCurve(new Keyframe(0.0f, bones[i].transform.localRotation.x));
				curve.AddKey(new Keyframe(1.0f, bones[i].transform.localRotation.x));
				AnimationUtility.SetEditorCurve(newclip, binding0, curve);
				binding0 = EditorCurveBinding.FloatCurve(
					AnimationUtility.CalculateTransformPath(bones[i], bones[i].root), typeof(Transform), "m_LocalRotation.y");
				curve = new AnimationCurve(new Keyframe(0.0f, bones[i].transform.localRotation.y));
				curve.AddKey(new Keyframe(1.0f, bones[i].transform.localRotation.y));
				AnimationUtility.SetEditorCurve(newclip, binding0, curve);
				binding0 = EditorCurveBinding.FloatCurve(
					AnimationUtility.CalculateTransformPath(bones[i], bones[i].root), typeof(Transform), "m_LocalRotation.z");
				curve = new AnimationCurve(new Keyframe(0.0f, bones[i].transform.localRotation.z));
				curve.AddKey(new Keyframe(1.0f, bones[i].transform.localRotation.z));
				AnimationUtility.SetEditorCurve(newclip, binding0, curve);
				binding0 = EditorCurveBinding.FloatCurve(
					AnimationUtility.CalculateTransformPath(bones[i], bones[i].root), typeof(Transform), "m_LocalRotation.w");
				curve = new AnimationCurve(new Keyframe(0.0f, bones[i].transform.localRotation.w));
				curve.AddKey(new Keyframe(1.0f, bones[i].transform.localRotation.w));
				AnimationUtility.SetEditorCurve(newclip, binding0, curve);
				binding0 = EditorCurveBinding.FloatCurve(
					AnimationUtility.CalculateTransformPath(bones[i], bones[i].root), typeof(Transform), "m_LocalScale.x");
				curve = new AnimationCurve(new Keyframe(0.0f, bones[i].transform.localScale.x));
				curve.AddKey(new Keyframe(1.0f, bones[i].transform.localScale.x));
				AnimationUtility.SetEditorCurve(newclip, binding0, curve);
				binding0 = EditorCurveBinding.FloatCurve(
					AnimationUtility.CalculateTransformPath(bones[i], bones[i].root), typeof(Transform), "m_LocalScale.y");
				curve = new AnimationCurve(new Keyframe(0.0f, bones[i].transform.localScale.y));
				curve.AddKey(new Keyframe(1.0f, bones[i].transform.localScale.y));
				AnimationUtility.SetEditorCurve(newclip, binding0, curve);
				binding0 = EditorCurveBinding.FloatCurve(
					AnimationUtility.CalculateTransformPath(bones[i], bones[i].root), typeof(Transform), "m_LocalScale.z");
				curve = new AnimationCurve(new Keyframe(0.0f, bones[i].transform.localScale.z));
				curve.AddKey(new Keyframe(1.0f, bones[i].transform.localScale.z));
				AnimationUtility.SetEditorCurve(newclip, binding0, curve);
			}
		}

		private static void AnimationCurveMix(AnimationCurve curve, float f0, float val0, float maxtime)
		{
			var keys = curve.keys;
			if(f0 == 0.0f)
			{
				keys[0] = new Keyframe(f0, val0);
				curve.keys = keys;
				return;
			}
			if(f0 == maxtime)
			{
				keys[keys.Length - 1] = new Keyframe(f0, val0);
				curve.keys = keys;
				return;
			}
			var keyminf = -1.0f;
			var minint = -1;
			for(var i = 0; i < keys.Length; i++)
			{
				var keyf0 = Mathf.Abs(keys[i].time - f0);
				if(keyf0 < keyminf || keyminf < 0)
				{
					keyminf = keyf0;
					minint = i;
				}
			}
			if(keyminf < 0.1f)
			{
				var fp0 = Mathf.Clamp(keys[minint].value, val0, Mathf.Clamp01(1.0f - keyminf));

				//if (f0 == 0.0f) fp0 = val0;
				curve.RemoveKey(minint);
				curve.AddKey(new Keyframe(f0, fp0));
			}
			else
			{
				curve.AddKey(new Keyframe(f0, val0));
			}
		}

		private static void AnimationCurveOverride(AnimationCurve curve, float f0, float f1, float val0, float maxtime)
		{
			var keys = curve.keys;
			var addedkey = 0;
			for(var i = 0; i < keys.Length; i++)
			{
				var time = keys[i].time;
				if(time < f0 || time > f1)
				{
					continue;
				}
				curve.RemoveKey(i);
				curve.AddKey(new Keyframe(time, val0));
				addedkey++;
			}

			// if (f0 <= 0) f0 = 0.01f;
			//if (f1 >= maxtime) f1 = maxtime - 0.01f;
			curve.AddKey(new Keyframe(f0, val0));
			curve.AddKey(new Keyframe(f1, val0));

			//if(addedkey==0)
			//{
			//    curve.AddKey(new Keyframe(f0, val0));
			//    curve.AddKey(new Keyframe(f1, val0));
			//}
		}

		private static void AnimationClipRemoveKey(AnimationClip aclip, float keytime)
		{
			var bindings = AnimationUtility.GetCurveBindings(aclip);
			for(var i = 0; i < bindings.Length; i++)
			{
				var curve = AnimationUtility.GetEditorCurve(aclip, bindings[i]);
				var keys = curve.keys;
				for(var j = 0; j < keys.Length; j++)
				{
					var time = keys[j].time;
					if(time == keytime)
					{
						curve.RemoveKey(j);
					}
				}
				AnimationUtility.SetEditorCurve(aclip, bindings[i], curve);
			}
		}

		private static void AnimationPoseLoad(float f0)
		{
			var aclips = AnimationUtility.GetAnimationClips(currentObject.transform.root.gameObject);
			if(aclips.Length < 1 || aclipidx >= aclips.Length)
			{
				return;
			}
			var aclip = aclips[aclipidx];

			//bool flag0 = false;
			if(!AnimationMode.InAnimationMode())
			{
				AnimationMode.StartAnimationMode();

				//flag0 = true;
			}
			AnimationMode.BeginSampling();
			AnimationMode.SampleAnimationClip(currentObject.transform.root.gameObject, aclip, f0);
			AnimationMode.EndSampling();

			var anim = currentObject.transform.root.gameObject.GetComponent<Animator>()
			           == true
				? currentObject.transform.root.gameObject.GetComponent<Animator>()
				: null;
			if(anim != null)
			{
				if(anim.isHuman)
				{
					var iki = GetRootFromAnimationClip(aclip);
					var posehand = new HumanPoseHandler(anim.avatar, currentObject.transform.root);

					//Transform rootTra = currentObject.transform.root;
					//if(currentSkinned!=null)
					//{
					//    if(currentSkinned.rootBone!=null)
					//    {
					//        rootTra = currentSkinned.rootBone;
					//    }
					//}
					//HumanPoseHandler posehand = new HumanPoseHandler(anim.avatar, rootTra);
					var humanpose = new HumanPose();
					posehand.GetHumanPose(ref humanpose);
					humanpose.bodyPosition = iki.position;
					humanpose.bodyRotation = iki.rotate;
					posehand.SetHumanPose(ref humanpose);
				}
			}

			//if (flag0) SceneView.lastActiveSceneView.FrameSelected();
			SceneView.RepaintAll();
			ChangeMaterial();
		}

		private static void AnimationStop()
		{
			AnimationMode.StopAnimationMode();

			//SceneView.lastActiveSceneView.FrameSelected();
			AnimePTime = 0.0f;
			IsPreviewAnimation = false;
			NeedFrameSelected = true;

			//if (IsOldAnimationImport == true) ModelImporterChangeAnimeImport(false);
		}

		private static void AnimationQuit()
		{
			if(AnimationMode.InAnimationMode())
			{
				//if (IsBoneMove) AnimationSave(animeslider, 0.0f, false);
				AnimationSaveBoneMove(animeslider, 0.0f, false);
				AnimationMode.StopAnimationMode();
				animeslider = 0.0f;
				oldanimsli = 0.0f;
			}
			if(IsPreviewAnimation)
			{
				AnimationStop();
			}

			//Added 2025/03/20
			IsBoneMove = false;

			//BoneMoveHash.Clear();
			//End Added 2025/03/20

			////Added 2025/09/12 to backup bindpose to fix bug that animtion not work.
			//if ((currentSkinned != null) && (currentMesh != null))
			//{
			//    //oldbind = currentMesh.bindposes;
			//    ResetBindPose2();
			//}
			////End added 2025/09/12
		}

		private static void AnimationPoseReset()
		{
			var aclips = AnimationUtility.GetAnimationClips(currentObject.transform.root.gameObject);
			var aclip = aclips[aclipidx];
			AnimationMode.StartAnimationMode();
			AnimationMode.BeginSampling();
			AnimationMode.SampleAnimationClip(currentObject.transform.root.gameObject, aclip, 0.0f);
			AnimationMode.EndSampling();
			SceneView.RepaintAll();
			var boint = currentSkinned.bones.Length;
			var animevecs = Enumerable.Repeat(Vector3.zero, boint).ToArray();
			var animerots = Enumerable.Repeat(Quaternion.identity, boint).ToArray();
			for(var i = 0; i < boint; i++)
			{
				animerots[i] = currentSkinned.bones[i].transform.rotation;
				animevecs[i] = currentSkinned.bones[i].transform.position;
			}
			AnimationMode.StopAnimationMode();
			for(var i = 0; i < boint; i++)
			{
				currentSkinned.bones[i].transform.rotation = animerots[i];
				currentSkinned.bones[i].transform.position = animevecs[i];
			}
			currentSkinned.BakeMesh(currentMesh);
			EditorSculptPrefab(false, true);
			ChangeMaterial();
		}

		private static void AnimeSliderReset()
		{
			if(aclipidx != oldaclipidx || BrushString != BrushStringOld)
			{
				//if (IsBoneMove) AnimationSave(oldanimsli, 0.0f, false);
				AnimationSaveBoneMove(oldanimsli, 0.0f, false);
				if(IsPreviewAnimation)
				{
					AnimationStop();
				}

#if MemoryAnimation
				AnimationClip[] aclips;
				AnimationClip aclip;
				if((aclipidx != oldaclipidx) && (!IsAnimationSaved) && (!IsSkipDialog))
				{
					if(EditorUtility.DisplayDialog("Caution", "The Animation Clip hasn't saved yet. Do you want to save?", "OK", "Cancel"))
					{
					}
					else
					{
						aclips = AnimationUtility.GetAnimationClips(currentObject.transform.root.gameObject);
						if(aclips.Length > 0 && aclipidx < aclips.Length)
						{
							aclip = aclips[oldaclipidx];
							AnimationUtility.SetEditorCurves(aclip, memoryBindings, memoryCurves);

							//foreach (AnimationCurve memoryCurve in memoryCurves)
							//{
							//    foreach (EditorCurveBinding memoryBinding in memoryBindings)
							//    {
							//        AnimationUtility.SetEditorCurve(aclip, memoryBinding, memoryCurve);
							//    }
							//}
						}
						AnimationPoseLoad(animeslider);
					}
				}
#endif

				oldaclipidx = aclipidx;
				animeslider = 0.0f;
				oldanimsli = 0.0f;

				currentObject.transform.root.gameObject.GetComponent<Animator>().applyRootMotion = false;

				//NeedFrameSelected = true;
				AnimationPoseLoad(animeslider);
				SceneView.lastActiveSceneView.FrameSelected();
				SceneView.RepaintAll();
				ChangeMaterial();

#if MemoryAnimation
				aclips = AnimationUtility.GetAnimationClips(currentObject.transform.root.gameObject);
				if(aclips.Length < 1) return;
				aclip = aclips[aclipidx];
				memoryBindings = AnimationUtility.GetCurveBindings(aclip);
				List<AnimationCurve> memcurveList = new List<AnimationCurve>();
				foreach(EditorCurveBinding bind in memoryBindings)
				{
					AnimationCurve curve = AnimationUtility.GetEditorCurve(aclip, bind);
					memcurveList.Add(curve);
				}
				memoryCurves = memcurveList.ToArray();
#endif
			}
		}

		private static void BoneDelete()
		{
			var bones = currentSkinned.bones;
			var bonelist = new List<Transform>();
			var binds = currentMesh.bindposes;
			var bindlist = new List<Matrix4x4>();
			var IsNotDelete = false;
			var anim = currentObject.transform.root.gameObject.GetComponent<Animator>() == true
				? currentObject.transform.root.gameObject.GetComponent<Animator>()
				: null;
			HumanBone[] hubones = null;
			SkeletonBone[] skeletons = null;
			RuntimeAnimatorController oldRuncont = null;
			if(anim != null)
			{
				try
				{
					var humand = anim.avatar.humanDescription;
					hubones = humand.human;
					skeletons = humand.skeleton;
				}
				catch
				{
				}

				//New in 2020/11/19
				try
				{
					oldRuncont = anim.runtimeAnimatorController;
					anim.runtimeAnimatorController = null;
				}
				catch
				{
				}

				//End New in 2020/11/19
			}
			var IsAclip = false;
			var aclips = AnimationUtility.GetAnimationClips(currentObject.transform.root.gameObject);
			var curvelist = new List<EditorCurveBinding>();
			for(var i = 0; i < aclips.Length; i++)
			{
				curvelist.AddRange(AnimationUtility.GetCurveBindings(aclips[i]));
			}
			for(var i = 0; i < curvelist.Count; i++)
			{
				Transform trans = null;
				try
				{
					trans = (Transform)AnimationUtility.GetAnimatedObject(currentObject.transform.root.gameObject, curvelist[i]);
				}
				catch
				{
					continue;
				}
				if(trans != null)
				{
					try
					{
						//if (trans.name == bones[delboneidx].name)
						if(trans == bones[delboneidx])
						{
							IsAclip = true;
							break;
						}
					}
					catch
					{
					}
				}
			}
			for(var i = 0; i < bones.Length; i++)
			{
				if(i != delboneidx)
				{
					bonelist.Add(bones[i]);
					bindlist.Add(binds[i]);
				}
				else
				{
					var bonename = bones[i].name;
					if(hubones != null)
					{
						for(var j = 0; j < hubones.Length; j++)
						{
							if(bonename == hubones[j].boneName || bonename == hubones[j].humanName)
							{
								IsNotDelete = true;
								break;
							}
						}
					}
					if(skeletons != null)
					{
						for(var j = 0; j < skeletons.Length; j++)
						{
							if(bonename == skeletons[j].name)
							{
								EditorUtility.DisplayDialog("Caution", "You cann't delete the Avatar's skeleton bones!", "OK");
								anim.runtimeAnimatorController = oldRuncont;
								delboneidx = -1;
								return;
							}
						}
					}
				}
			}
			if(IsNotDelete)
			{
				if(EditorUtility.DisplayDialog("Caution", "This is a part of the human bones.I discourage you to delete this bone", "OK", "Delete"))
				{
					return;
				}
			}
			if(IsAclip)
			{
				if(EditorUtility.DisplayDialog("Caution", "This is a part of the existing animation clip.I discourage you to delete this bone",
					   "OK", "Delete"))
				{
					return;
				}
			}
			currentSkinned.bones = bonelist.ToArray();
			currentMesh.bindposes = bindlist.ToArray();
			var weight1s = currentMesh.GetAllBoneWeights().ToArray();
			var pervers = currentMesh.GetBonesPerVertex().ToArray();
			var bwlist0 = new List<BoneWeight1>();
			var w1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
			var vcnt = currentMesh.vertexCount;
			var newpervers = Enumerable.Repeat((byte)0, vcnt).ToArray();
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var pcnt = 0;
			var parentt = bones[delboneidx].parent;
			var parentidx2 = -1;
			for(var i = 0; i < bones.Length; i++)
			{
				if(bones[i] == parentt)
				{
					parentidx2 = i;
				}
			}

			var maxidx = -1;
			for(var i = 0; i < vcnt; i++)
			{
				int pv0 = pervers[i];
				if(pv0 > 0)
				{
					for(var j = 0; j < pv0; j++)
					{
						try
						{
							var bw1 = weight1s[boneidxs[i] + j];
							if(bw1.boneIndex > maxidx)
							{
								maxidx = bw1.boneIndex;
							}
						}
						catch
						{
						}
					}
				}
			}
			var parentpos = currentObject.transform.InverseTransformPoint(parentt.position);
			var vertices = currentMesh.vertices;
			var cntarr = Enumerable.Repeat(0, maxidx).ToArray();
			for(var i = 0; i < vcnt; i++)
			{
				var v0 = vertices[i];
				var d0 = Vector3.Distance(parentpos, v0);
				var flag0 = false;
				for(var j = 0; j < bones.Length; j++)
				{
					var d1 = Vector3.Distance(currentObject.transform.InverseTransformPoint(bones[j].position), v0);
					if(d1 < d0)
					{
						flag0 = true;
						break;
					}
				}
				if(flag0)
				{
					continue;
				}
				int pv0 = pervers[i];
				for(var j = 0; j < pv0; j++)
				{
					try
					{
						var bw1 = weight1s[boneidxs[i] + j];
						cntarr[bw1.boneIndex]++;
					}
					catch
					{
					}
				}
			}
			var addweights = Enumerable.Repeat(1.0f, weight1s.Length).ToArray();

			for(var i = 0; i < pervers.Length; i++)
			{
				boneidxs[i] = pcnt;
				pcnt += pervers[i];
			}
			if(boneidxs.Length < 1)
			{
				boneidxs = Enumerable.Repeat(0, vcnt).ToArray();
			}
			for(var i = 0; i < vcnt; i++)
			{
				int pv0 = pervers[i];
				var bwlist1 = new List<BoneWeight1>();
				if(pv0 > 0)
				{
					for(var j = 0; j < pv0; j++)
					{
						try
						{
							var bw1 = weight1s[boneidxs[i] + j];
							if(bw1.boneIndex == delboneidx)
							{
								var bw2 = weight1s[boneidxs[i] + j];
								addweights[boneidxs[i] + j] = bw2.weight;
							}
						}
						catch
						{
						}
					}
				}
			}

			var newbwlist2 = new List<BoneWeight1>();
			var newpervers2 = Enumerable.Repeat((byte)1, vcnt).ToArray();
			for(var i = 0; i < vcnt; i++)
			{
				int pv0 = pervers[i];
				var bwlist1 = new List<BoneWeight1>();
				if(pv0 > 0)
				{
					for(var j = 0; j < pv0; j++)
					{
						try
						{
							var bw1 = weight1s[boneidxs[i] + j];
							if(bw1.boneIndex < delboneidx)
							{
								bwlist1.Add(bw1);
							}
							else if(bw1.boneIndex != delboneidx)
							{
								if(bw1.boneIndex > 0)
								{
									bw1.boneIndex -= 1;
									bwlist1.Add(bw1);
								}
							}
							else
							{
								bw1.boneIndex = parentidx2;
								bwlist1.Add(bw1);
							}
						}
						catch
						{
							bwlist1.Add(w1);
						}
					}
					if(bwlist1.Count > 0)
					{
						newbwlist2.AddRange(bwlist1);
						newpervers2[i] = (byte)bwlist1.Count;
					}
					else
					{
						newbwlist2.Add(w1);
						newpervers2[i] = 1;
					}
				}
			}

			var ntweight1s = new NativeArray<BoneWeight1>(newbwlist2.ToArray(), Allocator.Temp);
			var PerVerts = new NativeArray<byte>(newpervers2, Allocator.Temp);
			currentMesh.SetBoneWeights(PerVerts, ntweight1s);

			GameObject delobj = null;
			try
			{
				delobj = bones[delboneidx].gameObject;
			}
			catch
			{
				return;
			}
			var delpar = delobj.transform.parent.gameObject;
			for(var i = 0; i < delobj.transform.childCount; i++)
			{
				var childobj = delobj.transform.GetChild(i).gameObject;
				var temppos = childobj.transform.position;
				var temprot = childobj.transform.rotation;
				var tempscale = childobj.transform.localScale;
				GameObjectUtility.SetParentAndAlign(childobj, delpar);
				childobj.transform.position = temppos;
				childobj.transform.rotation = temprot;
				childobj.transform.localScale = tempscale;
			}
			Selection.activeGameObject = delobj;
			DestroyImmediate(delobj);
			Selection.activeGameObject = currentObject;

			//New in 2020/11/19
			if(CheckAnimationBrush(BrushString))
			{
				//NeedFrameSelected = true;
				AnimatorSetup(false);
			}

			//End New in 2020/11/19

			EditorSculptPrefab(false, true);
		}

		private static Transform[] GetHumanBones()
		{
			var bonelist = new List<Transform>();
			Animator anim = null;
			try
			{
				anim = currentObject.transform.root.gameObject.GetComponent<Animator>();
			}
			catch
			{
				return bonelist.ToArray();
			}
			for(var i = 0; i < (int)HumanBodyBones.LastBone; i++)
			{
				bonelist.Add(anim.GetBoneTransform((HumanBodyBones)i));
			}
			return bonelist.ToArray();
		}

		private static Transform[] GetAnimatorBones()
		{
			try
			{
				if(currentSkinned.bones.Length > 0)
				{
					return currentSkinned.bones;
				}
			}
			catch
			{
			}
			var bonelist = new List<Transform>();
			var objs = EditorUtility.CollectDeepHierarchy(new[] { currentObject.transform.root.gameObject });
			foreach(var obj in objs)
			{
				if(obj.GetType() == typeof(GameObject))
				{
					bonelist.Add(((GameObject)obj).transform);
				}
			}
			return bonelist.ToArray();
		}

		private static Vector3[] GetExtraBoneVec(Transform[] bones)
		{
			var extraveclist = new List<Vector3>();
			for(var i = 0; i < bones.Length; i++)
			{
				if(bones[i].childCount > 0 || bones[i].parent == null)
				{
					extraveclist.Add(Vector3.zero);
				}
				else
				{
					var ch0 = bones[i].position;
					extraveclist.Add(ch0 + (ch0 - bones[i].parent.position));
				}
			}
			return extraveclist.ToArray();
		}

		private static bool AnimationValidateHuman(Transform trans)
		{
			var anim = trans.root.gameObject.GetComponent<Animator>();
			if(!anim)
			{
				return false;
			}

			//Error on Unity2022.1
			//for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
			//{
			//    //if (i == (int)HumanBodyBones.LeftIndexDistal || i==(int)HumanBodyBones.LeftIndexIntermediate || i==(int)HumanBodyBones.LeftIndexProximal
			//    //    || i==(int)HumanBodyBones.LeftLittleDistal || i==(int)HumanBodyBones.LeftLittleIntermediate || i==(int)HumanBodyBones.LeftLittleProximal
			//    //    || i==(int)HumanBodyBones.LeftMiddleDistal || i==(int)HumanBodyBones.LeftMiddleIntermediate || i==(int)HumanBodyBones.LeftMiddleProximal 
			//    //    || i==(int)HumanBodyBones.LeftRingDistal || i==(int)HumanBodyBones.LeftRingIntermediate ||i==(int)HumanBodyBones.LeftRingProximal 
			//    //    || i==(int)HumanBodyBones.LeftThumbDistal|| i==(int)HumanBodyBones.LeftThumbIntermediate || i==(int)HumanBodyBones.LeftThumbProximal 
			//    //    || i==(int)HumanBodyBones.RightIndexDistal || i==(int)HumanBodyBones.RightIndexIntermediate || i==(int)HumanBodyBones.RightIndexProximal
			//    //    || i==(int)HumanBodyBones.RightLittleDistal || i==(int)HumanBodyBones.RightLittleIntermediate || i==(int)HumanBodyBones.RightLittleProximal
			//    //    || i==(int)HumanBodyBones.RightMiddleDistal || i==(int)HumanBodyBones.RightMiddleIntermediate || i==(int)HumanBodyBones.RightMiddleProximal
			//    //    || i==(int)HumanBodyBones.RightRingDistal || i==(int)HumanBodyBones.RightRingIntermediate || i==(int)HumanBodyBones.RightRingProximal
			//    //    || i==(int)HumanBodyBones.RightThumbDistal || i==(int)HumanBodyBones.RightThumbIntermediate || i==(int)HumanBodyBones.RightThumbProximal
			//    //    ) continue;
			//    if (i == 29 || i == 28 || i == 27 || i == 38 || i == 37 || i == 36 || i == 32 || i == 31 || i == 30 || i == 35 || i == 34 || i == 33 || i == 26 || i == 25 || i == 24
			//        || i == 44 || i == 43 || i == 42 || i == 53 || i == 52 || i == 51 || i == 47 || i == 46 || i == 45 || i == 50 || i == 49 || i == 48 || i == 41 || i == 40 || i == 39) continue;
			//    if (trans == anim.GetBoneTransform((HumanBodyBones)i)) return true;
			//}

			if(anim.isHuman)
			{
				return true;
			}

			return false;
		}

		private static int[] TransformToMuscleIds(Transform trans, Animator anim)
		{
			var midxs = new[] { 0, 0, 0 };
			for(var i = 0; i < HumanTrait.BoneCount; i++)
			{
				if(trans == anim.GetBoneTransform((HumanBodyBones)i))
				{
					for(var j = 0; j < 3; j++)
					{
						var idx = HumanTrait.MuscleFromBone(i, j);
						if(idx > 0)
						{
							midxs[j] = idx;
						}
					}
				}
			}
			return midxs;
		}

		private static void AnimationHumanSymmetry()
		{
			var anim = currentObject.transform.root.gameObject.GetComponent<Animator>();
			if(!anim || !currentSkinned)
			{
				return;
			}
			if(!anim.isHuman)
			{
				return;
			}
			var posehand = new HumanPoseHandler(currentObject.transform.root.gameObject.GetComponent<Animator>().avatar
				, currentObject.transform.root);
			var humanPose = new HumanPose();
			posehand.GetHumanPose(ref humanPose);
			var musclefs = humanPose.muscles;
			var IsRight = false;
			var r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			var pdist = Vector3.Distance(r.origin, currentObject.transform.TransformPoint(BrushHitPos));
			var rp = r.GetPoint(pdist);
			var rr0 = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position));
			var rl0 = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).position));
			var dr = Vector3.Distance(rp, rr0.GetPoint(pdist));
			var dl = Vector3.Distance(rp, rl0.GetPoint(pdist));
			if(dl < dr)
			{
				IsRight = true;
			}
			var lidxs = new[] { 13, 1, 19, 21, 5, 17, 29, 28, 27, 38, 37, 36, 15, 3, 32, 31, 30, 35, 34, 33, 11 };
			var ridxs = new[] { 14, 2, 20, 22, 6, 18, 44, 43, 42, 53, 52, 51, 16, 4, 47, 46, 45, 50, 49, 48, 12 };
			for(var i = 0; i < lidxs.Length; i++)
			{
				var lti0 = TransformToMuscleIds(anim.GetBoneTransform((HumanBodyBones)lidxs[i]), anim);
				var rti0 = TransformToMuscleIds(anim.GetBoneTransform((HumanBodyBones)ridxs[i]), anim);
				for(var j = 0; j < lti0.Length; j++)
				{
					if(rti0[j] == 0 || lti0[j] == 0)
					{
						continue;
					}
					if(IsRight)
					{
						musclefs[lti0[j]] = musclefs[rti0[j]];
					}
					else
					{
						musclefs[rti0[j]] = musclefs[lti0[j]];
					}
				}
			}

			humanPose.muscles = musclefs;
			posehand.SetHumanPose(ref humanPose);
		}

		private static void AnimationSymmetry()
		{
			var anim = currentObject.transform.root.gameObject.GetComponent<Animator>();
			if(!anim || !currentSkinned)
			{
				return;
			}
			if(anim.isHuman)
			{
				var posehand = new HumanPoseHandler(currentObject.transform.root.gameObject.GetComponent<Animator>().avatar
					, currentObject.transform.root);
				var humanPose = new HumanPose();
				posehand.GetHumanPose(ref humanPose);
				var musclefs = humanPose.muscles;
				var IsRight = false;
				var r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				var pdist = Vector3.Distance(r.origin, currentObject.transform.TransformPoint(BrushHitPos));
				var rp = r.GetPoint(pdist);
				var rr0 = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position));
				var rl0 = HandleUtility.GUIPointToWorldRay(HandleUtility.WorldToGUIPoint(anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).position));
				var dr = Vector3.Distance(rp, rr0.GetPoint(pdist));
				var dl = Vector3.Distance(rp, rl0.GetPoint(pdist));
				if(dl < dr)
				{
					IsRight = true;
				}
				var lidxs = new[] { 13, 1, 19, 21, 5, 17, 29, 28, 27, 38, 37, 36, 15, 3, 32, 31, 30, 35, 34, 33, 11 };
				var ridxs = new[] { 14, 2, 20, 22, 6, 18, 44, 43, 42, 53, 52, 51, 16, 4, 47, 46, 45, 50, 49, 48, 12 };
				for(var i = 0; i < lidxs.Length; i++)
				{
					var lti0 = TransformToMuscleIds(anim.GetBoneTransform((HumanBodyBones)lidxs[i]), anim);
					var rti0 = TransformToMuscleIds(anim.GetBoneTransform((HumanBodyBones)ridxs[i]), anim);
					for(var j = 0; j < lti0.Length; j++)
					{
						if(rti0[j] == 0 || lti0[j] == 0)
						{
							continue;
						}
						if(IsRight)
						{
							musclefs[lti0[j]] = musclefs[rti0[j]];
						}
						else
						{
							musclefs[rti0[j]] = musclefs[lti0[j]];
						}
					}
				}
				humanPose.muscles = musclefs;
				posehand.SetHumanPose(ref humanPose);
			}
			else
			{
				var bones = currentSkinned.bones;
				var bonenames = Enumerable.Repeat("", bones.Length).ToArray();
				var boneNameDict = new Dictionary<string, int>();
				var issymmed = Enumerable.Repeat(false, bones.Length).ToArray();
				for(var i = 0; i < bones.Length; i++)
				{
					/*String subname = bones[i].name.ToUpper().Replace("LEFT", "").Replace("RIGHT", "");
					try
					{
					    if (subname.EndsWith("L") || subname.EndsWith("R")) subname.Substring(0,subname.Length - 1);
					    Debug.Log(subname);
					}
					catch { }
					//boneNameDict[i] = subname;
					int idx0;
					if (boneNameDict.TryGetValue(subname, out idx0))
					{
					    if(bones[i].name.Contains("LEFT") || bones[i].name.Contains("RIGHT"))
					    {

					    }
					}*/

					var subname = bones[i].name.ToUpper();
					var isleft = false;
					if(subname.Contains("LEFT"))
					{
						isleft = true;
						subname = subname.Replace("LEFT", "");
					}
					else if(subname.EndsWith("L"))
					{
						isleft = true;
						subname = subname.Substring(0, subname.Length - 1);
					}
					if(isleft)
					{
						var idx0 = -1;
						boneNameDict.TryGetValue(subname, out idx0);
						if(idx0 >= 0 && idx0 != i)
						{
							var symmvec = bones[i].position;
							var symmq = bones[i].localRotation;

							//symmvec.x = -bones[idx0].position.x;

							symmq = bones[idx0].localRotation;

							//symmq.x = bones[idx0].rotation.z;
							//symmq.y = bones[idx0].rotation.w;
							//symmq.z = bones[idx0].rotation.x;
							//symmq.w = bones[idx0].rotation.y;

							//symmq.x = bones[idx0].localRotation.z;
							//symmq.y = bones[idx0].localRotation.w;
							//symmq.z = bones[idx0].localRotation.x;
							//symmq.w = bones[idx0].localRotation.y;

							//bones[i].SetPositionAndRotation(symmvec, symmq);
							bones[i].position = symmvec;
							bones[i].localRotation = symmq;
						}
						else
						{
							boneNameDict[subname] = i;
						}
					}
					else
					{
						var isright = false;
						if(subname.Contains("RIGHT"))
						{
							isright = true;
							subname = subname.Replace("RIGHT", "");
						}
						else if(subname.EndsWith("R"))
						{
							isright = true;
							subname = subname.Substring(0, subname.Length - 1);
						}
						if(isright)
						{
							var idx0 = 0;
							boneNameDict.TryGetValue(subname, out idx0);
							if(idx0 >= 0 && idx0 != i)
							{
								var symmvec = bones[i].position;
								var symmq = bones[i].localRotation;

								//symmvec.x = -bones[idx0].position.x;

								symmq = bones[idx0].localRotation;

								//symmq.x = bones[idx0].rotation.z;
								//symmq.y = bones[idx0].rotation.w;
								//symmq.z = bones[idx0].rotation.x;
								//symmq.w = bones[idx0].rotation.y;

								//symmq.x = bones[idx0].localRotation.z;
								//symmq.y = bones[idx0].localRotation.w;
								//symmq.z = bones[idx0].localRotation.x;
								//symmq.w = bones[idx0].localRotation.y;

								//bones[i].SetPositionAndRotation(symmvec, symmq);
								bones[i].position = symmvec;
								bones[i].localRotation = symmq;
							}
							else
							{
								boneNameDict[subname] = i;
							}
						}
					}
				}
			}
		}

		private static GameObject HumanBodySetParent(Transform parentt, int idx)
		{
			GameObject go = null;
			try
			{
				go = parentt.Find(HumanTrait.BoneName[idx]).gameObject;
			}
			catch
			{
			}
			if(go == null)
			{
				go = new GameObject();
				go.name = HumanTrait.BoneName[idx];
				GameObjectUtility.SetParentAndAlign(go, parentt.gameObject);
			}
			return go;
		}

		private static void RebuildSkeleton()
		{
			var roott = currentObject.transform.root;
			GameObject skeletonRootObj = null;
			try
			{
				skeletonRootObj = roott.Find("EditorSculptSkeleton").gameObject;
			}
			catch
			{
			}
			if(skeletonRootObj == null)
			{
				skeletonRootObj = new GameObject();
				skeletonRootObj.name = "EditorSculptSkeleton";
				GameObjectUtility.SetParentAndAlign(skeletonRootObj, roott.gameObject);
			}
			var hipsobj = HumanBodySetParent(skeletonRootObj.transform, (int)HumanBodyBones.Hips);
			var spineObj = HumanBodySetParent(hipsobj.transform, (int)HumanBodyBones.Spine);
			var leftUpperLegObj = HumanBodySetParent(spineObj.transform, (int)HumanBodyBones.LeftUpperLeg);
			var leftLowerLegObj = HumanBodySetParent(leftUpperLegObj.transform, (int)HumanBodyBones.LeftLowerLeg);
			var leftfootobj = HumanBodySetParent(leftLowerLegObj.transform, (int)HumanBodyBones.LeftFoot);
			var rightUpperLegObj = HumanBodySetParent(spineObj.transform, (int)HumanBodyBones.RightUpperLeg);
			var rightLowerLegObj = HumanBodySetParent(rightUpperLegObj.transform, (int)HumanBodyBones.RightLowerLeg);
			var rightfootobj = HumanBodySetParent(rightLowerLegObj.transform, (int)HumanBodyBones.RightFoot);
			var chestObj = HumanBodySetParent(spineObj.transform, (int)HumanBodyBones.Chest);
			var upperChestObj = HumanBodySetParent(chestObj.transform, (int)HumanBodyBones.UpperChest);
			var neckObj = HumanBodySetParent(upperChestObj.transform, (int)HumanBodyBones.Neck);
			var headObj = HumanBodySetParent(neckObj.transform, (int)HumanBodyBones.Head);
			var leftShoulderObj = HumanBodySetParent(upperChestObj.transform, (int)HumanBodyBones.LeftShoulder);
			var leftUpperArmObj = HumanBodySetParent(leftShoulderObj.transform, (int)HumanBodyBones.LeftUpperArm);
			var leftLowerArmObj = HumanBodySetParent(leftUpperArmObj.transform, (int)HumanBodyBones.LeftLowerArm);
			var lefthandobj = HumanBodySetParent(leftLowerArmObj.transform, (int)HumanBodyBones.LeftHand);
			var rightShoulderObj = HumanBodySetParent(upperChestObj.transform, (int)HumanBodyBones.RightShoulder);
			var rightUpperArmObj = HumanBodySetParent(rightShoulderObj.transform, (int)HumanBodyBones.RightUpperArm);
			var rightLowerArmObj = HumanBodySetParent(rightUpperArmObj.transform, (int)HumanBodyBones.RightLowerArm);
			var righthandobj = HumanBodySetParent(rightLowerArmObj.transform, (int)HumanBodyBones.RightHand);

			var translist = new List<Transform>();
			var bindposelist = new List<Matrix4x4>();
			var childs = GetChildObjects(skeletonRootObj);
			foreach(var child in childs)
			{
				translist.Add(child.transform);
				bindposelist.Add(child.transform.worldToLocalMatrix * currentObject.transform.localToWorldMatrix);
			}
			currentSkinned.bones = translist.ToArray();
			currentMesh.bindposes = bindposelist.ToArray();

			var vertices = currentMesh.vertices;
			var weight1list = new List<BoneWeight1>();
			var perverlist = new List<byte>();
			var BrushRadFix = GlobalBrushRad
				? BrushRadius
				  * HandleUtility.GetHandleSize(currentObject.transform.TransformPoint(BrushBoneHitPos)) * 2.0f
				: BrushRadius;
			for(var i = 0; i < vertices.Length; i++)
			{
				var v0 = vertices[i];

				//v0 = currentObject.transform.TransformPoint(v0);
				var minint = -1;
				var minf = -1.0f;
				for(var j = 0; j < translist.Count; j++)
				{
					var d0 = Vector3.Distance(v0, currentObject.transform.InverseTransformPoint(translist[j].position));
					if(minf < 0.0f || d0 < minf)
					{
						minf = d0;
						minint = j;
					}
				}
				weight1list.Add(new BoneWeight1 { boneIndex = minint, weight = 1.0f - minf / BrushRadFix });
				perverlist.Add(1);
			}
			var weight1s = new NativeArray<BoneWeight1>(weight1list.ToArray(), Allocator.Temp);
			var PerVerts = new NativeArray<byte>(perverlist.ToArray(), Allocator.Temp);
			currentMesh.SetBoneWeights(PerVerts, weight1s);
		}

		private static void GetEditorSculptHumanDict()
		{
			humanDict = new Dictionary<string, Transform>();
			var skeletonRoot = currentObject.transform.root.Find("EditorSculptSkeleton");
			if(skeletonRoot == null)
			{
				return;
			}
			var hipt = skeletonRoot.Find("Hips");
			var spinet = hipt != null ? hipt.Find("Spine") : null;
			var leftUpperLegt = spinet != null ? spinet.Find("LeftUpperLeg") : null;
			var leftLowerLegt = leftUpperLegt != null ? leftUpperLegt.Find("LeftLowerLeg") : null;
			var leftFoott = leftLowerLegt != null ? leftLowerLegt.Find("LeftFoot") : null;
			var rightUpperLegt = spinet != null ? spinet.Find("RightUpperLeg") : null;
			var rightLowerLegt = rightUpperLegt != null ? rightUpperLegt.Find("RightLowerLeg") : null;
			var rightFoott = rightLowerLegt != null ? rightLowerLegt.Find("RightFoot") : null;
			var chestt = spinet != null ? spinet.Find("Chest") : null;
			var upperChestt = chestt != null ? chestt.Find("UpperChest") : null;
			var neckt = upperChestt != null ? upperChestt.Find("Neck") : null;
			var headt = neckt != null ? neckt.Find("Head") : null;
			var leftShouldert = upperChestt != null ? upperChestt.Find("LeftShoulder") : null;
			var leftUpperArmt = leftShouldert != null ? leftShouldert.Find("LeftUpperArm") : null;
			var leftLowerArmt = leftUpperArmt != null ? leftUpperArmt.Find("LeftLowerArm") : null;
			var leftHandt = leftLowerArmt != null ? leftLowerArmt.Find("LeftHand") : null;
			var rightShouldert = upperChestt != null ? upperChestt.Find("RightShoulder") : null;
			var rightUpperArmt = rightShouldert != null ? rightShouldert.Find("RightUpperArm") : null;
			var rightLowerArmt = rightUpperArmt != null ? rightUpperArmt.Find("RightLowerArm") : null;
			var rightHandt = rightLowerArmt != null ? rightLowerArmt.Find("RightHand") : null;

			if(hipt != null)
			{
				humanDict["Hips"] = hipt;
			}
			if(spinet != null)
			{
				humanDict["Spine"] = spinet;
			}
			if(leftUpperLegt != null)
			{
				humanDict["LeftUpperLeg"] = leftUpperLegt;
			}
			if(leftLowerLegt != null)
			{
				humanDict["LeftLowerLeg"] = leftLowerLegt;
			}
			if(leftFoott != null)
			{
				humanDict["LeftFoot"] = leftFoott;
			}
			if(rightUpperLegt != null)
			{
				humanDict["RightUpperLeg"] = rightUpperLegt;
			}
			if(rightLowerLegt != null)
			{
				humanDict["RightLowerLeg"] = rightLowerLegt;
			}
			if(rightFoott != null)
			{
				humanDict["RightFoot"] = rightFoott;
			}
			if(chestt != null)
			{
				humanDict["Chest"] = chestt;
			}
			if(upperChestt != null)
			{
				humanDict["UpperChest"] = upperChestt;
			}
			if(neckt != null)
			{
				humanDict["Neck"] = neckt;
			}
			if(headt != null)
			{
				humanDict["Head"] = headt;
			}
			if(leftShouldert != null)
			{
				humanDict["LeftShoulder"] = leftShouldert;
			}
			if(leftUpperArmt != null)
			{
				humanDict["LeftUpperArm"] = leftUpperArmt;
			}
			if(leftLowerArmt != null)
			{
				humanDict["LeftLowerArm"] = leftLowerArmt;
			}
			if(leftHandt != null)
			{
				humanDict["LeftHand"] = leftHandt;
			}
			if(rightShouldert != null)
			{
				humanDict["RightShoulder"] = rightShouldert;
			}
			if(rightUpperArmt != null)
			{
				humanDict["RightUpperArm"] = rightUpperArmt;
			}
			if(rightLowerArmt != null)
			{
				humanDict["RightLowerArm"] = rightLowerArmt;
			}
			if(rightHandt != null)
			{
				humanDict["RightHand"] = rightHandt;
			}

			//List<HumanBone> humanBoneList = new List<HumanBone>();
			//String[] humanName = HumanTrait.BoneName;
			//for (int i = 0; i < HumanTrait.BoneCount; i++)
			//{
			//    if (!HumanTrait.RequiredBone(i) && i != (int)HumanBodyBones.Chest && i != (int)HumanBodyBones.UpperChest && i != (int)HumanBodyBones.Neck
			//        && i != (int)HumanBodyBones.LeftShoulder && i != (int)HumanBodyBones.RightShoulder) continue;
			//    //humanBoneList.Add(humanb);
			//}
		}

		private static void HumanWizardNextIdx()
		{
			var roott = currentObject.transform.root;
			GameObject skeletonRootObj = null;
			try
			{
				skeletonRootObj = roott.Find("EditorSculptSkeleton").gameObject;
			}
			catch
			{
			}
			if(skeletonRootObj == null)
			{
				skeletonRootObj = new GameObject();
				skeletonRootObj.name = "EditorSculptSkeleton";
				GameObjectUtility.SetParentAndAlign(skeletonRootObj, roott.gameObject);
				humanWizardIdx = (int)HumanBodyBones.Hips;
			}
			var oldWizardidx = humanWizardIdx;
			switch(humanWizardIdx)
			{
				case (int)HumanBodyBones.Hips:
					humanWizardIdx = (int)HumanBodyBones.Spine;
					break;
				case (int)HumanBodyBones.Spine:
					humanWizardIdx = (int)HumanBodyBones.LeftUpperLeg;
					break;
				case (int)HumanBodyBones.LeftUpperLeg:
					humanWizardIdx = (int)HumanBodyBones.LeftLowerLeg;
					break;
				case (int)HumanBodyBones.LeftLowerLeg:
					humanWizardIdx = (int)HumanBodyBones.LeftFoot;
					break;
				case (int)HumanBodyBones.LeftFoot:
					humanWizardIdx = (int)HumanBodyBones.RightUpperLeg;
					break;
				case (int)HumanBodyBones.RightUpperLeg:
					humanWizardIdx = (int)HumanBodyBones.RightLowerLeg;
					break;
				case (int)HumanBodyBones.RightLowerLeg:
					humanWizardIdx = (int)HumanBodyBones.RightFoot;
					break;
				case (int)HumanBodyBones.RightFoot:
					humanWizardIdx = (int)HumanBodyBones.Chest;
					break;
				case (int)HumanBodyBones.Chest:
					humanWizardIdx = (int)HumanBodyBones.UpperChest;
					break;
				case (int)HumanBodyBones.UpperChest:
					humanWizardIdx = (int)HumanBodyBones.Neck;
					break;
				case (int)HumanBodyBones.Neck:
					humanWizardIdx = (int)HumanBodyBones.Head;
					break;
				case (int)HumanBodyBones.Head:
					humanWizardIdx = (int)HumanBodyBones.LeftShoulder;
					break;
				case (int)HumanBodyBones.LeftShoulder:
					humanWizardIdx = (int)HumanBodyBones.LeftUpperArm;
					break;
				case (int)HumanBodyBones.LeftUpperArm:
					humanWizardIdx = (int)HumanBodyBones.LeftLowerArm;
					break;
				case (int)HumanBodyBones.LeftLowerArm:
					humanWizardIdx = (int)HumanBodyBones.LeftHand;
					break;
				case (int)HumanBodyBones.LeftHand:
					humanWizardIdx = (int)HumanBodyBones.RightShoulder;
					break;
				case (int)HumanBodyBones.RightShoulder:
					humanWizardIdx = (int)HumanBodyBones.RightUpperArm;
					break;
				case (int)HumanBodyBones.RightUpperArm:
					humanWizardIdx = (int)HumanBodyBones.RightLowerArm;
					break;
				case (int)HumanBodyBones.RightLowerArm:
					humanWizardIdx = (int)HumanBodyBones.RightHand;
					break;
				case (int)HumanBodyBones.RightHand:
					humanWizardIdx = (int)HumanBodyBones.Hips;
					humantrans = null;
					return;

				//break;
			}
			if(humanWizardIdx < 0)
			{
				humanWizardIdx = 0;
			}
			if(oldWizardidx != humanWizardIdx)
			{
				humantrans = humanDict[HumanTrait.BoneName[humanWizardIdx]];
			}

			//Debug.Log(humantrans.name);
		}

		private static void MeshScale(Mesh mesh)
		{
			//Vector3 boundsvec = Vector3.zero;
			//try
			//{
			//    boundsvec = currentSkinned.localBounds.center - currentObject.transform.InverseTransformPoint(currentSkinned.bounds.center);
			//    float skinnb = currentSkinned.localBounds.size.magnitude;
			//    float skinnb2 = currentSkinned.bounds.size.magnitude;
			//}
			//catch { }

			if(CheckEditorSculptObj(mesh))
			{
				return;
			}
			if(CheckEditorSculptGameObj(currentObject))
			{
				return;
			}

			//if (currentSkinned != null) return;

			var scalef = (IsModelImporter ? importSize : 1.0f) / currentObject.transform.lossyScale.x;
			currentObject.transform.root.localScale = new Vector3(scalef, scalef, scalef);

			//if (currentSkinned != null)
			//{
			//    currentObject.transform.root.localScale= new Vector3(scalef, scalef, scalef);
			//    return;
			//}

			//Vector3[] vertices = mesh.vertices;

			////float scalef = currentObject.transform.lossyScale.x;
			////float scalef = currentObject.transform.lossyScale.x * (IsModelImporter ? importSize : 1.0f);
			////Canged 2024/12/14
			////float scalef = (IsModelImporter ? importSize : 1.0f)/ currentObject.transform.lossyScale.x;
			////End Changed 2024/12/14
			////Vector3 centpos;
			////if (currentSkinned != null)
			////{
			////    try
			////    {
			////        if (currentSkinned.rootBone.parent != currentObject.transform.parent)
			////        {
			////            currentSkinned.rootBone.parent.localScale = new Vector3(scalef, scalef, scalef);
			////        }

			////        //currentSkinned.rootBone.parent.localScale *= scalef;

			////        //centpos = currentSkinned.rootBone.parent.position + (currentSkinned.rootBone.localPosition) * scalef;
			////        //GameObject[] childs = GetChildObjects(currentSkinned.rootBone.parent.gameObject);
			////        //bool isSameRoot = false;
			////        //foreach(GameObject go in childs)
			////        //{
			////        //    if(currentObject==go)
			////        //    {
			////        //        isSameRoot = true;
			////        //        break;
			////        //    }
			////        //}
			////        //if(!isSameRoot)currentSkinned.rootBone.parent.localScale *= scalef;
			////    }
			////    catch { }
			////    for (int i = 0; i < vertices.Length; i++)
			////    {
			////        vertices[i] = vertices[i] * scalef;
			////        //vertices[i] = (vertices[i] - centpos) * scalef + centpos;
			////    }
			////}
			////else
			////{
			////    for (int i = 0; i < vertices.Length; i++)
			////    {
			////        vertices[i] = vertices[i] * scalef;
			////    }
			////}

			//for (int i = 0; i < vertices.Length; i++)
			//{
			//    vertices[i] = vertices[i] * scalef;
			//}
			//currentMesh.vertices = vertices;
			//currentObject.transform.localScale = Vector3.one;
			//currentMesh.RecalculateBounds();

			//BlendShapeScale(mesh, scalef);
		}

		private static void BlendShapeScale(Mesh mesh, float scalef)
		{
			var bcnt = mesh.blendShapeCount;
			if(bcnt < 1)
			{
				return;
			}
			var vertices = mesh.vertices;
			var vcnt = mesh.vertexCount;
			var blendnames = new List<string>();
			var bweights = new List<float>();
			var bvertices = new List<Vector3[]>();
			var bnormals = new List<Vector3[]>();
			var btangents = new List<Vector3[]>();
			for(var i = 0; i < bcnt; i++)
			{
				var fcnt = mesh.GetBlendShapeFrameCount(i);
				for(var j = 0; j < fcnt; j++)
				{
					blendnames.Add(mesh.GetBlendShapeName(i));
					bweights.Add(mesh.GetBlendShapeFrameWeight(i, j));
					var dverts = Enumerable.Repeat(Vector3.zero, vertices.Length).ToArray();
					var dnormals = Enumerable.Repeat(Vector3.zero, vertices.Length).ToArray();
					var dtangents = Enumerable.Repeat(Vector3.zero, vertices.Length).ToArray();
					mesh.GetBlendShapeFrameVertices(i, j, dverts, dnormals, dtangents);
					for(var k = 0; k < vcnt; k++)
					{
						dverts[k] *= scalef;
					}
					bvertices.Add(dverts);
					bnormals.Add(dnormals);
					btangents.Add(dtangents);
				}
			}

			mesh.ClearBlendShapes();
			for(var i = 0; i < blendnames.Count; i++)
			{
				mesh.AddBlendShapeFrame(blendnames[i], bweights[i], bvertices[i], bnormals[i], btangents[i]);
			}
		}

		private static void MergeVerts(Mesh mesh)
		{
			var vertices = mesh.vertices;
			var subcnt = IsMonoSubmesh ? 1 : mesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}
			var colors = mesh.colors;
			if(colors.Length < 1)
			{
				colors = Enumerable.Repeat(Color.white, vertices.Length).ToArray();
			}
			var colorlist = new List<Color>();
			var uv0s = mesh.uv;
			var uv2s = mesh.uv2;
			var uv3s = mesh.uv3;
			var uv4s = mesh.uv4;
			var binds = mesh.bindposes;
			var weight1s = mesh.GetAllBoneWeights().ToArray();
			var pervers = mesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var pcnt = 0;
			if(IsWeight)
			{
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			if(uv0s.Length < 1)
			{
				uv0s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv2s.Length < 1)
			{
				uv2s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv3s.Length < 1)
			{
				uv3s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv4s.Length < 1)
			{
				uv4s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			var cnt0 = 0;
			var w1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
			var subddict = new Dictionary<Vector3, int>();
			var newvlist = new List<Vector3>();
			var newcolorlist = new List<Color>();
			var newuv0list = new List<Vector2>();
			var newuv2list = new List<Vector2>();
			var newuv3list = new List<Vector2>();
			var newuv4list = new List<Vector2>();
			var newperverlist = new List<byte>();
			var newbwlist = new List<BoneWeight1>();
			var newtrilistlist = new List<List<int>>();
			for(var h = 0; h < subcnt; h++)
			{
				var uv0list = new List<Vector2>();
				var uv2list = new List<Vector2>();
				var uv3list = new List<Vector2>();
				var uv4list = new List<Vector2>();
				var bonew1list = new List<BoneWeight1>();
				var perverlist = new List<byte>();
				var newverlist = new List<Vector3>();
				var triarr = triarrlist[h];
				var newtrilist = new List<int>();
				for(var i = 0; i < triarr.Length; i += 3)
				{
					cnt0 = newtrilist.Count;
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					if(v0 == v1 || v1 == v2 || v0 == v2)
					{
						continue;
					}
					newverlist.AddRange(new[] { v0, v1, v2 });
					newtrilist.AddRange(new[] { cnt0, cnt0 + 1, cnt0 + 2 });
					colorlist.AddRange(new[] { colors[triarr[i]], colors[triarr[i + 1]], colors[triarr[i + 2]] });
					uv0list.AddRange(new[] { uv0s[triarr[i]], uv0s[triarr[i + 1]], uv0s[triarr[i + 2]] });
					uv2list.AddRange(new[] { uv2s[triarr[i]], uv2s[triarr[i + 1]], uv2s[triarr[i + 2]] });
					uv3list.AddRange(new[] { uv3s[triarr[i]], uv3s[triarr[i + 1]], uv3s[triarr[i + 2]] });
					uv4list.AddRange(new[] { uv4s[triarr[i]], uv4s[triarr[i + 1]], uv4s[triarr[i + 2]] });
					if(IsWeight)
					{
						int pv0 = pervers[triarr[i]];
						int pv1 = pervers[triarr[i + 1]];
						int pv2 = pervers[triarr[i + 2]];
						var bwlist0 = new List<BoneWeight1>();
						if(pv0 > 0)
						{
							for(var j = 0; j < pv0; j++)
							{
								try
								{
									bwlist0.Add(weight1s[boneidxs[triarr[i]] + j]);
								}
								catch
								{
									bwlist0.Add(w1);
								}
							}
						}
						if(bwlist0.Count < 1)
						{
							bwlist0.Add(w1);
						}
						var bwlist1 = new List<BoneWeight1>();
						if(pv1 > 0)
						{
							for(var j = 0; j < pv1; j++)
							{
								try
								{
									bwlist1.Add(weight1s[boneidxs[triarr[i + 1]] + j]);
								}
								catch
								{
									bwlist1.Add(w1);
								}
							}
						}
						if(bwlist1.Count < 1)
						{
							bwlist1.Add(w1);
						}
						var bwlist2 = new List<BoneWeight1>();
						if(pv2 > 0)
						{
							for(var j = 0; j < pv2; j++)
							{
								try
								{
									bwlist2.Add(weight1s[boneidxs[triarr[i + 2]] + j]);
								}
								catch
								{
									bwlist2.Add(w1);
								}
							}
						}
						if(bwlist2.Count < 1)
						{
							bwlist2.Add(w1);
						}
						bonew1list.AddRange(bwlist0);
						bonew1list.AddRange(bwlist1);
						bonew1list.AddRange(bwlist2);
						var bi0 = (byte)bwlist0.Count;
						var bi1 = (byte)bwlist1.Count;
						var bi2 = (byte)bwlist2.Count;
						perverlist.AddRange(new[] { bi0, bi1, bi2 });
					}
				}
				pcnt = 0;
				if(IsWeight)
				{
					boneidxs = Enumerable.Repeat(0, perverlist.Count).ToArray();
					for(var i = 0; i < perverlist.Count; i++)
					{
						boneidxs[i] = pcnt;
						pcnt += perverlist[i];
					}
					if(boneidxs.Length < 1)
					{
						boneidxs = Enumerable.Repeat(0, newvlist.Count).ToArray();
					}
				}

				//vertices = newverlist.ToArray();
				triarr = newtrilist.ToArray();
				var calcarr = new int[newverlist.Count];
				for(var i = 0; i < newverlist.Count; i++)
				{
					int j;
					if(subddict.TryGetValue(newverlist[i], out j))
					{
						calcarr[i] = j;
					}
					else
					{
						var idx0 = subddict.Count;
						subddict[newverlist[i]] = idx0;
						calcarr[i] = idx0;
						newvlist.Add(newverlist[i]);
						newcolorlist.Add(colorlist[i]);
						newuv0list.Add(uv0list[i]);
						newuv2list.Add(uv2list[i]);
						newuv3list.Add(uv3list[i]);
						newuv4list.Add(uv4list[i]);
						if(IsWeight)
						{
							int pi0 = perverlist[i];
							var bwlist0 = new List<BoneWeight1>();
							if(pi0 > 0)
							{
								for(var k = 0; k < pi0; k++)
								{
									try
									{
										bwlist0.Add(bonew1list[boneidxs[i] + k]);
									}
									catch
									{
										bwlist0.Add(w1);
									}
								}
							}
							if(bwlist0.Count < 1)
							{
								bwlist0.Add(w1);
							}
							newbwlist.AddRange(bwlist0);
							newperverlist.Add((byte)bwlist0.Count);
						}
					}
				}
				for(var i = 0; i < newtrilist.Count; i++)
				{
					var t = calcarr[triarr[i]];
					newtrilist[i] = t;
				}
				newtrilistlist.Add(newtrilist);
			}
			mesh.Clear();
			if(newvlist.Count > 65000)
			{
				try
				{
					mesh.indexFormat = IndexFormat.UInt32;
				}
				catch
				{
					return;
				}
			}
			else
			{
				mesh.indexFormat = IndexFormat.UInt16;
			}
			mesh.vertices = newvlist.ToArray();

			//matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint + 1; i++)
			{
				if(IsMonoSubmesh)
				{
					mesh.SetTriangles(newtrilistlist[0], i);
				}
				else
				{
					if(newtrilistlist.Count > i)
					{
						mesh.SetTriangles(newtrilistlist[i], i);
					}
					else
					{
						mesh.SetTriangles(newtrilistlist[0], i);
					}
				}
			}
			mesh.colors = newcolorlist.ToArray();
			mesh.uv = newuv0list.ToArray();
			mesh.uv2 = newuv2list.ToArray();
			mesh.uv3 = newuv3list.ToArray();
			mesh.uv4 = newuv4list.ToArray();
			mesh.bindposes = binds;
			if(IsWeight)
			{
				var ntweight1s = new NativeArray<BoneWeight1>(newbwlist.ToArray(), Allocator.Temp);
				var PerVerts = new NativeArray<byte>(newperverlist.ToArray(), Allocator.Temp);
				mesh.SetBoneWeights(PerVerts, ntweight1s);
			}
			else
			{
				BoneWeight1Create(mesh);
			}
			CalcMeshNormals(mesh);
			mesh.RecalculateBounds();
			mesh.RecalculateTangents();
		}

		private static void MergetriGenerate(Mesh mesh)
		{
			var vertices = mesh.vertices;
			var subcnt = mesh.subMeshCount;
			var tarrlist = new List<int>();
			var mergelist = new List<int>();
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}
			var trihash = new HashSet<Vector3>();
			for(var i = 0; i < subcnt; i++)
			{
				var triangles = mesh.GetTriangles(i);
				for(var j = 0; j < triangles.Length; j += 3)
				{
					var v0 = vertices[triangles[j]];
					var v1 = vertices[triangles[j + 1]];
					var v2 = vertices[triangles[j + 2]];
					var vp0 = (v0 + v1 + v2) * 0.333f;
					if(trihash.Contains(vp0))
					{
						continue;
					}
					trihash.Add(vp0);
					tarrlist.AddRange(new[] { triangles[j], triangles[j + 1], triangles[j + 2] });
				}
			}
			tris = tarrlist.ToArray();
			mergedtris = new int[tris.Length];
			var newvdict = new Dictionary<Vector3, int>();
			for(var i = 0; i < vertices.Length; i++)
			{
				newvdict[vertices[i]] = i;
			}
			for(var i = 0; i < tris.Length; i++)
			{
				int j;
				newvdict.TryGetValue(vertices[tris[i]], out j);
				mergedtris[i] = j;
			}
			for(var i = 0; i < mergedtris.Length; i += 3)
			{
				var v0 = vertices[mergedtris[i]];
				var v1 = vertices[mergedtris[i + 1]];
				var v2 = vertices[mergedtris[i + 2]];
				if(v0 == v1 || v1 == v2 || v2 == v0)
				{
					mergedtris[i] = 0;
					mergedtris[i + 1] = 0;
					mergedtris[i + 2] = 0;
				}
			}
		}

		private static void MergetriApply()
		{
			var vertices = currentMesh.vertices;
			var triangles = currentMesh.triangles;
			var colors = currentMesh.colors;
			var uvs = currentMesh.uv;
			var uv2s = currentMesh.uv2;
			var uv3s = currentMesh.uv3;
			var uv4s = currentMesh.uv4;
			currentMesh.Clear();
			currentMesh.vertices = vertices;
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			currentMesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint; i++)
			{
				currentMesh.SetTriangles(triangles, i);
			}
			currentMesh.RecalculateBounds();
			CalcMeshNormals(currentMesh);
			currentMesh.colors = colors;
			currentMesh.uv = uvs;
			currentMesh.uv2 = uv2s;
			currentMesh.uv3 = uv3s;
			currentMesh.uv4 = uv4s;
		}

		private static void MergeVertsFast(Mesh mesh)
		{
			var vertices = mesh.vertices;

			//int subcnt = mesh.subMeshCount;
			var subcnt = IsMonoSubmesh ? 1 : mesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}
			var colors = mesh.colors;
			if(colors.Length < 1)
			{
				colors = Enumerable.Repeat(Color.white, vertices.Length).ToArray();
			}
			var uv0s = mesh.uv;
			var uv2s = mesh.uv2;
			var uv3s = mesh.uv3;
			var uv4s = mesh.uv4;
			var binds = mesh.bindposes;
			var weight1s = mesh.GetAllBoneWeights().ToArray();
			var pervers = mesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;

			//if (pervers.Length > 0) IsWeight = true;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var pcnt = 0;
			if(IsWeight)
			{
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}

			//if (pervers.Length < 1) pervers = Enumerable.Repeat((Byte)0, vertices.Length).ToArray();
			if(uv0s.Length < 1)
			{
				uv0s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv2s.Length < 1)
			{
				uv2s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv3s.Length < 1)
			{
				uv3s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv4s.Length < 1)
			{
				uv4s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			var bscnt = mesh.blendShapeCount;
			var blendarrlist = new List<Vector3[]>();
			var bnamelist = new List<string>();
			var bweightlist = new List<float>();
			var framecntlist = new List<int>();
			if(bscnt > 0)
			{
				for(var i = 0; i < bscnt; i++)
				{
					var fcnt = mesh.GetBlendShapeFrameCount(i);
					framecntlist.Add(fcnt);
					for(var j = 0; j < fcnt; j++)
					{
						var vcnt = mesh.vertexCount;
						var varr = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
						var vnorm = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
						var vtan = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
						mesh.GetBlendShapeFrameVertices(i, j, varr, vnorm, vtan);
						blendarrlist.Add(varr);
						bnamelist.Add(mesh.GetBlendShapeName(i));
						bweightlist.Add(mesh.GetBlendShapeFrameWeight(i, j));
					}
				}
			}
			var newtrilist = new List<List<int>>();
			var subddict = new Dictionary<Vector4, int>();
			var newvlist = new List<Vector3>();
			var newcolorlist = new List<Color>();
			var newuv0list = new List<Vector2>();
			var newuv2list = new List<Vector2>();
			var newuv3list = new List<Vector2>();
			var newuv4list = new List<Vector2>();
			var newperverlist = new List<byte>();
			var newbwlist = new List<BoneWeight1>();
			var newblendlistlist = new List<List<Vector3>>();
			for(var i = 0; i < bscnt; i++)
			{
				newblendlistlist.Add(new List<Vector3>());
			}
			var w1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
			for(var h = 0; h < subcnt; h++)
			{
				var triangles = triarrlist[h];
				var newverlist = new List<Vector3>();
				var uv0list = new List<Vector2>();
				var uv2list = new List<Vector2>();
				var uv3list = new List<Vector2>();
				var uv4list = new List<Vector2>();
				var colorlist = new List<Color>();
				var bonew1list = new List<BoneWeight1>();
				var perverlist = new List<byte>();
				var blendlistlist = new List<List<Vector3>>();
				for(var i = 0; i < bscnt; i++)
				{
					blendlistlist.Add(new List<Vector3>());
				}

				//Dictionary<Vector3, bool> polychkdict = new Dictionary<Vector3, bool>();
				var newtri = new List<int>();
				var cnt0 = 0;
				for(var i = 0; i < triangles.Length; i += 3)
				{
					cnt0 = newtri.Count;
					var v0 = vertices[triangles[i]];
					var v1 = vertices[triangles[i + 1]];
					var v2 = vertices[triangles[i + 2]];
					if(v0 == v1 || v1 == v2 || v0 == v2)
					{
						continue;
					}
					newverlist.AddRange(new[] { v0, v1, v2 });
					newtri.AddRange(new[] { cnt0, cnt0 + 1, cnt0 + 2 });
					colorlist.AddRange(new[] { colors[triangles[i]], colors[triangles[i + 1]], colors[triangles[i + 2]] });
					uv0list.AddRange(new[] { uv0s[triangles[i]], uv0s[triangles[i + 1]], uv0s[triangles[i + 2]] });
					uv2list.AddRange(new[] { uv2s[triangles[i]], uv2s[triangles[i + 1]], uv2s[triangles[i + 2]] });
					uv3list.AddRange(new[] { uv3s[triangles[i]], uv3s[triangles[i + 1]], uv3s[triangles[i + 2]] });
					uv4list.AddRange(new[] { uv4s[triangles[i]], uv4s[triangles[i + 1]], uv4s[triangles[i + 2]] });
					if(IsWeight)
					{
						//int pv0 = IsWeight ? pervers[triangles[i]] : 0;
						//int pv1 = IsWeight ? pervers[triangles[i + 1]] : 0;
						//int pv2 = IsWeight ? pervers[triangles[i + 2]] : 0;
						int pv0 = pervers[triangles[i]];
						int pv1 = pervers[triangles[i + 1]];
						int pv2 = pervers[triangles[i + 2]];
						var bwlist0 = new List<BoneWeight1>();
						if(pv0 > 0)
						{
							//for (int j = 0; j < pv0 - 1; j++)
							for(var j = 0; j < pv0; j++)
							{
								try
								{
									bwlist0.Add(weight1s[boneidxs[triangles[i]] + j]);
								}
								catch
								{
									bwlist0.Add(w1);
								}
							}
						}
						if(bwlist0.Count < 1)
						{
							bwlist0.Add(w1);
						}
						var bwlist1 = new List<BoneWeight1>();
						if(pv1 > 0)
						{
							//for (int j = 0; j < pv1 - 1; j++)
							for(var j = 0; j < pv1; j++)
							{
								try
								{
									bwlist1.Add(weight1s[boneidxs[triangles[i + 1]] + j]);
								}
								catch
								{
									bwlist1.Add(w1);
								}
							}
						}
						if(bwlist1.Count < 1)
						{
							bwlist1.Add(w1);
						}
						var bwlist2 = new List<BoneWeight1>();
						if(pv2 > 0)
						{
							//for (int j = 0; j < pv2 - 1; j++)
							for(var j = 0; j < pv2; j++)
							{
								try
								{
									bwlist2.Add(weight1s[boneidxs[triangles[i + 2]] + j]);
								}
								catch
								{
									bwlist2.Add(w1);
								}
							}
						}
						if(bwlist2.Count < 1)
						{
							bwlist2.Add(w1);
						}
						bonew1list.AddRange(bwlist0);
						bonew1list.AddRange(bwlist1);
						bonew1list.AddRange(bwlist2);
						var bi0 = (byte)bwlist0.Count;
						var bi1 = (byte)bwlist1.Count;
						var bi2 = (byte)bwlist2.Count;
						perverlist.AddRange(new[] { bi0, bi1, bi2 });
					}
					if(bscnt > 0)
					{
						for(var j = 0; j < bscnt; j++)
						{
							var b0 = blendarrlist[j][triangles[i]];
							var b1 = blendarrlist[j][triangles[i + 1]];
							var b2 = blendarrlist[j][triangles[i + 2]];
							blendlistlist[j].AddRange(new[] { b0, b1, b2 });
						}
					}
				}
				var varr = newverlist.ToArray();
				var triarr = newtri.ToArray();
				var calcarr = new int[newverlist.Count];
				pcnt = 0;
				if(IsWeight)
				{
					boneidxs = Enumerable.Repeat(0, perverlist.Count).ToArray();
					for(var i = 0; i < perverlist.Count; i++)
					{
						boneidxs[i] = pcnt;
						pcnt += perverlist[i];
					}
					if(boneidxs.Length < 1)
					{
						boneidxs = Enumerable.Repeat(0, varr.Length).ToArray();
					}
				}
				for(var i = 0; i < newverlist.Count; i++)
				{
					int j;
					var vec4 = new Vector4(varr[i].x, varr[i].y, varr[i].z, uv0list[i].x * 256.0f + uv0list[i].y);
					if(subddict.TryGetValue(vec4, out j))
					{
						calcarr[i] = j;
					}
					else
					{
						var idx0 = subddict.Count;
						subddict[vec4] = idx0;
						calcarr[i] = idx0;
						newvlist.Add(newverlist[i]);
						newcolorlist.Add(colorlist[i]);
						newuv0list.Add(uv0list[i]);
						newuv2list.Add(uv2list[i]);
						newuv3list.Add(uv3list[i]);
						newuv4list.Add(uv4list[i]);
						if(IsWeight)
						{
							int pi0 = perverlist[i];
							var bwlist0 = new List<BoneWeight1>();
							if(pi0 > 0)
							{
								//for (int k = 0; k < pi0 - 1; k++)
								for(var k = 0; k < pi0; k++)
								{
									try
									{
										bwlist0.Add(bonew1list[boneidxs[i] + k]);
									}
									catch
									{
										bwlist0.Add(w1);
									}
								}
							}
							if(bwlist0.Count < 1)
							{
								bwlist0.Add(w1);
							}
							newbwlist.AddRange(bwlist0);
							newperverlist.Add((byte)bwlist0.Count);
						}
						if(bscnt > 0)
						{
							for(var k = 0; k < bscnt; k++)
							{
								//newblendlistlist[k].Add(blendarrlist[k][j]);
								newblendlistlist[k].Add(blendlistlist[k][i]);
							}
						}
					}
				}
				for(var i = 0; i < newtri.Count; i++)
				{
					var t = calcarr[triarr[i]];
					newtri[i] = t;
				}
				newtrilist.Add(newtri);
			}
			mesh.Clear();
			if(newvlist.Count > 65000)
			{
				try
				{
					mesh.indexFormat = IndexFormat.UInt32;
				}
				catch
				{
					return;
				}
			}
			else
			{
				mesh.indexFormat = IndexFormat.UInt16;
			}
			mesh.vertices = newvlist.ToArray();
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint + 1; i++)
			{
				//if (newtrilist.Count >= i) mesh.SetTriangles(newtrilist[i], i);
				//else mesh.SetTriangles(newtrilist[0], i);
				if(IsMonoSubmesh)
				{
					mesh.SetTriangles(newtrilist[0], i);
				}
				else
				{
					if(newtrilist.Count > i)
					{
						mesh.SetTriangles(newtrilist[i], i);
					}
					else
					{
						mesh.SetTriangles(newtrilist[0], i);
					}
				}
			}
			mesh.colors = newcolorlist.ToArray();
			mesh.uv = newuv0list.ToArray();
			mesh.uv2 = newuv2list.ToArray();
			mesh.uv3 = newuv3list.ToArray();
			mesh.uv4 = newuv4list.ToArray();
			mesh.bindposes = binds;
			if(IsWeight)
			{
				var ntweight1s = new NativeArray<BoneWeight1>(newbwlist.ToArray(), Allocator.Temp);
				var PerVerts = new NativeArray<byte>(newperverlist.ToArray(), Allocator.Temp);
				mesh.SetBoneWeights(PerVerts, ntweight1s);
			}
			else
			{
				BoneWeight1Create(mesh);
			}
			if(bscnt > 0)
			{
				var cnt0 = 0;
				for(var i = 0; i < bscnt; i++)
				{
					var fcnt = framecntlist[i];
					for(var j = 0; j < fcnt; j++)
					{
						mesh.AddBlendShapeFrame(bnamelist[i], bweightlist[cnt0], newblendlistlist[cnt0].ToArray()
							, Enumerable.Repeat(Vector3.zero, mesh.vertexCount).ToArray()
							, Enumerable.Repeat(Vector3.zero, mesh.vertexCount).ToArray());
						cnt0++;
					}

					//mesh.AddBlendShapeFrame(bnamelist[i], bweightlist[i], newblendlistlist[i].ToArray()
					//    , Enumerable.Repeat(Vector3.zero, mesh.vertexCount).ToArray(), Enumerable.Repeat(Vector3.zero, mesh.vertexCount).ToArray());
				}
			}
			CalcMeshNormals(mesh);
			mesh.RecalculateBounds();
			mesh.RecalculateTangents();
		}

		private static void DecimateMesh(Mesh mesh)
		{
			var vertices = mesh.vertices;
			var subcnt = IsMonoSubmesh ? 1 : mesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}
			var colors = mesh.colors;
			if(colors.Length < 1)
			{
				colors = Enumerable.Repeat(Color.white, vertices.Length).ToArray();
			}
			var uv0s = mesh.uv;
			var uv2s = mesh.uv2;
			var uv3s = mesh.uv3;
			var uv4s = mesh.uv4;
			if(uv0s.Length < 1)
			{
				uv0s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv2s.Length < 1)
			{
				uv2s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv3s.Length < 1)
			{
				uv3s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv4s.Length < 1)
			{
				uv4s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			var binds = mesh.bindposes;
			var weight1s = mesh.GetAllBoneWeights().ToArray();
			var pervers = mesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var pcnt = 0;
			if(IsWeight)
			{
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			if(IsBrushedArray.Length != vertices.Length)
			{
				IsBrushedArray = Enumerable.Repeat(false, vertices.Length).ToArray();
			}
			var chkvarr = IsBrushedArray;
			var AdjIdxListList = new List<List<int>>();
			for(var i = 0; i < vertices.Length; i++)
			{
				AdjIdxListList.Add(new List<int>());
			}
			for(var i = 0; i < subcnt; i++)
			{
				var triarr = triarrlist[i];
				for(var j = 0; j < triarr.Length; j += 3)
				{
					var i0 = triarr[j];
					var i1 = triarr[j + 1];
					var i2 = triarr[j + 2];
					AdjIdxListList[triarr[j]].AddRange(new[] { i1, i2 });
					AdjIdxListList[triarr[j + 1]].AddRange(new[] { i0, i2 });
					AdjIdxListList[triarr[j + 2]].AddRange(new[] { i0, i1 });
				}
			}
			var dc0 = avgPointDist * 1.1f;
			var w1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
			var weight1list = new List<BoneWeight1>();
			var perverlist = new List<byte>();
			for(var i = 0; i < vertices.Length; i++)
			{
				//if (!chkvarr[i]) continue;
				var bonew1list = new List<BoneWeight1>();
				if(chkvarr[i])
				{
					var v0 = vertices[i];
					var AdjIdxList = AdjIdxListList[i];
					foreach(var t in AdjIdxList)
					{
						var vpos = vertices[t];
						var d0 = Vector3.Distance(v0, vpos);

						//if (d0 < dc0)
						//{
						//    if (AdjIdxListList[t].Count < 5) continue;
						//    vertices[t] = v0;
						//}
						if(d0 < dc0 && AdjIdxListList[t].Count >= 5)
						{
							vertices[t] = v0;
							if(IsWeight)
							{
								int pi0 = pervers[i];
								var bwlist0 = new List<BoneWeight1>();
								if(pi0 > 0)
								{
									for(var j = 0; j < pi0; j++)
									{
										try
										{
											bwlist0.Add(weight1s[boneidxs[i] + j]);
										}
										catch
										{
											bwlist0.Add(w1);
										}
									}
								}
								int pi1 = pervers[t];
								var bwlist1 = new List<BoneWeight1>();
								if(pi1 > 0)
								{
									for(var j = 0; j < pi1; j++)
									{
										try
										{
											bwlist1.Add(weight1s[boneidxs[t] + j]);
										}
										catch
										{
											bwlist1.Add(w1);
										}
									}
								}
								var bwlist2 = BoneWeight1Lerp(bwlist0, bwlist1);
								bonew1list = BoneWeight1Lerp(bonew1list, bwlist2);
							}
						}
					}
				}
				if(IsWeight && bonew1list.Count < 1)
				{
					int pi0 = pervers[i];
					var bwlist0 = new List<BoneWeight1>();
					if(pi0 > 0)
					{
						for(var j = 0; j < pi0; j++)
						{
							try
							{
								bwlist0.Add(weight1s[boneidxs[i] + j]);
							}
							catch
							{
								bwlist0.Add(w1);
							}
						}
					}
					if(bwlist0.Count < 1)
					{
						bwlist0.Add(w1);
					}
					bonew1list.AddRange(bwlist0);
				}
				if(IsWeight)
				{
					weight1list.AddRange(bonew1list);
					perverlist.Add((byte)bonew1list.Count);
				}
			}
			if(IsWeight)
			{
				weight1s = weight1list.ToArray();
				pervers = perverlist.ToArray();
				boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
				pcnt = 0;
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			var newtrilistlist = new List<List<int>>();
			for(var h = 0; h < subcnt; h++)
			{
				var newtrilist = new List<int>();
				var triarr = triarrlist[h];
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					if(v0 == v1 || v1 == v2 || v2 == v0)
					{
						continue;
					}
					newtrilist.AddRange(new[] { triarr[i], triarr[i + 1], triarr[i + 2] });
				}
				newtrilistlist.Add(newtrilist);
			}
			triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(newtrilistlist[i].ToArray());
			}
			var newvdict = new Dictionary<Vector3, int>();
			var newvlist = new List<Vector3>();
			var newcolorlist = new List<Color>();
			var newuv0list = new List<Vector2>();
			var newuv2list = new List<Vector2>();
			var newuv3list = new List<Vector2>();
			var newuv4list = new List<Vector2>();
			var newperverlist = new List<byte>();
			var newbwlist = new List<BoneWeight1>();
			var calcarr = new int[vertices.Length];
			for(var i = 0; i < vertices.Length; i++)
			{
				int j;
				if(newvdict.TryGetValue(vertices[i], out j))
				{
					calcarr[i] = j;
				}
				else
				{
					var idx0 = newvdict.Count;
					newvdict[vertices[i]] = idx0;
					calcarr[i] = idx0;
					newvlist.Add(vertices[i]);
					newcolorlist.Add(colors[i]);
					newuv0list.Add(uv0s[i]);
					newuv2list.Add(uv2s[i]);
					newuv3list.Add(uv3s[i]);
					newuv4list.Add(uv4s[i]);
					if(IsWeight)
					{
						int pi0 = pervers[i];
						var bwlist0 = new List<BoneWeight1>();
						if(pi0 > 0)
						{
							for(var k = 0; k < pi0; k++)
							{
								try
								{
									bwlist0.Add(weight1s[boneidxs[i] + k]);
								}
								catch
								{
									bwlist0.Add(w1);
								}
							}
						}
						if(bwlist0.Count < 1)
						{
							bwlist0.Add(w1);
						}
						newbwlist.AddRange(bwlist0);
						newperverlist.Add((byte)bwlist0.Count);
					}
				}
			}
			for(var i = 0; i < subcnt; i++)
			{
				var newtrilist = newtrilistlist[i];
				var triarr = triarrlist[i];
				for(var j = 0; j < newtrilist.Count; j++)
				{
					var t = calcarr[triarr[j]];
					newtrilist[j] = t;
				}
			}
			vertices = newvlist.ToArray();
			colors = newcolorlist.ToArray();
			uv0s = newuv0list.ToArray();
			uv2s = newuv2list.ToArray();
			uv3s = newuv3list.ToArray();
			uv4s = newuv4list.ToArray();
			mesh.Clear();
			if(vertices.Length > 65000)
			{
				try
				{
					mesh.indexFormat = IndexFormat.UInt32;
				}
				catch
				{
					return;
				}
			}
			else
			{
				mesh.indexFormat = IndexFormat.UInt16;
			}
			mesh.vertices = vertices;
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint + 1; i++)
			{
				if(IsMonoSubmesh)
				{
					mesh.SetTriangles(newtrilistlist[0], i);
				}
				else
				{
					if(newtrilistlist.Count > i)
					{
						mesh.SetTriangles(newtrilistlist[i], i);
					}
					else
					{
						mesh.SetTriangles(newtrilistlist[0], i);
					}
				}
			}
			mesh.colors = colors;
			mesh.uv = uv0s;
			mesh.uv2 = uv2s;
			mesh.uv3 = uv3s;
			mesh.uv4 = uv4s;
			mesh.bindposes = binds;
			if(IsWeight)
			{
				var ntweight1s = new NativeArray<BoneWeight1>(newbwlist.ToArray(), Allocator.Temp);
				var PerVerts = new NativeArray<byte>(newperverlist.ToArray(), Allocator.Temp);
				mesh.SetBoneWeights(PerVerts, ntweight1s);
			}
			else
			{
				BoneWeight1Create(mesh);
			}

			//mesh.RecalculateNormals();
			CalcMeshNormals(mesh);
			mesh.RecalculateBounds();
			mesh.RecalculateTangents();
		}

		private static void SymmetryMeshFast(Mesh mesh)
		{
			if(smode == SymmetryMode.None)
			{
				return;
			}
			var vertices = mesh.vertices;

			//int[] triangles = mesh.GetTriangles(0);
			var subcnt = IsMonoSubmesh ? 1 : mesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}
			var colors = mesh.colors;
			var uv0s = mesh.uv;
			var uv2s = mesh.uv2;
			var uv3s = mesh.uv3;
			var uv4s = mesh.uv4;
			var binds = mesh.bindposes;
			var weight1s = mesh.GetAllBoneWeights().ToArray();
			var pervers = mesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var pcnt = 0;
			if(IsWeight)
			{
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			for(var i = 0; i < vertices.Length; i++)
			{
				var v0 = vertices[i];
				if(smode == SymmetryMode.X_axis && v0.x < 0.01f)
				{
					v0.x = 0.0f;
				}
				if(smode == SymmetryMode.Y_axis && v0.y < 0.01f)
				{
					v0.y = 0.0f;
				}
				if(smode == SymmetryMode.Z_axis && v0.z < 0.01f)
				{
					v0.z = 0.0f;
				}
				vertices[i] = v0;
			}
			var idx = 0;
			var w1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
			var subddict = new Dictionary<Vector3, int>();
			var newvlist = new List<Vector3>();
			var newcolorlist = new List<Color>();
			var newuv0list = new List<Vector2>();
			var newuv2list = new List<Vector2>();
			var newuv3list = new List<Vector2>();
			var newuv4list = new List<Vector2>();
			var newperverlist = new List<byte>();
			var newbwlist = new List<BoneWeight1>();
			var newtrilistlist = new List<List<int>>();
			for(var h = 0; h < subcnt; h++)
			{
				var newverlist = new List<Vector3>();
				var colorlist = new List<Color>();
				var uv0list = new List<Vector2>();
				var uv2list = new List<Vector2>();
				var uv3list = new List<Vector2>();
				var uv4list = new List<Vector2>();
				var bonew1list = new List<BoneWeight1>();
				var perverlist = new List<byte>();
				var triarr = triarrlist[h];
				var newtrilist = new List<int>();
				for(var i = 0; i < triarr.Length; i += 3)
				{
					idx = newtrilist.Count;
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					var cv0 = colors[triarr[i]];
					var cv1 = colors[triarr[i + 1]];
					var cv2 = colors[triarr[i + 2]];
					var uv0_0 = uv0s[triarr[i]];
					var uv0_1 = uv0s[triarr[i + 1]];
					var uv0_2 = uv0s[triarr[i + 2]];
					var uv2_0 = uv2s[triarr[i]];
					var uv2_1 = uv2s[triarr[i + 1]];
					var uv2_2 = uv2s[triarr[i + 2]];
					var uv3_0 = uv3s[triarr[i]];
					var uv3_1 = uv3s[triarr[i + 1]];
					var uv3_2 = uv3s[triarr[i + 2]];
					var uv4_0 = uv4s[triarr[i]];
					var uv4_1 = uv4s[triarr[i + 1]];
					var uv4_2 = uv4s[triarr[i + 2]];
					if(smode == SymmetryMode.X_axis && (v0.x < 0.0f || v1.x < 0.0f || v2.x < 0.0f))
					{
						continue;
					}
					if(smode == SymmetryMode.X_axis && v0.x == 0.0f && v1.x == 0.0f && v2.x == 0.0f)
					{
						continue;
					}
					if(smode == SymmetryMode.Y_axis && (v0.y < 0.0f || v1.y < 0.0f || v2.y < 0.0f))
					{
						continue;
					}
					if(smode == SymmetryMode.Y_axis && v0.y == 0.0f && v1.y == 0.0f && v2.y == 0.0f)
					{
						continue;
					}
					if(smode == SymmetryMode.Z_axis && (v0.z < 0.0f || v1.z < 0.0f || v2.z < 0.0f))
					{
						continue;
					}
					if(smode == SymmetryMode.Z_axis && v0.z == 0.0f && v1.z == 0.0f && v2.z == 0.0f)
					{
						continue;
					}
					newverlist.AddRange(new[] { v0, v1, v2 });
					newtrilist.AddRange(new[] { idx, idx + 1, idx + 2 });
					colorlist.AddRange(new[] { cv0, cv1, cv2 });
					uv0list.AddRange(new[] { uv0_0, uv0_1, uv0_2 });
					uv2list.AddRange(new[] { uv2_0, uv2_1, uv2_2 });
					uv3list.AddRange(new[] { uv3_0, uv3_1, uv3_2 });
					uv4list.AddRange(new[] { uv4_0, uv4_1, uv4_2 });
					newverlist.AddRange(new[] { GetSymmetry(v0), GetSymmetry(v1), GetSymmetry(v2) });
					newtrilist.AddRange(new[] { idx + 5, idx + 4, idx + 3 });
					colorlist.AddRange(new[] { cv0, cv1, cv2 });
					uv0list.AddRange(new[] { uv0_0, uv0_1, uv0_2 });
					uv2list.AddRange(new[] { uv2_0, uv2_1, uv2_2 });
					uv3list.AddRange(new[] { uv3_0, uv3_1, uv3_2 });
					uv4list.AddRange(new[] { uv4_0, uv4_1, uv4_2 });
					if(IsWeight)
					{
						int pv0 = pervers[triarr[i]];
						int pv1 = pervers[triarr[i + 1]];
						int pv2 = pervers[triarr[i + 2]];
						var bwlist0 = new List<BoneWeight1>();
						if(pv0 > 0)
						{
							for(var j = 0; j < pv0; j++)
							{
								try
								{
									bwlist0.Add(weight1s[boneidxs[triarr[i]] + j]);
								}
								catch
								{
									bwlist0.Add(w1);
								}
							}
						}
						if(bwlist0.Count < 1)
						{
							bwlist0.Add(w1);
						}
						var bwlist1 = new List<BoneWeight1>();
						if(pv1 > 0)
						{
							for(var j = 0; j < pv1; j++)
							{
								try
								{
									bwlist1.Add(weight1s[boneidxs[triarr[i + 1]] + j]);
								}
								catch
								{
									bwlist1.Add(w1);
								}
							}
						}
						if(bwlist1.Count < 1)
						{
							bwlist1.Add(w1);
						}
						var bwlist2 = new List<BoneWeight1>();
						if(pv2 > 0)
						{
							for(var j = 0; j < pv2; j++)
							{
								try
								{
									bwlist2.Add(weight1s[boneidxs[triarr[i + 2]] + j]);
								}
								catch
								{
									bwlist2.Add(w1);
								}
							}
						}
						if(bwlist2.Count < 1)
						{
							bwlist2.Add(w1);
						}
						bonew1list.AddRange(bwlist0);
						bonew1list.AddRange(bwlist1);
						bonew1list.AddRange(bwlist2);
						var bi0 = (byte)bwlist0.Count;
						var bi1 = (byte)bwlist1.Count;
						var bi2 = (byte)bwlist2.Count;
						perverlist.AddRange(new[] { bi0, bi1, bi2 });
						bonew1list.AddRange(bwlist0);
						bonew1list.AddRange(bwlist1);
						bonew1list.AddRange(bwlist2);
						perverlist.AddRange(new[] { bi0, bi1, bi2 });
					}
				}

				//Vector3[] varr = newverlist.ToArray();
				triarr = newtrilist.ToArray();
				var calcarr = new int[newverlist.Count];
				for(var i = 0; i < newverlist.Count; i++)
				{
					int j;
					if(subddict.TryGetValue(newverlist[i], out j))
					{
						calcarr[i] = j;
					}
					else
					{
						var idx0 = subddict.Count;
						subddict[newverlist[i]] = idx0;
						calcarr[i] = idx0;
						newvlist.Add(newverlist[i]);
						newcolorlist.Add(colorlist[i]);
						newuv0list.Add(uv0list[i]);
						newuv2list.Add(uv2list[i]);
						newuv3list.Add(uv3list[i]);
						newuv4list.Add(uv4list[i]);
						if(IsWeight)
						{
							int pi0 = perverlist[i];
							var bwlist0 = new List<BoneWeight1>();
							if(pi0 > 0)
							{
								for(var k = 0; k < pi0; k++)
								{
									try
									{
										bwlist0.Add(bonew1list[boneidxs[i] + k]);
									}
									catch
									{
										bwlist0.Add(w1);
									}
								}
							}
							if(bwlist0.Count < 1)
							{
								bwlist0.Add(w1);
							}
							newbwlist.AddRange(bwlist0);
							newperverlist.Add((byte)bwlist0.Count);
						}
					}
				}
				for(var i = 0; i < newtrilist.Count; i++)
				{
					var t = calcarr[triarr[i]];
					newtrilist[i] = t;
				}
				newtrilistlist.Add(newtrilist);
			}
			vertices = newvlist.ToArray();
			mesh.Clear();
			if(vertices.Length > 65000)
			{
				try
				{
					mesh.indexFormat = IndexFormat.UInt32;
				}
				catch
				{
					return;
				}
			}
			else
			{
				mesh.indexFormat = IndexFormat.UInt16;
			}
			mesh.vertices = vertices;
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint + 1; i++)
			{
				if(IsMonoSubmesh)
				{
					mesh.SetTriangles(newtrilistlist[0], i);
				}
				else
				{
					if(newtrilistlist.Count > i)
					{
						mesh.SetTriangles(newtrilistlist[i], i);
					}
					else
					{
						mesh.SetTriangles(newtrilistlist[0], i);
					}
				}
			}
			mesh.colors = newcolorlist.ToArray();
			mesh.uv = newuv0list.ToArray();
			mesh.uv2 = newuv2list.ToArray();
			mesh.uv3 = newuv3list.ToArray();
			mesh.uv4 = newuv4list.ToArray();
			mesh.bindposes = binds;
			if(IsWeight)
			{
				var ntweight1s = new NativeArray<BoneWeight1>(newbwlist.ToArray(), Allocator.Temp);
				var PerVerts = new NativeArray<byte>(newperverlist.ToArray(), Allocator.Temp);
				mesh.SetBoneWeights(PerVerts, ntweight1s);
			}
			else
			{
				BoneWeight1Create(mesh);
			}
			mesh.RecalculateBounds();

			//mesh.RecalculateNormals();
			CalcMeshNormals(mesh);
			mesh.RecalculateTangents();
		}

		private static void DecimateCenter(Mesh mesh)
		{
			var vertices = mesh.vertices;
			var triangles = mesh.GetTriangles(0);
			var midxarr = Enumerable.Repeat(1, vertices.Length).ToArray();
			var centlist = new List<Vector3>();
			for(var i = 0; i < triangles.Length; i += 3)
			{
				var centv = (vertices[triangles[i]] + vertices[triangles[i + 1]] + vertices[triangles[i + 2]]) / 3;
				centlist.Add(centv);
				centlist.Add(centv);
				centlist.Add(centv);
			}
			for(var i = 0; i < triangles.Length; i += 3)
			{
				var v0 = vertices[triangles[i]];
				var v1 = vertices[triangles[i + 1]];
				var v2 = vertices[triangles[i + 2]];
				var d0 = Vector3.Distance(v0, v1);
				var d1 = Vector3.Distance(v1, v2);
				var d2 = Vector3.Distance(v2, v0);
				if(d0 < d1 && d0 < d2)
				{
					var dc0 = Vector3.Distance(v0, vertices[midxarr[triangles[i + 1]]]);
					var dc1 = Vector3.Distance(v1, vertices[midxarr[triangles[i]]]);
					if(dc0 > d0)
					{
						midxarr[triangles[i]] = triangles[i + 1];
					}
					if(dc1 > d0)
					{
						midxarr[triangles[i + 1]] = triangles[i];
					}
				}
				else if(d1 < d0 && d1 < d2)
				{
					var dc2 = Vector3.Distance(v1, vertices[midxarr[triangles[i + 2]]]);
					var dc3 = Vector3.Distance(v2, vertices[midxarr[triangles[i + 1]]]);
					if(dc2 > d1)
					{
						midxarr[triangles[i + 1]] = triangles[i + 2];
					}
					if(dc3 > d1)
					{
						midxarr[triangles[i + 2]] = triangles[i + 1];
					}
				}
				else if(d2 < d0 && d2 < d1)
				{
					var dc4 = Vector3.Distance(v2, vertices[midxarr[triangles[i]]]);
					var dc5 = Vector3.Distance(v0, vertices[midxarr[triangles[i + 2]]]);
					if(dc4 > d2)
					{
						midxarr[triangles[i + 2]] = triangles[i];
					}
					if(dc5 > d2)
					{
						midxarr[triangles[i]] = triangles[i + 2];
					}
				}
			}

			for(var i = 0; i < vertices.Length; i++)
			{
				var vpos = vertices[midxarr[i]];
				var vc = vertices[i];
				var flag0 = false;
				if(smode == SymmetryMode.X_axis && vpos.x == 0 && vc.x != 0)
				{
					flag0 = true;
				}
				else if(smode == SymmetryMode.X_axis && vpos.x == 0 && vc.x == 0)
				{
					flag0 = true;
				}
				else if(smode == SymmetryMode.Y_axis && vpos.y == 0 && vc.y != 0)
				{
					flag0 = true;
				}
				else if(smode == SymmetryMode.Z_axis && vpos.z == 0 && vc.z != 0)
				{
					flag0 = true;
				}
				var dist = Vector3.Distance(vc, vpos);
				if(flag0 && dist < avgPointDist * 1.0f)
				{
					vertices[midxarr[i]] = vertices[i];
				}
			}

			var nv1 = vertices;
			var newvlist = nv1.Distinct().ToList();
			var newvdict = new Dictionary<Vector3, int>();
			for(var i = 0; i < newvlist.Count; i++)
			{
				newvdict[newvlist[i]] = i;
			}
			for(var i = 0; i < triangles.Length; i++)
			{
				int j;
				newvdict.TryGetValue(vertices[triangles[i]], out j);
				triangles[i] = j;
			}
			mesh.Clear();
			mesh.vertices = newvlist.ToArray();
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint; i++)
			{
				mesh.SetTriangles(triangles, i);
			}
			mesh.RecalculateBounds();

			//mesh.RecalculateNormals();
			CalcMeshNormals(mesh);
		}

		private static void RemeshPolyWithoutHoles(Mesh mesh)
		{
			var vertices = mesh.vertices;

			//int[] triangles = mesh.GetTriangles(0);
			var subcnt = IsMonoSubmesh ? 1 : mesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}
			var colors = mesh.colors;
			if(colors.Length < 1)
			{
				colors = Enumerable.Repeat(Color.white, vertices.Length).ToArray();
			}
			var uv0s = mesh.uv;
			var uv2s = mesh.uv2;
			var uv3s = mesh.uv3;
			var uv4s = mesh.uv4;
			if(uv0s.Length < 1)
			{
				uv0s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv2s.Length < 1)
			{
				uv2s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv3s.Length < 1)
			{
				uv3s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv4s.Length < 1)
			{
				uv4s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			var binds = mesh.bindposes;
			var weight1s = mesh.GetAllBoneWeights().ToArray();
			var pervers = mesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var pcnt = 0;
			if(IsWeight)
			{
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			if(IsBrushedArray.Length != vertices.Length)
			{
				IsBrushedArray = Enumerable.Repeat(false, vertices.Length).ToArray();
			}
			var SubdHash = new HashSet<Vector3>();
			var subdlist = new List<Vector3>(vertices);
			var uv0list = new List<Vector2>(uv0s);
			var uv2list = new List<Vector2>(uv2s);
			var uv3list = new List<Vector2>(uv3s);
			var uv4list = new List<Vector2>(uv4s);
			var colorlist = new List<Color>(colors);
			var weight1list = new List<BoneWeight1>(weight1s);
			var perverlist = new List<byte>(pervers);
			var w1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
			var isbrushedlist = new List<bool>(IsBrushedArray);
			var dc = avgPointDist * retopoWeight * 5.0f;
			for(var h = 0; h < subcnt; h++)
			{
				var triarr = triarrlist[h];
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					var pv0 = IsWeight ? pervers[triarr[i]] : 0;
					var pv1 = IsWeight ? pervers[triarr[i + 1]] : 0;
					var pv2 = IsWeight ? pervers[triarr[i + 2]] : 0;
					var bwlist0 = new List<BoneWeight1>();
					if(pv0 > 0)
					{
						for(var j = 0; j < pv0; j++)
						{
							bwlist0.Add(weight1s[boneidxs[triarr[i]] + j]);
						}
					}
					else
					{
						bwlist0.Add(w1);
					}
					var bwlist1 = new List<BoneWeight1>();
					if(pv1 > 0)
					{
						for(var j = 0; j < pv1; j++)
						{
							bwlist1.Add(weight1s[boneidxs[triarr[i + 1]] + j]);
						}
					}
					else
					{
						bwlist1.Add(w1);
					}
					var bwlist2 = new List<BoneWeight1>();
					if(pv2 > 0)
					{
						for(var j = 0; j < pv2; j++)
						{
							bwlist2.Add(weight1s[boneidxs[triarr[i + 2]] + j]);
						}
					}
					else
					{
						bwlist2.Add(w1);
					}
					var d0 = Vector3.Distance(v0, v1);
					var d1 = Vector3.Distance(v1, v2);
					var d2 = Vector3.Distance(v2, v0);
					bool flag0 = false, flag1 = false, flag2 = false;
					if(d0 > d1 && d0 > d2)
					{
						var p0 = v2 + d2 / (d2 + d0) * (v1 - v2);
						var dp0 = Vector3.Distance(v0, p0);
						if(d0 > dc || d0 > dp0 * 5.0f)
						{
							flag0 = true;
						}
					}
					else if(d1 > d0 && d1 > d2)
					{
						var p1 = v0 + d0 / (d0 + d1) * (v2 - v0);
						var dp1 = Vector3.Distance(v1, p1);
						if(d1 > dc || d1 > dp1 * 5.0f)
						{
							flag1 = true;
						}
					}
					else if(d2 > d0 && d2 > d1)
					{
						var p2 = v1 + d1 / (d1 + d2) * (v0 - v1);
						var dp2 = Vector3.Distance(v2, p2);
						if(d2 > dc || d2 > dp2 * 5.0f)
						{
							flag2 = true;
						}
					}
					if(uv4s[triarr[i]].x < maskWeight || uv4s[triarr[i + 1]].x < maskWeight || uv4s[triarr[i + 2]].x < maskWeight)
					{
						flag0 = false;
						flag1 = false;
						flag2 = false;
					}
					var e0 = v0 * 0.5f + v1 * 0.5f;
					var e1 = v1 * 0.5f + v2 * 0.5f;
					var e2 = v2 * 0.5f + v0 * 0.5f;
					if(flag0)
					{
						if(!SubdHash.Contains(e0))
						{
							SubdHash.Add(e0);
							subdlist.Add(e0);
							uv0list.Add(uv0s[triarr[i]] * 0.5f + uv0s[triarr[i + 1]] * 0.5f);
							uv2list.Add(uv2s[triarr[i]] * 0.5f + uv2s[triarr[i + 1]] * 0.5f);
							uv3list.Add(uv3s[triarr[i]] * 0.5f + uv3s[triarr[i + 1]] * 0.5f);
							uv4list.Add(uv4s[triarr[i]] * 0.5f + uv4s[triarr[i + 1]] * 0.5f);
							colorlist.Add(colors[triarr[i]] * 0.5f + colors[triarr[i + 1]] * 0.5f);
							if(IsWeight)
							{
								var bwliste0 = BoneWeight1Lerp(bwlist0, bwlist1);
								weight1list.AddRange(bwliste0);
								perverlist.Add((byte)bwliste0.Count);
							}
							isbrushedlist.Add(IsBrushedArray[triarr[i]] && IsBrushedArray[triarr[i + 1]]);
						}
					}
					else if(flag1)
					{
						if(!SubdHash.Contains(e1))
						{
							SubdHash.Add(e1);
							subdlist.Add(e1);
							uv0list.Add(uv0s[triarr[i + 1]] * 0.5f + uv0s[triarr[i + 2]] * 0.5f);
							uv2list.Add(uv2s[triarr[i + 1]] * 0.5f + uv2s[triarr[i + 2]] * 0.5f);
							uv3list.Add(uv3s[triarr[i + 1]] * 0.5f + uv3s[triarr[i + 2]] * 0.5f);
							uv4list.Add(uv4s[triarr[i + 1]] * 0.5f + uv4s[triarr[i + 2]] * 0.5f);
							colorlist.Add(colors[triarr[i + 1]] * 0.5f + colors[triarr[i + 2]] * 0.5f);
							if(IsWeight)
							{
								var bwliste1 = BoneWeight1Lerp(bwlist1, bwlist2);
								weight1list.AddRange(bwliste1);
								perverlist.Add((byte)bwliste1.Count);
							}
							isbrushedlist.Add(IsBrushedArray[triarr[i + 1]] && IsBrushedArray[triarr[i + 2]]);
						}
					}
					else if(flag2)
					{
						if(!SubdHash.Contains(e2))
						{
							SubdHash.Add(e2);
							subdlist.Add(e2);
							uv0list.Add(uv0s[triarr[i + 2]] * 0.5f + uv0s[triarr[i]] * 0.5f);
							uv2list.Add(uv2s[triarr[i + 2]] * 0.5f + uv2s[triarr[i]] * 0.5f);
							uv3list.Add(uv3s[triarr[i + 2]] * 0.5f + uv3s[triarr[i]] * 0.5f);
							uv4list.Add(uv4s[triarr[i + 2]] * 0.5f + uv4s[triarr[i]] * 0.5f);
							colorlist.Add(colors[triarr[i + 2]] * 0.5f + colors[triarr[i]] * 0.5f);
							if(IsWeight)
							{
								var bwliste2 = BoneWeight1Lerp(bwlist2, bwlist0);
								weight1list.AddRange(bwliste2);
								perverlist.Add((byte)bwliste2.Count);
							}
							isbrushedlist.Add(IsBrushedArray[triarr[i + 2]] && IsBrushedArray[triarr[i]]);
						}
					}
				}
			}
			var SubdArr = SubdHash.ToArray();
			var NewVDict = Enumerable.Range(0, SubdArr.Length).ToDictionary(i => SubdArr[i], i => i + vertices.Length);
			var newtrilistlist = new List<List<int>>();
			for(var h = 0; h < subcnt; h++)
			{
				var triarr = triarrlist[h];
				var newtrilist = triarr.ToList();
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					var e0 = v0 * 0.5f + v1 * 0.5f;
					var e1 = v1 * 0.5f + v2 * 0.5f;
					var e2 = v2 * 0.5f + v0 * 0.5f;
					int cnt0 = 0, cnt1 = 0, cnt2 = 0;
					var flag0 = NewVDict.TryGetValue(e0, out cnt0);
					var flag1 = NewVDict.TryGetValue(e1, out cnt1);
					var flag2 = NewVDict.TryGetValue(e2, out cnt2);
					if(flag0 && !flag1 && !flag2)
					{
						newtrilist[i] = triarr[i];
						newtrilist[i + 1] = cnt0;
						newtrilist[i + 2] = triarr[i + 2];
						newtrilist.AddRange(new[] { triarr[i + 2], cnt0, triarr[i + 1] });
					}
					else if(!flag0 && flag1 && !flag2)
					{
						newtrilist[i] = triarr[i];
						newtrilist[i + 1] = triarr[i + 1];
						newtrilist[i + 2] = cnt1;
						newtrilist.AddRange(new[] { triarr[i], cnt1, triarr[i + 2] });
					}
					else if(!flag0 && !flag1 && flag2)
					{
						newtrilist[i] = triarr[i + 1];
						newtrilist[i + 1] = triarr[i + 2];
						newtrilist[i + 2] = cnt2;
						newtrilist.AddRange(new[] { triarr[i + 1], cnt2, triarr[i] });
					}
					else if(flag0 && flag1 && !flag2)
					{
						newtrilist[i] = triarr[i];
						newtrilist[i + 1] = cnt0;
						newtrilist[i + 2] = triarr[i + 2];
						newtrilist.AddRange(new[] { cnt0, triarr[i + 1], cnt1 });
						newtrilist.AddRange(new[] { cnt0, cnt1, triarr[i + 2] });
					}
					else if(!flag0 && flag1 && flag2)
					{
						newtrilist[i] = triarr[i];
						newtrilist[i + 1] = triarr[i + 1];
						newtrilist[i + 2] = cnt1;
						newtrilist.AddRange(new[] { triarr[i], cnt1, cnt2 });
						newtrilist.AddRange(new[] { cnt1, triarr[i + 2], cnt2 });
					}
					else if(flag0 && !flag1 && flag2)
					{
						newtrilist[i] = triarr[i];
						newtrilist[i + 1] = cnt0;
						newtrilist[i + 2] = cnt2;
						newtrilist.AddRange(new[] { cnt2, cnt0, triarr[i + 2] });
						newtrilist.AddRange(new[] { cnt0, triarr[i + 1], triarr[i + 2] });
					}
					else if(flag0 && flag1 && flag2)
					{
						newtrilist[i] = triarr[i];
						newtrilist[i + 1] = cnt0;
						newtrilist[i + 2] = cnt2;
						newtrilist.AddRange(new[] { cnt0, cnt1, cnt2 });
						newtrilist.AddRange(new[] { cnt0, triarr[i + 1], cnt1 });
						newtrilist.AddRange(new[] { cnt2, cnt1, triarr[i + 2] });
					}
				}
				newtrilistlist.Add(newtrilist);
			}
			vertices = subdlist.ToArray();
			uv0s = uv0list.ToArray();
			uv2s = uv2list.ToArray();
			uv3s = uv3list.ToArray();
			uv4s = uv4list.ToArray();
			colors = colorlist.ToArray();

			//triangles = newtri.ToArray();
			triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(newtrilistlist[i].ToArray());
			}
			IsBrushedArray = isbrushedlist.ToArray();
			pcnt = 0;
			if(IsWeight)
			{
				weight1s = weight1list.ToArray();
				pervers = perverlist.ToArray();
				boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			var AdjIdxListList = new List<List<int>>();
			for(var i = 0; i < vertices.Length; i++)
			{
				AdjIdxListList.Add(new List<int>());
			}
			for(var h = 0; h < subcnt; h++)
			{
				var triarr = triarrlist[h];
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var i0 = triarr[i];
					var i1 = triarr[i + 1];
					var i2 = triarr[i + 2];
					AdjIdxListList[triarr[i]].AddRange(new[] { i1, i2 });
					AdjIdxListList[triarr[i + 1]].AddRange(new[] { i2, i0 });
					AdjIdxListList[triarr[i + 2]].AddRange(new[] { i0, i1 });
				}
			}
			var dc0 = avgPointDist * retopoWeight * 1.0f;
			weight1list = new List<BoneWeight1>();
			perverlist = new List<byte>();
			for(var i = 0; i < vertices.Length; i++)
			{
				var bonew1list = new List<BoneWeight1>();
				var v0 = vertices[i];
				var AdjIdxList = AdjIdxListList[i];
				foreach(var t in AdjIdxList)
				{
					var vpos = vertices[t];
					var d0 = Vector3.Distance(v0, vpos);
					if(d0 < dc0 && t != 0)
					{
						vertices[t] = v0;
					}
				}
				if(IsWeight && bonew1list.Count < 1)
				{
					int pi0 = pervers[i];
					var bwlist0 = new List<BoneWeight1>();
					if(pi0 > 0)
					{
						for(var j = 0; j < pi0; j++)
						{
							try
							{
								bwlist0.Add(weight1s[boneidxs[i] + j]);
							}
							catch
							{
								bwlist0.Add(w1);
							}
						}
					}
					if(bwlist0.Count < 1)
					{
						bwlist0.Add(w1);
					}
					bonew1list.AddRange(bwlist0);
				}
				if(IsWeight)
				{
					weight1list.AddRange(bonew1list);
					perverlist.Add((byte)bonew1list.Count);
				}
			}
			if(IsWeight)
			{
				weight1s = weight1list.ToArray();
				pervers = perverlist.ToArray();
				boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
				pcnt = 0;
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			if(AutoCloseHole)
			{
				var AdjVSet = new HashSet<Vector3>();
				for(var h = 0; h < subcnt; h++)
				{
					var triarr = triarrlist[h];
					for(var i = 0; i < triarr.Length; i += 3)
					{
						var v0 = vertices[triarr[i]];
						var v1 = vertices[triarr[i + 1]];
						var v2 = vertices[triarr[i + 2]];
						if(v0 == v1 || v1 == v2 || v2 == v0)
						{
							continue;
						}
						var q0 = v0 * 0.5f + v1 * 0.5f;
						var q1 = v1 * 0.5f + v2 * 0.5f;
						var q2 = v2 * 0.5f + v0 * 0.5f;
						if(AdjVSet.Contains(q0))
						{
							AdjVSet.Remove(q0);
						}
						else
						{
							AdjVSet.Add(q0);
						}
						if(AdjVSet.Contains(q1))
						{
							AdjVSet.Remove(q1);
						}
						else
						{
							AdjVSet.Add(q1);
						}
						if(AdjVSet.Contains(q2))
						{
							AdjVSet.Remove(q2);
						}
						else
						{
							AdjVSet.Add(q2);
						}
					}
				}
				var edtrilist = new List<int>();
				var HoleSet = new HashSet<Vector3>();
				for(var h = 0; h < subcnt; h++)
				{
					var triarr = triarrlist[h];
					for(var i = 0; i < triarr.Length; i += 3)
					{
						var v0 = vertices[triarr[i]];
						var v1 = vertices[triarr[i + 1]];
						var v2 = vertices[triarr[i + 2]];
						var q0 = v0 * 0.5f + v1 * 0.5f;
						var q1 = v1 * 0.5f + v2 * 0.5f;
						var q2 = v2 * 0.5f + v0 * 0.5f;
						if(AdjVSet.Contains(q0))
						{
							edtrilist.Add(triarr[i]);
							edtrilist.Add(triarr[i + 1]);
							HoleSet.Add(q0);
						}
						else if(AdjVSet.Contains(q1))
						{
							edtrilist.Add(triarr[i + 1]);
							edtrilist.Add(triarr[i + 2]);
							HoleSet.Add(q1);
						}
						else if(AdjVSet.Contains(q2))
						{
							edtrilist.Add(triarr[i + 2]);
							edtrilist.Add(triarr[i]);
							HoleSet.Add(q2);
						}
					}
				}

				//newtri = new List<int>(triangles);
				for(var i = 0; i < edtrilist.Count; i += 2)
				{
					var i0 = edtrilist[i];
					var i1 = edtrilist[i + 1];
					int idx0 = 0, idx1 = 0;
					var e0 = vertices[i0];
					var e1 = vertices[i1];
					var AdjIdxList0 = AdjIdxListList[i0];
					var AdjIdxList1 = AdjIdxListList[i1];
					foreach(var idx in AdjIdxList0)
					{
						if(idx == i1)
						{
							continue;
						}
						var e2 = vertices[idx];
						var q0 = e0 * 0.5f + e2 * 0.5f;
						if(HoleSet.Contains(q0))
						{
							idx0 = idx;
							break;
						}
					}
					if(idx0 > 0)
					{
						var vc0 = vertices[i0] * 0.333f + vertices[i1] * 0.333f + vertices[idx0] * 0.333f;
						vertices[i0] = vc0;
						vertices[i1] = vc0;
						vertices[idx0] = vc0;
						var cc0 = colors[i0] * 0.333f + colors[i1] * 0.333f + colors[idx0] * 0.333f;
						colors[i0] = cc0;
						colors[i1] = cc0;
						colors[idx0] = cc0;
						var uvc0_0 = uv0s[i0] * 0.333f + uv0s[i1] * 0.333f + uv0s[idx0] * 0.333f;
						uv0s[i0] = uvc0_0;
						uv0s[i1] = uvc0_0;
						uv0s[idx0] = uvc0_0;
						var uvc2_0 = uv2s[i0] * 0.333f + uv2s[i1] * 0.333f + uv2s[idx0] * 0.333f;
						uv2s[i0] = uvc2_0;
						uv2s[i1] = uvc2_0;
						uv2s[idx0] = uvc2_0;
						var uvc3_0 = uv3s[i0] * 0.333f + uv3s[i1] * 0.333f + uv3s[idx0] * 0.333f;
						uv3s[i0] = uvc3_0;
						uv3s[i1] = uvc3_0;
						uv3s[idx0] = uvc3_0;
						var uvc4_0 = uv4s[i0] * 0.333f + uv4s[i1] * 0.333f + uv4s[idx0] * 0.333f;
						uv4s[i0] = uvc4_0;
						uv4s[i1] = uvc4_0;
						uv4s[idx0] = uvc4_0;
					}
					foreach(var idx in AdjIdxList1)
					{
						if(idx == i0)
						{
							continue;
						}
						var e3 = vertices[idx];
						var q1 = e1 * 0.5f + e3 * 0.5f;
						if(HoleSet.Contains(q1))
						{
							idx1 = idx;
							break;
						}
					}
					if(idx1 > 0)
					{
						var vc0 = vertices[i0] * 0.333f + vertices[i1] * 0.333f + vertices[idx1] * 0.333f;
						vertices[i0] = vc0;
						vertices[i1] = vc0;
						vertices[idx1] = vc0;
						var cc0 = colors[i0] * 0.333f + colors[i1] * 0.333f + colors[idx1] * 0.333f;
						colors[i0] = cc0;
						colors[i1] = cc0;
						colors[idx1] = cc0;
						var uvc0_0 = uv0s[i0] * 0.333f + uv0s[i1] * 0.333f + uv0s[idx1] * 0.333f;
						uv0s[i0] = uvc0_0;
						uv0s[i1] = uvc0_0;
						uv0s[idx1] = uvc0_0;
						var uvc2_0 = uv2s[i0] * 0.333f + uv2s[i1] * 0.333f + uv2s[idx1] * 0.333f;
						uv2s[i0] = uvc2_0;
						uv2s[i1] = uvc2_0;
						uv2s[idx1] = uvc2_0;
						var uvc3_0 = uv3s[i0] * 0.333f + uv3s[i1] * 0.333f + uv3s[idx1] * 0.333f;
						uv3s[i0] = uvc3_0;
						uv3s[i1] = uvc3_0;
						uv3s[idx1] = uvc3_0;
						var uvc4_0 = uv4s[i0] * 0.333f + uv4s[i1] * 0.333f + uv4s[idx1] * 0.333f;
						uv4s[i0] = uvc4_0;
						uv4s[i1] = uvc4_0;
						uv4s[idx1] = uvc4_0;
					}
				}
			}
			newtrilistlist = new List<List<int>>();
			for(var h = 0; h < subcnt; h++)
			{
				var newtrilist = new List<int>();
				var triarr = triarrlist[h];
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					if(v0 == v1 || v1 == v2 || v2 == v0)
					{
						continue;
					}
					newtrilist.AddRange(new[] { triarr[i], triarr[i + 1], triarr[i + 2] });
				}
				newtrilistlist.Add(newtrilist);
			}
			triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(newtrilistlist[i].ToArray());
			}
			var newvdict = new Dictionary<Vector3, int>();
			var newvlist = new List<Vector3>();
			var newcolorlist = new List<Color>();
			var newuv0list = new List<Vector2>();
			var newuv2list = new List<Vector2>();
			var newuv3list = new List<Vector2>();
			var newuv4list = new List<Vector2>();
			var newperverlist = new List<byte>();
			var newbwlist = new List<BoneWeight1>();
			var calcarr = new int[vertices.Length];
			for(var i = 0; i < vertices.Length; i++)
			{
				var j = 0;
				if(newvdict.TryGetValue(vertices[i], out j))
				{
					calcarr[i] = j;
				}
				else
				{
					var idx0 = newvdict.Count;
					newvdict[vertices[i]] = idx0;
					calcarr[i] = idx0;
					newvlist.Add(vertices[i]);
					newcolorlist.Add(colors[i]);
					newuv0list.Add(uv0s[i]);
					newuv2list.Add(uv2s[i]);
					newuv3list.Add(uv3s[i]);
					newuv4list.Add(uv4s[i]);
					if(IsWeight)
					{
						int pi0 = pervers[i];
						var bwlist0 = new List<BoneWeight1>();
						if(pi0 > 0)
						{
							for(var k = 0; k < pi0; k++)
							{
								try
								{
									bwlist0.Add(weight1s[boneidxs[i] + k]);
								}
								catch
								{
									bwlist0.Add(w1);
								}
							}
						}
						if(bwlist0.Count < 1)
						{
							bwlist0.Add(w1);
						}
						newbwlist.AddRange(bwlist0);
						newperverlist.Add((byte)bwlist0.Count);
					}
				}
			}
			var newtriarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				var newtrilist = newtrilistlist[i];
				var triarr = triarrlist[i];
				for(var j = 0; j < newtrilist.Count; j++)
				{
					var t = calcarr[triarr[j]];
					newtrilist[j] = t;
				}
				newtriarrlist.Add(newtrilist.ToArray());
			}
			triarrlist = newtriarrlist;
			vertices = newvlist.ToArray();

			//triangles = newtri.ToArray();
			colors = newcolorlist.ToArray();
			uv0s = newuv0list.ToArray();
			uv2s = newuv2list.ToArray();
			uv3s = newuv3list.ToArray();
			uv4s = newuv4list.ToArray();
			if(IsWeight)
			{
				weight1s = newbwlist.ToArray();
				pervers = newperverlist.ToArray();
				boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
				pcnt = 0;
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			if(AutoFixBlackPoly)
			{
				AdjIdxListList = new List<List<int>>();
				for(var i = 0; i < vertices.Length; i++)
				{
					AdjIdxListList.Add(new List<int>());
				}
				for(var i = 0; i < subcnt; i++)
				{
					var triarr = triarrlist[i];
					for(var j = 0; j < triarr.Length; j += 3)
					{
						var i0 = triarr[j];
						var i1 = triarr[j + 1];
						var i2 = triarr[j + 2];
						AdjIdxListList[triarr[j]].AddRange(new[] { i1, i2 });
						AdjIdxListList[triarr[j + 1]].AddRange(new[] { i2, i0 });
						AdjIdxListList[triarr[j + 2]].AddRange(new[] { i0, i1 });
					}
				}
				var MoveArr = new bool[vertices.Length];
				weight1list = new List<BoneWeight1>();
				perverlist = new List<byte>();
				for(var i = 0; i < vertices.Length; i++)
				{
					var AdjIdxList = AdjIdxListList[i];
					var flag0 = false;
					if(AdjIdxList.Count >= 5 && !MoveArr[i])
					{
						foreach(var t in AdjIdxList)
						{
							if(AdjIdxListList[t].Count < 5)
							{
								if(MoveArr[t])
								{
									continue;
								}

								//vertices[t] = v0;
								vertices[i] = vertices[t];
								MoveArr[t] = true;
								if(IsWeight)
								{
									int pi0 = pervers[i];
									var bwlist0 = new List<BoneWeight1>();
									if(pi0 > 0)
									{
										for(var j = 0; j < pi0; j++)
										{
											try
											{
												bwlist0.Add(weight1s[boneidxs[i] + j]);
											}
											catch
											{
												bwlist0.Add(w1);
											}
										}
									}
									int pi1 = pervers[t];
									var bwlist1 = new List<BoneWeight1>();
									if(pi1 > 0)
									{
										for(var j = 0; j < pi1; j++)
										{
											try
											{
												bwlist1.Add(weight1s[boneidxs[t] + j]);
											}
											catch
											{
												bwlist1.Add(w1);
											}
										}
									}
									var bwlist2 = BoneWeight1Lerp(bwlist0, bwlist1);
									weight1list.AddRange(bwlist2);
									perverlist.Add((byte)bwlist2.Count);
								}
								flag0 = true;
								break;
							}
						}
					}
					if(!flag0 && IsWeight)
					{
						int pi0 = pervers[i];
						var bwlist0 = new List<BoneWeight1>();
						if(pi0 > 0)
						{
							for(var j = 0; j < pi0; j++)
							{
								try
								{
									bwlist0.Add(weight1s[boneidxs[i] + j]);
								}
								catch
								{
									bwlist0.Add(w1);
								}
							}
						}
						if(bwlist0.Count < 1)
						{
							bwlist0.Add(w1);
						}
						weight1list.AddRange(bwlist0);
						perverlist.Add((byte)bwlist0.Count);
					}
				}
				if(IsWeight)
				{
					weight1s = weight1list.ToArray();
					pervers = perverlist.ToArray();
					boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
					pcnt = 0;
					for(var i = 0; i < pervers.Length; i++)
					{
						boneidxs[i] = pcnt;
						pcnt += pervers[i];
					}
					if(boneidxs.Length < 1)
					{
						boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
					}
				}
				newtrilistlist = new List<List<int>>();
				for(var i = 0; i < subcnt; i++)
				{
					var triarr = triarrlist[i];
					var newtrilist = new List<int>();
					for(var j = 0; j < triarr.Length; j += 3)
					{
						var v0 = vertices[triarr[j]];
						var v1 = vertices[triarr[j + 1]];
						var v2 = vertices[triarr[j + 2]];
						if(v0 == v1 || v1 == v2 || v2 == v0)
						{
							continue;
						}
						newtrilist.AddRange(new[] { triarr[j], triarr[j + 1], triarr[j + 2] });
					}
					newtrilistlist.Add(newtrilist);
				}

				//triangles = newtri.ToArray();
				triarrlist = new List<int[]>();
				for(var i = 0; i < subcnt; i++)
				{
					triarrlist.Add(newtrilistlist[i].ToArray());
				}
				newvdict = new Dictionary<Vector3, int>();
				newvlist = new List<Vector3>();
				newcolorlist = new List<Color>();
				newuv0list = new List<Vector2>();
				newuv2list = new List<Vector2>();
				newuv3list = new List<Vector2>();
				newuv4list = new List<Vector2>();
				newperverlist = new List<byte>();
				newbwlist = new List<BoneWeight1>();
				calcarr = new int[vertices.Length];
				for(var i = 0; i < vertices.Length; i++)
				{
					var j = 0;
					if(newvdict.TryGetValue(vertices[i], out j))
					{
						calcarr[i] = j;
					}
					else
					{
						var idx0 = newvdict.Count;
						newvdict[vertices[i]] = idx0;
						calcarr[i] = idx0;
						newvlist.Add(vertices[i]);
						newcolorlist.Add(colors[i]);
						newuv0list.Add(uv0s[i]);
						newuv2list.Add(uv2s[i]);
						newuv3list.Add(uv3s[i]);
						newuv4list.Add(uv4s[i]);
						if(IsWeight)
						{
							int pi0 = pervers[i];
							var bwlist0 = new List<BoneWeight1>();
							if(pi0 > 0)
							{
								for(var k = 0; k < pi0; k++)
								{
									try
									{
										bwlist0.Add(weight1s[boneidxs[i] + k]);
									}
									catch
									{
										bwlist0.Add(w1);
									}
								}
							}
							if(bwlist0.Count < 1)
							{
								bwlist0.Add(w1);
							}
							newbwlist.AddRange(bwlist0);
							newperverlist.Add((byte)bwlist0.Count);
						}
					}
				}
				newtriarrlist = new List<int[]>();
				for(var i = 0; i < subcnt; i++)
				{
					var triarr = triarrlist[i];
					var newtrilist = newtrilistlist[i];
					for(var j = 0; j < newtrilist.Count; j++)
					{
						var t = calcarr[triarr[j]];
						newtrilist[j] = t;
					}
					newtriarrlist.Add(newtrilist.ToArray());
				}
				triarrlist = newtriarrlist;
				vertices = newvlist.ToArray();
				colors = newcolorlist.ToArray();
				uv0s = newuv0list.ToArray();
				uv2s = newuv2list.ToArray();
				uv3s = newuv3list.ToArray();
				uv4s = newuv4list.ToArray();
			}
			mesh.Clear();
			if(vertices.Length > 65000)
			{
				try
				{
					mesh.indexFormat = IndexFormat.UInt32;
				}
				catch
				{
					return;
				}
			}
			else
			{
				mesh.indexFormat = IndexFormat.UInt16;
			}
			mesh.vertices = vertices;
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint + 1; i++)
			{
				if(IsMonoSubmesh)
				{
					mesh.SetTriangles(triarrlist[0], i);
				}
				else
				{
					if(triarrlist.Count > i)
					{
						mesh.SetTriangles(triarrlist[i], i);
					}
					else
					{
						mesh.SetTriangles(triarrlist[0], i);
					}
				}
			}
			mesh.colors = colors;
			mesh.uv = uv0s;
			mesh.uv2 = uv2s;
			mesh.uv3 = uv3s;
			mesh.uv4 = uv4s;
			mesh.bindposes = binds;
			if(IsWeight)
			{
				var ntweight1s = new NativeArray<BoneWeight1>(newbwlist.ToArray(), Allocator.Temp);
				var PerVerts = new NativeArray<byte>(newperverlist.ToArray(), Allocator.Temp);
				mesh.SetBoneWeights(PerVerts, ntweight1s);
			}
			else
			{
				BoneWeight1Create(mesh);
			}
			mesh.RecalculateTangents();
			MergetriGenerate(mesh);
			CalcMeshNormals(mesh);
			mesh.RecalculateBounds();
		}

		private static void RemeshPolyPrev(Mesh mesh)
		{
			var vertices = mesh.vertices;
			var subcnt = IsMonoSubmesh ? 1 : mesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}
			var colors = mesh.colors;
			if(colors.Length < 1)
			{
				colors = Enumerable.Repeat(Color.white, vertices.Length).ToArray();
			}
			var uv0s = mesh.uv;
			var uv2s = mesh.uv2;
			var uv3s = mesh.uv3;
			var uv4s = mesh.uv4;
			if(uv0s.Length < 1)
			{
				uv0s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv2s.Length < 1)
			{
				uv2s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv3s.Length < 1)
			{
				uv3s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv4s.Length < 1)
			{
				uv4s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			var binds = mesh.bindposes;
			var weight1s = mesh.GetAllBoneWeights().ToArray();
			var pervers = mesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var pcnt = 0;
			if(IsWeight)
			{
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			if(IsBrushedArray.Length != vertices.Length)
			{
				IsBrushedArray = Enumerable.Repeat(false, vertices.Length).ToArray();
			}
			if(IsOldVertList && vertices.Length != oldVertList.Count)
			{
				oldVertList = vertices.ToList();
			}
			if(IsOldNormList && mesh.normals.Length != oldNormalList.Count)
			{
				oldNormalList = mesh.normals.ToList();
			}
			var subdlist = new List<Vector3>(vertices);
			var uv0list = new List<Vector2>(uv0s);
			var uv2list = new List<Vector2>(uv2s);
			var uv3list = new List<Vector2>(uv3s);
			var uv4list = new List<Vector2>(uv4s);
			var colorlist = new List<Color>(colors);
			var bonew1list = new List<BoneWeight1>(weight1s);
			var perverlist = new List<byte>(pervers);
			var w1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
			var brushedlist = new List<bool>(IsBrushedArray);
			var dc = avgPointDist * retopoWeight * 5.0f;
			var newtrilist = new List<List<int>>();
			for(var h = 0; h < subcnt; h++)
			{
				var newtri = triarrlist[h].ToList();
				var triangles = triarrlist[h];
				for(var i = 0; i < triangles.Length; i += 3)
				{
					var t0 = triangles[i];
					var t1 = triangles[i + 1];
					var t2 = triangles[i + 2];

					//if (!IsBrushedArray[t0] && !IsBrushedArray[t1] && !IsBrushedArray[t2]) continue;
					var v0 = vertices[t0];
					var v1 = vertices[t1];
					var v2 = vertices[t2];
					var e0 = v0 * 0.5f + v1 * 0.5f;
					var e1 = v1 * 0.5f + v2 * 0.5f;
					var e2 = v2 * 0.5f + v0 * 0.5f;
					var pv0 = IsWeight ? pervers[triangles[i]] : 0;
					var pv1 = IsWeight ? pervers[triangles[i + 1]] : 0;
					var pv2 = IsWeight ? pervers[triangles[i + 2]] : 0;
					var bwlist0 = new List<BoneWeight1>();
					if(pv0 > 0)
					{
						for(var j = 0; j < pv0; j++)
						{
							bwlist0.Add(weight1s[boneidxs[triangles[i]] + j]);
						}
					}
					else
					{
						bwlist0.Add(w1);
					}
					var bwlist1 = new List<BoneWeight1>();
					if(pv1 > 0)
					{
						for(var j = 0; j < pv1; j++)
						{
							bwlist1.Add(weight1s[boneidxs[triangles[i + 1]] + j]);
						}
					}
					else
					{
						bwlist1.Add(w1);
					}
					var bwlist2 = new List<BoneWeight1>();
					if(pv2 > 0)
					{
						for(var j = 0; j < pv2; j++)
						{
							bwlist2.Add(weight1s[boneidxs[triangles[i + 2]] + j]);
						}
					}
					else
					{
						bwlist2.Add(w1);
					}
					var d0 = Vector3.Distance(v0, v1);
					var d1 = Vector3.Distance(v1, v2);
					var d2 = Vector3.Distance(v2, v0);
					bool flag0 = false, flag1 = false, flag2 = false;
					if(d0 > dc)
					{
						flag0 = true;
					}
					if(d1 > dc)
					{
						flag1 = true;
					}
					if(d2 > dc)
					{
						flag2 = true;
					}
					if(uv4s[t0].x < maskWeight || uv4s[t1].x < maskWeight)
					{
						flag0 = false;
					}
					if(uv4s[t1].x < maskWeight || uv4s[t2].x < maskWeight)
					{
						flag1 = false;
					}
					if(uv4s[t2].x < maskWeight || uv4s[t0].x < maskWeight)
					{
						flag2 = false;
					}
					if(flag0 && !flag1 && !flag2)
					{
						var cnt0 = subdlist.Count;
						subdlist.Add(e0);
						uv0list.Add(uv0s[t0] * 0.5f + uv0s[t1] * 0.5f);
						uv2list.Add(uv2s[t0] * 0.5f + uv2s[t1] * 0.5f);
						uv3list.Add(uv3s[t0] * 0.5f + uv3s[t1] * 0.5f);

						//uv4list.Add(uv4s[t0] * 0.5f + uv4s[t1] * 0.5f);
						uv4list.Add(new Vector2(1.0f, uv4s[t0].y * 0.5f + uv4s[t1].y * 0.5f));
						colorlist.Add(colors[t0] * 0.5f + colors[t1] * 0.5f);
						if(IsWeight)
						{
							var bwliste0 = BoneWeight1Lerp(bwlist0, bwlist1);
							bonew1list.AddRange(bwliste0);
							perverlist.Add((byte)bwliste0.Count);
						}
						brushedlist.Add(IsBrushedArray[t0] && IsBrushedArray[t1]);
						newtri[i] = t0;
						newtri[i + 1] = cnt0;
						newtri[i + 2] = t2;
						newtri.AddRange(new[] { cnt0, t1, t2 });
						if(IsOldVertList)
						{
							oldVertList.Add(oldVertList[t0] * 0.5f + oldVertList[t1] * 0.5f);
						}
						if(IsOldNormList)
						{
							oldNormalList.Add(oldNormalList[t0] * 0.5f + oldNormalList[t1] * 0.5f);
						}
					}
					else if(!flag0 && flag1 && !flag2)
					{
						var cnt1 = subdlist.Count;
						subdlist.Add(e1);
						uv0list.Add(uv0s[t1] * 0.5f + uv0s[t2] * 0.5f);
						uv2list.Add(uv2s[t1] * 0.5f + uv2s[t2] * 0.5f);
						uv3list.Add(uv3s[t1] * 0.5f + uv3s[t2] * 0.5f);

						//uv4list.Add(uv4s[t1] * 0.5f + uv4s[t2] * 0.5f);
						uv4list.Add(new Vector2(1.0f, uv4s[t1].y * 0.5f + uv4s[t2].y * 0.5f));
						colorlist.Add(colors[t1] * 0.5f + colors[t2] * 0.5f);
						if(IsWeight)
						{
							var bwliste1 = BoneWeight1Lerp(bwlist1, bwlist2);
							bonew1list.AddRange(bwliste1);
							perverlist.Add((byte)bwliste1.Count);
						}
						brushedlist.Add(IsBrushedArray[t1] && IsBrushedArray[t2]);
						newtri[i] = t0;
						newtri[i + 1] = t1;
						newtri[i + 2] = cnt1;
						newtri.AddRange(new[] { t0, cnt1, t2 });
						if(IsOldVertList)
						{
							oldVertList.Add(oldVertList[t1] * 0.5f + oldVertList[t2] * 0.5f);
						}
						if(IsOldNormList)
						{
							oldNormalList.Add(oldNormalList[t1] * 0.5f + oldNormalList[t2] * 0.5f);
						}
					}
					else if(!flag0 && !flag1 && flag2)
					{
						var cnt2 = subdlist.Count;
						subdlist.Add(e2);
						uv0list.Add(uv0s[t2] * 0.5f + uv0s[t0] * 0.5f);
						uv2list.Add(uv2s[t2] * 0.5f + uv2s[t0] * 0.5f);
						uv3list.Add(uv3s[t2] * 0.5f + uv3s[t0] * 0.5f);

						//uv4list.Add(uv4s[t2] * 0.5f + uv4s[t0] * 0.5f);
						uv4list.Add(new Vector2(1.0f, uv4s[t2].y * 0.5f + uv4s[t0].y * 0.5f));
						colorlist.Add(colors[t2] * 0.5f + colors[t0] * 0.5f);
						if(IsWeight)
						{
							var bwliste2 = BoneWeight1Lerp(bwlist2, bwlist0);
							bonew1list.AddRange(bwliste2);
							perverlist.Add((byte)bwliste2.Count);
						}
						brushedlist.Add(IsBrushedArray[t2] && IsBrushedArray[t0]);
						newtri[i] = t0;
						newtri[i + 1] = t1;
						newtri[i + 2] = cnt2;
						newtri.AddRange(new[] { cnt2, t1, t2 });
						if(IsOldVertList)
						{
							oldVertList.Add(oldVertList[t2] * 0.5f + oldVertList[t0] * 0.5f);
						}
						if(IsOldNormList)
						{
							oldNormalList.Add(oldNormalList[t2] * 0.5f + oldNormalList[t0] * 0.5f);
						}
					}
					else if(flag0 && flag1 && !flag2)
					{
						var cnt0 = subdlist.Count;
						subdlist.Add(e0);
						uv0list.Add(uv0s[t0] * 0.5f + uv0s[t1] * 0.5f);
						uv2list.Add(uv2s[t0] * 0.5f + uv2s[t1] * 0.5f);
						uv3list.Add(uv3s[t0] * 0.5f + uv3s[t1] * 0.5f);

						//uv4list.Add(uv4s[t0] * 0.5f + uv4s[t1] * 0.5f);
						uv4list.Add(new Vector2(1.0f, uv4s[t0].y * 0.5f + uv4s[t0].y * 0.5f));
						colorlist.Add(colors[t0] * 0.5f + colors[t1] * 0.5f);
						brushedlist.Add(IsBrushedArray[t0] && IsBrushedArray[t1]);
						var cnt1 = subdlist.Count;
						subdlist.Add(e1);
						uv0list.Add(uv0s[t1] * 0.5f + uv0s[t2] * 0.5f);
						uv2list.Add(uv2s[t1] * 0.5f + uv2s[t2] * 0.5f);
						uv3list.Add(uv3s[t1] * 0.5f + uv3s[t2] * 0.5f);

						//uv4list.Add(uv4s[t1] * 0.5f + uv4s[t2] * 0.5f);
						uv4list.Add(new Vector2(1.0f, uv4s[t1].y * 0.5f + uv4s[t2].y * 0.5f));
						colorlist.Add(colors[t1] * 0.5f + colors[t2] * 0.5f);
						if(IsWeight)
						{
							var bwliste0 = BoneWeight1Lerp(bwlist0, bwlist1);
							bonew1list.AddRange(bwliste0);
							perverlist.Add((byte)bwliste0.Count);
							var bwliste1 = BoneWeight1Lerp(bwlist1, bwlist2);
							bonew1list.AddRange(bwliste1);
							perverlist.Add((byte)bwliste1.Count);
						}
						brushedlist.Add(IsBrushedArray[t1] && IsBrushedArray[t2]);
						newtri[i] = t0;
						newtri[i + 1] = cnt0;
						newtri[i + 2] = t2;
						newtri.AddRange(new[] { cnt0, cnt1, t2 });
						newtri.AddRange(new[] { cnt0, t1, cnt1 });
						if(IsOldVertList)
						{
							oldVertList.Add(oldVertList[t0] * 0.5f + oldVertList[t1] * 0.5f);
							oldVertList.Add(oldVertList[t1] * 0.5f + oldVertList[t2] * 0.5f);
						}
						if(IsOldNormList)
						{
							oldNormalList.Add(oldNormalList[t0] * 0.5f + oldNormalList[t1] * 0.5f);
							oldNormalList.Add(oldNormalList[t1] * 0.5f + oldNormalList[t2] * 0.5f);
						}
					}
					else if(!flag0 && flag1 && flag2)
					{
						var cnt1 = subdlist.Count;
						subdlist.Add(e1);
						uv0list.Add(uv0s[t1] * 0.5f + uv0s[t2] * 0.5f);
						uv2list.Add(uv2s[t1] * 0.5f + uv2s[t2] * 0.5f);
						uv3list.Add(uv3s[t1] * 0.5f + uv3s[t2] * 0.5f);

						//uv4list.Add(uv4s[t1] * 0.5f + uv4s[t2] * 0.5f);
						uv4list.Add(new Vector2(1.0f, uv4s[t1].y * 0.5f + uv4s[t2].y * 0.5f));
						colorlist.Add(colors[t1] * 0.5f + colors[t2] * 0.5f);
						brushedlist.Add(IsBrushedArray[t1] && IsBrushedArray[t2]);
						var cnt2 = subdlist.Count;
						subdlist.Add(e2);
						uv0list.Add(uv0s[t2] * 0.5f + uv0s[t0] * 0.5f);
						uv2list.Add(uv2s[t2] * 0.5f + uv2s[t0] * 0.5f);
						uv3list.Add(uv3s[t2] * 0.5f + uv3s[t0] * 0.5f);

						//uv4list.Add(uv4s[t2] * 0.5f + uv4s[t0] * 0.5f);
						uv4list.Add(new Vector2(1.0f, uv4s[t2].y * 0.5f + uv4s[t0].y * 0.5f));
						colorlist.Add(colors[t2] * 0.5f + colors[t0] * 0.5f);
						if(IsWeight)
						{
							var bwliste1 = BoneWeight1Lerp(bwlist1, bwlist2);
							bonew1list.AddRange(bwliste1);
							perverlist.Add((byte)bwliste1.Count);
							var bwliste2 = BoneWeight1Lerp(bwlist2, bwlist0);
							bonew1list.AddRange(bwliste2);
							perverlist.Add((byte)bwliste2.Count);
						}
						brushedlist.Add(IsBrushedArray[t2] && IsBrushedArray[t0]);
						newtri[i] = t0;
						newtri[i + 1] = t1;
						newtri[i + 2] = cnt1;
						newtri.AddRange(new[] { t0, cnt1, cnt2 });
						newtri.AddRange(new[] { cnt2, cnt1, t2 });
						if(IsOldVertList)
						{
							oldVertList.Add(oldVertList[t1] * 0.5f + oldVertList[t2] * 0.5f);
							oldVertList.Add(oldVertList[t2] * 0.5f + oldVertList[t0] * 0.5f);
						}
						if(IsOldNormList)
						{
							oldNormalList.Add(oldNormalList[t1] * 0.5f + oldNormalList[t2] * 0.5f);
							oldNormalList.Add(oldNormalList[t2] * 0.5f + oldNormalList[t0] * 0.5f);
						}
					}
					else if(flag0 && !flag1 && flag2)
					{
						var cnt0 = subdlist.Count;
						subdlist.Add(e0);
						uv0list.Add(uv0s[t0] * 0.5f + uv0s[t1] * 0.5f);
						uv2list.Add(uv2s[t0] * 0.5f + uv2s[t1] * 0.5f);
						uv3list.Add(uv3s[t0] * 0.5f + uv3s[t1] * 0.5f);

						//uv4list.Add(uv4s[t0] * 0.5f + uv4s[t1] * 0.5f);
						uv4list.Add(new Vector2(1.0f, uv4s[t0].y * 0.5f + uv4s[t1].y * 0.5f));
						colorlist.Add(colors[t0] * 0.5f + colors[t1] * 0.5f);
						brushedlist.Add(IsBrushedArray[t0] && IsBrushedArray[t1]);
						var cnt2 = subdlist.Count;
						subdlist.Add(e2);
						uv0list.Add(uv0s[t2] * 0.5f + uv0s[t0] * 0.5f);
						uv2list.Add(uv2s[t2] * 0.5f + uv2s[t0] * 0.5f);
						uv3list.Add(uv3s[t2] * 0.5f + uv3s[t0] * 0.5f);

						//uv4list.Add(uv4s[t2] * 0.5f + uv4s[t0] * 0.5f);
						uv4list.Add(new Vector2(1.0f, uv4s[t2].y * 0.5f + uv4s[t0].y * 0.5f));
						colorlist.Add(colors[t2] * 0.5f + colors[t0] * 0.5f);
						if(IsWeight)
						{
							var bwliste0 = BoneWeight1Lerp(bwlist0, bwlist1);
							bonew1list.AddRange(bwliste0);
							perverlist.Add((byte)bwliste0.Count);
							var bwliste2 = BoneWeight1Lerp(bwlist2, bwlist0);
							bonew1list.AddRange(bwliste2);
							perverlist.Add((byte)bwliste2.Count);
						}
						brushedlist.Add(IsBrushedArray[t2] && IsBrushedArray[t0]);
						newtri[i] = t0;
						newtri[i + 1] = cnt0;
						newtri[i + 2] = cnt2;
						newtri.AddRange(new[] { cnt2, cnt0, t2 });
						newtri.AddRange(new[] { cnt0, t1, t2 });
						if(IsOldVertList)
						{
							oldVertList.Add(oldVertList[t0] * 0.5f + oldVertList[t1] * 0.5f);
							oldVertList.Add(oldVertList[t2] * 0.5f + oldVertList[t0] * 0.5f);
						}
						if(IsOldNormList)
						{
							oldNormalList.Add(oldNormalList[t0] * 0.5f + oldNormalList[t1] * 0.5f);
							oldNormalList.Add(oldNormalList[t2] * 0.5f + oldNormalList[t0] * 0.5f);
						}
					}
					else if(flag0 && flag1 && flag2)
					{
						var cnt0 = subdlist.Count;
						subdlist.Add(e0);
						uv0list.Add(uv0s[t0] * 0.5f + uv0s[t1] * 0.5f);
						uv2list.Add(uv2s[t0] * 0.5f + uv2s[t1] * 0.5f);
						uv3list.Add(uv3s[t0] * 0.5f + uv3s[t1] * 0.5f);

						//uv4list.Add(uv4s[t0] * 0.5f + uv4s[t1] * 0.5f);
						uv4list.Add(new Vector2(1.0f, uv4s[t0].y * 0.5f + uv4s[t1].y * 0.5f));
						colorlist.Add(colors[t0] * 0.5f + colors[t1] * 0.5f);
						brushedlist.Add(IsBrushedArray[t0] && IsBrushedArray[t1]);
						var cnt1 = subdlist.Count;
						subdlist.Add(e1);
						uv0list.Add(uv0s[t1] * 0.5f + uv0s[t2] * 0.5f);
						uv2list.Add(uv2s[t1] * 0.5f + uv2s[t2] * 0.5f);
						uv3list.Add(uv3s[t1] * 0.5f + uv3s[t2] * 0.5f);

						//uv4list.Add(uv4s[t1] * 0.5f + uv4s[t2] * 0.5f);
						uv4list.Add(new Vector2(1.0f, uv4s[t1].y * 0.5f + uv4s[t2].y * 0.5f));
						colorlist.Add(colors[t1] * 0.5f + colors[t2] * 0.5f);
						brushedlist.Add(IsBrushedArray[t1] && IsBrushedArray[t2]);
						var cnt2 = subdlist.Count;
						subdlist.Add(e2);
						uv0list.Add(uv0s[t2] * 0.5f + uv0s[t0] * 0.5f);
						uv2list.Add(uv2s[t2] * 0.5f + uv2s[t0] * 0.5f);
						uv3list.Add(uv3s[t2] * 0.5f + uv3s[t0] * 0.5f);

						//uv4list.Add(uv4s[t2] * 0.5f + uv4s[t0] * 0.5f);
						uv4list.Add(new Vector2(1.0f, uv4s[t2].y * 0.5f + uv4s[t0].y * 0.5f));
						colorlist.Add(colors[t2] * 0.5f + colors[t0] * 0.5f);
						if(IsWeight)
						{
							var bwliste0 = BoneWeight1Lerp(bwlist0, bwlist1);
							bonew1list.AddRange(bwliste0);
							perverlist.Add((byte)bwliste0.Count);
							var bwliste1 = BoneWeight1Lerp(bwlist1, bwlist2);
							bonew1list.AddRange(bwliste1);
							perverlist.Add((byte)bwliste1.Count);
							var bwliste2 = BoneWeight1Lerp(bwlist2, bwlist0);
							bonew1list.AddRange(bwliste2);
							perverlist.Add((byte)bwliste2.Count);
						}
						brushedlist.Add(IsBrushedArray[t2] && IsBrushedArray[t0]);
						newtri[i] = t0;
						newtri[i + 1] = cnt0;
						newtri[i + 2] = cnt2;
						newtri.AddRange(new[] { cnt0, cnt1, cnt2 });
						newtri.AddRange(new[] { cnt0, t1, cnt1 });
						newtri.AddRange(new[] { cnt2, cnt1, t2 });
						if(IsOldVertList)
						{
							oldVertList.Add(oldVertList[t0] * 0.5f + oldVertList[t1] * 0.5f);
							oldVertList.Add(oldVertList[t1] * 0.5f + oldVertList[t2] * 0.5f);
							oldVertList.Add(oldVertList[t2] * 0.5f + oldVertList[t0] * 0.5f);
						}
						if(IsOldNormList)
						{
							oldNormalList.Add(oldNormalList[t0] * 0.5f + oldNormalList[t1] * 0.5f);
							oldNormalList.Add(oldNormalList[t1] * 0.5f + oldNormalList[t2] * 0.5f);
							oldNormalList.Add(oldNormalList[t2] * 0.5f + oldNormalList[t0] * 0.5f);
						}
					}
				}
				newtrilist.Add(newtri);
			}
			vertices = subdlist.ToArray();
			var oldformat = mesh.indexFormat;
			if(vertices.Length > 65000)
			{
				try
				{
					if(oldformat != IndexFormat.UInt32)
					{
						mesh.indexFormat = IndexFormat.UInt32;
					}
				}
				catch
				{
					return;
				}
			}
			else
			{
				if(oldformat != IndexFormat.UInt16)
				{
					mesh.indexFormat = IndexFormat.UInt16;
				}
			}
			mesh.vertices = vertices;
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint + 1; i++)
			{
				if(IsMonoSubmesh)
				{
					mesh.SetTriangles(newtrilist[0], i);
				}
				else
				{
					if(newtrilist.Count > i)
					{
						mesh.SetTriangles(newtrilist[i], i);
					}
					else
					{
						mesh.SetTriangles(newtrilist[0], i);
					}
				}
			}
			mesh.colors = colorlist.ToArray();
			mesh.uv = uv0list.ToArray();
			mesh.uv2 = uv2list.ToArray();
			mesh.uv3 = uv3list.ToArray();
			mesh.uv4 = uv4list.ToArray();
			mesh.bindposes = binds;
			if(IsWeight)
			{
				var ntweight1s = new NativeArray<BoneWeight1>(bonew1list.ToArray(), Allocator.Temp);
				var PerVerts = new NativeArray<byte>(perverlist.ToArray(), Allocator.Temp);
				if(PerVerts.Length > 0)
				{
					mesh.SetBoneWeights(PerVerts, ntweight1s);
				}
			}
			else
			{
				BoneWeight1Create(mesh);
			}
			IsBrushedArray = brushedlist.ToArray();
		}

		private static void RemeshPolyFinish(Mesh mesh)
		{
			var vertices = mesh.vertices;
			var subcnt = IsMonoSubmesh ? 1 : mesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}

			//int[] triangles = mesh.GetTriangles(0);
			var colors = mesh.colors;
			if(colors.Length < 1)
			{
				colors = Enumerable.Repeat(Color.white, vertices.Length).ToArray();
			}
			var uv0s = mesh.uv;
			var uv2s = mesh.uv2;
			var uv3s = mesh.uv3;
			var uv4s = mesh.uv4;
			if(uv0s.Length < 1)
			{
				uv0s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv2s.Length < 1)
			{
				uv2s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv3s.Length < 1)
			{
				uv3s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv4s.Length < 1)
			{
				uv4s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			var binds = mesh.bindposes;
			var weight1s = mesh.GetAllBoneWeights().ToArray();
			var pervers = mesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var pcnt = 0;
			if(IsWeight)
			{
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			var newvdict = new Dictionary<Vector3, int>();
			var newvlist = new List<Vector3>();
			var newcolorlist = new List<Color>();
			var newuv0list = new List<Vector2>();
			var newuv2list = new List<Vector2>();
			var newuv3list = new List<Vector2>();
			var newuv4list = new List<Vector2>();
			var newperverlist = new List<byte>();
			var newbwlist = new List<BoneWeight1>();
			var calcarr = new int[vertices.Length];
			var w1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
			for(var i = 0; i < vertices.Length; i++)
			{
				int j;
				if(newvdict.TryGetValue(vertices[i], out j))
				{
					calcarr[i] = j;
				}
				else
				{
					var idx0 = newvdict.Count;
					newvdict[vertices[i]] = idx0;
					calcarr[i] = idx0;
					newvlist.Add(vertices[i]);
					newcolorlist.Add(colors[i]);
					newuv0list.Add(uv0s[i]);
					newuv2list.Add(uv2s[i]);
					newuv3list.Add(uv3s[i]);
					newuv4list.Add(uv4s[i]);
					if(IsWeight)
					{
						int pi0 = pervers[i];
						var bwlist0 = new List<BoneWeight1>();
						if(pi0 > 0)
						{
							for(var k = 0; k < pi0; k++)
							{
								try
								{
									bwlist0.Add(weight1s[boneidxs[i] + k]);
								}
								catch
								{
									bwlist0.Add(w1);
								}
							}
						}
						if(bwlist0.Count < 1)
						{
							bwlist0.Add(w1);
						}
						newbwlist.AddRange(bwlist0);
						newperverlist.Add((byte)bwlist0.Count);
					}
				}
			}
			var newtriarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				var triarr = triarrlist[i];
				var newtriarr = triarr;
				for(var j = 0; j < triarr.Length; j++)
				{
					var t = calcarr[triarr[j]];
					newtriarr[j] = t;
				}
				newtriarrlist.Add(newtriarr);
			}
			vertices = newvlist.ToArray();
			triarrlist = newtriarrlist;

			//triangles = newtri.ToArray();
			colors = newcolorlist.ToArray();
			uv0s = newuv0list.ToArray();
			uv2s = newuv2list.ToArray();
			uv3s = newuv3list.ToArray();
			uv4s = newuv4list.ToArray();
			weight1s = newbwlist.ToArray();
			pervers = newperverlist.ToArray();
			boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			pcnt = 0;
			for(var i = 0; i < pervers.Length; i++)
			{
				boneidxs[i] = pcnt;
				pcnt += pervers[i];
			}
			if(boneidxs.Length < 1)
			{
				boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
			}
			var AdjIdxListList = new List<List<int>>();
			for(var i = 0; i < vertices.Length; i++)
			{
				AdjIdxListList.Add(new List<int>());
			}
			for(var i = 0; i < subcnt; i++)
			{
				var triarr = triarrlist[i];
				for(var j = 0; j < triarr.Length; j += 3)
				{
					var i0 = triarr[j];
					var i1 = triarr[j + 1];
					var i2 = triarr[j + 2];
					AdjIdxListList[triarr[j]].AddRange(new[] { i1, i2 });
					AdjIdxListList[triarr[j + 1]].AddRange(new[] { i2, i0 });
					AdjIdxListList[triarr[j + 2]].AddRange(new[] { i0, i1 });
				}
			}
			var dc0 = avgPointDist * retopoWeight * 1.0f;
			var weight1list = new List<BoneWeight1>();
			var perverlist = new List<byte>();
			for(var i = 0; i < vertices.Length; i++)
			{
				//if (uv4s[i].x < maskWeight) continue;
				var bonew1list = new List<BoneWeight1>();
				var v0 = vertices[i];
				var AdjIdxList = AdjIdxListList[i];
				foreach(var t in AdjIdxList)
				{
					var vpos = vertices[t];
					var d0 = Vector3.Distance(v0, vpos);
					if(d0 < dc0 && t != 0)
					{
						vertices[t] = v0;
					}
				}
				if(IsWeight && bonew1list.Count < 1)
				{
					int pi0 = pervers[i];
					var bwlist0 = new List<BoneWeight1>();
					if(pi0 > 0)
					{
						for(var j = 0; j < pi0; j++)
						{
							try
							{
								bwlist0.Add(weight1s[boneidxs[i] + j]);
							}
							catch
							{
								bwlist0.Add(w1);
							}
						}
					}
					if(bwlist0.Count < 1)
					{
						bwlist0.Add(w1);
					}
					bonew1list.AddRange(bwlist0);
				}
				if(IsWeight)
				{
					weight1list.AddRange(bonew1list);
					perverlist.Add((byte)bonew1list.Count);
				}
			}
			if(IsWeight)
			{
				weight1s = weight1list.ToArray();
				pervers = perverlist.ToArray();
				boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
				pcnt = 0;
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			if(AutoCloseHole)
			{
				var AdjVSet = new HashSet<Vector3>();
				for(var i = 0; i < subcnt; i++)
				{
					var triarr = triarrlist[i];
					for(var j = 0; j < triarr.Length; j += 3)
					{
						var v0 = vertices[triarr[j]];
						var v1 = vertices[triarr[j + 1]];
						var v2 = vertices[triarr[j + 2]];
						if(v0 == v1 || v1 == v2 || v2 == v0)
						{
							continue;
						}
						var q0 = v0 * 0.5f + v1 * 0.5f;
						var q1 = v1 * 0.5f + v2 * 0.5f;
						var q2 = v2 * 0.5f + v0 * 0.5f;
						if(AdjVSet.Contains(q0))
						{
							AdjVSet.Remove(q0);
						}
						else
						{
							AdjVSet.Add(q0);
						}
						if(AdjVSet.Contains(q1))
						{
							AdjVSet.Remove(q1);
						}
						else
						{
							AdjVSet.Add(q1);
						}
						if(AdjVSet.Contains(q2))
						{
							AdjVSet.Remove(q2);
						}
						else
						{
							AdjVSet.Add(q2);
						}
					}
				}
				var edtrilist = new List<int>();
				var HoleSet = new HashSet<Vector3>();
				for(var i = 0; i < subcnt; i++)
				{
					var triarr = triarrlist[i];
					for(var j = 0; j < triarr.Length; j += 3)
					{
						var v0 = vertices[triarr[j]];
						var v1 = vertices[triarr[j + 1]];
						var v2 = vertices[triarr[j + 2]];
						var q0 = v0 * 0.5f + v1 * 0.5f;
						var q1 = v1 * 0.5f + v2 * 0.5f;
						var q2 = v2 * 0.5f + v0 * 0.5f;
						if(AdjVSet.Contains(q0))
						{
							edtrilist.Add(triarr[j]);
							edtrilist.Add(triarr[j + 1]);
							HoleSet.Add(q0);
						}
						else if(AdjVSet.Contains(q1))
						{
							edtrilist.Add(triarr[j + 1]);
							edtrilist.Add(triarr[j + 2]);
							HoleSet.Add(q1);
						}
						else if(AdjVSet.Contains(q2))
						{
							edtrilist.Add(triarr[j + 2]);
							edtrilist.Add(triarr[j]);
							HoleSet.Add(q2);
						}
					}
				}

				//newtri = new List<int>(triangles);
				var newvarr = new List<Vector3>(vertices).ToArray();
				for(var i = 0; i < edtrilist.Count; i += 2)
				{
					var i0 = edtrilist[i];
					var i1 = edtrilist[i + 1];
					int idx0 = 0, idx1 = 0;
					var e0 = vertices[i0];
					var e1 = vertices[i1];
					var AdjIdxList0 = AdjIdxListList[i0];
					var AdjIdxList1 = AdjIdxListList[i1];
					foreach(var idx in AdjIdxList0)
					{
						if(idx == i1)
						{
							continue;
						}
						var e2 = vertices[idx];
						var q0 = e0 * 0.5f + e2 * 0.5f;
						if(HoleSet.Contains(q0))
						{
							idx0 = idx;
							break;
						}
					}
					if(idx0 > 0)
					{
						var vc0 = newvarr[i0] * 0.333f + newvarr[i1] * 0.333f + newvarr[idx0] * 0.333f;
						newvarr[i0] = vc0;
						newvarr[i1] = vc0;
						newvarr[idx0] = vc0;
						var cc0 = colors[i0] * 0.333f + colors[i1] * 0.333f + colors[idx0] * 0.333f;
						colors[i0] = cc0;
						colors[i1] = cc0;
						colors[idx0] = cc0;
						var uvc0_0 = uv0s[i0] * 0.333f + uv0s[i1] * 0.333f + uv0s[idx0] * 0.333f;
						uv0s[i0] = uvc0_0;
						uv0s[i1] = uvc0_0;
						uv0s[idx0] = uvc0_0;
						var uvc2_0 = uv2s[i0] * 0.333f + uv2s[i1] * 0.333f + uv2s[idx0] * 0.333f;
						uv2s[i0] = uvc2_0;
						uv2s[i1] = uvc2_0;
						uv2s[idx0] = uvc2_0;
						var uvc3_0 = uv3s[i0] * 0.333f + uv3s[i1] * 0.333f + uv3s[idx0] * 0.333f;
						uv3s[i0] = uvc3_0;
						uv3s[i1] = uvc3_0;
						uv3s[idx0] = uvc3_0;
						var uvc4_0 = uv4s[i0] * 0.333f + uv4s[i1] * 0.333f + uv4s[idx0] * 0.333f;
						uv4s[i0] = uvc4_0;
						uv4s[i1] = uvc4_0;
						uv4s[idx0] = uvc4_0;
					}
					foreach(var idx in AdjIdxList1)
					{
						if(idx == i0)
						{
							continue;
						}
						var e3 = vertices[idx];
						var q1 = e1 * 0.5f + e3 * 0.5f;
						if(HoleSet.Contains(q1))
						{
							idx1 = idx;
							break;
						}
					}
					if(idx1 > 0)
					{
						var vc0 = newvarr[i0] * 0.333f + newvarr[i1] * 0.333f + newvarr[idx1] * 0.333f;
						newvarr[i0] = vc0;
						newvarr[i1] = vc0;
						newvarr[idx1] = vc0;
						var cc0 = colors[i0] * 0.333f + colors[i1] * 0.333f + colors[idx1] * 0.333f;
						colors[i0] = cc0;
						colors[i1] = cc0;
						colors[idx1] = cc0;
						var uvc0_0 = uv0s[i0] * 0.333f + uv0s[i1] * 0.333f + uv0s[idx1] * 0.333f;
						uv0s[i0] = uvc0_0;
						uv0s[i1] = uvc0_0;
						uv0s[idx1] = uvc0_0;
						var uvc2_0 = uv2s[i0] * 0.333f + uv2s[i1] * 0.333f + uv2s[idx1] * 0.333f;
						uv2s[i0] = uvc2_0;
						uv2s[i1] = uvc2_0;
						uv2s[idx1] = uvc2_0;
						var uvc3_0 = uv3s[i0] * 0.333f + uv3s[i1] * 0.333f + uv3s[idx1] * 0.333f;
						uv3s[i0] = uvc3_0;
						uv3s[i1] = uvc3_0;
						uv3s[idx1] = uvc3_0;
						var uvc4_0 = uv4s[i0] * 0.333f + uv4s[i1] * 0.333f + uv4s[idx1] * 0.333f;
						uv4s[i0] = uvc4_0;
						uv4s[i1] = uvc4_0;
						uv4s[idx1] = uvc4_0;
					}
				}
				vertices = new List<Vector3>(newvarr).ToArray();
			}
			var newtrilist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				var triarr = triarrlist[i];
				var newtri = new List<int>();
				for(var j = 0; j < triarr.Length; j += 3)
				{
					var v0 = vertices[triarr[j]];
					var v1 = vertices[triarr[j + 1]];
					var v2 = vertices[triarr[j + 2]];
					if(v0 == v1 || v1 == v2 || v2 == v0)
					{
						continue;
					}
					newtri.AddRange(new[] { triarr[j], triarr[j + 1], triarr[j + 2] });
				}
				newtrilist.Add(newtri.ToArray());
			}
			triarrlist = newtrilist;
			newvdict = new Dictionary<Vector3, int>();
			newvlist = new List<Vector3>();
			newcolorlist = new List<Color>();
			newuv0list = new List<Vector2>();
			newuv2list = new List<Vector2>();
			newuv3list = new List<Vector2>();
			newuv4list = new List<Vector2>();
			newperverlist = new List<byte>();
			newbwlist = new List<BoneWeight1>();
			calcarr = new int[vertices.Length];
			for(var i = 0; i < vertices.Length; i++)
			{
				int j;
				if(newvdict.TryGetValue(vertices[i], out j))
				{
					calcarr[i] = j;
				}
				else
				{
					var idx0 = newvdict.Count;
					newvdict[vertices[i]] = idx0;
					calcarr[i] = idx0;
					newvlist.Add(vertices[i]);
					newcolorlist.Add(colors[i]);
					newuv0list.Add(uv0s[i]);
					newuv2list.Add(uv2s[i]);
					newuv3list.Add(uv3s[i]);
					newuv4list.Add(uv4s[i]);
					if(IsWeight)
					{
						int pi0 = pervers[i];
						var bwlist0 = new List<BoneWeight1>();
						if(pi0 > 0)
						{
							for(var k = 0; k < pi0; k++)
							{
								try
								{
									bwlist0.Add(weight1s[boneidxs[i] + k]);
								}
								catch
								{
									bwlist0.Add(w1);
								}
							}
						}
						if(bwlist0.Count < 1)
						{
							bwlist0.Add(w1);
						}
						newbwlist.AddRange(bwlist0);
						newperverlist.Add((byte)bwlist0.Count);
					}
				}
			}
			newtriarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				var triarr = triarrlist[i];
				var newtriarr = triarr;
				for(var j = 0; j < triarr.Length; j++)
				{
					var t = calcarr[triarr[j]];
					newtriarr[j] = t;
				}
				newtriarrlist.Add(newtriarr);
			}
			vertices = newvlist.ToArray();

			//triangles = newtri.ToArray();
			triarrlist = newtriarrlist;
			colors = newcolorlist.ToArray();
			uv0s = newuv0list.ToArray();
			uv2s = newuv2list.ToArray();
			uv3s = newuv3list.ToArray();
			uv4s = newuv4list.ToArray();
			if(IsWeight)
			{
				weight1s = newbwlist.ToArray();
				pervers = newperverlist.ToArray();
				boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
				pcnt = 0;
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}

			//triangles = newtri.ToArray();
			if(AutoFixBlackPoly)
			{
				AdjIdxListList = new List<List<int>>();
				for(var i = 0; i < vertices.Length; i++)
				{
					AdjIdxListList.Add(new List<int>());
				}
				for(var i = 0; i < subcnt; i++)
				{
					var triarr = triarrlist[i];
					for(var j = 0; j < triarr.Length; j += 3)
					{
						var i0 = triarr[j];
						var i1 = triarr[j + 1];
						var i2 = triarr[j + 2];
						AdjIdxListList[triarr[j]].AddRange(new[] { i1, i2 });
						AdjIdxListList[triarr[j + 1]].AddRange(new[] { i2, i0 });
						AdjIdxListList[triarr[j + 2]].AddRange(new[] { i0, i1 });
					}
				}
				var MoveArr = new bool[vertices.Length];
				weight1list = new List<BoneWeight1>();
				perverlist = new List<byte>();
				for(var i = 0; i < vertices.Length; i++)
				{
					var AdjIdxList = AdjIdxListList[i];
					var flag0 = false;
					if(AdjIdxList.Count >= 5 && !MoveArr[i])
					{
						foreach(var t in AdjIdxList)
						{
							if(AdjIdxListList[t].Count < 5)
							{
								if(MoveArr[t])
								{
									continue;
								}
								vertices[i] = vertices[t];
								MoveArr[t] = true;
								if(IsWeight)
								{
									int pi0 = pervers[i];
									var bwlist0 = new List<BoneWeight1>();
									if(pi0 > 0)
									{
										for(var j = 0; j < pi0; j++)
										{
											try
											{
												bwlist0.Add(weight1s[boneidxs[i] + j]);
											}
											catch
											{
												bwlist0.Add(w1);
											}
										}
									}
									int pi1 = pervers[t];
									var bwlist1 = new List<BoneWeight1>();
									if(pi1 > 0)
									{
										for(var j = 0; j < pi1; j++)
										{
											try
											{
												bwlist1.Add(weight1s[boneidxs[t] + j]);
											}
											catch
											{
												bwlist1.Add(w1);
											}
										}
									}
									var bwlist2 = BoneWeight1Lerp(bwlist0, bwlist1);
									weight1list.AddRange(bwlist2);
									perverlist.Add((byte)bwlist2.Count);
								}
								flag0 = true;
								break;
							}
						}
					}
					if(!flag0 && IsWeight)
					{
						int pi0 = pervers[i];
						var bwlist0 = new List<BoneWeight1>();
						if(pi0 > 0)
						{
							for(var j = 0; j < pi0; j++)
							{
								try
								{
									bwlist0.Add(weight1s[boneidxs[i] + j]);
								}
								catch
								{
									bwlist0.Add(w1);
								}
							}
						}
						if(bwlist0.Count < 1)
						{
							bwlist0.Add(w1);
						}
						weight1list.AddRange(bwlist0);
						perverlist.Add((byte)bwlist0.Count);
					}
				}
				if(IsWeight)
				{
					weight1s = weight1list.ToArray();
					pervers = perverlist.ToArray();
					boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
					pcnt = 0;
					for(var i = 0; i < pervers.Length; i++)
					{
						boneidxs[i] = pcnt;
						pcnt += pervers[i];
					}
					if(boneidxs.Length < 1)
					{
						boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
					}
				}
				newvdict = new Dictionary<Vector3, int>();
				newvlist = new List<Vector3>();
				newcolorlist = new List<Color>();
				newuv0list = new List<Vector2>();
				newuv2list = new List<Vector2>();
				newuv3list = new List<Vector2>();
				newuv4list = new List<Vector2>();
				newperverlist = new List<byte>();
				newbwlist = new List<BoneWeight1>();
				calcarr = new int[vertices.Length];
				for(var i = 0; i < vertices.Length; i++)
				{
					int j;
					if(newvdict.TryGetValue(vertices[i], out j))
					{
						calcarr[i] = j;
					}
					else
					{
						var idx0 = newvdict.Count;
						newvdict[vertices[i]] = idx0;
						calcarr[i] = idx0;
						newvlist.Add(vertices[i]);
						newcolorlist.Add(colors[i]);
						newuv0list.Add(uv0s[i]);
						newuv2list.Add(uv2s[i]);
						newuv3list.Add(uv3s[i]);
						newuv4list.Add(uv4s[i]);
						if(IsWeight)
						{
							int pi0 = pervers[i];
							var bwlist0 = new List<BoneWeight1>();
							if(pi0 > 0)
							{
								for(var k = 0; k < pi0; k++)
								{
									try
									{
										bwlist0.Add(weight1s[boneidxs[i] + k]);
									}
									catch
									{
										bwlist0.Add(w1);
									}
								}
							}
							if(bwlist0.Count < 1)
							{
								bwlist0.Add(w1);
							}
							newbwlist.AddRange(bwlist0);
							newperverlist.Add((byte)bwlist0.Count);
						}
					}
				}
				newtriarrlist = new List<int[]>();
				for(var i = 0; i < subcnt; i++)
				{
					var trirarr = triarrlist[i];
					var newtriarr = trirarr;
					for(var j = 0; j < trirarr.Length; j++)
					{
						var t = calcarr[trirarr[j]];
						newtriarr[j] = t;
					}
					newtriarrlist.Add(newtriarr);
				}
				vertices = newvlist.ToArray();
				triarrlist = newtriarrlist;
				colors = newcolorlist.ToArray();
				uv0s = newuv0list.ToArray();
				uv2s = newuv2list.ToArray();
				uv3s = newuv3list.ToArray();
				uv4s = newuv4list.ToArray();
			}
			mesh.Clear();
			if(vertices.Length > 65000)
			{
				try
				{
					mesh.indexFormat = IndexFormat.UInt32;
				}
				catch
				{
					return;
				}
			}
			else
			{
				mesh.indexFormat = IndexFormat.UInt16;
			}
			mesh.vertices = vertices;
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint + 1; i++)
			{
				if(IsMonoSubmesh)
				{
					mesh.SetTriangles(triarrlist[0], i);
				}
				else
				{
					if(triarrlist.Count > i)
					{
						mesh.SetTriangles(triarrlist[i], i);
					}
					else
					{
						mesh.SetTriangles(triarrlist[0], i);
					}
				}
			}
			mesh.colors = colors;
			mesh.uv = uv0s;
			mesh.uv2 = uv2s;
			mesh.uv3 = uv3s;
			mesh.uv4 = uv4s;
			mesh.bindposes = binds;
			if(IsWeight)
			{
				var ntweight1s = new NativeArray<BoneWeight1>(newbwlist.ToArray(), Allocator.Temp);
				var PerVerts = new NativeArray<byte>(newperverlist.ToArray(), Allocator.Temp);
				mesh.SetBoneWeights(PerVerts, ntweight1s);
			}
			else
			{
				BoneWeight1Create(mesh);
			}
			MergetriGenerate(mesh);
			CalcMeshNormals(mesh);
			mesh.RecalculateTangents();

			//mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		}

		private static void FixBlackPoly(Mesh mesh)
		{
			var vertices = mesh.vertices;
			var subcnt = IsMonoSubmesh ? 1 : mesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}
			var colors = mesh.colors;
			var uv0s = mesh.uv;
			var uv2s = mesh.uv2;
			var uv3s = mesh.uv3;
			var uv4s = mesh.uv4;
			var binds = mesh.bindposes;
			var weight1s = currentMesh.GetAllBoneWeights().ToArray();
			var pervers = currentMesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;

			//if (pervers.Length > 0) IsWeight = true;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var pcnt = 0;
			if(IsWeight)
			{
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
			}
			if(boneidxs.Length < 1)
			{
				boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
			}
			var AdjIdxListList = new List<List<int>>();
			for(var i = 0; i < vertices.Length; i++)
			{
				AdjIdxListList.Add(new List<int>());
			}
			for(var i = 0; i < subcnt; i++)
			{
				var triarr = triarrlist[i];
				for(var j = 0; j < triarr.Length; j += 3)
				{
					var i0 = triarr[j];
					var i1 = triarr[j + 1];
					var i2 = triarr[j + 2];
					AdjIdxListList[triarr[j]].AddRange(new[] { i1, i2 });
					AdjIdxListList[triarr[j + 1]].AddRange(new[] { i2, i0 });
					AdjIdxListList[triarr[j + 2]].AddRange(new[] { i0, i1 });
				}
			}
			var MoveArr = new bool[vertices.Length];
			var weight1list = new List<BoneWeight1>();
			var perverlist = new List<byte>();
			var w1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
			for(var i = 0; i < vertices.Length; i++)
			{
				var AdjIdxList = AdjIdxListList[i];
				var flag0 = false;
				if(AdjIdxList.Count >= 5 && !MoveArr[i])
				{
					foreach(var t in AdjIdxList)
					{
						if(AdjIdxListList[t].Count < 5)
						{
							if(MoveArr[t])
							{
								continue;
							}
							vertices[i] = vertices[t];
							MoveArr[t] = true;
							if(IsWeight)
							{
								int pi0 = pervers[i];
								var bwlist0 = new List<BoneWeight1>();
								if(pi0 > 0)
								{
									for(var j = 0; j < pi0; j++)
									{
										try
										{
											bwlist0.Add(weight1s[boneidxs[i] + j]);
										}
										catch
										{
											bwlist0.Add(w1);
										}
									}
								}
								int pi1 = pervers[t];
								var bwlist1 = new List<BoneWeight1>();
								if(pi1 > 0)
								{
									for(var j = 0; j < pi1; j++)
									{
										try
										{
											bwlist1.Add(weight1s[boneidxs[t] + j]);
										}
										catch
										{
											bwlist1.Add(w1);
										}
									}
								}
								var bwlist2 = BoneWeight1Lerp(bwlist0, bwlist1);
								weight1list.AddRange(bwlist2);
								perverlist.Add((byte)bwlist2.Count);
							}
							flag0 = true;
							break;
						}
					}
				}
				if(!flag0 && IsWeight)
				{
					int pi0 = pervers[i];
					var bwlist0 = new List<BoneWeight1>();
					if(pi0 > 0)
					{
						for(var j = 0; j < pi0; j++)
						{
							try
							{
								bwlist0.Add(weight1s[boneidxs[i] + j]);
							}
							catch
							{
								bwlist0.Add(w1);
							}
						}
					}
					if(bwlist0.Count < 1)
					{
						bwlist0.Add(w1);
					}
					weight1list.AddRange(bwlist0);
					perverlist.Add((byte)bwlist0.Count);
				}
			}
			if(IsWeight)
			{
				weight1s = weight1list.ToArray();
				pervers = perverlist.ToArray();
				pcnt = 0;
				boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			var newtri = new List<int>();
			var newtrilistlist = new List<List<int>>();
			for(var i = 0; i < subcnt; i++)
			{
				var triarr = triarrlist[i];
				var newtrilist = new List<int>();
				for(var j = 0; j < triarr.Length; j += 3)
				{
					var v0 = vertices[triarr[j]];
					var v1 = vertices[triarr[j + 1]];
					var v2 = vertices[triarr[j + 2]];
					if(v0 == v1 || v1 == v2 || v2 == v0)
					{
						continue;
					}
					newtrilist.AddRange(new[] { triarr[j], triarr[j + 1], triarr[j + 2] });
				}
				newtrilistlist.Add(newtrilist);
			}
			triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(newtrilistlist[i].ToArray());
			}
			var newvdict = new Dictionary<Vector3, int>();
			var newvlist = new List<Vector3>();
			var newcolorlist = new List<Color>();
			var newuv0list = new List<Vector2>();
			var newuv2list = new List<Vector2>();
			var newuv3list = new List<Vector2>();
			var newuv4list = new List<Vector2>();
			var newbwlist = new List<BoneWeight1>();
			var newperverlist = new List<byte>();
			var calcarr = new int[vertices.Length];
			for(var i = 0; i < vertices.Length; i++)
			{
				int j;
				if(newvdict.TryGetValue(vertices[i], out j))
				{
					calcarr[i] = j;
				}
				else
				{
					var idx0 = newvdict.Count;
					newvdict[vertices[i]] = idx0;
					calcarr[i] = idx0;
					newvlist.Add(vertices[i]);
					newcolorlist.Add(colors[i]);
					newuv0list.Add(uv0s[i]);
					newuv2list.Add(uv2s[i]);
					newuv3list.Add(uv3s[i]);
					newuv4list.Add(uv4s[i]);
					if(IsWeight)
					{
						int pi0 = perverlist[i];
						var bwlist0 = new List<BoneWeight1>();
						if(pi0 > 0)
						{
							for(var k = 0; k < pi0; k++)
							{
								try
								{
									bwlist0.Add(weight1s[boneidxs[i] + k]);
								}
								catch
								{
									bwlist0.Add(w1);
								}
							}
						}
						if(bwlist0.Count < 1)
						{
							bwlist0.Add(w1);
						}
						newbwlist.AddRange(bwlist0);
						newperverlist.Add((byte)bwlist0.Count);
					}
				}
			}
			var newtriarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				var triarr = triarrlist[i];
				var newtriarr = triarr;
				for(var j = 0; j < triarr.Length; j++)
				{
					var t = calcarr[triarr[j]];
					newtriarr[j] = t;
				}
				newtriarrlist.Add(newtriarr);
			}
			triarrlist = newtriarrlist;
			vertices = newvlist.ToArray();

			//triangles = newtri.ToArray();
			colors = newcolorlist.ToArray();
			uv0s = newuv0list.ToArray();
			uv2s = newuv2list.ToArray();
			uv3s = newuv3list.ToArray();
			uv4s = newuv4list.ToArray();
			mesh.Clear();
			if(vertices.Length > 65000)
			{
				try
				{
					mesh.indexFormat = IndexFormat.UInt32;
				}
				catch
				{
					return;
				}
			}
			else
			{
				mesh.indexFormat = IndexFormat.UInt16;
			}
			mesh.vertices = vertices;
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint + 1; i++)
			{
				if(IsMonoSubmesh)
				{
					mesh.SetTriangles(triarrlist[0], i);
				}
				else
				{
					if(triarrlist.Count > i)
					{
						mesh.SetTriangles(triarrlist[i], i);
					}
					else
					{
						mesh.SetTriangles(triarrlist[0], i);
					}
				}
			}
			mesh.colors = colors;
			mesh.uv = uv0s;
			mesh.uv2 = uv2s;
			mesh.uv3 = uv3s;
			mesh.uv4 = uv4s;
			mesh.bindposes = binds;
			if(IsWeight)
			{
				var ntweight1s = new NativeArray<BoneWeight1>(newbwlist.ToArray(), Allocator.Temp);
				var PerVerts = new NativeArray<byte>(newperverlist.ToArray(), Allocator.Temp);
				mesh.SetBoneWeights(PerVerts, ntweight1s);
			}
			else
			{
				BoneWeight1Create(mesh);
			}

			//mesh.RecalculateNormals();
			MergetriGenerate(mesh);
			CalcMeshNormals(mesh);
			mesh.RecalculateBounds();
		}

		private static void FixAutoRemeshFinsh()
		{
			if(currentMesh == null)
			{
				return;
			}
			if(esmode == EditorSculptMode.RemeshSculpt || esmode == EditorSculptMode.RemeshBeta)
			{
				if(IsDoneRealtimeRemesh && BrushString != "Smooth" && !IsDoneSmooth)
				{
					RemeshPolyFinish(currentMesh);
					IsDoneRealtimeRemesh = false;
				}
				else if(IsRealTimeAutoRemesh && BrushString != "Smooth" && !IsDoneSmooth)
				{
					RemeshPolyFinish(currentMesh);
					IsDoneRealtimeRemesh = false;
				}
			}
		}

		//private static void FixImportedMesh()
		//{
		//    if (currentObject.GetComponent<MeshFilter>())
		//    {
		//        try
		//        {
		//            string meshPath = AssetDatabase.GetAssetPath(currentMeshFilter.sharedMesh);
		//            if (meshPath.Length < 1) return;
		//            ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(meshPath);
		//            AssetDatabase.ImportAsset(meshPath);
		//            ImportPath = meshPath;
		//            IsModelImporter = true;
		//        }
		//        catch { }
		//    }
		//    else if (currentObject.GetComponent<SkinnedMeshRenderer>())
		//    {
		//        try
		//        {
		//            string meshPath = AssetDatabase.GetAssetPath(currentSkinned.sharedMesh);
		//            if (meshPath.Length < 1) return;
		//            ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(meshPath);
		//            AssetDatabase.ImportAsset(meshPath);
		//            ImportPath = meshPath;
		//            IsModelImporter = true;
		//        }
		//        catch { }
		//    }
		//    CurrentScale = currentObject.transform.lossyScale.x;
		//}

		private static void RevertMetaFile(string metapath, string metadata)
		{
			if(metapath.Length > 1 && metadata.Length > 1)
			{
				AssetDatabase.ReleaseCachedFileHandles();
				var oldattr = File.GetAttributes(metapath);
				File.SetAttributes(metapath, FileAttributes.Normal);
				File.WriteAllText(metapath, metadata);
				File.SetAttributes(metapath, oldattr);

				//List<int> verarr = UnityVersionToIntList(Application.version);
				//Debug.Log(verarr[0] + "  :" + verarr[1]);
				//if(verarr[0]<=2019 && verarr[1]<4)
				//{
				//    FileAttributes oldattr = File.GetAttributes(metadata);
				//    File.SetAttributes(metapath, FileAttributes.Normal);
				//    File.WriteAllText(metapath, metadata);
				//    File.SetAttributes(metapath, oldattr);
				//}
				//else
				//{
				//    File.WriteAllText(metapath, metadata);
				//}

				AssetDatabase.ReleaseCachedFileHandles();
				AssetDatabase.Refresh();
			}
		}

		//private static void RevertPreMetaFile()
		//{
		//    if (PreimportMetaPath.Length <= 1 || PremetaContent.Length <= 1) return;
		//    AssetDatabase.ReleaseCachedFileHandles();
		//    FileAttributes oldattr = File.GetAttributes(PreimportMetaPath);
		//    File.SetAttributes(PreimportMetaPath, FileAttributes.Normal);
		//    File.WriteAllText(PreimportMetaPath, PremetaContent);
		//    File.SetAttributes(PreimportMetaPath, oldattr);

		//    AssetDatabase.ReleaseCachedFileHandles();
		//    AssetDatabase.Refresh();
		//    PreimportMetaPath = "";
		//    PremetaContent = "";
		//}

		private static Matrix4x4[] GetBindPoseFromImporter()
		{
			if(currentObject == null || currentMesh == null)
			{
				return null;
			}
			try
			{
				var meshPath = AssetDatabase.GetAssetPath(currentMesh);

				//Debug.Log(meshPath);
				var aimp = AssetImporter.GetAtPath(meshPath);

				//Debug.Log(aimp.userData);
				if(aimp.userData.Length > 1)
				{
					var mesh = (Mesh)AssetDatabase.LoadAssetAtPath(aimp.userData, typeof(Mesh));

					//Debug.Log(mesh.name);
					if(mesh != null)
					{
						return mesh.bindposes;
					}
				}
			}
			catch
			{
			}
			return currentMesh.bindposes;
		}

		//private static void ModelImporterChangeAnimeImport(bool IsEnable)
		//{
		//    if ((currentObject == null) || (currentMesh==null)) return;
		//    if (currentObject.GetComponent<MeshFilter>())
		//    {
		//        try
		//        {
		//            string meshPath = AssetDatabase.GetAssetPath(currentObject.GetComponent<MeshFilter>().sharedMesh);
		//            AssetImporter aimp = AssetImporter.GetAtPath(meshPath);
		//            ModelImporter mimp = (ModelImporter)AssetImporter.GetAtPath(aimp.userData);
		//            mimp.importAnimation = IsEnable;
		//            mimp.SaveAndReimport();
		//            if (!IsEnable) IsDisabledAnimationImport = true;
		//        }
		//        catch { }
		//    }
		//    else if (currentObject.GetComponent<SkinnedMeshRenderer>())
		//    {
		//        try
		//        {
		//            string meshPath = AssetDatabase.GetAssetPath(currentObject.GetComponent<SkinnedMeshRenderer>().sharedMesh);
		//            AssetImporter aimp = AssetImporter.GetAtPath(meshPath);
		//            ModelImporter mimp = (ModelImporter)AssetImporter.GetAtPath(aimp.userData);
		//            mimp.importAnimation = IsEnable;
		//            mimp.SaveAndReimport();
		//            if (!IsEnable) IsDisabledAnimationImport = true;
		//        }
		//        catch { }
		//    }
		//}

		private static void FixImportedMesh()
		{
			if(currentObject == null)
			{
				return;
			}

			//PreimportMetaPath = "";
			//PremetaContent = "";
			if(currentObject.GetComponent<MeshFilter>())
			{
				try
				{
					var meshPath = AssetDatabase.GetAssetPath(currentMeshFilter.sharedMesh);

					//string meshPath = "";

					//    meshPath = AssetDatabase.GetAssetPath(currentMeshFilter.sharedMesh);
					//}
					//finally
					//{
					//    AssetDatabase.StopAssetEditing();
					//}
					var importer = (ModelImporter)AssetImporter.GetAtPath(meshPath);
					if(importer)
					{
						//if (importer.importAnimation == true)
						//{
						//    if (EditorUtility.DisplayDialog("Caution", "Import Animation is on." +
						//        "Editor Sculpt does't work correctly with it on. Do you want to disable that?", "OK", "NO"))
						//    {
						//        importer.importAnimation = false;
						//        try
						//        {
						//            AssetDatabase.StartAssetEditing();
						//            AssetDatabase.WriteImportSettingsIfDirty(meshPath);
						//            //AssetDatabase.Refresh();
						//            AssetDatabase.ImportAsset(meshPath);
						//        }
						//        finally
						//        {
						//            AssetDatabase.StopAssetEditing();
						//        }
						//    }
						//}

						//IsOldAnimationImport = importer.importAnimation;
						//ModelImporterPath = meshPath;

						importMetaPath = AssetDatabase.GetTextMetaFilePathFromAssetPath(meshPath);
						metaContent = File.ReadAllText(importMetaPath);
						importer.importNormals = ModelImporterNormals.Calculate;
						importer.normalCalculationMode = ModelImporterNormalCalculationMode.AngleWeighted;
						importer.normalSmoothingAngle = 60.0f;

						//importSize = importer.fileScale;
						//importSize *= importer.useFileUnits ? 100.0f : 1.0f;
						importSize = 1 / importer.fileScale;
						try
						{
							AssetDatabase.StartAssetEditing();
							AssetDatabase.WriteImportSettingsIfDirty(meshPath);

							//AssetDatabase.Refresh();
							AssetDatabase.ImportAsset(meshPath);
						}
						finally
						{
							AssetDatabase.StopAssetEditing();
						}
						IsModelImporter = true;
						UseModelImporter = true;
					}
					else
					{
						IsModelImporter = false;
					}
				}
				catch
				{
					IsModelImporter = false;
				}
				var strings = AssetDatabase.GetLabels(currentMeshFilter.sharedMesh);

				//if (strings.Length > 0) IsModelImporter = true;
				var Isimportlabel = false;
				foreach(var tempstr in strings)
				{
					if(tempstr == "EditorSculptImported")
					{
						Isimportlabel = true;
						break;
					}
				}
				if(Isimportlabel)
				{
					IsModelImporter = true;
				}

				try
				{
					var meshPath2 = AssetDatabase.GetAssetPath(currentMeshFilter.sharedMesh);
					var importer2 = AssetImporter.GetAtPath(meshPath2);

					//ModelImporter importer3 = (ModelImporter)AssetImporter.GetAtPath(importer2.userData);
					//IsOldAnimationImport = importer3.importAnimation;
					ModelImporterPath = importer2.userData;
				}
				catch
				{
				}
			}
			else if(currentObject.GetComponent<SkinnedMeshRenderer>())
			{
				try
				{
					//string meshPath = AssetDatabase.GetAssetPath(currentSkinned.sharedMesh);
					var meshPath = AssetDatabase.GetAssetPath(currentObject.GetComponent<SkinnedMeshRenderer>().sharedMesh);
					var importer = (ModelImporter)AssetImporter.GetAtPath(meshPath);
					if(importer)
					{
						//if (importer.importAnimation == true)
						//{
						//    //PreimportMetaPath = AssetDatabase.GetTextMetaFilePathFromAssetPath(meshPath);
						//    //PremetaContent = File.ReadAllText(PreimportMetaPath);
						//    //importer.importAnimation = false;
						//    //try
						//    //{
						//    //    AssetDatabase.StartAssetEditing();
						//    //    AssetDatabase.WriteImportSettingsIfDirty(meshPath);
						//    //    //AssetDatabase.Refresh();
						//    //    AssetDatabase.ImportAsset(meshPath);
						//    //}
						//    //finally
						//    //{
						//    //    AssetDatabase.StopAssetEditing();
						//    //}

						//    if (EditorUtility.DisplayDialog("Caution", "Import Animation is on." +
						//        "Editor Sculpt does't work with it on. Do you want to disable that?", "OK", "NO"))
						//    {
						//        importer.importAnimation = false;
						//        try
						//        {
						//            AssetDatabase.StartAssetEditing();
						//            AssetDatabase.WriteImportSettingsIfDirty(meshPath);
						//            //AssetDatabase.Refresh();
						//            AssetDatabase.ImportAsset(meshPath);
						//        }
						//        finally
						//        {
						//            AssetDatabase.StopAssetEditing();
						//        }
						//    }
						//}

						//IsOldAnimationImport = importer.importAnimation;
						//ModelImporterPath = meshPath;

						importMetaPath = AssetDatabase.GetTextMetaFilePathFromAssetPath(meshPath);
						metaContent = File.ReadAllText(importMetaPath);
						importer.importNormals = ModelImporterNormals.Calculate;
						importer.normalCalculationMode = ModelImporterNormalCalculationMode.AngleWeighted;
						importer.normalSmoothingAngle = 60.0f;

						//importSize = importer.fileScale;
						//importSize *= importer.useFileUnits ? 100.0f : 1.0f;
						importSize = 1 / importer.fileScale;
						try
						{
							AssetDatabase.StartAssetEditing();
							AssetDatabase.WriteImportSettingsIfDirty(meshPath);

							//AssetDatabase.Refresh();
							AssetDatabase.ImportAsset(meshPath);
						}
						finally
						{
							AssetDatabase.StopAssetEditing();
						}
						IsModelImporter = true;
						UseModelImporter = true;
					}
					else
					{
						IsModelImporter = false;
					}
				}
				catch
				{
					IsModelImporter = false;
				}
				var strings = AssetDatabase.GetLabels(currentObject.GetComponent<SkinnedMeshRenderer>().sharedMesh);
				var Isimportlabel = false;
				foreach(var tempstr in strings)
				{
					if(tempstr == "EditorSculptImported")
					{
						Isimportlabel = true;
						break;
					}
				}
				if(Isimportlabel)
				{
					IsModelImporter = true;
				}

				try
				{
					var meshPath2 = AssetDatabase.GetAssetPath(currentObject.GetComponent<SkinnedMeshRenderer>().sharedMesh);
					var importer2 = AssetImporter.GetAtPath(meshPath2);

					//ModelImporter importer3 = (ModelImporter)AssetImporter.GetAtPath(importer2.userData);
					//IsOldAnimationImport = importer3.importAnimation;
					ModelImporterPath = importer2.userData;
				}
				catch
				{
				}
			}

			//if(IsModelImporter && importMetaPath.Length>1 && metaContent.Length>1)
			//{
			//    AssetDatabase.ReleaseCachedFileHandles();
			//    File.WriteAllText(importMetaPath, metaContent);
			//    AssetDatabase.Refresh();
			//}
		}

		//private static void FixAndCopyImportedMesh()
		//{
		//    if (currentObject == null) return;
		//    if (currentObject.GetComponent<MeshFilter>())
		//    {
		//    }
		//    else if (currentObject.GetComponent<SkinnedMeshRenderer>())
		//    {
		//        try
		//        {
		//            String meshPath = AssetDatabase.GetAssetPath(currentMesh);
		//            //Debug.Log(meshPath);
		//            ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(meshPath);
		//            String newMeshPath = Path.GetDirectoryName(meshPath) + "/" + Path.GetFileNameWithoutExtension(meshPath)
		//                + "_BackUP" + Path.GetExtension(meshPath);
		//            newMeshPath = AssetDatabase.GenerateUniqueAssetPath(newMeshPath);
		//            AssetDatabase.CopyAsset(meshPath, newMeshPath);

		//            //Debug.Log(newMeshPath);

		//            ModelImporter assetImp = (ModelImporter)AssetImporter.GetAtPath(newMeshPath);
		//            EditorUtility.CopySerialized(importer, assetImp);

		//            assetImp.SaveAndReimport();

		//            //assetImp.AddRemap(new SourceAssetIdentifier(currentMesh), currentMesh);

		//            //Dictionary<SourceAssetIdentifier, UnityEngine.Object> ExternalObjectMap = importer.GetExternalObjectMap();
		//            //SourceAssetIdentifier[] keyArr = ExternalObjectMap.Keys.ToArray();
		//            //Debug.Log("keyArr Length:"+keyArr.Length);
		//            //foreach (SourceAssetIdentifier source in keyArr)
		//            //{
		//            //    Debug.Log(source.name);
		//            //}

		//            //var clone = UnityEngine.Object.Instantiate(importer);
		//            //AssetDatabase.CreateAsset(clone, "Assets/hogehoge.fbx");
		//            //ModelImporter newimp = new ModelImporter();

		//        }
		//        catch { }
		//    }
		//}

		private static void FixMissingImport()
		{
			if(!UseModelImporter)
			{
				return;
			}
			if(!IsFixMissing)
			{
				return;
			}
			var rootobj = currentObject.transform.root.gameObject;
			var childlist = new List<GameObject>();
			var objs = EditorUtility.CollectDeepHierarchy(new[] { rootobj });
			foreach(var obj in objs)
			{
				try
				{
					if(obj.GetType() == typeof(GameObject))
					{
						childlist.Add((GameObject)obj);
					}
				}
				catch
				{
				}
			}
			var IsSkip = false;
			for(var i = 0; i < childlist.Count; i++)
			{
				var child = childlist[i];
				GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child);
				bool IsmissingMesh = false, IsmissingMat = false;
				var chskin = child.GetComponent<SkinnedMeshRenderer>();
				var chmeshf = child.GetComponent<MeshFilter>();
				if(chskin != null)
				{
					IsmissingMesh = chskin.sharedMesh == null;
					IsmissingMat = chskin.sharedMaterials.Length == 0;
				}
				else if(chmeshf != null)
				{
					IsmissingMesh = chmeshf.sharedMesh == null;
					var meshren = child.GetComponent<MeshRenderer>();
					if(meshren != null)
					{
						IsmissingMat = meshren.sharedMaterials.Length == 0;
					}
				}
				if(!IsSkip && (IsmissingMat || IsmissingMesh))
				{
					if(EditorUtility.DisplayDialog("Caution", "Some objects in the model is missing. Do you want to try fix that?", "OK", "Cancel"))
					{
						IsSkip = true;
					}
					else
					{
						return;
					}
				}
				if(IsmissingMesh)
				{
					var meshPath = AssetDatabase.GetAssetPath(currentMesh);
					var assets = AssetDatabase.LoadAllAssetsAtPath(meshPath);
					foreach(var obj in assets)
					{
						if(obj.GetType() == typeof(GameObject))
						{
							var go = (GameObject)obj;
							if(go.name == child.name)
							{
								var goskin = go.GetComponent<SkinnedMeshRenderer>();
								var gomeshf = go.GetComponent<MeshFilter>();
								if(chskin != null)
								{
									if(goskin != null)
									{
										chskin.sharedMesh = goskin.sharedMesh;
										chskin.sharedMaterials = goskin.sharedMaterials;
									}
									else if(gomeshf != null)
									{
										chskin.sharedMesh = gomeshf.sharedMesh;
										var gomeshren = go.GetComponent<MeshRenderer>();
										if(gomeshren != null)
										{
											chskin.sharedMaterials = gomeshren.sharedMaterials;
										}
									}
								}
								else if(chmeshf != null)
								{
									if(goskin != null)
									{
										chmeshf.sharedMesh = goskin.sharedMesh;
										var chmeshren = child.GetComponent<MeshRenderer>();
										if(chmeshren != null)
										{
											chmeshren.sharedMaterials = goskin.sharedMaterials;
										}
									}
									else if(gomeshf != null)
									{
										chmeshf.sharedMesh = gomeshf.sharedMesh;
										var chmeshren = child.GetComponent<MeshRenderer>();
										var gomeshren = go.GetComponent<MeshRenderer>();
										if(gomeshren != null)
										{
											if(chmeshren != null)
											{
												chmeshren.sharedMaterials = gomeshren.sharedMaterials;
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		private static void ResetBindPose()
		{
			if(currentSkinned == null)
			{
				return;
			}
			var bones = currentSkinned.bones;
			var binds = currentMesh.bindposes;
			for(var i = 0; i < bones.Length; i++)
			{
				binds[i] = bones[i].worldToLocalMatrix * currentObject.transform.localToWorldMatrix;
			}
			currentMesh.bindposes = binds;
		}

		private static void ResetBindPose2()
		{
			//AnimationStop();
			//GameObject rootgo = currentObject.transform.root.gameObject;
			//if(currentSkinned!=null)
			//{
			//    if(currentSkinned.rootBone!=null)
			//    {
			//        rootgo = currentSkinned.rootBone.gameObject;
			//    }
			//}
			//GameObject[] bindobjs = GetChildObjects(rootgo);
			//for (int i = 0; i < bindobjs.Length; i++)
			//{
			//    SkinnedMeshRenderer bindskin = bindobjs[i].GetComponent<SkinnedMeshRenderer>();
			//    if (bindskin == null) continue;
			//    Mesh bindmesh = bindskin.sharedMesh;
			//    if (bindmesh == null) continue;
			//    Undo.RegisterCompleteObjectUndo(bindmesh, "RestBindPose" + Undo.GetCurrentGroup());
			//    Transform[] bones = bindskin.bones;
			//    Matrix4x4[] binds = bindmesh.bindposes;
			//    for (int j = 0; j < bones.Length; j++)
			//    {
			//        binds[j] = bones[j].worldToLocalMatrix * bindobjs[i].transform.localToWorldMatrix;
			//    }
			//    bindmesh.bindposes = binds;
			//}
			//if (IsAnimationBrush) AnimationModeStart();

			AnimationStop();
			if(currentSkinned != null && currentMesh != null)
			{
				try
				{
					var bones = currentSkinned.bones;
					var binds = currentMesh.bindposes;
					for(var i = 0; i < bones.Length; i++)
					{
						binds[i] = bones[i].worldToLocalMatrix * currentObject.transform.localToWorldMatrix;
					}
					currentMesh.bindposes = binds;
				}
				catch
				{
				}
			}
			if(IsAnimationBrush)
			{
				AnimationModeStart();
			}
		}

		private static void BuildAvatar()
		{
			if(currentSkinned == null)
			{
				return;
			}
			Animator anim;
			anim = currentObject.transform.root.gameObject.GetComponent<Animator>();
			if(anim == null)
			{
				return;
			}
			if(anim.avatar.isHuman)
			{
				var humand = anim.avatar.humanDescription;
				anim.avatar = AvatarBuilder.BuildHumanAvatar(currentObject.transform.root.gameObject, humand);
			}
			else
			{
				AvatarBuilder.BuildGenericAvatar(currentObject.transform.root.gameObject, currentObject.transform.root.gameObject.name);
			}
		}

		private static void SubdivideStandard(Mesh mesh)
		{
			var vertices = mesh.vertices;
			var triarrlist = new List<int[]>();

			//int subcnt = mesh.subMeshCount;
			var subcnt = IsMonoSubmesh ? 1 : mesh.subMeshCount;
			var colors = mesh.colors;
			if(colors.Length < 1)
			{
				colors = Enumerable.Repeat(Color.white, vertices.Length).ToArray();
			}
			var uv0s = mesh.uv;
			var uv2s = mesh.uv2;
			var uv3s = mesh.uv3;
			var uv4s = mesh.uv4;
			if(uv0s.Length < 1)
			{
				uv0s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv2s.Length < 1)
			{
				uv2s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv3s.Length < 1)
			{
				uv3s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv4s.Length < 1)
			{
				uv4s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			var bscnt = mesh.blendShapeCount;
			var blendarrlist = new List<Vector3[]>();
			var bnamelist = new List<string>();
			var bweightlist = new List<float>();
			var framecntlist = new List<int>();
			if(bscnt > 0)
			{
				for(var i = 0; i < bscnt; i++)
				{
					var fcnt = mesh.GetBlendShapeFrameCount(i);
					framecntlist.Add(fcnt);
					for(var j = 0; j < fcnt; j++)
					{
						var vcnt = mesh.vertexCount;
						var varr = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
						var vnorm = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
						var vtan = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
						mesh.GetBlendShapeFrameVertices(i, j, varr, vnorm, vtan);
						blendarrlist.Add(varr);
						bnamelist.Add(mesh.GetBlendShapeName(i));
						bweightlist.Add(mesh.GetBlendShapeFrameWeight(i, j));
					}
				}
			}
			var blefcnt = blendarrlist.Count;
			var binds = mesh.bindposes;
			var weight1s = currentMesh.GetAllBoneWeights().ToArray();
			var pervers = currentMesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;

			//if (pervers.Length > 0) IsWeight = true;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var pcnt = 0;
			if(IsWeight)
			{
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}

			//if (pervers.Length < 1) pervers = Enumerable.Repeat((Byte)0, vertices.Length).ToArray();
			var subdlist = new List<Vector3>();
			var colorlist = new List<Color>();
			var uv0list = new List<Vector2>();
			var uv2list = new List<Vector2>();
			var uv3list = new List<Vector2>();
			var uv4list = new List<Vector2>();
			var blendlistlist = new List<List<Vector3>>();
			for(var i = 0; i < blefcnt; i++)
			{
				blendlistlist.Add(new List<Vector3>());
			}
			var bonew1list = new List<BoneWeight1>();
			var perverlist = new List<byte>();
			var newtrilist = new List<List<int>>();
			var chkvlist = new List<bool>();
			var cnt0 = 0;
			for(var i = 0; i < subcnt; i++)
			{
				var triarr = mesh.GetTriangles(i);
				var newtri = new List<int>();
				for(var j = 0; j < triarr.Length; j += 3)
				{
					var v0 = vertices[triarr[j]];
					var v1 = vertices[triarr[j + 1]];
					var v2 = vertices[triarr[j + 2]];
					var d0 = Vector3.Distance(v0, v1);
					var d1 = Vector3.Distance(v1, v2);
					var d2 = Vector3.Distance(v2, v0);
					var dc = avgPointDist * 0.5f;
					var cv0 = colors[triarr[j]];
					var cv1 = colors[triarr[j + 1]];
					var cv2 = colors[triarr[j + 2]];
					var uv0_0 = uv0s[triarr[j]];
					var uv0_1 = uv0s[triarr[j + 1]];
					var uv0_2 = uv0s[triarr[j + 2]];
					var uv2_0 = uv2s[triarr[j]];
					var uv2_1 = uv2s[triarr[j + 1]];
					var uv2_2 = uv2s[triarr[j + 2]];
					var uv3_0 = uv0s[triarr[j]];
					var uv3_1 = uv0s[triarr[j + 1]];
					var uv3_2 = uv0s[triarr[j + 2]];
					var uv4_0 = uv4s[triarr[j]];
					var uv4_1 = uv4s[triarr[j + 1]];
					var uv4_2 = uv4s[triarr[j + 2]];
					var pv0 = IsWeight ? pervers[triarr[j]] : 0;
					var pv1 = IsWeight ? pervers[triarr[j + 1]] : 0;
					var pv2 = IsWeight ? pervers[triarr[j + 2]] : 0;
					var bwlist0 = new List<BoneWeight1>();
					if(pv0 > 0)
					{
						for(var k = 0; k < pv0 - 1; k++)
						{
							bwlist0.Add(weight1s[boneidxs[triarr[j]] + k]);
						}
					}
					var bwlist1 = new List<BoneWeight1>();
					if(pv1 > 0)
					{
						for(var k = 0; k < pv1 - 1; k++)
						{
							bwlist1.Add(weight1s[boneidxs[triarr[j + 1]] + k]);
						}
					}
					var bwlist2 = new List<BoneWeight1>();
					if(pv2 > 0)
					{
						for(var k = 0; k < pv2 - 1; k++)
						{
							bwlist2.Add(weight1s[boneidxs[triarr[j + 2]] + k]);
						}
					}
					var bi0 = (byte)bwlist0.Count;
					var bi1 = (byte)bwlist1.Count;
					var bi2 = (byte)bwlist2.Count;
					bool flag0 = false, flag1 = false, flag2 = false;
					if(d0 > d1 && d0 > d2)
					{
						var p0 = v2 + d2 / (d2 + d0) * (v1 - v2);
						var dp0 = Vector3.Distance(v0, p0);
						if(d0 > dc || d0 > dp0 * 5.0f)
						{
							flag0 = true;
						}
					}
					else if(d1 > d0 && d1 > d2)
					{
						var p1 = v0 + d0 / (d0 + d1) * (v2 - v0);
						var dp1 = Vector3.Distance(v1, p1);
						if(d1 > dc || d1 > dp1 * 5.0f)
						{
							flag1 = true;
						}
					}
					else if(d2 > d0 && d2 > d1)
					{
						var p2 = v1 + d1 / (d1 + d2) * (v0 - v1);
						var dp2 = Vector3.Distance(v2, p2);
						if(d2 > dc || d2 > dp2 * 5.0f)
						{
							flag2 = true;
						}
					}

					if(uv4_0.x < maskWeight || uv4_1.x < maskWeight || uv4_2.x < maskWeight)
					{
						flag0 = false;
						flag1 = false;
						flag2 = false;
					}
					if(currentStatus == SculptStatus.Active && IsBrushedArray[triarr[j]] == false
					                                        && IsBrushedArray[triarr[j + 1]] == false && IsBrushedArray[triarr[j + 2]] == false)
					{
						flag0 = false;
						flag1 = false;
						flag2 = false;
					}

					//cnt0 = newtri.Count;
					cnt0 = subdlist.Count;
					if(flag0)
					{
						var e0 = (v0 + v1) * 0.5f;
						var cve0 = Color.Lerp(cv0, cv1, 0.5f);
						var uve0_0 = Vector2.Lerp(uv0_0, uv0_1, 0.5f);
						var uve2_0 = Vector2.Lerp(uv2_0, uv2_1, 0.5f);
						var uve3_0 = Vector2.Lerp(uv3_0, uv3_1, 0.5f);
						var uve4_0 = Vector2.Lerp(uv4_0, uv4_1, 0.5f);
						subdlist.AddRange(new[] { v0, e0, v2, e0, v1, v2 });
						colorlist.AddRange(new[] { cv0, cve0, cv2, cve0, cv1, cv2 });
						uv0list.AddRange(new[] { uv0_0, uve0_0, uv0_2, uve0_0, uv0_1, uv0_2 });
						uv2list.AddRange(new[] { uv2_0, uve2_0, uv2_2, uve2_0, uv2_1, uv2_2 });
						uv3list.AddRange(new[] { uv3_0, uve3_0, uv3_2, uve3_0, uv3_1, uv3_2 });
						uv4list.AddRange(new[] { uv4_0, uve4_0, uv4_2, uve4_0, uv4_1, uv4_2 });
						if(blefcnt > 0)
						{
							for(var k = 0; k < blefcnt; k++)
							{
								var b0 = blendarrlist[k][triarr[j]];
								var b1 = blendarrlist[k][triarr[j + 1]];
								var b2 = blendarrlist[k][triarr[j + 2]];
								var be0 = (b0 + b1) * 0.5f;
								blendlistlist[k].AddRange(new[] { b0, be0, b2, be0, b1, b2 });
							}
						}
						if(IsWeight)
						{
							var bwliste0 = BoneWeight1Lerp(bwlist0, bwlist1);
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwliste0);
							bonew1list.AddRange(bwlist2);
							bonew1list.AddRange(bwliste0);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwlist2);
							perverlist.AddRange(new[] { bi0, (byte)bwliste0.Count, bi2, (byte)bwliste0.Count, bi1, bi2 });
						}
						newtri.AddRange(new[] { cnt0, cnt0 + 1, cnt0 + 2, cnt0 + 3, cnt0 + 4, cnt0 + 5 });
						chkvlist.AddRange(new[] { true, true, true, true, true, true });

						//foreach (Vector3 vec in new Vector3[] { v0, v1, v2, e0 }) chkvdict[vec] = true;
					}
					else if(flag1)
					{
						var e1 = (v1 + v2) * 0.5f;
						var cve1 = Color.Lerp(cv1, cv2, 0.5f);
						var uve0_1 = Vector2.Lerp(uv0_1, uv0_2, 0.5f);
						var uve2_1 = Vector2.Lerp(uv2_1, uv2_2, 0.5f);
						var uve3_1 = Vector2.Lerp(uv3_1, uv3_2, 0.5f);
						var uve4_1 = Vector2.Lerp(uv4_1, uv4_2, 05f);
						subdlist.AddRange(new[] { v0, v1, e1, v0, e1, v2 });
						colorlist.AddRange(new[] { cv0, cv1, cve1, cv0, cve1, cv2 });
						uv0list.AddRange(new[] { uv0_0, uv0_1, uve0_1, uv0_0, uve0_1, uv0_2 });
						uv2list.AddRange(new[] { uv2_0, uv2_1, uve2_1, uv2_0, uve2_1, uv2_2 });
						uv3list.AddRange(new[] { uv3_0, uv3_1, uve3_1, uv3_0, uve3_1, uv3_2 });
						uv4list.AddRange(new[] { uv4_0, uv4_1, uve4_1, uv4_0, uve4_1, uv4_2 });
						if(blefcnt > 0)
						{
							for(var k = 0; k < blefcnt; k++)
							{
								var b0 = blendarrlist[k][triarr[j]];
								var b1 = blendarrlist[k][triarr[j + 1]];
								var b2 = blendarrlist[k][triarr[j + 2]];
								var be1 = (b1 + b2) * 0.5f;
								blendlistlist[k].AddRange(new[] { b0, b1, be1, b0, be1, b2 });
							}
						}
						if(IsWeight)
						{
							var bwliste1 = BoneWeight1Lerp(bwlist1, bwlist2);
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwliste1);
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwliste1);
							bonew1list.AddRange(bwlist2);
							perverlist.AddRange(new[] { bi0, bi1, (byte)bwliste1.Count, bi0, (byte)bwliste1.Count, bi2 });
						}
						newtri.AddRange(new[] { cnt0, cnt0 + 1, cnt0 + 2, cnt0 + 3, cnt0 + 4, cnt0 + 5 });
						chkvlist.AddRange(new[] { true, true, true, true, true, true });

						//foreach (Vector3 vec in new Vector3[] { v0, v1, v2, e1 }) chkvdict[vec] = true;
					}
					else if(flag2)
					{
						var e2 = (v2 + v0) * 0.5f;
						var cve2 = Color.Lerp(cv2, cv0, 0.5f);
						var uve0_2 = Vector2.Lerp(uv0_2, uv0_0, 0.5f);
						var uve2_2 = Vector2.Lerp(uv2_2, uv2_0, 0.5f);
						var uve3_2 = Vector2.Lerp(uv3_2, uv3_0, 0.5f);
						var uve4_2 = Vector2.Lerp(uv4_2, uv4_0, 0.5f);
						subdlist.AddRange(new[] { v0, v1, e2, e2, v1, v2 });
						colorlist.AddRange(new[] { cv0, cv1, cve2, cve2, cv1, cv2 });
						uv0list.AddRange(new[] { uv0_0, uv0_1, uve0_2, uve0_2, uv0_1, uv0_2 });
						uv2list.AddRange(new[] { uv2_0, uv2_1, uve2_2, uve2_2, uv2_1, uv2_2 });
						uv3list.AddRange(new[] { uv3_0, uv3_1, uve3_2, uve3_2, uv3_1, uv3_2 });
						uv4list.AddRange(new[] { uv4_0, uv4_1, uve4_2, uve4_2, uv4_1, uv4_2 });
						if(blefcnt > 0)
						{
							for(var k = 0; k < blefcnt; k++)
							{
								var b0 = blendarrlist[k][triarr[j]];
								var b1 = blendarrlist[k][triarr[j + 1]];
								var b2 = blendarrlist[k][triarr[j + 2]];
								var be2 = (b2 + b0) * 0.5f;
								blendlistlist[k].AddRange(new[] { b0, b1, be2, be2, b1, b2 });
							}
						}
						if(IsWeight)
						{
							var bwliste2 = BoneWeight1Lerp(bwlist2, bwlist0);
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwliste2);
							bonew1list.AddRange(bwliste2);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwlist2);
							perverlist.AddRange(new[] { bi0, bi1, (byte)bwliste2.Count, (byte)bwliste2.Count, bi1, bi2 });
						}
						newtri.AddRange(new[] { cnt0, cnt0 + 1, cnt0 + 2, cnt0 + 3, cnt0 + 4, cnt0 + 5 });
						chkvlist.AddRange(new[] { true, true, true, true, true, true });

						//foreach (Vector3 vec in new Vector3[] { v0, v1, v2, e2 }) chkvdict[vec] = true;
					}
					else
					{
						subdlist.AddRange(new[] { v0, v1, v2 });
						colorlist.AddRange(new[] { cv0, cv1, cv2 });
						uv0list.AddRange(new[] { uv0_0, uv0_1, uv0_2 });
						uv2list.AddRange(new[] { uv2_0, uv2_1, uv2_2 });
						uv3list.AddRange(new[] { uv3_0, uv3_1, uv3_2 });
						uv4list.AddRange(new[] { uv4_0, uv4_1, uv4_2 });
						if(blefcnt > 0)
						{
							for(var k = 0; k < blefcnt; k++)
							{
								var b0 = blendarrlist[k][triarr[j]];
								var b1 = blendarrlist[k][triarr[j + 1]];
								var b2 = blendarrlist[k][triarr[j + 2]];
								blendlistlist[k].AddRange(new[] { b0, b1, b2 });
							}
						}
						if(IsWeight)
						{
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwlist2);
							perverlist.AddRange(new[] { bi0, bi1, bi2 });
						}
						newtri.AddRange(new[] { cnt0, cnt0 + 1, cnt0 + 2 });
						chkvlist.AddRange(new[] { false, false, false });

						//foreach (Vector3 vec in new Vector3[] { v0, v1, v2 }) chkvdict[vec] = true;
					}
				}
				newtrilist.Add(newtri);
			}
			triarrlist = new List<int[]>();
			var calcarr = new int[subdlist.Count];
			vertices = subdlist.ToArray();
			for(var i = 0; i < newtrilist.Count; i++)
			{
				triarrlist.Add(newtrilist[i].ToArray());
			}
			colors = colorlist.ToArray();
			uv0s = uv0list.ToArray();
			uv2s = uv2list.ToArray();
			uv3s = uv3list.ToArray();
			uv4s = uv4list.ToArray();
			if(blefcnt > 0)
			{
				for(var i = 0; i < blefcnt; i++)
				{
					blendarrlist[i] = blendlistlist[i].ToArray();
				}
			}
			weight1s = bonew1list.ToArray();
			pervers = perverlist.ToArray();
			if(IsWeight)
			{
				boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
				pcnt = 0;
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			newtrilist = new List<List<int>>();
			cnt0 = 0;

			var newvlist = new List<Vector3>();
			var newcolorlist = new List<Color>();
			var newuv0list = new List<Vector2>();
			var newuv2list = new List<Vector2>();
			var newuv3list = new List<Vector2>();
			var newuv4list = new List<Vector2>();
			var newperverlist = new List<byte>();
			var newbwlist = new List<BoneWeight1>();
			var w1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
			var newblendlistlist = new List<List<Vector3>>();
			for(var i = 0; i < blefcnt; i++)
			{
				newblendlistlist.Add(new List<Vector3>());
			}
			var vertdict = new Dictionary<Vector3, int>();
			var subddict = new Dictionary<Vector4, int>();
			for(var i = 0; i < vertices.Length; i++)
			{
				vertdict[vertices[i]] = i;
			}
			for(var i = 0; i < subcnt; i++)
			{
				subdlist = new List<Vector3>();
				colorlist = new List<Color>();
				uv0list = new List<Vector2>();
				uv2list = new List<Vector2>();
				uv3list = new List<Vector2>();
				uv4list = new List<Vector2>();
				bonew1list = new List<BoneWeight1>();
				perverlist = new List<byte>();
				blendlistlist = new List<List<Vector3>>();
				for(var j = 0; j < blefcnt; j++)
				{
					blendlistlist.Add(new List<Vector3>());
				}
				var triangles = triarrlist[i].ToArray();

				//List<int> newtri = triangles.ToList();
				var newtri = new List<int>();
				for(var j = 0; j < triangles.Length; j += 3)
				{
					var v0 = vertices[triangles[j]];
					var v1 = vertices[triangles[j + 1]];
					var v2 = vertices[triangles[j + 2]];
					var q0 = (v0 + v1) * 0.5f;
					var q1 = (v1 + v2) * 0.5f;
					var q2 = (v2 + v0) * 0.5f;
					var cv0 = colors[triangles[j]];
					var cv1 = colors[triangles[j + 1]];
					var cv2 = colors[triangles[j + 2]];
					var uv0_0 = uv0s[triangles[j]];
					var uv0_1 = uv0s[triangles[j + 1]];
					var uv0_2 = uv0s[triangles[j + 2]];
					var uv2_0 = uv2s[triangles[j]];
					var uv2_1 = uv2s[triangles[j + 1]];
					var uv2_2 = uv2s[triangles[j + 2]];
					var uv3_0 = uv3s[triangles[j]];
					var uv3_1 = uv3s[triangles[j + 1]];
					var uv3_2 = uv3s[triangles[j + 2]];
					var uv4_0 = uv4s[triangles[j]];
					var uv4_1 = uv4s[triangles[j + 1]];
					var uv4_2 = uv4s[triangles[j + 2]];
					var pv0 = IsWeight ? pervers[triangles[j]] : 0;
					var pv1 = IsWeight ? pervers[triangles[j + 1]] : 0;
					var pv2 = IsWeight ? pervers[triangles[j + 2]] : 0;
					var bwlist0 = new List<BoneWeight1>();
					if(pv0 > 0)
					{
						for(var k = 0; k < pv0 - 1; k++)
						{
							try
							{
								bwlist0.Add(weight1s[boneidxs[triangles[j]] + k]);
							}
							catch
							{
								bwlist0.Add(w1);
							}
						}
					}
					if(bwlist0.Count < 1)
					{
						bwlist0.Add(w1);
					}
					var bwlist1 = new List<BoneWeight1>();
					if(pv1 > 0)
					{
						for(var k = 0; k < pv1 - 1; k++)
						{
							try
							{
								bwlist1.Add(weight1s[boneidxs[triangles[j + 1]] + k]);
							}
							catch
							{
								bwlist1.Add(w1);
							}
						}
					}
					if(bwlist1.Count < 1)
					{
						bwlist1.Add(w1);
					}
					var bwlist2 = new List<BoneWeight1>();
					if(pv2 > 0)
					{
						for(var k = 0; k < pv2 - 1; k++)
						{
							try
							{
								bwlist2.Add(weight1s[boneidxs[triangles[j + 2]] + k]);
							}
							catch
							{
								bwlist2.Add(w1);
							}
						}
					}
					if(bwlist2.Count < 1)
					{
						bwlist2.Add(w1);
					}
					var bi0 = (byte)bwlist0.Count;
					var bi1 = (byte)bwlist1.Count;
					var bi2 = (byte)bwlist2.Count;
					bool flag0 = false, flag1 = false, flag2 = false;
					if(vertdict.ContainsKey(q0))
					{
						flag0 = true;
					}
					if(vertdict.ContainsKey(q1))
					{
						flag1 = true;
					}
					if(vertdict.ContainsKey(q2))
					{
						flag2 = true;
					}
					cnt0 = newtri.Count;

					//cnt0 += newtri.Count;
					if(flag0 && !flag1 && !flag2)
					{
						var cvq0 = Color.Lerp(cv0, cv1, 0.5f);
						var uvq0_0 = Vector2.Lerp(uv0_0, uv0_1, 0.5f);
						var uvq2_0 = Vector2.Lerp(uv2_0, uv2_1, 0.5f);
						var uvq3_0 = Vector2.Lerp(uv3_0, uv3_1, 0.5f);
						var uvq4_0 = Vector2.Lerp(uv4_0, uv4_1, 0.5f);
						subdlist.AddRange(new[] { v0, q0, v2, q0, v1, v2 });
						colorlist.AddRange(new[] { cv0, cvq0, cv2, cvq0, cv1, cv2 });
						uv0list.AddRange(new[] { uv0_0, uvq0_0, uv0_2, uvq0_0, uv0_1, uv0_2 });
						uv2list.AddRange(new[] { uv2_0, uvq2_0, uv2_2, uvq2_0, uv2_1, uv2_2 });
						uv3list.AddRange(new[] { uv3_0, uvq3_0, uv3_2, uvq3_0, uv3_1, uv3_2 });
						uv4list.AddRange(new[] { uv4_0, uvq4_0, uv4_2, uvq4_0, uv4_1, uv4_2 });
						if(blefcnt > 0)
						{
							for(var k = 0; k < blefcnt; k++)
							{
								var b0 = blendarrlist[k][triangles[j]];
								var b1 = blendarrlist[k][triangles[j + 1]];
								var b2 = blendarrlist[k][triangles[j + 2]];
								var bq0 = (b0 + b1) * 0.5f;
								blendlistlist[k].AddRange(new[] { b0, bq0, b2, bq0, b1, b2 });
							}
						}
						if(IsWeight)
						{
							var bwlistq0 = BoneWeight1Lerp(bwlist0, bwlist1);
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlistq0);
							bonew1list.AddRange(bwlist2);
							bonew1list.AddRange(bwlistq0);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwlist2);
							perverlist.AddRange(new[] { bi0, (byte)bwlistq0.Count, bi2, (byte)bwlistq0.Count, bi1, bi2 });
						}
						newtri.AddRange(new[] { cnt0, cnt0 + 1, cnt0 + 2, cnt0 + 3, cnt0 + 4, cnt0 + 5 });

						//foreach (Vector3 vec in new Vector3[] { v0, v1, v2, q0 }) chkvdict[vec] = true;
					}
					else if(flag1 && !flag0 && !flag2)
					{
						var cvq1 = Color.Lerp(cv1, cv2, 0.5f);
						var uvq0_1 = Vector2.Lerp(uv0_1, uv0_2, 0.5f);
						var uvq2_1 = Vector2.Lerp(uv2_1, uv2_2, 0.5f);
						var uvq3_1 = Vector2.Lerp(uv3_1, uv3_2, 0.5f);
						var uvq4_1 = Vector2.Lerp(uv4_1, uv4_2, 0.5f);
						subdlist.AddRange(new[] { v0, v1, q1, v0, q1, v2 });
						colorlist.AddRange(new[] { cv0, cv1, cvq1, cv0, cvq1, cv2 });
						uv0list.AddRange(new[] { uv0_0, uv0_1, uvq0_1, uv0_0, uvq0_1, uv0_2 });
						uv2list.AddRange(new[] { uv2_0, uv2_1, uvq2_1, uv2_0, uvq2_1, uv2_2 });
						uv3list.AddRange(new[] { uv3_0, uv3_1, uvq3_1, uv3_0, uvq3_1, uv3_2 });
						uv4list.AddRange(new[] { uv4_0, uv4_1, uvq4_1, uv4_0, uvq4_1, uv4_2 });
						if(blefcnt > 0)
						{
							for(var k = 0; k < blefcnt; k++)
							{
								var b0 = blendarrlist[k][triangles[j]];
								var b1 = blendarrlist[k][triangles[j + 1]];
								var b2 = blendarrlist[k][triangles[j + 2]];
								var bq1 = (b1 + b2) * 0.5f;
								blendlistlist[k].AddRange(new[] { b0, b1, bq1, b0, bq1, b2 });
							}
						}
						if(IsWeight)
						{
							var bwlistq1 = BoneWeight1Lerp(bwlist1, bwlist2);
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwlistq1);
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlistq1);
							bonew1list.AddRange(bwlist2);
							perverlist.AddRange(new[] { bi0, bi1, (byte)bwlistq1.Count, bi0, (byte)bwlistq1.Count, bi2 });
						}
						newtri.AddRange(new[] { cnt0, cnt0 + 1, cnt0 + 2, cnt0 + 3, cnt0 + 4, cnt0 + 5 });

						//foreach (Vector3 vec in new Vector3[] { v0, v1, v2, q1 }) chkvdict[vec] = true;
					}
					else if(flag2 && !flag0 && !flag1)
					{
						var cvq2 = Color.Lerp(cv2, cv0, 0.5f);
						var uvq0_2 = Vector2.Lerp(uv0_2, uv0_0, 0.5f);
						var uvq2_2 = Vector2.Lerp(uv2_2, uv2_0, 0.5f);
						var uvq3_2 = Vector2.Lerp(uv3_2, uv3_0, 0.5f);
						var uvq4_2 = Vector2.Lerp(uv4_2, uv4_0, 0.5f);
						subdlist.AddRange(new[] { v0, v1, q2, q2, v1, v2 });
						colorlist.AddRange(new[] { cv0, cv1, cvq2, cvq2, cv1, cv2 });
						uv0list.AddRange(new[] { uv0_0, uv0_1, uvq0_2, uvq0_2, uv0_1, uv0_2 });
						uv2list.AddRange(new[] { uv2_0, uv2_1, uvq2_2, uvq2_2, uv2_1, uv2_2 });
						uv3list.AddRange(new[] { uv3_0, uv3_1, uvq3_2, uvq3_2, uv3_1, uv3_2 });
						uv4list.AddRange(new[] { uv4_0, uv4_1, uvq4_2, uvq4_2, uv4_1, uv4_2 });
						if(blefcnt > 0)
						{
							for(var k = 0; k < blefcnt; k++)
							{
								var b0 = blendarrlist[k][triangles[j]];
								var b1 = blendarrlist[k][triangles[j + 1]];
								var b2 = blendarrlist[k][triangles[j + 2]];
								var bq2 = (b2 + b0) * 0.5f;
								blendlistlist[k].AddRange(new[] { b0, b1, bq2, bq2, b1, b2 });
							}
						}
						if(IsWeight)
						{
							var bwlistq2 = BoneWeight1Lerp(bwlist2, bwlist0);
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwlistq2);
							bonew1list.AddRange(bwlistq2);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwlist2);
							perverlist.AddRange(new[] { bi0, bi1, (byte)bwlistq2.Count, (byte)bwlistq2.Count, bi1, bi2 });
						}
						newtri.AddRange(new[] { cnt0, cnt0 + 1, cnt0 + 2, cnt0 + 3, cnt0 + 4, cnt0 + 5 });

						//foreach (Vector3 vec in new Vector3[] { v0, v1, v2, q2 }) chkvdict[vec] = true;
					}
					else if(flag0 && flag1 && !flag2)
					{
						var cvq0 = Color.Lerp(cv0, cv1, 0.5f);
						var cvq1 = Color.Lerp(cv1, cv2, 0.5f);
						var uvq0_0 = Vector2.Lerp(uv0_0, uv0_1, 0.5f);
						var uvq0_1 = Vector2.Lerp(uv0_1, uv0_2, 0.5f);
						var uvq2_0 = Vector2.Lerp(uv2_0, uv2_1, 0.5f);
						var uvq2_1 = Vector2.Lerp(uv2_1, uv2_2, 0.5f);
						var uvq3_0 = Vector2.Lerp(uv3_0, uv3_1, 0.5f);
						var uvq3_1 = Vector2.Lerp(uv3_1, uv3_2, 0.5f);
						var uvq4_0 = Vector2.Lerp(uv4_0, uv4_1, 0.5f);
						var uvq4_1 = Vector2.Lerp(uv4_1, uv4_2, 0.5f);
						for(var k = 0; k < 9; k++)
						{
							newtri.Add(cnt0 + k);
						}

						//foreach (Vector3 vec in new Vector3[] { v0, v1, v2, c0, q0, q1 }) chkvdict[vec] = true;
						subdlist.AddRange(new[] { v1, q1, q0, v0, q0, q1, v0, q1, v2 });
						colorlist.AddRange(new[] { cv1, cvq1, cvq0, cv0, cvq0, cvq1, cv0, cvq1, cv2 });
						uv0list.AddRange(new[] { uv0_1, uvq0_1, uvq0_0, uv0_0, uvq0_0, uvq0_1, uv0_0, uvq0_1, uv0_2 });
						uv2list.AddRange(new[] { uv2_1, uvq2_1, uvq2_0, uv2_0, uvq2_0, uvq2_1, uv2_0, uvq2_1, uv2_2 });
						uv3list.AddRange(new[] { uv3_1, uvq3_1, uvq3_0, uv3_0, uvq3_0, uvq3_1, uv3_0, uvq3_1, uv3_2 });
						uv4list.AddRange(new[] { uv4_1, uvq4_1, uvq4_0, uv4_0, uvq4_0, uvq4_1, uv4_0, uvq4_1, uv4_2 });
						if(blefcnt > 0)
						{
							for(var k = 0; k < blefcnt; k++)
							{
								var b0 = blendarrlist[k][triangles[j]];
								var b1 = blendarrlist[k][triangles[j + 1]];
								var b2 = blendarrlist[k][triangles[j + 2]];
								var bq0 = (b0 + b1) * 0.5f;
								var bq1 = (b1 + b2) * 0.5f;
								blendlistlist[k].AddRange(new[] { b1, bq1, bq0, b0, bq0, bq1, b0, bq1, b2 });
							}
						}
						if(IsWeight)
						{
							var bwlistq0 = BoneWeight1Lerp(bwlist0, bwlist1);
							var bwlistq1 = BoneWeight1Lerp(bwlist1, bwlist2);
							var biq0 = (byte)bwlistq0.Count;
							var biq1 = (byte)bwlistq1.Count;
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwlistq1);
							bonew1list.AddRange(bwlistq0);
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlistq0);
							bonew1list.AddRange(bwlistq1);
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlistq1);
							bonew1list.AddRange(bwlist2);
							perverlist.AddRange(new[] { bi1, biq1, biq0, bi0, biq0, biq1, bi0, biq1, bi2 });
						}
					}
					else if(flag1 && flag2 && !flag0)
					{
						var cvq1 = Color.Lerp(cv1, cv2, 0.5f);
						var cvq2 = Color.Lerp(cv2, cv0, 0.5f);
						var uvq0_1 = Vector2.Lerp(uv0_1, uv0_2, 0.5f);
						var uvq0_2 = Vector2.Lerp(uv0_2, uv0_0, 0.5f);
						var uvq2_1 = Vector2.Lerp(uv2_1, uv2_2, 0.5f);
						var uvq2_2 = Vector2.Lerp(uv2_2, uv2_0, 0.5f);
						var uvq3_1 = Vector2.Lerp(uv3_1, uv3_2, 0.5f);
						var uvq3_2 = Vector2.Lerp(uv3_2, uv3_0, 0.5f);
						var uvq4_1 = Vector2.Lerp(uv4_1, uv4_2, 0.5f);
						var uvq4_2 = Vector2.Lerp(uv4_2, uv4_0, 0.5f);
						for(var k = 0; k < 9; k++)
						{
							newtri.Add(cnt0 + k);
						}

						//foreach (Vector3 vec in new Vector3[] { v0, v1, v2, c0, q1, q2 }) chkvdict[vec] = true;
						subdlist.AddRange(new[] { v0, v1, q1, v0, q1, q2, q2, q1, v2 });
						colorlist.AddRange(new[] { cv0, cv1, cvq1, cv0, cvq1, cvq2, cvq2, cvq1, cv2 });
						uv0list.AddRange(new[] { uv0_0, uv0_1, uvq0_1, uv0_0, uvq0_1, uvq0_2, uvq0_2, uvq0_1, uv0_2 });
						uv2list.AddRange(new[] { uv2_0, uv2_1, uvq2_1, uv2_0, uvq2_1, uvq2_2, uvq2_2, uvq2_1, uv2_2 });
						uv3list.AddRange(new[] { uv3_0, uv3_1, uvq3_1, uv3_0, uvq3_1, uvq3_2, uvq3_2, uvq3_1, uv3_2 });
						uv4list.AddRange(new[] { uv4_0, uv4_1, uvq4_1, uv4_0, uvq4_1, uvq4_2, uvq4_2, uvq4_1, uv4_2 });
						if(blefcnt > 0)
						{
							for(var k = 0; k < blefcnt; k++)
							{
								var b0 = blendarrlist[k][triangles[j]];
								var b1 = blendarrlist[k][triangles[j + 1]];
								var b2 = blendarrlist[k][triangles[j + 2]];
								var bq1 = (b1 + b2) * 0.5f;
								var bq2 = (b2 + b0) * 0.5f;
								blendlistlist[k].AddRange(new[] { b0, b1, bq1, b0, bq1, bq2, bq2, bq1, b2 });
							}
						}
						if(IsWeight)
						{
							var bwlistq1 = BoneWeight1Lerp(bwlist1, bwlist2);
							var bwlistq2 = BoneWeight1Lerp(bwlist2, bwlist0);
							var biq1 = (byte)bwlistq1.Count;
							var biq2 = (byte)bwlistq2.Count;
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwlistq1);
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlistq1);
							bonew1list.AddRange(bwlistq2);
							bonew1list.AddRange(bwlistq2);
							bonew1list.AddRange(bwlistq1);
							bonew1list.AddRange(bwlist2);
							perverlist.AddRange(new[] { bi0, bi1, biq1, bi0, biq1, biq2, biq2, biq1, bi2 });
						}
					}
					else if(flag2 && flag0 && !flag1)
					{
						var cvq0 = Color.Lerp(cv0, cv1, 0.5f);
						var cvq2 = Color.Lerp(cv2, cv0, 0.5f);
						var uvq0_0 = Vector2.Lerp(uv0_0, uv0_1, 0.5f);
						var uvq0_2 = Vector2.Lerp(uv0_2, uv0_0, 0.5f);
						var uvq2_0 = Vector2.Lerp(uv2_0, uv2_1, 0.5f);
						var uvq2_2 = Vector2.Lerp(uv2_2, uv2_0, 0.5f);
						var uvq3_0 = Vector2.Lerp(uv3_0, uv3_1, 0.5f);
						var uvq3_2 = Vector2.Lerp(uv3_2, uv3_0, 0.5f);
						var uvq4_0 = Vector2.Lerp(uv4_0, uv4_1, 0.5f);
						var uvq4_2 = Vector2.Lerp(uv4_2, uv4_0, 0.5f);
						for(var k = 0; k < 9; k++)
						{
							newtri.Add(cnt0 + k);
						}

						//foreach (Vector3 vec in new Vector3[] { v0, v1, v2, c0, q0, q2 }) chkvdict[vec] = true;
						subdlist.AddRange(new[] { v0, q0, q2, q2, q0, v1, q2, v1, v2 });
						colorlist.AddRange(new[] { cv0, cvq0, cvq2, cvq2, cvq0, cv1, cvq2, cv1, cv2 });
						uv0list.AddRange(new[] { uv0_0, uvq0_0, uvq0_2, uvq0_2, uvq0_0, uv0_1, uvq0_2, uv0_1, uv0_2 });
						uv2list.AddRange(new[] { uv2_0, uvq2_0, uvq2_2, uvq2_2, uvq2_0, uv2_1, uvq2_2, uv2_1, uv2_2 });
						uv3list.AddRange(new[] { uv3_0, uvq3_0, uvq3_2, uvq3_2, uvq3_0, uv3_1, uvq3_2, uv3_1, uv3_2 });
						uv4list.AddRange(new[] { uv4_0, uvq4_0, uvq4_2, uvq4_2, uvq4_0, uv4_1, uvq4_2, uv4_1, uv4_2 });
						if(blefcnt > 0)
						{
							for(var k = 0; k < blefcnt; k++)
							{
								var b0 = blendarrlist[k][triangles[j]];
								var b1 = blendarrlist[k][triangles[j + 1]];
								var b2 = blendarrlist[k][triangles[j + 2]];
								var bq0 = (b0 + b1) * 0.5f;
								var bq2 = (b2 + b0) * 0.5f;
								blendlistlist[k].AddRange(new[] { b0, bq0, bq2, bq2, bq0, b1, bq2, b1, b2 });
							}
						}
						if(IsWeight)
						{
							var bwlistq0 = BoneWeight1Lerp(bwlist0, bwlist1);
							var bwlistq2 = BoneWeight1Lerp(bwlist2, bwlist0);
							var biq0 = (byte)bwlistq0.Count;
							var biq2 = (byte)bwlistq2.Count;
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlistq0);
							bonew1list.AddRange(bwlistq2);
							bonew1list.AddRange(bwlistq2);
							bonew1list.AddRange(bwlistq0);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwlistq2);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwlist2);
							perverlist.AddRange(new[] { bi0, biq0, biq2, biq2, biq0, bi1, biq2, bi1, bi2 });
						}
					}
					else if(flag0 && flag1 && flag2)
					{
						var cvq0 = Color.Lerp(cv0, cv1, 0.5f);
						var cvq1 = Color.Lerp(cv1, cv2, 0.5f);
						var cvq2 = Color.Lerp(cv2, cv0, 0.5f);
						var uvq0_0 = Vector2.Lerp(uv0_0, uv0_1, 0.5f);
						var uvq0_1 = Vector2.Lerp(uv0_1, uv0_2, 0.5f);
						var uvq0_2 = Vector2.Lerp(uv0_2, uv0_0, 0.5f);
						var uvq2_0 = Vector2.Lerp(uv2_0, uv2_1, 0.5f);
						var uvq2_1 = Vector2.Lerp(uv2_1, uv2_2, 0.5f);
						var uvq2_2 = Vector2.Lerp(uv2_2, uv2_0, 0.5f);
						var uvq3_0 = Vector2.Lerp(uv3_0, uv3_1, 0.5f);
						var uvq3_1 = Vector2.Lerp(uv3_1, uv3_2, 0.5f);
						var uvq3_2 = Vector2.Lerp(uv3_2, uv3_0, 0.5f);
						var uvq4_0 = Vector2.Lerp(uv4_0, uv4_1, 0.5f);
						var uvq4_1 = Vector2.Lerp(uv4_1, uv4_2, 0.5f);
						var uvq4_2 = Vector2.Lerp(uv4_2, uv4_0, 0.5f);
						for(var k = 0; k < 12; k++)
						{
							newtri.Add(cnt0 + k);
						}

						//foreach (Vector3 vec in new Vector3[] { v0, v1, v2, c0, q0, q1, q2 }) chkvdict[vec] = true;
						subdlist.AddRange(new[] { v0, q0, q2, q2, q1, v2, q0, v1, q1, q0, q1, q2 });
						colorlist.AddRange(new[] { cv0, cvq0, cvq2, cvq2, cvq1, cv2, cvq0, cv1, cvq1, cvq0, cvq1, cvq2 });
						uv0list.AddRange(new[] { uv0_0, uvq0_0, uvq0_2, uvq0_2, uvq0_1, uv0_2, uvq0_0, uv0_1, uvq0_1, uvq0_0, uvq0_1, uvq0_2 });
						uv2list.AddRange(new[] { uv2_0, uvq2_0, uvq2_2, uvq2_2, uvq2_1, uv2_2, uvq2_0, uv2_1, uvq2_1, uvq2_0, uvq2_1, uvq2_2 });
						uv3list.AddRange(new[] { uv3_0, uvq3_0, uvq3_2, uvq3_2, uvq3_1, uv3_2, uvq3_0, uv3_1, uvq3_1, uvq3_0, uvq3_1, uvq3_2 });
						uv4list.AddRange(new[] { uv4_0, uvq4_0, uvq4_2, uvq4_2, uvq4_1, uv4_2, uvq4_0, uv4_1, uvq4_1, uvq4_0, uvq4_1, uvq4_2 });
						if(blefcnt > 0)
						{
							for(var k = 0; k < blefcnt; k++)
							{
								var b0 = blendarrlist[k][triangles[j]];
								var b1 = blendarrlist[k][triangles[j + 1]];
								var b2 = blendarrlist[k][triangles[j + 2]];
								var bq0 = (b0 + b1) * 0.5f;
								var bq1 = (b1 + b2) * 0.5f;
								var bq2 = (b2 + b0) * 0.5f;
								blendlistlist[k].AddRange(new[] { b0, bq0, bq2, bq2, bq1, b2, bq0, b1, bq1, bq0, bq1, bq2 });
							}
						}
						if(IsWeight)
						{
							var bwlistq0 = BoneWeight1Lerp(bwlist0, bwlist1);
							var bwlistq1 = BoneWeight1Lerp(bwlist1, bwlist2);
							var bwlistq2 = BoneWeight1Lerp(bwlist2, bwlist0);
							var biq0 = (byte)bwlistq0.Count;
							var biq1 = (byte)bwlistq1.Count;
							var biq2 = (byte)bwlistq2.Count;
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlistq0);
							bonew1list.AddRange(bwlistq2);
							bonew1list.AddRange(bwlistq2);
							bonew1list.AddRange(bwlistq1);
							bonew1list.AddRange(bwlist2);
							bonew1list.AddRange(bwlistq0);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwlistq1);
							bonew1list.AddRange(bwlistq0);
							bonew1list.AddRange(bwlistq1);
							bonew1list.AddRange(bwlistq2);
							perverlist.AddRange(new[] { bi0, biq0, biq2, biq2, biq1, bi2, biq0, bi1, biq1, biq0, biq1, biq2 });
						}
					}
					else
					{
						subdlist.AddRange(new[] { v0, v1, v2 });
						colorlist.AddRange(new[] { cv0, cv1, cv2 });
						uv0list.AddRange(new[] { uv0_0, uv0_1, uv0_2 });
						uv2list.AddRange(new[] { uv2_0, uv2_1, uv2_2 });
						uv3list.AddRange(new[] { uv3_0, uv3_1, uv3_2 });
						uv4list.AddRange(new[] { uv4_0, uv4_1, uv4_2 });
						if(blefcnt > 0)
						{
							for(var k = 0; k < blefcnt; k++)
							{
								var b0 = blendarrlist[k][triangles[j]];
								var b1 = blendarrlist[k][triangles[j + 1]];
								var b2 = blendarrlist[k][triangles[j + 2]];
								blendlistlist[k].AddRange(new[] { b0, b1, b2 });
							}
						}
						if(IsWeight)
						{
							bonew1list.AddRange(bwlist0);
							bonew1list.AddRange(bwlist1);
							bonew1list.AddRange(bwlist2);
							perverlist.AddRange(new[] { bi0, bi1, bi2 });
						}
						newtri.AddRange(new[] { cnt0, cnt0 + 1, cnt0 + 2 });

						//foreach (Vector3 vec in new Vector3[] { v0, v1, v2 }) chkvdict[vec] = true;
					}
				}

				var varr = subdlist.ToArray();
				calcarr = new int[subdlist.Count];
				pcnt = 0;
				if(IsWeight)
				{
					boneidxs = Enumerable.Repeat(0, perverlist.Count).ToArray();
					for(var j = 0; j < perverlist.Count; j++)
					{
						boneidxs[j] = pcnt;
						pcnt += perverlist[j];
					}
					if(boneidxs.Length < 1)
					{
						boneidxs = Enumerable.Repeat(0, varr.Length).ToArray();
					}
				}
				for(var j = 0; j < subdlist.Count; j++)
				{
					int k;
					var vec4 = new Vector4(varr[j].x, varr[j].y, varr[j].z, uv0list[j].x * 256.0f + uv0list[j].y);
					if(subddict.TryGetValue(vec4, out k))
					{
						calcarr[j] = k;
					}
					else
					{
						var idx0 = subddict.Count;
						subddict[vec4] = idx0;
						calcarr[j] = idx0;
						newvlist.Add(subdlist[j]);
						newcolorlist.Add(colorlist[j]);
						newuv0list.Add(uv0list[j]);
						newuv2list.Add(uv2list[j]);
						newuv3list.Add(uv3list[j]);
						newuv4list.Add(uv4list[j]);
						if(IsWeight)
						{
							int pi0 = perverlist[j];
							var bwlist0 = new List<BoneWeight1>();
							if(pi0 > 0)
							{
								for(var l = 0; l < pi0 - 1; l++)
								{
									try
									{
										bwlist0.Add(bonew1list[boneidxs[j] + l]);
									}
									catch
									{
										bwlist0.Add(w1);
									}
								}
							}
							if(bwlist0.Count < 1)
							{
								bwlist0.Add(w1);
							}
							newbwlist.AddRange(bwlist0);
							newperverlist.Add((byte)bwlist0.Count);
						}
						if(blefcnt > 0)
						{
							for(var l = 0; l < blefcnt; l++)
							{
								//newblendlistlist[l].Add(blendarrlist[l][j]);
								newblendlistlist[l].Add(blendlistlist[l][j]);
							}
						}
					}
				}
				triangles = newtri.ToArray();
				for(var j = 0; j < newtri.Count; j++)
				{
					var t = calcarr[triangles[j]];
					newtri[j] = t;
				}
				newtrilist.Add(newtri);

				//triarrlist.Add(newtri.ToArray());
			}
			triarrlist = new List<int[]>();
			for(var i = 0; i < newtrilist.Count; i++)
			{
				triarrlist.Add(newtrilist[i].ToArray());
			}
			pervers = perverlist.ToArray();
			boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			if(IsWeight)
			{
				pcnt = 0;
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			mesh.Clear();
			if(newvlist.Count > 65000)
			{
				try
				{
					mesh.indexFormat = IndexFormat.UInt32;
				}
				catch
				{
					return;
				}
			}
			else
			{
				mesh.indexFormat = IndexFormat.UInt16;
			}
			mesh.vertices = newvlist.ToArray();
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.colors = newcolorlist.ToArray();
			mesh.uv = newuv0list.ToArray();
			mesh.uv2 = newuv2list.ToArray();
			mesh.uv3 = newuv3list.ToArray();
			mesh.uv4 = newuv4list.ToArray();
			mesh.bindposes = binds;
			mesh.subMeshCount = triarrlist.Count;
			for(var i = 0; i < triarrlist.Count; i++)
			{
				if(IsMonoSubmesh)
				{
					mesh.SetTriangles(triarrlist[0], i);
				}
				else
				{
					var triarr = triarrlist[i];
					mesh.SetTriangles(triarr, i);
				}
			}
			if(IsWeight)
			{
				var ntweight1s = new NativeArray<BoneWeight1>(newbwlist.ToArray(), Allocator.Temp);
				var PerVerts = new NativeArray<byte>(newperverlist.ToArray(), Allocator.Temp);
				if(PerVerts.Length > 0)
				{
					mesh.SetBoneWeights(PerVerts, ntweight1s);
				}
			}
			else
			{
				BoneWeight1Create(mesh);
			}
			if(bscnt > 0)
			{
				var cnt1 = 0;
				for(var i = 0; i < bscnt; i++)
				{
					var fcnt = framecntlist[i];
					for(var j = 0; j < fcnt; j++)
					{
						mesh.AddBlendShapeFrame(bnamelist[i], bweightlist[cnt1], newblendlistlist[cnt1].ToArray()
							, Enumerable.Repeat(Vector3.zero, mesh.vertexCount).ToArray()
							, Enumerable.Repeat(Vector3.zero, mesh.vertexCount).ToArray());
						cnt1++;
					}

					//mesh.AddBlendShapeFrame(bnamelist[i], bweightlist[i], newblendlistlist[i].ToArray()
					//    , Enumerable.Repeat(Vector3.zero, mesh.vertexCount).ToArray(), Enumerable.Repeat(Vector3.zero, mesh.vertexCount).ToArray());
				}
			}

			//mesh.RecalculateNormals();
			CalcMeshNormals(mesh);
			mesh.RecalculateBounds();
			IsBrushedArray = Enumerable.Repeat(false, mesh.vertexCount).ToArray();
			mesh.RecalculateTangents();
		}

		private static void SubdivideMesh(Mesh mesh)
		{
			var vertices = mesh.vertices;
			var triarrlist = new List<int[]>();
			var subcnt = IsMonoSubmesh ? 1 : mesh.subMeshCount;
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}
			var colors = mesh.colors;
			if(colors.Length < 1)
			{
				colors = Enumerable.Repeat(Color.white, vertices.Length).ToArray();
			}
			var uv0s = mesh.uv;
			var uv2s = mesh.uv2;
			var uv3s = mesh.uv3;
			var uv4s = mesh.uv4;
			if(uv0s.Length < 1)
			{
				uv0s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv2s.Length < 1)
			{
				uv2s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv3s.Length < 1)
			{
				uv3s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv4s.Length < 1)
			{
				uv4s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			var binds = mesh.bindposes;
			var weight1s = mesh.GetAllBoneWeights().ToArray();
			var pervers = mesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var pcnt = 0;
			if(IsWeight)
			{
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}

			//bool[] chkvarr = Enumerable.Repeat(false, vertices.Length).ToArray();
			if(IsBrushedArray.Length != vertices.Length)
			{
				IsBrushedArray = Enumerable.Repeat(false, vertices.Length).ToArray();
			}
			var SubdHash = new HashSet<Vector3>();

			//Dictionary<Vector3, int> VecIDDict = Enumerable.Range(0, vertices.Length).ToDictionary(i => vertices[i], i => i);
			var subdlist = new List<Vector3>(vertices);
			var uv0list = new List<Vector2>(uv0s);
			var uv2list = new List<Vector2>(uv2s);
			var uv3list = new List<Vector2>(uv3s);
			var uv4list = new List<Vector2>(uv4s);
			var colorlist = new List<Color>(colors);
			var bonew1list = new List<BoneWeight1>(weight1s);
			var perverlist = new List<byte>(pervers);
			var w1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
			for(var h = 0; h < subcnt; h++)
			{
				var triarr = triarrlist[h];
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					var pv0 = IsWeight ? pervers[triarr[i]] : 0;
					var pv1 = IsWeight ? pervers[triarr[i + 1]] : 0;
					var pv2 = IsWeight ? pervers[triarr[i + 2]] : 0;
					var bwlist0 = new List<BoneWeight1>();
					if(pv0 > 0)
					{
						for(var j = 0; j < pv0; j++)
						{
							bwlist0.Add(weight1s[boneidxs[triarr[i]] + j]);
						}
					}
					else
					{
						bwlist0.Add(w1);
					}
					var bwlist1 = new List<BoneWeight1>();
					if(pv1 > 0)
					{
						for(var j = 0; j < pv1; j++)
						{
							bwlist1.Add(weight1s[boneidxs[triarr[i + 1]] + j]);
						}
					}
					else
					{
						bwlist1.Add(w1);
					}
					var bwlist2 = new List<BoneWeight1>();
					if(pv2 > 0)
					{
						for(var j = 0; j < pv2; j++)
						{
							bwlist2.Add(weight1s[boneidxs[triarr[i + 2]] + j]);
						}
					}
					else
					{
						bwlist2.Add(w1);
					}
					var d0 = Vector3.Distance(v0, v1);
					var d1 = Vector3.Distance(v1, v2);
					var d2 = Vector3.Distance(v2, v0);
					var dc = avgPointDist * 0.5f;
					bool flag0 = false, flag1 = false, flag2 = false;
					if(d0 > d1 && d0 > d2)
					{
						var p0 = v2 + d2 / (d2 + d0) * (v1 - v2);
						var dp0 = Vector3.Distance(v0, p0);
						if(d0 > dc || d0 > dp0 * 5.0f)
						{
							flag0 = true;
						}
					}
					else if(d1 > d0 && d1 > d2)
					{
						var p1 = v0 + d0 / (d0 + d1) * (v2 - v0);
						var dp1 = Vector3.Distance(v1, p1);
						if(d1 > dc || d1 > dp1 * 5.0f)
						{
							flag1 = true;
						}
					}
					else if(d2 > d0 && d2 > d1)
					{
						var p2 = v1 + d1 / (d1 + d2) * (v0 - v1);
						var dp2 = Vector3.Distance(v2, p2);
						if(d2 > dc || d2 > dp2 * 5.0f)
						{
							flag2 = true;
						}
					}
					if(uv4s[triarr[i]].x < maskWeight || uv4s[triarr[i + 1]].x < maskWeight)
					{
						flag0 = false;
					}
					if(uv4s[triarr[i + 1]].x < maskWeight || uv4s[triarr[i + 2]].x < maskWeight)
					{
						flag1 = false;
					}
					if(uv4s[triarr[i + 2]].x < maskWeight || uv4s[triarr[i]].x < maskWeight)
					{
						flag2 = false;
					}
					if(currentStatus == SculptStatus.Active)
					{
						if(!IsBrushedArray[triarr[i]] && !IsBrushedArray[triarr[i + 1]] && !IsBrushedArray[triarr[i + 2]])
						{
							flag0 = false;
							flag1 = false;
							flag2 = false;
						}
					}
					var e0 = v0 * 0.5f + v1 * 0.5f;
					var e1 = v1 * 0.5f + v2 * 0.5f;
					var e2 = v2 * 0.5f + v0 * 0.5f;
					if(flag0)
					{
						if(!SubdHash.Contains(e0))
						{
							SubdHash.Add(e0);
							subdlist.Add(e0);
							uv0list.Add(uv0s[triarr[i]] * 0.5f + uv0s[triarr[i + 1]] * 0.5f);
							uv2list.Add(uv2s[triarr[i]] * 0.5f + uv2s[triarr[i + 1]] * 0.5f);
							uv3list.Add(uv3s[triarr[i]] * 0.5f + uv3s[triarr[i + 1]] * 0.5f);
							uv4list.Add(uv4s[triarr[i]] * 0.5f + uv4s[triarr[i + 1]] * 0.5f);
							colorlist.Add(colors[triarr[i]] * 0.5f + colors[triarr[i + 1]] * 0.5f);
							if(IsWeight)
							{
								var bwliste0 = BoneWeight1Lerp(bwlist0, bwlist1);
								bonew1list.AddRange(bwliste0);
								perverlist.Add((byte)bwliste0.Count);
							}
						}
					}
					else if(flag1)
					{
						if(!SubdHash.Contains(e1))
						{
							SubdHash.Add(e1);
							subdlist.Add(e1);
							uv0list.Add(uv0s[triarr[i + 1]] * 0.5f + uv0s[triarr[i + 2]] * 0.5f);
							uv2list.Add(uv2s[triarr[i + 1]] * 0.5f + uv2s[triarr[i + 2]] * 0.5f);
							uv3list.Add(uv3s[triarr[i + 1]] * 0.5f + uv3s[triarr[i + 2]] * 0.5f);
							uv4list.Add(uv4s[triarr[i + 1]] * 0.5f + uv4s[triarr[i + 2]] * 0.5f);
							colorlist.Add(colors[triarr[i + 1]] * 0.5f + colors[triarr[i + 2]] * 0.5f);
							if(IsWeight)
							{
								var bwliste1 = BoneWeight1Lerp(bwlist1, bwlist2);
								bonew1list.AddRange(bwliste1);
								perverlist.Add((byte)bwliste1.Count);
							}
						}
					}
					else if(flag2)
					{
						if(!SubdHash.Contains(e2))
						{
							SubdHash.Add(e2);
							subdlist.Add(e2);
							uv0list.Add(uv0s[triarr[i + 2]] * 0.5f + uv0s[triarr[i]] * 0.5f);
							uv2list.Add(uv2s[triarr[i + 2]] * 0.5f + uv2s[triarr[i]] * 0.5f);
							uv3list.Add(uv3s[triarr[i + 2]] * 0.5f + uv3s[triarr[i]] * 0.5f);
							uv4list.Add(uv4s[triarr[i + 2]] * 0.5f + uv4s[triarr[i]] * 0.5f);
							colorlist.Add(colors[triarr[i + 2]] * 0.5f + colors[triarr[i]] * 0.5f);
							if(IsWeight)
							{
								var bwliste2 = BoneWeight1Lerp(bwlist2, bwlist0);
								bonew1list.AddRange(bwliste2);
								perverlist.Add((byte)bwliste2.Count);
							}
						}
					}
				}
			}
			var SubdArr = SubdHash.ToArray();
			var NewVDict = Enumerable.Range(0, SubdArr.Length).ToDictionary(i => SubdArr[i], i => i + vertices.Length);
			var newtrilistlist = new List<List<int>>();
			for(var h = 0; h < subcnt; h++)
			{
				var triarr = triarrlist[h];
				var newtrilist = triarr.ToList();
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					var e0 = v0 * 0.5f + v1 * 0.5f;
					var e1 = v1 * 0.5f + v2 * 0.5f;
					var e2 = v2 * 0.5f + v0 * 0.5f;
					int cnt0 = 0, cnt1 = 0, cnt2 = 0;
					var flag0 = NewVDict.TryGetValue(e0, out cnt0);
					var flag1 = NewVDict.TryGetValue(e1, out cnt1);
					var flag2 = NewVDict.TryGetValue(e2, out cnt2);
					if(flag0 && !flag1 && !flag2)
					{
						newtrilist[i] = triarr[i];
						newtrilist[i + 1] = cnt0;
						newtrilist[i + 2] = triarr[i + 2];
						newtrilist.AddRange(new[] { triarr[i + 2], cnt0, triarr[i + 1] });
					}
					else if(!flag0 && flag1 && !flag2)
					{
						newtrilist[i] = triarr[i];
						newtrilist[i + 1] = triarr[i + 1];
						newtrilist[i + 2] = cnt1;
						newtrilist.AddRange(new[] { triarr[i], cnt1, triarr[i + 2] });
					}
					else if(!flag0 && !flag1 && flag2)
					{
						newtrilist[i] = triarr[i + 1];
						newtrilist[i + 1] = triarr[i + 2];
						newtrilist[i + 2] = cnt2;
						newtrilist.AddRange(new[] { triarr[i + 1], cnt2, triarr[i] });
					}
					else if(flag0 && flag1 && !flag2)
					{
						newtrilist[i] = triarr[i];
						newtrilist[i + 1] = cnt0;
						newtrilist[i + 2] = triarr[i + 2];
						newtrilist.AddRange(new[] { cnt0, triarr[i + 1], cnt1 });
						newtrilist.AddRange(new[] { cnt0, cnt1, triarr[i + 2] });
					}
					else if(!flag0 && flag1 && flag2)
					{
						newtrilist[i] = triarr[i];
						newtrilist[i + 1] = triarr[i + 1];
						newtrilist[i + 2] = cnt1;
						newtrilist.AddRange(new[] { triarr[i], cnt1, cnt2 });
						newtrilist.AddRange(new[] { cnt1, triarr[i + 2], cnt2 });
					}
					else if(flag0 && !flag1 && flag2)
					{
						newtrilist[i] = triarr[i];
						newtrilist[i + 1] = cnt0;
						newtrilist[i + 2] = cnt2;
						newtrilist.AddRange(new[] { cnt2, cnt0, triarr[i + 2] });
						newtrilist.AddRange(new[] { cnt0, triarr[i + 1], triarr[i + 2] });
					}
					else if(flag0 && flag1 && flag2)
					{
						newtrilist[i] = triarr[i];
						newtrilist[i + 1] = cnt0;
						newtrilist[i + 2] = cnt2;
						newtrilist.AddRange(new[] { cnt0, cnt1, cnt2 });
						newtrilist.AddRange(new[] { cnt0, triarr[i + 1], cnt1 });
						newtrilist.AddRange(new[] { cnt2, cnt1, triarr[i + 2] });
					}
				}
				newtrilistlist.Add(newtrilist);
			}
			vertices = subdlist.ToArray();
			mesh.Clear();
			if(vertices.Length > 65000)
			{
				try
				{
					mesh.indexFormat = IndexFormat.UInt32;
				}
				catch
				{
					return;
				}
			}
			else
			{
				mesh.indexFormat = IndexFormat.UInt16;
			}
			mesh.vertices = vertices;
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint + 1; i++)
			{
				if(IsMonoSubmesh)
				{
					mesh.SetTriangles(newtrilistlist[0], i);
				}
				else
				{
					if(newtrilistlist.Count > i)
					{
						mesh.SetTriangles(newtrilistlist[i], i);
					}
					else
					{
						mesh.SetTriangles(newtrilistlist[0], i);
					}
				}
			}
			mesh.colors = colorlist.ToArray();
			mesh.uv = uv0list.ToArray();
			mesh.uv2 = uv2list.ToArray();
			mesh.uv3 = uv3list.ToArray();
			mesh.uv4 = uv4list.ToArray();
			mesh.bindposes = binds;
			if(IsWeight)
			{
				var ntweight1s = new NativeArray<BoneWeight1>(bonew1list.ToArray(), Allocator.Temp);
				var PerVerts = new NativeArray<byte>(perverlist.ToArray(), Allocator.Temp);
				if(PerVerts.Length > 0)
				{
					mesh.SetBoneWeights(PerVerts, ntweight1s);
				}
			}
			else
			{
				BoneWeight1Create(mesh);
			}
			mesh.RecalculateBounds();

			//mesh.RecalculateNormals();
			CalcMeshNormals(mesh);
			mesh.RecalculateTangents();
		}

		private static void CatmullClarkMerged(Mesh mesh, float smoothdeg)
		{
			var vertices = mesh.vertices;

			//int[] triangles = mesh.GetTriangles(0);
			var triarrlist = new List<int[]>();
			for(var i = 0; i < mesh.subMeshCount; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}
			if(mergedtris.Length < 3)
			{
				MergetriGenerate(currentMesh);
			}
			if(mesh.uv4.Length != vertices.Length)
			{
				mesh.uv4 = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			var uv4s = mesh.uv4;
			var adjarr = new int[vertices.Length];
			var avarr = new Vector3[vertices.Length];
			try
			{
				for(var i = 0; i < mergedtris.Length; i += 3)
				{
					if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
					{
						continue;
					}
					adjarr[mergedtris[i]] += 2;
					adjarr[mergedtris[i + 1]] += 2;
					adjarr[mergedtris[i + 2]] += 2;
				}
			}
			catch
			{
				MergetriGenerate(currentMesh);
				for(var i = 0; i < mergedtris.Length; i += 3)
				{
					if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
					{
						continue;
					}
					adjarr[mergedtris[i]] += 2;
					adjarr[mergedtris[i + 1]] += 2;
					adjarr[mergedtris[i + 2]] += 2;
				}
				if(DebugMode)
				{
					Debug.Log("Mesh get error.EditorSculpt fix that automatically");
				}
			}
			for(var i = 0; i < mergedtris.Length; i += 3)
			{
				//New in 2020/06/24
				if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
				{
					continue;
				}

				//End New in 2020/06/24
				var v0 = vertices[mergedtris[i]];
				var v1 = vertices[mergedtris[i + 1]];
				var v2 = vertices[mergedtris[i + 2]];
				var a0 = adjarr[mergedtris[i]] == 0 ? 1 : adjarr[mergedtris[i]];
				var a1 = adjarr[mergedtris[i + 1]] == 0 ? 1 : adjarr[mergedtris[i + 1]];
				var a2 = adjarr[mergedtris[i + 2]] == 0 ? 1 : adjarr[mergedtris[i + 2]];
				avarr[mergedtris[i]] = avarr[mergedtris[i]] + v1 / a0 + v2 / a0;
				avarr[mergedtris[i + 1]] = avarr[mergedtris[i + 1]] + v0 / a1 + v2 / a1;
				avarr[mergedtris[i + 2]] = avarr[mergedtris[i + 2]] + v0 / a2 + v1 / a2;
			}
			for(var i = 0; i < mergedtris.Length; i += 3)
			{
				if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
				{
					continue;
				}
				if(avarr[mergedtris[i]] != avarr[tris[i]])
				{
					avarr[tris[i]] = avarr[mergedtris[i]];
				}
				if(avarr[mergedtris[i + 1]] != avarr[tris[i + 1]])
				{
					avarr[tris[i + 1]] = avarr[mergedtris[i + 1]];
				}
				if(avarr[mergedtris[i + 2]] != avarr[tris[i + 2]])
				{
					avarr[tris[i + 2]] = avarr[mergedtris[i + 2]];
				}
			}
			if(IsBrushedArray.Length != vertices.Length)
			{
				IsBrushedArray = Enumerable.Repeat(false, vertices.Length).ToArray();
			}
			for(var i = 0; i < vertices.Length; i++)
			{
				if(IsBrushedArray[i] == false)
				{
					continue;
				}
				if(uv4s[i].x < maskWeight)
				{
					continue;
				}

				//vertices[i] = (avarr[i] + vertices[i]) * 0.5f;
				vertices[i] = avarr[i] * smoothdeg + vertices[i] * (1.0f - smoothdeg);
			}
			mesh.vertices = vertices;
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			CalcMeshNormals(mesh);
			mesh.RecalculateBounds();
		}

		private static void CalcMeshNormals(Mesh mesh)
		{
			if(!IsCalcNormal)
			{
				if(mergedtris.Length < 4)
				{
					MergetriGenerate(mesh);
				}
				mesh.RecalculateNormals();
				var normals = mesh.normals;
				var flag0 = false;
				for(var i = 0; i < mergedtris.Length; i += 3)
				{
					if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
					{
						continue;
					}
					try
					{
						if(normals[mergedtris[i]] != normals[tris[i]])
						{
							normals[tris[i]] = normals[mergedtris[i]];
						}
						if(normals[mergedtris[i + 1]] != normals[tris[i + 1]])
						{
							normals[tris[i + 1]] = normals[mergedtris[i + 1]];
						}
						if(normals[mergedtris[i + 2]] != normals[tris[i + 2]])
						{
							normals[tris[i + 2]] = normals[mergedtris[i + 2]];
						}
					}
					catch
					{
						flag0 = true;
						break;
					}
					;
				}
				if(flag0)
				{
					MergetriGenerate(mesh);
					for(var i = 0; i < mergedtris.Length; i += 3)
					{
						if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
						{
							continue;
						}
						if(normals[mergedtris[i]] != normals[tris[i]])
						{
							normals[tris[i]] = normals[mergedtris[i]];
						}
						if(normals[mergedtris[i + 1]] != normals[tris[i + 1]])
						{
							normals[tris[i + 1]] = normals[mergedtris[i + 1]];
						}
						if(normals[mergedtris[i + 2]] != normals[tris[i + 2]])
						{
							normals[tris[i + 2]] = normals[mergedtris[i + 2]];
						}
					}
					if(DebugMode)
					{
						Debug.Log("Mesh get error.EditorSculpt fix that automatically");
					}
				}
				mesh.normals = normals;
				return;
			}
			var vertices = mesh.vertices;
			var adjarr = new int[vertices.Length];
			var normarr = new Vector3[vertices.Length];
			var avnorm = new Vector3[vertices.Length];
			if(mergedtris.Length < 4)
			{
				MergetriGenerate(mesh);
			}
			for(var i = 0; i < mergedtris.Length; i += 3)
			{
				if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
				{
					continue;
				}
				try
				{
					adjarr[mergedtris[i]] = adjarr[mergedtris[i]] + 2;
					adjarr[mergedtris[i + 1]] = adjarr[mergedtris[i + 1]] + 2;
					adjarr[mergedtris[i + 2]] = adjarr[mergedtris[i + 2]] + 2;

					//Vector3 norm = Vector3.Cross(vertices[triangles[i + 1]] - vertices[triangles[i]], vertices[triangles[i + 2]] - vertices[triangles[i]]).normalized;
					var norm = Vector3.Cross((vertices[mergedtris[i + 1]] - vertices[mergedtris[i]]).normalized, (vertices[mergedtris[i + 2]] - vertices[mergedtris[i]]).normalized).normalized;
					normarr[mergedtris[i]] = normarr[mergedtris[i]] + norm * 2.0f;
					normarr[mergedtris[i + 1]] = normarr[mergedtris[i + 1]] + norm * 2.0f;
					normarr[mergedtris[i + 2]] = normarr[mergedtris[i + 2]] + norm * 2.0f;
				}
				catch
				{
				}
			}
			for(var i = 0; i < mergedtris.Length; i += 3)
			{
				if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
				{
					continue;
				}
				try
				{
					var n0 = normarr[mergedtris[i]];
					var n1 = normarr[mergedtris[i + 1]];
					var n2 = normarr[mergedtris[i + 2]];
					var a0 = adjarr[mergedtris[i]] == 0 ? 1 : adjarr[mergedtris[i]];
					var a1 = adjarr[mergedtris[i + 1]] == 0 ? 1 : adjarr[mergedtris[i + 1]];
					var a2 = adjarr[mergedtris[i + 2]] == 0 ? 1 : adjarr[mergedtris[i + 2]];
					avnorm[mergedtris[i]] = avnorm[mergedtris[i]] + n1 / a0 + n2 / a0;
					avnorm[mergedtris[i + 1]] = avnorm[mergedtris[i + 1]] + n0 / a1 + n2 / a1;
					avnorm[mergedtris[i + 2]] = avnorm[mergedtris[i + 2]] + n0 / a2 + n1 / a2;
				}
				catch
				{
				}
			}
			for(var i = 0; i < mergedtris.Length; i += 3)
			{
				if(mergedtris[i] == 0 && mergedtris[i + 1] == 0 && mergedtris[i + 2] == 0)
				{
					continue;
				}
				try
				{
					if(avnorm[mergedtris[i]] != avnorm[tris[i]])
					{
						avnorm[tris[i]] = avnorm[mergedtris[i]];
					}
					if(avnorm[mergedtris[i + 1]] != avnorm[tris[i + 1]])
					{
						avnorm[tris[i + 1]] = avnorm[mergedtris[i + 1]];
					}
					if(avnorm[mergedtris[i + 2]] != avnorm[tris[i + 2]])
					{
						avnorm[tris[i + 2]] = avnorm[mergedtris[i + 2]];
					}
				}
				catch
				{
				}
			}
			for(var i = 0; i < tris.Length; i++)
			{
				try
				{
					avnorm[tris[i]] = avnorm[tris[i]].normalized;
				}
				catch
				{
				}
			}
			mesh.normals = avnorm;
		}

		private static void CloseHole(Mesh mesh)
		{
			var vertices = mesh.vertices;
			var subcnt = IsMonoSubmesh ? 1 : mesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}
			var colors = mesh.colors;
			var uv0s = mesh.uv;
			var uv2s = mesh.uv2;
			var uv3s = mesh.uv3;
			var uv4s = mesh.uv4;
			var binds = mesh.bindposes;
			var weight1s = currentMesh.GetAllBoneWeights().ToArray();
			var pervers = currentMesh.GetBonesPerVertex().ToArray();
			var IsWeight = false;
			if(currentObject.GetComponent<SkinnedMeshRenderer>() && pervers.Length > 0)
			{
				IsWeight = true;
			}
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var pcnt = 0;
			if(IsWeight)
			{
				for(var i = 0; i < pervers.Length; i++)
				{
					boneidxs[i] = pcnt;
					pcnt += pervers[i];
				}
				if(boneidxs.Length < 1)
				{
					boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
				}
			}
			if(uv0s.Length < 1)
			{
				uv0s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv2s.Length < 1)
			{
				uv2s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv3s.Length < 1)
			{
				uv3s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			if(uv4s.Length < 1)
			{
				uv4s = Enumerable.Repeat(Vector2.one, vertices.Length).ToArray();
			}
			var newtrilist = new List<List<int>>();
			var adjvdict = new Dictionary<Vector3, Vector3>();
			for(var i = 0; i < subcnt; i++)
			{
				var triarr = triarrlist[i];
				for(var j = 0; j < triarr.Length; j += 3)
				{
					var v0 = vertices[triarr[j]];
					var v1 = vertices[triarr[j + 1]];
					var v2 = vertices[triarr[j + 2]];
					var q0 = (v0 + v1) * 0.5f;
					var q1 = (v1 + v2) * 0.5f;
					var q2 = (v2 + v0) * 0.5f;
					Vector3 a0;
					var adjv0 = adjvdict.TryGetValue(q0, out a0);
					if(adjv0)
					{
						if(a0 != v2)
						{
							adjvdict[q0] = adjvdict[q0] + v2;
						}
					}
					else
					{
						adjvdict[q0] = v2;
					}
					Vector3 a1;
					var adjv1 = adjvdict.TryGetValue(q1, out a1);
					if(adjv1)
					{
						if(a1 != v0)
						{
							adjvdict[q1] = adjvdict[q1] + v0;
						}
					}
					else
					{
						adjvdict[q1] = v0;
					}
					Vector3 a2;
					var adjv2 = adjvdict.TryGetValue(q2, out a2);
					if(adjv2)
					{
						if(a2 != v1)
						{
							adjvdict[q2] = adjvdict[q2] + v1;
						}
					}
					else
					{
						adjvdict[q2] = v1;
					}
				}
			}
			var edgelist = new List<Vector3>();
			var edtrilist = new List<int>();
			for(var i = 0; i < subcnt; i++)
			{
				var triarr = triarrlist[i];
				for(var j = 0; j < triarr.Length; j += 3)
				{
					var v0 = vertices[triarr[j]];
					var v1 = vertices[triarr[j + 1]];
					var v2 = vertices[triarr[j + 2]];
					var q0 = (v0 + v1) * 0.5f;
					var q1 = (v1 + v2) * 0.5f;
					var q2 = (v2 + v0) * 0.5f;
					if(adjvdict[q0] == v2)
					{
						edgelist.Add(v0);
						edgelist.Add(v1);
						edtrilist.Add(triarr[j]);
						edtrilist.Add(triarr[j + 1]);
					}
					else if(adjvdict[q1] == v0)
					{
						edgelist.Add(v1);
						edgelist.Add(v2);
						edtrilist.Add(triarr[j + 1]);
						edtrilist.Add(triarr[j + 2]);
					}
					else if(adjvdict[q2] == v1)
					{
						edgelist.Add(v2);
						edgelist.Add(v0);
						edtrilist.Add(triarr[j + 2]);
						edtrilist.Add(triarr[j]);
					}
				}
			}
			Vector3 c0;
			for(var i = 0; i < edgelist.Count; i += 2)
			{
				var e0 = edgelist[i];
				var e1 = edgelist[i + 1];
				for(var j = i; j < edgelist.Count; j += 2)
				{
					var f0 = edgelist[j];
					var f1 = edgelist[j + 1];
					if(e0 == f0 && e1 != f1)
					{
						c0 = (e1 + f1 + e0) / 3;
						vertices[edtrilist[i + 1]] = c0;
						vertices[edtrilist[j + 1]] = c0;
						vertices[edtrilist[i]] = c0;
						var cc0 = (colors[edtrilist[i + 1]] + colors[edtrilist[j + 1]] + colors[edtrilist[i]]) * 0.333f;
						colors[edtrilist[i + 1]] = cc0;
						colors[edtrilist[j + 1]] = cc0;
						colors[edtrilist[i]] = cc0;
						var uvc0_0 = (uv0s[edtrilist[i + 1]] + uv0s[edtrilist[j + 1]] + uv0s[edtrilist[i]]) * 0.333f;
						uv0s[edtrilist[i + 1]] = uvc0_0;
						uv0s[edtrilist[j + 1]] = uvc0_0;
						uv0s[edtrilist[i]] = uvc0_0;
						var uvc2_0 = (uv2s[edtrilist[i + 1]] + uv2s[edtrilist[j + 1]] + uv2s[edtrilist[i]]) * 0.333f;
						uv2s[edtrilist[i + 1]] = uvc2_0;
						uv2s[edtrilist[j + 1]] = uvc2_0;
						uv2s[edtrilist[i]] = uvc2_0;
						var uvc3_0 = (uv3s[edtrilist[i + 1]] + uv3s[edtrilist[j + 1]] + uv3s[edtrilist[i]]) * 0.333f;
						uv3s[edtrilist[i + 1]] = uvc3_0;
						uv3s[edtrilist[j + 1]] = uvc3_0;
						uv3s[edtrilist[i]] = uvc3_0;
						var uvc4_0 = (uv4s[edtrilist[i + 1]] + uv4s[edtrilist[j + 1]] + uv4s[edtrilist[i]]) * 0.333f;
						uv4s[edtrilist[i + 1]] = uvc4_0;
						uv4s[edtrilist[j + 1]] = uvc4_0;
						uv4s[edtrilist[i]] = uvc4_0;
					}
					else if(e0 == f1 && e1 != f0)
					{
						c0 = (e1 + f0 + e0) / 3;
						vertices[edtrilist[i + 1]] = c0;
						vertices[edtrilist[j]] = c0;
						vertices[edtrilist[i]] = c0;
						var cc0 = (colors[edtrilist[i + 1]] + colors[edtrilist[j]] + colors[edtrilist[i]]) * 0.333f;
						colors[edtrilist[i + 1]] = cc0;
						colors[edtrilist[j]] = cc0;
						colors[edtrilist[i]] = cc0;
						var uvc0_0 = (uv0s[edtrilist[i + 1]] + uv0s[edtrilist[j]] + uv0s[edtrilist[i]]) * 0.333f;
						uv0s[edtrilist[i + 1]] = uvc0_0;
						uv0s[edtrilist[j]] = uvc0_0;
						uv0s[edtrilist[i]] = uvc0_0;
						var uvc2_0 = (uv2s[edtrilist[i + 1]] + uv2s[edtrilist[j]] + uv2s[edtrilist[i]]) * 0.333f;
						uv2s[edtrilist[i + 1]] = uvc2_0;
						uv2s[edtrilist[j]] = uvc2_0;
						uv2s[edtrilist[i]] = uvc2_0;
						var uvc3_0 = (uv3s[edtrilist[i + 1]] + uv3s[edtrilist[j]] + uv3s[edtrilist[i]]) * 0.333f;
						uv3s[edtrilist[i + 1]] = uvc3_0;
						uv3s[edtrilist[j]] = uvc3_0;
						uv3s[edtrilist[i]] = uvc3_0;
						var uvc4_0 = (uv4s[edtrilist[i + 1]] + uv4s[edtrilist[j]] + uv4s[edtrilist[i]]) * 0.333f;
						uv4s[edtrilist[i + 1]] = uvc4_0;
						uv4s[edtrilist[j]] = uvc4_0;
						uv4s[edtrilist[i]] = uvc4_0;
					}
					else if(e1 == f0 && e0 != f1)
					{
						c0 = (e0 + f1 + e1) / 3;
						vertices[edtrilist[i]] = c0;
						vertices[edtrilist[j + 1]] = c0;
						vertices[edtrilist[i + 1]] = c0;
						var cc0 = (colors[edtrilist[i]] + colors[edtrilist[j + 1]] + colors[edtrilist[i + 1]]) * 0.333f;
						colors[edtrilist[i]] = cc0;
						colors[edtrilist[j + 1]] = cc0;
						colors[edtrilist[i + 1]] = cc0;
						var uvc0_0 = (uv0s[edtrilist[i]] + uv0s[edtrilist[j + 1]] + uv0s[edtrilist[i + 1]]) * 0.333f;
						uv0s[edtrilist[i]] = uvc0_0;
						uv0s[edtrilist[j + 1]] = uvc0_0;
						uv0s[edtrilist[i + 1]] = uvc0_0;
						var uvc2_0 = (uv2s[edtrilist[i]] + uv2s[edtrilist[j + 1]] + uv2s[edtrilist[i + 1]]) * 0.333f;
						uv2s[edtrilist[i]] = uvc2_0;
						uv2s[edtrilist[j + 1]] = uvc2_0;
						uv2s[edtrilist[i + 1]] = uvc2_0;
						var uvc3_0 = (uv3s[edtrilist[i]] + uv3s[edtrilist[j + 1]] + uv3s[edtrilist[i + 1]]) * 0.333f;
						uv3s[edtrilist[i]] = uvc3_0;
						uv3s[edtrilist[j + 1]] = uvc3_0;
						uv3s[edtrilist[i + 1]] = uvc3_0;
						var uvc4_0 = (uv4s[edtrilist[i]] + uv4s[edtrilist[j + 1]] + uv4s[edtrilist[i + 1]]) * 0.333f;
						uv4s[edtrilist[i]] = uvc4_0;
						uv4s[edtrilist[j + 1]] = uvc4_0;
						uv4s[edtrilist[i + 1]] = uvc4_0;
					}
					else if(e1 == f1 && e0 != f0)
					{
						c0 = (e0 + f1 + e1) / 3;
						vertices[edtrilist[i]] = c0;
						vertices[edtrilist[j + 1]] = c0;
						vertices[edtrilist[i + 1]] = c0;
						var cc0 = (colors[edtrilist[i]] + colors[edtrilist[j + 1]] + colors[edtrilist[i + 1]]) * 0.333f;
						colors[edtrilist[i]] = cc0;
						colors[edtrilist[j + 1]] = cc0;
						colors[edtrilist[i + 1]] = cc0;
						var uvc0_0 = (uv0s[edtrilist[i]] + uv0s[edtrilist[j + 1]] + uv0s[edtrilist[i + 1]]) * 0.333f;
						uv0s[edtrilist[i]] = uvc0_0;
						uv0s[edtrilist[j + 1]] = uvc0_0;
						uv0s[edtrilist[i + 1]] = uvc0_0;
						var uvc2_0 = (uv2s[edtrilist[i]] + uv2s[edtrilist[j + 1]] + uv2s[edtrilist[i + 1]]) * 0.333f;
						uv2s[edtrilist[i]] = uvc2_0;
						uv2s[edtrilist[j + 1]] = uvc2_0;
						uv2s[edtrilist[i + 1]] = uvc2_0;
						var uvc3_0 = (uv3s[edtrilist[i]] + uv3s[edtrilist[j + 1]] + uv3s[edtrilist[i + 1]]) * 0.333f;
						uv3s[edtrilist[i]] = uvc3_0;
						uv3s[edtrilist[j + 1]] = uvc3_0;
						uv3s[edtrilist[i + 1]] = uvc3_0;
						var uvc4_0 = (uv4s[edtrilist[i]] + uv4s[edtrilist[j + 1]] + uv4s[edtrilist[i + 1]]) * 0.333f;
						uv4s[edtrilist[i]] = uvc4_0;
						uv4s[edtrilist[j + 1]] = uvc4_0;
						uv4s[edtrilist[i + 1]] = uvc4_0;
					}
				}
			}
			var subddict = new Dictionary<Vector4, int>();
			var newvlist = new List<Vector3>();
			var newcolorlist = new List<Color>();
			var newuv0list = new List<Vector2>();
			var newuv2list = new List<Vector2>();
			var newuv3list = new List<Vector2>();
			var newuv4list = new List<Vector2>();
			var newperverlist = new List<byte>();
			var newbwlist = new List<BoneWeight1>();
			var w1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
			for(var h = 0; h < subcnt; h++)
			{
				var triangles = triarrlist[h];
				var newverlist = new List<Vector3>();
				var uv0list = new List<Vector2>();
				var uv2list = new List<Vector2>();
				var uv3list = new List<Vector2>();
				var uv4list = new List<Vector2>();
				var colorlist = new List<Color>();
				var bonew1list = new List<BoneWeight1>();
				var perverlist = new List<byte>();

				//Dictionary<Vector3, bool> polychkdict = new Dictionary<Vector3, bool>();
				var newtri = new List<int>();
				var cnt0 = 0;
				for(var i = 0; i < triangles.Length; i += 3)
				{
					cnt0 = newtri.Count;
					var v0 = vertices[triangles[i]];
					var v1 = vertices[triangles[i + 1]];
					var v2 = vertices[triangles[i + 2]];
					var cv0 = colors[triangles[i]];
					var cv1 = colors[triangles[i + 1]];
					var cv2 = colors[triangles[i + 2]];
					var uv0_0 = uv0s[triangles[i]];
					var uv0_1 = uv0s[triangles[i + 1]];
					var uv0_2 = uv0s[triangles[i + 2]];
					var uv2_0 = uv2s[triangles[i]];
					var uv2_1 = uv2s[triangles[i + 1]];
					var uv2_2 = uv2s[triangles[i + 2]];
					var uv3_0 = uv3s[triangles[i]];
					var uv3_1 = uv3s[triangles[i + 1]];
					var uv3_2 = uv3s[triangles[i + 2]];
					var uv4_0 = uv4s[triangles[i]];
					var uv4_1 = uv4s[triangles[i + 1]];
					var uv4_2 = uv4s[triangles[i + 2]];
					newverlist.AddRange(new[] { v0, v1, v2 });
					newtri.AddRange(new[] { cnt0, cnt0 + 1, cnt0 + 2 });
					colorlist.AddRange(new[] { cv0, cv1, cv2 });
					uv0list.AddRange(new[] { uv0_0, uv0_1, uv0_2 });
					uv2list.AddRange(new[] { uv2_0, uv2_1, uv2_2 });
					uv3list.AddRange(new[] { uv3_0, uv3_1, uv3_2 });
					uv4list.AddRange(new[] { uv4_0, uv4_1, uv4_2 });
					if(IsWeight)
					{
						//int pv0 = IsWeight ? pervers[triangles[i]] : 0;
						//int pv1 = IsWeight ? pervers[triangles[i + 1]] : 0;
						//int pv2 = IsWeight ? pervers[triangles[i + 2]] : 0;
						int pv0 = pervers[triangles[i]];
						int pv1 = pervers[triangles[i + 1]];
						int pv2 = pervers[triangles[i + 2]];
						var bwlist0 = new List<BoneWeight1>();
						if(pv0 > 0)
						{
							for(var j = 0; j < pv0; j++)
							{
								try
								{
									bwlist0.Add(weight1s[boneidxs[triangles[i]] + j]);
								}
								catch
								{
									bwlist0.Add(w1);
								}
							}
						}
						if(bwlist0.Count < 1)
						{
							bwlist0.Add(w1);
						}
						var bwlist1 = new List<BoneWeight1>();
						if(pv1 > 0)
						{
							for(var j = 0; j < pv1; j++)
							{
								try
								{
									bwlist1.Add(weight1s[boneidxs[triangles[i + 1]] + j]);
								}
								catch
								{
									bwlist1.Add(w1);
								}
							}
						}
						if(bwlist1.Count < 1)
						{
							bwlist1.Add(w1);
						}
						var bwlist2 = new List<BoneWeight1>();
						if(pv2 > 0)
						{
							for(var j = 0; j < pv2; j++)
							{
								try
								{
									bwlist2.Add(weight1s[boneidxs[triangles[i + 2]] + j]);
								}
								catch
								{
									bwlist2.Add(w1);
								}
							}
						}
						if(bwlist2.Count < 1)
						{
							bwlist2.Add(w1);
						}
						bonew1list.AddRange(bwlist0);
						bonew1list.AddRange(bwlist1);
						bonew1list.AddRange(bwlist2);
						var bi0 = (byte)bwlist0.Count;
						var bi1 = (byte)bwlist1.Count;
						var bi2 = (byte)bwlist2.Count;
						perverlist.AddRange(new[] { bi0, bi1, bi2 });
					}
				}
				var varr = newverlist.ToArray();
				var triarr = newtri.ToArray();
				var calcarr = new int[newverlist.Count];
				pcnt = 0;
				if(IsWeight)
				{
					boneidxs = Enumerable.Repeat(0, perverlist.Count).ToArray();
					for(var i = 0; i < perverlist.Count; i++)
					{
						boneidxs[i] = pcnt;
						pcnt += perverlist[i];
					}
					if(boneidxs.Length < 1)
					{
						boneidxs = Enumerable.Repeat(0, varr.Length).ToArray();
					}
				}
				for(var i = 0; i < newverlist.Count; i++)
				{
					int j;
					var vec4 = new Vector4(varr[i].x, varr[i].y, varr[i].z, uv0list[i].x * 256.0f + uv0list[i].y);
					if(subddict.TryGetValue(vec4, out j))
					{
						calcarr[i] = j;
					}
					else
					{
						var idx0 = subddict.Count;
						subddict[vec4] = idx0;
						calcarr[i] = idx0;
						newvlist.Add(newverlist[i]);
						newcolorlist.Add(colorlist[i]);
						newuv0list.Add(uv0list[i]);
						newuv2list.Add(uv2list[i]);
						newuv3list.Add(uv3list[i]);
						newuv4list.Add(uv4list[i]);
						if(IsWeight)
						{
							int pi0 = perverlist[i];
							var bwlist0 = new List<BoneWeight1>();
							if(pi0 > 0)
							{
								//for (int k = 0; k < pi0 - 1; k++)
								for(var k = 0; k < pi0; k++)
								{
									try
									{
										bwlist0.Add(bonew1list[boneidxs[i] + k]);
									}
									catch
									{
										bwlist0.Add(w1);
									}
								}
							}
							if(bwlist0.Count < 1)
							{
								bwlist0.Add(w1);
							}
							newbwlist.AddRange(bwlist0);
							newperverlist.Add((byte)bwlist0.Count);
						}
					}
				}
				for(var i = 0; i < newtri.Count; i++)
				{
					var t = calcarr[triarr[i]];
					newtri[i] = t;
				}
				newtrilist.Add(newtri);
			}
			mesh.Clear();
			if(newvlist.Count > 65000)
			{
				try
				{
					mesh.indexFormat = IndexFormat.UInt32;
				}
				catch
				{
					return;
				}
			}
			else
			{
				mesh.indexFormat = IndexFormat.UInt16;
			}
			mesh.vertices = newvlist.ToArray();
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint + 1; i++)
			{
				if(IsMonoSubmesh)
				{
					mesh.SetTriangles(newtrilist[0], i);
				}
				else
				{
					if(newtrilist.Count >= i)
					{
						mesh.SetTriangles(newtrilist[i], i);
					}
					else
					{
						mesh.SetTriangles(newtrilist[0], i);
					}
				}
			}
			mesh.colors = newcolorlist.ToArray();
			mesh.uv = newuv0list.ToArray();
			mesh.uv2 = newuv2list.ToArray();
			mesh.uv3 = newuv3list.ToArray();
			mesh.uv4 = newuv4list.ToArray();
			mesh.bindposes = binds;
			if(IsWeight)
			{
				var ntweight1s = new NativeArray<BoneWeight1>(newbwlist.ToArray(), Allocator.Temp);
				var PerVerts = new NativeArray<byte>(newperverlist.ToArray(), Allocator.Temp);
				mesh.SetBoneWeights(PerVerts, ntweight1s);
			}
			else
			{
				BoneWeight1Create(mesh);
			}
			CalcMeshNormals(mesh);

			//mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			mesh.RecalculateTangents();
		}

		private static void CloseHoleFast(Mesh mesh)
		{
			var vertices = mesh.vertices;
			var subcnt = IsMonoSubmesh ? 1 : mesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}
			var colors = mesh.colors;
			var uv0s = mesh.uv;
			var uv2s = mesh.uv2;
			var uv3s = mesh.uv3;
			var uv4s = mesh.uv4;
			var AdjVSet = new HashSet<Vector3>();
			for(var h = 0; h < subcnt; h++)
			{
				var triarr = triarrlist[h];
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					var q0 = v0 * 0.5f + v1 * 0.5f;
					var q1 = v1 * 0.5f + v2 * 0.5f;
					var q2 = v2 * 0.5f + v0 * 0.5f;
					if(AdjVSet.Contains(q0))
					{
						AdjVSet.Remove(q0);
					}
					else
					{
						AdjVSet.Add(q0);
					}
					if(AdjVSet.Contains(q1))
					{
						AdjVSet.Remove(q1);
					}
					else
					{
						AdjVSet.Add(q1);
					}
					if(AdjVSet.Contains(q2))
					{
						AdjVSet.Remove(q2);
					}
					else
					{
						AdjVSet.Add(q2);
					}
				}
			}
			var edtrilist = new List<int>();
			var HoleSet = new HashSet<Vector3>();
			for(var h = 0; h < subcnt; h++)
			{
				var triarr = triarrlist[h];
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var v0 = vertices[triarr[i]];
					var v1 = vertices[triarr[i + 1]];
					var v2 = vertices[triarr[i + 2]];
					var q0 = v0 * 0.5f + v1 * 0.5f;
					var q1 = v1 * 0.5f + v2 * 0.5f;
					var q2 = v2 * 0.5f + v0 * 0.5f;
					if(AdjVSet.Contains(q0))
					{
						edtrilist.Add(triarr[i]);
						edtrilist.Add(triarr[i + 1]);
						HoleSet.Add(q0);
					}
					else if(AdjVSet.Contains(q1))
					{
						edtrilist.Add(triarr[i + 1]);
						edtrilist.Add(triarr[i + 2]);
						HoleSet.Add(q1);
					}
					else if(AdjVSet.Contains(q2))
					{
						edtrilist.Add(triarr[i + 2]);
						edtrilist.Add(triarr[i]);
						HoleSet.Add(q2);
					}
				}
			}
			var AdjIdxListList = new List<List<int>>();
			for(var i = 0; i < vertices.Length; i++)
			{
				AdjIdxListList.Add(new List<int>());
			}
			for(var h = 0; h < subcnt; h++)
			{
				var triarr = triarrlist[h];
				for(var i = 0; i < triarr.Length; i += 3)
				{
					var i0 = triarr[i];
					var i1 = triarr[i + 1];
					var i2 = triarr[i + 2];
					AdjIdxListList[triarr[i]].AddRange(new[] { i1, i2 });
					AdjIdxListList[triarr[i + 1]].AddRange(new[] { i2, i0 });
					AdjIdxListList[triarr[i + 2]].AddRange(new[] { i0, i1 });
				}
			}
			var newvarr = new List<Vector3>(vertices).ToArray();
			for(var i = 0; i < edtrilist.Count; i += 2)
			{
				var i0 = edtrilist[i];
				var i1 = edtrilist[i + 1];
				int idx0 = 0, idx1 = 0;
				var e0 = vertices[i0];
				var e1 = vertices[i1];
				var AdjIdxList0 = AdjIdxListList[i0];
				var AdjIdxList1 = AdjIdxListList[i1];
				foreach(var idx in AdjIdxList0)
				{
					if(idx == i1)
					{
						continue;
					}
					var e2 = vertices[idx];
					var q0 = e0 * 0.5f + e2 * 0.5f;
					if(HoleSet.Contains(q0))
					{
						idx0 = idx;
						break;
					}
				}
				if(idx0 > 0)
				{
					//Vector3 vc0 = vertices[i0] * 0.333f + vertices[i1] * 0.333f + vertices[idx0] * 0.333f;
					var vc0 = newvarr[i0] * 0.333f + newvarr[i1] * 0.333f + newvarr[idx0] * 0.333f;

					//vertices[i0] = vc0;
					//vertices[i1] = vc0;
					//vertices[idx0] = vc0;
					newvarr[i0] = vc0;
					newvarr[i1] = vc0;
					newvarr[idx0] = vc0;
					var cc0 = colors[i0] * 0.333f + colors[i1] * 0.333f + colors[idx0] * 0.333f;
					colors[i0] = cc0;
					colors[i1] = cc0;
					colors[idx0] = cc0;
					var uvc0_0 = uv0s[i0] * 0.333f + uv0s[i1] * 0.333f + uv0s[idx0] * 0.333f;
					uv0s[i0] = uvc0_0;
					uv0s[i1] = uvc0_0;
					uv0s[idx0] = uvc0_0;
					var uvc2_0 = uv2s[i0] * 0.333f + uv2s[i1] * 0.333f + uv2s[idx0] * 0.333f;
					uv2s[i0] = uvc2_0;
					uv2s[i1] = uvc2_0;
					uv2s[idx0] = uvc2_0;
					var uvc3_0 = uv3s[i0] * 0.333f + uv3s[i1] * 0.333f + uv3s[idx0] * 0.333f;
					uv3s[i0] = uvc3_0;
					uv3s[i1] = uvc3_0;
					uv3s[idx0] = uvc3_0;
					var uvc4_0 = uv4s[i0] * 0.333f + uv4s[i1] * 0.333f + uv4s[idx0] * 0.333f;
					uv4s[i0] = uvc4_0;
					uv4s[i1] = uvc4_0;
					uv4s[idx0] = uvc4_0;
				}
				foreach(var idx in AdjIdxList1)
				{
					if(idx == i0)
					{
						continue;
					}
					var e3 = vertices[idx];
					var q1 = e1 * 0.5f + e3 * 0.5f;
					if(HoleSet.Contains(q1))
					{
						idx1 = idx;
						break;
					}
				}
				if(idx1 > 0)
				{
					//Vector3 vc0 = vertices[i0] * 0.333f + vertices[i1] * 0.333f + vertices[idx1] * 0.333f;
					var vc0 = newvarr[i0] * 0.333f + newvarr[i1] * 0.333f + newvarr[idx1] * 0.333f;

					//vertices[i0] = vc0;
					//vertices[i1] = vc0;
					//vertices[idx1] = vc0;
					newvarr[i0] = vc0;
					newvarr[i1] = vc0;
					newvarr[idx1] = vc0;
					var cc0 = colors[i0] * 0.333f + colors[i1] * 0.333f + colors[idx1] * 0.333f;
					colors[i0] = cc0;
					colors[i1] = cc0;
					colors[idx1] = cc0;
					var uvc0_0 = uv0s[i0] * 0.333f + uv0s[i1] * 0.333f + uv0s[idx1] * 0.333f;
					uv0s[i0] = uvc0_0;
					uv0s[i1] = uvc0_0;
					uv0s[idx1] = uvc0_0;
					var uvc2_0 = uv2s[i0] * 0.333f + uv2s[i1] * 0.333f + uv2s[idx1] * 0.333f;
					uv2s[i0] = uvc2_0;
					uv2s[i1] = uvc2_0;
					uv2s[idx1] = uvc2_0;
					var uvc3_0 = uv3s[i0] * 0.333f + uv3s[i1] * 0.333f + uv3s[idx1] * 0.333f;
					uv3s[i0] = uvc3_0;
					uv3s[i1] = uvc3_0;
					uv3s[idx1] = uvc3_0;
					var uvc4_0 = uv4s[i0] * 0.333f + uv4s[i1] * 0.333f + uv4s[idx1] * 0.333f;
					uv4s[i0] = uvc4_0;
					uv4s[i1] = uvc4_0;
					uv4s[idx1] = uvc4_0;
				}
			}
			vertices = new List<Vector3>(newvarr).ToArray();
			mesh.vertices = vertices;
		}

		private static void ReversePolygon(Mesh mesh)
		{
			var verticesr = mesh.vertices;
			var newtrlist = new List<int>();
			var subcnt = IsMonoSubmesh ? 1 : mesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(mesh.GetTriangles(i));
			}
			var trianglesr = mesh.GetTriangles(0);
			var colors = mesh.colors;
			var uvs = mesh.uv;
			var uv2s = mesh.uv2;
			var uv3s = mesh.uv3;
			var uv4s = mesh.uv4;
			var binds = mesh.bindposes;
			var weight1s = mesh.GetAllBoneWeights().ToArray();
			var pervers = mesh.GetBonesPerVertex().ToArray();
			var newtrilistlist = new List<List<int>>();
			for(var i = 0; i < subcnt; i++)
			{
				var triarr = triarrlist[i];
				var newtri = new List<int>();
				for(var j = 0; j < triarr.Length; j += 3)
				{
					newtri.AddRange(new[] { triarr[j + 2], triarr[j + 1], triarr[j] });
				}
				newtrilistlist.Add(newtri);
			}
			mesh.Clear();
			mesh.vertices = verticesr;
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint; i++)
			{
				mesh.SetTriangles(newtrlist.ToArray(), i);
			}
			for(var i = 0; i < matint + 1; i++)
			{
				if(IsMonoSubmesh)
				{
					mesh.SetTriangles(newtrilistlist[0], i);
				}
				else
				{
					if(newtrilistlist.Count > i)
					{
						mesh.SetTriangles(newtrilistlist[i], i);
					}
					else
					{
						mesh.SetTriangles(newtrilistlist[0], i);
					}
				}
			}
			if(colors.Length == mesh.vertexCount)
			{
				mesh.colors = colors;
			}
			if(uvs.Length == mesh.vertexCount)
			{
				mesh.uv = uvs;
			}
			if(uv2s.Length == mesh.vertexCount)
			{
				mesh.uv2 = uv2s;
			}
			if(uv3s.Length == mesh.vertexCount)
			{
				mesh.uv3 = uv3s;
			}
			if(uv4s.Length == mesh.vertexCount)
			{
				mesh.uv4 = uv4s;
			}
			if(pervers.Length > 0)
			{
				var ntweight1s = new NativeArray<BoneWeight1>(weight1s.ToArray(), Allocator.Temp);
				var PerVerts = new NativeArray<byte>(pervers.ToArray(), Allocator.Temp);
				mesh.SetBoneWeights(PerVerts, ntweight1s);
			}
			mesh.bindposes = binds;
			mesh.RecalculateBounds();
			CalcMeshNormals(mesh);
		}

		private static void RemoveIsland(Mesh mesh)
		{
			var vertices = mesh.vertices;
			var triangles = mesh.GetTriangles(0);
			var adjvdict = new Dictionary<Vector3, Vector3>();
			for(var i = 0; i < triangles.Length; i += 3)
			{
				var v0 = vertices[triangles[i]];
				var v1 = vertices[triangles[i + 1]];
				var v2 = vertices[triangles[i + 2]];
				var q0 = (v0 + v1) * 0.5f;
				var q1 = (v1 + v2) * 0.5f;
				var q2 = (v2 + v0) * 0.5f;
				Vector3 a0;
				var adjv0 = adjvdict.TryGetValue(q0, out a0);
				if(adjv0)
				{
					if(a0 != v2)
					{
						adjvdict[q0] = adjvdict[q0] + v2;
					}
				}
				else
				{
					adjvdict[q0] = v2;
				}
				Vector3 a1;
				var adjv1 = adjvdict.TryGetValue(q1, out a1);
				if(adjv1)
				{
					if(a1 != v0)
					{
						adjvdict[q1] = adjvdict[q1] + v0;
					}
				}
				else
				{
					adjvdict[q1] = v0;
				}
				Vector3 a2;
				var adjv2 = adjvdict.TryGetValue(q2, out a2);
				if(adjv2)
				{
					if(a2 != v1)
					{
						adjvdict[q2] = adjvdict[q2] + v1;
					}
				}
				else
				{
					adjvdict[q2] = v1;
				}
			}

			var newvlist = new List<Vector3>();
			var newtri = new List<int>();
			var cnt0 = 0;
			for(var i = 0; i < triangles.Length; i += 3)
			{
				if(vertices.Length == IsBrushedArray.Length)
				{
					if(currentStatus == SculptStatus.Active && IsBrushedArray[triangles[i]] == false && IsBrushedArray[triangles[i + 1]] == false && IsBrushedArray[triangles[i + 2]] == false)
					{
						continue;
					}
				}
				var cnt1 = 0;
				var v0 = vertices[triangles[i]];
				var v1 = vertices[triangles[i + 1]];
				var v2 = vertices[triangles[i + 2]];
				var q0 = (v0 + v1) * 0.5f;
				var q1 = (v1 + v2) * 0.5f;
				var q2 = (v2 + v0) * 0.5f;
				if(adjvdict[q0] == v2)
				{
					cnt1++;
				}
				else if(adjvdict[q1] == v0)
				{
					cnt1++;
				}
				else if(adjvdict[q2] == v1)
				{
					cnt1++;
				}
				if(cnt1 < 2)
				{
					newvlist.Add(v0);
					newvlist.Add(v1);
					newvlist.Add(v2);
					newtri.Add(cnt0);
					newtri.Add(cnt0 + 1);
					newtri.Add(cnt0 + 2);
					cnt0 += 3;
				}
			}

			var oldlist = newvlist;
			newvlist = newvlist.Distinct().ToList();
			var newvdict = new Dictionary<Vector3, int>();
			for(var i = 0; i < newvlist.Count; i++)
			{
				newvdict[newvlist[i]] = i;
			}
			for(var i = 0; i < newtri.Count; i++)
			{
				int j;
				newvdict.TryGetValue(oldlist[newtri[i]], out j);
				newtri[i] = j;
			}

			mesh.Clear();
			mesh.vertices = newvlist.ToArray();
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint; i++)
			{
				mesh.SetTriangles(newtri.ToArray(), i);
			}

			//mesh.RecalculateNormals();
			CalcMeshNormals(mesh);
			mesh.RecalculateBounds();
		}

		private static Vector2 CalcInterSectVector2(Vector2 ua0, Vector2 ua1, Vector2 ub0, Vector2 ub1)
		{
			var ua1ua0 = ua1 - ua0;
			var ub1ub0 = ub1 - ub0;
			var ub0ua0 = ub0 - ua0;
			var p0 = ua0 + ua1ua0 * (ub1ub0.x * ub0ua0.y - ub0ua0.x * ub1ub0.y) / (ub1ub0.x * ua1ua0.y - ua1ua0.x * ub1ub0.y);
			return p0;
		}

		private static float CalcCrossVector2(Vector2 v0, Vector2 v1)
		{
			var vcf = v0.x * v1.y - v1.x * v0.y;
			return vcf;
		}

		private static bool CalcIsInterSect(Vector2 ua0, Vector2 ua1, Vector2 ub0, Vector2 ub1)
		{
			var flagr = false;
			bool flag0 = false, flag1 = false;
			if(CalcCrossVector2(ua1 - ua0, ub0 - ua0) * CalcCrossVector2(ua1 - ua0, ub1 - ua0) < 0.0f)
			{
				flag0 = true;
			}
			if(CalcCrossVector2(ub1 - ub0, ua0 - ub0) * CalcCrossVector2(ub1 - ub0, ua1 - ub0) < 0.0f)
			{
				flag1 = true;
			}
			if(flag0 && flag1)
			{
				flagr = true;
			}
			return flagr;
		}

		private static bool CalcISInPoly2d(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 p0)
		{
			var va0 = CalcInterSectVector2(p0, p0 + v2 - v0, v0, v1);
			if(va0.y > v0.y && va0.y > v1.y)
			{
				return false;
			}
			if(va0.x > v0.x && va0.x > v1.x)
			{
				return false;
			}
			if(va0.y < v0.y && va0.y < v1.y)
			{
				return false;
			}
			if(va0.x < v0.x && va0.x < v1.x)
			{
				return false;
			}

			//if ((v0.y - v1.y) / (v0.x - v1.x) != (va0.y - v1.y) / (va0.x - v1.x)) return false;
			//float d0 = (v0.y - v1.y) / (v0.x - v1.x);
			//float d1 = (va0.y - v1.y) / (va0.x - v1.x);
			//if (d0 < 0.9f * d1 || d0 > 1.1f * d1) return false;
			var va2 = CalcInterSectVector2(p0, p0 + v0 - v2, v2, v1);
			if(va2.y > v2.y && va2.y > v1.y)
			{
				return false;
			}
			if(va2.x > v2.x && va2.x > v1.x)
			{
				return false;
			}
			if(va2.y < v2.y && va2.y < v1.y)
			{
				return false;
			}
			if(va2.x < v2.x && va2.x < v1.x)
			{
				return false;
			}

			//if ((v2.y - v1.y) / (v2.x - v1.x) != (va2.y - v1.y) / (va2.x - v1.x)) return false;
			//float d2 = (v2.y - v1.y) / (v2.x - v1.x);
			//float d3 = (va2.y - v1.y) / (va2.x - v1.x);
			//if (d2 < 0.9f * d3 || d2 > 1.1f * d3) return false;
			//if ((p0 - va0).normalized != (va2 - p0).normalized) return false;
			return true;
		}

		private static bool CalcISInPoly2d2(Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 up0)
		{
			var uvc = (uv0 + uv1 + uv2) / 3;
			var tu0 = (uv0.y - uv1.y) / (uv0.x - uv1.x);
			var tu1 = (uv1.y - uv2.y) / (uv1.x - uv2.x);
			var tu2 = (uv0.y - uv2.y) / (uv0.x - uv2.x);
			bool uflag0 = false, uflag1 = false, uflag2 = false;
			if(uvc.y > (uvc.x - uv1.x) * tu0 + uv1.y)
			{
				uflag0 = true;
			}
			if(uvc.y > (uvc.x - uv2.x) * tu1 + uv2.y)
			{
				uflag1 = true;
			}
			if(uvc.y > (uvc.x - uv2.x) * tu2 + uv2.y)
			{
				uflag2 = true;
			}
			if(uflag0 && up0.y < (up0.x - uv1.x) * tu0 + uv1.y)
			{
				return false;
			}
			if(!uflag0 && up0.y > (up0.x - uv1.x) * tu0 + uv1.y)
			{
				return false;
			}
			if(uflag1 && up0.y < (up0.x - uv2.x) * tu1 + uv2.y)
			{
				return false;
			}
			if(!uflag1 && up0.y > (up0.x - uv2.x) * tu1 + uv2.y)
			{
				return false;
			}
			if(uflag2 && up0.y < (up0.x - uv2.x) * tu2 + uv2.y)
			{
				return false;
			}
			if(!uflag2 && up0.y > (up0.x - uv2.x) * tu2 + uv2.y)
			{
				return false;
			}
			return true;
		}

		private static Vector3 CalcHitPos(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 rayorg, Vector3 raydir)
		{
			var hitpos = Vector3.zero;
			var norm = Vector3.Cross(p1 - p0, p2 - p0).normalized;
			var dot0 = Vector3.Dot(p0 - rayorg, norm);
			var dot1 = Vector3.Dot(rayorg + raydir * CameraSize * 10.0f, norm);
			var tlen = dot0 / dot1;
			if(dot1 >= -0.0001f)
			{
				return Vector3.zero;
			}
			var hitp = rayorg + tlen * (rayorg + raydir * CameraSize * 10.0f);
			var norm0 = Vector3.Cross(p1 - p0, hitp - p0);
			if(Vector3.Dot(norm, norm0) < 0)
			{
				return Vector3.zero;
			}
			var norm1 = Vector3.Cross(p2 - p1, hitp - p1);
			if(Vector3.Dot(norm, norm1) < 0)
			{
				return Vector3.zero;
			}
			var norm2 = Vector3.Cross(p0 - p2, hitp - p2);
			if(Vector3.Dot(norm, norm2) < 0)
			{
				return Vector3.zero;
			}
			return hitp;
		}

		private static float CalcDotLineDist(Vector3 p0, Vector3 a0, Vector3 b0)
		{
			var abvec = b0 - a0;
			var apvec = p0 - a0;
			var dist = Mathf.Abs(Vector3.Cross(abvec, apvec).magnitude / Vector3.Distance(a0, b0));
			if(dist > Vector3.Distance(p0, a0) || dist > Vector3.Distance(p0, b0))
			{
				return -1.0f;
			}
			return dist;
		}

		private static float CalcDotLineDist2(Vector3 p0, Vector3 a0, Vector3 b0)
		{
			var t0 = Vector3.Dot((b0 - a0).normalized, p0 - a0) / (b0 - a0).magnitude;
			if(t0 < 0.0f)
			{
				return Vector3.Distance(p0, a0);
			}
			if(t0 >= 1.0f)
			{
				return Vector3.Distance(p0, b0);
			}
			var x0 = a0 + (b0 - a0).normalized * Vector3.Distance(a0, b0) * t0;
			var dist = Vector3.Distance(p0, x0);

			//if (t0 < 0.0f) return Vector3.Distance(p0, a0);
			//if (t0 >= 1.0f) return Vector3.Distance(p0, b0);
			return dist;
		}

		private static float CalcDotLineDist2d(Vector2 p0, Vector2 a0, Vector2 b0)
		{
			var t0 = Vector2.Dot((b0 - a0).normalized, p0 - a0) / (b0 - a0).magnitude;
			Vector3 x0 = a0 + (b0 - a0).normalized * Vector2.Distance(a0, b0) * t0;
			var dist = Vector2.Distance(p0, x0);
			if(t0 < 0.0f)
			{
				return -1.0f;
			}
			if(t0 >= 1.0f)
			{
				return -1.0f;
			}

			//if (t0 < 0.0f) return Vector2.Distance(p0, a0);
			//if (t0 >= 1.0f) return Vector2.Distance(p0, b0);
			return dist;
		}

		private static float CalcDotLineDist2d2(Vector2 p0, Vector2 a0, Vector2 b0)
		{
			var dim = Mathf.Abs(CalcCrossVector2(b0 - a0, p0 - a0));
			var d0 = Vector2.Distance(a0, b0);
			if(Vector2.Distance(a0, p0) + Vector2.Distance(p0, b0) > d0 * 1.1f)
			{
				return -1.0f;
			}
			return dim / d0;
		}

		private static float CalcLineLineDist(Vector3 p0, Vector3 v0, Vector3 p1, Vector3 v1, out Vector3 outpos)
		{
			var d0 = Vector3.Dot(p1 - p0, v0);
			var d1 = Vector3.Dot(p1 - p0, v1);
			var c0 = Vector3.Cross(v0, v1);
			var dv = Vector3.SqrMagnitude(c0);
			if(dv < 0.000001f)
			{
				outpos = Vector3.zero;
				return Vector3.Magnitude(Vector3.Cross(p1 - p0, v0));
			}

			var dot0 = Vector3.Dot(v0, v1);
			var t0 = (d0 - d1 * dot0) / (1.0f - dot0 * dot0);
			var t1 = (d1 - d0 * dot0) / (dot0 * dot0 - 1.0f);
			var q0 = p0 + t0 * v0;
			var q1 = p1 + t1 * v1;

			outpos = q1;
			return Vector3.Magnitude(q1 - q0);
		}

		private static Vector3 CalcLineLineIntersect(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			var ab0 = p1 - p0;
			var cd0 = p3 - p2;
			var n0 = ab0.normalized;
			var n1 = cd0.normalized;
			var w0 = Vector3.Dot(n0, n1);
			var w1 = 1.0f - w0 * w0;
			if(w1 == 0.0f)
			{
				return Vector3.zero;
			}
			var ac0 = p2 - p0;
			var d0 = (Vector3.Dot(ac0, n0) - w0 * Vector3.Dot(ac0, n1)) / w1;

			//float d1 = (w0 * Vector3.Dot(ac0, n0) - Vector3.Dot(ac0, n1)) / w1;
			var retvec = Vector3.zero;
			retvec.x = p0.x + d0 * n0.x;
			retvec.y = p0.y + d0 * n0.y;
			retvec.z = p0.z + d0 * n0.z;
			return retvec;
		}

		private static Vector3 CalcHitPosMesh(Mesh mesh, Vector3 rayorg, Vector3 raydir)
		{
			var varr = mesh.vertices;
			var tarr = mesh.triangles;
			var hitpos = Vector3.zero;
			for(var i = 0; i < tarr.Length; i += 3)
			{
				var p0 = varr[tarr[i]];
				var p1 = varr[tarr[i + 1]];
				var p2 = varr[tarr[i + 2]];
				var norm = Vector3.Cross(p1 - p0, p2 - p0).normalized;
				var dot0 = Vector3.Dot(p0 - rayorg, norm);
				var dot1 = Vector3.Dot(rayorg + raydir * CameraSize * 10.0f, norm);
				var tlen = dot0 / dot1;
				if(dot1 >= -0.0001f)
				{
					continue;
				}
				var hitp = rayorg + tlen * (rayorg + raydir * CameraSize * 10.0f);
				var norm0 = Vector3.Cross(p1 - p0, hitp - p0);
				if(Vector3.Dot(norm, norm0) < 0)
				{
					continue;
				}
				var norm1 = Vector3.Cross(p2 - p1, hitp - p1);
				if(Vector3.Dot(norm, norm1) < 0)
				{
					continue;
				}
				var norm2 = Vector3.Cross(p0 - p2, hitp - p2);
				if(Vector3.Dot(norm, norm2) < 0)
				{
					continue;
				}
				hitpos = hitp;
			}

			//return Vector3.zero;
			return hitpos;
		}

		private static string MeshToString(GameObject go, bool flag0)
		{
			var savemesh = new Mesh();
			var meshf = go.GetComponent<MeshFilter>();
			var skinned = go.GetComponent<SkinnedMeshRenderer>();
			if(meshf)
			{
				savemesh = meshf.sharedMesh;
			}
			else if(skinned)
			{
				savemesh = skinned.sharedMesh;
			}
			else
			{
				return "";
			}
			var mesh = new Mesh();
			mesh.Clear();
			if(currentMesh.indexFormat == IndexFormat.UInt32)
			{
				mesh.indexFormat = IndexFormat.UInt32;
			}
			mesh.vertices = savemesh.vertices;
			var triarrlist = new List<int[]>();
			var subcnt = savemesh.subMeshCount;
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(savemesh.GetTriangles(i));
			}
			var triangles1 = savemesh.GetTriangles(0);
			matint = go.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = matint + 1;
			for(var i = 0; i < subcnt; i++)
			{
				mesh.SetTriangles(triarrlist[i], i);
			}
			for(var i = subcnt + 1; i < matint; i++)
			{
				if(triarrlist.Count > i)
				{
					mesh.SetTriangles(triarrlist[i], i);
				}
				else
				{
					mesh.SetTriangles(triangles1, i);
				}
			}
			mesh.colors = savemesh.colors;
			mesh.uv = savemesh.uv;
			mesh.uv2 = savemesh.uv2;
			mesh.uv3 = savemesh.uv3;
			mesh.uv4 = savemesh.uv4;

			//mesh.RecalculateNormals();
			MergetriGenerate(mesh);
			CalcMeshNormals(mesh);
			mesh.RecalculateBounds();

			//if (IsExportMerged) MergeVerts(mesh);
			//New in 2020/06/11
			if(IsExportMerged)
			{
				MergeVertsFast(mesh);
			}

			//End New in 2020/06/11
			var materials = go.GetComponent<Renderer>().sharedMaterials;

			var sb = new StringBuilder();

			sb.Append("g ").Append(mesh.name).Append("\n");
			if(!flag0 || mesh.colors.Length != mesh.vertexCount)
			{
				foreach(var vec in mesh.vertices)
				{
					sb.Append(string.Format("v {0} {1} {2}\n", vec.x, vec.y, vec.z));
				}
			}
			else
			{
				var vertices = mesh.vertices;
				var colors = mesh.colors;
				for(var i = 0; i < vertices.Length; i++)
				{
					var vec = vertices[i];
					var col = colors[i];
					sb.Append(string.Format("v {0} {1} {2} {3} {4} {5}\n", vec.x, vec.y, vec.z, col.r, col.g, col.b));
				}
			}
			sb.Append("\n");
			foreach(var norm in mesh.normals)
			{
				sb.Append(string.Format("vn {0} {1} {2}\n", norm.x, norm.y, norm.z));
			}
			sb.Append("\n");
			foreach(Vector3 uv in mesh.uv)
			{
				sb.Append(string.Format("vt {0} {1}\n", uv.x, uv.y));
			}
			for(var i = 0; i < mesh.subMeshCount; i++)
			{
				//if (materials.Length > i && !materials[i].mainTexture) continue;
				if(materials.Length > i && !materials[i].mainTexture)
				{
					var triangles0 = mesh.GetTriangles(i);
					for(var j = 0; j < triangles0.Length; j += 3)
					{
						sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", triangles0[j] + 1, triangles0[j + 1] + 1, triangles0[j + 2] + 1));
					}
					continue;
				}
				sb.Append("\n");
				if(materials.Length > i)
				{
					sb.Append("usemtl ").Append(materials[i].name).Append("\n");
					sb.Append("usemap ").Append(materials[i].name).Append("\n");
				}
				else if(materials.Length > 0)
				{
					sb.Append("usemtl ").Append(materials[0].name).Append("\n");
					sb.Append("usemap ").Append(materials[0].name).Append("\n");
				}
				var triangles = mesh.GetTriangles(i);
				for(var j = 0; j < triangles.Length; j += 3)
				{
					sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", triangles[j] + 1, triangles[j + 1] + 1, triangles[j + 2] + 1));
				}
			}
			return sb.ToString();
		}

		private static void MeshToFile(GameObject go, string filename, bool flag0)
		{
			using(var sw = new StreamWriter(filename))
			{
				sw.Write(MeshToString(go, flag0));
			}
		}

		private static void SubMeshGenerate(Mesh mesh)
		{
			var triangles = mesh.GetTriangles(0);

			//return;
			if(IsOptimizeTriangles)
			{
				mesh.triangles = triangles;
			}
			else if(!IsPreserveTriangles)
			{
				var subcnt = mesh.subMeshCount;
				matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
				mesh.subMeshCount = matint + 1;
				for(var i = subcnt + 1; i < matint; i++)
				{
					mesh.SetTriangles(triangles, i);
				}
			}
			else
			{
				var trianglelist = new List<int[]>();
				for(var i = 0; i < mesh.subMeshCount; i++)
				{
					trianglelist.Add(mesh.GetTriangles(i));
				}
				matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
				mesh.subMeshCount = matint + 1;
				var subcnt = mesh.subMeshCount;
				for(var i = subcnt + 1; i < matint; i++)
				{
					if(trianglelist.Count > i)
					{
						mesh.SetTriangles(trianglelist[i], i);
					}
					else
					{
						mesh.SetTriangles(triangles, i);
					}
				}
			}
		}

		private static void CheckMonoSubMesh(Mesh mesh)
		{
			IsMonoSubmesh = false;
			var triangles = mesh.GetTriangles(0);
			var subcnt = mesh.subMeshCount;
			if(subcnt < 2)
			{
				IsMonoSubmesh = false;
				return;
			}
			var flag0 = false;
			for(var i = 1; i < subcnt; i++)
			{
				//trianglelist.Add(mesh.GetTriangles(i));
				var triarr = mesh.GetTriangles(i);
				if(triarr.Length != triangles.Length)
				{
					flag0 = true;
					break;
				}
				for(var j = 0; j < triarr.Length; j++)
				{
					if(triarr[j] != triangles[j])
					{
						flag0 = true;
						break;
					}
				}
			}
			if(flag0)
			{
				IsMonoSubmesh = false;

				//Debug.Log("No MonoSubmesh");
				return;
			}

			//Debug.Log("Mono Submesh!");
			IsMonoSubmesh = true;
		}

		private static void CreatePlane()
		{
			currentStatus = SculptStatus.Inactive;
			var go = new GameObject
				{ name = Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath("Assets/Plane.prefab")) + "_EditorSculpt" };
			var meshfilter = ObjectFactory.AddComponent<MeshFilter>(go);
			meshfilter.mesh = new Mesh();
			var mesh = meshfilter.sharedMesh;
			mesh.name = "Plane";
			var v0 = new Vector3(1.0f, 0.0f, 1.0f);
			var v1 = new Vector3(1.0f, 0.0f, -1.0f);
			var v2 = new Vector3(-1.0f, 0.0f, 1.0f);
			var v3 = new Vector3(-1.0f, 0.0f, -1.0f);
			mesh.Clear();
			mesh.vertices = new[] { v0, v1, v2, v3 };
			mesh.triangles = new[] { 0, 1, 2, 3, 2, 1 };
			mesh.colors = Enumerable.Repeat(Color.white, mesh.vertexCount).ToArray();

			//Material mat0 = new Material(Shader.Find("Standard"));
			Material mat0 = null;
			try
			{
				mat0 = GraphicsSettings.currentRenderPipeline.defaultMaterial;
			}
			catch
			{
				mat0 = new Material(Shader.Find("Standard"));
			}
			var mat1 = new Material(Shader.Find("Custom/EditorSculpt"));
			var meshrenderer = ObjectFactory.AddComponent<MeshRenderer>(go);
			meshrenderer.materials = new[] { mat0, mat1 };
			Undo.RegisterCreatedObjectUndo(go, "CreatePlane");
			Selection.activeGameObject = go;
			currentObject = go;
			currentMesh = mesh;
			SubdivideStandard(currentMesh);
			MergeVertsFast(currentMesh);
			IsBrushedArray = Enumerable.Repeat(true, currentMesh.vertexCount).ToArray();
			for(var i = 0; i < primitiveres; i++)
			{
				SubdivideStandard(currentMesh);
			}
			var vertices = currentMesh.vertices;
			var triangles = currentMesh.GetTriangles(0);
			var uvs = currentMesh.uv;
			for(var i = 0; i < vertices.Length; i++)
			{
				var vc = vertices[i];
				uvs[i].x = 1.0f - (vc.x + 1.0f) * 0.5f;
				uvs[i].y = 1.0f - (vc.z + 1.0f) * 0.5f;
			}
			mesh.Clear();
			mesh.vertices = vertices;
			var mint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = mint + 1;
			for(var i = 0; i < mint; i++)
			{
				mesh.SetTriangles(triangles, i);
			}
			mesh.RecalculateBounds();

			//mesh.RecalculateNormals();
			MergetriGenerate(mesh);
			CalcMeshNormals(mesh);
			mesh.colors = Enumerable.Repeat(Color.white, mesh.vertexCount).ToArray();
			mesh.uv = uvs;

			//mesh.uv = Enumerable.Repeat(Vector2.one, mesh.vertexCount).ToArray();
			mesh.uv2 = Enumerable.Repeat(Vector2.one, mesh.vertexCount).ToArray();
			mesh.uv3 = Enumerable.Repeat(Vector2.one, mesh.vertexCount).ToArray();
			mesh.uv4 = Enumerable.Repeat(Vector2.one, mesh.vertexCount).ToArray();
			IsBrushedArray = Enumerable.Repeat(false, mesh.vertexCount).ToArray();

			//BoneWeight1Create(mesh);
			SceneView.lastActiveSceneView.FrameSelected();
		}

		private static void CreateSphere()
		{
			currentStatus = SculptStatus.Inactive;
			var go = new GameObject
				{ name = Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath("Assets/Sphere.prefab")) + "_EditorSculpt" };
			var meshfilter = ObjectFactory.AddComponent<MeshFilter>(go);
			meshfilter.mesh = new Mesh();
			var mesh = meshfilter.sharedMesh;
			mesh.name = "Sphere";
			var v0 = new Vector3(1.0f, 1.0f, -1.0f);
			var v1 = new Vector3(1.0f, -1.0f, -1.0f);
			var v2 = new Vector3(-1.0f, 1.0f, -1.0f);
			var v3 = new Vector3(-1.0f, -1.0f, -1.0f);
			var v4 = new Vector3(1.0f, 1.0f, 1.0f);
			var v5 = new Vector3(1.0f, -1.0f, 1.0f);
			var v6 = new Vector3(-1.0f, 1.0f, 1.0f);
			var v7 = new Vector3(-1.0f, -1.0f, 1.0f);
			mesh.Clear();

			//mesh.vertices = new Vector3[] { v0, v1, v2, v3, v4, v5, v6, v7 };
			mesh.vertices = new[] { v0, v1, v2, v3, v4, v5, v6, v7, v2, v0, v0, v1, v2, v3, v1, v3 };

			//mesh.triangles = new int[] { 0, 1, 2, 3, 2, 1, 0, 4, 1, 4, 5, 1, 6, 4, 2, 4, 0, 2, 7, 3, 5, 5, 3, 1, 2, 7, 6, 2, 3, 7, 6, 7, 4, 4, 7, 5 };
			mesh.triangles = new[] { 0, 1, 2, 3, 2, 1, 10, 4, 11, 4, 5, 11, 6, 4, 8, 4, 9, 8, 7, 15, 5, 5, 15, 14, 12, 7, 6, 12, 13, 7, 6, 7, 4, 4, 7, 5 };
			mesh.uv = new[]
			{
				new Vector2(0.25f, 1.0f), new Vector2(0.25f, 0.75f), new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.75f),
				new Vector2(0.625f, 0.625f), new Vector2(0.625f, 0.375f), new Vector2(0.375f, 0.625f), new Vector2(0.375f, 0.375f),
				new Vector2(0.375f, 0.875f), new Vector2(0.625f, 0.875f), new Vector2(0.875f, 0.625f), new Vector2(0.875f, 0.375f),
				new Vector2(0.125f, 0.625f), new Vector2(0.125f, 0.375f), new Vector2(0.625f, 0.125f), new Vector2(0.375f, 0.125f)
			};
			mesh.colors = Enumerable.Repeat(Color.white, mesh.vertexCount).ToArray();

			//Material mat0 = new Material(Shader.Find("Standard"));
			Material mat0 = null;
			try
			{
				mat0 = GraphicsSettings.currentRenderPipeline.defaultMaterial;
			}
			catch
			{
				mat0 = new Material(Shader.Find("Standard"));
			}
			var mat1 = new Material(Shader.Find("Custom/EditorSculpt"));
			var meshrenderer = ObjectFactory.AddComponent<MeshRenderer>(go);
			meshrenderer.materials = new[] { mat0, mat1 };
			Undo.RegisterCreatedObjectUndo(go, "CreateSphere");
			Selection.activeGameObject = go;
			currentObject = go;
			currentMesh = mesh;
			SubdivideStandard(currentMesh);
			MergeVertsFast(currentMesh);
			IsBrushedArray = Enumerable.Repeat(true, currentMesh.vertexCount).ToArray();
			MergetriGenerate(currentMesh);
			CatmullClarkMerged(currentMesh, 0.5f);
			IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
			for(var i = 0; i < primitiveres; i++)
			{
				SubdivideStandard(currentMesh);
			}
			MergeVertsFast(currentMesh);
			var vertices = currentMesh.vertices;
			var triangles = currentMesh.GetTriangles(0);
			var uvs = currentMesh.uv;
			for(var i = 0; i < vertices.Length; i++)
			{
				var p0 = vertices[i];
				p0 = p0.normalized * 1.0f;
				vertices[i] = p0;
			}
			mesh.Clear();
			mesh.vertices = vertices;
			mesh.uv = uvs;
			var mint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = mint + 1;
			for(var i = 0; i < mint; i++)
			{
				mesh.SetTriangles(triangles, i);
			}
			mesh.RecalculateBounds();
			MergetriGenerate(mesh);
			CalcMeshNormals(mesh);

			//mesh.RecalculateNormals();
			mesh.colors = Enumerable.Repeat(Color.white, mesh.vertexCount).ToArray();
			mesh.uv = uvs;
			mesh.uv2 = Enumerable.Repeat(Vector2.one, mesh.vertexCount).ToArray();
			mesh.uv3 = Enumerable.Repeat(Vector2.one, mesh.vertexCount).ToArray();
			mesh.uv4 = Enumerable.Repeat(Vector2.one, mesh.vertexCount).ToArray();
			IsBrushedArray = Enumerable.Repeat(false, mesh.vertexCount).ToArray();

			//BoneWeight1Create(mesh);
			SceneView.lastActiveSceneView.FrameSelected();
		}

		private static void CreateCube()
		{
			currentStatus = SculptStatus.Inactive;
			var go = new GameObject
				{ name = Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath("Assets/Cube.prefab")) + "_EditorSculpt" };
			var meshfilter = ObjectFactory.AddComponent<MeshFilter>(go);
			meshfilter.mesh = new Mesh();
			var mesh = meshfilter.sharedMesh;
			mesh.name = "Cube";
			var v0 = new Vector3(1.0f, 1.0f, -1.0f);
			var v1 = new Vector3(1.0f, -1.0f, -1.0f);
			var v2 = new Vector3(-1.0f, 1.0f, -1.0f);
			var v3 = new Vector3(-1.0f, -1.0f, -1.0f);
			var v4 = new Vector3(1.0f, 1.0f, 1.0f);
			var v5 = new Vector3(1.0f, -1.0f, 1.0f);
			var v6 = new Vector3(-1.0f, 1.0f, 1.0f);
			var v7 = new Vector3(-1.0f, -1.0f, 1.0f);
			mesh.Clear();

			//mesh.vertices = new Vector3[] { v0, v1, v2, v3, v4, v5, v6, v7 };
			mesh.vertices = new[] { v0, v1, v2, v3, v4, v5, v6, v7, v2, v0, v0, v1, v2, v3, v1, v3 };

			//mesh.triangles = new int[] { 0, 1, 2, 3, 2, 1, 0, 4, 1, 4, 5, 1, 6, 4, 2, 4, 0, 2, 7, 3, 5, 5, 3, 1, 2, 7, 6, 2, 3, 7, 6, 7, 4, 4, 7, 5 };
			mesh.triangles = new[] { 0, 1, 2, 3, 2, 1, 10, 4, 11, 4, 5, 11, 6, 4, 8, 4, 9, 8, 7, 15, 5, 5, 15, 14, 12, 7, 6, 12, 13, 7, 6, 7, 4, 4, 7, 5 };
			mesh.uv = new[]
			{
				new Vector2(0.25f, 1.0f), new Vector2(0.25f, 0.75f), new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.75f),
				new Vector2(0.625f, 0.625f), new Vector2(0.625f, 0.375f), new Vector2(0.375f, 0.625f), new Vector2(0.375f, 0.375f),
				new Vector2(0.375f, 0.875f), new Vector2(0.625f, 0.875f), new Vector2(0.875f, 0.625f), new Vector2(0.875f, 0.375f),
				new Vector2(0.125f, 0.625f), new Vector2(0.125f, 0.375f), new Vector2(0.625f, 0.125f), new Vector2(0.375f, 0.125f)
			};
			mesh.colors = Enumerable.Repeat(Color.white, mesh.vertexCount).ToArray();

			//Material mat0 = new Material(Shader.Find("Standard"));
			Material mat0 = null;
			try
			{
				mat0 = GraphicsSettings.currentRenderPipeline.defaultMaterial;
			}
			catch
			{
				mat0 = new Material(Shader.Find("Standard"));
			}
			var mat1 = new Material(Shader.Find("Custom/EditorSculpt"));
			var meshrenderer = ObjectFactory.AddComponent<MeshRenderer>(go);
			meshrenderer.materials = new[] { mat0, mat1 };
			Undo.RegisterCreatedObjectUndo(go, "CreateCube");
			Selection.activeGameObject = go;
			currentObject = go;
			currentMesh = mesh;
			SubdivideStandard(currentMesh);
			MergeVertsFast(currentMesh);
			IsBrushedArray = Enumerable.Repeat(false, currentMesh.vertexCount).ToArray();
			for(var i = 0; i < primitiveres; i++)
			{
				SubdivideStandard(currentMesh);
			}
			MergeVertsFast(currentMesh);
			var vertices = currentMesh.vertices;
			var triangles = currentMesh.GetTriangles(0);
			var uvs = currentMesh.uv;
			mesh.Clear();
			mesh.vertices = vertices;
			var mint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			mesh.subMeshCount = mint + 1;
			for(var i = 0; i < mint; i++)
			{
				mesh.SetTriangles(triangles, i);
			}
			mesh.RecalculateBounds();

			//mesh.RecalculateNormals();
			MergetriGenerate(mesh);
			CalcMeshNormals(mesh);
			mesh.colors = Enumerable.Repeat(Color.white, mesh.vertexCount).ToArray();
			mesh.uv = uvs;
			mesh.uv2 = Enumerable.Repeat(Vector2.one, mesh.vertexCount).ToArray();
			mesh.uv3 = Enumerable.Repeat(Vector2.one, mesh.vertexCount).ToArray();
			mesh.uv4 = Enumerable.Repeat(Vector2.one, mesh.vertexCount).ToArray();
			IsBrushedArray = Enumerable.Repeat(false, mesh.vertexCount).ToArray();

			//BoneWeight1Create(mesh);
			SceneView.lastActiveSceneView.FrameSelected();
		}

		private static void BoneWeight1Create(Mesh mesh)
		{
			var weightlist = new List<BoneWeight1>();
			var vcount = mesh.vertexCount;
			for(var i = 0; i < vcount; i++)
			{
				var weight1 = new BoneWeight1 { boneIndex = 0, weight = 1.0f };
				weightlist.Add(weight1);
			}
			var perverlist = new List<byte>();
			for(var i = 0; i < vcount; i++)
			{
				perverlist.Add(1);
			}
			var weight1s = new NativeArray<BoneWeight1>(weightlist.ToArray(), Allocator.Temp);
			var PerVerts = new NativeArray<byte>(perverlist.ToArray(), Allocator.Temp);
			mesh.SetBoneWeights(PerVerts, weight1s);
		}

		private static void BoneWeight1Transfar(Mesh mesh)
		{
			var weight1s = currentMesh.GetAllBoneWeights().ToArray();
			var pervers = currentMesh.GetBonesPerVertex().ToArray();
			var newweight1s = new List<BoneWeight1>(weight1s).ToArray();
			var newpervers = new List<byte>(pervers).ToArray();

			var vertices = currentMesh.vertices;
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var bones = currentSkinned.bones;
			var pcnt = 0;
			for(var i = 0; i < pervers.Length; i++)
			{
				boneidxs[i] = pcnt;
				pcnt += pervers[i];
			}
			if(boneidxs.Length < 1)
			{
				boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
			}
			for(var i = 0; i < vertices.Length; i++)
			{
				var pi0 = (int)pervers[i];
				for(var j = 0; j < pi0; j++)
				{
					var bw1 = weight1s[boneidxs[i + j]];
					var bwn = new BoneWeight1();
					bwn.boneIndex = bw1.boneIndex;
					bwn.weight = 1.0f;
					newweight1s[boneidxs[i + j]] = bwn;
				}
			}
			mesh.SetBoneWeights(new NativeArray<byte>(newpervers, Allocator.Temp), new NativeArray<BoneWeight1>(newweight1s, Allocator.Temp));
		}

		private static void BakeAnimationToMesh(string bakepath)
		{
			//AnimationMode.StopAnimationMode();
			var binds = currentMesh.bindposes;
			var tempmesh = new Mesh();
			currentSkinned.BakeMesh(tempmesh);

			//Matrix4x4[] binds = currentMesh.bindposes;
			BoneWeight1Create(tempmesh);

			//BoneWeight1Transfar(tempmesh);
			var weight1s = currentMesh.GetAllBoneWeights().ToArray();
			var pervers = currentMesh.GetBonesPerVertex().ToArray();
			var newweight1s = new List<BoneWeight1>(weight1s).ToArray();
			var newpervers = new List<byte>(pervers).ToArray();
			var vertices = currentMesh.vertices;
			var boneidxs = Enumerable.Repeat(0, pervers.Length).ToArray();
			var bones = currentSkinned.bones;
			var pcnt = 0;
			for(var i = 0; i < pervers.Length; i++)
			{
				boneidxs[i] = pcnt;
				pcnt += pervers[i];
			}
			if(boneidxs.Length < 1)
			{
				boneidxs = Enumerable.Repeat(0, vertices.Length).ToArray();
			}
			for(var i = 0; i < vertices.Length; i++)
			{
				var v0 = currentObject.transform.TransformPoint(vertices[i]);
				var minf0 = 0.0f;
				var minidx = -1;
				for(var j = 0; j < bones.Length; j++)
				{
					var dist = Vector3.Distance(bones[j].position, v0);
					if(dist < minf0 || minidx < 0)
					{
						minidx = j;
					}
				}
				var pi0 = (int)pervers[i];
				for(var j = 0; j < pi0; j++)
				{
					var bw1 = weight1s[boneidxs[i + j]];
					var bwn = new BoneWeight1();

					//if(minidx>=0)bwn.boneIndex = minidx;
					bwn.boneIndex = bw1.boneIndex;

					//bwn.weight = 1.0f;
					bwn.weight = bw1.weight;
					newweight1s[boneidxs[i + j]] = bwn;
				}
			}
			tempmesh.SetBoneWeights(new NativeArray<byte>(newpervers, Allocator.Temp), new NativeArray<BoneWeight1>(newweight1s, Allocator.Temp));

			//AnimationMode.StopAnimationMode();
			//tempmesh.bindposes = currentMesh.bindposes;
			//tempmesh.bindposes = binds;

			//currentSkinned.sharedMesh = tempmesh;
			AssetDatabase.CreateAsset(tempmesh, bakepath);

			//currentSkinned.sharedMesh = tempmesh;
			//currentMesh = tempmesh;

			//EditorSculptPrefab(true);
		}

		private static void BakeAnimationToMesh2(string bakepath)
		{
			var tempmesh = new Mesh();
			currentSkinned.BakeMesh(tempmesh);
			AssetDatabase.CreateAsset(tempmesh, bakepath);
		}

		private static void EditorSculptPrefab(bool IsSkip, bool isOverride)
		{
			//Added 2021/09/14
			if(currentObject == null || currentMesh == null)
			{
				return;
			}

			//End Added 2021/09/14
			var IsLabel = false;
			var strings = AssetDatabase.GetLabels(currentMesh);
			foreach(var tempstr in strings)
			{
				if(tempstr == "EditorSculpt" || tempstr == "EditorSculptImported")
				{
					IsLabel = true;
				}
			}

			//Added 2023/02/28
			//if (IsLabel)
			if(IsLabel && IsSkip)
			{
				IsSaved = true;
				return;
			}

			//End Added 2023/02/28
			GetCurrentMesh(false);
			SubMeshGenerate(currentMesh);
			CheckMonoSubMesh(currentMesh);
			var MaterialList = currentObject.GetComponent<Renderer>().sharedMaterials.ToList();
			var NewMatList = new List<Material>();
			var IsEditorSculptMat = false;
			if(File.Exists(Application.dataPath + "/../" + "Assets/" + currentObject.name + "_EditorSculpt.png") || uvtexture.height > 0)
			{
				IsEditorSculptMat = true;
			}
			if(MaterialList[0].GetTexturePropertyNames().Length > 0)
			{
				IsEditorSculptMat = false;
			}

			//if (MaterialList[0].mainTexture!=null) IsEditorSculptMat = false;
			//if (MaterialList[0].HasProperty("_MainTex")) IsEditorSculptMat = false;
			//Changed 2019/08/26
			//if (!MaterialList[0].HasProperty("_MainTex")) IsEditorSculptMat = false;
			//End Changed 2019/08/26
			for(var i = 0; i < MaterialList.Count; i++)
			{
				var mat = new Material(MaterialList[i]);
				if(mat.shader.name != "Custom/EditorSculpt" || IsEditorSculptMat || IsExportESMat)
				{
					NewMatList.Add(mat);
				}
			}

			//Mesh oldmesh = currentMesh;
			var savemesh = new Mesh();
			EditorUtility.CopySerialized(currentMesh, savemesh);
			var weightarr = currentMesh.GetAllBoneWeights().ToArray();
			var perverarr = currentMesh.GetBonesPerVertex().ToArray();
			var weight1s = new NativeArray<BoneWeight1>(weightarr, Allocator.Temp);
			var PerVerts = new NativeArray<byte>(perverarr, Allocator.Temp);
			savemesh.SetBoneWeights(PerVerts, weight1s);

			//}
			savemesh.bindposes = currentMesh.bindposes;
			savemesh.subMeshCount = matint + 1;
			var AllowOverride = true;
			for(var i = 0; i < matint; i++)
			{
				//savemesh.SetTriangles(currentMesh.GetTriangles(i), i);
				if(currentMesh.isReadable)
				{
					savemesh.SetTriangles(currentMesh.GetTriangles(i), i);
				}
			}
			if(!IsLabel)
			{
				var meshpath = AssetDatabase.GenerateUniqueAssetPath(SaveFolderPath + currentMesh.name + ".asset");

				//String meshpath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentMesh.name + ".asset");
				//if (meshpath != "Assets/" + currentMesh.name + ".asset") AllowOverride = false;
				if(meshpath != SaveFolderPath + currentMesh.name + ".asset")
				{
					AllowOverride = false;
				}
				if(meshpath != "")
				{
					try
					{
						AssetDatabase.StartAssetEditing();
						AssetDatabase.CreateAsset(savemesh, meshpath);
					}
					finally
					{
						AssetDatabase.StopAssetEditing();
					}
					var labellist = AssetDatabase.GetLabels(currentMesh).ToList();
					labellist.Add("EditorSculpt");
					if(IsModelImporter)
					{
						labellist.Add("EditorSculptImported");
					}
					var eslabel = labellist.ToArray();
					AssetDatabase.SetLabels(savemesh, eslabel);
				}
			}
			var matpathlist = new List<string>();
			var SaveMaterialList = new List<Material>();
			if(!IsLabel)
			{
				for(var i = 0; i < NewMatList.Count; i++)
				{
					var mat = new Material(NewMatList[i]);
					var matpath = AssetDatabase.GenerateUniqueAssetPath(SaveFolderPath + currentObject.name + "_"
					                                                    + mat.shader.name.Replace("/", "") + ".mat");

					//String matpath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + currentObject.name + "_"
					//    + mat.shader.name.Replace("/", "") + ".mat");
					//String matpath = EditorUtility.SaveFilePanelInProject("SaveMaterial",
					//    currentObject.name + "_" + mat.shader.name.Replace("/", "") + ".mat","mat","Materila");
					matpathlist.Add(matpath);
					if(matpath != "")
					{
						try
						{
							AssetDatabase.StartAssetEditing();
							AssetDatabase.CreateAsset(mat, matpath);
						}
						finally
						{
							AssetDatabase.StopAssetEditing();
						}
					}
					var maintexst = "";
					try
					{
						//if (mat.HasProperty("_MainTex")) maintexst = AssetDatabase.GetAssetPath(mat.GetTexture("_MainTex"));
						//if (mat.mainTexture!=null) maintexst = AssetDatabase.GetAssetPath(GetMainTexture(mat));
						//if (mat.mainTexture != null) maintexst = AssetDatabase.GetAssetPath(mat.mainTexture);
						if(mat.GetTexturePropertyNames().Length > 0)
						{
							maintexst = AssetDatabase.GetAssetPath(mat.mainTexture);
						}
					}
					catch
					{
					}
					if(maintexst.Length > 0 && !maintexst.Contains("unity_builtin_extra"))
					{
						ImportMatPath = maintexst;
						IsStartTexture = true;
					}

					//else if (mat.HasProperty(("_MainTex")) && (BrushString == "TexturePaint" || BrushString == "BETA_Texture"))
					//else if ((mat.mainTexture!=null) && (BrushString == "TexturePaint" || BrushString == "BETA_Texture"))
					else if(mat.GetTexturePropertyNames().Length > 0 && (BrushString == "TexturePaint" || BrushString == "BETA_Texture"))
					{
						var outtex = new Texture2D(texwidth, texheight);
						var y = 0;
						while(y < outtex.height)
						{
							var x = 0;
							while(x < outtex.width)
							{
								var col = Color.white;
								col.a = 1.0f;
								outtex.SetPixel(x, y, col);
								++x;
							}
							++y;
						}
						outtex.Apply();
						var savetex = new Texture2D(0, 0);
						savetex = outtex;
						try
						{
							AssetDatabase.StartAssetEditing();
							AssetDatabase.AddObjectToAsset(savetex, AssetDatabase.GetAssetPath(mat));
							AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mat));
						}
						finally
						{
							AssetDatabase.StopAssetEditing();
						}

						//SetMainTexture(mat, savetex);
						mat.mainTexture = savetex;
					}
					ImportMatPathList.Add(maintexst);
				}
				for(var i = 0; i < matpathlist.Count; i++)
				{
					var mat = (Material)AssetDatabase.LoadAssetAtPath(matpathlist[i], typeof(Material));
					SaveMaterialList.Add(mat);
				}
			}
			if(currentObject.GetComponent<SkinnedMeshRenderer>())
			{
				if(!IsLabel)
				{
					currentSkinned.sharedMaterials = SaveMaterialList.ToArray();
					currentSkinned.sharedMesh = savemesh;
					currentMesh = savemesh;
				}
				var preroot = currentObject.GetComponent<SkinnedMeshRenderer>().rootBone;
				if(preroot == null)
				{
					preroot = currentObject.transform.root;
				}
				var trans = currentObject.transform.root;

				var prepath = "";
				var coresObj = PrefabUtility.GetCorrespondingObjectFromSource(currentObject);
				var loadpath = CheckEditorSculptGameObj(coresObj) ? AssetDatabase.GetAssetPath(coresObj) : "";

				if(loadpath != "")
				{
					prepath = loadpath;
				}
				else
				{
					var prename = preroot.root.gameObject.name;

					//prepath = "Assets/" + prename + ".prefab";
					prepath = SaveFolderPath + prename + ".prefab";
					if(!CheckAllowOverridePrefab(prepath, savemesh) || !AllowOverride)
					{
						isOverride = false;
					}
					if(!isOverride)
					{
						//if (IsGenerateUniqueName) prepath = AssetDatabase.GenerateUniqueAssetPath(prepath);
						if(!IsOverridePrefab)
						{
							prepath = AssetDatabase.GenerateUniqueAssetPath(prepath);
						}
						prepath = IsPrefabDialog ? EditorUtility.SaveFilePanelInProject("SavePrefrab", prename, "prefab", "SavePrefab") : prepath;
					}
				}
				if(prepath != "")
				{
					var rootobj = PrefabUtility.SaveAsPrefabAssetAndConnect(preroot.root.gameObject, prepath, InteractionMode.AutomatedAction);
					trans.gameObject.name = Path.GetFileNameWithoutExtension(prepath);
					IsSaved = true;

					FixMaterialPrefab(rootobj);
				}
			}
			else if(currentObject.GetComponent<MeshFilter>())
			{
				if(!IsLabel)
				{
					currentObject.GetComponent<MeshRenderer>().sharedMaterials = SaveMaterialList.ToArray();
					currentMeshFilter.sharedMesh = savemesh;
					currentMesh = savemesh;
				}
				var trans = currentObject.transform.root;
				var prepath = "";
				var coresObj = PrefabUtility.GetCorrespondingObjectFromSource(currentObject);
				var loadpath = CheckEditorSculptGameObj(coresObj) ? AssetDatabase.GetAssetPath(coresObj) : "";

				if(loadpath != "")
				{
					prepath = loadpath;
				}
				else
				{
					var prename = trans.gameObject.name;

					//prepath = "Assets/" + prename + ".prefab";
					prepath = SaveFolderPath + prename + ".prefab";
					if(!CheckAllowOverridePrefab(prepath, savemesh) || !AllowOverride)
					{
						isOverride = false;
					}
					if(!isOverride)
					{
						//if (IsGenerateUniqueName) prepath = AssetDatabase.GenerateUniqueAssetPath(prepath);
						if(!IsOverridePrefab)
						{
							prepath = AssetDatabase.GenerateUniqueAssetPath(prepath);
						}
						prepath = IsPrefabDialog ? EditorUtility.SaveFilePanelInProject("SavePrefrab", prename, "prefab", "SavePrefab") : prepath;
					}
				}

				if(prepath != "")
				{
					var rootobj = PrefabUtility.SaveAsPrefabAssetAndConnect(trans.gameObject, prepath, InteractionMode.AutomatedAction);
					trans.gameObject.name = Path.GetFileNameWithoutExtension(prepath);
					IsSaved = true;

					FixMaterialPrefab(rootobj);
				}
			}
			if(window != null)
			{
				if(IsEnable && focusedWindow != window)
				{
					if(Selection.activeGameObject != currentObject && EditorGUIUtility.currentViewWidth > window.position.width)
					{
						Selection.activeGameObject = currentObject;
						window.OnSelectionChange();
					}
				}
			}
			GetCurrentMesh(false);
		}

		private static void FixMaterialPrefab(GameObject go)
		{
			if(!PrefabUtility.IsPartOfAnyPrefab(go))
			{
				return;
			}
			AssetDatabase.OpenAsset(go);

			//List<Material> matlist = new List<Material>();

			var skinneds = go.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach(var skinned in skinneds)
			{
				var matlist = skinned.gameObject.GetComponent<Renderer>().sharedMaterials.ToList();
				var flag0 = false;
				foreach(var mat in matlist)
				{
					if(mat == null)
					{
						flag0 = true;
					}
				}
				if(!flag0)
				{
					continue;
				}
				var newmatList = new List<Material>();
				foreach(var mat in matlist)
				{
					if(mat == null)
					{
						continue;
					}
					newmatList.Add(mat);
				}
				skinned.gameObject.GetComponent<Renderer>().sharedMaterials = newmatList.ToArray();

				//matlist.AddRange(skinned.gameObject.GetComponent<Renderer>().sharedMaterials.ToList());
				//if (skinned.sharedMesh == mesh)
				//{
				//    matlist = skinned.gameObject.GetComponent<Renderer>().sharedMaterials.ToList();
				//}
			}
			var meshfs = go.GetComponentsInChildren<MeshFilter>();
			foreach(var meshf in meshfs)
			{
				var matlist = meshf.gameObject.GetComponent<Renderer>().sharedMaterials.ToList();
				var flag0 = false;
				foreach(var mat in matlist)
				{
					if(mat == null)
					{
						flag0 = true;
					}
				}
				if(!flag0)
				{
					continue;
				}
				var newmatList = new List<Material>();
				foreach(var mat in matlist)
				{
					if(mat == null)
					{
						continue;
					}
					newmatList.Add(mat);
				}
				meshf.gameObject.GetComponent<Renderer>().sharedMaterials = newmatList.ToArray();
			}
			PrefabUtility.SavePrefabAsset(go);
			StageUtility.GoToMainStage();
		}

		private static void SetLabelObject(Object obj, string label)
		{
			if(obj == null)
			{
				return;
			}
			var labellist = AssetDatabase.GetLabels(obj).ToList();
			labellist.Add(label);
			AssetDatabase.SetLabels(obj, labellist.ToArray());
		}

		private static string GetPrefabPath(Mesh mesh, bool IsSkip)
		{
			if(!IsSkip)
			{
				return "";
			}
			var assetgos = AssetDatabase.FindAssets("t:GameObject");
			var pathhash = new HashSet<string>();
			for(var i = 0; i < assetgos.Length; i++)
			{
				pathhash.Add(AssetDatabase.GUIDToAssetPath(assetgos[i]));
			}
			foreach(var str in pathhash)
			{
				var objarr = AssetDatabase.LoadAllAssetsAtPath(str);
				foreach(var obj in objarr)
				{
					try
					{
						if(obj.GetType() == typeof(GameObject))
						{
							var go = (GameObject)obj;
							Mesh tempmesh = null;
							try
							{
								tempmesh = go.GetComponent<SkinnedMeshRenderer>().sharedMesh;
							}
							catch
							{
							}
							if(tempmesh == null)
							{
								try
								{
									tempmesh = go.GetComponent<MeshFilter>().sharedMesh;
								}
								catch
								{
								}
							}
							if(tempmesh != null)
							{
								if(tempmesh == mesh)
								{
									return str;
								}
							}
						}
					}
					catch
					{
					}
				}
			}
			return "";
		}

		private static bool CheckAllowOverridePrefab(string path, Mesh mesh)
		{
			var go = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
			if(go == null)
			{
				return true;
			}
			var meshfs = go.transform.root.gameObject.GetComponentsInChildren<MeshFilter>();
			var skinneds = go.transform.root.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			if(meshfs != null)
			{
				if(meshfs.Length > 0)
				{
					foreach(var meshf in meshfs)
					{
						try
						{
							if(meshf.sharedMesh.name == mesh.name)
							{
								var strings = AssetDatabase.GetLabels(mesh);
								foreach(var tempstr in strings)
								{
									if(tempstr == "EditorSculpt" || tempstr == "EditorSculptImported")
									{
										return false;
									}
								}
							}
						}
						catch
						{
						}
					}
				}
			}
			if(skinneds != null)
			{
				if(skinneds.Length > 0)
				{
					foreach(var skinned in skinneds)
					{
						try
						{
							if(skinned.sharedMesh.name == mesh.name)
							{
								var strings = AssetDatabase.GetLabels(mesh);
								foreach(var tempstr in strings)
								{
									if(tempstr == "EditorSculpt" || tempstr == "EditorSculptImported")
									{
										return false;
									}
								}
							}
						}
						catch
						{
						}
					}
				}
			}
			return true;
		}

		private static void EditorSculptExport()
		{
			if(!currentObject || !currentMesh)
			{
				return;
			}
			var exportpath = EditorUtility.SaveFilePanel("Export OBJ", "", currentObject.name + ".obj", "obj");
			if(exportpath.Length != 0)
			{
				MeshToFile(currentObject, exportpath, true);
			}
			var mtlpath = exportpath.Replace(currentObject.name, "").Replace(".obj", "") + currentObject.name + ".mtl";
			var MaterialList = currentObject.GetComponent<Renderer>().sharedMaterials.ToList();
			var TexNameList = new List<string>();
			var IsEditorSculptMat = false;
			if(File.Exists(Application.dataPath + "/../" + "Assets/" + currentObject.name + "_EditorSculpt.png") || uvtexture.height > 0)
			{
				IsEditorSculptMat = true;
			}
			if(MaterialList[0].GetTexturePropertyNames().Length > 0)
			{
				IsEditorSculptMat = false;
			}

			//if (MaterialList[0].mainTexture!=null) IsEditorSculptMat = false;
			//if (MaterialList[0].HasProperty("_MainTex")) IsEditorSculptMat = false;
			for(var i = 0; i < MaterialList.Count; i++)
			{
				if(currentObject.GetComponent<Renderer>().sharedMaterials[i].shader.name != "Custom/EditorSculpt" || IsEditorSculptMat)
				{
					try
					{
						var texstr = currentObject.name + MaterialList[i].mainTexture.name + i + ".png";
						var matpath = exportpath.Replace(currentObject.name, "").Replace(".obj", "") + texstr;

						//matpath = AssetDatabase.GenerateUniqueAssetPath(matpath);
						texstr = Path.GetFileName(matpath);
						var exmattex = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(MaterialList[i].mainTexture), typeof(Texture2D));

						//Texture2D exmattex = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(GetMainTexture(MaterialList[i])), typeof(Texture2D));
						var bytes = exmattex.EncodeToPNG();
						var exptex = new Texture2D(512, 512);
						exptex.LoadImage(bytes);
						if(IsExportAlpha)
						{
							var y = 0;
							while(y < exptex.height)
							{
								var x = 0;
								while(x < exptex.width)
								{
									var col = exptex.GetPixel(x, y);
									col.a = 1.0f;
									exptex.SetPixel(x, y, col);
									++x;
								}
								++y;
							}
							exptex.Apply();
						}
						bytes = exptex.EncodeToPNG();
						File.WriteAllBytes(matpath, bytes);
						TexNameList.Add(texstr);
					}
					catch
					{
						if(DebugMode)
						{
							Debug.Log("An Error has occured while exportng mesh.");
						}
					}
					;
				}
			}
			if(mtlpath.Length != 0)
			{
				MaterialToFile(currentObject, mtlpath, TexNameList.ToArray());
			}
		}

		private static string MaterialToString(GameObject go, string filename, string[] texnames)
		{
			var mesh = new Mesh();
			var meshf = go.GetComponent<MeshFilter>();
			var skinned = go.GetComponent<SkinnedMeshRenderer>();
			if(meshf)
			{
				mesh = meshf.sharedMesh;
			}
			else if(skinned)
			{
				mesh = skinned.sharedMesh;
			}
			else
			{
				return "";
			}
			var materials = go.GetComponent<Renderer>().sharedMaterials;
			var sb = new StringBuilder();
			var j = 0;
			for(var i = 0; i < materials.Length; i++)
			{
				if(materials[i].shader.name == "Custom/EditorSculpt" && IsExportESMat)
				{
					sb.Append("\n");
					sb.Append(string.Format("newmtl {0}\n", materials[i].name));
					sb.Append("Ka  0.6 0.6 0.6\n");
					sb.Append("Kd  0.6 0.6 0.6\n");
					sb.Append("Ks  0.9 0.9 0.9\n");
					sb.Append("d  1.0\n");
					sb.Append("Ns  0.0\n");
					sb.Append("illum 2\n");

					//sb.Append(string.Format("map_Kd {0}", texnames[i]));
					sb.Append("\n\n\n");
				}
				else if(materials[i].mainTexture)
				{
					sb.Append("\n");
					sb.Append(string.Format("newmtl {0}\n", materials[i].name));
					sb.Append("Ka  0.6 0.6 0.6\n");
					sb.Append("Kd  0.6 0.6 0.6\n");
					sb.Append("Ks  0.9 0.9 0.9\n");
					sb.Append("d  1.0\n");
					sb.Append("Ns  0.0\n");
					sb.Append("illum 2\n");

					//sb.Append(string.Format("map_Kd {0}", texnames[i]));
					sb.Append(string.Format("map_Kd {0}", texnames[j]));
					sb.Append("\n\n\n");
					j++;
				}
			}
			return sb.ToString();
		}

		private static void MaterialToFile(GameObject go, string filename, string[] texnames)
		{
			using(var sw = new StreamWriter(filename))
			{
				sw.Write(MaterialToString(go, filename, texnames));
			}
		}

		private static void BakeVertexColor(Mesh mesh)
		{
			var triangles = mesh.GetTriangles(0);
			var uvs = mesh.uv;
			var colors = mesh.colors;
			var uvboollist = new List<List<bool>>();
			for(var j = 0; j < uvtexture.width + 1; j++)
			{
				var tempbool = new List<bool>();
				for(var k = 0; k < uvtexture.height + 1; k++)
				{
					tempbool.Add(false);
				}
				uvboollist.Add(tempbool);
			}
			for(var i = 0; i < triangles.Length; i += 3)
			{
				var c0 = colors[triangles[i]];
				var c1 = colors[triangles[i + 1]];
				var c2 = colors[triangles[i + 2]];

				var uv0 = uvs[triangles[i]];
				uv0.x *= uvtexture.width;
				uv0.y *= uvtexture.height;
				var uv1 = uvs[triangles[i + 1]];
				uv1.x *= uvtexture.width;
				uv1.y *= uvtexture.height;
				var uv2 = uvs[triangles[i + 2]];
				uv2.x *= uvtexture.width;
				uv2.y *= uvtexture.height;
				var uxmin = (int)Mathf.Min(uv0.x, uv1.x, uv2.x);
				var uxmax = (int)Mathf.Max(uv0.x, uv1.x, uv2.x);
				var uymin = (int)Mathf.Min(uv0.y, uv1.y, uv2.y);
				var uymax = (int)Mathf.Max(uv0.y, uv1.y, uv2.y);
				var uvc = (uv0 + uv1 + uv2) / 3;
				var tu0 = (uv0.y - uv1.y) / (uv0.x - uv1.x);
				var tu1 = (uv1.y - uv2.y) / (uv1.x - uv2.x);
				var tu2 = (uv0.y - uv2.y) / (uv0.x - uv2.x);
				bool uflag0 = false, uflag1 = false, uflag2 = false;
				if(uvc.y > (uvc.x - uv1.x) * tu0 + uv1.y)
				{
					uflag0 = true;
				}
				if(uvc.y > (uvc.x - uv2.x) * tu1 + uv2.y)
				{
					uflag1 = true;
				}
				if(uvc.y > (uvc.x - uv2.x) * tu2 + uv2.y)
				{
					uflag2 = true;
				}
				for(var j = uymin; j < uymax + 1; j++)
				{
					for(var k = uxmin; k < uxmax + 1; k++)
					{
						if(uvboollist[k][j])
						{
							continue;
						}
						var up0 = new Vector2(k, j);

						if(uflag0 && up0.y < (up0.x - uv1.x) * tu0 + uv1.y)
						{
							continue;
						}
						if(!uflag0 && up0.y > (up0.x - uv1.x) * tu0 + uv1.y)
						{
							continue;
						}
						if(uflag1 && up0.y < (up0.x - uv2.x) * tu1 + uv2.y)
						{
							continue;
						}
						if(!uflag1 && up0.y > (up0.x - uv2.x) * tu1 + uv2.y)
						{
							continue;
						}
						if(uflag2 && up0.y < (up0.x - uv2.x) * tu2 + uv2.y)
						{
							continue;
						}
						if(!uflag2 && up0.y > (up0.x - uv2.x) * tu2 + uv2.y)
						{
							continue;
						}

						Vector2 uc0, uc1, u2u1, c1c0, c0u1, ua0, c0c1, u0u1, c1u1, ub0;
						float ta0, tb0, tc0;
						Color cva, cvb, cvp;
						uc0 = up0 + (uv2 - uv0);
						uc1 = up0 + (uv0 - uv2);
						u2u1 = uv2 - uv1;
						c1c0 = uc1 - uc0;
						c0u1 = uc0 - uv1;
						var fc0 = c1c0.x * c0u1.y - c1c0.y * c0u1.x;
						var fc1 = c1c0.x * u2u1.y - c1c0.y * u2u1.x;
						ua0 = uv1 + u2u1 * fc0 / fc1;
						u0u1 = uv0 - uv1;
						c0c1 = uc0 - uc1;
						c1u1 = uc1 - uv1;
						var fc2 = c0c1.x * c1u1.y - c0c1.y * c1u1.x;
						var fc3 = c0c1.x * u0u1.y - c0c1.y * u0u1.x;
						ub0 = uv1 + u0u1 * fc2 / fc3;
						if(fc0 == 0.0f || fc2 == 0.0f)
						{
							continue;
						}
						ta0 = Vector2.Distance(uv1, ua0) / Vector2.Distance(uv1, uv2);
						tb0 = Vector2.Distance(uv1, ub0) / Vector2.Distance(uv1, uv0);
						tc0 = Vector2.Distance(ua0, up0) / Vector2.Distance(ua0, ub0);
						cva = c1 + ta0 * (c2 - c1);
						cvb = c1 + tb0 * (c0 - c1);
						cvp = cva + tc0 * (cvb - cva);
						uvtexture.SetPixel(k, j, cvp);
						uvboollist[k][j] = true;
					}
				}
			}
			uvtexture.Apply();
		}

		private static bool BrushHitPosMethod(Vector3[] varr, List<int[]> tarrlist, Vector3[] rvd)
		{
			IsBrushHitPos = false;
			var hitpos = Vector3.zero;
			var hitnom = Vector3.zero;
			var mindist = 0.0f;
			var minsubidx = 0;
			var ltscale = CameraSize * 10.0f;
			for(var i = 0; i < tarrlist.Count; i++)
			{
				if(IsPaintBrush && i != paintmatidx)
				{
					continue;
				}
				var tarr = tarrlist[i];
				for(var j = 0; j < tarr.Length; j += 3)
				{
					var p0 = varr[tarr[j]];
					var p1 = varr[tarr[j + 1]];
					var p2 = varr[tarr[j + 2]];
					var norm = Vector3.Cross(p1 - p0, p2 - p0).normalized;
					var dot0 = Vector3.Dot(p0 - rvd[0], norm);

					//float dot1 = Vector3.Dot((rvd[0] + rvd[1] * 1000.0f), norm);
					var dot1 = Vector3.Dot(rvd[0] + rvd[1] * ltscale, norm);
					var tlen = dot0 / dot1;
					if(dot1 >= -0.0001f)
					{
						continue;
					}

					//Vector3 hitp = rvd[0] + tlen * (rvd[0] + rvd[1] * 1000.0f);
					var hitp = rvd[0] + tlen * (rvd[0] + rvd[1] * ltscale);
					var norm0 = Vector3.Cross(p1 - p0, hitp - p0);
					if(Vector3.Dot(norm, norm0) < 0)
					{
						continue;
					}
					var norm1 = Vector3.Cross(p2 - p1, hitp - p1);
					if(Vector3.Dot(norm, norm1) < 0)
					{
						continue;
					}
					var norm2 = Vector3.Cross(p0 - p2, hitp - p2);
					if(Vector3.Dot(norm, norm2) < 0)
					{
						continue;
					}
					var dist = Vector3.Distance(hitp, rvd[0]);
					if(mindist == 0.0f || dist < mindist)
					{
						hitpos = hitp;
						hitnom = norm;
						mindist = dist;
						minsubidx = j;
					}
				}
			}
			if(hitpos != Vector3.zero)
			{
				//BrushHitPos = hitpos;
				//BrushHitNorm = hitnom;
				//BrushHitInt = minsubidx;
			}
			return true;
		}

		private static void BrushHitPosCallback(IAsyncResult async)
		{
			IsBrushHitPos = true;
		}

		//private static bool BrushBoneHitPosMethod(Vector3[] varr, List<int[]> tarrlist, Vector3[] rvd)
		//{
		//    IsBrushBonePos = false;
		//    Vector3 hitpos = Vector3.zero;
		//    float mindist = 0.0f;
		//    float ltscale = CameraSize * 10.0f;
		//    for (int i = 0; i < tarrlist.Count; i++)
		//    {
		//        int[] tarr = tarrlist[i];
		//        for (int j = 0; j < tarr.Length; j += 3)
		//        {
		//            Vector3 p0 = varr[tarr[j]];
		//            Vector3 p1 = varr[tarr[j + 1]];
		//            Vector3 p2 = varr[tarr[j + 2]];
		//            Vector3 norm = Vector3.Cross((p1 - p0), (p2 - p0)).normalized;
		//            float dot0 = Vector3.Dot((p0 - rvd[0]), norm);
		//            float dot1 = Vector3.Dot((rvd[0] + rvd[1] * ltscale), norm);
		//            float tlen = dot0 / dot1;
		//            if (dot1 >= -0.0001f) continue;
		//            Vector3 hitp = rvd[0] + tlen * (rvd[0] + rvd[1] * ltscale);
		//            Vector3 norm0 = Vector3.Cross((p1 - p0), (hitp - p0));
		//            if (Vector3.Dot(norm, norm0) < 0) continue;
		//            Vector3 norm1 = Vector3.Cross((p2 - p1), (hitp - p1));
		//            if (Vector3.Dot(norm, norm1) < 0) continue;
		//            Vector3 norm2 = Vector3.Cross((p0 - p2), (hitp - p2));
		//            if (Vector3.Dot(norm, norm2) < 0) continue;
		//            float dist = Vector3.Distance(hitp, rvd[0]);
		//            if (mindist == 0.0f || dist < mindist)
		//            {
		//                hitpos = hitp;
		//                mindist = dist;
		//            }
		//        }
		//    }
		//    BrushBoneHitPos = hitpos;
		//    return true;
		//}

		//private static void BrushBoneHitPosCallback(IAsyncResult async)
		//{
		//    IsBrushBonePos = true;
		//}

		private static Vector3 GetHitPos(Vector3[] varr, int[] tarr, Vector3[] rvd)
		{
			var hitpos = Vector3.zero;
			var ltscale = CameraSize * 10.0f;
			for(var i = 0; i < tarr.Length; i += 3)
			{
				var p0 = varr[tarr[i]];
				var p1 = varr[tarr[i + 1]];
				var p2 = varr[tarr[i + 2]];
				var norm = Vector3.Cross(p1 - p0, p2 - p0).normalized;
				var dot0 = Vector3.Dot(p0 - rvd[0], norm);
				var dot1 = Vector3.Dot(rvd[0] + rvd[1] * ltscale, norm);
				var tlen = dot0 / dot1;
				if(dot1 >= -0.0001f)
				{
					continue;
				}
				var hitp = rvd[0] + tlen * (rvd[0] + rvd[1] * ltscale);
				var norm0 = Vector3.Cross(p1 - p0, hitp - p0);
				if(Vector3.Dot(norm, norm0) < 0)
				{
					continue;
				}
				var norm1 = Vector3.Cross(p2 - p1, hitp - p1);
				if(Vector3.Dot(norm, norm1) < 0)
				{
					continue;
				}
				var norm2 = Vector3.Cross(p0 - p2, hitp - p2);
				if(Vector3.Dot(norm, norm2) < 0)
				{
					continue;
				}
				hitpos = hitp;
			}
			return hitpos;
		}

		private static void Spline3DSave()
		{
			try
			{
				if(Spline3DListList.Count < 1)
				{
					return;
				}
				if(Spline3DListList[Spline3DListList.Count - 1].Count < 1)
				{
					return;
				}
				var cnt0 = 0;
				for(var i = 0; i < 256; i++)
				{
					var flag1 = false;
					flag1 = GameObject.Find(currentObject.name + "/" + "Spline" + i);
					if(!flag1)
					{
						cnt0 = i;
						break;
					}
				}
				if(cnt0 < 256)
				{
					var splineObject = new GameObject { name = "Spline" + cnt0 };
					GameObjectUtility.SetParentAndAlign(splineObject, currentObject);

					var linr = new LineRenderer();
					linr = ObjectFactory.AddComponent<LineRenderer>(splineObject);
					var mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/EditorSculptLine.mat", typeof(Material));
					if(mat == null)
					{
						mat = new Material(Shader.Find("Unlit/Color"));
						AssetDatabase.CreateAsset(mat, "Assets/EditorSculptLine.mat");
					}
					linr.material = mat;

					//linr.material = new Material(Shader.Find("Unlit/Color"));
					linr.sharedMaterial.color = guiLineColor;
					EditorUtility.SetSelectedRenderState(splineObject.GetComponent<Renderer>(), EditorSelectedRenderState.Hidden);
					linr.startColor = guiLineColor;
					linr.endColor = guiLineColor;
					linr.startWidth = 0.02f;
					if(Spline3DListList.Count <= cnt0)
					{
						return;
					}
					linr.useWorldSpace = false;
					linr.positionCount = Spline3DListList[cnt0].Count;
					linr.SetPositions(Spline3DListList[cnt0].ToArray());

					var sp2dObj = new GameObject { name = "2dSpline" };
					GameObjectUtility.SetParentAndAlign(sp2dObj, splineObject);
					var linr2 = new LineRenderer();
					linr2 = ObjectFactory.AddComponent<LineRenderer>(sp2dObj);
					linr2.positionCount = Spline2DListList[cnt0].Count;
					linr2.useWorldSpace = false;
					linr2.SetPositions(Spline2DListList[cnt0].ToArray());

					var dirobj = new GameObject { name = "dirCurve" };
					GameObjectUtility.SetParentAndAlign(dirobj, splineObject);
					var linr3 = new LineRenderer();
					linr3 = ObjectFactory.AddComponent<LineRenderer>(dirobj);
					linr3.positionCount = SplineDirListList[cnt0].Count;

					//linr3.useWorldSpace = false;
					linr3.SetPositions(SplineDirListList[cnt0].ToArray());
					EditorSculptPrefab(false, true);
				}
			}
			catch
			{
			}
		}

		private static void Spline3DUpdate()
		{
			try
			{
				IsSplineUpdate = false;
				var h = 0;
				Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
				foreach(LineRenderer linr in components)
				{
					if(!linr.name.StartsWith("Spline"))
					{
						continue;
					}
					var lineobj = linr.gameObject;
					var lsize = (currentObject.transform.localScale.x + currentObject.transform.localScale.y + currentObject.transform.localScale.z) * 0.333f * 0.02f;
					linr.startWidth = lsize;
					if(!SplineSubdivide)
					{
						linr.positionCount = Spline3DListList[h].Count;
						linr.startColor = guiLineColor;
						linr.endColor = guiLineColor;
						linr.useWorldSpace = false;
						linr.SetPositions(Spline3DListList[h].ToArray());
						var chobj = linr.gameObject;
						Component[] chicomp = linr.gameObject.GetComponentsInChildren<LineRenderer>();
						foreach(LineRenderer linren in chicomp)
						{
							if(linren.name.StartsWith("2d"))
							{
								linren.positionCount = Spline2DListList[h].Count;
								linren.startWidth = 0.0f;
								linren.useWorldSpace = false;
								linren.SetPositions(Spline2DListList[h].ToArray());
							}
							if(linren.name.StartsWith("dir"))
							{
								linren.positionCount = SplineDirListList[h].Count;
								linren.startWidth = 0.0f;
								linren.SetPositions(SplineDirListList[h].ToArray());
							}
						}
					}
					else if(SplineSubDListList.Count > h)
					{
						linr.positionCount = SplineSubDListList[h].Count;
						linr.startColor = guiLineColor;
						linr.endColor = guiLineColor;
						linr.useWorldSpace = false;
						linr.SetPositions(SplineSubDListList[h].ToArray());
					}
					h++;
				}

				//if (BrushString != "BETA_Spline") EditorSculptPrefab(true);
				if(!SplineAsMesh)
				{
					return;
				}
				if(SplineVertListList.Count < 1)
				{
					return;
				}
				h = 0;
				components = currentObject.GetComponentsInChildren<LineRenderer>();
				foreach(LineRenderer linr in components)
				{
					if(!linr.name.StartsWith("Spline"))
					{
						continue;
					}
					if(SplineVertListList.Count <= h)
					{
						break;
					}
					if(Spline3DVertListList.Count <= h)
					{
						break;
					}
					var lineobj = linr.gameObject;
					var splinemf = lineobj.GetComponent<MeshFilter>();
					if(!splinemf)
					{
						splinemf = ObjectFactory.AddComponent<MeshFilter>(lineobj);
						splinemf.mesh = new Mesh();
					}
					else if(splinemf.sharedMesh == null)
					{
						splinemf.mesh = new Mesh();
					}
					if(splinemf)
					{
						var spmesh = splinemf.sharedMesh;
						spmesh.Clear();
						spmesh.vertices = SplineVertListList[h].ToArray();
						spmesh.triangles = SplineTriListList[h].ToArray();
						MeshRenderer spmeshrender;
						spmeshrender = lineobj.GetComponent<MeshRenderer>();

						//if (spmeshrender) spmeshrender.material = new Material(Shader.Find("Standard"));
						if(spmeshrender)
						{
							try
							{
								spmeshrender.material = GraphicsSettings.currentRenderPipeline.defaultMaterial;
							}
							catch
							{
								spmeshrender.material = new Material(Shader.Find("Standard"));
							}
						}
						else
						{
							spmeshrender = ObjectFactory.AddComponent<MeshRenderer>(lineobj);

							//spmeshrender.material = new Material(Shader.Find("Standard"));
							try
							{
								spmeshrender.material = GraphicsSettings.currentRenderPipeline.defaultMaterial;
							}
							catch
							{
								spmeshrender.material = new Material(Shader.Find("Standard"));
							}
						}
					}
					h++;
				}
			}
			catch
			{
			}
		}

		private static void SplineToVertList()
		{
			try
			{
				if(!SplineSubdivide)
				{
					SplineVertListList = new List<List<Vector3>>();
					SplineTriListList = new List<List<int>>();
					for(var i = 0; i < Spline3DListList.Count; i++)
					{
						var vertlist = new List<Vector3>();
						var trilist = new List<int>();
						var ReverseSplineMesh = false;
						if(Spline3DListList[i].Count > 1)
						{
							var vp0 = Spline3DListList[i][0];
							vp0 = currentObject.transform.InverseTransformPoint(vp0);
							var vp1 = Spline3DListList[i][1];
							vp1 = currentObject.transform.InverseTransformPoint(vp1);
							var vp2 = Spline3DListList[i][Spline3DListList[i].Count - 1];
							vp2 = currentObject.transform.InverseTransformPoint(vp2);
							if(Vector3.Dot(SplineDirListList[i][0], Vector3.Cross(vp1 - vp0, vp2 - vp0)) <= 0)
							{
								ReverseSplineMesh = true;
							}
						}
						for(var j = 0; j < Spline3DListList[i].Count; j++)
						{
							var v0 = Spline3DListList[i][j];
							vertlist.Add(v0);
							if(j >= Spline3DListList[i].Count - 1)
							{
								break;
							}
							if(ReverseSplineMesh)
							{
								trilist.AddRange(new[] { j, j + 1, Spline3DListList[i].Count - j - 1 });
							}
							else
							{
								trilist.AddRange(new[] { Spline3DListList[i].Count - j - 1, j + 1, j });
							}
						}
						SplineVertListList.Add(vertlist);
						SplineTriListList.Add(trilist);
					}
					Spline3DVertListList = SplineVertListList;
				}

				if(SplineSubdivide)
				{
					SplineSubDListList = new List<List<Vector3>>();
					var SubDDirListList = new List<List<Vector3>>();
					for(var i = 0; i < Spline3DListList.Count; i++)
					{
						if(Spline3DListList[i].Count > 1)
						{
							var SubDList = new List<Vector3>();
							for(var j = 0; j < Spline3DListList[i].Count - 1; j++)
							{
								var p0 = Spline3DListList[i][j];
								var p1 = Spline3DListList[i][j + 1];
								var q0 = (p0 + p1) * 0.5f;
								SubDList.AddRange(new[] { p0, q0 });
							}
							var vp0 = Spline3DListList[i][Spline3DListList[i].Count - 1];
							var vp1 = Spline3DListList[i][0];
							var vq0 = (vp0 + vp1) * 0.5f;
							SubDList.AddRange(new[] { vp0, vq0 });
							SubDList.AddRange(new[] { vq0, vp1 });
							SplineSubDListList.Add(SubDList);

							var SubDDirList = new List<Vector3>();
							for(var j = 0; j < Spline3DListList[i].Count - 1; j++)
							{
								var d0 = SplineDirListList[i][j];
								var d1 = SplineDirListList[i][j + 1];
								var dq0 = (d0 + d1) * 0.5f;
								SubDDirList.AddRange(new[] { d0, dq0 });
								SubDDirList.AddRange(new[] { dq0, d1 });
							}
							var dp0 = SplineDirListList[i][SplineDirListList[i].Count - 1];
							var dp1 = SplineDirListList[i][0];
							var dpq0 = (dp0 + dp1) * 0.5f;
							SubDDirList.AddRange(new[] { dp0, dpq0 });
							SubDDirList.AddRange(new[] { dpq0, dp1 });
							SubDDirListList.Add(SubDDirList);
						}
						else
						{
							var SplineSubDDirList = new List<Vector3>();
							SplineSubDDirList.AddRange(new[] { Vector3.zero, Vector3.zero });
							SplineSubDListList.Add(SplineSubDDirList);
							var SplineDirList = new List<Vector3>();
							SplineDirList.AddRange(new[] { Vector3.zero, Vector3.zero });
							SubDDirListList.Add(SplineDirList);
						}
					}
					var SubDCalcListList = SplineSubDListList;
					for(var i = 0; i < SplineSubDListList.Count; i++)
					{
						for(var j = 1; j < SplineSubDListList[i].Count - 1; j++)
						{
							var p0 = SplineSubDListList[i][j - 1];
							var p1 = SplineSubDListList[i][j];
							var p2 = SplineSubDListList[i][j + 1];
							var q0 = (p0 + p1) * 0.5f;
							var q1 = (p1 + p2) * 0.5f;
							SubDCalcListList[i][j] = (q0 + q1) * 0.5f;
						}
					}
					SplineSubDListList = SubDCalcListList;

					SplineVertListList = new List<List<Vector3>>();
					SplineTriListList = new List<List<int>>();
					SplineSubDDirListList = new List<List<Vector3>>();
					for(var i = 0; i < SplineSubDListList.Count; i++)
					{
						var vertlist = new List<Vector3>();
						var dirlist = new List<Vector3>();
						var trilist = new List<int>();
						var ReverseSplineMesh = false;
						var centpos = Vector3.zero;
						if(SplineSubDListList[i].Count < 3)
						{
							var vp0p = SplineSubDListList[i][0];
							vertlist.Add(vp0p);
							trilist.AddRange(new[] { 0, 0, 0 });
							dirlist.Add(SubDDirListList[i][0]);
							SplineVertListList.Add(vertlist);
							SplineTriListList.Add(trilist);
							SplineSubDDirListList.Add(dirlist);
							continue;
						}
						for(var j = 0; j < Spline3DListList[i].Count; j++)
						{
							centpos += Spline3DListList[i][j];
						}
						centpos /= Spline3DListList[i].Count;
						var centposi = currentObject.transform.InverseTransformPoint(centpos);
						var vp0 = SplineSubDListList[i][0];
						var vp0i = currentObject.transform.InverseTransformPoint(vp0);
						var vp1 = SplineSubDListList[i][1];
						var vp1i = currentObject.transform.InverseTransformPoint(vp1);
						var vp2 = SplineSubDListList[i][2];
						var pp0 = (vp0 + centpos) * 0.5f;
						var pp1 = (vp0 + vp1) * 0.5f;
						var pp2 = (vp1 + centpos) * 0.5f;
						var pp3 = (vp1 + vp2) * 0.5f;
						var pp4 = (vp2 + centpos) * 0.5f;
						var pp0i = (vp0i + centposi) * 0.5f;
						var pp1i = (vp0i + vp1i) * 0.5f;
						vertlist.AddRange(new[] { vp0, pp0, pp1, pp2, vp1, centpos, pp3, pp4, vp2 });
						if(Vector3.Dot(SplineDirListList[i][0], Vector3.Cross(pp1i - pp0i, vp0i - pp0i)) > 0)
						{
							ReverseSplineMesh = true;
						}
						if(ReverseSplineMesh)
						{
							trilist.AddRange(new[] { 2, 1, 0, 4, 3, 2, 3, 5, 1, 2, 3, 1, 6, 3, 4, 8, 7, 6, 7, 5, 3, 6, 7, 3 });
						}
						else
						{
							trilist.AddRange(new[] { 0, 1, 2, 2, 3, 4, 1, 5, 3, 1, 3, 2, 4, 3, 6, 6, 7, 8, 3, 5, 7, 3, 7, 6 });
						}
						var dp0 = SubDDirListList[i][0];
						var dp1 = SubDDirListList[i][1];
						var dp2 = SubDDirListList[i][2];
						var centdir = Vector3.zero;
						for(var j = 0; j < SplineDirListList[i].Count; j++)
						{
							centdir += SplineDirListList[i][j];
						}
						centdir /= SplineDirListList[i].Count;
						var dpp0 = (dp0 + centdir) * 0.5f;
						var dpp1 = (dp0 + dp1) * 0.5f;
						var dpp2 = (dp1 + centdir) * 0.5f;
						var dpp3 = (dp1 + dp2) * 0.5f;
						var dpp4 = (dp2 + centdir) * 0.5f;
						dirlist.AddRange(new[] { dp0, dpp0, dpp1, dpp2, dp1, centdir, dpp3, dpp4, dp2 });
						for(var j = 3; j < SplineSubDListList[i].Count; j++)
						{
							var v0 = SplineSubDListList[i][j];
							var v1 = Vector3.zero;
							if(j >= SplineSubDListList[i].Count - 1)
							{
								v1 = SplineSubDListList[i][0];
							}
							else
							{
								v1 = SplineSubDListList[i][j + 1];
							}
							var p1 = (v0 + v1) * 0.5f;
							var p2 = (v1 + centpos) * 0.5f;
							vertlist.AddRange(new[] { p1, p2, v1 });
							var n = j * 3;
							if(ReverseSplineMesh)
							{
								trilist.AddRange(new[] { n, n - 2, n - 1, n + 2, n + 1, n, n + 1, 5, n - 2, n, n + 1, n - 2 });
							}
							else
							{
								trilist.AddRange(new[] { n - 1, n - 2, n, n, n + 1, n + 2, n - 2, 5, n + 1, n - 2, n + 1, n });
							}
							var d0 = SubDDirListList[i][j];
							var d1 = Vector3.zero;
							if(j >= SubDDirListList[i].Count - 1)
							{
								d1 = SubDDirListList[i][0];
							}
							else
							{
								d1 = SubDDirListList[i][j + 1];
							}
							dirlist.AddRange(new[] { (d0 + d1) * 0.5f, (d1 + centdir) * 0.5f, d1 });
						}
						SplineVertListList.Add(vertlist);
						SplineTriListList.Add(trilist);
						SplineSubDDirListList.Add(dirlist);
					}

					Spline3DVertListList = SplineVertListList;
				}
			}
			catch
			{
			}
		}

		private static void Spline3DLoad()
		{
			if(!currentObject)
			{
				return;
			}
			if(Spline3DListList.Count < 1)
			{
				Spline2DListList = new List<List<Vector3>>();
				Spline3DListList = new List<List<Vector3>>();
				SplineDirListList = new List<List<Vector3>>();
				SplineSubDListList = new List<List<Vector3>>();
				Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
				foreach(LineRenderer lineren in components)
				{
					if(!lineren.name.StartsWith("Spline"))
					{
						continue;
					}
					lineren.useWorldSpace = false;
					var pos = Enumerable.Repeat(Vector3.zero, lineren.positionCount).ToArray();
					lineren.GetPositions(pos);
					var lineobj = lineren.gameObject;
					Spline3DListList.Add(pos.ToList());

					//SplineDirListList.Add(Enumerable.Repeat(lineobj.transform.eulerAngles, pos.Length).ToList());

					Component[] chdcomps = lineobj.GetComponentsInChildren<LineRenderer>();
					foreach(LineRenderer linr in chdcomps)
					{
						if(linr.name.StartsWith("2d"))
						{
							var chipos = Enumerable.Repeat(Vector3.zero, linr.positionCount).ToArray();
							linr.useWorldSpace = false;
							linr.GetPositions(chipos);
							Spline2DListList.Add(chipos.ToList());
						}
						else if(linr.name.StartsWith("dir"))
						{
							var chipos = Enumerable.Repeat(Vector3.zero, linr.positionCount).ToArray();

							//linr.useWorldSpace = false;
							linr.GetPositions(chipos);
							SplineDirListList.Add(chipos.ToList());
						}
					}
				}
			}
		}

		private static void Spline2Dload()
		{
			if(!currentObject)
			{
				return;
			}
			if(Spline2DListList.Count < 1)
			{
				return;
			}
			if(BrushString == "BETA_Spline" && splinepln != SplinePlane.FREE_3D)
			{
				var splineplanes = Get2DSplinePlane(false);
				var n0 = Vector3.Cross(splineplanes[2] - splineplanes[1], splineplanes[1] - splineplanes[0]);
				var sv0 = splineplanes[0];
				for(var i = 0; i < Spline2DListList.Count; i++)
				{
					for(var j = 0; j < Spline2DListList[i].Count; j++)
					{
						var rv = Spline2DListList[i][j];
						var rd = SplineDirListList[i][j];
						rv -= rd * CameraSize * 100.0f;
						var d0 = Mathf.Abs(Vector3.Dot(rv - sv0, n0)) / Mathf.Abs(Vector3.Dot(rd, n0));
						var r = new Ray(rv, rd);
						Spline2DListList[i][j] = r.GetPoint(d0);
					}
				}
				Spline3DUpdate();
			}
		}

		private static bool SplineMethod(Vector3[] varr, int[] tarr, Vector3[] rvd, int[] ij)
		{
			var hitpos = Vector3.zero;
			for(var i = 0; i < tarr.Length; i += 3)
			{
				var p0 = varr[tarr[i]];
				var p1 = varr[tarr[i + 1]];
				var p2 = varr[tarr[i + 2]];
				var norm = Vector3.Cross(p1 - p0, p2 - p0).normalized;
				var dot0 = Vector3.Dot(p0 - rvd[0], norm);

				//float dot1 = Vector3.Dot((rvd[0] + rvd[1] * 1000.0f), norm);
				var dot1 = Vector3.Dot(rvd[0] + rvd[1] * CameraSize * 10.0f, norm);
				var tlen = dot0 / dot1;
				if(dot1 >= -0.0001f)
				{
					continue;
				}
				var hitp = rvd[0] + tlen * (rvd[0] + rvd[1] * CameraSize * 10.0f);
				var norm0 = Vector3.Cross(p1 - p0, hitp - p0);
				if(Vector3.Dot(norm, norm0) < 0)
				{
					continue;
				}
				var norm1 = Vector3.Cross(p2 - p1, hitp - p1);
				if(Vector3.Dot(norm, norm1) < 0)
				{
					continue;
				}
				var norm2 = Vector3.Cross(p0 - p2, hitp - p2);
				if(Vector3.Dot(norm, norm2) < 0)
				{
					continue;
				}
				hitpos = hitp;
			}
			var idx0 = ij[0];
			var idx1 = ij[1];
			if(hitpos != Vector3.zero)
			{
				//hitpos -= rvd[1] * splineOffset * 0.1f;
				var doff = splineOffset * CameraSize * 0.00001f;
				doff += doff * 0.1f * idx0;
				hitpos -= rvd[1] * doff;
				Spline3DListList[idx0][idx1] = hitpos;
			}
			return true;
		}

		private static void SplineCallback(IAsyncResult async)
		{
			//Spline2DUpdate();
			//Save2DSpline();
		}

		private static bool SplineMeshMethod(Vector3[] varr, int[] tarr, Vector3[] rvd, int[] ij)
		{
			var hitpos = Vector3.zero;
			for(var i = 0; i < tarr.Length; i += 3)
			{
				var p0 = varr[tarr[i]];
				var p1 = varr[tarr[i + 1]];
				var p2 = varr[tarr[i + 2]];
				var norm = Vector3.Cross(p1 - p0, p2 - p0).normalized;
				var dot0 = Vector3.Dot(p0 - rvd[0], norm);
				var dot1 = Vector3.Dot(rvd[0] + rvd[1] * CameraSize * 10.0f, norm);
				var tlen = dot0 / dot1;
				if(dot1 >= -0.0001f)
				{
					continue;
				}
				var hitp = rvd[0] + tlen * (rvd[0] + rvd[1] * CameraSize * 10.0f);
				var norm0 = Vector3.Cross(p1 - p0, hitp - p0);
				if(Vector3.Dot(norm, norm0) < 0)
				{
					continue;
				}
				var norm1 = Vector3.Cross(p2 - p1, hitp - p1);
				if(Vector3.Dot(norm, norm1) < 0)
				{
					continue;
				}
				var norm2 = Vector3.Cross(p0 - p2, hitp - p2);
				if(Vector3.Dot(norm, norm2) < 0)
				{
					continue;
				}
				hitpos = hitp;
			}
			var idx0 = ij[0];
			var idx1 = ij[1];
			if(hitpos != Vector3.zero)
			{
				var doff = splineOffset * CameraSize * 0.00001f;
				doff += doff * 0.1f * idx0;
				hitpos -= rvd[1] * doff;
				Spline3DVertListList[idx0][idx1] = hitpos;
			}
			return true;
		}

		private static void SplineMeshCallback(IAsyncResult async)
		{
			IsSplineUpdate = true;
		}

		private static Vector3[] Get2DSplinePlane(bool flag0)
		{
			var bmin = currentMesh.bounds.min;
			var bmax = currentMesh.bounds.max;
			Vector3 v0 = Vector3.zero, v1 = Vector3.zero, v2 = Vector3.zero, v3 = Vector3.zero;
			switch(splinepln)
			{
				case SplinePlane.XY_Near:
					v0 = new Vector3(bmin.x, bmin.y, bmin.z);
					v1 = new Vector3(bmax.x, bmin.y, bmin.z);
					v2 = new Vector3(bmin.x, bmax.y, bmin.z);
					v3 = new Vector3(bmax.x, bmax.y, bmin.z);
					break;

				case SplinePlane.XY_Far:
					v0 = new Vector3(bmin.x, bmin.y, bmax.z);
					v1 = new Vector3(bmax.x, bmin.y, bmax.z);
					v2 = new Vector3(bmin.x, bmax.y, bmax.z);
					v3 = new Vector3(bmax.x, bmax.y, bmax.z);
					break;

				case SplinePlane.YZ_Near:
					v0 = new Vector3(bmin.x, bmax.y, bmax.z);
					v1 = new Vector3(bmin.x, bmin.y, bmax.z);
					v2 = new Vector3(bmin.x, bmax.y, bmin.z);
					v3 = new Vector3(bmin.x, bmin.y, bmin.z);
					break;

				case SplinePlane.YZ_Far:
					v0 = new Vector3(bmax.x, bmax.y, bmax.z);
					v1 = new Vector3(bmax.x, bmin.y, bmax.z);
					v2 = new Vector3(bmax.x, bmax.y, bmin.z);
					v3 = new Vector3(bmax.x, bmin.y, bmin.z);
					break;

				case SplinePlane.ZX_far:
					v0 = new Vector3(bmax.x, bmax.y, bmax.z);
					v1 = new Vector3(bmax.x, bmax.y, bmin.z);
					v2 = new Vector3(bmin.x, bmax.y, bmax.z);
					v3 = new Vector3(bmin.x, bmax.y, bmin.z);
					break;

				case SplinePlane.ZX_Near:
					v0 = new Vector3(bmax.x, bmin.y, bmax.z);
					v1 = new Vector3(bmin.x, bmin.y, bmax.z);
					v2 = new Vector3(bmax.x, bmin.y, bmin.z);
					v3 = new Vector3(bmin.x, bmin.y, bmin.z);
					break;
			}
			if(flag0)
			{
				v0 = currentObject.transform.TransformPoint(v0);
				v1 = currentObject.transform.TransformPoint(v1);
				v2 = currentObject.transform.TransformPoint(v2);
				v3 = currentObject.transform.TransformPoint(v3);
			}
			return new[] { v0, v1, v3, v2 };
		}

		private static void DecalLoad()
		{
			if(!currentObject)
			{
				return;
			}
			if(Decal2DListList.Count > 0)
			{
				return;
			}
			Decal2DListList = new List<List<Vector3>>();
			Decal3DListList = new List<List<Vector3>>();
			DecalTriListList = new List<List<int>>();
			DecalUVListList = new List<List<Vector2>>();
			DecalDirListList = new List<List<Vector3>>();
			DecalBaseListList = new List<List<Vector3>>();

			Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
			var h = 0;
			foreach(LineRenderer linren in components)
			{
				if(!linren.name.StartsWith("Decal"))
				{
					continue;
				}
				var decalpoints = Enumerable.Repeat(Vector3.zero, linren.positionCount).ToArray();
				linren.useWorldSpace = false;
				linren.GetPositions(decalpoints);
				Decal3DListList.Add(decalpoints.ToList());
				var decalobj = linren.gameObject;
				Component[] chicomps = decalobj.GetComponentsInChildren<LineRenderer>();
				foreach(LineRenderer linr in chicomps)
				{
					if(linr.name.StartsWith("DirDecal"))
					{
						var dirpoints = Enumerable.Repeat(Vector3.zero, linr.positionCount).ToArray();
						linr.useWorldSpace = false;
						linr.GetPositions(dirpoints);
						DecalDirListList.Add(dirpoints.ToList());
					}
					else if(linr.name.StartsWith("Triangles"))
					{
						var tripoints = Enumerable.Repeat(Vector3.zero, linr.positionCount).ToArray();
						linr.useWorldSpace = false;
						linr.GetPositions(tripoints);
						var triarr = new List<int>();
						for(var i = 0; i < tripoints.Length; i++)
						{
							triarr.Add(Mathf.CeilToInt(tripoints[i].x * 100.0f));
							triarr.Add(Mathf.CeilToInt(tripoints[i].y * 100.0f));
							triarr.Add(Mathf.CeilToInt(tripoints[i].z * 100.0f));
						}
						DecalTriListList.Add(triarr.ToList());
					}
					else if(linr.name.StartsWith("UVs"))
					{
						var uvpoints = Enumerable.Repeat(Vector3.zero, linr.positionCount).ToArray();
						linr.useWorldSpace = false;
						linr.GetPositions(uvpoints);
						var uvlist = new List<Vector2>();
						for(var i = 0; i < uvpoints.Length; i++)
						{
							uvlist.Add(new Vector2(uvpoints[i].x, uvpoints[i].y));
						}
						DecalUVListList.Add(uvlist);
					}
					else if(linr.name.StartsWith("Origin"))
					{
						var origins = Enumerable.Repeat(Vector3.zero, linr.positionCount).ToArray();
						linr.useWorldSpace = false;
						linr.GetPositions(origins);
						Decal2DListList.Add(origins.ToList());
					}
					else if(linr.name.StartsWith("Base"))
					{
						var bases = Enumerable.Repeat(Vector3.zero, linr.positionCount).ToArray();
						linr.useWorldSpace = false;
						linr.GetPositions(bases);
						DecalBaseListList.Add(bases.ToList());
					}
				}
				var decalmeshf = decalobj.GetComponent<MeshFilter>();
				var isMeshf = false;
				if(decalmeshf)
				{
					if(decalmeshf.sharedMesh != null)
					{
						var decalmesh = decalmeshf.sharedMesh;
						decalmesh.vertices = decalpoints;
					}
					else
					{
						isMeshf = true;
					}
				}
				else
				{
					isMeshf = true;
				}
				if(isMeshf)
				{
					try
					{
						var decalmesh = new Mesh();
						decalmeshf.sharedMesh = decalmesh;
						decalmesh.vertices = decalpoints;
						decalmesh.uv = DecalUVListList[h].ToArray();
						decalmesh.SetTriangles(DecalTriListList[h].ToArray(), 0);
					}
					catch
					{
					}

					//decalmesh.SetTriangles(DecalTriListList[h].ToArray(), h);
				}
				h++;
			}
		}

		private static void DecalUpdate()
		{
			var h = 0;
			if(Decal2DListList.Count < 1)
			{
				return;
			}
			if(Decal3DListList.Count < 1)
			{
				return;
			}
			Component[] components = currentObject.GetComponentsInChildren<LineRenderer>();
			foreach(LineRenderer linr in components)
			{
				if(!linr.name.StartsWith("Decal"))
				{
					continue;
				}
				linr.positionCount = Decal2DListList[h].Count;
				linr.useWorldSpace = false;
				linr.startWidth = 0.0f;
				linr.SetPositions(Decal2DListList[h].ToArray());
				var chobj = linr.gameObject;
				Component[] chicomp = linr.gameObject.GetComponentsInChildren<LineRenderer>();
				foreach(LineRenderer linren in chicomp)
				{
					if(linren.name.StartsWith("Origin"))
					{
						linren.positionCount = Decal2DListList[h].Count;
						linren.useWorldSpace = false;
						linren.startWidth = 0.0f;
						linren.SetPositions(Decal2DListList[h].ToArray());
					}
					if(linren.name.StartsWith("DirDecal"))
					{
						linren.positionCount = DecalDirListList[h].Count;
						linren.useWorldSpace = false;
						linren.startWidth = 0.0f;
						linren.SetPositions(DecalDirListList[h].ToArray());
					}
					if(linren.name.StartsWith("UVs"))
					{
						linren.positionCount = DecalUVListList[h].Count;
						linren.useWorldSpace = false;
						linren.startWidth = 0.0f;
						var uvlist = new List<Vector3>();
						for(var i = 0; i < DecalUVListList[h].Count; i++)
						{
							var uvpos = DecalUVListList[h][i];
							uvlist.Add(new Vector3(uvpos.x, uvpos.y, 0.0f));
						}
						linren.SetPositions(uvlist.ToArray());
					}
					if(linren.name.StartsWith("Base"))
					{
						linren.positionCount = DecalBaseListList[h].Count;
						linren.useWorldSpace = false;
						linren.startWidth = 0.0f;
						linren.SetPositions(DecalBaseListList[h].ToArray());
					}
				}
				var decalmeshf = chobj.GetComponent<MeshFilter>();
				if(!decalmeshf)
				{
					decalmeshf = ObjectFactory.AddComponent<MeshFilter>(chobj);
					decalmeshf.mesh = new Mesh();
				}
				else
				{
					if(decalmeshf.sharedMesh == null)
					{
						decalmeshf.mesh = new Mesh();
					}
				}
				if(decalmeshf)
				{
					var decalmesh = decalmeshf.sharedMesh;
					decalmesh.Clear();
					decalmesh.vertices = Decal3DListList[h].ToArray();
					decalmesh.triangles = DecalTriListList[h].ToArray();
					decalmesh.uv = DecalUVListList[h].ToArray();
				}
				h++;
			}
		}

		private static bool DecalMeshMethod(Vector3[] varr, int[] tarr, Vector3[] rvd, int[] ij)
		{
			var hitpos = Vector3.zero;
			for(var i = 0; i < tarr.Length; i += 3)
			{
				var p0 = varr[tarr[i]];
				var p1 = varr[tarr[i + 1]];
				var p2 = varr[tarr[i + 2]];
				var norm = Vector3.Cross(p1 - p0, p2 - p0).normalized;
				var dot0 = Vector3.Dot(p0 - rvd[0], norm);
				var dot1 = Vector3.Dot(rvd[0] + rvd[1] * CameraSize * 10.0f, norm);
				var tlen = dot0 / dot1;
				if(dot1 >= -0.0001f)
				{
					continue;
				}
				var hitp = rvd[0] + tlen * (rvd[0] + rvd[1] * CameraSize * 10.0f);
				var norm0 = Vector3.Cross(p1 - p0, hitp - p0);
				if(Vector3.Dot(norm, norm0) < 0)
				{
					continue;
				}
				var norm1 = Vector3.Cross(p2 - p1, hitp - p1);
				if(Vector3.Dot(norm, norm1) < 0)
				{
					continue;
				}
				var norm2 = Vector3.Cross(p0 - p2, hitp - p2);
				if(Vector3.Dot(norm, norm2) < 0)
				{
					continue;
				}
				hitpos = hitp;
			}
			var idx0 = ij[0];
			var idx1 = ij[1];
			if(hitpos != Vector3.zero)
			{
				var doff = decaloffset * CameraSize * 0.00001f;
				doff += doff * 0.1f * idx0;
				hitpos -= rvd[1] * doff;
				Decal3DListList[idx0][idx1] = hitpos;
			}
			return true;
		}

		private static void DecalMeshCallback(IAsyncResult async)
		{
			decalidx++;
		}

		private static void UndoCallbackFunc()
		{
			//if ((currentObject == null) || (currentMesh == null) || (currentMeshFilter == null)) return;
			if(currentObject == null || currentMesh == null)
			{
				return;
			}

			//New in 2020/11/24
			//if (IsShortcut) return;
			var tempstr = "";
			try
			{
				if(Event.current.control && !Event.current.alt && !Event.current.Equals(Event.KeyboardEvent(KeyCode.Z.ToString())))
				{
					tempstr = BrushString;
					BrushString = BrushStringOld;
				}
			}
			catch
			{
			}

			//End New in 2020/11/24
			var vertices = currentMesh.vertices;

			//int[] triangles = currentMesh.GetTriangles(0);
			var subcnt = IsMonoSubmesh ? 1 : currentMesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(currentMesh.GetTriangles(i));
			}
			var colors = currentMesh.colors;
			var uvs = currentMesh.uv;
			var uv2s = currentMesh.uv2;
			var uv3s = currentMesh.uv3;
			var uv4s = currentMesh.uv4;
			var weightarr = currentMesh.GetAllBoneWeights().ToArray();
			var perverarr = currentMesh.GetBonesPerVertex().ToArray();
			var binds = currentMesh.bindposes;

			//BoneWeight[] boneweights = currentMesh.boneWeights;
			var bscnt = currentMesh.blendShapeCount;
			var blendarrlist = new List<Vector3[]>();
			var bnamelist = new List<string>();
			var bweightlist = new List<float>();
			if(bscnt > 0)
			{
				for(var i = 0; i < bscnt; i++)
				{
					var vcnt = currentMesh.vertexCount;
					var varr = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
					var vnorm = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
					var vtan = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
					currentMesh.GetBlendShapeFrameVertices(i, 0, varr, vnorm, vtan);
					blendarrlist.Add(varr);
					bnamelist.Add(currentMesh.GetBlendShapeName(i));
					bweightlist.Add(currentMesh.GetBlendShapeFrameWeight(i, 0));
				}
			}
			currentMesh.Clear();
			currentMesh.vertices = vertices;

			//currentMesh.triangles = triangles;
			currentMesh.colors = colors;
			currentMesh.uv = uvs;
			currentMesh.uv2 = uv2s;
			currentMesh.uv3 = uv3s;
			currentMesh.uv4 = uv4s;
			var weight1s = new NativeArray<BoneWeight1>(weightarr, Allocator.Temp);
			var PerVerts = new NativeArray<byte>(perverarr, Allocator.Temp);

			//if (currentObject.GetComponent<SkinnedMeshRenderer>())
			//{
			if(PerVerts.Length > 0)
			{
				currentMesh.SetBoneWeights(PerVerts, weight1s);
			}
			currentMesh.bindposes = binds;

			//}
			//currentMesh.boneWeights = boneweights;
			if(bscnt > 0)
			{
				for(var i = 0; i < bscnt; i++)
				{
					currentMesh.AddBlendShapeFrame(bnamelist[i], bweightlist[i], blendarrlist[i]
						, Enumerable.Repeat(Vector3.zero, currentMesh.vertexCount).ToArray(), Enumerable.Repeat(Vector3.zero, currentMesh.vertexCount).ToArray());
				}
			}
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			currentMesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint + 1; i++)
			{
				if(IsMonoSubmesh)
				{
					currentMesh.SetTriangles(triarrlist[0], i);
				}
				else
				{
					if(triarrlist.Count > i)
					{
						currentMesh.SetTriangles(triarrlist[i], i);
					}
					else
					{
						currentMesh.SetTriangles(triarrlist[0], i);
					}
				}
			}
			MergetriGenerate(currentMesh);
			ChangeMaterial();
			CalcMeshNormals(currentMesh);
			currentMesh.RecalculateBounds();
			currentMesh.RecalculateTangents();
			avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));

			if(tempstr != "")
			{
				BrushString = tempstr;
			}

			Spline2DListList = new List<List<Vector3>>();
			Spline3DListList = new List<List<Vector3>>();
			SplineSubDListList = new List<List<Vector3>>();
			SplineDirListList = new List<List<Vector3>>();
			Spline3DLoad();

			if(Spline3DListList.Count > 0)
			{
				SplineToVertList();
				Spline3DUpdate();
			}
			Decal2DListList = new List<List<Vector3>>();
			Decal3DListList = new List<List<Vector3>>();
			DecalTriListList = new List<List<int>>();
			DecalUVListList = new List<List<Vector2>>();
			DecalDirListList = new List<List<Vector3>>();
			DecalLoad();
			decalidx = 0;
		}

		private static void RestoreMemoryMesh()
		{
			if(currentObject == null || currentMesh == null || memoryMesh == null)
			{
				return;
			}

			var tempstr = "";
			try
			{
				if(Event.current.control && !Event.current.alt && !Event.current.Equals(Event.KeyboardEvent(KeyCode.Z.ToString())))
				{
					tempstr = BrushString;
					BrushString = BrushStringOld;
				}
			}
			catch
			{
			}
			var vertices = memoryMesh.vertices;
			var subcnt = IsMonoSubmesh ? 1 : memoryMesh.subMeshCount;
			var triarrlist = new List<int[]>();
			for(var i = 0; i < subcnt; i++)
			{
				triarrlist.Add(memoryMesh.GetTriangles(i));
			}
			var colors = memoryMesh.colors;
			var uvs = memoryMesh.uv;
			var uv2s = memoryMesh.uv2;
			var uv3s = memoryMesh.uv3;
			var uv4s = memoryMesh.uv4;
			var weightarr = memoryMesh.GetAllBoneWeights().ToArray();
			var perverarr = memoryMesh.GetBonesPerVertex().ToArray();
			var binds = memoryMesh.bindposes;
			var bscnt = memoryMesh.blendShapeCount;
			var blendarrlist = new List<Vector3[]>();
			var bnamelist = new List<string>();
			var bweightlist = new List<float>();
			if(bscnt > 0)
			{
				for(var i = 0; i < bscnt; i++)
				{
					var vcnt = memoryMesh.vertexCount;
					var varr = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
					var vnorm = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
					var vtan = Enumerable.Repeat(Vector3.zero, vcnt).ToArray();
					memoryMesh.GetBlendShapeFrameVertices(i, 0, varr, vnorm, vtan);
					blendarrlist.Add(varr);
					bnamelist.Add(memoryMesh.GetBlendShapeName(i));
					bweightlist.Add(memoryMesh.GetBlendShapeFrameWeight(i, 0));
				}
			}
			currentMesh.Clear();
			currentMesh.vertices = vertices;

			//currentMesh.triangles = triangles;
			currentMesh.colors = colors;
			currentMesh.uv = uvs;
			currentMesh.uv2 = uv2s;
			currentMesh.uv3 = uv3s;
			currentMesh.uv4 = uv4s;
			var weight1s = new NativeArray<BoneWeight1>(weightarr, Allocator.Temp);
			var PerVerts = new NativeArray<byte>(perverarr, Allocator.Temp);

			//if (currentObject.GetComponent<SkinnedMeshRenderer>())
			//{
			if(PerVerts.Length > 0)
			{
				currentMesh.SetBoneWeights(PerVerts, weight1s);
			}
			currentMesh.bindposes = binds;

			//}
			//currentMesh.boneWeights = boneweights;
			if(bscnt > 0)
			{
				for(var i = 0; i < bscnt; i++)
				{
					currentMesh.AddBlendShapeFrame(bnamelist[i], bweightlist[i], blendarrlist[i]
						, Enumerable.Repeat(Vector3.zero, currentMesh.vertexCount).ToArray(), Enumerable.Repeat(Vector3.zero, currentMesh.vertexCount).ToArray());
				}
			}
			matint = currentObject.GetComponent<Renderer>().sharedMaterials.ToList().Count;
			currentMesh.subMeshCount = matint + 1;
			for(var i = 0; i < matint + 1; i++)
			{
				if(IsMonoSubmesh)
				{
					currentMesh.SetTriangles(triarrlist[0], i);
				}
				else
				{
					if(triarrlist.Count > i)
					{
						currentMesh.SetTriangles(triarrlist[i], i);
					}
					else
					{
						currentMesh.SetTriangles(triarrlist[0], i);
					}
				}
			}
			if(memoryTexBytes != null && uvtexture != null && !IsTexSaved)
			{
				uvtexture.LoadRawTextureData(memoryTexBytes);

				uvtexture.Apply();
				IsTexSaved = true;
			}
#if MemoryAnimation
			if((memoryBindings != null) && (memoryCurves != null) && (!IsAnimationSaved))
			{
				AnimationClip[] aclips = AnimationUtility.GetAnimationClips(currentObject.transform.root.gameObject);
				if(aclips.Length > 0 && aclipidx < aclips.Length)
				{
					AnimationClip aclip = aclips[aclipidx];
					AnimationUtility.SetEditorCurves(aclip, memoryBindings, memoryCurves);

					//foreach(AnimationCurve memoryCurve in memoryCurves)
					//{
					//    foreach(EditorCurveBinding memoryBinding in  memoryBindings)
					//    {
					//        AnimationUtility.SetEditorCurve(aclip, memoryBinding, memoryCurve);
					//    }
					//}
				}
				AnimationPoseLoad(animeslider);
			}
#endif
			MergetriGenerate(currentMesh);
			ChangeMaterial();
			CalcMeshNormals(currentMesh);
			currentMesh.RecalculateBounds();
			currentMesh.RecalculateTangents();
			avgPointDist = GetAveragePointDistance(currentMesh.vertices, currentMesh.GetTriangles(0));

			//Maybe Unsafe!
			////Added 2025/02/22
			//try
			//{
			//    currentSkinned.localBounds = currentMesh.bounds;
			//    EditorSculptPrefab(false, true);
			//}
			//catch { }
			////End Added 2025/02/22
			if(tempstr != "")
			{
				BrushString = tempstr;
			}

			Spline2DListList = new List<List<Vector3>>();
			Spline3DListList = new List<List<Vector3>>();
			SplineSubDListList = new List<List<Vector3>>();
			SplineDirListList = new List<List<Vector3>>();
			Spline3DLoad();

			if(Spline3DListList.Count > 0)
			{
				SplineToVertList();
				Spline3DUpdate();
			}
			Decal2DListList = new List<List<Vector3>>();
			Decal3DListList = new List<List<Vector3>>();
			DecalTriListList = new List<List<int>>();
			DecalUVListList = new List<List<Vector2>>();
			DecalDirListList = new List<List<Vector3>>();
			DecalLoad();
			decalidx = 0;
		}

		/*private static void AssetCreateOverWrite(UnityEngine.Object assset, string expotpath)
		{
		    if (!File.Exists(expotpath))
		    {
		        AssetDatabase.CreateAsset(assset, expotpath);
		        return;
		    }
		    String namestr = Path.GetFileName(expotpath);
		    //String tempdirpath = Path.Combine(expotpath.Replace(namestr, ""), "tempdir");
		    String tempdirpath = expotpath.Replace(namestr, "") + "/tempdir";
		    Directory.CreateDirectory(tempdirpath);
		    //String tempfilepath = Path.Combine(tempdirpath, namestr);
		    String tempfilepath = tempdirpath + "/" + namestr;
		    AssetDatabase.CreateAsset(assset, tempfilepath);
		    FileUtil.ReplaceFile(tempfilepath, expotpath);
		    AssetDatabase.DeleteAsset(tempdirpath);
		    AssetDatabase.ImportAsset(expotpath);
		}*/

		private static Object LoadAssetFastWithoutPath(Object asset, Type type)
		{
			Object retobj = null;
			try
			{
				AssetDatabase.StartAssetEditing();
				retobj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(asset), type);
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
			return retobj;
		}

		private static Object LoadAssetFastWithPath(string path, Type type)
		{
			Object retobj = null;
			try
			{
				AssetDatabase.StartAssetEditing();
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
			return retobj;
		}

		private static string GetAssetPathFast(Object obj)
		{
			string retstr = null;
			try
			{
				AssetDatabase.StartAssetEditing();
				retstr = AssetDatabase.GetAssetPath(obj);
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
			return retstr;
		}

		private static void AddObjectToAssetFast(Object obj, string path)
		{
			try
			{
				AssetDatabase.StartAssetEditing();
				AssetDatabase.AddObjectToAsset(obj, path);
				AssetDatabase.ImportAsset(path);
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}

		private static void CreateAssetFast(Object asset, string path)
		{
			try
			{
				AssetDatabase.StartAssetEditing();
				AssetDatabase.CreateAsset(asset, path);
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}

		private static void CreateAssetAndAddObjFast(Object asset, string path, Object obj)
		{
			try
			{
				AssetDatabase.StartAssetEditing();
				AssetDatabase.CreateAsset(asset, path);
				AssetDatabase.AddObjectToAsset(obj, path);
				AssetDatabase.ImportAsset(path);
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}

		private static void AssetSaveOverride(Object asset, string name, string ext)
		{
			//String saveStr = "Assets/" + name + ext;
			var saveStr = SaveFolderPath + name + ext;
			var savePath = AssetDatabase.GenerateUniqueAssetPath(saveStr);
			if(savePath != saveStr)
			{
				AssetDatabase.CreateAsset(asset, savePath);
				AssetDatabase.DeleteAsset(saveStr);
				AssetDatabase.RenameAsset(savePath, name);
			}
			else
			{
				AssetDatabase.CreateAsset(asset, savePath);
			}

			 
		}
	}  
}  
