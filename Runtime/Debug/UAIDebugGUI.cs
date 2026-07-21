// // smidgens @ github
//
// // ReSharper disable All
//
// using System.Collections.Generic;
// using System.Linq;
//
// namespace Smidgenomics.Unity.UAI
// {
// 	using UnityEngine;
// 	using System;
//
// 	internal static class UAIDebugGUI
// 	{
// 		internal static void DrawActivityOverlay()
// 		{
// 			
// 			var manager = UAIManager.GetInstance();
//
// 			if (!manager)
// 			{
// 				return;
// 			}
// 			
// 			DrawLegend();
//
// 			List<UAIManager.TrackedBrain> brains = new();
// 		
// 			manager.ForEachTrackedBrain((in UAIManager.TrackedBrain tbrain) => brains.Add(tbrain));
//
// 			foreach (var tbrain in brains.OrderByDescending(x => GetDistanceToCamera(x.AIBrain)))
// 			{
// 				DrawActivity(tbrain);
// 			}
// 		}
//
// 		public static Vector2 WorldToScreenPos(Vector3 pos)
// 		{
// 			Vector3 spos = Camera.main.WorldToScreenPoint(pos);
// 			return new Vector2(spos.x, Screen.height - spos.y);
// 		}
//
// 		public static float GetDistanceToCamera(UAIBrain aiBrain)
// 		{
// 			return Vector3.Distance(Camera.main.transform.position, aiBrain.GetContext().agent.gameObject.transform.position);
// 		}
//
// 		public static void ClampRectInsideOther(ref Rect rect, in Rect other)
// 		{
// 			var pos = rect.position;
//
// 			if (pos.y < 0)
// 			{
// 				pos.y = 0;
// 			}
// 			else if (pos.y + rect.height > other.height)
// 			{
// 				pos.y = other.height - rect.height;
// 			}
//
// 			if (pos.x < 0)
// 			{
// 				pos.x = 0;
// 			}
// 			else if (pos.x + rect.width > other.width)
// 			{
// 				pos.x = other.width - rect.width;
// 			}
// 			rect.position = pos;
// 		}
//
// 		// 
// 		internal static void DrawActivity(in UAIManager.TrackedBrain tbrain)
// 		{
// 			var brain = tbrain.AIBrain;
//
// 			var wpos = brain.GetContext().agent.gameObject.transform.position;
//
// 			Vector2 spos = WorldToScreenPos(wpos);
//
// 			var timerPadding = 2f;
// 			
// 			int bucketCount = brain.GetBucketCount();
// 			int actionCount = brain.GetCurrentBucketActionCount();
//
// 			var currentActionBucketID = brain.GetCurrentActionBucketID();
//
// 			// this likely means an action is still finishing up
// 			if (brain.IsValidBucketID(currentActionBucketID) && currentActionBucketID != brain.GetCurrentBucketID())
// 			{
// 				actionCount++;
// 			}
//
// 			// 
// 			float longestWidth = UAIDebugStyles.LabelStyle.CalcSize(new GUIContent(GetLongestActionName(brain))).x;
//
// 			// 
// 			float rowWidth = UAIDebugStyles.TXT_PADDING * 2;
//
// 			// icon column
// 			rowWidth += UAIDebugStyles.ActionRowHeight + UAIDebugStyles.TXT_PADDING;
//
// 			// action name
// 			rowWidth += UAIDebugStyles.LabelStyle.CalcSize(new GUIContent(GetLongestActionName(brain))).x;
// 			rowWidth += 50; // extra spacing
// 	
// 			// cooldown column
// 			rowWidth += 50 + UAIDebugStyles.TXT_PADDING;
//
// 			// score column
// 			rowWidth += UAIDebugStyles.MaxScoreWidth;
//
// 			// 
// 			float totalHeight =
// 			UAIDebugStyles.ActionRowHeight * actionCount
// 			+ UAIDebugStyles.BucketRowHeight * bucketCount;
// 			totalHeight += UAIDebugStyles.TIMER_HEIGHT * 2;
//
// 			totalHeight += timerPadding;
//
// 			Rect fullRect = new Rect(0, 0, rowWidth, totalHeight);
//
// 			fullRect.position = spos;
//
// 			var screenRect = new Rect(0, 0, Screen.width, Screen.height);
//
// 			ClampRectInsideOther(ref fullRect, screenRect);
//
// 			Rect outerRect = fullRect;
// 			outerRect.Resize(1);
//
// 			Rect outerRect2 = outerRect;
// 			outerRect2.Resize(1);
//
// 			DrawRect(outerRect2, Color.black);
// 			DrawRect(outerRect, Color.white);
// 			DrawRect(fullRect, Color.black);
//
// 			var timerHeader = fullRect.SliceTop(UAIDebugStyles.TIMER_HEIGHT * 2);
// 			var lTimerRect = timerHeader.SliceLeft(timerHeader.height);
// 			var rTimerRect = timerHeader.SliceRight(timerHeader.height);
//
// 			DrawSelectorIcon(lTimerRect, brain.GetCurrentBucketSelector());
// 			DrawSelectorIcon(rTimerRect, brain.GetCurrentActionSelector());
// 			
// 			DrawTimer(timerHeader.SliceTop(UAIDebugStyles.TIMER_HEIGHT), brain.GetBucketScoringProgress());
// 			DrawTimer(timerHeader.SliceTop(UAIDebugStyles.TIMER_HEIGHT), brain.GetActionScoringProgress());
// 			fullRect.SliceTop(timerPadding);
// 			
// 			brain.ForEachBucket((in UAIBrain.BucketRecord bucket) =>
// 			{
// 				DrawBucketRow(ref fullRect, bucket, brain);
// 			});
// 		}
//
// 		private struct LegendItem
// 		{
// 			public string label;
// 			public Color color;
// 			public Texture2D icon;
//
// 			public LegendItem(string l, Color c)
// 			{
// 				label = l;
// 				color = c;
// 				icon = null;
// 			}
// 			
// 			public LegendItem(string l, Texture2D i)
// 			{
// 				label = l;
// 				color = Color.white;
// 				icon = i;
// 			}
// 		}
//
// 		private static readonly List<LegendItem> _legendItems = new()
// 		{
// 			new LegendItem("Active", UAIConstants.COLOR_ACTION_ACTIVE),
// 			new LegendItem("Cancelled", UAIConstants.COLOR_ACTION_CANCELLED),
// 			new LegendItem("Uncancellable", UAIConstants.COLOR_ACTION_UNCANCELLABLE),
// 			new LegendItem("Deactivating", UAIConstants.COLOR_ACTION_DEACTIVATING),
// 			new LegendItem("Finished", UAIConstants.COLOR_ACTION_FINISHED),
// 			new LegendItem("Selectable", UAIConstants.COLOR_SELECTABLE),
// 			new LegendItem("Muted", UAIConstants.COLOR_MUTED),
// 		};
//
// 		private static Dictionary<Type, Texture2D> _selectorIcons = new();
//
// 		private static void DrawSelectorIcon(in Rect rect, UAISelector selector)
// 		{
// 			var icon = GetSelectorIcon(selector);
// 			GUI.DrawTexture(rect.Resized(-2), icon);
// 		}
//
// 		private static Texture2D GetSelectorIcon(UAISelector selector)
// 		{
// 			if (!_selectorIcons.ContainsKey(selector.GetType()))
// 			{
// 				var path = selector.GetDebugIconPath();
// 				var icon = Resources.Load<Texture2D>(path);
// 				_selectorIcons[selector.GetType()] = icon;
//
// 				_legendItems.Add(new LegendItem(selector.GetDisplayName(), icon));
// 				
// 				return icon;
// 			}
// 			
// 			return _selectorIcons[selector.GetType()];
// 		}
//
// 		private static void DrawLegend()
// 		{
// 			var labelStyle = UAIDebugStyles.SmallLabelStyle;
// 			var labelheight = labelStyle.CalcSize(new GUIContent("")).y;
// 			var padding = UAIDebugStyles.TXT_PADDING_SM;
// 			
// 			var screenRect = new Rect(0, 0, Screen.width, Screen.height);
//
// 			var labelWidth = 200;
// 			var legendWidth = labelWidth + padding * 2;
// 			
// 			var rowHeight = labelheight + padding * 2;
//
// 			legendWidth += rowHeight + padding;
//
// 			var legendRect = new Rect(0, 0, legendWidth, _legendItems.Count * rowHeight);
// 			legendRect.position = new Vector2(screenRect.width - legendRect.width, 0);
//
// 			DrawRect(legendRect, Color.black);
// 			
// 			foreach(var item in _legendItems)
// 			{
// 				var itemRect = legendRect.SliceTop(rowHeight);
// 				var iconRect = itemRect.SliceLeft(itemRect.height);
// 				iconRect.Resize(-iconRect.width * 0.3f);
// 				itemRect.SliceLeft(padding);
// 				DrawLabel(itemRect, item.label, Color.white, labelStyle);
//
// 				if (item.icon)
// 				{
// 					GUI.DrawTexture(iconRect, item.icon);
// 				}
// 				else 
// 				{
// 					DrawRect(iconRect, item.color);
// 					
// 				}
// 			}
//
// 		}
//
// 		// 
// 		private static void DrawBucketRow(ref Rect rect, in UAIBrain.BucketRecord bucket, UAIBrain aiBrain)
// 		{
// 			
// 			DrawBucketHeader(rect.SliceTop(UAIDebugStyles.BucketRowHeight), bucket);
// 			
// 			var tempRect = rect;
//
// 			var currentActionBucketID = aiBrain.GetCurrentActionBucketID();
//
// 			if (aiBrain.GetCurrentBucketID() == bucket.ID)
// 			{
// 				aiBrain.ForEachActionInBucket(bucket.ID, (in UAIBrain.ActionRecord action) =>
// 				{
// 					DrawActionRow(tempRect.SliceTop(UAIDebugStyles.ActionRowHeight), action, aiBrain);
// 				});
// 			}
// 			else if (currentActionBucketID == bucket.ID)
// 			{
// 				ref readonly UAIBrain.ActionRecord action = ref aiBrain.GetCurrentAction();
// 				DrawActionRow(tempRect.SliceTop(UAIDebugStyles.ActionRowHeight), action, aiBrain);
// 			}
// 			rect = tempRect;
// 		}
//
// 		private static void DrawBucketHeader(in Rect rect, in UAIBrain.BucketRecord bucket)
// 		{
// 			var color = UAIConstants.COLOR_SELECTABLE;
//
// 			if (Mathf.Approximately(bucket.score, 0f))
// 			{
// 				color = UAIConstants.COLOR_MUTED;
// 			}
// 			
// 			DrawRect(rect, Color.black);
// 			DrawRect(rect.Resized(-1), color);
// 	
// 			var headerInnerRect = rect;
// 			headerInnerRect.Resize(-UAIDebugStyles.TXT_PADDING - 1);
// 			DrawLabel(headerInnerRect, bucket.name, Color.black, UAIDebugStyles.LargeLabelStyle);
// 			var scoreLabel = new GUIContent(bucket.score.ToString("0.0000"));
// 			var scoreSize = UAIDebugStyles.LargeLabelStyle.CalcSize(scoreLabel);
// 			var scoreRect = headerInnerRect;
// 			scoreRect.position += new Vector2(scoreRect.width - scoreSize.x, 0);
// 			DrawLabel(scoreRect, scoreLabel.text, Color.black, UAIDebugStyles.LargeLabelStyle);
// 		}
//
// 		// 
// 		private static void DrawTimer(in Rect rect, float progress)
// 		{
// 			DrawRect(rect, Color.black);
// 			var innerRect = rect;
// 			innerRect.Resize(-2f);
// 			innerRect.height += 1;
// 			innerRect.width *= progress;
// 			var color = Color.Lerp(Color.white * 0.3f, Color.white, progress);
// 			DrawRect(innerRect, color);
// 		}
//
// 		// 
// 		private static void DrawActionRow(Rect rect, in UAIBrain.ActionRecord action, UAIBrain aiBrain)
// 		{
// 			DrawRect(rect, Color.black);
//
// 			rect.Resize(-UAIDebugStyles.TXT_PADDING);
//
// 			var iconRect = rect.SliceLeft(rect.height);
// 			iconRect.Resize(-iconRect.width * 0.2f);
//
// 			rect.SliceLeft(UAIDebugStyles.TXT_PADDING);
//
// 			Color actionColor = GetActionColor(action, aiBrain);
//
// 			DrawRect(iconRect, GetActionColor(action, aiBrain));
//
// 			var lrect = rect;
// 			lrect.height = UAIDebugStyles.LabelHeight;
// 			lrect.center = rect.center;
//
// 			var textColor = Color.white;
// 			var onCooldown = action.OnCooldown();
//
// 			lrect.SliceLeft(UAIDebugStyles.TXT_PADDING);
// 			lrect.SliceRight(UAIDebugStyles.TXT_PADDING);
// 			var wCol = lrect.SliceRight(UAIDebugStyles.MaxScoreWidth);
// 			lrect.SliceRight(UAIDebugStyles.TXT_PADDING);
//
// 			var cdCol = lrect.SliceRight(50);
// 			lrect.SliceRight(UAIDebugStyles.TXT_PADDING);
//
// 			GUI.Label(lrect, action.template.Name, UAIDebugStyles.LabelStyle);
// 			GUI.Label(wCol, action.score.ToString("0.00000"), UAIDebugStyles.ScoreLabelStyle);
//
// 			if (onCooldown)
// 			{
// 				var remainder = action.cooldownEnd - Time.time;
// 				int val = remainder < 1 ? (int)(remainder * 1000) : (int)remainder;
// 				var timeLabel = val + (remainder < 1 ? "ms" : "s");
// 				GUI.Label(cdCol, timeLabel, UAIDebugStyles.CountdownLabelStyle);
// 			}
// 		}
// 		
// 		// 
// 		internal static string GetLongestActionName(UAIBrain aiBrain)
// 		{
// 			var longest = "";
// 			foreach (var action in aiBrain._actionRecords)
// 			{
// 				if (action.template.Name.Length > longest.Length)
// 				{
// 					longest = action.template.Name;
// 				}
// 			}
// 			return longest;
// 		}
// 		
// 		// 
// 		private static Color GetActionColor(in UAIBrain.ActionRecord action, UAIBrain aiBrain)
// 		{
// 			if (aiBrain.GetCurrentActionID() == action.actionID)
// 			{
// 				if (action.deactivating)
// 				{
// 					return UAIConstants.COLOR_ACTION_DEACTIVATING;
// 				}
//
// 				if (!action.cancellable)
// 				{
// 					return UAIConstants.COLOR_ACTION_UNCANCELLABLE;
// 				}
// 				
// 				return UAIConstants.COLOR_ACTION_ACTIVE;
// 			}
// 			else if (action.OnCooldown())
// 			{
// 				if (action.cancelled)
// 				{
// 					return UAIConstants.COLOR_ACTION_CANCELLED;
// 				}
// 				return UAIConstants.COLOR_ACTION_FINISHED;
// 			}
//
// 			if (Mathf.Approximately(action.score, 0))
// 			{
// 				return UAIConstants.COLOR_MUTED;
// 			}
// 			
// 			return UAIConstants.COLOR_SELECTABLE;
// 		}
// 		
// 		private static void DrawRect(in Rect rect, Color color)
// 		{
// 			var tColor = GUI.color;
// 			GUI.color = color;
// 			if (WhiteTex == null || !WhiteTex.Value)
// 			{
// 				WhiteTex = new(() =>
// 				{
// 					var t = new Texture2D(1,1);
// 					t.SetPixel(0,0, Color.white);
// 					t.Apply();
// 					return t;
// 				});
// 			}
// 			GUI.DrawTexture(rect, WhiteTex.Value);
// 			GUI.color = tColor;
// 		}
//
// 		private static void DrawLabel(in Rect rect, string text, Color color, GUIStyle style = null)
// 		{
// 			var tColor = GUI.color;
// 			GUI.color = color; 
// 			GUI.Label(rect, text, style ?? GUI.skin.label);
// 			GUI.color = tColor; 
// 		}
//
// 		private static Lazy<Texture2D> WhiteTex = null;
//
// 	}
// }
//
// namespace Smidgenomics.Unity.UAI
// {
// 	using UnityEngine;
// 	using System;
//
// 	internal static class UAIDebugStyles
// 	{
// 		public const float TXT_PADDING = 2f;
// 		public const float TXT_PADDING_SM = 1f;
// 		public const float TIMER_HEIGHT = 8;
// 		
// 		public static GUIStyle LabelStyle => _labelStyle.Value;
// 		
// 		// 
// 		private static readonly Lazy<GUIStyle> _labelStyle = new (() =>
// 		{
// 			var s = new GUIStyle(GUI.skin.label);
// 			s.alignment = TextAnchor.MiddleLeft;
// 			return s;
// 		});
//
// 		public static GUIStyle LargeLabelStyle => _largeLabelStyle.Value;
// 		
// 		// 
// 		private static readonly Lazy<GUIStyle> _largeLabelStyle = new (() =>
// 		{
// 			var style = new GUIStyle(_labelStyle.Value);
// 			style.fontSize = (int)(style.fontSize * 1.3f);
// 			style.fontStyle = FontStyle.Bold;
// 			return style;
// 		});
// 		
// 		public static GUIStyle ScoreLabelStyle => _scoreLabelStyle.Value;
//
// 		// 
// 		private static readonly Lazy<GUIStyle> _scoreLabelStyle = new (() =>
// 		{
// 			var style = new GUIStyle(GUI.skin.label);
// 			style.alignment = TextAnchor.MiddleRight;
// 			return style;
// 		});
// 		
// 		public static GUIStyle CountdownLabelStyle => _countdownLabelStyle.Value;
//
// 		private static readonly Lazy<GUIStyle> _countdownLabelStyle = new (() =>
// 		{
// 			var style = new GUIStyle(GUI.skin.label);
// 			style.alignment = TextAnchor.MiddleRight;
// 			return style;
// 		});
// 		
// 		public static GUIStyle SmallLabelStyle => _smallLabelStyle.Value;
//
// 		private static readonly Lazy<GUIStyle> _smallLabelStyle = new (() =>
// 		{
// 			var style = new GUIStyle(GUI.skin.label);
// 			style.alignment = TextAnchor.MiddleLeft;
// 			style.fontSize = (int)(style.fontSize * 0.7f);
// 			return style;
// 		});
//
// 		public static float LabelHeight => _labelHeight.Value;
//
// 		// 
// 		private static readonly Lazy<float> _labelHeight = new(() =>
// 		{
// 			return _labelStyle.Value.CalcSize(new GUIContent("a")).y;
// 		});
// 		
// 		public static float MaxScoreWidth => _maxScoreWidth.Value;
//
// 		// 
// 		private static readonly Lazy<float> _maxScoreWidth = new(() =>
// 		{
// 			return _labelStyle.Value.CalcSize(new GUIContent("0.000000")).x;
// 		});
//
// 		public static float LargeLabelHeight => _largeLabelHeight.Value;
// 		
// 		// 
// 		private static readonly Lazy<float> _largeLabelHeight = new(() =>
// 		{
// 			return _largeLabelStyle.Value.CalcSize(new GUIContent("a")).y;
// 		});
//
// 		public static float BucketRowHeight => _bucketRowHeight.Value;
//
// 		// 
// 		private static readonly Lazy<float> _bucketRowHeight = new(() =>
// 		{
// 			return _largeLabelHeight.Value + TXT_PADDING * 2;
// 		});
//
// 		public static float ActionRowHeight => _actionRowHeight.Value;
//
// 		// 
// 		private static readonly Lazy<float> _actionRowHeight = new(() =>
// 		{
// 			return _labelHeight.Value + TXT_PADDING * 2;
// 		});
// 	}
// }
