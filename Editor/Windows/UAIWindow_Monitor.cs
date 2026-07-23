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

		private enum EPanelFloat { Left, Right }

		private enum EBrainTab { Actions, Timeline }

		[Flags]
		private enum EActivePanel
		{
			Legend = 1,
			Memory = 2,
			Considerations = 4,
		}

		private static readonly Type[] WIN_DOCK =
		{
			Type.GetType("UnityEditor.ProjectBrowser, UnityEditor.CoreModule")
		};

		private const string _GUID_WIN = "628a4b08cba10804e937831e77ee8dea";
		private const string _GUID_WIN_DARK = "60518c8b07651564bb034a06b388aa6f";

		private static readonly float _W_PANEL_BRAINS = EditorGUIUtility.singleLineHeight * 7;
		private static readonly float _W_PANEL_MEMORY = EditorGUIUtility.singleLineHeight * 7;
		private static readonly float _W_PANEL_CONSIDERATIONS = EditorGUIUtility.singleLineHeight * 10;
		private static readonly float _W_TIMER_WIDTH = EditorGUIUtility.singleLineHeight * 5;

		private const int _W_SEPARATOR = 1;
		private static readonly Color _SEPARATOR_COLOR = Color.black * 0.3f;

		[SerializeField] private bool _showLegend;
		[SerializeField] private bool _showMemory;
		[SerializeField] private bool _showConsiderations = true;
		[SerializeField] private EBrainTab _tabBrain;
		private UAIBrain _currentBrain;
		private GUIContent _tabContentMemory;
		private GUIContent _tabContentLegend;
		private GUIContent _tabContentConsiderations;
		private GUIContent _tabLabelTimeline;
		private GUIContent _tabLabelActions;
		private readonly Dictionary<string, Texture2D> _loadedTextures = new();
		private Vector2 _scrollBrainList;
		private Vector2 _scrollActivityView;
		private Vector2 _scrollLegend;
		private Vector2 _scrollMemory;
		private Vector2 _scrollConsiderations;

		private UAIEditorAtlas _iconAtlas;
		private UAIEditorStyles _editorStyles;
		private float _legendWidth;

		private readonly (EUAIAtlasIcon, string)[] _legendItems =
		{
			(EUAIAtlasIcon.Active, "Active"),
			(EUAIAtlasIcon.Cancelled, "Cancelled"),
			(EUAIAtlasIcon.Uncancellable, "Uncancellable"),
			(EUAIAtlasIcon.Deactivating, "Deactivating"),
			(EUAIAtlasIcon.Finished, "Finished"),
			(EUAIAtlasIcon.Selectable, "Selectable"),
			(EUAIAtlasIcon.Muted, "Muted"),
			(EUAIAtlasIcon.Action, "Action"),
			(EUAIAtlasIcon.Bucket, "Bucket"),
			(EUAIAtlasIcon.Consideration, "Consideration"),
			(EUAIAtlasIcon.SelectRandom, "Random"),
			(EUAIAtlasIcon.SelectTop, "Top"),
			(EUAIAtlasIcon.SelectTopPercentage, "Top %"),
		};

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

		private UAIAtlasIcon GetAtlasIcon(EUAIAtlasIcon icon) => _iconAtlas.GetIcon(icon);

		private static T LoadFromGUID<T>(string guid) where T : UnityEngine.Object
		{
			return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
		}

		private void OnEnable()
		{
			_iconAtlas = UAIEditorAtlas.Create();

			titleContent = GetWindowTitle();

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

			DrawPanel_BrainList(wRect.SliceLeft(_W_PANEL_BRAINS));

			if (_showLegend)
			{
				DrawPanel_Legend(wRect.SliceRight(_legendWidth + _editorStyles.ScrollbarWidth));
			}

			if (_showMemory)
			{
				DrawPanel_Memory(wRect.SliceRight(_W_PANEL_MEMORY + _editorStyles.ScrollbarWidth));
			}

			if (_showConsiderations)
			{
				DrawPanel_Considerations(wRect.SliceRight(_W_PANEL_CONSIDERATIONS + _editorStyles.ScrollbarWidth));
			}

			DrawBrainView(wRect);
		}

		private static void PulseRect(in Rect rect, float startTime)
		{
			var color = EditorGUIUtility.isProSkin
			? Color.white
			: Color.black;
			color.a = 0.3f;
			float duration = 0.4f;
			var endTime = startTime + duration;
			if (Time.time > endTime)
			{
				return;
			}
			var t = Mathf.Clamp01((endTime - Time.time) / duration);
			t = Mathf.PingPong(t, 2) / 0.5f;

			EditorGUI.DrawRect(rect, Color.Lerp(Color.clear, color, t));

		}

		private void DrawToolbar(Rect r)
		{
			GUI.Box(r, GUIContent.none, EditorStyles.toolbar);
			_showLegend = DoToolbarButton(ref r, _tabContentLegend, _showLegend, EPanelFloat.Right);
			_showMemory = DoToolbarButton(ref r, _tabContentMemory, _showMemory, EPanelFloat.Right);
			_showConsiderations = DoToolbarButton(ref r, _tabContentConsiderations, _showConsiderations, EPanelFloat.Right);
		}

		private float GetLegendPanelWidth()
		{
			var longestLabel = "";
			foreach (var (_, label) in _legendItems)
			{
				if (label.Length > longestLabel.Length)
				{
					longestLabel = label;
				}
			}
			var size = _editorStyles.LegendLabelStyle.CalcSize(new GUIContent(longestLabel));
			return size.x + size.y;
		}

		private void DrawBrainTabs(ref Rect r)
		{
			GUI.Box(r, GUIContent.none, EditorStyles.toolbar);
			
			var tbRect = r.SliceTop(_editorStyles.ToolbarHeight);

			if (DoToolbarButton(ref tbRect, _tabLabelActions, _tabBrain == EBrainTab.Actions))
			{
				_tabBrain = EBrainTab.Actions;
			}

			if (DoToolbarButton(ref tbRect, _tabLabelTimeline, _tabBrain == EBrainTab.Timeline))
			{
				_tabBrain = EBrainTab.Timeline;
			}
		}

		private bool DoToolbarButton(ref Rect toolbarRect, GUIContent label, bool enabled, EPanelFloat pFloat = EPanelFloat.Left)
		{
			var style = _editorStyles.ToolbarButtonStyle;

			var size = style.CalcSize(label);

			var btnRect = pFloat == EPanelFloat.Right
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

		private Texture2D GetCachedTextureByGUID(string texGUID)
		{
			if (_loadedTextures.TryGetValue(texGUID, out var outTex))
			{
				return outTex;
			}
			var tex = LoadFromGUID<Texture2D>(texGUID);
			_loadedTextures[texGUID] = tex;
			return tex;
		}

		private UAIAtlasIcon GetSelectorIcon(UAISelector selector)
		{
			var tex = GetCachedTextureByGUID(selector.GetDebugIconGUID());
			return new UAIAtlasIcon(selector.GetDebugIconCoords(), tex);
		}

		private void DrawBrainFooter(Rect areaRect)
		{
			GUI.Box(areaRect, GUIContent.none, EditorStyles.toolbar);
			
			var sepRect = areaRect.SliceTop(_W_SEPARATOR);
			areaRect.SliceLeft(EditorGUIUtility.singleLineHeight * 0.25f);
			var bTimerRect = areaRect.SliceLeft(_W_TIMER_WIDTH);
			areaRect.SliceLeft(EditorGUIUtility.singleLineHeight * 0.25f);
			var aTimerRect = areaRect.SliceLeft(_W_TIMER_WIDTH);

			var aSelector = _currentBrain.GetCurrentActionSelector();
			var bSelector = _currentBrain.GetCurrentBucketSelector();

			var aIcon = GetAtlasIcon(EUAIAtlasIcon.Action);
			var bIcon = GetAtlasIcon(EUAIAtlasIcon.Bucket);

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

			if (_tabBrain == EBrainTab.Actions)
			{
				DrawBrainActions(r);
			}
			else if (_tabBrain == EBrainTab.Timeline)
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

			var pulseRect = bucketRow;

			var hColor = !Mathf.Approximately(br.score, 0f)
			? _SEPARATOR_COLOR
			: _SEPARATOR_COLOR * 0.7f;

			EditorGUI.DrawRect(bucketRow, hColor);

			var bIcon = bucketRow.SliceLeft(bucketRow.height).Resized(-bucketRow.height * 0.2f);
			
			GetAtlasIcon(EUAIAtlasIcon.Bucket).Draw(bIcon, UAIEditorStyles.GetIconColor());

			var bScoreRect = bucketRow.SliceRight(_editorStyles.ScoreLabelSize.x);

			EditorGUI.LabelField(bucketRow, br.bucketSO.BucketName, _editorStyles.BucketLabelStyle);
			EditorGUI.LabelField(bScoreRect, FormatScoreLabel(br.score), _editorStyles.ScoreLabelStyle);

			if (_currentBrain.CurrentBucketID == br.ID)
			{
				_currentBrain.ForEachActionInBucket(br.ID, (in UAIBrain.ActionRecord ar) =>
				{
					// var actionRow = r.SliceTop(aRowHeight);
					DrawActionRow(r.SliceTop(aRowHeight), ar);
				});
				
				PulseRect(pulseRect, br.lastActivation);
			}
			else
			{
				if (_currentBrain.CurrentActionID > -1)
				{
					ref readonly UAIBrain.ActionRecord currentAction = ref _currentBrain.GetCurrentAction();

					if (currentAction.bucketID == br.ID)
					{
						DrawActionRow(r.SliceTop(aRowHeight), currentAction);
					}

				}
			}
			
			
			
			EditorGUI.DrawRect(r.SliceTop(_W_SEPARATOR), _SEPARATOR_COLOR);
			rr = r;
		}

		private void DrawActionRow(Rect actionRow, in UAIBrain.ActionRecord ar)
		{
			var cachedRow = actionRow;
			
			actionRow.SliceLeft(actionRow.height * 1f);
			var iconRect = actionRow.SliceLeft(actionRow.height);
			var scoreRect = actionRow.SliceRight(_editorStyles.ScoreLabelSize.x);
			var cooldownRect = actionRow.SliceRight(_editorStyles.CooldownLabelSize.x);

			EditorGUI.LabelField(actionRow, ar.template.Name, _editorStyles.ActionLabelStyle);

			var scoreLabel = FormatScoreLabel(ar.SustainedScore());
			EditorGUI.LabelField(scoreRect, scoreLabel, _editorStyles.ScoreLabelStyle);
	
			var stateIcon = GetActionIcon(ar, _currentBrain);
			stateIcon.Draw(iconRect.Resized(-iconRect.height * 0.15f), UAIEditorStyles.GetIconColor());

			if (ar.OnCooldown())
			{
				var timeLabel = GetFormattedTimestamp(ar.cooldownEnd - Time.time);
				GUI.Label(cooldownRect, timeLabel, _editorStyles.CooldownLabelStyle);
			}
			if (ar.actionID == _currentBrain.CurrentActionID)
			{
				PulseRect(cachedRow, ar.lastActivation);
			}
			
		}

		private static string GetFormattedTimestamp(float timeSeconds)
		{
			int val = timeSeconds < 1 ? (int)(timeSeconds * 1000) : (int)timeSeconds;
			return val + (timeSeconds < 1 ? "ms" : "s");
		}

		private UAIAtlasIcon GetActionIcon(in UAIBrain.ActionRecord action, UAIBrain aiBrain)
		{
			if (aiBrain.CurrentActionID == action.actionID)
			{
				if (action.deactivating)
				{
					return GetAtlasIcon(EUAIAtlasIcon.Deactivating);
				}

				return !action.cancellable
				? GetAtlasIcon(EUAIAtlasIcon.Uncancellable)
				: GetAtlasIcon(EUAIAtlasIcon.Active);
			}
			if (action.OnCooldown())
			{
				return action.cancelled
				? GetAtlasIcon(EUAIAtlasIcon.Cancelled)
				: GetAtlasIcon(EUAIAtlasIcon.Finished);
			}

			return Mathf.Approximately(action.score, 0)
			? GetAtlasIcon(EUAIAtlasIcon.Muted)
			: GetAtlasIcon(EUAIAtlasIcon.Selectable);
		}

		private float GetCurrentBrainActivityHeight()
		{
			var bucketCount = _currentBrain.GetBucketCount();
			var actionCount = _currentBrain.GetCurrentBucketActionCount();
			// we're still exiting action in bucket
			if (_currentBrain.GetCurrentActionBucketID() != _currentBrain.CurrentBucketID)
			{
				actionCount++;
			}
			return
			bucketCount * _editorStyles.BucketLabelHeight
			+ actionCount * _editorStyles.ActionLabelHeight;
		}
		
		private void DrawPanel_BrainList(Rect r)
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

		private void DrawPanel_Considerations(Rect panelArea)
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

			if (_currentBrain.CurrentBucketID > -1)
			{
				var bucketLabel = _currentBrain.GetCurrentBucketLabel();

				var bHeaderRect = scrollRect.SliceTop(_editorStyles.HeaderLabelHeight);

				DrawHeaderLabel(bHeaderRect, bucketLabel, GetAtlasIcon(EUAIAtlasIcon.Bucket));
			
				_currentBrain.ForEachActiveBucketConsideration((in UAIBrain.ConsiderationInfo info) =>
				{
					var rowRect = scrollRect.SliceTop(_editorStyles.HeaderLabelHeight);
					EditorGUI.LabelField(rowRect, info.consideration._label, cLabelStyle);
					EditorGUI.LabelField(rowRect, FormatScoreLabel(info.score), _editorStyles.ScoreLabelStyle);
				});
			}

			if (_currentBrain.CurrentActionTemplate != null)
			{
				var aHeaderRect = scrollRect.SliceTop(_editorStyles.HeaderLabelHeight);
				var actionLabel = _currentBrain.CurrentActionTemplate.Name;
			
				DrawHeaderLabel(aHeaderRect, actionLabel, GetAtlasIcon(EUAIAtlasIcon.Action));

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

		private void DrawPanel_Memory(Rect panelArea)
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

			if (memory.ValueCount == 0)
			{
				EditorGUI.LabelField(panelArea, "Memory empty", EditorStyles.centeredGreyMiniLabel);
				return;
			}

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

		private void DrawPanel_Legend(Rect panelArea)
		{
			var totalHeight = _editorStyles.LegendLabelHeight * _legendItems.Length;

			var scrollRect = new Rect(Vector2.zero, new Vector2(_legendWidth, totalHeight));

			_scrollLegend = GUI.BeginScrollView(panelArea, _scrollLegend, scrollRect);
			
			foreach (var (iconKey, label) in _legendItems)
			{
				var rowRect = scrollRect.SliceTop(_editorStyles.LegendLabelHeight);
				var icoRect = rowRect.SliceLeft(rowRect.height).Resized(-rowRect.height * 0.15f);
				var icon = GetAtlasIcon(iconKey);
				icon.Draw(icoRect, UAIEditorStyles.GetIconColor());
				EditorGUI.LabelField(rowRect, label, _editorStyles.LegendLabelStyle);
			}
			
			GUI.EndScrollView();
			
			EditorGUI.DrawRect(panelArea.SliceLeft(1f), _SEPARATOR_COLOR);
			
		}


	}
}