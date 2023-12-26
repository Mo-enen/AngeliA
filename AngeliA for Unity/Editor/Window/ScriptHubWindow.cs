using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using AngeliaFramework;


namespace AngeliaForUnity.Editor {
	using Debug = UnityEngine.Debug;
	public interface IScriptHubConfig {
		public struct SearchPattern {
			public string Pattern;
			public string Label;
			public bool ShowTag;
			public SearchPattern (string pattern, string label, bool showTag) {
				Pattern = pattern;
				Label = label;
				ShowTag = showTag;
			}
		}
		string Title { get; }
		string[] Paths { get; }
		string IgnoreFolders => ""; // Use '\n' to split
		string IgnoreFiles => ""; // Use '\n' to split
		SearchPattern[] SearchPatterns { get; }
		int Order { get; }
		int Column { get => 1; }
		string GetFileName (string name) => name;
		string GetFolderName (string name) => name;
	}





	public class ScriptHubWindow : EditorWindow, IHasCustomMenu {




		#region --- VAR ---


		// SUB
		private class Item {
			public string Name;
			public string Path;
			public Texture2D Icon;
			public bool IsFile;
			public bool InPackage;
			public Item (string name, Texture2D icon, string path, bool isFile) {
				Name = name;
				Path = path;
				Icon = icon;
				IsFile = isFile;
				InPackage = path.StartsWith("packages", System.StringComparison.OrdinalIgnoreCase);
			}
		}

		private class WindowStyle {
			public GUIContent SearchContent = EditorGUIUtility.IconContent("Search Icon");
			public GUIStyle SearchWindowComponentButtonStyle = new("AC ComponentButton") {
				fontSize = 13,
				alignment = TextAnchor.MiddleLeft,
				stretchHeight = true,
				fixedHeight = 0f,
			};
			public GUIStyle LabelStyle = new(GUI.skin.label) {
				wordWrap = false,
				alignment = TextAnchor.MiddleLeft,
			};
		}

		private class HubSearchProvider : ScriptableObject, ISearchWindowProvider {

			public readonly List<SearchTreeEntry> List = new();
			public readonly List<Item> Items = new();
			public delegate void ClickHandler (string path);
			public ClickHandler OnClick = null;

			public List<SearchTreeEntry> CreateSearchTree (SearchWindowContext context) => List;

			public bool OnSelectEntry (SearchTreeEntry SearchTreeEntry, SearchWindowContext context) {
				string path = Util.FixPath(((Item)SearchTreeEntry.userData).Path);
				OnClick?.Invoke(path);
				return true;
			}

		}

		private class HubSearchEntry : SearchTreeEntry {
			public string Title = "";
			public HubSearchEntry (GUIContent content) : base(content) { }
		}



		// Api
		public static ScriptHubWindow Current { get; private set; } = null;

		// Data
		private Item[][] Scripts = new Item[0][];
		private string[] Titles = new string[0];
		private int[] Columns = new int[0];
		private Float2 ScrollPos = default;
		private float ColumnWidth = 1f;
		private WindowStyle Style { get; set; } = null;
		private HubSearchProvider HubSearch = null;
		private readonly GUIContent SearchTitleContent = new();
		private int RequireBlink = 0;

		// Saving
		private static readonly EditorSavingBool FocusGameOnPlay = new("ScriptHub.FocusGameOnPlay", false);
		private static readonly EditorSavingBool FocusHubOnEdit = new("ScriptHub.FocusHubOnEdit", false);
		private static readonly EditorSavingBool UseSpaceHotkey = new("ScriptHub.UseSpaceHotkey", true);


		#endregion




		#region --- MSG ---


		[MenuItem("AngeliA/Script Hub", false, 24)]
		private static void OpenWindow () {
			var window = GetWindow<ScriptHubWindow>("Script Hub", true, typeof(SceneView));
			if (window != null) {
				var iconContent = EditorGUIUtility.IconContent("UnityEditor.HierarchyWindow");
				if (iconContent != null && iconContent.image != null) {
					window.titleContent = new GUIContent(
						"Script Hub", iconContent.image
					);
				}
			}
		}


