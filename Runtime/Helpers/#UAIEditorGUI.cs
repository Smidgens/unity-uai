// smidgens @ github

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;

	internal static class UAIEditorGUI
	{
		public const float INDENT_WIDTH = 15f;

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