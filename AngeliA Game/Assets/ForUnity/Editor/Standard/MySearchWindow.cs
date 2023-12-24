using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using AngeliaFramework;


namespace AngeliaForUnity.Editor {


	[Serializable]
	public class MySearchTreeGroupEntry : SearchTreeGroupEntry {

		public int selectedIndex;
		public Float2 scroll;

		public MySearchTreeGroupEntry (GUIContent content, int level = 0) : base(content) {
			base.content = content;
			base.level = level;
		}
	}


	[InitializeOnLoad]
	public class MySearchWindow : EditorWindow {
		private class Styles {
			public GUIStyle header = "AC BoldHeader";

			public GUIStyle componentButton = "AC ComponentButton";

			public GUIStyle groupButton = new("AC GroupButton") {
				stretchHeight = true,
				fixedHeight = 0f,
			};

			public GUIStyle background = "grey_border";

			public GUIStyle rightArrow = "ArrowNavigationRight";

			public GUIStyle leftArrow = "ArrowNavigationLeft";
		}

		public delegate void DrawHandler (FRect rect, SearchTreeEntry entry, bool focus);
		public DrawHandler OnItemDraw { get; set; } = null;
		public float ItemHeight = 20f;

		private static Styles s_Styles;

		private static MySearchWindow s_FilterWindow;

		private static long s_LastClosedTime;

		private static bool s_DirtyList;

		private ScriptableObject m_Owner;

		private SearchWindowContext m_Context;

		private SearchTreeEntry[] m_Tree;

		private SearchTreeEntry[] m_SearchResultTree;

		private readonly List<MySearchTreeGroupEntry> m_SelectionStack = new();

		private float m_Anim = 1f;

		private int m_AnimTarget = 1;

		private long m_LastTime = 0L;

		private bool m_ScrollToSelected = false;

		private string m_DelayedSearch = null;

		private string m_Search = "";

		private ISearchWindowProvider provider => m_Owner as ISearchWindowProvider;

		private bool hasSearch => !string.IsNullOrEmpty(m_Search);

		private MySearchTreeGroupEntry activeParent {
			get {
				int num = m_SelectionStack.Count - 2 + m_AnimTarget;
				if (num < 0 || num >= m_SelectionStack.Count) {
					return null;
				}

				return m_SelectionStack[num];
			}
		}

		private SearchTreeEntry[] activeTree => hasSearch ? m_SearchResultTree : m_Tree;

		private SearchTreeEntry activeSearchTreeEntry {
			get {
				if (activeTree == null) {
					return null;
				}

				List<SearchTreeEntry> children = GetChildren(activeTree, activeParent);
				if (activeParent == null || activeParent.selectedIndex < 0 || activeParent.selectedIndex >= children.Count) {
					return null;
				}

				return children[activeParent.selectedIndex];
			}
		}

		private bool isAnimating => m_Anim != m_AnimTarget;

		static MySearchWindow () {
			s_FilterWindow = null;
			s_DirtyList = false;
			s_DirtyList = true;
		}

		private void OnEnable () {
			s_FilterWindow = this;
		}

		private void OnDisable () {
			s_LastClosedTime = DateTime.Now.Ticks / 10000;
			s_FilterWindow = null;
		}

		public static MySearchWindow Open<T> (SearchWindowContext context, T provider) where T : ScriptableObject, ISearchWindowProvider {
			UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(MySearchWindow));
			if (array.Length != 0) {
				try {
					((EditorWindow)array[0]).Close();
					return null;
				} catch (Exception) {
					s_FilterWindow = null;
				}
			}

			long num = DateTime.Now.Ticks / 10000;
			if (num >= s_LastClosedTime + 50) {
				if (s_FilterWindow == null) {
					s_FilterWindow = CreateInstance<MySearchWindow>();
					s_FilterWindow.hideFlags = HideFlags.HideAndDontSave;
				}

				s_FilterWindow.Init(context, provider);
				return s_FilterWindow;
			}

			return null;
		}

		private void Init (SearchWindowContext context, ScriptableObject provider) {
			m_Owner = provider;
			m_Context = context;
			float num = Math.Max(context.requestedWidth, 240f);
			float y = Math.Max(context.requestedHeight, 320f);
			FRect buttonRect = new(context.screenMousePosition.x - num / 2f, context.screenMousePosition.y - 16f, num, 1f);
			CreateSearchTree();
			ShowAsDropDown(buttonRect, new Float2(buttonRect.width, y));
			Focus();
			wantsMouseMove = true;
		}

