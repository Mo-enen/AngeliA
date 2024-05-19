global using Debug = AngeliA.Debug;
using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using AngeliA;


[assembly: ToolApplication]
[assembly: DisablePause]


namespace AngeliaEngine;


[RequireSpriteFromField]
[RequireLanguageFromField]
public class Engine {




	#region --- SUB ---


	private class ProjectData {
		public string Name;
		public string Path;
		public bool FolderExists;
		public long LastOpenTime;
	}


	private enum ProjectSortMode { Name, OpenTime, }


	#endregion




	#region --- VAR ---


	// Const
	private const int NOTIFY_DURATION = 120;
	private const int HUB_PANEL_WIDTH = 360;
	private const int WINDOW_BAR_WIDTH_FULL = 160;
	private const int WINDOW_BAR_WIDTH_NORMAL = 42;
	private static readonly SpriteCode UI_WINDOW_BG = "UI.MainBG";
	private static readonly SpriteCode PANEL_BG = "UI.HubPanel";
	private static readonly SpriteCode PROJECT_ICON = "UI.Project";
	private static readonly SpriteCode LABEL_PROJECTS = "Label.Projects";
	private static readonly LanguageCode BTN_CREATE = ("Hub.Create", "Create New Project");
	private static readonly LanguageCode BTN_ADD = ("Hub.Add", "Add Existing Project");
	private static readonly LanguageCode CREATE_PRO_TITLE = ("UI.CreateProjectTitle", "New Project");
	private static readonly LanguageCode ADD_PRO_TITLE = ("UI.AddProjectTitle", "Add Existing Project");
	private static readonly LanguageCode QUIT_MSG = ("UI.QuitMessage", "Quit editor?");
	private static readonly LanguageCode DELETE_PROJECT_MSG = ("UI.DeleteProjectMsg", "Remove project {0}?\nThis will NOT delete files in the disk.");
	private static readonly LanguageCode MENU_SORT_BY_NAME = ("Menu.SortProjectByName", "Sort by Name");
	private static readonly LanguageCode MENU_SORT_BY_TIME = ("Menu.SortProjectByTime", "Sort by Last Open Time");
	private static readonly LanguageCode NOTI_THEME_LOADED = ("Noti.ThemeLoaded", "Theme Loaded");
	private static readonly LanguageCode LOG_BUILD_UNKNOWN = ("Log.BuildError.Unknown", "Unknown error on building project in background. Error code:{0}");

	private static readonly LanguageCode LOG_ERROR_PROJECT_OBJECT_IS_NULL = ("Log.BuildError.ProjectObjectIsNull", "Build Error: Project object is Null");
	private static readonly LanguageCode LOG_ERROR_PROJECT_FOLDER_INVALID = ("Log.BuildError.ProjectFolderInvalid", "Build Error: Project folder path invalid");
	private static readonly LanguageCode LOG_ERROR_PUBLISH_DIR_INVALID = ("Log.BuildError.PublishDirInvalid", "Build Error: Publish folder path invalid");
	private static readonly LanguageCode LOG_ERROR_PROJECT_FOLDER_NOT_EXISTS = ("Log.BuildError.ProjectFolderNotExists", "Build Error: Project folder not exists");
	private static readonly LanguageCode LOG_ERROR_PRODUCT_NAME_INVALID = ("Log.BuildError.ProductNameInvalid", "Build Error: Product name invalid");
	private static readonly LanguageCode LOG_ERROR_DEV_NAME_INVALID = ("Log.BuildError.DevNameInvalid", "Build Error: Developer name invalid");
	private static readonly LanguageCode LOG_ERROR_RESULT_DLL_NOT_FOUND = ("Log.BuildError.ResultDllNotFound", "Build Error: Result dll file not found");
	private static readonly LanguageCode LOG_ERROR_RUNTIME_FILE_NOT_FOUND = ("Log.BuildError.RuntimeFileNotFound", "Build Error: Runtime file not found in the engine universe folder");
	private static readonly LanguageCode LOG_ERROR_UNIVERSE_FOLDER_NOT_FOUND = ("Log.BuildError.UniverseFolderNotFound", "Build Error: Universe folder not found");
	private static readonly LanguageCode LOG_ERROR_EXE_FOR_RUN_NOT_FOUND = ("Log.BuildError.ExeForRunNotFound", "Build Error: No Exe file to run");
	private static readonly LanguageCode LOG_ERROR_DOTNET_SDK_NOT_FOUND = ("Log.BuildError.DotnetSdkNotFound", "Build Error: Dotnet Sdk not found in the engine universe folder");
	private static readonly LanguageCode LOG_ERROR_ENTRY_PROJECT_NOT_FOUND = ("Log.BuildError.EntryProjectNotFound", "Build Error: Entry exe file for the project not found");
	private static readonly LanguageCode LOG_ERROR_ENTRY_RESULT_NOT_FOUND = ("Log.BuildError.EntryResultNotFound", "Build Error: Entry exe file result not found");

	private readonly GenericPopupUI GenericPopup = new() { Active = false };
	private readonly GenericDialogUI GenericDialog = new() { Active = false };
	private readonly FileBrowserUI FileBrowser = new() { Active = false };
	private readonly RiggedMapEditor RiggedMapEditor = new();
	private readonly PixelEditor PixelEditor = new();
	private readonly LanguageEditor LanguageEditor = new(ignoreRequirements: true);
	private readonly Console Console = new();
	private readonly ProjectEditor ProjectEditor = new();
	private readonly SettingWindow SettingWindow = new();
	private readonly EntityUI[] ALL_UI;

