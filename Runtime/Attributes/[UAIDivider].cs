// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Draws horizontal divider above field
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	internal sealed class UAIDividerAttribute : PropertyAttribute
	{
		public UAIDividerAttribute
		(
			byte marginTop = 4,
			byte marginBottom = 5,
			string color = null
		)
		{
			this.marginTop = marginTop;
			this.marginBottom = marginBottom;
			if (!string.IsNullOrEmpty(color) && ColorUtility.TryParseHtmlString(color, out var c))
			{
				hasColor = true;
				this.color = c;
			}
		}

		internal float marginTop { get; }
		internal float marginBottom { get; }
		internal bool hasColor { get; }
		internal Color color { get; }
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;

	internal static class Color_
	{
		public static Color Fade(this Color c, float a)
		{
			c.a = a;
			return c;
		}
	}

	[CustomPropertyDrawer(typeof(UAIDividerAttribute))]
	internal sealed class _UAIDividerAttribute : DecoratorDrawer
	{
		public override float GetHeight()
		{
			var attr = (attribute as UAIDividerAttribute)!;
			return _SEP_H + attr.marginTop + attr.marginBottom;
		}

		private const float _SEP_H = 1f;

		public static readonly Color SEP_COLOR = EditorGUIUtility.isProSkin
		? Color.white.Fade(0.1f) : Color.black.Fade(0.1f);

		public override void OnGUI(Rect p)
		{
			var attr = (attribute as UAIDividerAttribute)!;
			var pos = p;
			pos.SliceTop(attr.marginTop);
			var sepRect = pos.SliceTop(_SEP_H);
			pos.SliceTop(attr.marginBottom);
			var color = attr.hasColor
			? attr.color
			: SEP_COLOR;
			EditorGUI.DrawRect(sepRect, color);
		}
	}
}

#endif