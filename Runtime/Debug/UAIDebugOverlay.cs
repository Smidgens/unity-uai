// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	[AddComponentMenu(UAIConstants.AC_PATH + "UAI Debug Overlay")]
	[DisallowMultipleComponent]
	internal sealed class UAIDebugOverlay : MonoBehaviour
	{
		[SerializeField] private int _depth;

		private void OnGUI()
		{
			int td = GUI.depth;
			GUI.depth = _depth;
			UAIDebugGUI.DrawActivityOverlay();
			GUI.depth = td;
		}
	}
}