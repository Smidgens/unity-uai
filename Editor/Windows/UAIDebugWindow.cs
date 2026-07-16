// smidgens @ github

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;

	using UObject = UnityEngine.Object;

	internal class UAIDebugWindow : EditorWindow
	{
		public static class CFG
		{
			public const string TITLE = nameof(UAIDebugWindow);
			public static readonly Type[] DOCK =
			{
				// TODO: replace with project window
				typeof(SceneView),
			};
		}

		public static void Open()
		{
			GetWindow<UAIDebugWindow>(CFG.DOCK).Show();
		}

		private void OnEnable()
		{
			// init
			titleContent.text = CFG.TITLE;
		}

		private void OnDisable()
		{
			// cleanup
		}

		private void OnGUI()
		{
		
		}
	}
}