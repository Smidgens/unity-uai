// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	// Holds reference to assets used by debug overlays and gizmos
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
		[SerializeField] internal Texture2D _iconAtlas;

		private const float ICO_WIDTH = 0.125f;

		internal UAIAtlasIcon GetAtlasIcon(int x, int y)
		{
			var rect = new Rect(new Vector2(ICO_WIDTH * x, ICO_WIDTH * y), Vector2.one * ICO_WIDTH);
			return new UAIAtlasIcon(rect, _iconAtlas);
		}
	}
}