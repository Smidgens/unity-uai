// smidgens @ github

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;

	// misc editor and window options
	internal static class UAIEditorMenus
	{
		// debug window
		[MenuItem(UAIConstants.DEBUG_MENU)]
		public static void OpenDebugWindow()
		{
			UAIDebugWindow.Open();
		}
	}
}