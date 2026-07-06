// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;

	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class FloatIntervalAttribute : PropertyAttribute
	{
		public float Min { get; }
		public float Max { get; } 
		
		public FloatIntervalAttribute(float min, float max)
		{
			Min = min;
			Max = Mathf.Max(min, max);
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
	
	[CustomPropertyDrawer(typeof(FloatIntervalAttribute))]
	internal class _FloatIntervalAttribute : PropertyDrawer
	{
		public override void OnGUI(Rect pos, SP prop, GUIContent l)
		{
			// label not blank and item not inside array
			if(l != GUIContent.none && !fieldInfo.FieldType.IsArray)
			{
				pos = EditorGUI.PrefixLabel(pos, l);
			}

			var attr = attribute as FloatIntervalAttribute;

			var minVal = attr?.Min ?? 0f;
			var maxVal = attr?.Max ?? 0f;

			var minProp = prop.FindPropertyRelative("min");
			var maxProp = prop.FindPropertyRelative("max");

			using (new EditorGUI.PropertyScope(pos, l, prop))
			{
				var min = minProp.floatValue;
				var max = maxProp.floatValue;

				var leftLabel = pos.SliceLeft(35f);
				pos.SliceLeft(4f);
				var rightLabel = pos.SliceRight(35f);
				pos.SliceRight(4f);
				
				EditorGUI.MinMaxSlider(pos, ref min, ref max, minVal, maxVal);
				
				EditorGUI.LabelField(leftLabel, min.ToString("0.00"));
				EditorGUI.LabelField(rightLabel, max.ToString("0.00"));
				
				minProp.floatValue = min;
				maxProp.floatValue = max;


			}
		}

	}

}

#endif