	// Data
	private static readonly Engine Instance = new();
	private Project CurrentProject = null;
	private ProjectSortMode ProjectSort = ProjectSortMode.OpenTime;
	private readonly GUIStyle TooltipStyle = new(GUI.Skin.SmallLabel);
	private readonly GUIStyle NotificationLabelStyle = new(GUI.Skin.AutoLabel) { Alignment = Alignment.BottomRight, };
	private readonly GUIStyle NotificationSubLabelStyle = new(GUI.Skin.AutoLabel) { Alignment = Alignment.BottomRight, };
	private readonly List<ProjectData> Projects = new();
	private readonly Sheet ThemeSheet = new(ignoreGroups: true, ignoreSpriteWithIgnoreTag: true);
	private readonly GUISkin ThemeSkin = new() { Name = "Built-in" };
	private readonly RiggedTransceiver Transceiver = new(EngineUtil.RiggedExePath);
	private IRect ToolLabelRect;
	private IRect LastHoveringToolLabelRect;
	private int HoveringTooltipDuration = 0;
	private int CurrentWindowIndex = 0;
	private int HubPanelScroll = 0;
	private int CurrentProjectMenuIndex = -1;
	private int NotificationStartFrame = int.MinValue;
	private int ThemeSheetIndex = -1;
	private bool NotificationFlash = false;
	private string ToolLabel = null;
	private string NotificationContent = null;
	private string NotificationSubContent = null;
	private int RigGameFailToStartCount = 0;
	private int RigGameFailToStartFrame = int.MinValue;
	private int RigMapEditorWindowIndex = -1;
	private long RequireBackgroundBuildDate = 0;
	private int RigLastCalledFrame = -1;
	private int WindowCount = 0;
	private int LastShowingGenericUIFrame = int.MinValue;
	private bool IgnoreInputForRig = false;

	// Saving
	private static readonly SavingString ProjectPaths = new("Engine.ProjectPaths", "");
	private static readonly SavingString LastOpenProject = new("Engine.LastOpenProject", "");
	private static readonly SavingBool Maximize = new("Engine.Maximize", true);
	private static readonly SavingBool FullsizeMenu = new("Engine.FullsizeMenu", true);
	private static readonly SavingInt WindowSizeX = new("Engine.WindowSizeX", 1024);
	private static readonly SavingInt WindowSizeY = new("Engine.WindowSizeY", 1024);
	private static readonly SavingInt WindowPositionX = new("Engine.WindowPosX", 128);
	private static readonly SavingInt WindowPositionY = new("Engine.WindowPosY", 128);
	private static readonly SavingInt LastOpenedWindowIndex = new("Engine.LastOpenedWindowIndex", 0);


	#endregion




	#region --- MSG ---


	// Init
	private Engine () {
		ALL_UI = new EntityUI[]{
			GenericPopup, GenericDialog, FileBrowser, // Generic
			RiggedMapEditor, PixelEditor, LanguageEditor, Console, ProjectEditor, SettingWindow, // Window UI
		};
	}


	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {

#if DEBUG
		// Grow Engine Version
		string obsoleteInfoPath = Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Obsolete Info.json");
		if (Util.FileExists(obsoleteInfoPath)) {
			var universe = UniverseSystem.BuiltInUniverse;
			universe.Info.EngineBuildVersion++;
			JsonUtil.SaveJsonToPath(universe.Info, universe.InfoPath, prettyPrint: true);
			Util.DeleteFile(obsoleteInfoPath);
		}
#endif

		Instance?.InitializeEngine();

	}


	private void InitializeEngine () {

		CurrentWindowIndex = LastOpenedWindowIndex.Value;

		// Projects
		Projects.Clear();
		var projectPaths = ProjectPaths.Value.Split(';');
		if (projectPaths != null) {
			foreach (var path in projectPaths) {
				if (string.IsNullOrWhiteSpace(path)) continue;
				Projects.Add(new ProjectData() {
					Path = path,
					Name = Util.GetNameWithoutExtension(path),
					FolderExists = Util.FolderExists(path),
					LastOpenTime = Util.GetFolderModifyDate(path),
				});
			}
		}
		RefreshProjectCache();
		SortProjects();
		if (
			EngineSetting.OpenLastProjectOnStart.Value &&
			Projects.Any(data => data.Path == LastOpenProject.Value)
		) {
			OpenProject(LastOpenProject.Value);
		}

		// Engine Window
		if (Maximize.Value) {
			Game.IsWindowMaximized = Maximize.Value;
		} else {
			Game.SetWindowPosition(WindowPositionX.Value, WindowPositionY.Value);
			Game.SetWindowSize(WindowSizeX.Value, WindowSizeY.Value);
		}
		Game.SetEventWaiting(false);

		// UI Window
		WindowCount = 0;
		ALL_UI.ForEach<WindowUI>((win, index) => {
			win.OnActivated();
			if (win is RiggedMapEditor) RigMapEditorWindowIndex = index;
			WindowCount++;
		});
		SettingWindow.Initialize(
			EngineSetting.BackgroundColor.Value.ToColorF(),
			EngineSetting.BackgroundColor.DefaultValue
		);
		RiggedMapEditor.Initialize(Transceiver);
		ProjectEditor.Initialize(Transceiver);

		// Theme
		ThemeSheetIndex = Renderer.AddAltSheet(ThemeSheet);

	}


	// Rebuild
	[OnProjectBuiltInBackground]
	internal static void OnProjectBuiltInBackground (int code) => Instance?.RiggedGameRebuild(code);


