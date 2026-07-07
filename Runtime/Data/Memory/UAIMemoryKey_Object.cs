// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	// TODO: More robust inspector
	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Memory/Object")]
	public sealed class UAIMemoryKey_Object : UAIMemoryKey
	{
		public override bool Validate(ref UAIMemoryValue value)
		{
			var baseType = GetSystemType();
			if (baseType == null || value.objectValue == null)
			{
				return false;
			}
			return value.objectValue.GetType().IsSubclassOf(baseType);
		}

		[SerializeField] private string _baseType = typeof(GameObject).AssemblyQualifiedName;

		private (string, Type) _cachedType = default;

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