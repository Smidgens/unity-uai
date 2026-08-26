// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;
	using Conditional = System.Diagnostics.ConditionalAttribute;

	// display type fields expanded
	[AttributeUsage(AttributeTargets.Field)]
	[Conditional("UNITY_EDITOR")]
	internal sealed class UAIExpandAttribute : PropertyAttribute
	{
		public UAIExpandAttribute(bool innerOnly = false)
		{
			this.innerOnly = innerOnly;
		}
		internal bool innerOnly { get; }
	}
}


#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;
	using UnityEngine;
	using System.Reflection;
	using System.Collections.Generic;

	[CustomPropertyDrawer(typeof(UAIExpandAttribute))]
	internal sealed class _UAIExpandAttribute : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			EnsureInit();

			var h = 0f;

			var attr =  (attribute as UAIExpandAttribute)!;

			if (!attr.innerOnly)
			{
				h += EditorStyles.label.CalcSize(GUIContent.none).y;
				h += EditorGUIUtility.standardVerticalSpacing;
			}

			h += Mathf.Max(_fields.Count - 1, 0f) * EditorGUIUtility.standardVerticalSpacing;
			// property.isExpanded = true;
			foreach (var f in _fields)
			{
				var prop = property.FindPropertyRelative(f.Name);
				h += EditorGUI.GetPropertyHeight(prop);
			}
			return h;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var attr =  (attribute as UAIExpandAttribute)!;
			EditorGUI.BeginProperty(position, label, property);

			if (!attr.innerOnly)
			{
				var lHeight = EditorStyles.label.CalcSize(GUIContent.none).y;
				var lrow = position.SliceTop(lHeight);
				position.SliceTop(EditorGUIUtility.standardVerticalSpacing);
				EditorGUI.LabelField(lrow, label);
			}

			var extraIndent = attr.innerOnly ? 0 : 1;

			var tIndent = EditorGUI.indentLevel;

			if (fieldInfo.FieldType.IsArray)
			{
				extraIndent = 0;
				EditorGUI.indentLevel = 0;
			}

			EditorGUI.indentLevel += extraIndent;

			int i = -1;
			foreach (var f in _fields)
			{
				i++;
				var prop = property.FindPropertyRelative(f.Name);
				var frow = position.SliceTop(EditorGUI.GetPropertyHeight(prop));
				EditorGUI.PropertyField(frow, prop);
				if (i < _fields.Count - 1)
				{
					position.SliceTop(EditorGUIUtility.standardVerticalSpacing);
				}
			}

			EditorGUI.indentLevel = tIndent;
			EditorGUI.EndProperty();
		}

		private IReadOnlyList<FieldInfo> _fields;
		private bool _init;

		private void EnsureInit()
		{
			if (_init)
			{
				return;
			}

			var innerType = fieldInfo.FieldType;

			if (innerType.IsArray)
			{
				innerType = innerType.GetElementType();
			}
			_fields = innerType.FindInspectorFields<object>();
		}
	}

}

#endif