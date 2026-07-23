// smidgens @ github

// resharper disable all

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;
	using UnityEngine;

	internal abstract class UAIMonitorPanel
	{
		public void DrawPanel(Rect area)
		{
			var height = GetContentHeight();

			var scrollRect = new Rect(0, 0, area.width, height);
			
			if (height > area.height)
			{
				scrollRect.width -= GUI.skin.verticalScrollbar.CalcSize(GUIContent.none).x;
			}

			_scroll = GUI.BeginScrollView(area, _scroll, scrollRect);

			DrawScrollContent(scrollRect);

			GUI.EndScrollView();
		}

		protected virtual void DrawScrollContent(Rect area)
		{
			
		}
		
		protected abstract float GetContentHeight();

		private Vector2 _scroll;
	}
}