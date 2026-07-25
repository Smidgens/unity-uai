// smidgens @ github

// resharper disable all

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;
	using UnityEngine;

	internal abstract class UAIMonitorPanel
	{
		public virtual string GetTabLabel()
		{
			return string.Empty;
		}

		public virtual EUAIAtlasIcon GetTabIcon()
		{
			return EUAIAtlasIcon.None;
		}

		public void DrawPanel(in Rect area)
		{
			var height = GetContentHeight();

			var scrollRect = new Rect(0, 0, area.width, height);
			
			if (height > area.height)
			{
				scrollRect.width -= GUI.skin.verticalScrollbar.CalcSize(GUIContent.none).x;
			}

			_scroll = GUI.BeginScrollView(area, _scroll, scrollRect);
			OnDrawContent(scrollRect);
			GUI.EndScrollView();
		}

		protected virtual float GetContentWidth()
		{
			return 0f;
		}

		protected virtual void OnDrawContent(Rect area)
		{
			
		}

		protected virtual float GetContentHeight() => 0f;

		private Vector2 _scroll;
	}
}


namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;
	using UnityEngine;

	internal sealed class UAIMonitorPanel_Legend : UAIMonitorPanel
	{
		protected override void OnDrawContent(Rect area)
		{
			
		}

		protected override float GetContentHeight()
		{
			return 0f;
		}

	}
}