using UnityEditor;
using UnityEngine;

namespace editor
{
	public class DropToGround : Editor
	{
		[MenuItem("Tools/Drop to Ground %g")] // Ctrl+G (Cmd+G on Mac)
		static void DropSelected()
		{
			foreach (var obj in Selection.gameObjects)
			{
				var layer = LayerMask.GetMask("Default", "LevelGeometry");
				if (Physics.Raycast(obj.transform.position + Vector3.up * 100f, Vector3.down,  out RaycastHit hit, 200f, layer))
				{
					Undo.RecordObject(obj.transform, "Drop To Ground");
					obj.transform.position = hit.point;
				}
			}
		}
	}
}
