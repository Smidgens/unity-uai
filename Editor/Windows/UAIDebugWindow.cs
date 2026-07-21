// smidgens @ github

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Collections.Generic;

	internal class UAIDebugWindow : EditorWindow
	{
		public static void Open()
		{
			var w = GetWindow<UAIDebugWindow>(WIN_DOCK);
			w.Show();
		}

		public void Update()
		{
			if (Application.isPlaying)
			{
				Repaint();
			}

			if (_currentBrain != null && !_currentBrain.IsRunning())
			{
				_currentBrain = null;
			}
		}

		private const string WIN_TITLE = "UAI Debug";

		// preferred window dock points
		private static readonly Type[] WIN_DOCK =
		{
			Type.GetType("UnityEditor.ProjectBrowser, UnityEditor.CoreModule")
		};

		private static readonly float _W_PANEL_BRAINS = EditorGUIUtility.singleLineHeight * 9;
		private static readonly float _W_PANEL_MEMORY = EditorGUIUtility.singleLineHeight * 7;
		private static readonly float _H_TIMER = EditorGUIUtility.singleLineHeight * 0.5f;
		private const int _W_SEPARATOR = 1;
		private static readonly Color _SEPARATOR_COLOR = Color.black * 0.3f;

		[SerializeField] private bool _showLegend;
		[SerializeField] private bool _showMemory;
		[SerializeField] private Texture _defaultTabIcon;
		[SerializeField] private Texture _memoryIcon;
		[SerializeField] private Texture _iconGameObject;
		[SerializeField] private Texture _iconTabActions;
		[SerializeField] private Texture _iconTabTimeline;
		[SerializeField] private BrainTab _tabBrain;

		private UAIBrain _currentBrain;

		private GUIContent _tabContentMemory;
		private GUIContent _tabContentLegend;
		private GUIContent _tabIconAgent;
		private GUIContent _tabLabelActions;
		private GUIContent _tabLabelTimeline;
		private readonly Dictionary<string, Texture2D> _loadedIcons = new();
		private Vector2 _scrollBrainList;
		private Vector2 _scrollActivityView;
		private Vector2 _scrollLegend;
		private Vector2 _scrollMemory;

		private UAIAtlasIcon _iconCircle;
		private UAIAtlasIcon _iconAction;
		private UAIAtlasIcon _iconBucket;
		
		private UAIAtlasIcon _iconActive;
		private UAIAtlasIcon _iconCancelled;
		private UAIAtlasIcon _iconUncancellable;
		private UAIAtlasIcon _iconDeactivating;
		private UAIAtlasIcon _iconFinished;
		private UAIAtlasIcon _iconSelectable;
		private UAIAtlasIcon _iconMuted;
		private UAIAtlasIcon _iconRandom;
		private UAIAtlasIcon _iconTop;
		private UAIAtlasIcon _iconTopPercentage;

		private UAIDebugStyles _debugStyles;

		private float _legendWidth;

		private Texture2D _winIcon;
		private Texture2D _winIconDark;
		
		private List<LegendItem> _legItems;

		private readonly float _timerPanelWidth = EditorGUIUtility.singleLineHeight * 5;
		
		private readonly struct LegendItem
		{
			public readonly UAIAtlasIcon icon;
			public readonly GUIContent label;
			public readonly Color color;

			public LegendItem(UAIAtlasIcon i, string l)
			{
				icon = i;
				label = new GUIContent(l);
				color = Color.white;
			}

			public LegendItem(UAIAtlasIcon i, string l, Color c)
			{
				icon = i;
				label = new GUIContent(l);
				color = c;
			}
		}

		private void OnEnable()
		{
			_winIcon = Resources.Load<Texture2D>(UAIConstants.RES_PATH + "/{uai_win}");
			_winIconDark = Resources.Load<Texture2D>(UAIConstants.RES_PATH + "/{uai_win_dark}");
			
			titleContent.text = WIN_TITLE;
			titleContent.image = EditorGUIUtility.isProSkin
			? _winIconDark
			: _winIcon;

			_legendWidth = 0;
			
			var res = UAIDebugResources.Instance;

			_iconAction = res.GetAtlasIcon(0, 0);
			_iconBucket = res.GetAtlasIcon(1, 0);
			_iconCircle = res.GetAtlasIcon(0, 2);
			_iconActive = res.GetAtlasIcon(0, 2);
			_iconCancelled = res.GetAtlasIcon(1, 2);
			_iconUncancellable = res.GetAtlasIcon(2, 2);
			_iconDeactivating = res.GetAtlasIcon(3, 2);
			_iconFinished = res.GetAtlasIcon(4, 2);
			_iconSelectable = res.GetAtlasIcon(5, 2);
			_iconMuted = res.GetAtlasIcon(6, 2);

			_legItems = new()
			{
				new (_iconActive, "Active"),
				new (_iconCancelled, "Cancelled"),
				new (_iconUncancellable, "Uncancellable"),
				new (_iconDeactivating, "Deactivating"),
				new (_iconFinished, "Finished"),
				new (_iconSelectable, "Selectable"),
				new (_iconMuted, "Muted"),
				new (_iconAction, "Action"),
				new (_iconBucket, "Bucket"),
				new (res.GetAtlasIcon(0, 7), "Random"),
				new (res.GetAtlasIcon(1, 7), "Top"),
				new (res.GetAtlasIcon(2, 7), "Top %"),
			};

			_currentBrain = null;
			_iconGameObject = EditorGUIUtility.IconContent("d_GameObject Icon")?.image;
			_defaultTabIcon = EditorGUIUtility.IconContent("MainStageView")?.image;
			_iconTabActions = Resources.Load<Texture2D>(UAIConstants.RES_PATH + "/{uai_action}");
			_memoryIcon = EditorGUIUtility.IconContent("Profiler.Memory")?.image;
			_tabContentMemory = new GUIContent(_memoryIcon, "Agent Memory");
			
			_tabContentLegend = new GUIContent(_defaultTabIcon, "Legend");
			_tabIconAgent = new GUIContent(_iconGameObject, "Agent");

			_tabLabelActions = new GUIContent("Actions")
			{
				// image = _defaultTabIcon
			};

			_tabLabelTimeline = new GUIContent("Timeline")
			{
				// image = _defaultTabIcon
			};

			
		}

		private void OnDisable()
		{
			// cleanup
		}

		private enum PanelFloat { Left, Right }

		private enum BrainTab { Actions, Timeline }

		private void OnGUI()
		{
			_debugStyles ??= UAIDebugStyles.CreateInstance();

			if (Mathf.Approximately(0f, _legendWidth))
			{
				_legendWidth = GetLegendPanelWidth();
			}

			var wRect = new Rect(0f, 0f, position.width, position.height);

			DrawToolbar(wRect.SliceTop(_debugStyles.ToolbarHeight));
			DrawBrainListPanel(wRect.SliceLeft(_W_PANEL_BRAINS));

			if (_showLegend)
			{
				DrawLegendPanel(wRect.SliceRight(_legendWidth + _debugStyles.ScrollbarWidth));
			}

			if (_showMemory)
			{
				DrawMemoryPanel(wRect.SliceRight(_W_PANEL_MEMORY + _debugStyles.ScrollbarWidth));
			}

			DrawBrainView(wRect);
		}

		private void DrawToolbar(Rect r)
		{
			_showLegend = DoToolbarButton(ref r, _tabContentLegend, _showLegend, PanelFloat.Right);
			_showMemory = DoToolbarButton(ref r, _tabContentMemory, _showMemory, PanelFloat.Right);
			// _showTimers = DoToolbarButton(ref r, _tabContentMemory, _showTimers, PanelFloat.Right);
			EditorGUI.DrawRect(r.SliceBottom(_W_SEPARATOR), _SEPARATOR_COLOR);
		}

		private static Color GetIconColor()
		{
			return EditorGUIUtility.isProSkin
			? Color.white
			: Color.black;
		}
		
		private static Color GetIconColorInverse()
		{
			return !EditorGUIUtility.isProSkin
			? Color.white
			: Color.black;
		}

		private float GetLegendPanelWidth()
		{
			var longestLabel = GUIContent.none;
			foreach (var leg in _legItems)
			{
				if (leg.label.text.Length > longestLabel.text.Length)
				{
					longestLabel = leg.label;
				}
			}
			var size = _debugStyles.LegendLabelStyle.CalcSize(longestLabel);
			return size.x + size.y;
		}

		private void DrawBrainTabs(ref Rect r)
		{
			var tbRect = r.SliceTop(_debugStyles.ToolbarHeight);

			if (DoToolbarButton(ref tbRect, _tabLabelActions, _tabBrain == BrainTab.Actions))
			{
				_tabBrain = BrainTab.Actions;
			}

			if (DoToolbarButton(ref tbRect, _tabLabelTimeline, _tabBrain == BrainTab.Timeline))
			{
				_tabBrain = BrainTab.Timeline;
			}

			EditorGUI.DrawRect(tbRect.SliceBottom(_W_SEPARATOR), _SEPARATOR_COLOR);
		}

		private bool DoToolbarButton(ref Rect toolbarRect, GUIContent label, bool enabled, PanelFloat pFloat = PanelFloat.Left)
		{
			var style = _debugStyles.ToolbarButtonStyle;

			var size = style.CalcSize(label);

			var btnRect = pFloat == PanelFloat.Right
			? toolbarRect.SliceRight(size.x)
			: toolbarRect.SliceLeft(size.x);

			if (GUI.Button(btnRect, label, style))
			{
				enabled = !enabled;
			}
			if (enabled)
			{
				EditorGUI.DrawRect(btnRect.Resized(-1f), Color.white * 0.4f);
			}
			EditorGUI.DrawRect(btnRect.SliceBottom(_W_SEPARATOR), _SEPARATOR_COLOR);
			return enabled;
		}

		private Texture2D GetSelectorIconTex(UAISelector selector)
		{
			var iconGUID = selector.GetDebugIconGUID();

			if (string.IsNullOrEmpty(iconGUID))
			{
				return null;
			}

			if (_loadedIcons.TryGetValue(iconGUID, out var selectorIcon))
			{
				return selectorIcon;
			}

			var p = AssetDatabase.GUIDToAssetPath(iconGUID);

			if (string.IsNullOrEmpty(p))
			{
				_loadedIcons[iconGUID] = null;
				return null;
			}

			var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
			_loadedIcons[iconGUID] = icon;
			return icon;
		}
		
		private UAIAtlasIcon GetSelectorIcon(UAISelector selector)
		{
			var tex = GetSelectorIconTex(selector);
			return new UAIAtlasIcon(selector.GetDebugIconCoords(), tex);
		}

		private void DrawBrainFooter(Rect areaRect)
		{
			var sepRect = areaRect.SliceTop(_W_SEPARATOR);
			areaRect.SliceLeft(EditorGUIUtility.singleLineHeight * 0.25f);
			var bTimerRect = areaRect.SliceLeft(_timerPanelWidth);
			areaRect.SliceLeft(EditorGUIUtility.singleLineHeight * 0.25f);
			var aTimerRect = areaRect.SliceLeft(_timerPanelWidth);

			var aSelector = _currentBrain.GetCurrentActionSelector();
			var bSelector = _currentBrain.GetCurrentBucketSelector();

			var aIcon = _iconAction;
			var bIcon = _iconBucket;

			DrawTimer(bTimerRect, bIcon, bSelector,"Bucket", _currentBrain.GetBucketScoringProgress());
			DrawTimer(aTimerRect, aIcon, aSelector,"Action", _currentBrain.GetActionScoringProgress());
			EditorGUI.DrawRect(sepRect, _SEPARATOR_COLOR);
		}

		private void DrawTimer(Rect timerRect, in UAIAtlasIcon icon, UAISelector selector, string label, float progress)
		{
			var c = timerRect.center;
			timerRect.height *= 0.75f;
			timerRect.center = c;

			var sIconRect = timerRect.SliceRight(timerRect.height);
			var tIconRect = timerRect.SliceLeft(timerRect.height * 0.25f);

			EditorGUI.DrawRect(sIconRect, GetIconColorInverse() * 0.2f);
			EditorGUI.DrawRect(tIconRect, GetIconColor() * 0.5f);

			var sIcon = GetSelectorIcon(selector);
			sIcon.Draw(sIconRect.Resized(-sIconRect.height * 0.15f));

			EditorGUI.ProgressBar(timerRect, progress, "");
			EditorGUI.LabelField(timerRect, label, EditorStyles.centeredGreyMiniLabel);

		}
		

		private void DrawBrainView(Rect r)
		{
			if (_currentBrain == null)
			{
				return;
			}
			
			DrawBrainTabs(ref r);

			if (_tabBrain == BrainTab.Actions)
			{
				DrawBrainActions(r);
			}

			var footerRect = r.SliceBottom(_debugStyles.ToolbarHeight * 1f);
			DrawBrainFooter(footerRect);
		}

		private void DrawBrainActions(Rect areaRect)
		{
			var activityHeight = GetCurrentBrainActivityHeight();
			var scrollWidth = GUI.skin.verticalScrollbar.CalcSize(GUIContent.none).x;
			var activityWidth = areaRect.width;

			if (activityHeight > areaRect.height)
			{
				activityWidth -= scrollWidth;
			}

			var activityRect = new Rect(Vector2.zero, new Vector2(activityWidth, activityHeight));

			_scrollActivityView = GUI.BeginScrollView(areaRect, _scrollActivityView, activityRect);
			
			_currentBrain.ForEachBucket((in UAIBrain.BucketRecord br) =>
			{
				DrawBucketActivity(ref activityRect, br);
			});

			GUI.EndScrollView();
		}

		private static string FormatScoreLabel(float score)
		{
			if (Mathf.Approximately(score, 0))
			{
				return "0";
			}
			return score.ToString("0.000");
		}
		
		private void DrawBucketActivity(ref Rect rr, in UAIBrain.BucketRecord br)
		{
			var r = rr;

			var bRowHeight = _debugStyles.BucketLabelHeight;
			var aRowHeight = _debugStyles.ActionLabelHeight;
			var bucketRow = r.SliceTop(bRowHeight);

			var hColor = !Mathf.Approximately(br.score, 0f)
			? _SEPARATOR_COLOR
			: _SEPARATOR_COLOR * 0.7f;

			EditorGUI.DrawRect(bucketRow, hColor);

			var bIcon = bucketRow.SliceLeft(bucketRow.height).Resized(-bucketRow.height * 0.2f);
			
			_iconBucket.Draw(bIcon, GetIconColor());

			var bScoreRect = bucketRow.SliceRight(_debugStyles.ScoreLabelSize.x);

			EditorGUI.LabelField(bucketRow, br.bucketSO._label, _debugStyles.BucketLabelStyle);
			EditorGUI.LabelField(bScoreRect, FormatScoreLabel(br.score), _debugStyles.ScoreLabelStyle);

			if (_currentBrain.GetCurrentBucketID() == br.ID)
			{
				_currentBrain.ForEachActionInBucket(br.ID, (in UAIBrain.ActionRecord ar) =>
				{
					var actionRow = r.SliceTop(aRowHeight);

					actionRow.SliceLeft(actionRow.height * 1f);

					var iconRect = actionRow.SliceLeft(actionRow.height);
					var scoreRect = actionRow.SliceRight(_debugStyles.ScoreLabelSize.x);
					var cooldownRect = actionRow.SliceRight(_debugStyles.CooldownLabelSize.x);

					EditorGUI.LabelField(actionRow, ar.template._label, _debugStyles.ActionLabelStyle);
					EditorGUI.LabelField(scoreRect, FormatScoreLabel(ar.score), _debugStyles.ScoreLabelStyle);
					
					var stateIcon = GetActionIcon(ar, _currentBrain);
					stateIcon.Draw(iconRect.Resized(-iconRect.height * 0.15f), GetIconColor());

					if (ar.OnCooldown())
					{
						var timeLabel = GetFormattedTimestamp(ar.cooldownEnd - Time.time);
						GUI.Label(cooldownRect, timeLabel, _debugStyles.CooldownLabelStyle);
					}
				});
			}
			EditorGUI.DrawRect(r.SliceTop(_W_SEPARATOR), _SEPARATOR_COLOR);
			rr = r;
		}

		private static string GetFormattedTimestamp(float timeSeconds)
		{
			int val = timeSeconds < 1 ? (int)(timeSeconds * 1000) : (int)timeSeconds;
			return val + (timeSeconds < 1 ? "ms" : "s");
		}

		private static void DrawTimer(Rect rect, float progress)
		{
			EditorGUI.ProgressBar(rect, progress, "");
		}

		private UAIAtlasIcon GetActionIcon(in UAIBrain.ActionRecord action, UAIBrain aiBrain)
		{

			if (aiBrain.GetCurrentActionID() == action.actionID)
			{
				if (action.deactivating)
				{
					return _iconDeactivating;
				}

				return !action.cancellable
				? _iconUncancellable
				: _iconActive;
			}
			if (action.OnCooldown())
			{
				return action.cancelled
				? _iconCancelled
				: _iconFinished;
			}

			return Mathf.Approximately(action.score, 0)
			? _iconMuted
			: _iconSelectable;
		}

		private float GetCurrentBrainActivityHeight()
		{
			var bucketCount = _currentBrain.GetBucketCount();
			var actionCount = _currentBrain.GetCurrentBucketActionCount();
			// we're still exiting action in bucket
			if (_currentBrain.GetCurrentActionBucketID() != _currentBrain.GetCurrentBucketID())
			{
				actionCount++;
			}
			return
			bucketCount * _debugStyles.BucketLabelHeight
			+ actionCount * _debugStyles.ActionLabelHeight;
		}
		
		private void DrawBrainListPanel(Rect r)
		{
			EditorGUI.DrawRect(r.SliceRight(1f), _SEPARATOR_COLOR);

			var manager = UAIManager._instance;
			if (!manager)
			{
				return;
			}

			var listBtnHeight = _debugStyles.ListButtonHeight;
			var listHeight = listBtnHeight * manager.BrainCount;
			var scrollWidth = _debugStyles.ScrollbarWidth;
			var listWidth = r.width;
			
			if (listHeight > r.height)
			{
				listWidth -= scrollWidth;
			}

			var scrollRect = new Rect(Vector2.zero, new Vector2(listWidth, listHeight));

			_scrollBrainList = GUI.BeginScrollView(r, _scrollBrainList, scrollRect);

			manager.ForEachTrackedBrain((in UAIManager.TrackedBrain tb) =>
			{
				if (_currentBrain == null)
				{
					_currentBrain = tb.AIBrain;
				}
				var buttonRect = scrollRect.SliceTop(listBtnHeight);
				DrawBrainListButton(buttonRect, tb.AIBrain);
			});

			GUI.EndScrollView();
		}

		private void DrawBrainListButton(Rect r, UAIBrain brain)
		{
			var sepRect = r.SliceBottom(_W_SEPARATOR);

			if (brain == _currentBrain)
			{
				EditorGUI.DrawRect(r, Color.blue * 0.2f);
			}

			var tc = GUI.color;
			GUI.color = GetIconColor();
			EditorGUI.LabelField(r.SliceLeft(r.height).Resized(-1), _tabIconAgent);
			GUI.color = tc;
			EditorGUI.LabelField(r, brain.GetContext().agent.gameObject.name, _debugStyles.ListButtonLabelStyle);
			EditorGUI.DrawRect(sepRect, _SEPARATOR_COLOR);
			EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);

			if (GUI.Button(r, "", GUIStyle.none))
			{
				_currentBrain = brain;
			}
		}
		
		private void DrawMemoryPanel(Rect panelArea)
		{
			EditorGUI.DrawRect(panelArea.SliceLeft(_W_SEPARATOR), _SEPARATOR_COLOR);
			
			if (_currentBrain == null || !_currentBrain.IsRunning())
			{
				return;
			}

			var lineHeight = EditorGUIUtility.singleLineHeight;
			var itemHeight = lineHeight * 2;
			var memory = _currentBrain.GetMemory();
			var totalHeight = itemHeight * memory.ValueCount;
			var scrollRect = new Rect(0, 0, panelArea.width, totalHeight);

			if (totalHeight > panelArea.height)
			{
				scrollRect.width -= _debugStyles.ScrollbarWidth;
			}

			_scrollMemory = GUI.BeginScrollView(panelArea, _scrollMemory, scrollRect);
	
			memory.ForEachMemoryValue(((in UAIMemoryKey k, in UAIMemoryValue v) =>
			{
				var hRect = scrollRect.SliceTop(lineHeight);
				EditorGUI.DrawRect(hRect, Color.black * 0.2f);
				var hText = $"{k.Label} ({k.MemoryType.Name})";
				EditorGUI.LabelField(hRect, hText, EditorStyles.miniLabel);
				EditorGUI.LabelField(scrollRect.SliceTop(lineHeight), k.StringifyValue(v), EditorStyles.miniLabel);
			}));
			
			GUI.EndScrollView();
		}

		private void DrawLegendPanel(Rect panelArea)
		{
			var totalHeight = _debugStyles.LegendLabelHeight * _legItems.Count;

			var scrollRect = new Rect(Vector2.zero, new Vector2(_legendWidth, totalHeight));

			_scrollLegend = GUI.BeginScrollView(panelArea, _scrollLegend, scrollRect);
			
			foreach (var l in _legItems)
			{
				var rowRect = scrollRect.SliceTop(_debugStyles.LegendLabelHeight);
				var icoRect = rowRect.SliceLeft(rowRect.height).Resized(-rowRect.height * 0.15f);

				l.icon.Draw(icoRect, GetIconColor());
				EditorGUI.LabelField(rowRect, l.label, _debugStyles.LegendLabelStyle);
			}
			
			GUI.EndScrollView();
			
			EditorGUI.DrawRect(panelArea.SliceLeft(1f), _SEPARATOR_COLOR);
			
		}


	}
}