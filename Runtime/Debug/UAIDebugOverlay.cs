// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;

	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	[Obsolete("Debug info has been moved to UAI debug window: " + UAIConstants.WIN_PATH_DEBUG)]
	internal sealed class UAIDebugOverlay : MonoBehaviour
	{
		[SerializeField] private int _depth;

		private void OnGUI()
		{
			// int td = GUI.depth;
			// GUI.depth = _depth;
			// UAIDebugGUI.DrawActivityOverlay();
			// GUI.depth = td;
		}
	}
}