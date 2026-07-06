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
	using System.Reflection;
	

	// [CustomEditor(typeof(UtilityAISO))]
	internal class _UtilityAISO : Editor
	{
		// public override void OnInspectorGUI()
		// {
		// 	
		// }

		protected override bool ShouldHideOpenButton() => true;

		private List<FieldInfo> _listFields = new();

		// private void OnEnable()
		// {
		// 	_listFields = DrawAssetListAttribute.FindFieldsForType(target.GetType());
		// }

	}
}

#endif