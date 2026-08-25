// smidgens @ github

// resharper disable all

namespace Smidgenomics.Unity.UAI
{
	using System;
	using System.Diagnostics;
	using UnityEngine;

	[AttributeUsage(AttributeTargets.Field)]
	[Conditional("UNITY_EDITOR")]
	internal sealed class InstancedReferenceAttribute : PropertyAttribute
	{
		public string defaultValueLabel { get; set; } = "(none)";
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Editor;
	using System.ComponentModel;
	using UObject = UnityEngine.Object;

	[CustomPropertyDrawer(typeof(InstancedReferenceAttribute))]
	internal class _InstancedReferenceAttribute : PropertyDrawer
	{
		public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent l)
		{
			if (prop.propertyType != SerializedPropertyType.ManagedReference)
			{
				pos = EditorGUI.PrefixLabel(pos, l);
				EditorGUI.LabelField(pos, "Invalid type", EditorStyles.miniLabel);
				return;
			}

			var typeRect = pos.SliceTop(EditorGUIUtility.singleLineHeight);
			pos.SliceTop(2);

			if(l != GUIContent.none && !fieldInfo.FieldType.IsArray)
			{
				typeRect = EditorGUI.PrefixLabel(typeRect, l);
			}

			using (new EditorGUI.PropertyScope(pos, l, prop))
			{
				SelectorDropdown(typeRect, prop);
				if (prop.managedReferenceValue == null)
				{
					return;
				}

				if (_cachedFields.Item1 != prop.managedReferenceValue.GetType())
				{
					_cachedFields = (prop.managedReferenceValue.GetType(),
					prop.managedReferenceValue.GetType().FindInspectorFields());
				}

				var extraIndent = 1;
				EditorGUI.indentLevel += extraIndent;
				foreach (var field in _cachedFields.Item2)
				{
					var fRect = pos.SliceTop(EditorGUIUtility.singleLineHeight);
					var fProp = prop.serializedObject.FindProperty(prop.propertyPath + "." + field.Name);
					EditorGUI.PropertyField(fRect, fProp);
					pos.SliceTop(2);
				}
				EditorGUI.indentLevel -= extraIndent;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			int rowCount = 1;

			if (property.managedReferenceValue != null)
			{
				rowCount += property.managedReferenceValue.GetType()
				.FindInspectorFields().Count;
			}
			
			var padding = (rowCount - 1) * 2;
			return (rowCount) * EditorGUIUtility.singleLineHeight + padding;
		}

		
		private GUIContent _btnLabel = new();
		private (Type, IReadOnlyList<FieldInfo>) _cachedFields;

		private void SelectorDropdown(Rect pos, SerializedProperty prop)
		{
			Type currentType = prop.managedReferenceValue?.GetType();

			var defLabel = (attribute as InstancedReferenceAttribute).defaultValueLabel;

			var btnLabel = currentType != null
			? currentType.Name
			: defLabel;

			var dn = currentType?.GetCustomAttribute<DisplayNameAttribute>();
			if (dn != null)
			{
				btnLabel = dn.DisplayName;
			}
			
			_btnLabel.text = btnLabel;

			if (!EditorGUI.DropdownButton(pos, _btnLabel, FocusType.Keyboard))
			{
				return;
			}

			var m = UAIEditorUtils.CreateTypeMenu(GetFieldType(), o =>
			{
				var newType = (Type)o;
				if (newType == currentType)
				{
					return;
				}

				if (o == null)
				{
					prop.managedReferenceValue = null;
					prop.serializedObject.ApplyModifiedProperties();
					return;
				}
				
				prop.managedReferenceValue = Activator.CreateInstance(newType);
				prop.serializedObject.ApplyModifiedProperties();
			}, defLabel);
			
			m.DropDown(pos);
			
		}

		private Type GetFieldType()
		{
			return !fieldInfo.FieldType.IsArray
			? fieldInfo.FieldType
			: fieldInfo.FieldType.GetElementType();
		}
	}
}

#endif