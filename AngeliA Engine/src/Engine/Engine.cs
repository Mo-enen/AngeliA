global using Debug = AngeliA.Debug;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliA;

[assembly: ToolApplication]
[assembly: DisablePause]

namespace AngeliaEngine;

public partial class Engine {




	#region --- VAR ---


	// Const
	private const int NOTIFY_DURATION = 120;
	private const int WINDOW_BAR_WIDTH_FULL = 160;
	private const int WINDOW_BAR_WIDTH_NORMAL = 42;

	private static readonly SpriteCode UI_ENGINE_BAR = "UI.EngineSideBar";
	private static readonly SpriteCode UI_ENGINE_BAR_BTN = "UI.EngineSideButton";
	private static readonly SpriteCode UI_ENGINE_BAR_BTN_HIGHLIGHT = "UI.EngineSideButtonHighlight";
	private static readonly SpriteCode UI_ENGINE_BAR_BTN_WARNING = "UI.EngineSideButtonWarning";
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
	private static readonly LanguageCode FILE_DROP_MSG_PNG = ("UI.FileDropMsg.Png", "Import image {0} as:");
	private static readonly LanguageCode FILE_DROP_MSG_AUDIO = ("UI.FileDropMsg.Audio", "Import audio file {0} as:");
	private static readonly LanguageCode FILE_DROP_LABEL_ICON = ("UI.FileDropLabel.Icon", "Icon");
	private static readonly LanguageCode FILE_DROP_LABEL_ART = ("UI.FileDropLabel.Art", "Artwork");
	private static readonly LanguageCode FILE_DROP_LABEL_MUSIC = ("UI.FileDropLabel.Music", "Music");
	private static readonly LanguageCode FILE_DROP_LABEL_SOUND = ("UI.FileDropLabel.Sound", "Sound");

	// Data
	private static Engine Instance = null;
	private readonly GUIStyle TooltipStyle = new(GUI.Skin.SmallLabel);
	private readonly GUIStyle NotificationLabelStyle = new(GUI.Skin.AutoLabel) { Alignment = Alignment.BottomRight, };
	private readonly GUIStyle NotificationSubLabelStyle = new(GUI.Skin.AutoLabel) { Alignment = Alignment.BottomRight, };
	private readonly List<ProjectData> Projects = new();
	private readonly Sheet ThemeSheet = new(ignoreGroups: true, ignoreSpriteWithIgnoreTag: true);
	private readonly GUISkin ThemeSkin = new() { Name = "Built-in" };
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
	private int ConsoleWindowIndex = 0;
	private int LastNotInteractableFrame = int.MinValue;
	private int IgnoreFileDropFrame = int.MinValue;
	private bool NotificationFlash = false;
	private string ToolLabel = null;
	private string NotificationContent = null;
	private string NotificationSubContent = null;
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


	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {
		var engine = new Engine();
		Instance = engine;
		engine.AllWindows = new WindowUI[]{
			new RiggedMapEditor(),
			new PixelEditor(engine.AllRigCharacterNames),
			new CharacterAnimationEditorWindow(engine.AllRigCharacterNames),
			new LanguageEditor(),
			new ConsoleWindow(),
			new ProjectEditor(),
			new SettingWindow(EngineSetting.BackgroundColor.Value.ToColorF(), EngineSetting.BackgroundColor.DefaultValue),
		};
		engine.AllGenericUIs = new EntityUI[] {
			new GenericPopupUI(),
			new GenericDialogUI(),
			new FileBrowserUI(),
		};
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

		CheckEngineAsepriteChanged();

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

		// Engine Window
		if (Maximize.Value) {
			Game.IsWindowMaximized = Maximize.Value;
		} else {
			Game.SetWindowPosition(WindowPositionX.Value, WindowPositionY.Value);
			Game.SetWindowSize(WindowSizeX.Value, WindowSizeY.Value);
		}
		Game.SetEventWaiting(false);
		Game.ProcedureAudioVolume = 1000;

		// Rig Start Setting
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
			if (win is ConsoleWindow) ConsoleWindowIndex = i;
			if (win is CharacterAnimationEditorWindow) CharAniEditorWindowIndex = i;
		}

		SetCurrentWindowIndex(LastOpenedWindowIndex.Value, forceChange: true);
		ResetViewRect();