		private void CreateSearchTree () {
			List<SearchTreeEntry> list = provider.CreateSearchTree(m_Context);
			if (list != null) {
				m_Tree = list.ToArray();
			} else {
				m_Tree = new SearchTreeEntry[0];
			}

			if (m_SelectionStack.Count == 0) {
				m_SelectionStack.Add(m_Tree[0] as MySearchTreeGroupEntry);
			} else {
				MySearchTreeGroupEntry searchTreeGroupEntry = m_Tree[0] as MySearchTreeGroupEntry;
				int level = 0;
				while (true) {
					MySearchTreeGroupEntry searchTreeGroupEntry2 = m_SelectionStack[level];
					m_SelectionStack[level] = searchTreeGroupEntry;
					m_SelectionStack[level].selectedIndex = searchTreeGroupEntry2.selectedIndex;
					m_SelectionStack[level].scroll = searchTreeGroupEntry2.scroll;
					level++;
					if (level == m_SelectionStack.Count) {
						break;
					}

					List<SearchTreeEntry> children = GetChildren(activeTree, searchTreeGroupEntry);
					SearchTreeEntry searchTreeEntry = children.FirstOrDefault((SearchTreeEntry c) => c.name == m_SelectionStack[level].name);
					if (searchTreeEntry != null && searchTreeEntry is MySearchTreeGroupEntry) {
						searchTreeGroupEntry = searchTreeEntry as MySearchTreeGroupEntry;
					} else {
						m_SelectionStack.RemoveRange(level, m_SelectionStack.Count - level);
					}
				}
			}

			s_DirtyList = false;
			RebuildSearch();
		}

		private void OnGUI () {

			s_Styles ??= new Styles();

			GUI.Label(new FRect(0f, 0f, position.width, position.height), GUIContent.none, s_Styles.background);
			if (s_DirtyList) {
				CreateSearchTree();
			}

			HandleKeyboard();
			GUILayout.Space(7f);
			EditorGUI.FocusTextInControl("ComponentSearch");
			FRect rect = GUILayoutUtility.GetRect(10f, 20f);
			rect.x += 8f;
			rect.width -= 16f;
			GUI.SetNextControlName("ComponentSearch");
			EditorGUI.BeginChangeCheck();

			string text = EditorGUI.TextField(rect, m_DelayedSearch ?? m_Search);

			if (EditorGUI.EndChangeCheck() && (text != m_Search || m_DelayedSearch != null)) {
				if (!isAnimating) {
					m_Search = m_DelayedSearch ?? text;
					RebuildSearch();
					m_DelayedSearch = null;
				} else {
					m_DelayedSearch = text;
				}
			}

			ListGUI(activeTree, m_Anim, GetSearchTreeEntryRelative(0), GetSearchTreeEntryRelative(-1));
			if (m_Anim < 1f) {
				ListGUI(activeTree, m_Anim + 1f, GetSearchTreeEntryRelative(-1), GetSearchTreeEntryRelative(-2));
			}

			if (isAnimating && Event.current.type == EventType.Repaint) {
				long ticks = DateTime.Now.Ticks;
				float num = (float)(ticks - m_LastTime) / 1E+07f;
				m_LastTime = ticks;
				m_Anim = Mathf.MoveTowards(m_Anim, m_AnimTarget, num * 4f);
				if (m_AnimTarget == 0 && m_Anim == 0f) {
					m_Anim = 1f;
					m_AnimTarget = 1;
					m_SelectionStack.RemoveAt(m_SelectionStack.Count - 1);
				}

				Repaint();
			}
		}