	private void RiggedGameRebuild (int code) {

		switch (code) {

			case 0:
				RigGameFailToStartCount = 0;
				RigGameFailToStartFrame = int.MinValue;
				Console.RemoveAllCompileErrors();
				break;

			default:
				Debug.LogError(string.Format(LOG_BUILD_UNKNOWN, code));
				break;

			case EngineUtil.ERROR_USER_CODE_COMPILE_ERROR:
				bool keywordFound = false;
				Console.BeginCompileError();
				try {
					while (EngineUtil.BackgroundBuildMessages.Count > 0) {
						string msg = EngineUtil.BackgroundBuildMessages.Dequeue();
						if (keywordFound) {
							if (!msg.StartsWith("Time Elapsed", System.StringComparison.OrdinalIgnoreCase)) {
								Debug.LogError(msg);
							}
						} else if (msg.StartsWith("Build FAILED", System.StringComparison.OrdinalIgnoreCase)) {
							keywordFound = true;
						}
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
				Console.EndCompileError();
				break;

			case EngineUtil.ERROR_PROJECT_OBJECT_IS_NULL:
				Debug.LogError(LOG_ERROR_PROJECT_OBJECT_IS_NULL);
				break;

			case EngineUtil.ERROR_PROJECT_FOLDER_INVALID:
				Debug.LogError(LOG_ERROR_PROJECT_FOLDER_INVALID);
				break;

			case EngineUtil.ERROR_PUBLISH_DIR_INVALID:
				Debug.LogError(LOG_ERROR_PUBLISH_DIR_INVALID);
				break;

			case EngineUtil.ERROR_PROJECT_FOLDER_NOT_EXISTS:
				Debug.LogError(LOG_ERROR_PROJECT_FOLDER_NOT_EXISTS);
				break;

			case EngineUtil.ERROR_PRODUCT_NAME_INVALID:
				Debug.LogError(LOG_ERROR_PRODUCT_NAME_INVALID);
				break;

			case EngineUtil.ERROR_DEV_NAME_INVALID:
				Debug.LogError(LOG_ERROR_DEV_NAME_INVALID);
				break;

			case EngineUtil.ERROR_RESULT_DLL_NOT_FOUND:
				Debug.LogError(LOG_ERROR_RESULT_DLL_NOT_FOUND);
				break;

			case EngineUtil.ERROR_RUNTIME_FILE_NOT_FOUND:
				Debug.LogError(LOG_ERROR_RUNTIME_FILE_NOT_FOUND);
				break;

			case EngineUtil.ERROR_UNIVERSE_FOLDER_NOT_FOUND:
				Debug.LogError(LOG_ERROR_UNIVERSE_FOLDER_NOT_FOUND);
				break;

			case EngineUtil.ERROR_EXE_FOR_RUN_NOT_FOUND:
				Debug.LogError(LOG_ERROR_EXE_FOR_RUN_NOT_FOUND);
				break;

			case EngineUtil.ERROR_DOTNET_SDK_NOT_FOUND:
				Debug.LogError(LOG_ERROR_DOTNET_SDK_NOT_FOUND);
				break;

			case EngineUtil.ERROR_ENTRY_PROJECT_NOT_FOUND:
				Debug.LogError(LOG_ERROR_ENTRY_PROJECT_NOT_FOUND);
				break;

			case EngineUtil.ERROR_ENTRY_RESULT_NOT_FOUND:
				Debug.LogError(LOG_ERROR_ENTRY_RESULT_NOT_FOUND);
				break;
		}

	}


	// Focus
	[OnGameFocused]
	internal static void OnGameFocused () {
		Instance?.CheckScriptChanged();
		Instance?.RefreshProjectCache();
	}


	// Quit
	[OnGameTryingToQuit]
	internal static bool OnGameTryingToQuit () {
		if (Instance == null) return true;
		if (Instance.CheckAnyEditorDirty()) {
			GenericDialogUI.SpawnDialog_Button(
				QUIT_MSG,
				BuiltInText.UI_SAVE, SaveAndQuit,
				BuiltInText.UI_DONT_SAVE, Game.QuitApplication,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
		} else {
			GenericDialogUI.SpawnDialog_Button(
				QUIT_MSG,
				BuiltInText.UI_QUIT, Game.QuitApplication,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
			GenericDialogUI.SetItemTint(GUI.Skin.DeleteTint);
		}
		return false;
		// Func
		static void SaveAndQuit () {
			foreach (var ui in Instance.ALL_UI) {
				if (ui is WindowUI window && window.IsDirty) {
					window.Save();
				}
			}
			Game.QuitApplication();
		}
	}


	[OnGameQuitting]
	internal static void OnGameQuitting () {
		if (Instance == null) return;
		var windowPos = Game.GetWindowPosition();
		Maximize.Value = Game.IsWindowMaximized;
		if (!Game.IsWindowMinimized) {
			WindowSizeX.Value = Game.ScreenWidth;
			WindowSizeY.Value = Game.ScreenHeight;
			WindowPositionX.Value = windowPos.x;
			WindowPositionY.Value = windowPos.y;
		}
		ProjectPaths.Value = Instance.Projects.JoinArray(p => p.Path, ';');
		Instance.ALL_UI.ForEach<WindowUI>(win => win.OnInactivated());
		Instance.Transceiver.Quit();
	}


#if DEBUG
	[OnGameQuitting(int.MaxValue)]
	internal static void CloseWindowsTerminal () {
		System.Diagnostics.Process.GetProcessesByName(
			"WindowsTerminal"
		).ToList().ForEach(item => item.CloseMainWindow());
	}
#endif


	// Update
	[OnGameUpdate]
	internal static void OnGameUpdate () => Instance.UpdateRiggedGame();


	private void UpdateRiggedGame () {

		if (
			CurrentProject == null ||
			EngineUtil.BuildingProjectInBackground ||
			!Transceiver.RigProcessRunning ||
			CurrentWindowIndex != RigMapEditorWindowIndex
		) return;

		if (Input.AnyMouseButtonDown) {
			IgnoreInputForRig = IgnoreInputForRig || !WindowUI.WindowRect.Contains(Input.MouseGlobalPosition);
		}
		if (!Input.AnyMouseButtonHolding) {
			IgnoreInputForRig = false;
		}

		Transceiver.Call(
			ignoreInput: IgnoreInputForRig || Game.PauselessFrame < LastShowingGenericUIFrame + 6,
			leftPadding: (FullsizeMenu.Value ? GUI.Unify(WINDOW_BAR_WIDTH_FULL) : GUI.Unify(WINDOW_BAR_WIDTH_NORMAL)) + GUI.Unify(8),
			requiringWindowIndex: (byte)(CurrentWindowIndex == RigMapEditorWindowIndex ? 0 : 0)
		);

		RigLastCalledFrame = Game.GlobalFrame;

	}


	[OnGameUpdateLater(-4096)]
	internal static void OnGameUpdateLater () => Instance.OnGUI();


	private void OnGUI () {

		GUI.Enable = true;
		GUI.ForceUnifyBasedOnMonitor = true;
		if (CurrentWindowIndex != RigMapEditorWindowIndex) {
			Sky.ForceSkyboxTint(GUI.Skin.Background);
		}

		using var _ = Scope.Sheet(ThemeSheet.Sprites.Count > 0 ? ThemeSheetIndex : -1);
		using var __ = Scope.GuiSkin(ThemeSkin);

		using (Scope.RendererLayerUI()) {
			OnGUI_Tooltip();
			OnGUI_Notify();
		}

		if (CurrentProject == null) {
			OnGUI_Hub();
		} else {
			OnGUI_Window();
			OnGUI_Hotkey();
		}
		OnGUI_RiggedGame();

	}


	// GUI Hub
	private void OnGUI_Hub () {

		var cameraRect = Renderer.CameraRect;
		int hubPanelWidth = GUI.Unify(HUB_PANEL_WIDTH);

		// --- Generic UI ---
		foreach (var ui in ALL_UI) {
			if (ui is WindowUI) {
				ui.Active = false;
				continue;
			}
			if (ui.Active) {
				ui.FirstUpdate();
				ui.BeforeUpdate();
				ui.Update();
				ui.LateUpdate();
				if (GUI.Enable && ui is not WindowUI) {
					GUI.Enable = false;
				}
			}
		}

		// --- File Browser ---
		if (FileBrowser.Active) {
			FileBrowser.Width = GUI.Unify(800);
			FileBrowser.Height = GUI.Unify(600);
		}

		using (Scope.RendererLayerUI()) {

			// --- BG ---
			int bodyBorder = GUI.Unify(6);
			Renderer.DrawSlice(
				UI_WINDOW_BG, cameraRect,
				bodyBorder, bodyBorder, bodyBorder, bodyBorder
			);

			// --- Panel ---
			{
				var panelRect = cameraRect.EdgeInside(Direction4.Left, hubPanelWidth);
				int itemPadding = GUI.Unify(8);

				var rect = new IRect(
					panelRect.x + itemPadding,
					panelRect.yMax - itemPadding * 2,
					panelRect.width - itemPadding * 2,
					GUI.Unify(42)
				);

				// Create
				rect.y -= rect.height + itemPadding;
				if (GUI.DarkButton(rect, BTN_CREATE)) {
					FileBrowserUI.SaveFolder(CREATE_PRO_TITLE, "New Project", CreateNewProjectAt);
				}

				// Add
				rect.y -= rect.height + itemPadding;
				if (GUI.DarkButton(rect, BTN_ADD)) {
					FileBrowserUI.OpenFolder(ADD_PRO_TITLE, AddExistsProjectAt);
				}
			}

			// --- Content ---

			int border = GUI.Unify(8);
			int padding = GUI.Unify(8);
			int scrollWidth = GUI.Unify(12);
			int itemHeight = GUI.Unify(52);
			int extendHeight = GUI.Unify(128);
			var contentRect = cameraRect.EdgeInside(Direction4.Right, cameraRect.width - hubPanelWidth).Shrink(
				padding, padding + scrollWidth, padding, padding
			);
			var projects = Projects;

			// BG
			Renderer.DrawSlice(PANEL_BG, contentRect, border, border, border, border, Color32.WHITE, z: 0);

			// Big Label
			if (Renderer.TryGetSprite(LABEL_PROJECTS, out var bigLabelSprite)) {
				Renderer.Draw(
					bigLabelSprite,
					contentRect.Shift(GUI.Unify(-24), GUI.Unify(48)).CornerInside(
						Alignment.BottomRight, GUI.Unify(256)
					).Fit(bigLabelSprite, 1000, 0),
					Color32.WHITE.WithNewA(4)
				);
			}

			// Project List
			using (var scroll = Scope.GUIScroll(
				contentRect, HubPanelScroll,
				0, Util.Max(0, projects.Count * itemHeight + extendHeight - contentRect.height))
			) {
				HubPanelScroll = scroll.ScrollPosition;

				var rect = contentRect.Shrink(border).EdgeInside(Direction4.Up, itemHeight);
				bool stepTint = false;

				for (int i = 0; i < projects.Count; i++) {
					string projectPath = projects[i].Path;
					bool folderExists = projects[i].FolderExists;
					var itemContentRect = rect.Shrink(padding);

					using var _ = Scope.GUIEnable(folderExists);

					// Step Tint
					if (stepTint) Renderer.DrawPixel(rect, Color32.WHITE_6);
					stepTint = !stepTint;

					// Red Highlight
					if (!folderExists && rect.MouseInside()) {
						Renderer.DrawPixel(rect, Color32.RED.WithNewA(32));
					}

					// Button
					if (GUI.Button(rect, 0, GUI.Skin.HighlightPixel)) {
						OpenProject(projectPath);
					}

					// Icon
					using (Scope.GUIContentColor(folderExists ? Color32.WHITE : Color32.WHITE_128)) {
						GUI.Icon(
							itemContentRect.EdgeInside(Direction4.Left, itemContentRect.height),
							PROJECT_ICON
						);
					}

					// Name
					GUI.SmallLabel(
						itemContentRect.Shrink(itemContentRect.height + padding, 0, itemContentRect.height / 2, 0),
						Util.GetNameWithoutExtension(projectPath)
					);

					// Path
					GUI.Label(
						itemContentRect.Shrink(itemContentRect.height + padding, 0, 0, itemContentRect.height / 2),
						projectPath,
						GUI.Skin.SmallGreyLabel
					);

					// Click
					if (rect.MouseInside()) {
						// Menu
						if (Input.MouseRightButtonDown) {
							Input.UseAllMouseKey();
							OpenHubItemPopup(i, folderExists);
						}

						// Delete Not Exists
						if (!folderExists) {
							if (Input.MouseLeftButtonDown) {
								CurrentProjectMenuIndex = i;
								DeleteProjectConfirm();
							}
							Cursor.SetCursorAsHand();
						}
					}

					rect.SlideDown();
				}

				if (Input.MouseRightButtonDown) {
					Input.UseAllMouseKey();
					OpenHubPanelPopup();
				}

			}

			// Scrollbar
			HubPanelScroll = GUI.ScrollBar(
				701635, cameraRect.EdgeInside(Direction4.Right, scrollWidth),
				HubPanelScroll, projects.Count * itemHeight + extendHeight, contentRect.height
			);

		}

	}


	// GUI Window
	private void OnGUI_Window () {

		if (CurrentProject == null) return;

		// Window
		int contentPadding = GUI.Unify(8);
		int barWidth = (FullsizeMenu.Value ? GUI.Unify(WINDOW_BAR_WIDTH_FULL) : GUI.Unify(WINDOW_BAR_WIDTH_NORMAL)) + contentPadding;
		var cameraRect = Renderer.CameraRect;
		var barRect = cameraRect.EdgeInside(Direction4.Left, barWidth);
		var mousePos = Input.MouseGlobalPosition;
		bool mousePress = Input.MouseLeftButtonDown;
		var rect = barRect.EdgeInside(Direction4.Up, GUI.Unify(42));
		bool interactable = true;

		foreach (var ui in ALL_UI) {
			if (ui is not WindowUI && ui.Active) {
				interactable = false;
				LastShowingGenericUIFrame = Game.PauselessFrame;
				break;
			}
		}

		using (Scope.GUIEnable(true, interactable))
		using (Scope.RendererLayerUI()) {

			bool menuButtonClicked = false;

			// Tab BG
			Renderer.DrawPixel(barRect, GUI.Skin.BackgroundDarkPanel);

			// Menu
			{
				var menuRect = rect.Shrink(contentPadding, contentPadding, contentPadding / 2, contentPadding / 2);

				// Menu Button
				if (GUI.BlankButton(rect, out _)) {
					menuButtonClicked = true;
				}

				// Menu Icon
				GUI.Icon(menuRect.EdgeInside(Direction4.Left, menuRect.height), BuiltInSprite.ICON_MENU);

				rect.y -= rect.height;
			}

			// Window Tabs
			SetCurrentWindowIndex(CurrentWindowIndex);

			int index = 0;
			for (int i = 0; i < ALL_UI.Length; i++) {

				if (ALL_UI[i] is not WindowUI window) continue;
				if (index >= WindowCount) break;

				bool selecting = index == CurrentWindowIndex;
				bool hovering = GUI.Enable && rect.Contains(mousePos);

				// Cursor
				if (!selecting && hovering) Cursor.SetCursorAsHand();

				if (selecting) {
					// Select Highlight
					Renderer.DrawPixel(rect, GUI.Skin.HighlightColorWeak);
				} else if (hovering) {
					// Hovering Highlight
					Renderer.DrawPixel(rect, GUI.Skin.HighlightColorWeak.WithNewA(128));
				}
				var contentRect = rect.Shrink(contentPadding, contentPadding, contentPadding / 2, contentPadding / 2);

				// Special Highlight
				if (window is Console console && console.HasCompileError) {
					Renderer.DrawPixel(
						rect,
						Color32.LerpUnclamped(GUI.Skin.ErrorTint.WithNewA(0), GUI.Skin.ErrorTint, Ease.InOutQuad(Game.GlobalFrame.PingPong(60) / 60f))
					);
				}

				// Icon
				int iconSize = contentRect.height;
				var iconRect = contentRect.EdgeInside(Direction4.Left, iconSize);
				Renderer.Draw(window.TypeID, iconRect);

				// Dirty Mark
				if (window.IsDirty) {
					int markSize = GUI.Unify(10);
					Renderer.Draw(
						BuiltInSprite.ICON_STAR,
						new IRect(iconRect.xMax - markSize / 2, iconRect.yMax - markSize / 2, markSize, markSize),
						Color32.ORANGE_BETTER
					);
				}

				// Label
				if (FullsizeMenu.Value) {
					GUI.SmallLabel(
						contentRect.Shrink(iconSize + contentPadding, 0, 0, 0),
						Language.Get(window.TypeID, window.DefaultName)
					);
				}

				// Click
				if (mousePress && hovering) {
					SetCurrentWindowIndex(index);
				}

				// Next
				rect.SlideDown();
				index++;
			}

			// Back to Hub
			if (FullsizeMenu.Value) {
				if (GUI.Button(
					barRect.EdgeInside(Direction4.Down, rect.height),
					BuiltInText.UI_BACK, GUI.Skin.SmallCenterLabelButton
				)) {
					TryCloseProject();
				}
			} else {
				if (GUI.Button(
					barRect.EdgeInside(Direction4.Down, rect.height),
					BuiltInSprite.ICON_BACK, GUI.Skin.IconButton
				)) {
					TryCloseProject();
				}
			}

			// Menu Cache
			if (menuButtonClicked) {
				FullsizeMenu.Value = !FullsizeMenu.Value;
			}

		}

		// Switch Active Window
		{
			WindowUI.ForceWindowRect(cameraRect.Shrink(barWidth, 0, 0, 0));
			int index = 0;
			foreach (var ui in ALL_UI) {
				if (ui is not WindowUI win) continue;
				bool active = index == CurrentWindowIndex;
				index++;
				if (active == win.Active) continue;
				win.Active = active;
				if (active) {
					win.OnActivated();
				} else {
					win.OnInactivated();
				}
			}
		}

		// Update UI
		bool oldE = GUI.Interactable;
		foreach (var ui in ALL_UI) {
			if (!ui.Active) continue;
			GUI.Interactable = interactable || ui is not WindowUI;
			ui.FirstUpdate();
		}
		foreach (var ui in ALL_UI) {
			if (!ui.Active) continue;
			GUI.Interactable = interactable || ui is not WindowUI;
			ui.BeforeUpdate();
		}
		foreach (var ui in ALL_UI) {
			if (!ui.Active) continue;
			GUI.Interactable = interactable || ui is not WindowUI;
			ui.Update();
		}
		foreach (var ui in ALL_UI) {
			if (!ui.Active) continue;
			GUI.Interactable = interactable || ui is not WindowUI;
			ui.LateUpdate();
		}
		GUI.Interactable = oldE;

		// Misc
		if (GenericDialogUI.ShowingDialog) {
			Game.IgnoreGizmos(1);
		}

		// Change Theme
		if (SettingWindow.RequireChangeThemePath != null) {
			string path = SettingWindow.RequireChangeThemePath;
			SettingWindow.RequireChangeThemePath = null;
			if (path != "" && Util.FileExists(path) && ThemeSheet.LoadFromDisk(path)) {
				ThemeSkin.Name = Util.GetDisplayName(Util.GetNameWithoutExtension(path));
				ThemeSkin.LoadColorFromSheet(ThemeSheet);
			} else {
				ThemeSheet.Clear();
				ThemeSkin.Name = "Built-in";
				ThemeSkin.LoadColorFromSkin(GUISkin.Default);
			}
			// Notify
			NotificationFlash = Game.GlobalFrame < NotificationStartFrame + NOTIFY_DURATION;
			NotificationStartFrame = Game.GlobalFrame;
			NotificationContent = NOTI_THEME_LOADED;
			NotificationSubContent = ThemeSkin.Name;
		}

		// Update Tooltip
		bool hoveringSameRect = false;
		foreach (var ui in ALL_UI) {
			if (ui is not WindowUI window || !window.Active) continue;
			string content = window.RequiringTooltipContent;
			if (content != null && EngineSetting.UseTooltip.Value) {
				ToolLabel = content;
				ToolLabelRect = window.RequiringTooltipRect;
				if (ToolLabelRect.MouseInside()) {
					LastHoveringToolLabelRect = ToolLabelRect;
					if (!hoveringSameRect && LastHoveringToolLabelRect == ToolLabelRect) {
						HoveringTooltipDuration++;
						hoveringSameRect = true;
					}
				}
			}
			window.RequiringTooltipContent = null;
		}
		if (!hoveringSameRect) HoveringTooltipDuration = 0;

		// Update Notify
		foreach (var ui in ALL_UI) {
			if (ui is not WindowUI window) continue;
			if (window.NotificationContent != null && EngineSetting.UseNotification.Value) {
				NotificationFlash = Game.GlobalFrame < NotificationStartFrame + NOTIFY_DURATION;
				NotificationStartFrame = Game.GlobalFrame;
				NotificationContent = window.NotificationContent;
				NotificationSubContent = window.NotificationSubContent;
			}
			window.NotificationContent = null;
		}

	}


	private void OnGUI_Hotkey () {

		// Clear Console
		if (Input.KeyboardDownWithCtrlAndShift(KeyboardKey.C)) {
			Console.Clear();
		}

		// Update Project Editor
		if (Input.KeyboardDownWithCtrl(KeyboardKey.R) || ProjectEditor.RequiringRebuildFrame == Game.GlobalFrame) {
			RequireBackgroundBuildDate = EngineUtil.LastBackgroundBuildModifyDate;
			if (RequireBackgroundBuildDate == 0) {
				RequireBackgroundBuildDate = EngineUtil.GetScriptModifyDate(CurrentProject);
			}
		}

		// Run Game
		if (Input.KeyboardDownWithCtrlAndShift(KeyboardKey.R)) {
			EngineUtil.RunAngeliaBuild(CurrentProject);
		}

	}


	private void OnGUI_Tooltip () {
		if (ToolLabel == null) return;
		if (!EngineSetting.UseTooltip.Value) {
			ToolLabel = null;
			return;
		}
		if (GenericPopupUI.ShowingPopup || GenericDialogUI.ShowingDialog || FileBrowser.Active) return;
		if (HoveringTooltipDuration < 60) return;
		var cameraRect = Renderer.CameraRect;
		bool leftSide = ToolLabelRect.CenterX() < cameraRect.CenterX();
		bool downSide = ToolLabelRect.CenterY() < cameraRect.CenterY();
		TooltipStyle.Alignment =
			leftSide && downSide ? Alignment.BottomLeft :
			leftSide && !downSide ? Alignment.TopLeft :
			!leftSide && downSide ? Alignment.BottomRight :
			Alignment.TopRight;
		GUI.BackgroundLabel(
			ToolLabelRect.EdgeOutside(Direction4.Down, GUI.Unify(24)).Shift(
				leftSide ? GUI.Unify(20) : GUI.Unify(-20),
				downSide ? GUI.Unify(10) : GUI.Unify(-10)
			),
			ToolLabel, Color32.BLACK,
			GUI.Unify(6), false, TooltipStyle
		);
		ToolLabel = null;
	}


	private void OnGUI_Notify () {
		if (EngineUtil.BuildingProjectInBackground) {

			// Building In Background
			int padding = GUI.Unify(6);
			int size = GUI.Unify(32);
			int x = WindowUI.WindowRect.xMax - size / 2 - padding;
			int y = WindowUI.WindowRect.yMin + size / 2 + padding;

			Renderer.Draw(
				BuiltInSprite.ICON_REFRESH,
				x, y, 500, 500, Game.GlobalFrame * 10,
				size, size, Color32.GREY_128
			);

		} else {

			if (!EngineSetting.UseNotification.Value || Game.GlobalFrame > NotificationStartFrame + NOTIFY_DURATION) return;

			int padding = GUI.Unify(2);
			int labelHeight = GUI.Unify(28);
			int subLabelHeight = GUI.Unify(20);
			var rect = WindowUI.WindowRect.CornerInside(Alignment.BottomRight, GUI.Unify(384), labelHeight + subLabelHeight);
			rect.y += padding * 2;
			rect.x -= padding * 2;
			int top = rect.yMax;
			bool hasSub = !string.IsNullOrEmpty(NotificationSubContent);

			// BG
			var bg = Renderer.DrawPixel(
				default,
				NotificationFlash ? Color32.Lerp(
					Color32.GREEN, Color32.BLACK,
					(Game.GlobalFrame - NotificationStartFrame) / (NOTIFY_DURATION / 4f)
				) : Color32.BLACK
			);

			// Main
			rect.y = top - labelHeight - (hasSub ? 0 : subLabelHeight);
			rect.height = labelHeight;
			GUI.Label(rect, NotificationContent, out var bound, NotificationLabelStyle);

			// Sub
			if (hasSub) {
				rect.y = top - labelHeight - subLabelHeight;
				rect.height = subLabelHeight;
				GUI.Label(rect, NotificationSubContent, out var subBound, NotificationSubLabelStyle);
				bound.xMin = Util.Min(bound.xMin, subBound.xMin);
				bound.yMin = Util.Min(bound.yMin, subBound.yMin);
			}

			// BG
			bg.SetRect(bound.Expand(GUI.Unify(6)));
		}

	}


	private void OnGUI_RiggedGame () {

		if (CurrentProject == null) {
			if (Transceiver.RigProcessRunning) {
				Transceiver.Abort();
			}
			return;
		}

		bool buildingProjectInBackground = EngineUtil.BuildingProjectInBackground;

		// Rebuild Check
		if (!buildingProjectInBackground && RequireBackgroundBuildDate > 0) {
			Transceiver.Abort();
			EngineUtil.BuildAngeliaProjectInBackground(CurrentProject, RequireBackgroundBuildDate);
			buildingProjectInBackground = true;
			RequireBackgroundBuildDate = 0;
		}

		// Abort when Building
		if (Transceiver.RigProcessRunning && buildingProjectInBackground) {
			Transceiver.Abort();
		}

		if (buildingProjectInBackground) return;

		// Update Rig
		if (Transceiver.RigProcessRunning) {
			// Rig Running
			if (RigLastCalledFrame == Game.GlobalFrame) {
				Transceiver.Respond(PixelEditor.SheetIndex);
			}
		} else if (
			(RigGameFailToStartCount < 16 && Game.GlobalFrame > RigGameFailToStartFrame + 30) ||
			Game.GlobalFrame > RigGameFailToStartFrame + 6000
		) {
			// No Rig Game Running
			int code = Transceiver.Start(CurrentProject.BuildPath, CurrentProject.BuildLibraryPath);
			if (code == 0) {
				// Start
				RigGameFailToStartCount = 0;
				RigGameFailToStartFrame = int.MinValue;
			} else {
				// Fail to Start
				RigGameFailToStartFrame = Game.GlobalFrame;
				RigGameFailToStartCount++;
			}
		}
	}


	#endregion




	#region --- LGC ---


	private void OpenHubItemPopup (int index, bool folderExists) {
		CurrentProjectMenuIndex = index;
		GenericPopupUI.BeginPopup();
		GenericPopupUI.AddItem(BuiltInText.UI_EXPLORE, OpenProjectInExplorer, enabled: folderExists);
		GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteProjectConfirm);
	}


	private void OpenHubPanelPopup () {
		GenericPopupUI.BeginPopup();
		GenericPopupUI.AddItem(
			MENU_SORT_BY_NAME,
			SortByName,
			@checked: ProjectSort == ProjectSortMode.Name
		);
		GenericPopupUI.AddItem(
			MENU_SORT_BY_TIME,
			SortByTime,
			@checked: ProjectSort == ProjectSortMode.OpenTime
		);
		static void SortByName () {
			Instance.ProjectSort = ProjectSortMode.Name;
			Instance.SortProjects();
		}
		static void SortByTime () {
			Instance.ProjectSort = ProjectSortMode.OpenTime;
			Instance.SortProjects();
		}
	}


	private void OpenProjectInExplorer () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= Projects.Count) return;
		string path = Projects[CurrentProjectMenuIndex].Path;
		if (Util.FolderExists(path)) {
			Game.OpenUrl(path);
		}
	}


	private void DeleteProjectConfirm () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= Projects.Count) return;
		string name = Util.GetNameWithoutExtension(Projects[CurrentProjectMenuIndex].Path);
		string msg = string.Format(DELETE_PROJECT_MSG, name);
		GenericDialogUI.SpawnDialog_Button(
			msg,
			BuiltInText.UI_DELETE, DeleteProject,
			BuiltInText.UI_CANCEL, Const.EmptyMethod
		);
		GenericDialogUI.SetItemTint(GUI.Skin.DeleteTint);
	}


	private void DeleteProject () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= Projects.Count) return;
		Projects.RemoveAt(CurrentProjectMenuIndex);
	}


	private bool CheckAnyEditorDirty () {
		foreach (var ui in ALL_UI) if (ui is WindowUI window && window.IsDirty) return true;
		return false;
	}


	private void SetCurrentWindowIndex (int index) {
		index = index.Clamp(0, WindowCount - 1);
		if (index == RigMapEditorWindowIndex && CurrentWindowIndex != index) {
			if (Transceiver.RigProcessRunning) Transceiver.RequireFocusInvoke();
		} else if (index != RigMapEditorWindowIndex && CurrentWindowIndex == index) {
			if (Transceiver.RigProcessRunning) Transceiver.RequireLostFocusInvoke();
			Stage.SetViewRectImmediately(
				new IRect(0, 0, Const.VIEW_RATIO * Game.DefaultViewHeight / 1000, Game.DefaultViewHeight),
				remapAllRenderingCells: true
			);
		}
		CurrentWindowIndex = index;
		LastOpenedWindowIndex.Value = index;
	}


	// Workflow
	private void OpenProject (string projectPath) {

		if (CurrentProject != null && projectPath == CurrentProject.ProjectPath) return;
		if (!Util.FolderExists(projectPath)) return;

		CurrentProject = Project.LoadProject(projectPath);
		LastOpenProject.Value = projectPath;
		foreach (var project in Projects) {
			if (project.Path == projectPath) {
				long time = Util.GetLongTime();
				project.LastOpenTime = time;
				Util.SetFolderModifyDate(projectPath, time);
				break;
			}
		}
		SortProjects();
		Game.SetWindowTitle($"Project - {Util.GetNameWithoutExtension(projectPath)}");

		// Windows
		LanguageEditor.SetLanguageRoot(AngePath.GetLanguageRoot(CurrentProject.UniversePath));
		PixelEditor.LoadSheetFromDisk(AngePath.GetSheetPath(CurrentProject.UniversePath));
		ProjectEditor.CurrentProject = CurrentProject;

		// Audio
		Game.SyncAudioPool(UniverseSystem.BuiltInUniverse.UniverseRoot, CurrentProject.UniversePath);

		// Script
		CheckScriptChanged();

		// Sync Engine Version
		if (UniverseSystem.BuiltInUniverse.Info.EngineBuildVersion != CurrentProject.Universe.Info.EngineBuildVersion) {
			CurrentProject.Universe.Info.EngineBuildVersion = UniverseSystem.BuiltInUniverse.Info.EngineBuildVersion;
			EngineUtil.SyncProjectWithEngine(CurrentProject);
			JsonUtil.SaveJsonToPath(CurrentProject.Universe.Info, CurrentProject.Universe.InfoPath, true);
		}

	}


	private void TryCloseProject () {
		if (CheckAnyEditorDirty()) {
			GenericDialogUI.SpawnDialog_Button(
				QUIT_MSG,
				BuiltInText.UI_SAVE, SaveAndClose,
				BuiltInText.UI_DONT_SAVE, Close,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
		} else {
			Close();
		}
		// Func
		static void SaveAndClose () {
			foreach (var ui in Instance.ALL_UI) {
				if (ui is WindowUI window && window.IsDirty) {
					window.Save();
				}
			}
			Close();
		}
		static void Close () {
			Instance.CurrentProject = null;
			foreach (var ui in Instance.ALL_UI) {
				ui.Active = false;
				if (ui is WindowUI) {
					ui.OnInactivated();
				}
			}
			Instance.LanguageEditor.SetLanguageRoot("");
			Instance.PixelEditor.LoadSheetFromDisk("");
			ProjectEditor.CurrentProject = null;
			Game.SetWindowTitle("AngeliA Engine");
		}
	}


	private void CreateNewProjectAt (string projectFolder) {

		if (string.IsNullOrWhiteSpace(projectFolder)) return;

		// Copy from Template
		Util.CopyFolder(EngineUtil.ProjectTemplatePath, projectFolder, true, true);

		// Change Info
		string infoPath = AngePath.GetUniverseInfoPath(AngePath.GetUniverseRoot(projectFolder));
		var info = new UniverseInfo {
			ProductName = Util.GetNameWithoutExtension(projectFolder),
			DeveloperName = System.Environment.UserName,
			ModifyDate = Util.GetLongTime(),
			MajorVersion = 0,
			MinorVersion = 0,
			PatchVersion = 0,
			EngineBuildVersion = UniverseSystem.BuiltInUniverse.Info.EngineBuildVersion,
		};
		JsonUtil.SaveJsonToPath(info, infoPath, prettyPrint: true);

		// Add Project into List
		AddExistsProjectAt(projectFolder);
	}


	private void AddExistsProjectAt (string path) {
		if (string.IsNullOrEmpty(path) || !Util.FolderExists(path)) return;
		if (!Projects.Any(data => data.Path == path)) {
			// Add to Path List
			long time = Util.GetLongTime();
			var item = new ProjectData() {
				Name = Util.GetNameWithoutExtension(path),
				Path = path,
				FolderExists = true,
				LastOpenTime = time,
			};
			Util.SetFolderModifyDate(path, time);
			Projects.Add(item);
			SortProjects();
		}
	}


	private void CheckScriptChanged () {
		long dllModifyDate = EngineUtil.GetBuildLibraryModifyDate(CurrentProject);
		long srcModifyDate = EngineUtil.GetScriptModifyDate(CurrentProject);
		if (srcModifyDate > dllModifyDate && srcModifyDate > EngineUtil.LastBackgroundBuildModifyDate) {
			RequireBackgroundBuildDate = srcModifyDate;
		} else {
			RequireBackgroundBuildDate = 0;
		}
	}


	private void RefreshProjectCache () {
		foreach (var project in Projects) {
			project.FolderExists = Util.FolderExists(project.Path);
		}
	}


	private void SortProjects () {
		switch (ProjectSort) {
			case ProjectSortMode.Name:
				Projects.Sort((a, b) => a.Name.CompareTo(b.Name));
				break;
			case ProjectSortMode.OpenTime:
				Projects.Sort((a, b) => b.LastOpenTime.CompareTo(a.LastOpenTime));
				break;
		}
	}


	#endregion




}