		[InitializeOnLoadMethod]
		private static void Init () {
			EditorApplication.playModeStateChanged += (mode) => {
				if (mode == PlayModeStateChange.EnteredEditMode) {
					if (FocusHubOnEdit.Value) FocusWindowIfItsOpen<ScriptHubWindow>();
				}
				if (mode == PlayModeStateChange.EnteredPlayMode) {
					if (FocusGameOnPlay.Value) {
						EditorApplication.ExecuteMenuItem("Window/General/Game");
					}
				}
			};
		}


		[MenuItem("Window/Search/Script Hub Search _SPACE")]
		private static void SearchHotkey () {
			if (!UseSpaceHotkey.Value || EditorApplication.isPlaying) return;
			if (Current != null) {
				Current.SearchTitleContent.text = "Script Hub";
				Current.OpenSearchWindow();
			}
		}


		public void AddItemsToMenu (GenericMenu menu) {
			menu.AddItem(new GUIContent("Focus Game View on Play"), FocusGameOnPlay.Value, () => {
				FocusGameOnPlay.Value = !FocusGameOnPlay.Value;
			});
			menu.AddItem(new GUIContent("Focus Hub on Edit"), FocusHubOnEdit.Value, () => {
				FocusHubOnEdit.Value = !FocusHubOnEdit.Value;
			});
			menu.AddItem(new GUIContent("Use Hotkey Space for Search Window"), UseSpaceHotkey.Value, () => {
				UseSpaceHotkey.Value = !UseSpaceHotkey.Value;
			});


		}


		private void OnEnable () {
			var iconContent = EditorGUIUtility.IconContent("UnityEditor.HierarchyWindow");
			if (iconContent != null && iconContent.image != null) {
				titleContent = new GUIContent(
					"Script Hub", iconContent.image
				);
			}
			Current = this;
			RequireBlink = 0;
			ReloadAllScripts();
		}


		private void OnDestroy () {
			if (Current == this) Current = null;
		}


		private void OnGUI () {

			if (Event.current.type == EventType.Repaint && RequireBlink > 0) {
				RequireBlink = (RequireBlink - 1).Clamp(0, 32);
				Repaint();
				return;
			}

			Style ??= new();

			// Search
			//var oldB = GUI.backgroundColor;
			//GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
			//var sRect = new Rect(2f, 2f, 32f, 32f);
			//if (GUI.Button(sRect, GUIContent.none, GUI.skin.textField)) {
			//	SearchTitleContent.text = UseSpaceHotkey.Value ? "Hotkey: Space" : "";
			//	OpenSearchWindow();
			//}
			//GUI.backgroundColor = oldB;
			//GUI.Label(sRect.Shrink(3), Style.SearchContent, MGUI.CenteredLabel);
			//EditorGUIUtility.AddCursorRect(sRect, MouseCursor.Link);

			// Reload
			//var rRect = new Rect(0f, 34f, 64f, 22f);
			//if (GUI.Button(rRect, "Reload", EditorStyles.linkLabel)) {
			//	ReloadAllScripts();
			//	RequireBlink = 3;
			//}
			//EditorGUIUtility.AddCursorRect(rRect, MouseCursor.Link);

			// Content
			using var scroll = new GUILayout.ScrollViewScope(ScrollPos);
			using var _ = new GUILayout.VerticalScope();
			ScrollPos = scroll.scrollPosition;
			var rect = MGUI.Rect((int)EditorGUIUtility.currentViewWidth - 20, 1);
			if (Event.current.type == EventType.Repaint) {
				float allColumns = 0;
				foreach (var c in Columns) allColumns += c;
				ColumnWidth = rect.width / allColumns;
			}
			using var h = new GUILayout.HorizontalScope();
			MGUI.Rect(0, 1);
			const int H_GAP = 12;
			for (int i = 0; i < Scripts.Length; i++) {
				ScriptListGUI(Scripts[i], Titles[i], (ColumnWidth - H_GAP).Clamp(64f, 260f) * Columns[i], Columns[i]);
				MGUI.Space(H_GAP);
			}
			MGUI.Rect(0, 1);
			MGUI.CancelFocusOnClick(this);

		}


