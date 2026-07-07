// smidgens @ github

// ReSharper disable PossibleNullReferenceException
// ReSharper disable ReplaceSubstringWithRangeIndexer

namespace Smidgenomics.Unity.UAI
{
	using System;
	using System.Diagnostics;
	using UnityEngine;

	[AttributeUsage(AttributeTargets.Field)]
	[Conditional("UNITY_EDITOR")]
	public sealed class EditConditionToggleAttribute : PropertyAttribute
	{
		public string toggleFieldName { get; }

		public EditConditionToggleAttribute(string fieldName)
		{
			toggleFieldName = fieldName;
		}
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using SP = UnityEditor.SerializedProperty;
	
	[CustomPropertyDrawer(typeof(EditConditionToggleAttribute))]
	internal class _EditConditionToggleAttribute : PropertyDrawer
	{
		public override void OnGUI(Rect pos, SP prop, GUIContent l)
		{
			var tIndent = EditorGUI.indentLevel;
			
			// label not blank and item not inside array
			if(l != GUIContent.none && !fieldInfo.FieldType.IsArray)
			{
				pos = EditorGUI.PrefixLabel(pos, l);
			}
			
			EditorGUI.indentLevel = 0;

			var attr = attribute as EditConditionToggleAttribute;

			var fieldName = prop.name;
			var basePath = prop.propertyPath.Substring(0, prop.propertyPath.Length - fieldName.Length);
			var togglePath = $"{basePath}{attr.toggleFieldName}";
			var toggleProp = prop.serializedObject.FindProperty(togglePath);

			if (toggleProp is not { propertyType: SerializedPropertyType.Boolean })
			{
				EditorGUI.indentLevel = tIndent;
				return;
			}
		
			using (new EditorGUI.PropertyScope(pos, l, prop))
			{
				var toggleRect = pos.SliceLeft(18);
				toggleRect = toggleRect.SliceTop(18);
				
				EditorGUI.PropertyField(toggleRect, toggleProp, GUIContent.none);
				bool tempEnabled = GUI.enabled;
				GUI.enabled = toggleProp.boolValue;
				EditorGUI.PropertyField(pos, prop, GUIContent.none);
				GUI.enabled = tempEnabled;

			}

			EditorGUI.indentLevel = tIndent;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}
	}

}

#endif