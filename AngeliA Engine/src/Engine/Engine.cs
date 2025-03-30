global using Debug = AngeliA.Debug;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliA;

[assembly: ToolApplication]

namespace AngeliaEngine;

internal partial class Engine {




	#region --- VAR ---


	// Const 
	private const int NOTIFY_DURATION = 120;
	private const int WINDOW_BAR_WIDTH_FULL = 120;
	private const int WINDOW_BAR_WIDTH_NORMAL = 42;

	private static readonly SpriteCode UI_ENGINE_BAR = "UI.EngineSideBar";
	private static readonly SpriteCode UI_ENGINE_BAR_BTN = "UI.EngineSideButton";
	private static readonly SpriteCode UI_ENGINE_BAR_BTN_HIGHLIGHT = "UI.EngineSideButtonHighlight";
	private static readonly SpriteCode UI_ENGINE_BAR_BTN_WARNING = "UI.EngineSideButtonWarning";
	private static readonly SpriteCode ICON_TAB_BACK = "Icon.MainTabBack";

	private static readonly LanguageCode BUILDING_HINT = ("UI.Rig.BuildingHint", "Recompiling");
	private static readonly LanguageCode BUILD_ERROR_HINT = ("UI.Rig.BuildError", "Compile Error in Game Script :(\nAll errors must be fixed otherwise the game cannot run");
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
	private static readonly LanguageCode NOTI_THEME_NOT_LOADED = ("Noti.ThemeLoaded", "Fail to Load Theme");
	private static readonly LanguageCode FILE_DROP_MSG_AUDIO = ("UI.FileDropMsg.Audio", "Import audio file {0} as:");
	private static readonly LanguageCode FILE_DROP_LABEL_MUSIC = ("UI.FileDropLabel.Music", "Music");
	private static readonly LanguageCode FILE_DROP_LABEL_SOUND = ("UI.FileDropLabel.Sound", "Sound");

	// Data
	private static Engine Instance = null;
	private readonly GUIStyle TooltipStyle = new(GUI.Skin.SmallLabel);
	private readonly GUIStyle NotificationLabelStyle = new(GUI.Skin.Label) { Alignment = Alignment.BottomRight, };
	private readonly GUIStyle NotificationSubLabelStyle = new(GUI.Skin.SmallLabel) { Alignment = Alignment.BottomRight, };
	private readonly List<ProjectData> Projects = [];
	private readonly Sheet RenderingSheet = new(ignoreGroups: false, ignoreSpriteWithPaletteTag: true);
	private readonly Sheet ThemeSheet = new(ignoreGroups: true, ignoreSpriteWithPaletteTag: true);
	private readonly GUISkin ThemeSkin = new() { Name = "Built-in" };
	private EntityUI[] AllGenericUIs;
	private WindowUI[] AllWindows;
	private Project CurrentProject = null;
	private ProjectData CurrentProjectData = null;
	private ProjectSortMode ProjectSort = ProjectSortMode.OpenTime;
	private IRect ToolLabelRect;
	private IRect LastHoveringToolLabelRect;
	private int HoveringTooltipDuration = 0;
	private int CurrentWindowIndex = 0;
	private int HubPanelScroll = 0;
	private int CurrentProjectMenuIndex = -1;
	private int NotificationStartFrame = int.MinValue;
	private int ThemeSheetIndex = -1;
	private int LastNotInteractableFrame = int.MinValue;
	private int IgnoreFileDropFrame = int.MinValue;
	private bool NotificationFlash = false;
	private bool AnyGenericWindowActived = false;
	private string ToolLabel = null;
	private string NotificationContent = null;
	private string NotificationSubContent = null;
	private string DroppingFilePath = "";

