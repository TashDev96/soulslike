using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ImageColorExtractorWindow : EditorWindow
{
    private Texture2D _sourceImage;
    private string _targetPath = "/Users/dream/Library/Preferences/Unity/Editor-5.x/Presets/coolClr.colors";
    private int _maxColors = 1000;

    [MenuItem("Tools/Generate/Image Color Extractor")]
    public static void ShowWindow()
    {
        GetWindow<ImageColorExtractorWindow>("Color Extractor");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Extract Colors from Image", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        _sourceImage = (Texture2D)EditorGUILayout.ObjectField("Source Image", _sourceImage, typeof(Texture2D), false);
        if (EditorGUI.EndChangeCheck() && _sourceImage != null)
        {
            string directory = "/Users/dream/Library/Preferences/Unity/Editor-5.x/Presets/";
            _targetPath = Path.Combine(directory, _sourceImage.name + ".colors");
        }

        _targetPath = EditorGUILayout.TextField("Target Path", _targetPath);
        _maxColors = EditorGUILayout.IntField("Max Colors Limit", _maxColors);

        EditorGUILayout.Space();

        if (GUILayout.Button("Extract and Save Colors", GUILayout.Height(30)))
        {
            if (_sourceImage == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a source image.", "OK");
                return;
            }

            ExtractAndSaveColors();
        }
    }

    private void ExtractAndSaveColors()
    {
        string path = AssetDatabase.GetAssetPath(_sourceImage);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected asset is not a texture.", "OK");
            return;
        }

        bool wasReadable = importer.isReadable;
        if (!wasReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        try
        {
            Color32[] pixels = _sourceImage.GetPixels32();
            HashSet<Color32> uniqueColors = new HashSet<Color32>();

            foreach (var color in pixels)
            {
                if (color.a > 10) // Ignore almost transparent pixels
                {
                    uniqueColors.Add(color);
                }
            }

            List<Color32> colorList = uniqueColors.ToList();
            for(int i = 0; i < colorList.Count; i++)
            {
	            var clr = colorList[i];
	            clr.a = 255;
	            colorList[i] = clr;
            }

            if (colorList.Count > _maxColors)
            {
                Debug.LogWarning($"Found {colorList.Count} unique colors. Limiting to {_maxColors}.");
                colorList = colorList.Take(_maxColors).ToList();
            }

            SaveColorsToPreset(colorList);
            
            // Force Unity to refresh the color presets if it's using them
            EditorPrefs.SetString("ColorPresetLibrary_LastPath", _targetPath);
            
            EditorUtility.DisplayDialog("Success", $"Extracted {colorList.Count} unique colors to {_targetPath}", "OK");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to extract colors: {e.Message}\n{e.StackTrace}");
        }
        finally
        {
            if (!wasReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
        }
    }

    private void SaveColorsToPreset(List<Color32> colors)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("%YAML 1.1");
        sb.AppendLine("%TAG !u! tag:unity3d.com,2011:");
        sb.AppendLine("--- !u!114 &1");
        sb.AppendLine("MonoBehaviour:");
        sb.AppendLine("  m_ObjectHideFlags: 52");
        sb.AppendLine("  m_CorrespondingSourceObject: {fileID: 0}");
        sb.AppendLine("  m_PrefabInstance: {fileID: 0}");
        sb.AppendLine("  m_PrefabAsset: {fileID: 0}");
        sb.AppendLine("  m_GameObject: {fileID: 0}");
        sb.AppendLine("  m_Enabled: 1");
        sb.AppendLine("  m_EditorHideFlags: 0");
        sb.AppendLine("  m_Script: {fileID: 12323, guid: 0000000000000000e000000000000000, type: 0}");
        sb.AppendLine("  m_Name: ");
        sb.AppendLine("  m_EditorClassIdentifier: UnityEditor.dll::UnityEditor.ColorPresetLibrary");
        sb.AppendLine("  m_Presets:");

        foreach (var c in colors)
        {
            sb.AppendLine("  - m_Name: ");
            float r = c.r / 255f;
            float g = c.g / 255f;
            float b = c.b / 255f;
            float a = 1;
            sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "    m_Color: {{r: {0:F8}, g: {1:F8}, b: {2:F8}, a:1}}", r, g, b));
        }

        string directory = Path.GetDirectoryName(_targetPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        File.WriteAllText(_targetPath, sb.ToString());
    }
}