		private void HandleKeyboard () {
			Event current = Event.current;
			if (current.type != EventType.KeyDown) {
				return;
			}

			if (current.keyCode == KeyCode.DownArrow) {
				activeParent.selectedIndex++;
				activeParent.selectedIndex = Mathf.Min(activeParent.selectedIndex, GetChildren(activeTree, activeParent).Count - 1);
				m_ScrollToSelected = true;
				current.Use();
			}

			if (current.keyCode == KeyCode.UpArrow) {
				activeParent.selectedIndex--;
				activeParent.selectedIndex = Mathf.Max(activeParent.selectedIndex, 0);
				m_ScrollToSelected = true;
				current.Use();
			}

			if ((current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) && activeSearchTreeEntry != null) {
				SelectEntry(activeSearchTreeEntry, shouldInvokeCallback: true);
				current.Use();
			}

			if (!hasSearch) {
				if (current.keyCode == KeyCode.LeftArrow || current.keyCode == KeyCode.Backspace) {
					GoToParent();
					current.Use();
				}

				if (current.keyCode == KeyCode.RightArrow && activeSearchTreeEntry != null) {
					SelectEntry(activeSearchTreeEntry, shouldInvokeCallback: false);
					current.Use();
				}

				if (current.keyCode == KeyCode.Escape) {
					Close();
					current.Use();
				}
			}
		}

		private void RebuildSearch () {
			if (!hasSearch) {
				m_SearchResultTree = null;
				if (m_SelectionStack[^1].name == "Search") {
					m_SelectionStack.Clear();
					m_SelectionStack.Add(m_Tree[0] as MySearchTreeGroupEntry);
				}

				m_AnimTarget = 1;
				m_LastTime = DateTime.Now.Ticks;
				return;
			}

			string[] array = m_Search.ToLower().Split(' ');
			var list = new List<SearchTreeEntry>();
			var list2 = new List<SearchTreeEntry>();
			SearchTreeEntry[] tree = m_Tree;
			foreach (SearchTreeEntry searchTreeEntry in tree) {
				if (searchTreeEntry is MySearchTreeGroupEntry) {
					continue;
				}

				string text = searchTreeEntry.name.ToLower().Replace(" ", "");
				bool flag = true;
				bool flag2 = false;
				for (int j = 0; j < array.Length; j++) {
					string value = array[j];
					if (text.Contains(value)) {
						if (j == 0 && text.StartsWith(value)) {
							flag2 = true;
						}

						continue;
					}

					flag = false;
					break;
				}

				if (flag) {
					if (flag2) {
						list.Add(searchTreeEntry);
					} else {
						list2.Add(searchTreeEntry);
					}
				}
			}

			list.Sort();
			list2.Sort();
			var list3 = new List<SearchTreeEntry> {
				new MySearchTreeGroupEntry(new GUIContent("Search")),
			};
			list3.AddRange(list);
			list3.AddRange(list2);
			m_SearchResultTree = list3.ToArray();
			m_SelectionStack.Clear();
			m_SelectionStack.Add(m_SearchResultTree[0] as MySearchTreeGroupEntry);
			if (GetChildren(activeTree, activeParent).Count >= 1) {
				activeParent.selectedIndex = 0;
			} else {
				activeParent.selectedIndex = -1;
			}
		}

		private MySearchTreeGroupEntry GetSearchTreeEntryRelative (int rel) {
			int num = m_SelectionStack.Count + rel - 1;
			if (num < 0 || num >= m_SelectionStack.Count) {
				return null;
			}

			return m_SelectionStack[num];
		}

		private void GoToParent () {
			if (m_SelectionStack.Count > 1) {
				m_AnimTarget = 0;
				m_LastTime = DateTime.Now.Ticks;
			}
		}

