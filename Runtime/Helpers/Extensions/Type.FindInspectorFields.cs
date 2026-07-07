// smidgens @ github

// Resharper disable all

// #if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;
	using System.Reflection;
	using System.Collections.Generic;

	public static partial class FieldInfo_
	{
		// can field be drawn by inspector
		public static bool IsInspectorField(this FieldInfo f)
		{
			// explicitly public but non-serialized
			if (f.IsPublic && f.GetCustomAttribute<NonSerializedAttribute>() != null) { return false; }

			// explicitly hidden
			if (f.GetCustomAttribute<HideInInspector>() != null) { return false; }

			// private, non serialized
			if (!f.IsPublic && f.GetCustomAttribute<SerializeField>() == null) { return false; }

			// at this point, either the field is public, or private and using SerializeField
			return true;
		}
	}

	public static partial class Type_
	{

		/// <summary>
		/// [Editor] Find all fields that Unity would default render in the inspector
		/// </summary>
		public static IEnumerable<FieldInfo> FindInspectorFields<T>(this Type owner) where T : Component
		{
			// NOTE: doesn't work properly for unity components, flags might need to be different

			var baseType = typeof(T);

			List<FieldInfo> fields = new List<FieldInfo>();
			LinkedList<Type> hierarchy = new LinkedList<Type>(); // linked for efficient prepend

			// traverse parent hierarchy, stop at MonoBehaviour
			Type currentType = owner;
			while (currentType != baseType && currentType != null)
			{
				hierarchy.AddFirst(currentType);
				currentType = currentType.BaseType;
			}

			// append fields in
			// same order as Unity would normally list them
			foreach (Type htype in hierarchy)
			{
				foreach (FieldInfo field in htype.GetFields(FIELD_FLAGS))
				{
					if (!field.IsInspectorField()) { continue; }
					fields.Add(field);
				}
			}
			return fields;
		}

		/// <summary>
		/// Find all fields that would be rendered by unity in the inspector
		/// </summary>
		public static IEnumerable<FieldInfo> FindInspectorFields(this Type owner)
		{
			return FindInspectorFields<Component>(owner);
		}

		// instance fields declared by type
		private const BindingFlags FIELD_FLAGS =
		BindingFlags.NonPublic
		| BindingFlags.Public
		| BindingFlags.DeclaredOnly
		| BindingFlags.Instance;

	}

}

// #endif