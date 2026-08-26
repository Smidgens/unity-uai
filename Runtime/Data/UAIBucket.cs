// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;

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

		[UAIInstancedReference(defaultValueLabel = "Default")]
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

		private struct DisplayTab
		{
			public string field;
			public Type type;
			public string emptyText;
			public string nameOverride;
			public NestedAssetList list;
			public GUIContent headerLabel;
		}

		private DisplayTab[] _displayTabs =
		{
			new()
			{
				field = nameof(UAIBucket._actions),
				type = typeof(UAIAction),
				emptyText = "No actions, bucket will be ignored"
			},
			new()
			{
				field = nameof(UAIBucket._bucketConsiderations),
				type = typeof(UAIConsideration),
				nameOverride = "Considerations",
				emptyText = "List empty, bucket base score will default to 1"
			},
			new()
			{
				field = nameof(UAIBucket._services),
				type = typeof(UAIService),
				emptyText = "No services"
			},
		};
		private readonly PrefsHandle_Int _prefsTab = new ($"{nameof(UAIBucket)}.tab");
		private GUIStyle _tabBtnMid;
		private GUIStyle _tabBtnLeft;
		private GUIStyle _tabBtnRight;

		protected override void OnInit()
		{
			for (int i = 0; i < _displayTabs.Length; i++)
			{
				ref DisplayTab tab = ref _displayTabs[i];
				var prop = serializedObject.FindProperty(tab.field);
				var tName = tab.nameOverride ?? prop.displayName;
				tab.list = CreateAssetList(prop, tab);
				tab.headerLabel = new GUIContent($"<b>{tName}</b>", tName);
			}
		}

		private void OnDisable()
		{
			foreach (var t in _displayTabs)
			{
				if (t.list != null)
				{
					t.list.DisposeGUI();
				}
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
		}
		
		private void DrawTabs()
		{
			var tbHeight = 21f;

			var rect = EditorGUILayout.GetControlRect(GUILayout.Height(1f));
			rect.position += Vector2.up * 3f;
			rect.height = tbHeight;

			var sepRect = rect.SliceBottom(1.5f);

			rect.SliceRight(30f);

			var currentList = _displayTabs[_prefsTab.Value].list;

			currentList.OnListGUI();
			serializedObject.ApplyModifiedProperties();
			
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

				var label = new GUIContent($"{tab.headerLabel.text} <color=#{countColor}>({tab.list.Count})</color>")
				{
					tooltip = tab.headerLabel.tooltip
				};

				var size = style.CalcSize(label);

				var id = GUIUtility.GetControlID(FocusType.Keyboard);
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

			EditorGUI.DrawRect(sepRect, UAIEditorGUI.DIVIDER_COLOR);
		}

		private static NestedAssetList CreateAssetList(SerializedProperty prop, in DisplayTab tab)
		{
			var view = new NestedAssetList(prop, tab.type);
			view.EmptyText = tab.emptyText;
			return view;
		}

	}
}

#endif