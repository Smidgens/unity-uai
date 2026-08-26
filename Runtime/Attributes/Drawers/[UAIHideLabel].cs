// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;

	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class UAIHideLabelAttribute : PropertyAttribute
	{
	
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(UAIHideLabelAttribute))]
	internal sealed class _UAIHideLabelAttribute : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.PropertyField(position, property, GUIContent.none);
		}
	}
}

#endif