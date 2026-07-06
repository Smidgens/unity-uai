// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Memory/Object")]
	public sealed class UAIMK_Object : UtilityAIMemoryKey
	{
		public override bool Validate(ref UtilityAIMemoryValue value)
		{
			var baseType = GetSystemType();
			if (baseType == null || value.Object == null)
			{
				return false;
			}
			return value.Object.GetType().IsSubclassOf(baseType);
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