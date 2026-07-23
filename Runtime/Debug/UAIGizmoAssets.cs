// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	// Holds reference to assets used by debug overlays and gizmos
	internal sealed class UAIGizmoAssets : ScriptableObject
	{
		public static UAIGizmoAssets Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = Resources.Load<UAIGizmoAssets>(UAIConstants.RES_PATH + "/{gizmos}");
				}
				return _instance;
			}
		}

		private static UAIGizmoAssets _instance;
		[SerializeField] internal Mesh _sphereMesh;
		[SerializeField] internal Material _material;
	}
}