// smidgens @ github

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using System.Reflection;
	using UnityEditor.IMGUI.Controls;

	/// <summary>
	/// General-use dropdown window
	/// </summary>
	[Serializable]
	internal sealed class GenericDropdown<T> : AdvancedDropdown
	{
		public static GenericDropdown<T> Create(string title, T currentValue = default)
		{
			return new GenericDropdown<T>(title, currentValue, new AdvancedDropdownState());
		}

		private GenericDropdown(string title, T currentValue, AdvancedDropdownState state) : base(state)
		{
			_state = state;
			_rootNode = new Node
			{
				name = title
			};
			
			this.currentValue = currentValue;
		}

		public T currentValue;
		public Action<T> onSelected;

		public float MinHeight
		{
			get => _minHeight;
			set => _minHeight = Mathf.Max(value, 100f);
		}

		private float _minHeight = 200f;

		private AdvancedDropdownState _state;

		public void AddItem(string label, T value, Texture2D icon = null, bool enabled = true)
		{
			var newNode = _rootNode.AddChild(label);
			newNode.valueIndex = _values.Count;
			newNode.icon = icon;
			newNode.enabled = enabled;
			_values.Add((newNode, value));
		}

		public void AddSeparator(string path)
		{
			// TODO: use path
			_rootNode.AddChild(null);
		}

		public void Show(Rect pos, float maxHeight)
		{
			minimumSize = new Vector2(pos.x, MinHeight);
			var titleWidth = EditorStyles.boldLabel.CalcSize(new GUIContent(_rootNode.name)).x;
			Show(pos);
			var maxWidth = Mathf.Max(pos.width * 1.5f, Mathf.Max(200f, titleWidth));
			SetLastDropdownHeight(pos, maxHeight, maxWidth);
		}

		protected override void ItemSelected(AdvancedDropdownItem item)
		{
			if (item is GenericDropdownItem { valueIndex: > -1 } it)
			{
				var value = _values[it.valueIndex].Item2;
				if (!AreEqual(value, currentValue))
				{
					onSelected?.Invoke(value);
				}
			}
		}

		// cached delegate for setting selected index of item
		private Action<AdvancedDropdownItem, int> _setStateIndexFn;

		private void SetSelectedOfItem(AdvancedDropdownItem item, int newIndex)
		{
			// hack to work around unity obnoxiously not exposing
			// any way to change the initial dropdown state
			// (and it's been like that for years jfc)
			if (_setStateIndexFn == null)
			{
				var m = typeof(AdvancedDropdownState)
				.GetMethod("SetSelectedIndex", BindingFlags.Instance | BindingFlags.NonPublic);
				_setStateIndexFn = (Action<AdvancedDropdownItem, int>)m?.CreateDelegate(typeof(Action<AdvancedDropdownItem, int>), _state);
			}
			_setStateIndexFn?.Invoke(item, newIndex);
		}

		protected override AdvancedDropdownItem BuildRoot()
		{
			var currIndex = _values.FindIndex(v => AreEqual(v.Item2, currentValue));
			var root = _rootNode.GetDropdownItem(currIndex);
			SetSelectedOfItem(root, -1);
			return root;
		}

		private List<(Node, T)> _values = new();
		private Node _rootNode;

		private sealed class Node
		{
			public string name;
			public Texture2D icon;
			public int valueIndex = -1;
			public bool enabled = true;
			public Node parent { get; private set; }
			private readonly List<Node> _children = new();

			public GenericDropdownItem GetDropdownItem(int currIndex = -1, GenericDropdownItem iParent = null)
			{
				var item = new GenericDropdownItem(name, valueIndex, iParent)
				{
					enabled = enabled && (valueIndex != currIndex || valueIndex < 0),
					icon = icon
				};
				foreach (var c in _children)
				{
					if (c == null)
					{
						item.AddSeparator();
						continue;
					}
					item.AddChild(c.GetDropdownItem(currIndex, item));
				}
				return item;
			}

			public Node AddChild(string path)
			{
				if (path == null)
				{
					_children.Add(null);
					return null;
				}

				var sIndex = 0;
				var currentNode = this;
				while (sIndex < path.Length)
				{
					var nextLength = CountSegmentLength(path, sIndex);
					if (nextLength == 0)
					{
						nextLength = path.Length - sIndex;
					}
					var currName = path.AsSpan(sIndex, nextLength);
					currentNode = currentNode.FindOrCreateChild(currName);
					sIndex += nextLength + 1;
				}
				return currentNode;
			}

			private static int CountSegmentLength(string path, int startIndex, char stopToken = '/')
			{
				for (int i = startIndex; i < path.Length; i++)
				{
					if (path[i] == stopToken)
					{
						return i - startIndex;
					}
				}
				return 0;
			}

			private Node FindOrCreateChild(in ReadOnlySpan<char> cName)
			{
				var inStr = cName.ToString();
				foreach (var c in _children)
				{
					if (c == null)
					{
						continue;
					}
					if (c.name == inStr)
					{
						return c;
					}
				}
				_children.Add(new Node
				{
					name = inStr,
					parent = this,
				});
				return _children[^1];
			}
			
		}

		private sealed class GenericDropdownItem : AdvancedDropdownItem
		{
			public GenericDropdownItem(string name, int vIndex, GenericDropdownItem parent = null) : base(name)
			{
				valueIndex = vIndex;
				this.parent = parent;
			}
			public GenericDropdownItem parent { get; }
			public int valueIndex { get; }
		}

		// hardly the most robust comparison, should switch to comparable later
		private bool AreEqual(T v1, T v2)
		{
			var h1 = v1 == null ? 0 : v1.GetHashCode();
			var h2 = v2 == null ? 0 : v2.GetHashCode();
			return h1 == h2;
		}

		// hack to force height
		private static void SetLastDropdownHeight(Rect rect, float maxHeight, float maxWidth = 0f)
		{
			var window = EditorWindow.focusedWindow;

			if(!window || window.GetType().Name != "AdvancedDropdownWindow")
			{
				return;
			}

			var position = window.position;

			position.height = Mathf.Min(maxHeight, position.height);
			
			if (!Mathf.Approximately(0f, maxWidth))
			{
				position.width = Mathf.Min(maxWidth, position.width);
			}
			window.minSize = position.size;
			window.maxSize = position.size;
			window.position = position;
			window.ShowAsDropDown(GUIUtility.GUIToScreenRect(rect), position.size);
		}

		
	}
}

#endif