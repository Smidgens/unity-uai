// smidgens @ github

#pragma warning disable CS0414

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	/// <summary>
	/// Background logic
	/// design TBD
	///
	/// Notes:
	/// - Service instances are always reused
	/// </summary>
	public abstract class UAIService : UAINode, IUAIService
	{
		/// <summary>
		/// Runs exactly once after service instance is created
		/// </summary>
		public virtual void InitService()
		{
			// override me
		}

		/// <summary>
		/// Runs every time service becomes active
		/// </summary>
		public virtual void StartService()
		{
			// override me
		}

		/// <summary>
		/// Runs every time service becomes inactive
		/// </summary>
		public virtual void StopService()
		{
			// override me
		}

		public IUAIService Clone(UAIBrain owningBrain)
		{
			var s = ScriptableObject.Instantiate(this);
			s.SetBrain(owningBrain);
			return s;
		}

	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using SP = UnityEditor.SerializedProperty;

	[CustomEditor(typeof(UAIService), true)]
	internal class _UAIService : _UAIScriptableObject
	{
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();

			EditorGUILayout.BeginVertical(GUI.skin.box);
			foreach (var prop in _props)
			{
				if (prop == null)
				{
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
			var listProp = serializedObject.FindProperty(nameof(UAIAction._considerations));
		}
		
		private void OnDisable()
		{
		}

		
	}
}

#endif