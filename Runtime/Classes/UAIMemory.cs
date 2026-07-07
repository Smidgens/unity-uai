// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using System.Collections.Generic;

	/**
	 * Blackboard of sorts
	 * design TBD
	 */
	public sealed class UAIMemory
	{
		public bool HasValue(UAIMemoryKey key)
		{
			return _values.ContainsKey(key);
		}

		public bool TrySetFloat(UAIMemoryKey_Float key, float value)
		{
			return TrySetValue(key, new UAIMemoryValue
			{
				floatValue = value
			});
		}

		public bool TrySetObject(UAIMemoryKey_Object key, object value)
		{
			return TrySetValue(key, new UAIMemoryValue
			{
				objectValue = value
			});
		}

		public bool TrySetInt(UAIMemoryKey_Int key, int value)
		{
			return TrySetValue(key, new UAIMemoryValue
			{
				intValue = value
			});
		}

		public bool TrySetBool(UAIMemoryKey_Bool key, bool value)
		{
			return TrySetValue(key, new UAIMemoryValue
			{
				boolValue = value
			});
		}

		/// <summary>
		/// Clears given key of value, returns false if no value exists
		/// </summary>
		public bool ClearValue(UAIMemoryKey key)
		{
			if (!HasValue(key))
			{
				return false;
			}
			_values.Remove(key);
			return false;
		}

		public bool TryGetObject(UAIMemoryKey_Object key, out object value) => TryGetValue(key, out value, GetObject);
		public bool TryGetFloat(UAIMemoryKey_Float key, out float value) => TryGetValue(key, out value, GetFloat);
		public bool TryGetInt(UAIMemoryKey_Int key, out int value) => TryGetValue(key, out value, GetInt);
		public bool TryGetBool(UAIMemoryKey_Bool key, out bool value) => TryGetValue(key, out value, GetBool);

		private delegate T ValueGetter<T>(in UAIMemoryValue v);
		
		private bool TrySetValue(UAIMemoryKey key, UAIMemoryValue value)
		{
			if (!key.Validate(ref value))
			{
				return false;
			}
			_values[key] = value;
			return true;
		}
		
		private bool TryGetValue<T>(UAIMemoryKey key, out T value, ValueGetter<T> getter)
		{
			value = default;
			if (!HasValue(key))
			{
				return false;
			}
			value = getter.Invoke(_values[key]);
			return true;
		}

		private bool GetBool(in UAIMemoryValue v) => v.boolValue;
		private int GetInt(in UAIMemoryValue v) => v.intValue;
		private object GetObject(in UAIMemoryValue v) => v.objectValue;
		private float GetFloat(in UAIMemoryValue v) => v.floatValue;

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

		public void ClearAllValues()
		{
			_values.Clear();
		}

		// constructor private for now
		internal UAIMemory(){}

		private Dictionary<UAIMemoryKey, UAIMemoryValue> _values = new();
	}
}