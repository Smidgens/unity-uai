// smidgens @ github

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using System.Reflection;
	using UnityEngine;
	using UnityEditor;

	internal static class UAIEditorGUI
	{
		public const float INDENT_WIDTH = 15f;

		private delegate bool DoControlFn(Rect r, int id, bool on, bool hover, GUIContent l, GUIStyle s);
		private const BindingFlags _STATIC_BF = BindingFlags.Static | BindingFlags.NonPublic;

		private static DoControlFn _doControlFn;

		// wrapper around internal Unity GUI method
		public static bool DoControl(Rect r, int id, bool on, bool hover, GUIContent l, GUIStyle s)
		{
			if (_doControlFn == null)
			{
				var m = typeof(GUI).GetMethod(nameof(DoControl), _STATIC_BF);
				_doControlFn = (DoControlFn)m?.CreateDelegate(typeof(DoControlFn));
			}
			return _doControlFn!.Invoke(r, id, on, hover, l, s);
		}

		public static void TimedPulse(in Rect rect, float startTime)
		{
			var color = EditorGUIUtility.isProSkin
			? Color.white
			: Color.black;
			color.a = 0.3f;
			float duration = 0.4f;
			var endTime = startTime + duration;
			if (Time.time > endTime)
			{
				return;
			}
			var t = Mathf.Clamp01((endTime - Time.time) / duration);
			t = Mathf.PingPong(t, 2) / 0.5f;
			EditorGUI.DrawRect(rect, Color.Lerp(Color.clear, color, t));
		}

		public static void DrawTypeIcon(Rect rect, ScriptableObject asset, Color color = default)
		{
			var ms = MonoScript.FromScriptableObject(asset);
			var path = AssetDatabase.GetAssetPath(ms);
			Texture ico = AssetDatabase.GetCachedIcon(path);
			if (!ico)
			{
				return;
			}
			if (Mathf.Approximately(color.a, 0f))
			{
				color = Color.white;
			}
			var tc = GUI.color;
			GUI.color = color;
			GUI.DrawTexture(rect, ico, ScaleMode.StretchToFill);
			GUI.color = tc;
		}

		public static string GetFormattedDuration(float timeSeconds)
		{
			if (Mathf.Approximately(timeSeconds, 0f))
			{
				return "0s";
			}

			if (timeSeconds >= 60f)
			{
				return $"{timeSeconds / 60f}m";
			}

			if (timeSeconds < 1f)
			{
				return $"{(int)(timeSeconds * 1000)}ms";
			}
			return $"{(int)(timeSeconds)}s";
		}
	}
	
}

#endif