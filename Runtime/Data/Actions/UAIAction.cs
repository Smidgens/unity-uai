// smidgens @ github

#pragma warning disable 0414
#pragma warning disable 0067

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using System.Collections.Generic;
	using IEnumerator = System.Collections.IEnumerator;

	// action base
	public abstract class UAIAction :  UAINode, IUAIAction
	{
		/// <summary>
		/// Activation status in UAI brain
		/// </summary>
		public EUAIActionStatus GetActionStatus()
		{
			return _status;
		}
		
		/// <summary>
		/// Returns wait time before action can be considered again after deactivation
		/// </summary>
		public virtual float GetActionCooldown()
		{
			return 1;
		}

		/// <summary>
		/// Run the entirety of an action's logic
		/// </summary>
		public virtual IEnumerator ActivateAction()
		{
			return null;
		}
		
		/// <summary>
		/// Deactivation routine, will always run to completion
		/// </summary>
		public virtual IEnumerator DeactivateAction()
		{
			return null;
		}
		
		/// <summary>
		/// When true, action can be cancelled if another is selected
		/// </summary>
		public virtual bool CanCancelAction()
		{
			return true;
		}

		/// <summary>
		/// Opportunity for action to reset its state manually for when action is re-used
		/// </summary>
		public virtual void ResetAction()
		{
			
		}

		// called by UAI brain when action is reused
		void IUAIAction.ResetActionInternal()
		{
			ResetAction();
			// TODO: other base class cleanup
		}

		// 
		public float GetTotalScore(in UAIAgentContext context)
		{
			if (Mathf.Approximately(_weight, 0f))
			{
				return 0f;
			}

			if (_considerations.Count == 0)
			{
				return _weight * UAIDefaults.DEFAULT_ACTION_SCORE;
			}
	
			var score = UAIMath.ScoreConsiderations(context, _considerations.GetItems(), out int Count);
			return _weight * score;
		}

		protected void FinishAction()
		{
			FinishWithStatus(EUAIActionStatus.Finished);
		}

		internal void CancelAction()
		{
			FinishWithStatus(EUAIActionStatus.Cancelled);
		}

		internal UAIAction InstantiateAction()
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
		
		internal Action onActionFinished;

		[Min(0f)]
		[HideInInspector]
		[SerializeField] internal float _weight = 1f;
		
		/// <summary>
		/// When enabled, the action will not be re-scored while it's active (it sustains the same score)
		/// </summary>
		[SerializeField] private bool _sustainAction;

		[EditConditionHidden(nameof(_sustainAction))]
		[SerializeField] internal AnimationCurve _sustainCurve = AnimationCurve.Linear(0, 1, 1, 1);

		[HideInInspector]
		[SerializeField] internal SOArray<UAIConsideration> _considerations = new();

		internal EUAIActionStatus _status = EUAIActionStatus.Inactive;

		private void FinishWithStatus(EUAIActionStatus status)
		{
			_status = status;
			Action ev = onActionFinished;
			onActionFinished = null;
			ev?.Invoke();
		}
		
		// editor/inspector convenience
		internal override void GatherNestedAssets(List<UAIScriptableObject> assets)
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
	using System.Collections.Generic;
	using SP = UnityEditor.SerializedProperty;

	[CustomEditor(typeof(UAIAction), true)]
	internal class _UAIAction : _UAIScriptableObject
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

			DrawInfoMessages();
		}

		private NestedAssetList<UAIConsideration> _considerationView = null;
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
			_considerationView = CreateConsiderationList(listProp);
		}
		
		private void OnDisable()
		{
			_considerationView?.DisposeGUI();
		}

		private void DrawInfoMessages()
		{
			if (_considerationView != null && _considerationView.Count == 0)
			{
				EditorGUILayout.HelpBox($"Action has no considerations, score will default to {UAIDefaults.DEFAULT_ACTION_SCORE}", MessageType.Info);
			}
		}


		private static NestedAssetList<UAIConsideration> CreateConsiderationList(SerializedProperty listProp)
		{
			NestedAssetList<UAIConsideration> view = new (listProp);
			view.DefaultTypeIconGUID = UAIEditorConstants.DEFAULT_CONSIDERATION_ICON_GUID;
			view.DrawTypeIcon = true;
			view.onDrawListItem = (ref Rect rect, SerializedProperty prop, UAIConsideration so) =>
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