		private void ScriptListGUI (Item[] scripts, string title, float width, int column) {
			if (scripts == null) return;
			using var _ = new GUILayout.VerticalScope(GUIStyle.none, GUILayout.Width(width));
			MGUI.Space(2);
			EditorGUI.DropShadowLabel(MGUI.Rect(0, 24), title, MGUI.CenteredBoldLabel);
			MGUI.Space(2);
			string folderName = "";
			var assetIconColor = new Byte4(209, 136, 60, 128);
			for (int i = 0; i < scripts.Length;) {
				using (new GUILayout.HorizontalScope()) {
					for (int col = 0; col < column && i < scripts.Length; col++, i++) {
						var item = scripts[i];
						var name = item.Name;
						var icon = item.Icon;
						var path = item.Path;
						if (item.IsFile) {
							// File
							if (icon == null) icon = AssetPreview.GetMiniThumbnail(AssetDatabase.LoadAssetAtPath<Object>(path));
							if (item.Icon == null && icon != null) item.Icon = icon;
							var rect = MGUI.Rect((int)(width / column), 22);
							var iconRect = rect.Shrink(0, rect.width - rect.height, 0, 0);
							var itemRect = rect.Shrink(rect.height, 0, 0, 0);
							GUI.Label(rect, new GUIContent("", name), EditorStyles.toolbarButton);
							if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) {
								OnItemClick(path);
							}
							if (icon != null) GUI.DrawTexture(iconRect.Shrink(2).Fit((float)icon.width / icon.height), icon);
							GUI.Label(itemRect, name, Style.LabelStyle);
							if (!item.InPackage) {
								EditorGUI.DrawRect(new FRect(rect.x - 1, rect.y + 2, 1, rect.height - 4), assetIconColor);
							}
							EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
						} else if (i < scripts.Length - 1 && scripts[i + 1].IsFile) {
							// Folder
							folderName = item.Name;
							i++;
							break;
						}
					}
				}
				if (!string.IsNullOrEmpty(folderName)) {
					// Folder
					MGUI.Space(2);
					GUI.Label(MGUI.Rect(0, 12), folderName, MGUI.MiniGreyLabel);
					MGUI.Space(2);
					folderName = "";
				}
			}
			MGUI.Space(64);
		}


		#endregion




		#region --- API ---


