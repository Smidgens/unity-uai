// smidgens @ github

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.ComponentModel;
	using UnityEditor.Search;

	/**
	 *
	 */
	internal static class UAIEditorUtils
	{
		public static IEnumerable<Type> GetDerivedTypes(Type baseType)
		{
			List<Type> outTypes = new();

			foreach (var t in TypeCache.GetTypesDerivedFrom(baseType))
			{
				if (!t.IsAbstract)
				{
					outTypes.Add(t);
				}
			}
			return outTypes;
		}

		public static bool IsDefaultScriptIcon(Texture tex)
		{
			// empty guid: 0000000000000000d000000000000000
			// default script icon: d_cs Script Icon
			// default so icon: "d_ScriptableObject Icon"
			return tex?.name == "d_cs Script Icon";
		}

		public static void OpenScriptEditor(UnityEngine.Object asset)
		{
			AssetDatabase.OpenAsset(GetObjectMonoscript(asset));
		}

		public static MonoScript GetObjectMonoscript(UnityEngine.Object asset)
		{
			var stype = asset.GetType();

			// this seems slightly more tedious than it needs to be, whatever...
			var isSO = typeof(ScriptableObject).IsAssignableFrom(stype);
			var script = isSO
			? MonoScript.FromScriptableObject((ScriptableObject)asset)
			: MonoScript.FromMonoBehaviour((MonoBehaviour)asset);

			return script;
		}
		
		public static bool CanEditScriptForType(Type type)
		{
			// TODO: Only return false if script was installed via package manager
			if (type.Assembly == typeof(UAIAction).Assembly)
			{
#if SM_DEV
				return true;
#else
				return false;
#endif
			}
			return true;
		}
		
		public static GenericDropdown<Type> CreateTypeDropdown(Type baseType,  Action<Type> fn, string defaultLabel = "(none)")
		{
			var dropdown = GenericDropdown<Type>.Create(ObjectNames.NicifyVariableName(baseType.Name));
			dropdown.onSelected = fn;
			
			var types = GetDerivedTypes(baseType);

			if (!string.IsNullOrEmpty(defaultLabel))
			{
				dropdown.AddItem(defaultLabel, null);
				dropdown.AddSeparator(string.Empty);
			}

			Assembly currentAssembly = null;

			foreach (var type in types)
			{
				if (type.IsDefined(typeof(ObsoleteAttribute)))
				{
					continue;
				}

				if (currentAssembly != type.Assembly)
				{
					if (currentAssembly != null)
					{
						// dropdown.AddSeparator("");
					}
					currentAssembly = type.Assembly;
				}

				var dname = GetTypeLabel(type);
				var icon = SearchUtils.GetTypeIcon(type);
				dropdown.AddItem(dname.text, type, icon:icon);
			}
			return dropdown;
		}

		private static GUIContent GetTypeLabel(Type type)
		{
			string category = null;
			string dname = null;

			var md = type.GetCustomAttribute<DisplayNameAttribute>();

			if (md != null)
			{
				// category = md.category;
				dname = md.DisplayName;
			}
			if (dname == null)
			{
				dname = type.Name;
			}
			var path = category != null ? category + "/" + dname : dname;
			return new GUIContent(path);
		}

	}
}

#endif