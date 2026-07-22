// smidgens @ github

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;

	// misc editor and window options
	internal static class UAIEditorMenus
	{
		// debug window
		[MenuItem(UAIConstants.WIN_PATH_DEBUG)]
		public static void OpenDebugWindow()
		{
			UAIWindow_Monitor.Open();
		}
	}
}