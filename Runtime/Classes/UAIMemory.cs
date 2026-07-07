// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	/**
	 * Blackboard of sorts
	 * design TBD
	 */
	public sealed class UAIMemory
	{
		public bool HasValue(UAIMemoryKey key)
		{
			return false;
		}

		/// <summary>
		/// Clears given key of value, returns false if no value exists
		/// </summary>
		public bool ClearValue(UAIMemoryKey key)
		{
			return false;
		}

		public bool TryGetObject(UAIMemoryKey_Object key, out object value)
		{
			value = null;
			return false;
		}

		public bool TryGetObjectAsType<T>(UAIMemoryKey_Object key, out T value) where T : class
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
		internal UAIMemory(){}
	}
}