	// Saving
	private static readonly SavingString ProjectPaths = new("Engine.ProjectPaths", "#", SavingLocation.Global);
	private static readonly SavingString LastOpenProject = new("Engine.LastOpenProject", "", SavingLocation.Global);
	private static readonly SavingString CurrentThemeName = new("Engine.CurrentThemeName", "", SavingLocation.Global);
	private static readonly SavingBool Maximize = new("Engine.Maximize", true, SavingLocation.Global);
	private static readonly SavingBool FullsizeMenu = new("Engine.FullsizeMenu", true, SavingLocation.Global);
	private static readonly SavingInt WindowSizeX = new("Engine.WindowSizeX", 1024, SavingLocation.Global);
	private static readonly SavingInt WindowSizeY = new("Engine.WindowSizeY", 1024, SavingLocation.Global);
	private static readonly SavingInt WindowPositionX = new("Engine.WindowPosX", 128, SavingLocation.Global);
	private static readonly SavingInt WindowPositionY = new("Engine.WindowPosY", 128, SavingLocation.Global);
	private static readonly SavingInt LastOpenedWindowIndex = new("Engine.LastOpenedWindowIndex", 0, SavingLocation.Global);
	private static readonly SavingInt ProjectSortIndex = new("Engine.ProjectSortIndex", 0, SavingLocation.Global);


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {

		var engine = new Engine();
		Instance = engine;
		engine.AllWindows = [
			new GameEditor(),
			new PixelEditor(),
			new LanguageEditor(),
			new ConsoleWindow(),
			new ProjectEditor(),
			new PackageManager(),
			new SettingWindow(),
		];
		engine.AllGenericUIs = [
			new GenericPopupUI(),
			new GenericDialogUI(),
			new FileBrowserUI(),
		];

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

		engine.ProjectSort = (ProjectSortMode)ProjectSortIndex.Value;

		// Projects
		engine.Projects.Clear();
		if (ProjectPaths.Value == "#") {
			// Init for First Time Open
			ProjectPaths.Value = "";
			// Add Built-in Projects
			foreach (var path in Util.EnumerateFolders(EngineUtil.BuiltInProjectRoot, true)) {
				engine.Projects.Add(new ProjectData(
					path: path,
					folderExists: true,
					lastOpenTime: Util.GetFolderModifyDate(path)
				));
			}
		}
		var projectPaths = ProjectPaths.Value.Split(';');
		if (projectPaths != null) {
			for (int i = 0; i < projectPaths.Length; i++) {
				string path = projectPaths[i];
				if (string.IsNullOrWhiteSpace(path)) continue;
				if (!System.IO.Path.IsPathRooted(path)) {
					path = System.IO.Path.GetFullPath(path, Universe.BuiltIn.UniverseRoot);
				}
				engine.Projects.Add(new ProjectData(
					path: path,
					folderExists: Util.FolderExists(path),
					lastOpenTime: Util.GetFolderModifyDate(path)
				));
			}
		}
		engine.RefreshProjectCache();
		engine.SortProjects();
		engine.RefreshAllProjectDisplayName();
		SyncIconSpriteToMainSheet();

		// Engine Window
		if (Maximize.Value) {
			Game.IsWindowMaximized = Maximize.Value;
		} else {
			Game.SetWindowPosition(WindowPositionX.Value, WindowPositionY.Value);
			Game.SetWindowSize(WindowSizeX.Value, WindowSizeY.Value);
		}
		Game.SetEventWaiting(false);
		Game.ProcedureAudioVolume = 1000;

		// UI Window
		for (int i = 0; i < engine.AllWindows.Length; i++) {
			var win = engine.AllWindows[i];
			win.OnActivated();
		}

		engine.SetCurrentWindow(LastOpenedWindowIndex.Value, forceChange: true);
		engine.ResetViewRect();

		// Sheet/Theme
		engine.RenderingSheetIndex = Renderer.AddAltSheet(engine.RenderingSheet);
		engine.ThemeSheetIndex = Renderer.AddAltSheet(engine.ThemeSheet);
		if (!string.IsNullOrEmpty(CurrentThemeName.Value)) {
			engine.LoadTheme(
				Util.CombinePaths(EngineUtil.ThemeRoot, $"{CurrentThemeName.Value}.{AngePath.SHEET_FILE_EXT}"),
				false
			);
		}

	}


	[OnGameInitializeLater(5000)]
	internal static void OpenProjectOnStart () {
		if (
			EngineSetting.OpenLastProjectOnStart.Value &&
			Instance.Projects.Any(data => data.Path == LastOpenProject.Value)
		) {
			foreach (var proData in Instance.Projects) {
				if (proData.Path == LastOpenProject.Value) {
					Instance.OpenProject(proData);
					break;
				}
			}
		} else {
			Game.SetWindowTitle("AngeliA Engine");
			Game.SetWindowIcon("WindowIcon".AngeHash());
		}
	}


