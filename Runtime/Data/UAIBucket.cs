// smidgens @ github

// resharper disable all

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;
	using System.Collections.Generic;
	using UnityEngine.Serialization;

	/// <summary>
	/// Houses list of actions
	/// </summary>
	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Bucket")]
	[ExcludeFromPreset]
	public sealed class UAIBucket : UAIScriptableObject
	{
		public string BucketName => !string.IsNullOrEmpty(_label) ? _label : name;

		[UAITextArea(minLines:1, topLabel:false)]
		[SerializeField] internal string _comment = string.Empty;

		[SerializeField,Min(0)] internal float _weight = 1;
		
		[Min(UAIConstants.MIN_SCORING_RATE)]
		[SerializeField] internal float _bucketScoringRate = 5f;
		
		[Min(UAIConstants.MIN_SCORING_RATE)]
		[SerializeField] internal float _actionScoringRate = 1f;

		[HideInInspector]
		[SerializeField] internal UAIService[] _externalServices = Array.Empty<UAIService>();

		[InstancedReference(defaultValueLabel = "Default")]
		[SerializeReference]
		internal UAISelector _actionSelector = new UAISelector_TopScore();

		[SerializeField, HideInInspector] internal SOArray<UAIAction> _actions = new();
		[SerializeField, HideInInspector] internal SOArray<UAIConsideration> _bucketConsiderations = new();
		[SerializeField, HideInInspector] internal SOArray<UAIService> _services = new();

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

	[CustomEditor(typeof(UAIBucket))]
	internal sealed class _UAIBucket : _UAIScriptableObject
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			InitGUI();
			DrawDivider();
			DrawTabs();
		}

		protected override bool ShouldShowTypeInfo() => false;
		protected override bool ShouldGroupFields() => false;

		protected override bool ShowLabelProperty()
		{
			return true;
		}

		private NestedAssetList<UAIAction> _actionAssetList = null;
		private NestedAssetList<UAIConsideration> _bucketConsiderations = null;
		private NestedAssetList<UAIService> _services = null;
		private IReadOnlyList<SerializedProperty> _props = null;

		private struct DisplayTab
		{
			public GUIContent label;
			public Action fn;
			public Vector2 size;
			public int count;
			public EUAIAtlasIcon icon;
		}

		private enum DisplayTabs
		{
			Considerations,
			Actions,
			Services
		}

		private DisplayTab[] _displayTabs = Array.Empty<DisplayTab>();
		private GUIContent[] _tabLabels = Array.Empty<GUIContent>();
		private int[] _tabCounts = Array.Empty<int>();
		private PrefsHandle_Int _prefsTab = new PrefsHandle_Int($"{nameof(UAIBucket)}.tab");
		private GUIStyle _tabBtnMid;
		private GUIStyle _tabBtnLeft;
		private GUIStyle _tabBtnRight;

		protected override void OnInit()
		{
			_displayTabs = new DisplayTab[]
			{
				new DisplayTab
				{
					label = new GUIContent("<b>Actions</b>", "Actions"),
					fn = DrawTab_Actions,
					icon = EUAIAtlasIcon.Action
				},
				new DisplayTab
				{
					label = new GUIContent("<b>Considerations</b>", "Considerations"),
					fn = DrawTab_Considerations,
					icon = EUAIAtlasIcon.Consideration
				},
				new DisplayTab
				{
					label = new GUIContent("<b>Services</b>", "Services"),
					fn = DrawTab_Services,
					icon = EUAIAtlasIcon.Service
				},
				
			};

			_tabLabels = _displayTabs.Select(x => x.label).ToArray();
			
			var listFields = DrawAssetListAttribute.FindFieldsForType(target.GetType());

			var pl = new List<SP>();
			foreach (var f in target.GetType().FindInspectorFields<Component>())
			{
				var p = serializedObject.FindProperty(f.Name);
				if (p != null)
				{
					pl.Add(p);
				}
			}
			_props = pl;
			_actionAssetList = CreateActionList(serializedObject.FindProperty(nameof(UAIBucket._actions)));
			_bucketConsiderations = CreateConsiderationList(serializedObject.FindProperty(nameof(UAIBucket._bucketConsiderations)));
			_services = CreateServiceList(serializedObject.FindProperty(nameof(UAIBucket._services)));
		}

		private void OnDisable()
		{
			// cleanup
			if (_actionAssetList != null)
			{
				_actionAssetList.DisposeGUI();
			}

			if (_bucketConsiderations != null)
			{
				_bucketConsiderations.DisposeGUI();
			}
		}

		private static GUIStyle InitBtnStyle(GUIStyle b)
		{
			return new GUIStyle(b)
			{
				alignment = TextAnchor.UpperCenter,
				padding = new RectOffset(4,4,4,4),
				fontSize = (int)(b.fontSize * 0.85),
				stretchHeight = true,
				fixedHeight = 0,
				richText = true
			};
		}

		private void InitGUI()
		{
			if (_tabBtnMid != null)
			{
				return;
			}

			_tabBtnLeft = InitBtnStyle(EditorStyles.miniButtonLeft);
			_tabBtnMid = InitBtnStyle(EditorStyles.miniButtonMid);
			_tabBtnRight = InitBtnStyle(EditorStyles.miniButtonRight);

			for (int i = 0; i < _displayTabs.Length; i++)
			{
				var tempLabel = new GUIContent(_displayTabs[i].label.text);
				var size = _tabBtnMid.CalcSize(tempLabel);
				_displayTabs[i].size = size;
			}
		}
		
		private void DrawTabs()
		{
			_displayTabs[0].count = _actionAssetList.Count;
			_displayTabs[1].count = _bucketConsiderations.Count;
			_displayTabs[2].count = _services.Count;
			
			for (int i = 0; i < _displayTabs.Length; i++)
			{
				ref DisplayTab tab = ref _displayTabs[i];
			}
			
			var tbHeight = 21f;

			var sepColor = EditorGUIUtility.isProSkin
			? (Color.white * 0.15f).Fade(1f)
			: (Color.white * 0.6f).Fade(1f);
			
			var rect = EditorGUILayout.GetControlRect(GUILayout.Height(1f));
			rect.position += Vector2.up * 3f;
			rect.height = tbHeight;

			var sepRect = rect.SliceBottom(1.5f);

			rect.SliceRight(30f);

			_displayTabs[_prefsTab.Value].fn.Invoke();
			
			GUI.BeginClip(rect);
			var clipRect = rect;
			clipRect.position = default;

			int ii = -1;
			foreach (var tab in _displayTabs)
			{
				ii++;
				var style = _tabBtnMid;
				if (ii == 0)
				{
					style = _tabBtnLeft;
				}
				else if (ii == _displayTabs.Length - 1)
				{
					style = _tabBtnRight;
				}

				var countColor = ColorUtility.ToHtmlStringRGBA(style.normal.textColor * 0.9f);

				var label = new GUIContent($"{tab.label.text} <color=#{countColor}>({tab.count})</color>");
				label.tooltip = tab.label.tooltip;

				var size = style.CalcSize(label);

				var id = EditorGUIUtility.GetControlID(FocusType.Keyboard);
				var btnRect = clipRect.SliceLeft(size.x);
				btnRect.height *= 1.5f;

				var hovered = btnRect.Contains(Event.current.mousePosition);
				var active = _prefsTab.Value == ii;

				if (UAIEditorGUI.DoControl(btnRect, id, active, hovered, label, style))
				{
					_prefsTab.Value = ii;
				}
			}
			GUI.EndClip();
			
			EditorGUI.DrawRect(sepRect, sepColor);
		}

		private void DrawTab_Considerations()
		{
			_bucketConsiderations.OnListGUI();
			serializedObject.ApplyModifiedProperties();
		}

		private void DrawTab_Actions()
		{
			_actionAssetList.OnListGUI();
			serializedObject.ApplyModifiedProperties();
		}

		private void DrawTab_Services()
		{
			_services.OnListGUI();
			serializedObject.ApplyModifiedProperties();
		}

		private static NestedAssetList<UAIAction> CreateActionList(SerializedProperty prop)
		{
			var view = new NestedAssetList<UAIAction>(prop);

			view.DefaultTypeIconGUID = UAIConstants.DEFAULT_ACTION_ICON_GUID;
			view.DrawTypeIcon = true;

			view.onDrawNone = r => GUI.Label(r, "No actions, bucket will be ignored");

			view.onDrawListItem = (ref Rect rect, SerializedProperty prop, UAIAction so) =>
			{
				var wrect = rect.SliceRight(EditorGUIUtility.singleLineHeight * 2);
				var newWeight = Mathf.Max(EditorGUI.FloatField(wrect, so._weight), 0f);
				if (newWeight != so._weight)
				{
					EditorApplication.delayCall += () =>
					{
						Undo.RecordObject(so, "Change weight");
						so._weight = newWeight;
					};
				}
			};
			return view;
		}

		private static NestedAssetList<UAIService> CreateServiceList(SerializedProperty prop)
		{
			var view = new NestedAssetList<UAIService>(prop);
			view.DefaultTypeIconGUID = UAIConstants.DEFAULT_SERVICE_ICON_GUID;
			view.DrawTypeIcon = true;
			view.onDrawNone = r => GUI.Label(r, "No services");
			view.onDrawListItem = (ref Rect rect, SerializedProperty prop, UAIService so) =>
			{
			
			};
			return view;
		}

		private static NestedAssetList<UAIConsideration> CreateConsiderationList(SerializedProperty prop)
		{
			NestedAssetList<UAIConsideration> view = new (prop);
			view.DefaultTypeIconGUID = UAIConstants.DEFAULT_CONSIDERATION_ICON_GUID;
			view.DrawTypeIcon = true;
			view.onDrawNone = r => GUI.Label(r, "List empty, bucket base score will default to 1");
			// view.HeaderHeight = 0f;
			view.onDrawListItem = (ref Rect rect, SerializedProperty itemProp, UAIConsideration so) =>
			{
				if (!so)
				{
					return;
				}
				var curveRect = rect.SliceRight(rect.height * 1.5f);
				
				var tMatrix = GUI.matrix;

				EditorGUI.BeginChangeCheck();

				if (so._invert)
				{
					GUIUtility.ScaleAroundPivot(new Vector2(-1, 1), curveRect.center);
				}
				
				var curveColor = so._invert
				? Color.blue
				: Color.green;
				var clampRange = new Rect(0, 0, 1f, 1f);
				var changedCurve = EditorGUI.CurveField(curveRect, new AnimationCurve(so._curve.keys), curveColor, clampRange);
				
				GUI.matrix = tMatrix;
				
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