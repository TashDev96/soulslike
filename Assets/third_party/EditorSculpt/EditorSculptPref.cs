using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorSculptPreference
{
    [Serializable]
    public class EditorSculptPref : ScriptableObject
    {
        public bool IsImportDependencies;
        public bool IsLoadAssetMesh;
        public List<bool> boolList = new List<bool>();
        public List<float> floatList = new List<float>();
        public List<int> intList = new List<int>();
        public List<Vector2> vec2List = new List<Vector2>();
        public List<Vector3> vec3List = new List<Vector3>();
        public List<Color> colorList = new List<Color>();
        public List<String> strList = new List<String>();
    }
}
