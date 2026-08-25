// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;

	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class UAICurveAttribute : PropertyAttribute
	{
		public UAICurveAttribute
		(
			string color = null,
			float x = 0,
			float y = 0,
			float w = 0,
			float h = 0
		)
		{
			if (ColorUtility.TryParseHtmlString(color, out var c))
			{
				this.color = c;
			}
			ranges = new Rect(x, y, w, h);
		}
		internal Color color { get; } = Color.green;
		internal Rect ranges { get; }
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(UAICurveAttribute))]
	internal sealed class _UAICurveAttribute : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			var attr = (attribute as UAICurveAttribute)!;
			prop.animationCurveValue
			= EditorGUI.CurveField(position, prop.animationCurveValue, attr.color, attr.ranges);
		}
	}
}

#endif