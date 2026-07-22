// smidgens @ github

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;

	// debug styles for editor windows
	internal sealed class UAIEditorStyles
	{
		public static Color GetIconColor()
		{
			return EditorGUIUtility.isProSkin
			? Color.white
			: Color.black;
		}
		
		public static Color GetIconColorInverse()
		{
			return !EditorGUIUtility.isProSkin
			? Color.white
			: Color.black;
		}
		
		public GUIStyle ToolbarButtonStyle { get; }
		public GUIStyle HeaderLabelStyle { get; }
		public GUIStyle ScoreLabelStyle { get; }
		public GUIStyle BucketLabelStyle { get; }
		public GUIStyle ActionLabelStyle { get; }
		public GUIStyle CooldownLabelStyle { get; }
		public GUIStyle ListButtonStyle { get; }
		public GUIStyle LegendLabelStyle { get; }
		public float ToolbarHeight { get; }
		public float ActionLabelHeight { get; }
		public float BucketLabelHeight { get; }
		public float ListButtonHeight { get; }
		public Vector2 ScoreLabelSize { get; }
		public Vector2 CooldownLabelSize { get; }
		public float LegendLabelHeight { get; }
		public float ScrollbarWidth { get; }
		public float HeaderLabelHeight { get; }

		public static UAIEditorStyles CreateInstance()
		{
			var s = new UAIEditorStyles();
			// note: might want to init styles here to avoid constructor exceptions
			return s;
		}

		private UAIEditorStyles()
		{
			ToolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
			{
				padding = new RectOffset(4,4,4,4),
				stretchWidth = false,
				border = new RectOffset(),
				margin = new RectOffset()
			};

			HeaderLabelStyle = new GUIStyle(EditorStyles.miniLabel)
			{
				// fontSize = (int)(GUI.skin.label.fontSize * 1f),
				// alignment = TextAnchor.MiddleLeft,
				padding = new RectOffset(3,3,3,3),
			};

			BucketLabelStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = (int)(GUI.skin.label.fontSize * 1f),
				alignment = TextAnchor.MiddleLeft,
				padding = new RectOffset(2,2,4,4),
			};
			
			ActionLabelStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = (int)(GUI.skin.label.fontSize * 0.9),
				alignment = TextAnchor.MiddleLeft,
				padding = new RectOffset(2,2,3,3),
			};

			ScoreLabelStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = (int)(GUI.skin.label.fontSize * 0.8),
				alignment = TextAnchor.MiddleRight,
			};

			CooldownLabelStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleRight,
				fontSize = (int)(GUI.skin.label.fontSize * 0.8),
			};

			ListButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
			{
				padding = new RectOffset(5,5,3,3),
				alignment = TextAnchor.MiddleLeft
			};

			LegendLabelStyle = new GUIStyle(GUI.skin.label)
			{
				padding = new RectOffset(2,2,2,2),
				fontSize = (int)(GUI.skin.label.fontSize * 0.9),
			};

			var dummyLabel = new GUIContent("a");

			ToolbarHeight = EditorStyles.toolbarButton.CalcSize(dummyLabel).y;
			ActionLabelHeight = ActionLabelStyle.CalcHeight(dummyLabel, 200);
			BucketLabelHeight = BucketLabelStyle.CalcHeight(dummyLabel, 200);
			ScoreLabelSize = ScoreLabelStyle.CalcSize(new GUIContent("00.0000000"));
			CooldownLabelSize = CooldownLabelStyle.CalcSize(new GUIContent("0000000"));
			ListButtonHeight = ListButtonStyle.CalcHeight(dummyLabel, 100);
			LegendLabelHeight = LegendLabelStyle.CalcHeight(dummyLabel, 100);
			ScrollbarWidth = GUI.skin.verticalScrollbar.CalcSize(GUIContent.none).x;
			HeaderLabelHeight = HeaderLabelStyle.CalcHeight(dummyLabel, 100);
		}

	}
}