// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	//[CreateAssetMenu]
	internal sealed class UAIDebugResources : ScriptableObject
	{
		public static UAIDebugResources Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = Resources.Load<UAIDebugResources>(UAIConstants.ICON_RES_PATH + "/{debug}");
				}
				return _instance;
			}
		}

		private static UAIDebugResources _instance;

		[SerializeField] internal Mesh _locationDebugGizmo;
		[SerializeField] internal Material _debugGizmoMaterial;
	}
}