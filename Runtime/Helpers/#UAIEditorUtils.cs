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

		public static GenericMenu CreateTypeMenu(Type baseType,  GenericMenu.MenuFunction2 fn, string defaultLabel = "(none)")
		{
			var menu = new GenericMenu();

			var types = GetDerivedTypes(baseType);

			if (!string.IsNullOrEmpty(defaultLabel))
			{
				menu.AddItem(new GUIContent(defaultLabel), false, fn, null);
				menu.AddSeparator(string.Empty);
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
						menu.AddSeparator("");
					}
					currentAssembly = type.Assembly;
					menu.AddDisabledItem(new GUIContent(currentAssembly.GetName().Name));
				}
				
				var dname = GetTypeLabel(type);
				menu.AddItem(dname, false, fn,  type);
			}
			return menu;
		}

		public static void ReimportAsset(UnityEngine.Object asset)
		{
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
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