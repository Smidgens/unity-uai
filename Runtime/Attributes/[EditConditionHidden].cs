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
	internal sealed class EditConditionHiddenAttribute : PropertyAttribute
	{
		public string toggleFieldName { get; }

		public EditConditionHiddenAttribute(string fieldName)
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
	
	[CustomPropertyDrawer(typeof(EditConditionHiddenAttribute))]
	internal class _EditConditionHiddenAttribute : PropertyDrawer
	{
		public override void OnGUI(Rect pos, SP prop, GUIContent l)
		{
			if (pos.height == 0)
			{
				return;
			}
			
			var tIndent = EditorGUI.indentLevel;
			
			// label not blank and item not inside array
			if(l != GUIContent.none && !fieldInfo.FieldType.IsArray)
			{
				pos = EditorGUI.PrefixLabel(pos, l);
			}
			
			EditorGUI.indentLevel = 0;

			var attr = attribute as EditConditionHiddenAttribute;

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
				GUI.enabled = toggleProp.boolValue;
				EditorGUI.PropertyField(pos, prop, GUIContent.none);
			}

			EditorGUI.indentLevel = tIndent;
		}

		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
		{
			if (prop == null)
			{
				return 0;
			}
			var fieldName = prop.name;
			var attr = attribute as EditConditionHiddenAttribute;
			var basePath = prop.propertyPath.Substring(0, prop.propertyPath.Length - fieldName.Length);
			var togglePath = $"{basePath}{attr.toggleFieldName}";
			var toggleProp = prop.serializedObject.FindProperty(togglePath);

			if (toggleProp is not { propertyType: SerializedPropertyType.Boolean })
			{
				return 0;
			}

			if (!toggleProp.boolValue)
			{
				return 0;
			}

			return EditorGUI.GetPropertyHeight(toggleProp);
		}
	}

}

#endif