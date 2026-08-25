// smidgens @ github

#pragma warning disable 0414

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using UnityEngine;
	using UnityEditor;
	using Component = UnityEngine.Component;

	internal abstract class _UAIInspector : Editor
	{
		protected override bool ShouldHideOpenButton() => true;

		protected virtual bool ShouldShowTypeInfo() => true;
		protected virtual bool ShouldShowOverrides() => true;
		protected virtual bool ShouldGroupFields() => true;

		protected virtual bool ShowLabelProperty() => false;

		protected override void OnHeaderGUI()
		{
			var height = EditorGUIUtility.singleLineHeight * 2f;
			var pos = EditorGUILayout.GetControlRect(GUILayout.Height(height));
			var clickRect = pos;
			var icoRect = pos.SliceLeft(pos.height).Resized(-pos.height * 0.1f);
			UAIEditorGUI.DrawTypeIcon(icoRect, target as ScriptableObject, Color.white);
			var labelRect = pos.SliceTop(EditorGUIUtility.singleLineHeight);
			var typeRect = pos;
			GUI.Label(labelRect, target.name);
			GUI.Label(typeRect, ObjectNames.NicifyVariableName(target.GetType().Name), EditorStyles.miniLabel);

#if SM_DEV
			if (Event.current.type == EventType.ContextClick && clickRect.Contains(Event.current.mousePosition))
			{
				var m = new GenericMenu();
				m.AddItem(new GUIContent("Edit Script"), false, () =>
				{
					UAIEditorUtils.OpenScriptEditor(target as ScriptableObject);
				});
				m.ShowAsContext();
				Event.current.Use();
			}
#endif
			
			DrawDivider(Color.black * 0.5f, 0f);
		}

		public override void OnInspectorGUI()
		{
			if (_obsoleteInfo.Item1)
			{
				DrawObsoleteInfo();
			}

			if (ShouldShowTypeInfo())
			{
				DrawTypeInfo();
			}

			serializedObject.UpdateIfRequiredOrScript();
			
			foreach (var p in _extraProps)
			{
				EditorGUILayout.PropertyField(p);
			}

			if (ShouldGroupFields())
			{
				// EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			}

			bool inGroup = false;
			Assembly lastAssembly = null;
			foreach (var (type, prop) in _props)
			{
				if (ShouldGroupFields())
				{
					if (lastAssembly != type.Assembly)
					{
						if (inGroup)
						{
							EditorGUILayout.EndVertical();
						}
						lastAssembly = type.Assembly;
						EditorGUILayout.BeginVertical(EditorStyles.helpBox);
						inGroup = true;
					}
				}
				EditorGUILayout.PropertyField(prop);
			}

			if (ShouldGroupFields() && inGroup)
			{
				EditorGUILayout.EndVertical();
			}
			serializedObject.ApplyModifiedProperties();

			OnAfterFields();
		}

		private IReadOnlyList<SerializedProperty> _extraProps;
		private IReadOnlyList<(Type, SerializedProperty)> _props;
		private const float _SEP_H = 1f;
		private readonly List<string> _overriddenMethods = new();
		private GUIContent _foldoutLabel;
		private bool _showTypeInfo;
		private string _displayName;
		private (bool, string) _obsoleteInfo;
		private readonly GUIContent _overrideLabel = new("");

		protected virtual void OnEnable()
		{

			var extraProps = new List<SerializedProperty>();

			if (ShowLabelProperty())
			{
				extraProps.Add(serializedObject.FindProperty(nameof(UAIScriptableObject._label)));
			}

			_extraProps = extraProps;
			
			List<(Type,SerializedProperty)> l = new();
			foreach(var f in target.GetType().FindInspectorFields<Component>())
			{
				l.Add((f.DeclaringType, serializedObject.FindProperty(f.Name)));
			}
			_props = l;
			
			var tType = target.GetType();

			if (ShouldShowTypeInfo() && ShouldShowOverrides())
			{
				var bFlags = BindingFlags.Instance | BindingFlags.Public;
				foreach(var am in tType.BaseType!.GetMethods(bFlags))
				{
					var m = tType.GetMethod(am.Name, bFlags);
					if (m?.DeclaringType != am.DeclaringType)
					{
						_overriddenMethods.Add(am.Name.ToSentence());
					}
				}
				var tp = "";
				for (int i = 0; i < _overriddenMethods.Count; i++)
				{
					tp += $"- {_overriddenMethods[i]}";
					if (i < _overriddenMethods.Count - 1)
					{
						tp += "\n";
					}
				}
				_overrideLabel.text = $"{_overriddenMethods.Count} Overrides";
				_overrideLabel.tooltip = tp;
			}

			_displayName
			= FindDisplayName()
			?? target.GetType().FullName
			?? target.GetType().Name;
			var obsoleteAttr = target.GetType().GetCustomAttribute<ObsoleteAttribute>();
			_obsoleteInfo = (obsoleteAttr != null, obsoleteAttr?.Message);
			//
			OnInit();
		}

		private string FindDisplayName()
		{
			var attr = target.GetType().GetCustomAttribute<DisplayNameAttribute>();
			if (attr == null)
			{
				return null;
			}
			var dn = attr.DisplayName;
			var nameStart = dn.LastIndexOf('/');
			return nameStart >= 0 ? dn.Substring(nameStart + 1) : dn;
		}

		protected virtual void OnInit()
		{
			
		}

		protected virtual void OnAfterFields()
		{
			
		}

		private static readonly Lazy<Texture> _WARN_ICO
		= new(() => EditorGUIUtility.IconContent("Warning@2x")?.image);

		private void DrawObsoleteInfo()
		{
			var w = EditorGUILayout.GetControlRect(GUILayout.Height(1f)).width;
			
			var labelHeight = EditorStyles.label.CalcHeight(GUIContent.none, 20f);
			var messageHeight = EditorStyles.wordWrappedMiniLabel.CalcHeight(new GUIContent(_obsoleteInfo.Item2), w);

			var boxHeight = labelHeight + messageHeight;
			boxHeight += EditorStyles.helpBox.padding.top + EditorStyles.helpBox.padding.bottom;
			var pos = EditorGUILayout.GetControlRect(GUILayout.Height(boxHeight));

			var tColor = GUI.backgroundColor;
			GUI.backgroundColor = Color.yellow * 0.5f;
			GUI.Box(pos, GUIContent.none, EditorStyles.helpBox);
			GUI.Box(pos, GUIContent.none, GUI.skin.box);
			GUI.backgroundColor = tColor;

			var inner = pos.Padded(EditorStyles.helpBox.padding);
			
			var iconRect = inner.SliceLeft(inner.height);
			iconRect = iconRect.Resized(-iconRect.height * 0.1f);
			
			GUI.DrawTexture(iconRect, _WARN_ICO.Value);
			
			var titleRect = inner.SliceTop(labelHeight);
			var textRect = inner;
			GUI.Label(titleRect, "Obsolete");
			GUI.Label(textRect, _obsoleteInfo.Item2, EditorStyles.wordWrappedMiniLabel);
			
		}

		private void DrawTypeInfo()
		{
			var miniLabelHeight = EditorStyles.miniLabel.CalcHeight(GUIContent.none, 20f);
			var boldLabelHeight = EditorStyles.miniBoldLabel.CalcHeight(GUIContent.none, 20f);

			var typeBoxHeight = boldLabelHeight + miniLabelHeight;
			typeBoxHeight += EditorStyles.helpBox.padding.top + EditorStyles.helpBox.padding.bottom;

			var typeBoxRect = EditorGUILayout.GetControlRect(GUILayout.Height(typeBoxHeight));
			
			GUI.Box(typeBoxRect, GUIContent.none, EditorStyles.helpBox);
			GUI.Box(typeBoxRect, GUIContent.none, GUI.skin.box);

			var innerRect = typeBoxRect.Padded(EditorStyles.helpBox.padding);

			var iconRect = innerRect.SliceLeft(innerRect.height);
			iconRect = iconRect.Resized(-iconRect.height * 0.1f);

			var overrideWidth = EditorStyles.miniBoldLabel.CalcSize(_overrideLabel).x;

			var overrideRect = innerRect;
			overrideRect.position += new Vector2(overrideRect.width - overrideWidth, 0f);
			overrideRect.height = miniLabelHeight;

			var nameRect = innerRect.SliceTop(boldLabelHeight);
			var asmRect = innerRect;
			UAIEditorGUI.DrawTypeIcon(iconRect, target as ScriptableObject);
			GUI.Label(nameRect, _displayName, EditorStyles.miniBoldLabel);
			GUI.Label(asmRect, target.GetType().Assembly.GetName().Name, EditorStyles.miniLabel);
			GUI.Label(overrideRect, _overrideLabel, EditorStyles.miniLabel);
		}
		


		protected void DrawDivider(float mTop = 4f, float mBottom = 4f)
		{
			var color = EditorGUIUtility.isProSkin
			? Color.white.Fade(0.1f) : Color.black.Fade(0.1f);
			DrawDivider(color, mTop, mBottom);
		}
		
		protected void DrawDivider(Color c, float mTop = 4f, float mBottom = 4f)
		{
			var h = mTop + mBottom + _SEP_H;
			var pos = EditorGUILayout.GetControlRect(GUILayout.Height(h));
			pos.SliceTop(mTop);
			var sepRect = pos.SliceTop(_SEP_H);
			pos.SliceTop(mBottom);
			EditorGUI.DrawRect(sepRect, c);
		}

	}
}

#endif