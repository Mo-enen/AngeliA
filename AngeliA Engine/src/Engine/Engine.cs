global using Debug = AngeliA.Debug;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliA;
using AngeliaRigged;

[assembly: ToolApplication]
[assembly: DisablePause]

namespace AngeliaEngine;

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
	private static readonly SpriteCode UI_ENGINE_BAR = "UI.EngineSideBar";
	private static readonly SpriteCode UI_ENGINE_BAR_BTN = "UI.EngineSideButton";
	private static readonly SpriteCode UI_ENGINE_BAR_BTN_HIGHLIGHT = "UI.EngineSideButtonHighlight";
	private static readonly SpriteCode UI_ENGINE_BAR_BTN_WARNING = "UI.EngineSideButtonWarning";
	private static readonly SpriteCode PANEL_BG = "UI.HubPanel";
	private static readonly SpriteCode PROJECT_ICON = "UI.Project";
	private static readonly SpriteCode LABEL_PROJECTS = "Label.Projects";
	private static readonly SpriteCode ICON_TAB_BACK = "Icon.MainTabBack";

	private static readonly LanguageCode BUILDING_HINT = ("UI.Rig.BuildingHint", "Recompiling");
	private static readonly LanguageCode BUILD_ERROR_HINT = ("UI.Rig.BuildError", "Error in game script :(\nAll errors must be fixed before the game can run");
	private static readonly LanguageCode RIG_FAIL_HINT = ("UI.Rig.NotRunning", "Rigged Game Not Running :(\nThis should not happen. Please contact the developer and report this problem.");
	private static readonly LanguageCode BTN_CREATE = ("Hub.Create", "Create New Project");
	private static readonly LanguageCode BTN_ADD = ("Hub.Add", "Add Existing Project");
	private static readonly LanguageCode CREATE_PRO_TITLE = ("UI.CreateProjectTitle", "New Project");
	private static readonly LanguageCode ADD_PRO_TITLE = ("UI.AddProjectTitle", "Add Existing Project");
	private static readonly LanguageCode QUIT_MSG = ("UI.QuitMessage", "Close engine window ?");
	private static readonly LanguageCode DELETE_PROJECT_MSG = ("UI.DeleteProjectMsg", "Remove project {0}?\nThis will NOT delete files in the disk.");
	private static readonly LanguageCode MENU_SORT_BY_NAME = ("Menu.SortProjectByName", "Sort by Name");
	private static readonly LanguageCode MENU_SORT_BY_TIME = ("Menu.SortProjectByTime", "Sort by Last Open Time");
	private static readonly LanguageCode NOTI_THEME_LOADED = ("Noti.ThemeLoaded", "Theme Loaded");
	private static readonly LanguageCode LOG_BUILD_UNKNOWN = ("Log.BuildError.Unknown", "Unknown error on building project in background. Error code:{0}");
	private static readonly LanguageCode FILE_DROP_MSG_PNG = ("UI.FileDropMsg.Png", "Import image {0} as:");
	private static readonly LanguageCode FILE_DROP_MSG_AUDIO = ("UI.FileDropMsg.Audio", "Import audio file {0} as:");
	private static readonly LanguageCode FILE_DROP_LABEL_ICON = ("UI.FileDropLabel.Icon", "Icon");
	private static readonly LanguageCode FILE_DROP_LABEL_ART = ("UI.FileDropLabel.Art", "Artwork");
	private static readonly LanguageCode FILE_DROP_LABEL_MUSIC = ("UI.FileDropLabel.Music", "Music");
	private static readonly LanguageCode FILE_DROP_LABEL_SOUND = ("UI.FileDropLabel.Sound", "Sound");
	private static readonly LanguageCode HINT_PUBLISHING = ("Hint.Publishing", "Publishing");

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
	private static readonly LanguageCode LOG_ERROR_CSPROJ_NOT_EXISTS = ("Log.BuildError.CsprojNotExists", "Csproj file not found");

	// Data
	private static Engine Instance = null;
	private readonly GUIStyle TooltipStyle = new(GUI.Skin.SmallLabel);
	private readonly GUIStyle NotificationLabelStyle = new(GUI.Skin.AutoLabel) { Alignment = Alignment.BottomRight, };
	private readonly GUIStyle NotificationSubLabelStyle = new(GUI.Skin.AutoLabel) { Alignment = Alignment.BottomRight, };
	private readonly GUIStyle RigGameHintStyle = new(GUI.Skin.SmallCenterMessage) { LineSpace = 14 };
	private readonly List<ProjectData> Projects = new();
	private readonly Sheet ThemeSheet = new(ignoreGroups: true, ignoreSpriteWithIgnoreTag: true);
	private readonly GUISkin ThemeSkin = new() { Name = "Built-in" };
	private readonly RigTransceiver Transceiver = new(EngineUtil.RiggedExePath);
	private EntityUI[] AllGenericUIs;
	private WindowUI[] AllWindows;
	private Project CurrentProject = null;
	private ProjectSortMode ProjectSort = ProjectSortMode.OpenTime;
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
	private int RigMapEditorWindowIndex = 0;
	private int ConsoleWindowIndex = 0;
	private long RequireBackgroundBuildDate = 0;
	private int LastNotInteractableFrame = int.MinValue;
	private bool IgnoreInputForRig = false;
	private bool CurrentWindowRequireRigGame = false;
	private int NoGameRunningFrameCount = 0;
	private int IgnoreFileDropFrame = int.MinValue;
	private string DroppingFilePath = "";

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
	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {
		var engine = new Engine {
			AllWindows = new WindowUI[]{
				new RiggedMapEditor(),
				new PixelEditor(),
				new LanguageEditor(),
				new Console(),
				new ProjectEditor(),
				new SettingWindow(EngineSetting.BackgroundColor.Value.ToColorF(), EngineSetting.BackgroundColor.DefaultValue),
			},
			AllGenericUIs = new EntityUI[] {
				new GenericPopupUI(),
				new GenericDialogUI(),
				new FileBrowserUI(),
			}
		};
		Instance = engine;
		engine.InitializeEngine();
	}
	private void InitializeEngine () {

#if DEBUG
		// Grow Engine Version
		string obsoleteInfoPath = Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Obsolete Info.json");
		if (Util.FileExists(obsoleteInfoPath)) {
			var universe = Universe.BuiltIn;
			universe.Info.EngineBuildVersion++;
			JsonUtil.SaveJsonToPath(universe.Info, universe.InfoPath, prettyPrint: true);
			Util.DeleteFile(obsoleteInfoPath);
		}
#endif

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
		Game.ProcedureAudioVolume = 1000;

		if (EngineSetting.LastMapEditorViewHeight.Value > 0) {
			Transceiver.SetStartViewPos(
				EngineSetting.LastMapEditorViewX.Value,
				EngineSetting.LastMapEditorViewY.Value,
				EngineSetting.LastMapEditorViewZ.Value,
				EngineSetting.LastMapEditorViewHeight.Value
			);
		}

		// UI Window
		for (int i = 0; i < AllWindows.Length; i++) {
			var win = AllWindows[i];
			win.OnActivated();
			if (win is RiggedMapEditor) RigMapEditorWindowIndex = i;
			if (win is Console) ConsoleWindowIndex = i;
		}

		SetCurrentWindowIndex(LastOpenedWindowIndex.Value, forceChange: true);
		ResetViewRect();

		// Theme
		ThemeSheetIndex = Renderer.AddAltSheet(ThemeSheet);

	}


	// Rebuild
	[OnProjectBuiltInBackground]
	internal static void RiggedGameRebuild (int code) {

		if (Instance == null) return;

		RiggedMapEditor.Instance.CleanDirty();
		PixelEditor.Instance.ResetCharacterNameList();
		if (Instance.CurrentProject != null) {
			Util.DeleteFile(Instance.CurrentProject.Universe.CharacterInfoPath);
		}

		switch (code) {

			case 0:
				Instance.RigGameFailToStartCount = 0;
				Instance.RigGameFailToStartFrame = int.MinValue;
				Console.Instance.RemoveAllCompileErrors();
				break;

			default:
				Debug.LogError(string.Format(LOG_BUILD_UNKNOWN, code));
				break;

			case EngineUtil.ERROR_USER_CODE_COMPILE_ERROR:
				Console.Instance.BeginCompileError();
				try {
					while (EngineUtil.BackgroundBuildMessages.Count > 0) {
						Debug.LogError(EngineUtil.BackgroundBuildMessages.Dequeue());
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
				Console.Instance.EndCompileError();
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

			case EngineUtil.ERROR_CSPROJ_NOT_EXISTS:
				Debug.LogError(LOG_ERROR_CSPROJ_NOT_EXISTS);
				break;
		}

	}


	// Misc
	[OnGameFocused]
	internal static void OnGameFocused () {
		Instance?.CheckScriptChanged();
		Instance?.RefreshProjectCache();
		Instance?.CheckResourceChanged();
		Instance?.CheckEngineResourceChanged();
		Instance?.CheckDialogChanged();
	}


	[OnFileDropped]
	internal static void OnFileDropped (string path) {

		var project = Instance.CurrentProject;
		if (project == null || Game.PauselessFrame <= Instance.IgnoreFileDropFrame) return;

		Instance.DroppingFilePath = path;

		string ex = Util.GetExtensionWithDot(path);
		switch (ex) {
			case ".ase":
			case ".aseprite":
				PixelEditor.ImportAtlasFromFile(path);
				Instance.IgnoreFileDropFrame = Game.PauselessFrame;
				break;
			case ".png":
				GenericDialogUI.SpawnDialog_Button(
					string.Format(FILE_DROP_MSG_PNG, Util.GetNameWithExtension(path)),
					FILE_DROP_LABEL_ICON, ImportForIcon,
					FILE_DROP_LABEL_ART, ImportForArtwork,
					BuiltInText.UI_CANCEL, Const.EmptyMethod
				);
				Instance.IgnoreFileDropFrame = Game.PauselessFrame;
				break;
			case ".ico":
				Util.CopyFile(path, project.IconPath);
				ProjectEditor.Instance.ReloadIconUI();
				Instance.IgnoreFileDropFrame = Game.PauselessFrame;
				break;
			case ".wav":
			case ".mp3":
			case ".ogg":
				GenericDialogUI.SpawnDialog_Button(
					string.Format(FILE_DROP_MSG_AUDIO, Util.GetNameWithExtension(path)),
					FILE_DROP_LABEL_MUSIC, ImportForMusic,
					FILE_DROP_LABEL_SOUND, ImportForSound,
					BuiltInText.UI_CANCEL, Const.EmptyMethod
				);
				Instance.IgnoreFileDropFrame = Game.PauselessFrame;
				break;
		}

		// Func
		static void ImportForIcon () {
			string path = Instance.DroppingFilePath;
			var project = Instance.CurrentProject;
			if (project == null) return;
			bool success = EngineUtil.CreateIcoFromPng(path, project.IconPath);
			if (success) ProjectEditor.Instance.ReloadIconUI();
		}
		static void ImportForArtwork () {
			string path = Instance.DroppingFilePath;
			PixelEditor.ImportAtlasFromFile(path);
		}
		static void ImportForMusic () {
			string path = Instance.DroppingFilePath;
			var project = Instance.CurrentProject;
			if (project == null) return;
			Util.CopyFile(path, Util.CombinePaths(
				project.Universe.MusicRoot,
				Util.GetNameWithExtension(path)
			));
			Game.SyncAudioPool(Universe.BuiltIn.UniverseRoot, project.UniversePath);
		}
		static void ImportForSound () {
			string path = Instance.DroppingFilePath;
			var project = Instance.CurrentProject;
			if (project == null) return;
			Util.CopyFile(path, Util.CombinePaths(
				project.Universe.SoundRoot,
				Util.GetNameWithExtension(path)
			));
			Game.SyncAudioPool(Universe.BuiltIn.UniverseRoot, project.UniversePath);
		}
	}


	// Quit
	[OnGameTryingToQuit]
	internal static bool OnEngineTryingToQuit () {
		if (Instance == null) return true;
		GenericPopupUI.Instance.Active = false;
		if (Instance.CheckAnyEditorDirty()) {
			GenericDialogUI.SpawnDialog_Button(
				QUIT_MSG,
				BuiltInText.UI_SAVE, SaveAndQuit,
				BuiltInText.UI_DONT_SAVE, Game.QuitApplication,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
			GenericDialogUI.SetItemTint(Color32.GREEN_BETTER);
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
			foreach (var window in Instance.AllWindows) {
				if (window.IsDirty) window.Save();
			}
			Game.QuitApplication();
		}
	}


	[OnGameQuitting]
	internal static void OnEngineQuitting () {
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
		foreach (var win in Instance.AllWindows) win.OnInactivated();
		Instance.Transceiver.Quit();
		var viewPos = Instance.Transceiver.LastRigViewPos;
		var viewHeight = Instance.Transceiver.LastRigViewHeight;
		if (viewPos.HasValue) {
			EngineSetting.LastMapEditorViewX.Value = viewPos.Value.x;
			EngineSetting.LastMapEditorViewY.Value = viewPos.Value.y;
			EngineSetting.LastMapEditorViewZ.Value = viewPos.Value.z;
		}
		if (viewHeight.HasValue) {
			EngineSetting.LastMapEditorViewHeight.Value = viewHeight.Value;
		}
	}


	// Update
	[OnGameUpdateLater(-4096)]
	internal static void OnGUI () {

		GUI.Enable = true;
		GUI.ForceUnifyBasedOnMonitor = true;
		if (!Instance.CurrentWindowRequireRigGame) {
			Sky.ForceSkyboxTint(GUI.Skin.Background);
		}

		using var _ = Scope.Sheet(Instance.ThemeSheet.Sprites.Count > 0 ? Instance.ThemeSheetIndex : -1);
		using var __ = Scope.GuiSkin(Instance.ThemeSkin);

		Instance.OnGUI_Hint();

		if (Instance.CurrentProject == null) {
			Instance.OnGUI_Hub();
		} else {
			Instance.OnGUI_Window();
			Instance.OnGUI_Hotkey();
		}

		Instance.OnGUI_RiggedGame();
		Instance.OnGUI_Requirement();

	}


	// GUI Hub
	private void OnGUI_Hub () {

		var cameraRect = Renderer.CameraRect;
		int hubPanelWidth = GUI.Unify(HUB_PANEL_WIDTH);

		// --- Generic UI ---
		foreach (var win in AllWindows) win.Active = false;
		foreach (var ui in AllGenericUIs) {
			if (!ui.Active) continue;
			ui.FirstUpdate();
			ui.BeforeUpdate();
			ui.Update();
			ui.LateUpdate();
			if (GUI.Enable) GUI.Enable = false;
		}

		// --- File Browser ---
		if (FileBrowserUI.Instance.Active) {
			FileBrowserUI.Instance.Width = GUI.Unify(800);
			FileBrowserUI.Instance.Height = GUI.Unify(600);
		}

		using (Scope.RendererLayerUI()) {

			// --- BG ---
			GUI.DrawSliceOrTile(UI_WINDOW_BG, cameraRect);

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

			int padding = GUI.Unify(8);
			int scrollWidth = GUI.Unify(12);
			int itemHeight = GUI.Unify(52);
			int extendHeight = GUI.Unify(128);
			var contentRect = cameraRect.EdgeInside(Direction4.Right, cameraRect.width - hubPanelWidth).Shrink(
				padding, padding + scrollWidth, padding, padding
			);
			var projects = Projects;

			// BG
			GUI.DrawSliceOrTile(PANEL_BG, contentRect);

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

				var rect = contentRect.Shrink(
					Renderer.TryGetSprite(PANEL_BG, out var bgSprite) ? bgSprite.GlobalBorder : Int4.zero
				).EdgeInside(Direction4.Up, itemHeight);

				bool stepTint = false;

				for (int i = 0; i < projects.Count; i++) {
					string projectPath = projects[i].Path;
					bool folderExists = projects[i].FolderExists;
					var itemContentRect = rect.Shrink(padding);

					// Step Tint
					if (stepTint) Renderer.DrawPixel(rect, Color32.WHITE_6);
					stepTint = !stepTint;

					// Red Highlight
					if (!folderExists && GUI.Enable && rect.MouseInside()) {
						Renderer.DrawPixel(rect, Color32.RED.WithNewA(32));
					}

					// Button
					if (GUI.Button(rect, 0, GUI.Skin.HighlightPixel) && folderExists) {
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
					if (GUI.Enable && rect.MouseInside()) {

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

				if (GUI.Enable && Input.MouseRightButtonDown) {
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
		int barWidth = GetEngineLeftBarWidth(out int contentPadding);
		var barRect = Renderer.CameraRect.EdgeInside(Direction4.Left, barWidth);
		var mousePos = Input.MouseGlobalPosition;
		bool mousePress = Input.MouseLeftButtonDown;
		var rect = barRect.EdgeInside(Direction4.Up, GUI.Unify(42));

		// Interactable
		bool interactable = Game.GlobalFrame > ProjectEditor.Instance.RequiringPublishFrame + 2;
		if (interactable) {
			foreach (var ui in AllGenericUIs) {
				if (!ui.Active) continue;
				interactable = false;
				break;
			}
		}
		if (!interactable) {
			LastNotInteractableFrame = Game.PauselessFrame;
		}

		// UI
		using (Scope.GUIEnable(true, interactable))
		using (Scope.RendererLayerUI()) {

			bool menuButtonClicked = false;

			// Tab BG
			GUI.DrawSliceOrTile(UI_ENGINE_BAR, barRect);

			// Menu Button
			var menuRect = rect.Shrink(contentPadding, contentPadding, contentPadding / 2, contentPadding / 2);
			GUI.DrawSliceOrTile(UI_ENGINE_BAR_BTN, rect);
			if (GUI.BlankButton(rect, out var menuState)) {
				menuButtonClicked = true;
			}
			if (menuState == GUIState.Hover) {
				GUI.DrawSliceOrTile(UI_ENGINE_BAR_BTN_HIGHLIGHT, rect);
			}

			// Menu Icon
			GUI.Icon(menuRect.EdgeInside(Direction4.Left, menuRect.height), BuiltInSprite.ICON_MENU);
			rect.y -= rect.height;

			// Window Tabs
			for (int index = 0; index < AllWindows.Length; index++) {

				var window = AllWindows[index];

				bool selecting = index == CurrentWindowIndex;
				bool hovering = GUI.Enable && rect.Contains(mousePos);

				// Cursor
				if (!selecting && hovering) Cursor.SetCursorAsHand();

				// Body
				GUI.DrawSliceOrTile(UI_ENGINE_BAR_BTN, rect);

				// Highlight
				var bodyTint = Color32.CLEAR;
				int bodyID = UI_ENGINE_BAR_BTN_HIGHLIGHT;
				if (selecting) {
					bodyTint = Color32.WHITE;
				} else if (hovering) {
					bodyTint = Color32.WHITE_128;
				}
				if (window is Console console && console.HasCompileError) {
					bodyTint = Color32.WHITE.WithNewA(
						(byte)(Ease.InOutQuad(Game.GlobalFrame.PingPong(60) / 60f) * 255)
					);
					bodyID = UI_ENGINE_BAR_BTN_WARNING;
				}
				if (bodyTint.a > 0) {
					using (Scope.GUIColor(bodyTint)) {
						GUI.DrawSliceOrTile(bodyID, rect);
					}
				}

				// Content
				var contentRect = rect.Shrink(contentPadding, contentPadding, contentPadding / 2, contentPadding / 2);

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
					barWidth = GetEngineLeftBarWidth(out contentPadding);
				}

				// Next
				rect.SlideDown();
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
					ICON_TAB_BACK, GUI.Skin.IconButton
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
		WindowUI.ForceWindowRect(Renderer.CameraRect.Shrink(barWidth, 0, 0, 0));
		for (int i = 0; i < AllWindows.Length; i++) {
			var win = AllWindows[i];
			bool active = i == CurrentWindowIndex;
			if (active == win.Active) continue;
			win.Active = active;
			if (active) {
				win.OnActivated();
			} else {
				win.OnInactivated();
			}
		}

		// Update Generic UI
		bool oldE = GUI.Interactable;
		GUI.Interactable = true;
		foreach (var ui in AllGenericUIs) {
			if (!ui.Active) continue;
			ui.FirstUpdate();
		}
		foreach (var ui in AllGenericUIs) {
			if (!ui.Active) continue;
			ui.BeforeUpdate();
		}
		foreach (var ui in AllGenericUIs) {
			if (!ui.Active) continue;
			ui.Update();
		}
		foreach (var ui in AllGenericUIs) {
			if (!ui.Active) continue;
			ui.LateUpdate();
		}
		GUI.Interactable = oldE;

		// Update Window UI
		GUI.Interactable = interactable;
		foreach (var ui in AllWindows) {
			if (!ui.Active) continue;
			ui.FirstUpdate();
		}
		foreach (var ui in AllWindows) {
			if (!ui.Active) continue;
			ui.BeforeUpdate();
		}
		foreach (var ui in AllWindows) {
			if (!ui.Active) continue;
			ui.Update();
		}
		foreach (var ui in AllWindows) {
			if (!ui.Active) continue;
			ui.LateUpdate();
		}
		GUI.Interactable = oldE;

		// Misc
		if (GenericDialogUI.ShowingDialog) {
			Game.IgnoreGizmos(1);
		}

		// Change Theme
		if (SettingWindow.Instance.RequireChangeThemePath != null) {
			string path = SettingWindow.Instance.RequireChangeThemePath;
			SettingWindow.Instance.RequireChangeThemePath = null;
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
		foreach (var window in AllWindows) {
			if (!window.Active) continue;
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
		foreach (var window in AllWindows) {
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

		if (GUI.IsTyping || Transceiver.RespondMessage.IsTyping) return;

		// Clear Console
		if (EngineSetting.Hotkey_ClearConsole.Value.Down()) {
			Console.Instance.Clear();
		}

		// Update Project Editor
		if (EngineSetting.Hotkey_Recompile.Value.Down() || ProjectEditor.Instance.RequiringRebuildFrame == Game.GlobalFrame) {
			RequireBackgroundBuildDate = EngineUtil.LastBackgroundBuildModifyDate;
			if (RequireBackgroundBuildDate == 0) {
				RequireBackgroundBuildDate = EngineUtil.GetScriptModifyDate(CurrentProject);
			}
			if (RequireBackgroundBuildDate == 0) {
				RequireBackgroundBuildDate = 1;
			}
		}

		// Run Game
		if (EngineSetting.Hotkey_Run.Value.Down()) {
			EngineUtil.RunAngeliaBuild(CurrentProject);
		}

		// Switch Window
		if (EngineSetting.Hotkey_Window_MapEditor.Value.Down()) {
			SetCurrentWindowIndex<RiggedMapEditor>();
		}
		if (EngineSetting.Hotkey_Window_Artwork.Value.Down()) {
			SetCurrentWindowIndex<PixelEditor>();
		}
		if (EngineSetting.Hotkey_Window_Language.Value.Down()) {
			SetCurrentWindowIndex<LanguageEditor>();
		}
		if (EngineSetting.Hotkey_Window_Console.Value.Down()) {
			SetCurrentWindowIndex<Console>();
		}
		if (EngineSetting.Hotkey_Window_Project.Value.Down()) {
			SetCurrentWindowIndex<ProjectEditor>();
		}
		if (EngineSetting.Hotkey_Window_Setting.Value.Down()) {
			SetCurrentWindowIndex<SettingWindow>();
		}
	}


	private void OnGUI_Hint () {

		using var _ = Scope.RendererLayerUI();

		// Tooltip
		if (
			ToolLabel != null &&
			EngineSetting.UseTooltip.Value &&
			Game.PauselessFrame > LastNotInteractableFrame + 1 &&
			HoveringTooltipDuration >= 60
		) {
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
		}
		ToolLabel = null;

		// Hint
		bool buildingProjectInBackground = EngineUtil.BuildingProjectInBackground;
		if (CurrentProject != null) {

			// Publishing Hint
			if (Game.GlobalFrame <= ProjectEditor.Instance.RequiringPublishFrame + 3) {
				using (Scope.RendererLayerUI()) {
					Renderer.DrawPixel(Renderer.CameraRect, Color32.BLACK_96);
					GUI.BackgroundLabel(
						Renderer.CameraRect, HINT_PUBLISHING, Color32.BLACK, GUI.Unify(12),
						style: GUI.Skin.CenterLabel
					);
				}
			}

			// Hint - Circle
			if (!CurrentWindowRequireRigGame && buildingProjectInBackground) {
				var windowRect = WindowUI.WindowRect;
				int hintPadding = GUI.Unify(6);
				int size = GUI.Unify(32);
				int x = windowRect.xMax - size / 2 - hintPadding;
				int y = windowRect.yMin + size / 2 + hintPadding;
				Renderer.Draw(
					BuiltInSprite.ICON_REFRESH,
					x, y, 500, 500, Game.GlobalFrame * 10,
					size, size, Color32.GREY_128
				);
			}

			// Hint - Label
			if (CurrentWindowRequireRigGame) {
				var windowRect = WindowUI.WindowRect;
				if (!Transceiver.RigProcessRunning) {
					if (buildingProjectInBackground) {
						GUI.BackgroundLabel(
							windowRect, BUILDING_HINT, Color32.BLACK,
							backgroundPadding: GUI.Unify(12), style: RigGameHintStyle
						);
					} else if (EngineUtil.LastBackgroundBuildReturnCode != 0) {
						GUI.BackgroundLabel(
							windowRect, BUILD_ERROR_HINT, Color32.BLACK,
							backgroundPadding: GUI.Unify(12), style: RigGameHintStyle
						);
					} else {
						if (NoGameRunningFrameCount > 60) {
							GUI.BackgroundLabel(
								windowRect, RIG_FAIL_HINT, Color32.BLACK,
								backgroundPadding: GUI.Unify(12), style: RigGameHintStyle
							);
						}
					}
				}
			}
		}

		// Notification
		if (buildingProjectInBackground) return;
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


	private void OnGUI_RiggedGame () {

		var rigEdt = RiggedMapEditor.Instance;
		var calling = Transceiver.CallingMessage;
		var resp = Transceiver.RespondMessage;

		// Call
		bool called = false;
		bool runningGame = !rigEdt.FrameDebugging || rigEdt.RequireNextFrame;
		rigEdt.RequireNextFrame = false;

		if (
			CurrentProject != null &&
			!EngineUtil.BuildingProjectInBackground &&
			Transceiver.RigProcessRunning &&
			CurrentWindowRequireRigGame
		) {
			if (Input.AnyMouseButtonDown) {
				IgnoreInputForRig = IgnoreInputForRig ||
					!WindowUI.WindowRect.Contains(Input.MouseGlobalPosition) ||
					rigEdt.PanelRect.MouseInside();
			}
			if (!Input.AnyMouseButtonHolding) {
				IgnoreInputForRig = false;
			}

			if (rigEdt.DrawCollider) {
				calling.RequireDrawColliderGizmos();
			}
			if (rigEdt.DrawBounds) {
				calling.RequireDrawBoundsGizmos();
			}

			if (SettingWindow.Instance.RigSettingChanged) {
				SettingWindow.Instance.RigSettingChanged = false;
				calling.RequireSettingChange = true;
				calling.Setting_MEDT_Enable = EngineSetting.MapEditor_Enable.Value;
				calling.Setting_MEDT_AutoZoom = EngineSetting.MapEditor_AutoZoom.Value;
				calling.Setting_MEDT_QuickPlayerDrop = EngineSetting.MapEditor_QuickPlayerDrop.Value;
				calling.Setting_MEDT_ShowBehind = EngineSetting.MapEditor_ShowBehind.Value;
				calling.Setting_MEDT_ShowState = EngineSetting.MapEditor_ShowState.Value;
			}

			if (runningGame) {
				Transceiver.Call(
					ignoreMouseInput:
						IgnoreInputForRig ||
						Game.PauselessFrame < LastNotInteractableFrame + 6,
					ignoreKeyInput:
						false,
					leftPadding:
						GetEngineLeftBarWidth(out _),
					requiringWindowIndex:
						0
				);
			}
			called = true;
		}

		// Respond
		bool buildingProjectInBackground = EngineUtil.BuildingProjectInBackground;
		NoGameRunningFrameCount = Transceiver.RigProcessRunning || buildingProjectInBackground ? 0 : NoGameRunningFrameCount + 1;

		if (CurrentProject == null) {
			if (Transceiver.RigProcessRunning) {
				Transceiver.Abort();
			}
			return;
		}

		// Abort when Building
		if (Transceiver.RigProcessRunning && buildingProjectInBackground) {
			Transceiver.Abort();
		}

		var toolPanelRect = RiggedMapEditor.Instance.PanelRect;
		int sheetIndex = PixelEditor.Instance.SheetIndex;
		if (buildingProjectInBackground) {
			// Building in Background
			if (CurrentWindowRequireRigGame) {
				Transceiver.UpdateLastRespondedRender(sheetIndex, toolPanelRect, coverWithBlackTint: true);
			}
		} else if (Transceiver.RigProcessRunning) {
			// Rig Running
			if (called) {
				if (runningGame) {
					Transceiver.Respond(sheetIndex, CurrentWindowIndex == RigMapEditorWindowIndex, toolPanelRect);
					rigEdt.UpdateUsageData(
						resp.RenderUsages,
						resp.RenderCapacities,
						resp.EntityUsages,
						resp.EntityCapacities
					);
					rigEdt.HavingGamePlay = resp.GamePlaying;
					if (CurrentWindowIndex == RigMapEditorWindowIndex) {
						Sky.ForceSkyboxTint(resp.SkyTop, resp.SkyBottom, 3);
					}
				} else {
					Transceiver.UpdateLastRespondedRender(sheetIndex, toolPanelRect);
				}
			}
		} else if (
			(RigGameFailToStartCount < 16 && Game.GlobalFrame > RigGameFailToStartFrame + 30) ||
			Game.GlobalFrame > RigGameFailToStartFrame + 6000
		) {
			// No Rig Game Running
			if (EngineSetting.ClearCharacterConfigBeforeGameStart.Value) {
				string configFolder = CurrentProject.Universe.CharacterConfigRoot;
				Util.DeleteFolder(configFolder);
				Util.CreateFolder(configFolder);
			}
			int code = Transceiver.Start(
				CurrentProject.BuildPath,
				CurrentProject.UniversePath
			);
			if (code == 0) {
				// Start
				RigGameFailToStartCount = 0;
				RigGameFailToStartFrame = int.MinValue;
				SettingWindow.Instance.RigSettingChanged = true;
			} else {
				// Fail to Start
				RigGameFailToStartFrame = Game.GlobalFrame;
				RigGameFailToStartCount++;
			}
			if (CurrentWindowRequireRigGame) {
				// Still Render Last Image
				Transceiver.UpdateLastRespondedRender(sheetIndex, toolPanelRect, coverWithBlackTint: true);
			}
		}

	}


	private void OnGUI_Requirement () {

		if (EngineUtil.BuildingProjectInBackground) return;

		// Rebuild
		if (RequireBackgroundBuildDate > 0) {
			Transceiver.Abort();
			EngineUtil.BuildAngeliaProjectInBackground(CurrentProject, RequireBackgroundBuildDate);
			RequireBackgroundBuildDate = 0;
		}

		// Publish
		if (Game.GlobalFrame == ProjectEditor.Instance.RequiringPublishFrame) {
			SetCurrentWindowIndex(ConsoleWindowIndex);
		} else if (Game.GlobalFrame == ProjectEditor.Instance.RequiringPublishFrame + 2) {
			Transceiver.Abort();
			if (Instance.Transceiver.RigProcessRunning) {
				Instance.Transceiver.Abort();
			}
			string path = ProjectEditor.Instance.RequiringPublishPath;
			int returnCode = EngineUtil.PublishAngeliaProject(Instance.CurrentProject, path);
			if (returnCode != 0) {
				Debug.LogError("Error on publishing, code:" + returnCode);
			}
			if (Util.FolderExists(path)) {
				Game.OpenUrl(path);
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
		// Func
		static void DeleteProject () {
			if (Instance == null) return;
			int menuIndex = Instance.CurrentProjectMenuIndex;
			if (menuIndex < 0 || menuIndex >= Instance.Projects.Count) return;
			Instance.Projects.RemoveAt(menuIndex);
		}
	}


	private bool CheckAnyEditorDirty () {
		foreach (var window in AllWindows) if (window.IsDirty) return true;
		return false;
	}


	private void SetCurrentWindowIndex<W> (bool forceChange = false) where W : WindowUI {
		for (int i = 0; i < AllWindows.Length; i++) {
			if (AllWindows[i] is W) {
				SetCurrentWindowIndex(i, forceChange);
				return;
			}
		}
	}
	private void SetCurrentWindowIndex (int index, bool forceChange = false) {
		index = index.Clamp(0, AllWindows.Length - 1);
		if (!forceChange && index == CurrentWindowIndex) return;
		CurrentWindowRequireRigGame = index == RigMapEditorWindowIndex;
		if (CurrentWindowRequireRigGame) {
			// Rig Window
			if (Transceiver.RigProcessRunning) Transceiver.CallingMessage.RequireFocusInvoke();
		} else {
			// Normal Window
			if (Transceiver.RigProcessRunning) Transceiver.CallingMessage.RequireLostFocusInvoke();
			ResetViewRect(remapAllRenderingCells: true);
			Game.MusicVolume = 1000;
			Game.SoundVolume = 1000;
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
		LanguageEditor.Instance.SetLanguageRoot(CurrentProject.Universe.LanguageRoot);
		PixelEditor.Instance.SetCurrentProject(CurrentProject);
		ProjectEditor.Instance.SetCurrentProject(CurrentProject);
		RiggedMapEditor.Instance.CleanDirty();

		// Audio
		Game.SyncAudioPool(Universe.BuiltIn.UniverseRoot, CurrentProject.UniversePath);

		// Font
		Game.UnloadFontsFromPool(ignoreBuiltIn: true);
		Game.LoadFontsIntoPool(CurrentProject.Universe.FontRoot, builtIn: false);

		// Change Check
		CheckScriptChanged();
		CheckEngineResourceChanged();
		CheckDialogChanged();

		// Sync Engine Version
		if (Universe.BuiltIn.Info.EngineBuildVersion != CurrentProject.Universe.Info.EngineBuildVersion) {
			CurrentProject.Universe.Info.EngineBuildVersion = Universe.BuiltIn.Info.EngineBuildVersion;
			JsonUtil.SaveJsonToPath(CurrentProject.Universe.Info, CurrentProject.Universe.InfoPath, true);
		}

		// Rebuild
		Util.DeleteFolder(CurrentProject.BuildPath);
		EngineUtil.BuildAngeliaProjectInBackground(CurrentProject, RequireBackgroundBuildDate);
		RequireBackgroundBuildDate = 0;

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
			foreach (var window in Instance.AllWindows) {
				if (window.IsDirty) window.Save();
			}
			Close();
		}
		static void Close () {
			Instance.CurrentProject = null;
			foreach (var ui in Instance.AllWindows) {
				ui.Active = false;
				ui.OnInactivated();
			}
			foreach (var ui in Instance.AllGenericUIs) {
				ui.Active = false;
				ui.OnInactivated();
			}
			LanguageEditor.Instance.SetLanguageRoot("");
			PixelEditor.Instance.SetCurrentProject(null);
			ProjectEditor.Instance.SetCurrentProject(null);
			Game.SetWindowTitle("AngeliA Engine");
			Instance.Transceiver.RespondMessage.Reset(clearLastRendering: true);
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
			EngineBuildVersion = Universe.BuiltIn.Info.EngineBuildVersion,
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


	// Change Check
	private void CheckScriptChanged () {
		long dllModifyDate = EngineUtil.GetBuildLibraryModifyDate(CurrentProject);
		long srcModifyDate = EngineUtil.GetScriptModifyDate(CurrentProject);
		if (srcModifyDate > dllModifyDate && srcModifyDate > EngineUtil.LastBackgroundBuildModifyDate) {
			RiggedMapEditor.Instance.SetDirty();
			RequireBackgroundBuildDate = EngineSetting.AutoRecompile.Value ? srcModifyDate : 0;
		} else {
			RequireBackgroundBuildDate = 0;
		}
	}


	private void CheckResourceChanged () {
		if (CurrentProject == null) return;
		// Fonts
		bool changed = Game.SyncFontsWithPool(CurrentProject.Universe.FontRoot);
		if (changed) {
			Transceiver.CallingMessage.RequireClearCharPoolInvoke();
		}
		// Audio
		Game.SyncAudioPool(Universe.BuiltIn.UniverseRoot, CurrentProject.UniversePath);
		// Icon
		if (ProjectEditor.Instance.IconFileModified()) {
			ProjectEditor.Instance.ReloadIconUI();
		}
	}


	private void CheckEngineResourceChanged () {

		if (CurrentProject == null) return;

		// Framework Dll Files
		string sourceDllDebug = EngineUtil.TemplateFrameworkDll_Debug;
		if (Util.FileExists(sourceDllDebug)) {
			string targetPath = CurrentProject.FrameworkDllPath_Debug;
			long sourceDate = Util.GetFileModifyDate(sourceDllDebug);
			long targetDate = Util.GetFileModifyDate(targetPath);
			if (sourceDate != targetDate) {
				Util.CopyFile(sourceDllDebug, targetPath, true);
				Util.SetFileModifyDate(targetPath, sourceDate);
			}
		}
		string sourceDllRelease = EngineUtil.TemplateFrameworkDll_Release;
		if (Util.FileExists(sourceDllRelease)) {
			string targetPath = CurrentProject.FrameworkDllPath_Release;
			long sourceDate = Util.GetFileModifyDate(sourceDllRelease);
			long targetDate = Util.GetFileModifyDate(targetPath);
			if (sourceDate != targetDate) {
				Util.CopyFile(sourceDllRelease, targetPath, true);
				Util.SetFileModifyDate(targetPath, sourceDate);
			}
		}

		// Entry Exe File
		string sourceEntryPath = EngineUtil.EntryExePath;
		if (Util.FileExists(sourceEntryPath)) {
			var targetEntryPath = Util.CombinePaths(CurrentProject.BuildPath, Util.GetNameWithExtension(sourceEntryPath));
			long sourceDate = Util.GetFileModifyDate(sourceEntryPath);
			long targetDate = Util.GetFileModifyDate(sourceEntryPath);
			if (sourceDate != targetDate) {
				Util.CopyFile(sourceEntryPath, targetEntryPath, true);
				Util.SetFileModifyDate(targetEntryPath, sourceDate);
			}
		}


	}


	private void CheckDialogChanged () => EngineUtil.TryCompileDialogueFiles(
		Universe.BuiltIn.EditableConversationRoot,
		Universe.BuiltIn.ConversationRoot,
		forceCompile: false
	);


	// Misc
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


	private void ResetViewRect (bool remapAllRenderingCells = false) {
		Stage.SetViewRectImmediately(
			new IRect(0, 0, Const.VIEW_RATIO * Game.DefaultViewHeight / 1000, Game.DefaultViewHeight),
			remapAllRenderingCells
		);
		WindowUI.ForceWindowRect(Renderer.CameraRect.Shrink(GetEngineLeftBarWidth(out _), 0, 0, 0));
	}


	private int GetEngineLeftBarWidth (out int contentPadding) {
		contentPadding = GUI.Unify(8);
		return (FullsizeMenu.Value ? GUI.Unify(WINDOW_BAR_WIDTH_FULL) : GUI.Unify(WINDOW_BAR_WIDTH_NORMAL)) + contentPadding;
	}


	#endregion




}