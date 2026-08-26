// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;

	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class UAIIndentAttribute : PropertyAttribute
	{
		internal byte indent { get; }
		public UAIIndentAttribute(byte indent = 1)
		{
			this.indent = indent > 0 ? indent : (byte)1;
		}
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(UAIIndentAttribute))]
	internal sealed class _UAIIndentAttribute : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var level = ((UAIIndentAttribute)attribute).indent;
			EditorGUI.indentLevel += level;
			EditorGUI.PropertyField(position, property, label, true);
			EditorGUI.indentLevel -= level;
		}
	}
}

#endif