// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;

	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class UAIToggleIcon : PropertyAttribute
	{
		public UAIToggleIcon(int iconOff, int iconOn)
		{
			this.iconOff = iconOff;
			this.iconOn = iconOn;
		}

		internal int iconOff { get; }
		internal int iconOn { get; }
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using System;
	using System.Reflection;
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(UAIToggleIcon))]
	internal sealed class _UAIToggleIcon : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label);
		}

		private string _tooltip;

		public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
		{
			if (label != GUIContent.none)
			{
				pos = EditorGUI.PrefixLabel(pos, label);
			}
			var attr = (attribute as UAIToggleIcon)!;

			if (_tooltip == null)
			{
				_tooltip = fieldInfo.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? label.tooltip;
			}

			pos.width = pos.height;
		
			var icoRect = pos.Resized(-pos.height * 0.1f);

			if (GUI.Button(pos, new GUIContent(string.Empty, _tooltip), _iconBtnStyle.Value))
			{
				prop.boolValue = !prop.boolValue;
			}

			var ico = (EUAIAtlasIcon)(prop.boolValue ? attr.iconOn : attr.iconOff);
			UAIEditorAtlas.GetIcon(ico).Draw(icoRect, UAIEditorGUI.ICON_COLOR);
			
		}
		
		private static readonly Lazy<GUIStyle> _iconBtnStyle = new(() =>
		{
			return new GUIStyle(EditorStyles.iconButton)
			{
				stretchHeight = true,
				fixedHeight = 0,
				fixedWidth = 0,
				stretchWidth = true
			};
		});
	}
}

#endif