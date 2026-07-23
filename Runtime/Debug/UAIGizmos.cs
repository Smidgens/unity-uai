// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	public static class UAIGizmos
	{
		public static void DrawSpere(Vector3 pos, float radius)
		{
			var r = UAIGizmoAssets.Instance;
			var m = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * (radius * 2f));
			Graphics.DrawMesh(r._sphereMesh, m, r._material, 0);
		}
	}
}