// smidgens @ github

// Resharper disable all

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using System;
	using UnityEngine;
	using UnityEditor;

	internal static class UAIEditorConstants
	{
		public static Texture2D ContextIcon => _contextButtonIcon.Value;

		private static Lazy<Texture2D> _contextButtonIcon = new (() =>
		{
			return EditorGUIUtility.FindTexture("_Menu");
		});
	}
}

#endif