// smidgens @ github

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
// ReSharper disable StringLastIndexOfIsCultureSpecific.1
namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal sealed class UAIWindow_Monitor : EditorWindow
	{
		public static void Open()
		{
			var w = GetWindow<UAIWindow_Monitor>(_WIN_DOCK);
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

		[Flags]
		private enum EBrainPanel
		{
			Actions = 1,
			Timeline = 2,
			Stats = 4,
		}
		
		[Flags]
		private enum EInfoPanel
		{
			Legend = 1,
			Memory = 2,
			Considerations = 4,
			Services = 8,
		}

		private static readonly Type[] _WIN_DOCK =
		{
			Type.GetType("UnityEditor.ProjectBrowser, UnityEditor.CoreModule")
		};

		private const string _GUID_WIN = "628a4b08cba10804e937831e77ee8dea";
		private const string _GUID_WIN_DARK = "60518c8b07651564bb034a06b388aa6f";

		private static readonly float _W_PANEL_BRAINS = EditorGUIUtility.singleLineHeight * 7;
		private static readonly float _W_PANEL_MEMORY = EditorGUIUtility.singleLineHeight * 7;
		private static readonly float _W_PANEL_CONSIDERATIONS = EditorGUIUtility.singleLineHeight * 10;
		private static readonly float _W_PANEL_SERVICES = EditorGUIUtility.singleLineHeight * 7;
		private static readonly float _W_TIMER_WIDTH = EditorGUIUtility.singleLineHeight * 5;

		private const int _W_SEPARATOR = 1;
		private static readonly Color _SEPARATOR_COLOR = Color.black * 0.3f;

		private float _cachedLegendWidth;

		private readonly PrefsHandle_Int _prefsVisibleInfos = new ($"UAIEditor.panels");
		private readonly PrefsHandle_Int _prefsVisibleTab = new ($"UAIEditor.tab");

		private UAIBrain _currentBrain;
		
		// UAISelector types can provide their own icons
		private readonly Dictionary<Type, UAIAtlasIcon> _cachedSelectorIcons = new();

		private Vector2 _scrollBrainList;
		// private UAIEditorAtlas _iconAtlas;
		private UAIEditorStyles _editorStyles;
		
		private (string, Rect) _tooltip;
		private GUIStyle _tooltipStyle; // move later

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
			(EUAIAtlasIcon.Service, "Service"),
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

		private ref readonly UAIAtlasIcon GetAtlasIcon(EUAIAtlasIcon icon) => ref UAIEditorAtlas.GetIcon(icon);

		private static T LoadFromGUID<T>(string guid) where T : UnityEngine.Object
		{
			return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
		}

		private static bool HasBitFlag(int mask, int flag) => (mask & flag) != 0;

		private static int ToggleBitflag(int mask, int flag)
		{
			return HasBitFlag(mask, flag) ? mask & ~flag : mask | flag;
		}

		private class WInfoPanel
		{
			public GUIContent label;
			public Func<Vector2> sizeFn;
			public Action<Rect> bgDrawFn;
			public Action<Rect> drawFn;
			public EPanelFloat tabFloat;
			public Vector2 scroll;
			public bool fixScrollClip;
			public int visibilityFlag;
			public PrefsHandle_Int flagPrefs;
			public EUAIAtlasIcon icon = EUAIAtlasIcon.None;
		}

		private WInfoPanel[] _infoPanels = Array.Empty<WInfoPanel>();
		private WInfoPanel[] _mainPanels = Array.Empty<WInfoPanel>();

		private void OnEnable()
		{
			// _iconAtlas = UAIEditorAtlas.Create();

			titleContent = GetWindowTitle();

			_currentBrain = null;
			
			// Note:
			// this panel init is kind of horrible, but having things in one place
			// like this is a bit easier for now and to refactor later
			_mainPanels = new WInfoPanel[]
			{
				new()
				{
					label = new GUIContent("Actions"),
					visibilityFlag = (int)EBrainPanel.Actions,
					flagPrefs = _prefsVisibleTab,
					bgDrawFn = DrawPanel_Actions,
					sizeFn = () => new Vector2(0f, GetPanelHeight_Actions()),
					icon = EUAIAtlasIcon.Action,
				},
				new()
				{
					label = new GUIContent("Timeline"),
					visibilityFlag = (int)EBrainPanel.Timeline,
					flagPrefs = _prefsVisibleTab,
					bgDrawFn = DrawPanel_Timeline,
					icon = EUAIAtlasIcon.Timeline,
				},
				new()
				{
					label = new GUIContent("Stats"),
					visibilityFlag = (int)EBrainPanel.Stats,
					flagPrefs = _prefsVisibleTab,
					bgDrawFn = DrawPanel_Stats,
					icon = EUAIAtlasIcon.Stats,
					sizeFn = () =>
					{
						if (_currentBrain == null)
						{
							return default;
						}
						var rows = 1 + _currentBrain.TotalActionCount;

						var rowHeight = _editorStyles.ActionLabelHeight + _W_SEPARATOR;
						var height = rows * rowHeight;
						return new Vector2(0f, height);
					},
				},
			};

			_infoPanels = new WInfoPanel[]
			{
				// LEGEND
				new()
				{
					label = new GUIContent
					{
						image = EditorGUIUtility.FindTexture("_Help"),
						tooltip = "Legend"
					},
					visibilityFlag = (int)EInfoPanel.Legend,
					bgDrawFn = DrawPanel_Legend,
					sizeFn = () => new Vector2(GetPanelWidth_Legend(), _editorStyles.LegendLabelHeight * _legendItems.Length),
					fixScrollClip = true,
					tabFloat = EPanelFloat.Right,
					flagPrefs = _prefsVisibleInfos,
				},
				// SERVICES
				new()
				{
					label = new GUIContent
					{
						image = EditorGUIUtility.IconContent("DotFrameDotted")?.image,
						tooltip = "Services"
					},
					visibilityFlag = (int)EInfoPanel.Services,
					bgDrawFn = DrawPanel_Services,
					drawFn = r =>
					{
						if (_currentBrain == null || _currentBrain.GetActiveServiceCount() == 0)
						{
							EditorGUI.LabelField(r, "No services active", EditorStyles.centeredGreyMiniLabel);
						}
					},
					sizeFn = () =>
					{
						var height = _currentBrain != null
						? _editorStyles.LegendLabelHeight * _currentBrain.GetActiveServiceCount() + 1
						: 0;
						return new Vector2(_W_PANEL_SERVICES, height);
					},
					tabFloat = EPanelFloat.Right,
					flagPrefs = _prefsVisibleInfos,
				},
				// MEMORY
				new()
				{
					label = new GUIContent
					{
						image = EditorGUIUtility.IconContent("d_PreMatCylinder")?.image,
						tooltip = "Agent Memory"
					},
					visibilityFlag = (int)EInfoPanel.Memory,
					bgDrawFn = DrawPanel_Memory,
					drawFn = r =>
					{
						if (_currentBrain == null || _currentBrain.GetMemory().ValueCount == 0)
						{
							EditorGUI.LabelField(r, "Nothing in memory", EditorStyles.centeredGreyMiniLabel);
						}
					},
					sizeFn = () =>
					{
						if (_currentBrain == null)
						{
							return new Vector2(_W_PANEL_MEMORY, 0f);
						}
						var lineHeight = EditorGUIUtility.singleLineHeight;
						var itemHeight = lineHeight * 2; // name + value
						var totalHeight = itemHeight * _currentBrain.GetMemory().ValueCount;
						return new Vector2(_W_PANEL_MEMORY, totalHeight);
					},
					
					tabFloat = EPanelFloat.Right,
					flagPrefs = _prefsVisibleInfos,
				},
				// CONSIDERATIONS
				new()
				{
					label = new GUIContent
					{
						image = EditorGUIUtility.IconContent("Exposure")?.image,
						tooltip = "Active Considerations"
					},
					visibilityFlag = (int)EInfoPanel.Considerations,
					bgDrawFn = DrawPanel_Considerations,
					drawFn = r =>
					{
						if (_currentBrain == null || _currentBrain.GetActiveConsiderationCount() == 0)
						{
							EditorGUI.LabelField(r, "No considerations", EditorStyles.centeredGreyMiniLabel);
						}
					},
					sizeFn = () =>
					{
						if (_currentBrain == null)
						{
							return new Vector2(_W_PANEL_CONSIDERATIONS, 0f);
						}
						var rowCount = _currentBrain.GetActiveConsiderationCount() + 2;
						var height = rowCount * _editorStyles.HeaderLabelHeight;
						return new Vector2(_W_PANEL_CONSIDERATIONS, height);

					},
					tabFloat = EPanelFloat.Right,
					flagPrefs = _prefsVisibleInfos,
				},
				
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

		private bool CheckMouseInLocalRect(Rect localRect)
		{
			var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
			return localRect.Contains(Event.current.mousePosition);
		}

		private void SetTooltip(Vector2 mousePos, string label)
		{
			mousePos = GUIUtility.GUIToScreenPoint(mousePos);
			mousePos.y -= 25;
			
			if (_tooltipStyle == null)
			{
				_tooltipStyle = new GUIStyle(GUI.skin.label);
			}

			if (!position.Contains(mousePos))
			{
				return;
			}

			var wPos = position.position;
			var wSize = position.size;

			var tpSize = _tooltipStyle.CalcSize(new GUIContent(label));

			var tpPos = new Vector2(mousePos.x - wPos.x, mousePos.y - wPos.y);
	
			tpPos.x = Mathf.Clamp(tpPos.x, 0, wSize.x - tpSize.x);
			tpPos.y = Mathf.Clamp(tpPos.y, 0, wSize.y - tpSize.y);

			_tooltip = (label, new Rect(tpPos, tpSize));
		}


		private void OnGUI()
		{
			_editorStyles ??= UAIEditorStyles.CreateInstance();
			
			var wRect = new Rect(0f, 0f, position.width, position.height);

			DrawPanelTabs(ref wRect, _infoPanels);
			
			wRect.SliceTop(_W_SEPARATOR);

			DrawPanel_BrainList(wRect.SliceLeft(_W_PANEL_BRAINS));
			EditorGUI.DrawRect(wRect.SliceLeft(_W_SEPARATOR), _SEPARATOR_COLOR);

			DrawInfoPanels(ref wRect);
			DrawBrainView(wRect);

			if (!string.IsNullOrEmpty(_tooltip.Item1))
			{
				EditorGUI.DrawRect(_tooltip.Item2, Color.black * 0.7f);
				EditorGUI.LabelField(_tooltip.Item2, _tooltip.Item1, GUI.skin.label);
				_tooltip = default;
				
			}


		}

		private void DrawInfoPanels(ref Rect area)
		{
			for (int i = 0; i < _infoPanels.Length; i++)
			{
				ref var panel = ref _infoPanels[i];

				if (!HasBitFlag(panel.flagPrefs.Value, panel.visibilityFlag))
				{
					continue;
				}

				var size = panel.sizeFn.Invoke();
				var width = size.x + _editorStyles.ScrollbarWidth;

				var innerWidth = width;

				if (panel.fixScrollClip)
				{
					innerWidth -= _editorStyles.ScrollbarWidth;
				}

				var panelArea = panel.tabFloat == EPanelFloat.Left
				? area.SliceLeft(width)
				: area.SliceRight(width);
				DrawVerticalSeparator(ref area, panel.tabFloat);

				// background etc
				panel.drawFn?.Invoke(panelArea);

				var height = Mathf.Approximately(size.y, 0f) ? area.height : size.y;

				var scrollRect = new Rect(0, 0, innerWidth, height);

				if (!panel.fixScrollClip && scrollRect.height > area.height)
				{
					scrollRect.width -= _editorStyles.ScrollbarWidth;
				}
				panel.scroll = GUI.BeginScrollView(panelArea, panel.scroll, scrollRect);
				panel.bgDrawFn?.Invoke(scrollRect);
				GUI.EndScrollView();

			}
		}

		private void DrawPanelTabs(ref Rect r, WInfoPanel[] panels, bool multiFlag = true)
		{
			var tbRect = r.SliceTop(_editorStyles.ToolbarHeight);
			GUI.Box(tbRect, GUIContent.none, EditorStyles.toolbar);
			foreach (var panel in panels)
			{
				DoPanelToggle(ref tbRect, panel, multiFlag);
			}
		}

		private void DoPanelToggle(ref Rect r, in WInfoPanel panel, bool multiFlag = true)
		{
			var enabled = HasBitFlag(panel.flagPrefs.Value, panel.visibilityFlag);
			if (DoToolbarButton(ref r, panel, enabled))
			{
				if (!multiFlag)
				{
					panel.flagPrefs.Value = panel.visibilityFlag;
				}
				else
				{
					panel.flagPrefs.Value = ToggleBitflag(panel.flagPrefs.Value, panel.visibilityFlag);
				}
			}
		}

		private bool DoToolbarButton(ref Rect toolbarRect, in WInfoPanel panel, bool enabled)
		{
			var showIcon = panel.icon != EUAIAtlasIcon.None;

			var style = showIcon ? _editorStyles.ToolbarIconButtonStyle : _editorStyles.ToolbarButtonStyle;

			var size = style.CalcSize(panel.label);

			var btnRect = panel.tabFloat == EPanelFloat.Right
			? toolbarRect.SliceRight(size.x)
			: toolbarRect.SliceLeft(size.x);

			var pressed = GUI.Button(btnRect, panel.label, style);

			if (showIcon)
			{
				var iconRect = btnRect;
				iconRect = iconRect.SliceLeft(btnRect.height).Resized(-btnRect.height * 0.2f);
				
				UAIEditorAtlas.GetIcon(panel.icon).Draw(iconRect, UAIEditorStyles.GetIconColor() * 0.8f);
			}

			if (enabled)
			{
				EditorGUI.DrawRect(btnRect.Resized(-1f), Color.white * 0.4f);
			}
			return pressed;
		}

		private UAIAtlasIcon GetSelectorIcon(UAISelector selector)
		{
			if (_cachedSelectorIcons.TryGetValue(selector.GetType(), out var outIcon))
			{
				return outIcon;
			}
			var (guid, coords) = selector.GetDebugIcon();
			var tex = !string.IsNullOrEmpty(guid)
			? LoadFromGUID<Texture2D>(guid)
			: null;
			var icon = new UAIAtlasIcon(coords, tex);
			_cachedSelectorIcons[selector.GetType()] = icon;
			return icon;
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
			
			DrawPanelTabs(ref r, _mainPanels, false);
			
			var footerRect = r.SliceBottom(_editorStyles.ToolbarHeight);
			
			DrawBrainFooter(footerRect);

			for (int i = 0; i < _mainPanels.Length; i++)
			{
				ref var panel = ref _mainPanels[i];

				if (panel.bgDrawFn == null)
				{
					continue;
				}

				if (!HasBitFlag(panel.flagPrefs.Value, panel.visibilityFlag))
				{
					continue;
				}

				var size = panel.sizeFn?.Invoke() ?? r.size;

				panel.drawFn?.Invoke(r);

				var height = Mathf.Approximately(size.y, 0f) ? r.height : size.y;
	
				var scrollRect = new Rect(0, 0, r.width, height);

				if (scrollRect.height > r.height)
				{
					scrollRect.width -= _editorStyles.ScrollbarWidth;
				}
				panel.scroll = GUI.BeginScrollView(r, panel.scroll, scrollRect);
				panel.bgDrawFn.Invoke(scrollRect);
				GUI.EndScrollView();

				break;
			}
		}
		
		private void DrawPanel_Actions(Rect scrollRect)
		{
			_currentBrain.ForEachBucket((in UAIBrain.BucketRecord br) =>
			{
				DrawBucketActivity(ref scrollRect, br);
			});
		}

		private void DrawPanel_Timeline(Rect area)
		{
			EditorGUI.LabelField(area, "Not implemented...yet", EditorStyles.centeredGreyMiniLabel);
		}

		private (GUIContent, UAIDelegates.ActionRefRO<Rect, UAIBrain.ActionRecord>)[] _actionStatColumns;
		
		private void DrawPanel_Stats(Rect area)
		{
			// this is a mess...
			if (_actionStatColumns == null)
			{
				_actionStatColumns = new (GUIContent, UAIDelegates.ActionRefRO<Rect, UAIBrain.ActionRecord>)[]
				{
					(new GUIContent("Select %  "), (in Rect pos, in UAIBrain.ActionRecord action) =>
					{
						var ratio = _currentBrain.TotalActivations > 0
						? (float)(action.activations) / _currentBrain.TotalActivations
						: 0f;
						
						var label = $"{ratio * 100f:0}%";

						var barRect = pos.Resized(-2f);
						barRect.width -= 2f;
						barRect.center = pos.center;
						
						EditorGUI.ProgressBar(barRect, ratio, "");
						EditorGUI.LabelField(barRect, label, EditorStyles.centeredGreyMiniLabel);
						
					}),
					(new GUIContent("Avg. Score  "), (in Rect pos, in UAIBrain.ActionRecord action) =>
					{
						var label = action.activations > 0
						? FormatScoreLabel((float)(action.scoreSum / action.activations))
						: "-";
						EditorGUI.LabelField(pos, label, _editorStyles.ActionLabelStyle);
					}),
					(new GUIContent("Score   "), (in Rect pos, in UAIBrain.ActionRecord action) =>
					{
						var sust = action.ShouldSustainScore() && action.ID == _currentBrain.CurrentActionID;
						var label = true
						? $"{(sust ? "~" : "")}{FormatScoreLabel(action.GetScore())}"
						: "-";
						EditorGUI.LabelField(pos, label, _editorStyles.ActionLabelStyle);
					}),
					(new GUIContent("Activations  "), (in Rect pos, in UAIBrain.ActionRecord action) =>
					{
						var label = action.activations > 0
						? action.activations.ToString()
						: "-";
						EditorGUI.LabelField(pos, label, _editorStyles.ActionLabelStyle);
					}),
					(new GUIContent("Total Time  "), (in Rect pos, in UAIBrain.ActionRecord action) =>
					{
						var time = action.GetTotalActiveTime();

						var label = "-";

						if (!Mathf.Approximately(time, 0f))
						{
							if (time > 60f)
							{
								label = $"{(time / 60f):0}m {(time % 60f):0}s";
							}
							else
							{
								label = $"{time:0.0}s";
							}
						}
						EditorGUI.LabelField(pos, label, _editorStyles.ActionLabelStyle);
					}),
					(new GUIContent("Weight  "), (in Rect pos, in UAIBrain.ActionRecord action) =>
					{
						EditorGUI.LabelField(pos, $"{action.template._weight:0.0}", _editorStyles.ActionLabelStyle);
					}),
					
					(new GUIContent("Cooldown  "), (in Rect pos, in UAIBrain.ActionRecord action) =>
					{
						if (action.OnCooldown())
						{
							var timeLabel = UAIEditorGUI.GetFormattedDuration(action.cooldownEnd - Time.time);
							GUI.Label(pos, timeLabel, _editorStyles.ActionLabelStyle);
						}
						else
						{
							EditorGUI.LabelField(pos, "-", _editorStyles.ActionLabelStyle);
							
						}
					}),
					
				};
			}

			var rowHeight = _editorStyles.ActionLabelHeight;

			var rColor = _SEPARATOR_COLOR * 0.25f;
			var hColor = _SEPARATOR_COLOR * 0.7f;

			var rowStyle = _editorStyles.ActionLabelStyle;

			var headerRect = area.SliceTop(rowHeight);
			area.SliceTop(_W_SEPARATOR);
			
			EditorGUI.DrawRect(headerRect, hColor);
			
			foreach (var (colLabel, drawFn) in _actionStatColumns)
			{
				var colWidth = rowStyle.CalcSize(colLabel).x;
				EditorGUI.LabelField(headerRect.SliceRight(colWidth), colLabel, rowStyle);
			}

			// var aIcon = GetAtlasIcon(EUAIAtlasIcon.Action);
			

			EditorGUI.LabelField(headerRect, "Action", rowStyle);

			_currentBrain.ForEachBucket((in UAIBrain.BucketRecord bucket) =>
			{
				var bLabel = bucket.label;
				_currentBrain.ForEachActionInBucket(bucket.ID, (in UAIBrain.ActionRecord action) =>
				{
					var aRow = area.SliceTop(rowHeight);

					var aIcon = GetAtlasIcon(GetActionStatusIcon(action, _currentBrain));

					var overlayRect = aRow;
	
					EditorGUI.DrawRect(aRow, rColor);

					var label = $"{bLabel} / {action.template.Name}";

					foreach (var (colLabel, drawFn) in _actionStatColumns)
					{
						var colWidth = rowStyle.CalcSize(colLabel).x;
						drawFn.Invoke(aRow.SliceRight(colWidth), action);
					}
					
					var iconRect = aRow.SliceLeft(headerRect.height).Resized(-headerRect.height * 0.15f);

					var active = action.ID == _currentBrain.CurrentActionID;

					aIcon.Draw(iconRect, UAIEditorStyles.GetIconColor() * (active ? 1f : 0.5f));

					EditorGUI.LabelField(aRow, label, rowStyle);

					if (active)
					{
						var c = Color.white;
						c.a = 0.1f;
						EditorGUI.DrawRect(overlayRect, c);
					}
					
					area.SliceTop(_W_SEPARATOR);
				}, false);
			}, false);
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
					DrawActionRow(r.SliceTop(aRowHeight), ar);
				});
				UAIEditorGUI.TimedPulse(pulseRect, br.lastActivation);
			}
			else
			{
				if (_currentBrain.CurrentActionID > -1)
				{
					ref readonly UAIBrain.ActionRecord currentAction = ref _currentBrain.GetCurrentActionRef();

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

			var scoreLabel = FormatScoreLabel(ar.GetScore());
			EditorGUI.LabelField(scoreRect, scoreLabel, _editorStyles.ScoreLabelStyle);
	
			var stateIconType = GetActionStatusIcon(ar, _currentBrain);
			ref readonly var stateIcon = ref GetAtlasIcon(stateIconType);
			stateIcon.Draw(iconRect.Resized(-iconRect.height * 0.15f), UAIEditorStyles.GetIconColor());

			if (ar.OnCooldown())
			{
				var timeLabel = UAIEditorGUI.GetFormattedDuration(ar.cooldownEnd - Time.time);
				GUI.Label(cooldownRect, timeLabel, _editorStyles.CooldownLabelStyle);
			}
			if (ar.ID == _currentBrain.CurrentActionID)
			{
				UAIEditorGUI.TimedPulse(cachedRow, ar.lastActivation);
			}
		}

		private static EUAIAtlasIcon GetActionStatusIcon(in UAIBrain.ActionRecord action, UAIBrain brain)
		{
			if (action.bucketID != brain.CurrentBucketID)
			{
				return EUAIAtlasIcon.Muted;
			}
			
			if (brain.CurrentActionID == action.ID)
			{
				if (action.deactivating)
				{
					return EUAIAtlasIcon.Deactivating;
				}
				return !action.cancellable
				? EUAIAtlasIcon.Uncancellable
				: EUAIAtlasIcon.Active;
			}
			if (action.OnCooldown())
			{
				return action.cancelled
				? EUAIAtlasIcon.Cancelled
				: EUAIAtlasIcon.Finished;
			}
			return  Mathf.Approximately(action.score, 0)
			? EUAIAtlasIcon.Muted
			: EUAIAtlasIcon.Selectable;
		}

		private float GetPanelHeight_Actions()
		{
			var bucketCount = _currentBrain.BucketCount;
			var actionCount = _currentBrain.GetCurrentBucketActionCount();
			
			ref readonly var currentAction = ref _currentBrain.GetCurrentActionRef();

			if (currentAction.bucketID != _currentBrain.CurrentBucketID)
			{
				actionCount++;
			}
			return
			bucketCount * _editorStyles.BucketLabelHeight
			+ actionCount * _editorStyles.ActionLabelHeight;
		}

		private static void DrawVerticalSeparator(ref Rect r, EPanelFloat dir = EPanelFloat.Left)
		{
			var sepRect = dir == EPanelFloat.Left
			? r.SliceLeft(_W_SEPARATOR)
			: r.SliceRight(_W_SEPARATOR);
			EditorGUI.DrawRect(sepRect, _SEPARATOR_COLOR);
		}

		private void DrawPanel_Services(Rect scrollRect)
		{
			if (_currentBrain == null)
			{
				return;
			}

			ref readonly var currentBucket = ref _currentBrain.GetBucketRef(_currentBrain.CurrentBucketID);

			if (currentBucket.services.Length == 0)
			{
				return;
			}

			var lineHeight = _editorStyles.LegendLabelHeight;

			var headerRect = scrollRect.SliceTop(lineHeight);
			
			DrawHeaderLabel(headerRect, currentBucket.label, GetAtlasIcon(EUAIAtlasIcon.Bucket));
		
			// var rowCount = currentBucket.services.Length;

			// var totalHeight = rowCount * lineHeight;
			//
			// var scrollRect = new Rect(0, 0, r.width, totalHeight);
			//
			// if (totalHeight > r.height)
			// {
			// 	scrollRect.width -= _editorStyles.ScrollbarWidth;
			// }

			foreach (var s in currentBucket.services)
			{
				var rowRect = scrollRect.SliceTop(lineHeight);
				var iconRect = rowRect.SliceLeft(rowRect.height).Resized(-rowRect.height * 0.2f);
				
				EditorGUI.LabelField(rowRect, s.Name, EditorStyles.miniLabel);
				GetAtlasIcon(EUAIAtlasIcon.Service).Draw(iconRect);

			}
		}
		
		private void DrawPanel_BrainList(Rect r)
		{
			var manager = UAIManager._instance;
			if (!manager)
			{
				return;
			}

			var itemHeight = _editorStyles.ListButtonHeight + _W_SEPARATOR;
			var listHeight = itemHeight * manager.BrainCount;
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
				_currentBrain ??= tb.AIBrain;

				var btnRect = scrollRect.SliceTop(itemHeight);
				var label = tb.AIBrain.GetContext().agent.gameObject.name;
				var tEnabled = GUI.enabled;
				GUI.enabled = _currentBrain != tb.AIBrain;
				if (GUI.Button(btnRect, label, _editorStyles.ListButtonStyle))
				{
					_currentBrain = tb.AIBrain;
				}
				GUI.enabled = tEnabled;
				EditorGUI.DrawRect(scrollRect.SliceTop(_W_SEPARATOR), _SEPARATOR_COLOR);
			});
			GUI.EndScrollView();
		}

		private void DrawPanel_Considerations(Rect scrollRect)
		{
			if (_currentBrain == null)
			{
				return;
			}
			var cLabelStyle = _editorStyles.HeaderLabelStyle;

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

		private void DrawPanel_Memory(Rect scrollRect)
		{
			if (_currentBrain == null)
			{
				return;
			}

			var memory = _currentBrain.GetMemory();

			if (memory.ValueCount == 0)
			{
				return;
			}

			var lineHeight = EditorGUIUtility.singleLineHeight;

			memory.ForEachMemoryValue(((in UAIMemoryKey k, in UAIMemoryValue v) =>
			{
				var hRect = scrollRect.SliceTop(lineHeight);
				EditorGUI.DrawRect(hRect, Color.black * 0.2f);
				var hText = $"{k.Label} ({k.MemoryType.Name})";
				EditorGUI.LabelField(hRect, hText, EditorStyles.miniLabel);
				EditorGUI.LabelField(scrollRect.SliceTop(lineHeight), k.StringifyValue(v), EditorStyles.miniLabel);
			}));
		}

		private float GetPanelWidth_Legend()
		{
			if (!Mathf.Approximately(_cachedLegendWidth, 0f))
			{
				return _cachedLegendWidth;
			}
			var longestLabel = string.Empty;
			foreach (var (_, label) in _legendItems)
			{
				if (label.Length > longestLabel.Length)
				{
					longestLabel = label;
				}
			}
			var size = _editorStyles.LegendLabelStyle.CalcSize(new GUIContent(longestLabel));
			_cachedLegendWidth = size.x + size.y;
			return _cachedLegendWidth;
		}
	
		private void DrawPanel_Legend(Rect scrollRect)
		{
			foreach (var (iconKey, label) in _legendItems)
			{
				var rowRect = scrollRect.SliceTop(_editorStyles.LegendLabelHeight);
				var icoRect = rowRect.SliceLeft(rowRect.height).Resized(-rowRect.height * 0.15f);
				var icon = GetAtlasIcon(iconKey);
				icon.Draw(icoRect, UAIEditorStyles.GetIconColor());
				EditorGUI.LabelField(rowRect, label, _editorStyles.LegendLabelStyle);

				if (CheckMouseInLocalRect(rowRect))
				{
					// SetTooltip(Event.current.mousePosition, label);
				}
				
			}
		}


	}
}