		public void ReloadAllScripts () {

			// Get Roots
			var roots = new List<IScriptHubConfig>();
			foreach (var type in typeof(IScriptHubConfig).AllClassImplemented()) {
				if (System.Activator.CreateInstance(type) is IScriptHubConfig obj) {
					try {
						roots.Add(obj);
					} catch (System.Exception ex) { Debug.LogException(ex); }
				}
			}
			roots.Sort((a, b) => a.Order.CompareTo(b.Order));

			// Load All
			var searchItemCache = new List<List<Item>>();
			var scriptsList = new List<Item[]>();
			var titleList = new List<string>();
			var columnList = new List<int>();
			for (int i = 0; i < roots.Count; i++) {
				try {
					var obj = roots[i];
					// Ignores
					var ignoreFolder = new HashSet<string>();
					var ignoreFile = new HashSet<string>();
					var ignorePath = new HashSet<string>();
					var searchCache = new List<Item>();
					foreach (var ig in obj.IgnoreFolders.Split('\n')) ignoreFolder.TryAdd(ig);
					foreach (var ig in obj.IgnoreFiles.Split('\n')) ignoreFile.TryAdd(ig);
					// Scripts
					var scripts = new List<Item>();
					foreach (var path in obj.Paths) {
						LoadScripts(scripts, path, ignoreFolder, ignoreFile, ignorePath, obj.SearchPatterns, obj.GetFileName, obj.GetFolderName, searchCache);
					}
					// Add
					if (scripts.Count(_item => _item.IsFile) > 0) {
						scriptsList.Add(scripts.ToArray());
						titleList.Add(obj.Title);
						columnList.Add(obj.Column);
						searchItemCache.Add(searchCache);
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
			Scripts = scriptsList.ToArray();
			Titles = titleList.ToArray();
			Columns = columnList.ToArray();

			// Search Init
			if (HubSearch == null) {
				HubSearch = CreateInstance<HubSearchProvider>();
			}
			HubSearch.OnClick = OnItemClick;
			HubSearch.List.Clear();
			HubSearch.Items.Clear();
			HubSearch.List.Add(new MySearchTreeGroupEntry(SearchTitleContent, 0));
			for (int i = 0; i < searchItemCache.Count; i++) {
				HubSearch.List.Add(new MySearchTreeGroupEntry(new GUIContent(Titles[i]), 1));
				var searchCache = searchItemCache[i];
				for (int j = 0; j < searchCache.Count; j++) {
					var item = searchCache[j];
					if (item.IsFile) {
						HubSearch.List.Add(new HubSearchEntry(new GUIContent(item.Name)) {
							Title = Titles[i],
							userData = item,
							level = 2,
						});
					}
				}
			}
			//for (int i = 0; i < Scripts.Length; i++) {
			//	HubSearch.List.Add(new MySearchTreeGroupEntry(new GUIContent(Titles[i]), 1));
			//	var scripts = Scripts[i];
			//	for (int j = 0; j < scripts.Length; j++) {
			//		var item = scripts[j];
			//		if (item.IsFile) {
			//			HubSearch.List.Add(new HubSearchEntry(new GUIContent(item.Name)) {
			//				Title = Titles[i],
			//				userData = item,
			//				level = 2,
			//			});
			//		}
			//	}
			//}

			//  Scripts
			void LoadScripts (
				List<Item> scripts, string root,
				HashSet<string> ignoreFolder,
				HashSet<string> ignoreFile,
				HashSet<string> ignorePath,
				IScriptHubConfig.SearchPattern[] searchPats,
				System.Func<string, string> getFileName,
				System.Func<string, string> getFolderName,
				List<Item> searchCache
			) {
				string fullRoot = Path.GetFullPath(root);

				foreach (var pt in searchPats) {
					if (!string.IsNullOrEmpty(pt.Label)) {
						scripts.Add(new Item(pt.Label, null, "", false));
					}
					LoadAll(fullRoot, 0, pt.ShowTag, false, pt.Pattern);
				}

				void LoadAll (string folderFullPath, int depth, bool addFolderTags, bool ignore, params string[] search) {

					string folderName = Util.GetNameWithoutExtension(folderFullPath);

					ignore = ignore || ignoreFolder.Contains(folderName) || ignorePath.Contains(Util.FixPath(folderFullPath));

					if (!ignore && addFolderTags) {
						if (scripts.Count == 0 || scripts[^1].IsFile) {
							scripts.Add(new Item(getFolderName(folderName), null, folderFullPath, false));
						} else {
							scripts[^1] = new Item(getFolderName(folderName), null, folderFullPath, false);
						}
					}

					var files = new List<FileInfo>(GetFilesIn(folderFullPath, true, search));
					files.Sort((a, b) => a.Name.CompareTo(b.Name));
					foreach (var file in files) {
						string path = $"{root}/{file.FullName[fullRoot.Length..]}";
						var item = new Item(
							Util.GetDisplayName(getFileName(Util.GetNameWithoutExtension(path))),
							null, path, true
						);
						searchCache.Add(item);
						if (ignore) continue;
						if (ignoreFile.Contains(Util.GetNameWithExtension(file.FullName))) continue;
						if (ignorePath.Contains(Util.FixPath(file.FullName))) continue;
						scripts.Add(item);
					}

					// Sub Folders
					var folders = new List<DirectoryInfo>(GetFoldersIn(folderFullPath, true));
					folders.Sort((a, b) => getFolderName(a.Name).CompareTo(getFolderName(b.Name)));
					foreach (var folder in folders) {
						LoadAll(folder.FullName, depth + 1, addFolderTags, ignore, search);
					}
				}
			}
		}


		#endregion



		#region --- LGC ---


		private static FileInfo[] GetFilesIn (string path, bool topOnly, params string[] searchPattern) {
			var allFiles = new List<FileInfo>();
			if (!Util.FolderExists(path)) return allFiles.ToArray();
			if (Util.PathIsFolder(path)) {
				var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
				if (searchPattern.Length == 0) {
					allFiles.AddRange(new DirectoryInfo(path).GetFiles("*", option));
				} else {
					for (int i = 0; i < searchPattern.Length; i++) {
						allFiles.AddRange(new DirectoryInfo(path).GetFiles(searchPattern[i], option));
					}
				}
			}
			return allFiles.ToArray();
		}


		private static DirectoryInfo[] GetFoldersIn (string path, bool topOnly, string searchPattern = "*") {
			var allDirs = new List<DirectoryInfo>();
			if (Util.FolderExists(path)) {
				allDirs.AddRange(new DirectoryInfo(path).GetDirectories(searchPattern, topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories));
			}
			return allDirs.ToArray();
		}


		// Search
		private void OpenSearchWindow () {
			var window = MySearchWindow.Open(
				new SearchWindowContext(default, 320, 480),
				HubSearch
			);
			if (window != null) {
				window.position = new FRect(
					position.x,
					position.y + 20,
					window.position.width,
					window.position.height
				);
				window.OnItemDraw += OnSearchWindowItemDraw;
				window.ItemHeight = 22f;
			}
		}


		private void OnItemClick (string path) {
			path = Util.FixPath(path);
			if (Event.current.button == 0) {
				string ex = Util.GetExtension(path);
				if (ex == ".cs" || ex == ".shader") {
					EditorApplication.delayCall += () => {
						EditorUtil.OpenFileWithCodeEditor(path);
					};
				} else {
					EditorApplication.delayCall += () => {
						EditorUtility.OpenWithDefaultApp(Path.GetFullPath(path));
					};
				}
			} else if (Event.current.button == 1) {
				Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
			}
		}


		private void OnSearchWindowItemDraw (FRect rect, SearchTreeEntry entry, bool focus) {
			Style ??= new();
			float iconSize = rect.height;
			// Draw Basic
			Style.SearchWindowComponentButtonStyle.Draw(
				rect.Shrink(iconSize + 4f, 0, 0, 0), entry.content,
				isHover: false, isActive: false,
				focus, focus
			);
			// Draw Icon
			var item = (Item)entry.userData;
			if (item.Icon != null) {
				const float SHRINK = 1f;
				GUI.DrawTexture(
					rect.Shrink(SHRINK + 2f, rect.width - iconSize + SHRINK, SHRINK, SHRINK).Fit((float)item.Icon.width / item.Icon.height),
					item.Icon
				);
			}
			// Draw Title
			if (entry is HubSearchEntry hubEntry) {
				GUI.Label(rect, hubEntry.Title, MGUI.RightGreyMiniLabel);
			}
		}


		#endregion




	}


	public class ScriptHubProcess : AssetPostprocessor {
		private readonly static List<string> Extensions = new();
		[InitializeOnLoadMethod]
		public static void Init () {
			var list = new List<string>();
			foreach (var type in typeof(IScriptHubConfig).AllClassImplemented()) {
				var config = System.Activator.CreateInstance(type) as IScriptHubConfig;
				foreach (var pat in config.SearchPatterns) {
					if (string.IsNullOrWhiteSpace(pat.Pattern)) continue;
					string ex = Util.GetExtension(pat.Pattern);
					if (ex == ".cs") continue;
					list.Add(ex);
				}
			}
			Extensions.Clear();
			Extensions.AddRange(list.Distinct());
		}
		public static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			if (ScriptHubWindow.Current == null) return;
			if (
				HasHubFile(importedAssets) ||
				HasHubFile(deletedAssets) ||
				HasHubFile(movedAssets) ||
				HasHubFile(movedFromAssetPaths)
			) {
				ScriptHubWindow.Current.ReloadAllScripts();
			}
		}
		private static bool HasHubFile (string[] paths) {
			foreach (var ex in Extensions) {
				foreach (var path in paths) {
					if (path.EndsWith(ex)) return true;
				}
			}
			return false;
		}
	}
}
