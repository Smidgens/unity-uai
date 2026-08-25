// smidgens @ github

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;
	using System;

	// wrapper for read/write ops on editor prefs values
	internal abstract class PrefsHandle<T>
	{
		public T Value
		{
			get => _getter.Invoke(this);
			set => WriteValue(value);
		}
		
		protected abstract Func<string, T, T> PrefsGetter { get;  }
		protected abstract Action<string, T> PrefsSetter { get;  }
		
		protected PrefsHandle(string key)
		{
			_key = key;
		}
		private static T ReadPrefs(PrefsHandle<T> toggle)
		{
			toggle._value = toggle.PrefsGetter.Invoke(toggle._key, default);
			toggle._getter = ReadCached;
			return toggle._value;
		}
		private static T ReadCached(PrefsHandle<T> h) => h._value;

		private void WriteValue(T value)
		{
			_value = value;
			PrefsSetter.Invoke(_key, _value);
		}

		private T _value;
		private readonly string _key;
		private Func<PrefsHandle<T>, T> _getter = ReadPrefs;
	}

	internal sealed class PrefsHandle_Int : PrefsHandle<int>
	{
		public PrefsHandle_Int(string key) : base(key) {}
		
		protected override Func<string, int, int> PrefsGetter => SessionState.GetInt;
		protected override Action<string, int> PrefsSetter => SessionState.SetInt;
	}
	
	internal sealed class PrefsHandle_Bool : PrefsHandle<bool>
	{
		public PrefsHandle_Bool(string key) : base(key) {}
		protected override Func<string, bool, bool> PrefsGetter => SessionState.GetBool;
		protected override Action<string, bool> PrefsSetter => SessionState.SetBool;
	}
	
	internal sealed class PrefsHandle_Float : PrefsHandle<float>
	{
		public PrefsHandle_Float(string key) : base(key) {}
		protected override Func<string, float, float> PrefsGetter => SessionState.GetFloat;
		protected override Action<string, float> PrefsSetter => SessionState.SetFloat;
	}

	internal sealed class PrefsHandle_String : PrefsHandle<string>
	{
		public PrefsHandle_String(string key) : base(key) {}
		protected override Func<string, string, string> PrefsGetter => SessionState.GetString;
		protected override Action<string, string> PrefsSetter => SessionState.SetString;
	}
}

#endif