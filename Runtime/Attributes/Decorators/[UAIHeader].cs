// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;

	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class UAIHeaderAttribute : PropertyAttribute
	{
		internal string text { get; }
		public UAIHeaderAttribute(string text)
		{
			this.text = text;
		}
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(UAIHeaderAttribute))]
	internal sealed class _UAIHeaderAttribute : DecoratorDrawer
	{
		public override float GetHeight()
		{
			return EditorStyles.boldLabel.CalcHeight(GUIContent.none, 100f);
		}

		public override void OnGUI(Rect position)
		{
			GUI.Label(position, (attribute as UAIHeaderAttribute)!.text, EditorStyles.boldLabel);
		}
	}
}

#endif