// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Consideration List")]
	public sealed class UAIConsiderationList : ScriptableObject
	{
		[HideInInspector]
		[SerializeField] internal SOArray<UAIConsideration> _considerations = new();

		internal int GetEnabledConsiderationCount()
		{
			int count = 0;
			foreach(var c in _considerations.GetArr())
			{
				if (c.item && c.item._enabled)
				{
					count++;
				}
			}
			return count;
		}
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;

	[CustomEditor(typeof(UAIConsiderationList))]
	internal sealed class _UtilityConsiderationSetSO : _UAIScriptableObject
	{
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();
			_considerationAssetList.OnListGUI();
			serializedObject.ApplyModifiedProperties();
		}

		private NestedAssetList _considerationAssetList;

		protected override void OnInit()
		{
			var listProp = serializedObject.FindProperty(nameof(UAIConsiderationList._considerations));
			_considerationAssetList = new NestedAssetList(listProp, typeof(UAIConsideration));
		}

		private void OnDisable()
		{
			_considerationAssetList?.DisposeGUI();
		}


	}
}

#endif