// smidgens @ github

// ReSharper disable All

#pragma warning disable 0414
#pragma warning disable 0067

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using System.Collections.Generic;
	using IEnumerator = System.Collections.IEnumerator;

	// action base
	public abstract class UtilityAIAction : 
		UtilityAIBehaviourNode, 
		IUtilityAction
	{
		
		public EUtilityAIActionStatus GetActionStatus()
		{
			return _status;
		}

		public virtual float GetActionCooldown()
		{
			return 1;
		}

		public virtual IEnumerator ActivateAction()
		{
			return null;
		}

		public virtual IEnumerator DeactivateAction()
		{
			return null;
		}

		public virtual bool CanCancelAction()
		{
			return true;
		}

		// 
		public float GetTotalScore(in UtilityAIContext context)
		{
			if (Mathf.Approximately(_weight, 0f))
			{
				return 0f;
			}
			var score = UAIMath.ScoreConsiderations(context, _considerations.GetItems(), out int Count);
			return _weight * score;
		}

		protected void FinishAction()
		{
			FinishWithStatus(EUtilityAIActionStatus.Finished);
		}

		internal void CancelAction()
		{
			FinishWithStatus(EUtilityAIActionStatus.Cancelled);
		}

		internal UtilityAIAction InstantiateAction()
		{
			var instance = ScriptableObject.Instantiate(this);
			instance.name = name;
			return instance;
		}

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
		
		internal Action onActionFinished = null;

		[Min(0f)]
		[HideInInspector]
		[SerializeField] internal float _weight = 1f;

		[SerializeField] private bool _sustainAction = false;

		[EditConditionHidden(nameof(_sustainAction))]
		[SerializeField] private AnimationCurve _sustainCurve = AnimationCurve.Linear(0, 1, 1, 1);

		[HideInInspector]
		[SerializeField] internal SOArray<UtilityAIConsideration> _considerations = new();

		internal EUtilityAIActionStatus _status = EUtilityAIActionStatus.Inactive;

		private void FinishWithStatus(EUtilityAIActionStatus status)
		{
			_status = status;
			Action ev = onActionFinished;
			onActionFinished = null;
			ev?.Invoke();
		}

		internal override void GatherNestedAssets(List<UtilityAISO> assets)
		{
			foreach (var c in _considerations.GetArr())
			{
				if (c.item)
				{
					assets.Add(c.item);
				}
			}
		}
	}
}


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

	[CustomEditor(typeof(UtilityAIAction), true)]
	internal class _UtilityAIAction : _UtilityAISO
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
			EditorGUILayout.Space();
			_considerationView.OnListGUI();
			serializedObject.ApplyModifiedProperties();
		}

		private NestedAssetList<UtilityAIConsideration> _considerationView = null;
		private List<SP> _props = new();

		private void OnEnable()
		{
			_props = new List<SP>();
			foreach (var f in target.GetType().FindInspectorFields())
			{
				var prop = serializedObject.FindProperty(f.Name);
				_props.Add(prop);
			}
			var listProp = serializedObject.FindProperty(nameof(UtilityAIAction._considerations));
			_considerationView = CreateConsiderationList(listProp);
		}
		
		private void OnDisable()
		{
			_considerationView?.DisposeGUI();
		}

		private static NestedAssetList<UtilityAIConsideration> CreateConsiderationList(SerializedProperty listProp)
		{
			NestedAssetList<UtilityAIConsideration> view = new (listProp);
			view.DefaultTypeIconGUID = "d8ec218438d247b49a3a0f61ed39664d";
			view.DrawTypeIcon = true;
			view.onDrawListItem = (ref Rect rect, SerializedProperty prop, UtilityAIConsideration so) =>
			{
				if (!so)
				{
					return;
				}
				var curveRect = rect.SliceRight(rect.height * 1.5f);
				EditorGUI.BeginChangeCheck();
				var changedCurve = EditorGUI.CurveField(curveRect, new AnimationCurve(so._curve.keys));
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(so, "Change curve");
					so._curve = changedCurve;
				}
				
				var invertRect = rect.SliceRight(60f);
				var newInvert = EditorGUI.ToggleLeft(invertRect, new GUIContent("Invert"), so._invert);
				if (newInvert != so._invert)
				{
					Undo.RecordObject(so, "Toggle inverted");
					so._invert = newInvert;
				}
			};
			return view;
		}
		
	}
}

#endif