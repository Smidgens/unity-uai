// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Hide field if method override exists
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	internal sealed class UAIHideOnOverride : PropertyAttribute
	{
		public UAIHideOnOverride
		(
			string method
		)
		{
			this.method = method;
		}

		internal string method { get; }
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using System;
	using System.Reflection;
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(UAIHideOnOverride))]
	internal sealed class _UAIHideOnOverride : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			EnsureInit(property.serializedObject.targetObject.GetType());

			if (_hasOverride)
			{
				return 0f;
			}
			return EditorGUI.GetPropertyHeight(property, label);

			// return 0f;
		}


		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (_hasOverride)
			{
				return;
			}
			EditorGUI.PropertyField(position, property, label);
		}

		private bool _init;
		private bool _hasOverride;

		private void EnsureInit(Type type)
		{
			if (_init)
			{
				return;
			}
			_init = true;
			var attr = (attribute as UAIHideOnOverride)!;
			var bFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			var m = type.GetMethod(attr.method, bFlags);
			if (m == null)
			{
				return;
			}
			_hasOverride = m.DeclaringType == type;
		}
	}
}

#endif