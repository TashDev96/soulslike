using System;
using System.Collections.Generic;
using UnityEngine;

namespace EditorSculptPreference
{
	[Serializable]
	public class EditorSculptPref : ScriptableObject
	{
		public bool IsImportDependencies;
		public bool IsLoadAssetMesh;
		public List<bool> boolList = new();
		public List<float> floatList = new();
		public List<int> intList = new();
		public List<Vector2> vec2List = new();
		public List<Vector3> vec3List = new();
		public List<Color> colorList = new();
		public List<string> strList = new();
	}
}
