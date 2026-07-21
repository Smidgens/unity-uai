// smidgens @ github

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;

	// debug styles for editor windows
	internal sealed class UAIDebugStyles
	{
		public GUIStyle ToolbarButtonStyle { get; }
		public GUIStyle ScoreLabelStyle { get; }
		public GUIStyle BucketLabelStyle { get; }
		public GUIStyle ActionLabelStyle { get; }
		public GUIStyle CooldownLabelStyle { get; }
		public GUIStyle ListButtonLabelStyle { get; }
		public GUIStyle LegendLabelStyle { get; }
		public float ToolbarHeight { get; }
		public float ActionLabelHeight { get; }
		public float BucketLabelHeight { get; }
		public float ListButtonHeight { get; }
		public Vector2 ScoreLabelSize { get; }
		public Vector2 CooldownLabelSize { get; }
		
		public float LegendLabelHeight { get; }
		
		public float ScrollbarWidth { get; }

		public static UAIDebugStyles CreateInstance()
		{
			var s = new UAIDebugStyles();
			// note: might want to init styles here to avoid constructor exceptions
			return s;
		}

		private UAIDebugStyles()
		{

			ToolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
			{
				padding = new RectOffset(4,4,4,4),
			};

			ToolbarButtonStyle.stretchWidth = false;

			ToolbarButtonStyle.border = new RectOffset();
			ToolbarButtonStyle.margin = new RectOffset();
			
			
			BucketLabelStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = (int)(GUI.skin.label.fontSize * 1),
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

			ListButtonLabelStyle = new GUIStyle(GUI.skin.label)
			{
				padding = new RectOffset(2,2,3,3),
			};

			LegendLabelStyle = new GUIStyle(GUI.skin.label)
			{
				padding = new RectOffset(2,2,2,2),
				fontSize = (int)(GUI.skin.label.fontSize * 0.9),
			};

			ToolbarHeight = EditorStyles.toolbarButton.CalcSize(new GUIContent("a")).y;
			ActionLabelHeight = ActionLabelStyle.CalcHeight(new GUIContent("a"), 200);
			BucketLabelHeight = BucketLabelStyle.CalcHeight(new GUIContent("a"), 200);
			ScoreLabelSize = ScoreLabelStyle.CalcSize(new GUIContent("00.0000000"));
			CooldownLabelSize = CooldownLabelStyle.CalcSize(new GUIContent("0000000"));
			ListButtonHeight = ListButtonLabelStyle.CalcHeight(new GUIContent("a"), 100);
			LegendLabelHeight = LegendLabelStyle.CalcHeight(new GUIContent("a"), 100);
			ScrollbarWidth = GUI.skin.verticalScrollbar.CalcSize(GUIContent.none).x;
		}

	}
}