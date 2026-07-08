// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	/// <summary>
	/// Holds reference to assets used by debug overlays and gizmos
	/// </summary>
	internal sealed class UAIDebugResources : ScriptableObject
	{
		public static UAIDebugResources Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = Resources.Load<UAIDebugResources>(UAIConstants.RES_PATH + "/{debug}");
				}
				return _instance;
			}
		}

		private static UAIDebugResources _instance;

		[SerializeField] internal Mesh _locationDebugGizmo;
		[SerializeField] internal Material _debugGizmoMaterial;
	}
}