// smidgens @ github

// ReSharper disable VirtualMemberNeverOverridden.Global
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
		/// Optional multiplier for weight, can be used as extra guard for prerequisites before trying to activate
		/// </summary>
		public virtual float GetWeightModifier()
		{
			return 1f;
		}

		/// <summary>
		/// Returns wait time before action can be considered again after deactivation
		/// </summary>
		public virtual float GetActionCooldown()
		{
			return _defaultCooldown;
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

		public virtual bool IsReusable()
		{
			return true;
		}

		public string GetStatusText()
		{
			return _statusText;
		}

		protected void FinishAction()
		{
			FinishWithStatus(EUAIActionStatus.Finished);
		}

		internal float GetActionWeight()
		{
			return _weight * Mathf.Max(GetWeightModifier(), 0f);
		}

		internal void CancelAction()
		{
			FinishWithStatus(EUAIActionStatus.Cancelled);
		}

		internal UAIAction InstantiateAction()
		{
			var instance = Instantiate(this);
			instance.name = name;
			return instance;
		}

		internal Action onActionFinished;

		[SerializeField] internal string _statusText = string.Empty;

		[Min(0f)]
		[HideInInspector]
		[SerializeField] internal float _weight = 1f;
		
		/// <summary>
		/// When enabled, the action will not be re-scored while it's active (it sustains the same score)
		/// </summary>
		[HideInInspector]
		[SerializeField] internal bool _sustainAction;

		[EditConditionToggle(nameof(_sustainAction), "Sustain Action")]
		[SerializeField] internal AnimationCurve _sustainCurve = AnimationCurve.Linear(0, 1, 1, 1);

		// [HideInInspector]
		[UAIHideOnOverride(nameof(GetActionCooldown))]
		[SerializeField,Min(0f)] private float _defaultCooldown = UAIDefaults.DEFAULT_ACTION_COOLDOWN;

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

		#if UNITY_EDITOR
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
		#endif
	}
}


#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(UAIAction), true)]
	internal class _UAIAction : _UAIScriptableObject
	{
		protected override void OnAfterFields()
		{
			// EditorGUILayout.Space();
			DrawDivider();
			serializedObject.UpdateIfRequiredOrScript();
			_considerationView.OnListGUI();
			serializedObject.ApplyModifiedProperties();
		}

		private NestedAssetList<UAIConsideration> _considerationView;
		private SerializedProperty _defaultCooldown;
		private GUIContent _foldoutLabel;

		protected override void OnInit()
		{
			var listProp = serializedObject.FindProperty(nameof(UAIAction._considerations));
			_considerationView = CreateConsiderationList(listProp);
		}

		private void OnDisable()
		{
			_considerationView?.DisposeGUI();
		}

		private static NestedAssetList<UAIConsideration> CreateConsiderationList(SerializedProperty listProp)
		{
			NestedAssetList<UAIConsideration> view = new (listProp);
			view.DefaultTypeIconGUID = UAIConstants.DEFAULT_CONSIDERATION_ICON_GUID;
			view.DrawTypeIcon = true;
			view.HeaderLabel = "Action Considerations";
			view.onDrawNone = rect =>
			{
				GUI.Label(rect, "List empty, base score will default to 1.");
			};

			view.onDrawHeader = rect =>
			{
				var icoRect = rect.SliceLeft(rect.height).Resized(-rect.height * 0.2f);
				UAIEditorAtlas.GetIcon(EUAIAtlasIcon.Consideration).Draw(icoRect);
				GUI.Label(rect, "Action Considerations", EditorStyles.boldLabel);

			};
			
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