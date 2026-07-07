// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	[AddComponentMenu("Smidgenomics/Utility AI/UAI Debug Overlay")]
	[DisallowMultipleComponent]
	internal sealed class UAIDebugOverlay : MonoBehaviour
	{
		private void OnGUI()
		{
			UAIDebugGUI.DrawActivityOverlay();
		}
	}
}