	// Misc
	[OnFileDropped_StringPath]
	internal static void OnFileDropped (string path) {

		var project = Instance.CurrentProject;
		if (Game.PauselessFrame <= Instance.IgnoreFileDropFrame) return;

		Instance.DroppingFilePath = path;

		string ex = Util.GetExtensionWithDot(path);
		switch (ex) {
			case ".ase":
			case ".aseprite":
				if (project == null) break;
				if (Instance.CurrentWindow is not PixelEditor) break;
				PixelEditor.ImportAtlasFromFile(path);
				Instance.IgnoreFileDropFrame = Game.PauselessFrame;
				break;
			case ".png":
				if (project == null) break;
				if (Instance.CurrentWindow is not PixelEditor) break;
				PixelEditor.ImportAtlasFromFile(Instance.DroppingFilePath);
				Instance.IgnoreFileDropFrame = Game.PauselessFrame;
				break;
			case ".wav":
			case ".mp3":
			case ".ogg":
			case ".xm":
			case ".mod":
				if (project == null) break;
				if (Instance.CurrentWindow is not ProjectEditor) break;
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
		static void ImportForMusic () => EngineUtil.ImportMusicFile(Instance.CurrentProject, Instance.DroppingFilePath);
		static void ImportForSound () => EngineUtil.ImportSoundFile(Instance.CurrentProject, Instance.DroppingFilePath);
	}


	[OnGameFocused]
	internal static void OnGameFocused () {
		Instance?.CheckScriptChanged();
		Instance?.RefreshProjectCache();
		Instance?.CheckResourceChanged();
	}


	// Quit
	[OnGameTryingToQuit]
	internal static bool OnEngineTryingToQuit () {
		if (Instance == null || Instance.CurrentProject == null) return true;
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
		ProjectSortIndex.Value = (int)Instance.ProjectSort;
		ProjectPaths.Value = Instance.Projects.JoinArray(p => p.Path, ';');
		foreach (var win in Instance.AllWindows) win.OnInactivated();
	}


	// Update
	[OnGameUpdateLater(-4096)]
	internal static void OnGUI () {

		GUI.Enable = true;
		if (!Instance.CurrentWindowRequireRigGame) {
			Sky.ForceSkyboxTint(GUI.Skin.Background);
		}
		if (Instance.CurrentProject == null || !Instance.CurrentWindowRequireRigGame) {
			if (!Instance.AnyGenericWindowActived) {
				Game.ForceGizmosOnTopOfUI(1);
				Game.ForceDoodleOnTopOfUI(1);
			}
		}
		if (Instance.CurrentWindow is GameEditor gameEDT) {
			var padding = Int4.Direction(Instance.GetEngineLeftBarWidth(out int _), gameEDT.ToolbarWidth, 0, 0);
			padding.left = padding.left * Game.ScreenWidth / Renderer.CameraRect.width.GreaterOrEquel(1);
			padding.right = padding.right * Game.ScreenWidth / Renderer.CameraRect.width.GreaterOrEquel(1);
			Game.ScreenEffectPadding = padding;
		} else {
			Game.ScreenEffectPadding = default;
		}

		using var _ = new SheetIndexScope(Instance.ThemeSheet.Sprites.Count > 0 ? Instance.ThemeSheetIndex : -1);
		using var __ = new GUISkinScope(Instance.ThemeSkin);

		Instance.OnGUI_Interactable();
		Instance.OnGUI_Hint();
		if (Instance.CurrentProject == null) {
			Instance.OnGUI_Hub();
		} else {
			Instance.OnGUI_Window();
			Instance.OnGUI_Engine();
			Instance.OnGUI_Hotkey();
		}
		Instance.OnGUI_RiggedGame();
		Instance.OnGUI_Requirement();
	}


	// GUI Window
	private void OnGUI_Interactable () {
		AnyGenericWindowActived = false;
		bool interactable = Game.GlobalFrame > ProjectEditor.Instance.RequiringPublishFrame + 2;
		if (interactable) {
			foreach (var ui in AllGenericUIs) {
				if (!ui.Active) continue;
				AnyGenericWindowActived = true;
				interactable = false;
				break;
			}
		}
		if (!interactable) {
			LastNotInteractableFrame = Game.PauselessFrame;
		}
		GUI.Interactable = interactable;
	}


	private void OnGUI_Hint () {

		using var _ = new UILayerScope();

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
			var tipRect = ToolLabelRect.EdgeOutsideDown(GUI.Unify(24)).Shift(
				leftSide ? GUI.Unify(20) : GUI.Unify(-20),
				downSide ? GUI.Unify(10) : GUI.Unify(-10)
			);
			tipRect.ClampPositionInside(WindowUI.WindowRect);
			GUI.BackgroundLabel(
				tipRect, ToolLabel, Color32.BLACK,
				GUI.Unify(6), false, TooltipStyle
			);
		}
		ToolLabel = null;

		// Hint
		bool buildingProjectInBackground = EngineUtil.BuildingProjectInBackground;
		if (CurrentProject != null) {

			// Publishing Hint
			if (Game.GlobalFrame <= ProjectEditor.Instance.RequiringPublishFrame + 3) {
				using (new UILayerScope()) {
					Renderer.DrawPixel(Renderer.CameraRect, Color32.BLACK_96);
					GUI.BackgroundLabel(
						Renderer.CameraRect, HINT_PUBLISHING, Color32.BLACK, GUI.Unify(12),
						style: GUI.Skin.CenterLabel
					);
				}
			}

			// Hint - Label
			if (CurrentWindowRequireRigGame && !GenericDialogUI.ShowingDialog) {
				var windowRect = WindowUI.WindowRect;
				if (!Transceiver.RigProcessRunning) {
					if (buildingProjectInBackground) {
						GUI.BackgroundLabel(
							windowRect, BUILDING_HINT, Color32.BLACK,
							backgroundPadding: GUI.Unify(12), style: RigGameHintStyle
						);
					} else if (HasCompileError) {
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

		// Main Label
		rect.y = top - labelHeight - (hasSub ? 0 : subLabelHeight);
		rect.height = labelHeight;
		GUI.Label(rect, NotificationContent, out var bound, NotificationLabelStyle);

		// Sub Label
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


	private void OnGUI_Window () {

		int barWidth = GetEngineLeftBarWidth(out _);

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

		// Misc
		if (GenericDialogUI.ShowingDialog) {
			Game.CancelGizmosOnTopOfUI();
			Game.CancelDoodleOnTopOfUI();
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


	private void OnGUI_Engine () {

		using var _ = new UILayerScope();

		// Window
		int barWidth = GetEngineLeftBarWidth(out int contentPadding);
		var barRect = Renderer.CameraRect.EdgeInside(Direction4.Left, barWidth);
		var mousePos = Input.MouseGlobalPosition;
		bool mousePress = Input.MouseLeftButtonDown;
		var rect = barRect.EdgeInside(Direction4.Up, GUI.Unify(42));
		var projectType = CurrentProject.Universe.Info.ProjectType;
		bool menuButtonClicked = false;

		// Tab BG
		GUI.DrawSlice(UI_ENGINE_BAR, barRect);

		// Menu Button
		var menuRect = rect.Shrink(contentPadding, contentPadding, contentPadding / 2, contentPadding / 2);
		GUI.DrawSlice(UI_ENGINE_BAR_BTN, rect);
		if (GUI.BlankButton(rect, out var menuState)) {
			menuButtonClicked = true;
		}
		if (menuState == GUIState.Hover) {
			GUI.DrawSlice(UI_ENGINE_BAR_BTN_HIGHLIGHT, rect);
		}

		// Menu Icon
		GUI.Icon(menuRect.EdgeInside(Direction4.Left, menuRect.height), BuiltInSprite.ICON_MENU);
		rect.y -= rect.height;

		// Window Tabs
		for (int index = 0; index < AllWindows.Length; index++) {

			var window = AllWindows[index];

			// Put Setting Icon at Bottom
			if (window is SettingWindow && index == AllWindows.Length - 1) {
				rect = barRect.EdgeInsideDown(rect.height).Shift(0, rect.height);
			}

			bool selecting = index == CurrentWindowIndex;
			bool hovering = GUI.Enable && GUI.Interactable && rect.Contains(mousePos);

			// Skip Windows for Non-Game Project
			if (IsWindowIgnoredForProject(window, projectType)) {
				continue;
			}

			// Cursor
			if (!selecting && hovering) {
				Cursor.SetCursorAsHand();
			}

			// Body
			GUI.DrawSlice(UI_ENGINE_BAR_BTN, rect);

			// Highlight
			var bodyTint = Color32.CLEAR;
			int bodyID = UI_ENGINE_BAR_BTN_HIGHLIGHT;
			if (selecting) {
				bodyTint = Color32.WHITE;
			} else if (hovering) {
				bodyTint = Color32.WHITE_128;
			}
			if (window is ConsoleWindow console && console.HasCompileError) {
				bodyTint = EngineSetting.BlinkWhenError.Value ? Color32.WHITE.WithNewA(
					(byte)(Ease.InOutQuad(Game.GlobalFrame.PingPong(60) / 60f) * 255)
				) : Color32.WHITE;
				bodyID = UI_ENGINE_BAR_BTN_WARNING;
			}
			if (bodyTint.a > 0) {
				using (new GUIColorScope(bodyTint)) {
					GUI.DrawSlice(bodyID, rect);
				}
			}

			// Content
			var contentRect = rect.Shrink(contentPadding, contentPadding, contentPadding / 2, contentPadding / 2);

			// Icon
			int iconSize = contentRect.height;
			var iconRect = contentRect.EdgeInside(Direction4.Left, iconSize);
			int iconID = window is GameEditor && CurrentProjectData != null ? CurrentProjectData.IconID : window.TypeID;
			using (new SheetIndexScope(-1)) {
				if (Renderer.TryGetSprite(iconID, out var iconSP)) {
					Renderer.Draw(iconSP, iconRect);
				} else {
					Renderer.Draw(window.TypeID, iconRect);
				}
			}

			// Compling Mark
			if (window is GameEditor && EngineUtil.BuildingProjectInBackground) {
				int size = GUI.Unify(24);
				if (!FullsizeMenu.Value) {
					Renderer.DrawPixel(rect, Color32.BLACK_128);
				}
				Renderer.Draw(
					BuiltInSprite.ICON_REFRESH,
					FullsizeMenu.Value ? rect.xMax - size : rect.CenterX(),
					rect.CenterY(),
					500, 500, Game.GlobalFrame * 10,
					size, size, Color32.ORANGE_BETTER
				);
			}

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
				GUI.Label(
					contentRect.Shrink(iconSize + contentPadding, 0, 0, 0),
					Language.Get(window.TypeID, window.DefaultWindowName),
					GUI.Skin.SmallLabel
				);
			}

			// Click
			if (mousePress && hovering && GUI.Interactable) {
				Input.UseAllMouseKey();
				SetCurrentWindow(index);
				barWidth = GetEngineLeftBarWidth(out contentPadding);
			}

			// Next
			rect.SlideDown();
		}

		// Back to Hub
		if (FullsizeMenu.Value) {
			var btnRect = barRect.EdgeInside(Direction4.Down, rect.height);
			if (GUI.Button(btnRect, BuiltInText.UI_BACK, GUI.Skin.SmallCenterLabelButton)) {
				TryCloseProject();
			}
			GUI.Icon(
				btnRect.WithNewWidth(rect.height + contentPadding),
				ICON_TAB_BACK, GUI.Skin.IconButton, GUIState.Normal
			);
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


	private void OnGUI_Hotkey () {

		if (GUI.IsTyping || Transceiver.RespondMessage.IsTyping) return;

		// Clear Console
		if (EngineSetting.Hotkey_ClearConsole.Value.Down()) {
			ConsoleWindow.Instance.Clear();
		}

		if (!EngineUtil.BuildingProjectInBackground) {
			// Recompile
			if (
				EngineSetting.Hotkey_Recompile.Value.Down() ||
				ProjectEditor.Instance.RequiringRebuildFrame > 0 ||
				PackageManager.Instance.RequiringRebuildFrame > 0
			) {
				// Save First
				foreach (var window in Instance.AllWindows) {
					if (window.IsDirty) window.Save();
				}
				// Recompile
				ProjectEditor.Instance.RequiringRebuildFrame = -2;
				PackageManager.Instance.RequiringRebuildFrame = -2;
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
		}

		// Switch Window
		if (EngineSetting.Hotkey_Window_MapEditor.Value.Down()) {
			SetCurrentWindow<GameEditor>();
		}
		if (EngineSetting.Hotkey_Window_Artwork.Value.Down()) {
			SetCurrentWindow<PixelEditor>();
		}
		if (EngineSetting.Hotkey_Window_Language.Value.Down()) {
			SetCurrentWindow<LanguageEditor>();
		}
		if (EngineSetting.Hotkey_Window_Console.Value.Down()) {
			SetCurrentWindow<ConsoleWindow>();
		}
		if (EngineSetting.Hotkey_Window_Project.Value.Down()) {
			SetCurrentWindow<ProjectEditor>();
		}
		if (EngineSetting.Hotkey_Window_Package.Value.Down()) {
			SetCurrentWindow<PackageManager>();
		}
		if (EngineSetting.Hotkey_Window_Setting.Value.Down()) {
			SetCurrentWindow<SettingWindow>();
		}
	}


	private void OnGUI_Requirement () {

		if (EngineUtil.BuildingProjectInBackground) return;

		// Theme
		string requireChangeThemePath = SettingWindow.Instance.RequireChangeThemePath ?? PixelEditor.Instance.RequireChangeThemePath;
		if (requireChangeThemePath != null) {
			string path = requireChangeThemePath;
			SettingWindow.Instance.RequireChangeThemePath = null;
			PixelEditor.Instance.RequireChangeThemePath = null;
			LoadTheme(path, true);
		}

		// Ignore Rebuild for Non-Game Project
		if (CurrentProject != null && CurrentProject.Universe.Info.ProjectType != ProjectType.Game) {
			RequireBackgroundBuildDate = 0;
		}

		// Rebuild
		if (RequireBackgroundBuildDate > 0) {
			Transceiver.Abort();
			EngineUtil.BuildAngeliaProjectInBackground(CurrentProject, RequireBackgroundBuildDate);
			RequireBackgroundBuildDate = 0;
		}

		// Reload Sheet
		if (PackageManager.Instance.RequiringReloadSheet) {
			PackageManager.Instance.RequiringReloadSheet = false;
			ReloadRenderingSheet();
		}

		// Publish
		if (Game.GlobalFrame == ProjectEditor.Instance.RequiringPublishFrame) {
			SetCurrentWindow<ConsoleWindow>();
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

		// Rendering Sheet
		if (PixelEditor.Instance.RequireReloadRenderingSheet) {
			PixelEditor.Instance.RequireReloadRenderingSheet = false;
			ReloadRenderingSheet();
		}

		// Dirty
		if (PixelEditor.Instance.RequireUniverseDirty) {
			PixelEditor.Instance.RequireUniverseDirty = false;
			ProjectEditor.Instance.Save(true);
		}

	}


	#endregion




	#region --- LGC ---


	// Workflow
	private void OpenProject (ProjectData projectData) {

		if (projectData == null) return;
		string projectPath = projectData.Path;
		if (CurrentProject != null && projectPath == CurrentProject.ProjectPath) return;
		if (!Util.FolderExists(projectPath)) return;

		CurrentProject = Project.LoadProject(projectPath);
		CurrentProjectData = projectData;
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
		Game.SetWindowTitle($"AngeliA Engine - {projectData.Name}");
		Game.SetWindowIcon(projectData.IconID);

		// Windows
		LanguageEditor.Instance.SetCurrentProject(CurrentProject);
		PixelEditor.Instance.SetCurrentProject(CurrentProject);
		ProjectEditor.Instance.SetCurrentProject(CurrentProject);
		SettingWindow.Instance.SetCurrentProject(CurrentProject);
		GameEditor.Instance.CleanDirty();
		GameEditor.Instance.SetCurrentProject(CurrentProject);
		PackageManager.Instance.SetCurrentProject(CurrentProject);
		ConsoleWindow.Instance.SetCurrentProject(CurrentProject);

		// Audio
		Game.SyncAudioPool(Universe.BuiltIn.UniverseRoot, CurrentProject.UniversePath);

		// Font
		Game.UnloadFontsFromPool(ignoreBuiltIn: true);
		Game.LoadFontsIntoPool(CurrentProject.Universe.FontRoot, builtIn: false);

		// Update Built-in Sheet from Engine Sheet to Project Sheet
		if (!CurrentProject.IsEngineInternalProject || CurrentProject.Universe.Info.ProjectType != ProjectType.Artwork) {
			long builtInSheetModDate = Util.GetFileModifyDate(Universe.BuiltIn.GameSheetPath);
			if (builtInSheetModDate != Util.GetFileModifyDate(CurrentProject.Universe.BuiltInSheetPath)) {
				var engineSheet = new Sheet();
				if (engineSheet.LoadFromDisk(Universe.BuiltIn.GameSheetPath)) {
					engineSheet.RemoveAllAtlasAndSpritesInsideExcept("BuiltIn".AngeHash());
					if (engineSheet.Atlas.Count > 0) {
						engineSheet.SaveToDisk(CurrentProject.Universe.BuiltInSheetPath);
						Util.SetFileModifyDate(CurrentProject.Universe.BuiltInSheetPath, builtInSheetModDate);
						Debug.Log("Built-in Sheet Updated");
					}
				}
			}
		}

		// Change Check
		CheckScriptChanged();
		UpdateDllLibraryFiles();
		CheckResourceChanged();
		ReloadRenderingSheet();

		// Sync Engine Version
		if (Universe.BuiltInInfo.EngineBuildVersion != CurrentProject.Universe.Info.EngineBuildVersion) {
			CurrentProject.Universe.Info.EngineBuildVersion = Universe.BuiltInInfo.EngineBuildVersion;
			JsonUtil.SaveJsonToPath(CurrentProject.Universe.Info, CurrentProject.Universe.InfoPath, true);
		}

		// Rebuild
		Util.DeleteFolder(CurrentProject.BuildPath);
		RequireBackgroundBuildDate = 0;
		HasCompileError = false;

		// Project Type
		var pType = CurrentProject.Universe.Info.ProjectType;
		switch (pType) {

			case ProjectType.Game:
				// Build
				EngineUtil.BuildAngeliaProjectInBackground(CurrentProject, RequireBackgroundBuildDate);
				// Backup
				long timeNow = Util.GetLongTime();
				if (EngineSetting.BackupSaving.Value && !Util.IsSameDay(CurrentProject.Universe.Info.LastBackupSavingDate, timeNow)) {
					EngineUtil.BackupSaving(CurrentProject);
					CurrentProject.Universe.Info.LastBackupSavingDate = timeNow;
				}
				break;

			case ProjectType.Artwork:
			case ProjectType.EngineTheme:
				// Fix First Window
				if (IsWindowIgnoredForProject(AllWindows[CurrentWindowIndex.Clamp(0, AllWindows.Length - 1)], pType)) {
					SetCurrentWindow<PixelEditor>();
				}
				break;

			default:
				throw new System.NotImplementedException();
		}

		// Final
		Input.UseAllHoldingKeys();
		Input.UseAllMouseKey();

	}


	private void TryCloseProject () {
		if (CheckAnyEditorDirty()) {
			GenericDialogUI.SpawnDialog_Button(
				QUIT_MSG,
				BuiltInText.UI_SAVE, SaveAndClose,
				BuiltInText.UI_DONT_SAVE, Close,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
			GenericDialogUI.SetItemTint(Color32.GREEN_BETTER);
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
			if (Instance.CurrentProject != null) {
				foreach (var pData in Instance.Projects) {
					if (Util.IsSamePath(pData.Path, Instance.CurrentProject.ProjectPath)) {
						pData.Name = Instance.CurrentProject.Universe.Info.ProductName;
						break;
					}
				}
			}
			Instance.CurrentProject = null;
			Instance.CurrentProjectData = null;
			LastOpenProject.Value = "";
			foreach (var ui in Instance.AllWindows) {
				ui.Active = false;
				ui.OnInactivated();
			}
			foreach (var ui in Instance.AllGenericUIs) {
				ui.Active = false;
				ui.OnInactivated();
			}
			LanguageEditor.Instance.SetCurrentProject(null);
			PixelEditor.Instance.SetCurrentProject(null);
			ProjectEditor.Instance.SetCurrentProject(null);
			SettingWindow.Instance.SetCurrentProject(null);
			GameEditor.Instance.CleanDirty();
			GameEditor.Instance.SetCurrentProject(null);
			PackageManager.Instance.SetCurrentProject(null);
			ConsoleWindow.Instance.SetCurrentProject(null);
			Game.SetWindowTitle("AngeliA Engine");
			Game.SetWindowIcon("WindowIcon".AngeHash());
			Instance.Transceiver.RespondMessage.Reset(clearLastRendering: true);
			Instance.Transceiver.Abort();
			Input.UseAllHoldingKeys();
			Input.UseAllMouseKey();
		}
	}


	private bool CheckAnyEditorDirty () {
		foreach (var window in AllWindows) {
			if (window is not GameEditor && window.IsDirty) return true;
		}
		return false;
	}


	private void SetCurrentWindow<W> (bool forceChange = false) where W : WindowUI {
		for (int i = 0; i < AllWindows.Length; i++) {
			if (AllWindows[i] is W) {
				SetCurrentWindow(i, forceChange);
				return;
			}
		}
	}
	private void SetCurrentWindow (int index, bool forceChange = false) {
		index = index.Clamp(0, AllWindows.Length - 1);
		CurrentWindow = AllWindows[index];
		if (!forceChange && index == CurrentWindowIndex) return;
		CurrentWindowRequireRigGame = AllWindows[index] is GameEditor;
		if (CurrentWindowRequireRigGame) {
			// Rig Window
			if (Transceiver.RigProcessRunning) Transceiver.CallingMessage.RequireFocusInvoke();
			if (RequireBgmForRiggedGame) {
				Game.UnpauseMusic();
			}
		} else {
			// Normal Window
			if (Transceiver.RigProcessRunning) Transceiver.CallingMessage.RequireLostFocusInvoke();
			ResetViewRect(remapAllRenderingCells: true);
			Game.MusicVolume = 1000;
			Game.SoundVolume = 1000;
			Game.PauseMusic();
		}
		CurrentWindowIndex = index;
		LastOpenedWindowIndex.Value = index;
		if (!CurrentWindowRequireRigGame) {
			Game.HideDoodle();
		}
		Game.StopAllSounds();
	}


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
		int defHeight = Universe.BuiltInInfo.DefaultViewHeight;
		Stage.SetViewRectImmediately(
			new IRect(0, 0, Universe.BuiltInInfo.ViewRatio * defHeight / 1000, defHeight),
			remapAllRenderingCells
		);
		WindowUI.ForceWindowRect(Renderer.CameraRect.Shrink(GetEngineLeftBarWidth(out _), 0, 0, 0));
	}


	private int GetEngineLeftBarWidth (out int contentPadding) {
		contentPadding = GUI.Unify(8);
		return (FullsizeMenu.Value ? GUI.Unify(WINDOW_BAR_WIDTH_FULL) : GUI.Unify(WINDOW_BAR_WIDTH_NORMAL)) + contentPadding;
	}


	private void LoadTheme (string path, bool notification = true) {
		bool loaded;
		if (path != "" && Util.FileExists(path) && ThemeSheet.LoadFromDisk(path)) {
			// Load Custom Theme
			ThemeSkin.Name = Util.GetDisplayName(Util.GetNameWithoutExtension(path));
			ThemeSkin.LoadColorFromSheet(ThemeSheet);
			CurrentThemeName.Value = Util.GetNameWithoutExtension(path);
			// Load Built-In from Engine
			var builtInSheet = new Sheet();
			if (builtInSheet.LoadFromDisk(Universe.BuiltIn.GameSheetPath)) {
				builtInSheet.RemoveAllAtlasAndSpritesInsideExcept("BuiltIn".AngeHash());
				if (builtInSheet.Atlas.Count > 0) {
					ThemeSheet.CombineSheet(builtInSheet);
				}
			}
			loaded = true;
		} else {
			// Load Built-in Theme
			ThemeSheet.Clear();
			ThemeSkin.Name = "Built-in";
			ThemeSkin.LoadColorFromSkin(GUISkin.Default);
			CurrentThemeName.Value = "";
			loaded = false;
		}
		// Notify
		if (notification) {
			NotificationFlash = Game.GlobalFrame < NotificationStartFrame + NOTIFY_DURATION;
			NotificationStartFrame = Game.GlobalFrame;
			NotificationContent = loaded ? NOTI_THEME_LOADED : NOTI_THEME_NOT_LOADED;
			NotificationSubContent = loaded ? ThemeSkin.Name : "";
		}
	}


	// Change Check
	private void CheckScriptChanged () {
		if (EngineUtil.BuildingProjectInBackground) return;
		long dllModifyDate = EngineUtil.GetBuildLibraryModifyDate(CurrentProject);
		long srcModifyDate = EngineUtil.GetScriptModifyDate(CurrentProject);
		if (srcModifyDate > dllModifyDate && srcModifyDate > EngineUtil.LastBackgroundBuildModifyDate) {
			GameEditor.Instance.SetDirty();
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
		if (ProjectEditor.Instance.IsIconFileModified()) {
			ProjectEditor.Instance.ReloadIconUI();
		}
	}


	private void UpdateDllLibraryFiles () {

		if (CurrentProject == null) return;

		// Framework Dll Files
		Util.UpdateFile(EngineUtil.TemplateFrameworkDll_Debug, Util.CombinePaths(CurrentProject.DllLibPath_Debug, "AngeliA Framework.dll"));
		Util.UpdateFile(EngineUtil.TemplateFrameworkDll_Release, Util.CombinePaths(CurrentProject.DllLibPath_Release, "AngeliA Framework.dll"));

		// Raylib Dll Files for Entry
		Util.UpdateFile(EngineUtil.AngeRaylibDllPath, Util.CombinePaths(CurrentProject.LocalEntryRoot, "AngeliA Raylib.dll"));

		// Sync Package Dll Files
		PackageManager.Instance.SyncPackageWithProject(CurrentProject, syncDll: true, syncSheet: false);

	}


	private void ReloadRenderingSheet () {
		if (CurrentProject == null) return;
		if (!RenderingSheet.LoadFromDisk(CurrentProject.Universe.GameSheetPath)) return;
		PackageManager.Instance.SyncPackageWithProject(CurrentProject, syncDll: false, syncSheet: true);
		RenderingSheet.CombineAllSheetInFolder(
			CurrentProject.Universe.SheetRoot,
			topOnly: false,
			ignoreNameWithExtension: Util.GetNameWithExtension(CurrentProject.Universe.GameSheetPath)
		);
	}


	private bool IsWindowIgnoredForProject (WindowUI window, ProjectType type) {
		if (type == ProjectType.Game) return false;
#if DEBUG
		if (window is LanguageEditor) return !CurrentProject.IsEngineInternalProject;
#endif
		if (window is GameEditor) return true;
		return false;
	}


	#endregion




}
