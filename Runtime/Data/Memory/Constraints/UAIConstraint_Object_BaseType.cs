// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using System.ComponentModel;
	using UnityEngine;

	/// <summary>
	/// TODO: Better inspector with helpful type info
	/// </summary>
	[DisplayName("Base Type")]
	internal sealed class UAIConstraint_Object_BaseType : UAIConstraint_Object
	{
		public override bool Validate(object value)
		{
			var baseType = GetSystemType();
			if (baseType == null)
			{
				return false;
			}

			// Note: might wanna make inheritance rules configurable
			return
			value == null // might wanna make this configurable
			|| baseType == value.GetType() // value is exact allowed type
			|| value.GetType().IsSubclassOf(baseType); // value is subtype
		}
		
		[SerializeField] private string _baseType = typeof(GameObject).AssemblyQualifiedName;

		private (string, Type) _cachedType;
		
		private Type GetSystemType()
		{
			if (_baseType != _cachedType.Item1)
			{
				_cachedType = (_baseType, Type.GetType(_baseType));
			}
			return _cachedType.Item2;
		}
	}
}