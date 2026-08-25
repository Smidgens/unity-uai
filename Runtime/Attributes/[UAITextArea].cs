// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;

	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class UAITextAreaAttribute : PropertyAttribute
	{
		internal int minLines { get; }
		internal bool topLabel { get; }
		public UAITextAreaAttribute(int minLines = 2, bool topLabel = true)
		{
			this.minLines = minLines < 1 ? 1 : minLines;
			this.topLabel = topLabel;
		}
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(UAITextAreaAttribute))]
	internal sealed class _UAITextAreaAttribute : PropertyDrawer
	{
		private readonly GUIContent _valueLabel = new();
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var attr = (attribute as UAITextAreaAttribute)!;

			if (label != GUIContent.none)
			{
				if (attr.topLabel)
				{
					var lh = EditorStyles.label.CalcHeight(GUIContent.none, 10f);
					EditorGUI.LabelField(position.SliceTop(lh), label);
					position.SliceTop(EditorGUIUtility.standardVerticalSpacing);
				}
				else
				{
					position = EditorGUI.PrefixLabel(position, label);
				}
			}
			property.stringValue = EditorGUI.TextArea(position, property.stringValue);
		}
		
		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
		{
			var attr = (attribute as UAITextAreaAttribute)!;
			_valueLabel.text = prop.stringValue;
			var minHeight = EditorGUIUtility.singleLineHeight * attr.minLines;
			var h = Mathf.Max(EditorStyles.textArea.CalcHeight(_valueLabel, Screen.width), minHeight);
			if (label != GUIContent.none && attr.topLabel)
			{
				h += EditorStyles.label.CalcHeight(GUIContent.none, 10f);
				h += EditorGUIUtility.standardVerticalSpacing;
			}
			return h;
		}
	}
}

#endif