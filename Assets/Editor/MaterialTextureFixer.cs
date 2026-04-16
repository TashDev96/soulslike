using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class MaterialTextureFixer : EditorWindow
{
    private string materialsPath = "Assets/experiments/test_lesson_animations/spartan-armour-mkv-halo-reach/source/Materials";
    private string texturesPath = "Assets/experiments/test_lesson_animations/spartan-armour-mkv-halo-reach/source/textures";

    [MenuItem("Tools/Fix Spartan Materials")]
    public static void ShowWindow()
    {
        GetWindow<MaterialTextureFixer>("Fix Materials");
    }

    private void OnGUI()
    {
        GUILayout.Label("Material Texture Fixer", EditorStyles.boldLabel);
        
        materialsPath = EditorGUILayout.TextField("Materials Path", materialsPath);
        texturesPath = EditorGUILayout.TextField("Textures Path", texturesPath);

        if (GUILayout.Button("Fix Materials in Folder"))
        {
            FixMaterials(false);
        }

        if (GUILayout.Button("Fix Selected Materials"))
        {
            FixMaterials(true);
        }
    }

    private void FixMaterials(bool selectedOnly)
    {
        string[] materialGuids;
        if (selectedOnly)
        {
            materialGuids = Selection.assetGUIDs;
        }
        else
        {
            materialGuids = AssetDatabase.FindAssets("t:Material", new[] { materialsPath });
        }

        int count = 0;

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            bool changed = FixMaterial(mat);
            if (changed)
            {
                EditorUtility.SetDirty(mat);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Finished! Updated {count} materials.");
    }

    private bool FixMaterial(Material mat)
    {
        bool changed = false;
        string matName = mat.name;
        
        // Map suffixes to properties and toggles
        Dictionary<string, (string texProp, string toggleProp)> mapping = new Dictionary<string, (string, string)>
        {
            { "_BaseColor", ("_MainTex", "_UseColorMap") },
            { "_Colour-Opacity", ("_MainTex", "_UseColorMap") },
            { "_Normal", ("_BumpMap", "_UseNormalMap") },
            { "_Nrm", ("_BumpMap", "_UseNormalMap") },
            { "_Metallic", ("_MetallicGlossMap", "_UseMetallicMap") },
            { "_Roughness", ("_SpecGlossMap", "_UseRoughnessMap") },
            { "_AO", ("_OcclusionMap", "_UseAoMap") },
            { "_Emissive", ("_EmissionMap", "_UseEmissiveMap") }
        };

        // Find all textures in the texture directory
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture", new[] { texturesPath });
        
        foreach (var pair in mapping)
        {
            string suffix = pair.Key;
            string texProp = pair.Value.texProp;
            string toggleProp = pair.Value.toggleProp;

            if (!mat.HasProperty(texProp)) continue;

            // Try exact match first, then common variations
            string[] possibleMatNames = { 
                matName, 
                matName.EndsWith("s") ? matName.Substring(0, matName.Length - 1) : matName + "s",
                matName.Replace("Spartan", "ODST"), // Handle ODST_Shoulder vs Spartan_Shoulder
                matName.Replace("_Shoulders", "_Shoulder")
            };
            
            foreach (string mName in possibleMatNames)
            {
                string targetTexName = mName + suffix;
                bool found = false;

                foreach (string tGuid in textureGuids)
                {
                    string tPath = AssetDatabase.GUIDToAssetPath(tGuid);
                    string fileName = Path.GetFileNameWithoutExtension(tPath);
                    
                    if (fileName.Equals(targetTexName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(tPath);
                        if (tex != null)
                        {
                            mat.SetTexture(texProp, tex);
                            if (mat.HasProperty(toggleProp))
                            {
                                mat.SetFloat(toggleProp, 1.0f);
                            }
                            changed = true;
                            Debug.Log($"Assigned {fileName} to {mat.name}.{texProp}");
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }
        }

        return changed;
    }
}
