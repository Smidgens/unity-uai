// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	
	public static class UAIDebugGizmos
	{
		public static void DrawSpere(Vector3 pos, float radius)
		{
			var r = UAIDebugResources.Instance;
			var m = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * (radius * 2f));
			Graphics.DrawMesh(r._locationDebugGizmo, m, r._debugGizmoMaterial, 0);
		}
	}
}