		private void ListGUI (SearchTreeEntry[] tree, float anim, MySearchTreeGroupEntry parent, MySearchTreeGroupEntry grandParent) {
			anim = Mathf.Floor(anim) + Mathf.SmoothStep(0f, 1f, Mathf.Repeat(anim, 1f));
			FRect screenRect = position;
			screenRect.x = position.width * (1f - anim) + 1f;
			screenRect.y = 30f;
			screenRect.height -= 30f;
			screenRect.width -= 2f;
			GUILayout.BeginArea(screenRect);
			FRect rect = GUILayoutUtility.GetRect(10f, 25f);
			string text = parent.name;
			GUI.Label(rect, text, s_Styles.header);
			if (grandParent != null) {
				float num = (rect.height - s_Styles.leftArrow.fixedHeight) / 2f;
				var rect2 = new FRect(rect.x + s_Styles.leftArrow.margin.left, rect.y + num, s_Styles.leftArrow.fixedWidth, s_Styles.leftArrow.fixedHeight);
				if (Event.current.type == EventType.Repaint) {
					s_Styles.leftArrow.Draw(rect2, isHover: false, isActive: false, on: false, hasKeyboardFocus: false);
				}

				if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)) {
					GoToParent();
					Event.current.Use();
				}
			}

			ListGUI(tree, parent);
			GUILayout.EndArea();
		}

		private void SelectEntry (SearchTreeEntry e, bool shouldInvokeCallback) {
			if (e is MySearchTreeGroupEntry) {
				if (!hasSearch) {
					m_LastTime = DateTime.Now.Ticks;
					if (m_AnimTarget == 0) {
						m_AnimTarget = 1;
					} else if (m_Anim == 1f) {
						m_Anim = 0f;
						m_SelectionStack.Add(e as MySearchTreeGroupEntry);
					}
				}
			} else if (shouldInvokeCallback && provider.OnSelectEntry(e, m_Context)) {
				Close();
			}
		}

		private void ListGUI (SearchTreeEntry[] tree, MySearchTreeGroupEntry parent) {
			parent.scroll = GUILayout.BeginScrollView(parent.scroll);
			EditorGUIUtility.SetIconSize(new Float2(16f, 16f));
			List<SearchTreeEntry> children = GetChildren(tree, parent);
			FRect rect = default;
			for (int i = 0; i < children.Count; i++) {
				SearchTreeEntry searchTreeEntry = children[i];
				FRect rect2 = GUILayoutUtility.GetRect(16f, ItemHeight, GUILayout.ExpandWidth(expand: true));
				if ((Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDown) && parent.selectedIndex != i && rect2.Contains(Event.current.mousePosition)) {
					parent.selectedIndex = i;
					Repaint();
				}

				bool flag = false;
				if (i == parent.selectedIndex) {
					flag = true;
					rect = rect2;
				}

				if (Event.current.type == EventType.Repaint) {
					bool isGroup = searchTreeEntry is MySearchTreeGroupEntry;
					GUIStyle gUIStyle = ((searchTreeEntry is MySearchTreeGroupEntry) ? s_Styles.groupButton : s_Styles.componentButton);
					if (isGroup || OnItemDraw == null) {
						gUIStyle.Draw(rect2, searchTreeEntry.content, isHover: false, isActive: false, flag, flag);
					} else {
						OnItemDraw.Invoke(rect2, searchTreeEntry, flag);
					}
					if (isGroup) {
						float num = (rect2.height - s_Styles.rightArrow.fixedHeight) / 2f;
						var rect3 = new FRect(rect2.xMax - s_Styles.rightArrow.fixedWidth - (float)s_Styles.rightArrow.margin.right, rect2.y + num, s_Styles.rightArrow.fixedWidth, s_Styles.rightArrow.fixedHeight);
						s_Styles.rightArrow.Draw(rect3, isHover: false, isActive: false, on: false, hasKeyboardFocus: false);
					}
				}

				if (Event.current.type == EventType.MouseDown && rect2.Contains(Event.current.mousePosition)) {
					Event.current.Use();
					parent.selectedIndex = i;
					SelectEntry(searchTreeEntry, shouldInvokeCallback: true);
				}
			}

			EditorGUIUtility.SetIconSize(Float2.zero);
			GUILayout.EndScrollView();
			if (m_ScrollToSelected && Event.current.type == EventType.Repaint) {
				m_ScrollToSelected = false;
				FRect lastRect = GUILayoutUtility.GetLastRect();
				if (rect.yMax - lastRect.height > parent.scroll.y) {
					parent.scroll.y = rect.yMax - lastRect.height;
					Repaint();
				}

				if (rect.y < parent.scroll.y) {
					parent.scroll.y = rect.y;
					Repaint();
				}
			}
		}

		private List<SearchTreeEntry> GetChildren (SearchTreeEntry[] tree, SearchTreeEntry parent) {
			var list = new List<SearchTreeEntry>();
			int num = -1;
			int num2;
			for (num2 = 0; num2 < tree.Length; num2++) {
				if (tree[num2] == parent) {
					num = parent.level + 1;
					num2++;
					break;
				}
			}

			if (num == -1) {
				return list;
			}

			for (; num2 < tree.Length; num2++) {
				SearchTreeEntry searchTreeEntry = tree[num2];
				if (searchTreeEntry.level < num) {
					break;
				}

				if (searchTreeEntry.level <= num || hasSearch) {
					list.Add(searchTreeEntry);
				}
			}

			return list;
		}


	}
}