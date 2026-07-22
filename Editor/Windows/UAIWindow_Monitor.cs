// smidgens @ github

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
// ReSharper disable StringLastIndexOfIsCultureSpecific.1
namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Collections.Generic;

	internal sealed class UAIWindow_Monitor : EditorWindow
	{
		public static void Open()
		{
			var w = GetWindow<UAIWindow_Monitor>(WIN_DOCK);
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
		
		private enum PanelFloat { Left, Right }

		private enum BrainTab { Actions, Timeline }

		private static readonly Type[] WIN_DOCK =
		{
			Type.GetType("UnityEditor.ProjectBrowser, UnityEditor.CoreModule")
		};

		private const string _GUID_ATLAS = "a1446d554144a4944b389210a34ff6b9";
		private const string _GUID_WIN = "628a4b08cba10804e937831e77ee8dea";
		private const string _GUID_WIN_DARK = "60518c8b07651564bb034a06b388aa6f";

		private static readonly float _W_PANEL_BRAINS = EditorGUIUtility.singleLineHeight * 7;
		private static readonly float _W_PANEL_MEMORY = EditorGUIUtility.singleLineHeight * 7;
		private static readonly float _W_PANEL_CONSIDERATIONS = EditorGUIUtility.singleLineHeight * 10;
		private const int _W_SEPARATOR = 1;
		private static readonly Color _SEPARATOR_COLOR = Color.black * 0.3f;

		[SerializeField] private bool _showLegend;
		[SerializeField] private bool _showMemory;
		[SerializeField] private bool _showConsiderations = true;
		[SerializeField] private BrainTab _tabBrain;
		private UAIBrain _currentBrain;
		private GUIContent _tabContentMemory;
		private GUIContent _tabContentLegend;
		private GUIContent _tabContentConsiderations;
		private GUIContent _tabLabelTimeline;
		private GUIContent _tabLabelActions;
		private Texture2D _iconAtlas;
		private readonly Dictionary<string, Texture2D> _loadedIcons = new();
		private Vector2 _scrollBrainList;
		private Vector2 _scrollActivityView;
		private Vector2 _scrollLegend;
		private Vector2 _scrollMemory;
		private Vector2 _scrollConsiderations;
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

		private UAIEditorStyles _editorStyles;
		private float _legendWidth;
		private List<LegendItem> _legendItems;

		private readonly float _timerPanelWidth = EditorGUIUtility.singleLineHeight * 5;
		
		private readonly struct LegendItem
		{
			public readonly UAIAtlasIcon icon;
			public readonly GUIContent label;

			public LegendItem(UAIAtlasIcon i, string l)
			{
				icon = i;
				label = new GUIContent(l);
			}
		}

		private static GUIContent GetWindowTitle()
		{
			var iconGUID = EditorGUIUtility.isProSkin
			? _GUID_WIN_DARK
			: _GUID_WIN;

			var icon = LoadFromGUID<Texture2D>(iconGUID);
			var title = UAIConstants.WIN_PATH_DEBUG.Substring(UAIConstants.WIN_PATH_DEBUG.LastIndexOf("/") + 1);

			return new GUIContent
			{
				text = title,
				image = icon
			};
		}

		private static UAIAtlasIcon GetAtlasIcon(Texture2D atlas, int tileCount, int x, int y)
		{
			var tileSize = 1f / tileCount;
			var rect = new Rect(new Vector2(tileSize * x, tileSize * y), Vector2.one * tileSize);
			return new UAIAtlasIcon(rect, atlas);
		}
		
		private UAIAtlasIcon GetAtlasIcon(int x, int y)
		{
			return GetAtlasIcon(_iconAtlas, 8, x, y);
		}

		private static T LoadFromGUID<T>(string guid) where T : UnityEngine.Object
		{
			return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
		}

		private void OnEnable()
		{
			titleContent = GetWindowTitle();

			_iconAtlas = LoadFromGUID<Texture2D>(_GUID_ATLAS);
			_iconAction = GetAtlasIcon(0, 0);
			_iconBucket = GetAtlasIcon(1, 0);
			_iconActive = GetAtlasIcon(0, 2);
			_iconCancelled = GetAtlasIcon(1, 2);
			_iconUncancellable = GetAtlasIcon(2, 2);
			_iconDeactivating = GetAtlasIcon(3, 2);
			_iconFinished = GetAtlasIcon(4, 2);
			_iconSelectable = GetAtlasIcon(5, 2);
			_iconMuted = GetAtlasIcon(6, 2);

			InitLegend();

			_currentBrain = null;
			_tabContentMemory = new GUIContent
			{
				image = EditorGUIUtility.IconContent("d_PreMatCylinder")?.image,
				tooltip = "Agent memory"
			};
			_tabContentConsiderations = new GUIContent
			{
				image = EditorGUIUtility.IconContent("Exposure")?.image,
				tooltip = "Current considerations"
			};
			
			_tabContentLegend = new GUIContent
			{
				image = EditorGUIUtility.IconContent("_Help")?.image,
				tooltip = "Legend"
			};

			_tabLabelTimeline = new GUIContent("Timeline")
			{
				// image = _defaultTabIcon
			};

			_tabLabelActions = new GUIContent("Actions")
			{
				// image = _defaultTabIcon
			};

			Application.quitting -= OnQuittingApp;
			Application.quitting += OnQuittingApp;
		}

		private void OnDisable()
		{
			// cleanup
			Application.quitting -= OnQuittingApp;
		}

		private void OnQuittingApp()
		{
			_currentBrain = null;
			Repaint();
		}

		private void OnGUI()
		{
			_editorStyles ??= UAIEditorStyles.CreateInstance();

			if (Mathf.Approximately(0f, _legendWidth))
			{
				_legendWidth = GetLegendPanelWidth();
			}

			var wRect = new Rect(0f, 0f, position.width, position.height);

			DrawToolbar(wRect.SliceTop(_editorStyles.ToolbarHeight));
			wRect.SliceTop(_W_SEPARATOR);
			
			DrawBrainListPanel(wRect.SliceLeft(_W_PANEL_BRAINS));

			if (_showLegend)
			{
				DrawLegendPanel(wRect.SliceRight(_legendWidth + _editorStyles.ScrollbarWidth));
			}

			if (_showMemory)
			{
				DrawMemoryPanel(wRect.SliceRight(_W_PANEL_MEMORY + _editorStyles.ScrollbarWidth));
			}

			if (_showConsiderations)
			{
				DrawConsiderationPanel(wRect.SliceRight(_W_PANEL_CONSIDERATIONS + _editorStyles.ScrollbarWidth));
			}

			DrawBrainView(wRect);
		}

		private void InitLegend()
		{
			_legendWidth = 0;
			_legendItems = new()
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
				new (GetAtlasIcon(0, 7), "Random"),
				new (GetAtlasIcon(1, 7), "Top"),
				new (GetAtlasIcon(2, 7), "Top %"),
			};
		}

		private void DrawToolbar(Rect r)
		{
			GUI.Box(r, GUIContent.none, EditorStyles.toolbar);
			_showLegend = DoToolbarButton(ref r, _tabContentLegend, _showLegend, PanelFloat.Right);
			_showMemory = DoToolbarButton(ref r, _tabContentMemory, _showMemory, PanelFloat.Right);
			_showConsiderations = DoToolbarButton(ref r, _tabContentConsiderations, _showConsiderations, PanelFloat.Right);
		}

		private float GetLegendPanelWidth()
		{
			var longestLabel = GUIContent.none;
			foreach (var leg in _legendItems)
			{
				if (leg.label.text.Length > longestLabel.text.Length)
				{
					longestLabel = leg.label;
				}
			}
			var size = _editorStyles.LegendLabelStyle.CalcSize(longestLabel);
			return size.x + size.y;
		}

		private void DrawBrainTabs(ref Rect r)
		{
			GUI.Box(r, GUIContent.none, EditorStyles.toolbar);
			
			var tbRect = r.SliceTop(_editorStyles.ToolbarHeight);

			if (DoToolbarButton(ref tbRect, _tabLabelActions, _tabBrain == BrainTab.Actions))
			{
				_tabBrain = BrainTab.Actions;
			}

			if (DoToolbarButton(ref tbRect, _tabLabelTimeline, _tabBrain == BrainTab.Timeline))
			{
				_tabBrain = BrainTab.Timeline;
			}
		}

		private bool DoToolbarButton(ref Rect toolbarRect, GUIContent label, bool enabled, PanelFloat pFloat = PanelFloat.Left)
		{
			var style = _editorStyles.ToolbarButtonStyle;

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
			GUI.Box(areaRect, GUIContent.none, EditorStyles.toolbar);
			
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

			EditorGUI.DrawRect(sIconRect, UAIEditorStyles.GetIconColorInverse() * 0.2f);
			EditorGUI.DrawRect(tIconRect, UAIEditorStyles.GetIconColor() * 0.5f);

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

			if (_tabBrain == BrainTab.Timeline)
			{
				DrawBrainTimeline(r);
			}

			var footerRect = r.SliceBottom(_editorStyles.ToolbarHeight * 1f);
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

		private void DrawBrainTimeline(Rect area)
		{
			EditorGUI.LabelField(area, "Not implemented...yet", EditorStyles.centeredGreyMiniLabel);
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

			var bRowHeight = _editorStyles.BucketLabelHeight;
			var aRowHeight = _editorStyles.ActionLabelHeight;
			var bucketRow = r.SliceTop(bRowHeight);

			var hColor = !Mathf.Approximately(br.score, 0f)
			? _SEPARATOR_COLOR
			: _SEPARATOR_COLOR * 0.7f;

			EditorGUI.DrawRect(bucketRow, hColor);

			var bIcon = bucketRow.SliceLeft(bucketRow.height).Resized(-bucketRow.height * 0.2f);
			
			_iconBucket.Draw(bIcon, UAIEditorStyles.GetIconColor());

			var bScoreRect = bucketRow.SliceRight(_editorStyles.ScoreLabelSize.x);

			EditorGUI.LabelField(bucketRow, br.bucketSO._label, _editorStyles.BucketLabelStyle);
			EditorGUI.LabelField(bScoreRect, FormatScoreLabel(br.score), _editorStyles.ScoreLabelStyle);

			if (_currentBrain.GetCurrentBucketID() == br.ID)
			{
				_currentBrain.ForEachActionInBucket(br.ID, (in UAIBrain.ActionRecord ar) =>
				{
					var actionRow = r.SliceTop(aRowHeight);

					actionRow.SliceLeft(actionRow.height * 1f);

					var iconRect = actionRow.SliceLeft(actionRow.height);
					var scoreRect = actionRow.SliceRight(_editorStyles.ScoreLabelSize.x);
					var cooldownRect = actionRow.SliceRight(_editorStyles.CooldownLabelSize.x);

					EditorGUI.LabelField(actionRow, ar.template._label, _editorStyles.ActionLabelStyle);
					EditorGUI.LabelField(scoreRect, FormatScoreLabel(ar.score), _editorStyles.ScoreLabelStyle);
					
					var stateIcon = GetActionIcon(ar, _currentBrain);
					stateIcon.Draw(iconRect.Resized(-iconRect.height * 0.15f), UAIEditorStyles.GetIconColor());

					if (ar.OnCooldown())
					{
						var timeLabel = GetFormattedTimestamp(ar.cooldownEnd - Time.time);
						GUI.Label(cooldownRect, timeLabel, _editorStyles.CooldownLabelStyle);
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
			bucketCount * _editorStyles.BucketLabelHeight
			+ actionCount * _editorStyles.ActionLabelHeight;
		}
		
		private void DrawBrainListPanel(Rect r)
		{
			var sepRect = r;
			sepRect = sepRect.SliceRight(_W_SEPARATOR * 1.5f);

			var manager = UAIManager._instance;
			if (!manager)
			{
				return;
			}

			var listBtnHeight = _editorStyles.ListButtonHeight;
			var listHeight = listBtnHeight * manager.BrainCount;
			var scrollWidth = _editorStyles.ScrollbarWidth;
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
				DrawBrainListButton(scrollRect.SliceTop(listBtnHeight), tb.AIBrain);
			});

			GUI.EndScrollView();
			
			EditorGUI.DrawRect(sepRect, _SEPARATOR_COLOR);
		}

		private void DrawBrainListButton(Rect r, UAIBrain brain)
		{
			var sepRect = r.SliceBottom(_W_SEPARATOR);

			var label = brain.GetContext().agent.gameObject.name;

			var tEnabled = GUI.enabled;
			GUI.enabled = _currentBrain != brain;
			if (GUI.Button(r, label, _editorStyles.ListButtonStyle))
			{
				_currentBrain = brain;
			}

			GUI.enabled = tEnabled;
			
			EditorGUI.DrawRect(sepRect, _SEPARATOR_COLOR);
		}

		private void DrawConsiderationPanel(Rect panelArea)
		{
			if (_currentBrain == null || !_currentBrain.IsRunning())
			{
				return;
			}

			EditorGUI.DrawRect(panelArea.SliceLeft(_W_SEPARATOR), _SEPARATOR_COLOR);

			var rowCount = _currentBrain.GetActiveConsiderationCount() + 2;
			
			var totalHeight = rowCount * _editorStyles.HeaderLabelHeight;
			var totalWidth = panelArea.width;

			if (totalHeight > panelArea.height)
			{
				totalWidth -= _editorStyles.ScrollbarWidth;
			}

			var scrollRect = new Rect(0, 0, totalWidth, totalHeight);

			var cLabelStyle = _editorStyles.HeaderLabelStyle;

			_scrollConsiderations = GUI.BeginScrollView(panelArea, _scrollConsiderations, scrollRect);

			if (_currentBrain.GetCurrentBucketID() > -1)
			{
				var bucketLabel = _currentBrain.GetCurrentBucketLabel();

				var bHeaderRect = scrollRect.SliceTop(_editorStyles.HeaderLabelHeight);

				DrawHeaderLabel(bHeaderRect, bucketLabel, _iconBucket);
			
				_currentBrain.ForEachActiveBucketConsideration((in UAIBrain.ConsiderationInfo info) =>
				{
					var rowRect = scrollRect.SliceTop(_editorStyles.HeaderLabelHeight);
					EditorGUI.LabelField(rowRect, info.consideration._label, cLabelStyle);
					EditorGUI.LabelField(rowRect, FormatScoreLabel(info.score), _editorStyles.ScoreLabelStyle);
				});
			}

			if (_currentBrain.CurrentTemplate != null)
			{
				var aHeaderRect = scrollRect.SliceTop(_editorStyles.HeaderLabelHeight);
				var actionLabel = _currentBrain.CurrentTemplate.Name;
			
				DrawHeaderLabel(aHeaderRect, actionLabel, _iconAction);

				_currentBrain.ForEachActiveActionConsideration((in UAIBrain.ConsiderationInfo info) =>
				{
					var rowRect = scrollRect.SliceTop(_editorStyles.HeaderLabelHeight);
					EditorGUI.LabelField(rowRect, info.consideration._label, cLabelStyle);
					EditorGUI.LabelField(rowRect, FormatScoreLabel(info.score), _editorStyles.ScoreLabelStyle);
				});
			}

			GUI.EndScrollView();


		}

		private void DrawHeaderLabel(Rect pos, string label, in UAIAtlasIcon icon)
		{
			EditorGUI.DrawRect(pos, Color.black * 0.2f);

			if (icon.atlas)
			{
				icon.Draw(pos.SliceLeft(pos.height).Resized(-pos.height * 0.15f), UAIEditorStyles.GetIconColor());
			}
			EditorGUI.LabelField(pos, label, _editorStyles.HeaderLabelStyle);
			
			
			
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
				scrollRect.width -= _editorStyles.ScrollbarWidth;
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
			var totalHeight = _editorStyles.LegendLabelHeight * _legendItems.Count;

			var scrollRect = new Rect(Vector2.zero, new Vector2(_legendWidth, totalHeight));

			_scrollLegend = GUI.BeginScrollView(panelArea, _scrollLegend, scrollRect);
			
			foreach (var l in _legendItems)
			{
				var rowRect = scrollRect.SliceTop(_editorStyles.LegendLabelHeight);
				var icoRect = rowRect.SliceLeft(rowRect.height).Resized(-rowRect.height * 0.15f);

				l.icon.Draw(icoRect, UAIEditorStyles.GetIconColor());
				EditorGUI.LabelField(rowRect, l.label, _editorStyles.LegendLabelStyle);
			}
			
			GUI.EndScrollView();
			
			EditorGUI.DrawRect(panelArea.SliceLeft(1f), _SEPARATOR_COLOR);
			
		}


	}
}