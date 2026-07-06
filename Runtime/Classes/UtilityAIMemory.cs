// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	/**
	 * Blackboard of sorts
	 */
	public sealed class UtilityAIMemory
	{
		public bool TryGetObject(UAIMK_Object key, out object value)
		{
			value = null;
			return false;
		}

		public bool TryGetObjectAsType<T>(UAIMK_Object key, out T value) where T : class
		{
			value = null;
			if (!TryGetObject(key, out object obValue))
			{
				return false;
			}

			value = obValue as T;
			return value != null;
		}
		
		
		// constructor private for now
		internal UtilityAIMemory(){}
	}
}