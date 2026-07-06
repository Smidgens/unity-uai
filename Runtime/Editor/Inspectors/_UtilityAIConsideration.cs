// smidgens @ github

// ReSharper disable All

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using UObject = UnityEngine.Object;
	using SP = UnityEditor.SerializedProperty;
	using RL = UnityEditorInternal.ReorderableList;

	[CustomEditor(typeof(UtilityAIConsideration), true)]
	internal sealed class _UtilityAIConsideration : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();
			EditorGUILayout.BeginVertical(GUI.skin.box);
			foreach (var prop in _props)
			{
				if (prop == null)
				{
					EditorGUILayout.Space();
					continue;
				}
				EditorGUILayout.PropertyField(prop);
			}
			EditorGUILayout.EndVertical();
			serializedObject.ApplyModifiedProperties();
		}

		private List<SP> _props = new();

		private void OnEnable()
		{
			_props = new List<SP>();
			foreach (var f in target.GetType().FindInspectorFields())
			{
				var prop = serializedObject.FindProperty(f.Name);
				_props.Add(prop);
			}
		}

		private void OnDisable()
		{
			
		}

	}
}

#endif