		// Theme
		ThemeSheetIndex = Renderer.AddAltSheet(ThemeSheet);

	}


	[OnGameInitializeLater(5000)]
	internal static void OpenProjectOnStart () {
		if (
			EngineSetting.OpenLastProjectOnStart.Value &&
			Instance.Projects.Any(data => data.Path == LastOpenProject.Value)
		) {
			Instance.OpenProject(LastOpenProject.Value);
		}
	}


	// Misc
	[OnFileDropped]
	internal static void OnFileDropped (string path) {

		var project = Instance.CurrentProject;
		if (Game.PauselessFrame <= Instance.IgnoreFileDropFrame) return;

		Instance.DroppingFilePath = path;

		string ex = Util.GetExtensionWithDot(path);
		switch (ex) {
			case "":
				if (project != null) break;
				Instance.AddExistsProjectAt(path);
				break;
			case ".ase":
			case ".aseprite":
				if (project == null) break;
				PixelEditor.ImportAtlasFromFile(path);
				Instance.IgnoreFileDropFrame = Game.PauselessFrame;
				break;
			case ".png":
				if (project == null) break;
				GenericDialogUI.SpawnDialog_Button(
					string.Format(FILE_DROP_MSG_PNG, Util.GetNameWithExtension(path)),
					FILE_DROP_LABEL_ICON, ImportForIcon,
					FILE_DROP_LABEL_ART, ImportForArtwork,
					BuiltInText.UI_CANCEL, Const.EmptyMethod
				);
				Instance.IgnoreFileDropFrame = Game.PauselessFrame;
				break;
			case ".ico":
				if (project == null) break;
				Util.CopyFile(path, project.IconPath);
				ProjectEditor.Instance.ReloadIconUI();
				Instance.IgnoreFileDropFrame = Game.PauselessFrame;
				break;
			case ".wav":
			case ".mp3":
			case ".ogg":
				if (project == null) break;
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


	[OnGameFocused]
	internal static void OnGameFocused () {
		Instance?.CheckScriptChanged();
		Instance?.RefreshProjectCache();
		Instance?.CheckResourceChanged();
		Instance?.CheckFrameworkDllChanged();
		Instance?.CheckDialogChanged();
		Instance?.CheckEngineAsepriteChanged();
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
	}


	// Update
	[OnGameUpdateLater(-4096)]
	internal static void OnGUI () {

		GUI.Enable = true;
		if (!Instance.CurrentWindowRequireRigGame) {
			Sky.ForceSkyboxTint(GUI.Skin.Background);
		}

		using var _ = new SheetIndexScope(Instance.ThemeSheet.Sprites.Count > 0 ? Instance.ThemeSheetIndex : -1);
		using var __ = new GUISkinScope(Instance.ThemeSkin);

		Instance.OnGUI_Hint();
		if (Instance.CurrentProject == null) {
			Instance.OnGUI_Hub();
		} else {
			Instance.OnGUI_Engine();
			Instance.OnGUI_Window();
			Instance.OnGUI_Hotkey();
		}
		Instance.OnGUI_RiggedGame();
		Instance.OnGUI_Requirement();

	}


	// GUI Window
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
			var tipRect = ToolLabelRect.EdgeOutside(Direction4.Down, GUI.Unify(24)).Shift(
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


	private void OnGUI_Engine () {

		if (CurrentProject == null) return;

		// Window
		int barWidth = GetEngineLeftBarWidth(out int contentPadding);
		var barRect = Renderer.CameraRect.Edge(Direction4.Left, barWidth);
		var mousePos = Input.MouseGlobalPosition;
		bool mousePress = Input.MouseLeftButtonDown;
		var rect = barRect.Edge(Direction4.Up, GUI.Unify(42));

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
		GUI.Interactable = interactable;

		// UI
		using (new UILayerScope()) {

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
			GUI.Icon(menuRect.Edge(Direction4.Left, menuRect.height), BuiltInSprite.ICON_MENU);
			rect.y -= rect.height;

			// Window Tabs
			for (int index = 0; index < AllWindows.Length; index++) {

				var window = AllWindows[index];

				bool selecting = index == CurrentWindowIndex;
				bool hovering = GUI.Enable && rect.Contains(mousePos);

				// Cursor
				if (!selecting && hovering) Cursor.SetCursorAsHand();

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
				var iconRect = contentRect.Edge(Direction4.Left, iconSize);
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
					barRect.Edge(Direction4.Down, rect.height),
					BuiltInText.UI_BACK, GUI.Skin.SmallCenterLabelButton
				)) {
					TryCloseProject();
				}
			} else {
				if (GUI.Button(
					barRect.Edge(Direction4.Down, rect.height),
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
			Game.IgnoreGizmos(1);
		}
		CharacterAnimationEditorWindow.Instance.SheetIndex = PixelEditor.Instance.SheetIndex;

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
			ConsoleWindow.Instance.Clear();
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
		if (EngineSetting.Hotkey_Window_CharAni.Value.Down()) {
			SetCurrentWindowIndex<CharacterAnimationEditorWindow>();
		}
		if (EngineSetting.Hotkey_Window_Language.Value.Down()) {
			SetCurrentWindowIndex<LanguageEditor>();
		}
		if (EngineSetting.Hotkey_Window_Console.Value.Down()) {
			SetCurrentWindowIndex<ConsoleWindow>();
		}
		if (EngineSetting.Hotkey_Window_Project.Value.Down()) {
			SetCurrentWindowIndex<ProjectEditor>();
		}
		if (EngineSetting.Hotkey_Window_Setting.Value.Down()) {
			SetCurrentWindowIndex<SettingWindow>();
		}
	}


	private void OnGUI_Requirement () {

		if (EngineUtil.BuildingProjectInBackground) return;

		// Theme
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
		CharacterAnimationEditorWindow.Instance.SetCurrentProject(CurrentProject);
		RiggedMapEditor.Instance.CleanDirty();
		RiggedMapEditor.Instance.SetCurrentProject(CurrentProject);
		ConsoleWindow.Instance.RequireCodeAnalysis = -1;

		// Audio
		Game.SyncAudioPool(Universe.BuiltIn.UniverseRoot, CurrentProject.UniversePath);

		// Font
		Game.UnloadFontsFromPool(ignoreBuiltIn: true);
		Game.LoadFontsIntoPool(CurrentProject.Universe.FontRoot, builtIn: false);

		// Change Check
		CheckScriptChanged();
		CheckFrameworkDllChanged();
		CheckDialogChanged();
		CheckResourceChanged();
		ReloadCharacterNames();

		// Sync Engine Version
		if (Universe.BuiltIn.Info.EngineBuildVersion != CurrentProject.Universe.Info.EngineBuildVersion) {
			CurrentProject.Universe.Info.EngineBuildVersion = Universe.BuiltIn.Info.EngineBuildVersion;
			JsonUtil.SaveJsonToPath(CurrentProject.Universe.Info, CurrentProject.Universe.InfoPath, true);
		}

		// Rebuild
		Util.DeleteFolder(CurrentProject.BuildPath);
		EngineUtil.BuildAngeliaProjectInBackground(CurrentProject, RequireBackgroundBuildDate);
		RequireBackgroundBuildDate = 0;
		HasCompileError = false;

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
			CharacterAnimationEditorWindow.Instance.SetCurrentProject(null);
			RiggedMapEditor.Instance.CleanDirty();
			RiggedMapEditor.Instance.SetCurrentProject(null);
			Game.SetWindowTitle("AngeliA Engine");
			Instance.Transceiver.RespondMessage.Reset(clearLastRendering: true);
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
			// To Char Ani Editor
			if (index == CharAniEditorWindowIndex) {
				if (PixelEditor.Instance.IsGroupDataDirty) {
					PixelEditor.Instance.IsGroupDataDirty = false;
					PixelEditor.Sheet.CalculateExtraData();
				}
			}
		}
		CurrentWindowIndex = index;
		LastOpenedWindowIndex.Value = index;
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
		// Ase
		SheetUtil.RecreateSheetIfArtworkModified(CurrentProject.Universe.SheetPath, CurrentProject.Universe.AsepriteRoot);
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


	private void CheckFrameworkDllChanged () {

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

	}


	private void CheckDialogChanged () => EngineUtil.TryCompileDialogueFiles(Universe.BuiltIn.EditableConversationRoot, Universe.BuiltIn.ConversationRoot, forceCompile: false);


	private void CheckEngineAsepriteChanged () {
		bool recreated = SheetUtil.RecreateSheetIfArtworkModified(
			Universe.BuiltIn.SheetPath,
			Universe.BuiltIn.AsepriteRoot
		);
		if (recreated) {
			Renderer.LoadMainSheet();
		}
	}


	#endregion




}
