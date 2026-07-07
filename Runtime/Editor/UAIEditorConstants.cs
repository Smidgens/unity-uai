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

		public const string DEFAULT_CONSIDERATION_ICON_GUID = "d8ec218438d247b49a3a0f61ed39664d";
		public const string DEFAULT_ACTION_ICON_GUID = "b403041b6ec9a3744b4e92bc8014f7f6";

		public static Texture2D ContextIcon => _contextButtonIcon.Value;

		private static Lazy<Texture2D> _contextButtonIcon = new (() =>
		{
			return EditorGUIUtility.FindTexture("_Menu");
		});
	}
}

#endif