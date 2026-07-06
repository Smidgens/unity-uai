// smidgens @ github

// resharper disable all

namespace Smidgenomics.Unity.UAI
{
	using System;
	using System.Linq;
	using System.Diagnostics;
	using System.Collections.Generic;
	using System.Reflection;

	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class DrawAssetListAttribute : Attribute
	{
		public string fieldName { get; }

		public DrawAssetListAttribute(string fieldName)
		{
			this.fieldName = fieldName;
		}
		
		public static List<FieldInfo> FindFieldsForType(Type type)
		{
			List<FieldInfo> fields = new();
			var attributes =
			type.GetCustomAttributes(typeof(DrawAssetListAttribute), true)
			.Select(a => a as DrawAssetListAttribute);
			foreach (var a in attributes)
			{
				var field = type.GetField(a.fieldName, FIELD_FLAGS);
				if (field == null || !field.IsInspectorField())
				{
					continue;
				}
				fields.Add(field);
			}
			return fields;
		}
		
		// instance fields declared by type
		private const BindingFlags FIELD_FLAGS =
		BindingFlags.NonPublic
		| BindingFlags.Public
		| BindingFlags.DeclaredOnly
		| BindingFlags.Instance;
	}
}