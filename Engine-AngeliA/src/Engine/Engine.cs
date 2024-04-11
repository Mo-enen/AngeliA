using System.Linq;
using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

[RequireSpriteFromField]
[RequireLanguageFromField]
internal class Engine {




	#region --- VAR ---


	// SUB
	private enum WindowMode { Window, ConfirmQuit, }

	// Const
	private static int WINDOW_UI_COUNT = 2;
	private const int HUB_PANEL_WIDTH = 360;
	private static readonly SpriteCode UI_WINDOW_BG = "UI.MainBG";
	private static readonly SpriteCode PANEL_BG = "UI.HubPanel";
	private static readonly SpriteCode PROJECT_ICON = "UI.Project";
	private static readonly LanguageCode BTN_CREATE = ("Hub.Create", "Create New Project");
	private static readonly LanguageCode BTN_ADD = ("Hub.Add", "Add Existing Project");
	private static readonly LanguageCode CREATE_PRO_TITLE = ("UI.CreateProjectTitle", "New Project");
	private static readonly LanguageCode ADD_PRO_TITLE = ("UI.AddProjectTitle", "Add Existing Project");
	private static readonly LanguageCode QUIT_MSG = ("UI.QuitMessage", "Quit editor?");
	private static readonly LanguageCode DELETE_PROJECT_MSG = ("UI.DeleteProjectMsg", "Remove project {0}? This will NOT delete files in the disk.");
	private static readonly LanguageCode[] UI_TITLES = {
		("", ""),
		("", ""),
		("", ""),
		("Title.Pixel", "Artwork"),
		("Title.Language", "Language"),
		("Title.Setting", "Setting"),
	};
	private static readonly EntityUI[] ALL_UI = {
		new GenericPopupUI() { Active = false, },
		new GenericDialogUI() { Active = false, },
		new FileBrowserUI() { Active = false, },
		new PixelEditor(),
		new LanguageEditor(ignoreRequirements:true),
		new SettingWindow(),
	};

	// Api
	public static Project CurrentProject { get; private set; } = null;
	public static bool OpenLastProjectOnStart {
		get => Setting.OpenLastProjectOnStart;
		set => Setting.OpenLastProjectOnStart = value;
	}

	// Data
	private static readonly GUIStyle ConfirmMsgStyle = new(GUISkin.CenterLabel) { CharSize = 18 };
	private static readonly GUIStyle ConfirmBtnStyle = new(GUISkin.DarkButton) { CharSize = 18 };
	private static EngineSetting Setting;
	private static WindowMode CurrentWindowMode;
	private static bool SettingInitialized = false;
	private static int CurrentWindowIndex = 0;
	private static int HubPanelScroll = 0;
	private static int CurrentProjectMenuIndex = -1;


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitialize () {
		Setting = JsonUtil.LoadOrCreateJson<EngineSetting>(AngePath.PersistentDataPath);
		Setting.RefreshProjectFileExistsCache();
		SwitchWindowMode(WindowMode.Window);
		ALL_UI.ForEach<WindowUI>(win => win.OnActivated());
		WINDOW_UI_COUNT = ALL_UI.Count(ui => ui is WindowUI);
		Game.SetEventWaiting(false);
		if (Setting.OpenLastProjectOnStart) {
			OpenProject(Setting.LastOpenProject);
		}
	}


	[OnGameTryingToQuit]
	internal static bool OnGameTryingToQuit () {
		if (CurrentWindowMode != WindowMode.ConfirmQuit) {
			SwitchWindowMode(WindowMode.ConfirmQuit);
			return false;
		} else {
			return true;
		}
	}


	[OnGameQuitting]
	internal static void OnGameQuitting () {
		JsonUtil.SaveJson(Setting, AngePath.PersistentDataPath, prettyPrint: true);
		ALL_UI.ForEach<WindowUI>(win => win.OnInactivated());
	}


	[OnGameFocused]
	internal static void OnGameFocused () {
		Setting.RefreshProjectFileExistsCache();
	}


	// GUI
	[OnGameUpdateLater(-4096)]
	internal static void OnGUI () {

		if (Game.IsWindowMinimized) return;

		// On GUI
		GUI.Enable = true;
		GUI.UnifyBasedOnMonitor = true;
		Sky.ForceSkyboxTint(new Color32(38, 38, 38, 255));
		switch (CurrentWindowMode) {
			case WindowMode.Window:
				if (CurrentProject == null) {
					OnGUI_Hub();
				} else {
					OnGUI_Window();
				}
				break;
			case WindowMode.ConfirmQuit:
				OnGUI_ConfirmQuit();
				break;
		}

		// Clamp Window Pos
		if (Game.IsWindowDecorated) {
			var windowPos = Game.GetWindowPosition();
			if (windowPos.y < 24) {
				Game.SetWindowPosition(windowPos.x, 24);
			}
		}

	}


	// Window
	private static void OnGUI_Hub () {

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
		var browser = FileBrowserUI.Instance;
		if (browser.Active) {
			browser.Width = GUI.Unify(800);
			browser.Height = GUI.Unify(600);
		}

		using (Scope.RendererLayerUI()) {

			// --- BG ---
			int bodyBorder = GUI.Unify(6);
			Renderer.Draw_9Slice(
				UI_WINDOW_BG, cameraRect,
				bodyBorder, bodyBorder, bodyBorder, bodyBorder
			);

			// --- Panel ---
			{
				var panelRect = Setting.Projects.Count > 0 ?
					cameraRect.EdgeInside(Direction4.Left, hubPanelWidth) :
					new IRect(cameraRect.x + (cameraRect.width - hubPanelWidth) / 2, cameraRect.y, hubPanelWidth, cameraRect.height);
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
			if (Setting.Projects.Count > 0) {

				int border = GUI.Unify(8);
				int padding = GUI.Unify(8);
				int itemHeight = GUI.Unify(52);
				var contentRect = cameraRect.EdgeInside(Direction4.Right, cameraRect.width - hubPanelWidth).Shrink(padding);
				var projects = Setting.Projects;

				// BG
				Renderer.Draw_9Slice(PANEL_BG, contentRect, border, border, border, border, Color32.WHITE, z: 0);

				// Project List
				using (var scroll = Scope.GUIScroll(contentRect, HubPanelScroll, 0, Util.Max(0, projects.Count * itemHeight - contentRect.height))) {
					HubPanelScroll = scroll.ScrollPosition.y;

					var STEP_TINT = new Color32(42, 42, 42, 255);
					var rect = contentRect.Shrink(border).EdgeInside(Direction4.Up, itemHeight);
					bool stepTint = false;

					for (int i = 0; i < projects.Count; i++) {
						string projectPath = projects[i].Path;
						bool folderExists = projects[i].FolderExists;
						var itemContentRect = rect.Shrink(padding);

						// Step Tint
						if (stepTint) Renderer.DrawPixel(rect, STEP_TINT);
						stepTint = !stepTint;

						using (Scope.GUIEnable(folderExists)) {

							// Red Highlight
							if (!folderExists && rect.MouseInside()) {
								Renderer.DrawPixel(rect, Color32.RED.WithNewA(32));
							}

							// Button
							if (GUI.Button(rect, 0, GUISkin.HighlightPixel)) {
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
							GUI.Label(
								itemContentRect.Shrink(itemContentRect.height + padding, 0, itemContentRect.height / 2, 0),
								Util.GetNameWithoutExtension(projectPath),
								GUISkin.SmallLabel
							);

							// Path
							GUI.Label(
								itemContentRect.Shrink(itemContentRect.height + padding, 0, 0, itemContentRect.height / 2),
								projectPath,
								GUISkin.SmallGreyLabel
							);

							// Click
							if (GUI.Enable && rect.MouseInside()) {
								// Menu
								if (folderExists && Input.MouseRightButtonDown) {
									Input.UseAllMouseKey();
									OpenHubItemPopup(i);
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
						}

						rect.SlideDown();
					}
				}
			}
		}

		// Final
		IWindowEntityUI.ClipTextForAllUI(ALL_UI, ALL_UI.Length);

	}


	private static void OnGUI_Window () {

		// Window
		int contentPadding = GUI.Unify(8);
		int barWidth = Setting.FullsizeMenu ? GUI.Unify(200) : GUI.Unify(42) + contentPadding;
		int bodyBorder = GUI.Unify(6);
		var cameraRect = Renderer.CameraRect;
		int windowLen = CurrentProject == null ? 1 : WINDOW_UI_COUNT;
		var barRect = cameraRect.EdgeInside(Direction4.Left, barWidth);
		var mousePos = Input.MouseGlobalPosition;
		bool mousePress = Input.MouseLeftButtonDown;
		var rect = barRect.EdgeInside(Direction4.Up, GUI.Unify(42));
		bool interactable = true;

		foreach (var ui in ALL_UI) {
			if (ui is not WindowUI && ui.Active) {
				interactable = false;
				break;
			}
		}

		using (Scope.GUIEnable(interactable))
		using (Scope.RendererLayerUI()) {

			// Tab BG
			Renderer.DrawPixel(barRect, Color32.GREY_12);

			// Menu
			{
				var menuRect = rect.Shrink(contentPadding, contentPadding, contentPadding / 2, contentPadding / 2);

				// Menu Button
				if (GUI.BlankButton(rect, out _)) {
					Setting.FullsizeMenu = !Setting.FullsizeMenu;
				}

				// Menu Icon
				GUI.Icon(menuRect.EdgeInside(Direction4.Left, menuRect.height), BuiltInSprite.ICON_MENU);

				// Menu Label
				if (Setting.FullsizeMenu) {
					GUI.Label(menuRect.Shrink(menuRect.height + contentPadding, 0, 0, 0), BuiltInText.UI_MENU, GUISkin.SmallLabel);
				}
				rect.y -= rect.height;
			}

			// Window Tabs
			CurrentWindowIndex = CurrentWindowIndex.Clamp(0, windowLen - 1);
			int index = 0;
			for (int i = 0; i < ALL_UI.Length; i++) {

				if (ALL_UI[i] is not WindowUI window) continue;
				if (index >= windowLen) break;

				bool selecting = index == CurrentWindowIndex;
				bool hovering = rect.Contains(mousePos);

				// Cursor
				if (GUI.Enable && !selecting && hovering) Cursor.SetCursorAsHand();

				// Body
				Renderer.Draw_9Slice(
					Const.PIXEL, rect,
					bodyBorder, bodyBorder, bodyBorder, bodyBorder,
					selecting ? Color32.GREY_32 : Color32.GREY_12
				);
				var contentRect = rect.Shrink(contentPadding, contentPadding, contentPadding / 2, contentPadding / 2);

				// Icon
				int iconSize = contentRect.height;
				Renderer.Draw(window.TypeID, contentRect.EdgeInside(Direction4.Left, iconSize));

				// Label
				if (Setting.FullsizeMenu) {
					GUI.Label(
						contentRect.Shrink(iconSize + contentPadding, 0, 0, 0),
						UI_TITLES[i], GUISkin.SmallLabel
					);
				}

				// Click
				if (GUI.Enable && mousePress && hovering) CurrentWindowIndex = index;

				// Next
				rect.SlideDown();
				index++;
			}

			// Back to Hub
			if (Setting.FullsizeMenu) {
				if (GUI.Button(
					barRect.EdgeInside(Direction4.Down, rect.height),
					BuiltInText.UI_BACK, GUISkin.SmallCenterLabelButton
				)) {
					CloseProject();
				}
			} else {
				if (GUI.Button(
					barRect.EdgeInside(Direction4.Down, rect.height),
					BuiltInSprite.ICON_BACK, GUISkin.IconButton
				)) {
					CloseProject();
				}
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
		foreach (var ui in ALL_UI) {
			if (!ui.Active) continue;
			GUI.Enable = interactable || ui is not WindowUI;
			ui.FirstUpdate();
		}
		foreach (var ui in ALL_UI) {
			if (!ui.Active) continue;
			GUI.Enable = interactable || ui is not WindowUI;
			ui.BeforeUpdate();
		}
		foreach (var ui in ALL_UI) {
			if (!ui.Active) continue;
			GUI.Enable = interactable || ui is not WindowUI;
			ui.Update();
		}
		foreach (var ui in ALL_UI) {
			if (!ui.Active) continue;
			GUI.Enable = interactable || ui is not WindowUI;
			ui.LateUpdate();
		}

		// Final
		IWindowEntityUI.ClipTextForAllUI(ALL_UI, ALL_UI.Length);

	}


	// Confirm Quit
	private static void OnGUI_ConfirmQuit () {

		var cameraRect = Renderer.CameraRect;
		int buttonHeight = GUI.Unify(64);
		int btnPadding = GUI.Unify(8);

		// MSG 
		GUI.Label(
			cameraRect.EdgeInside(Direction4.Up, cameraRect.height - buttonHeight).Shrink(GUI.Unify(8)),
			QUIT_MSG, ConfirmMsgStyle
		);

		// Buttons 
		var rect = new IRect(cameraRect.x, 0, cameraRect.width / 2, buttonHeight);
		using (Scope.GUIBodyColor(Color32.RED_BETTER)) {
			if (GUI.Button(rect.Shrink(btnPadding), BuiltInText.UI_QUIT, ConfirmBtnStyle)) {
				Game.QuitApplication();
			}
		}

		rect.x += rect.width;
		if (GUI.Button(rect.Shrink(btnPadding), BuiltInText.UI_CANCEL, ConfirmBtnStyle)) {
			SwitchWindowMode(WindowMode.Window);
		}

	}


	#endregion




	#region --- LGC ---


	private static void SwitchWindowMode (WindowMode newMode) {

		// Cache
		if (SettingInitialized && CurrentWindowMode == WindowMode.Window) {
			var windowPos = Game.GetWindowPosition();
			Setting.Maximize = Game.IsWindowMaximized;
			Setting.WindowSizeX = Game.ScreenWidth;
			Setting.WindowSizeY = Game.ScreenHeight;
			Setting.WindowPositionX = windowPos.x;
			Setting.WindowPositionY = windowPos.y;
		}

		Input.UseAllHoldingKeys();

		// Set
		int targetWindowWidth = Game.ScreenWidth;
		int targetWindowHeight = Game.ScreenHeight;
		int minWindowSize = Game.MonitorHeight / 5;
		switch (newMode) {
			case WindowMode.Window: {
				// Window
				Game.IsWindowDecorated = true;
				Game.IsWindowTopmost = false;
				Game.IsWindowResizable = true;
				if (Setting.Maximize) {
					Game.SetWindowPosition(0, 0);
					targetWindowWidth = Game.MonitorWidth;
					targetWindowHeight = Game.MonitorHeight;
					Game.IsWindowMaximized = true;
				} else {
					Game.SetWindowPosition(Setting.WindowPositionX, Setting.WindowPositionY.GreaterOrEquel(24));
					targetWindowWidth = Setting.WindowSizeX;
					targetWindowHeight = Setting.WindowSizeY;
					Game.IsWindowMaximized = false;
				}
				break;
			}
			case WindowMode.ConfirmQuit: {
				// ConfirmQuit
				Game.IsWindowDecorated = true;
				Game.IsWindowTopmost = false;
				Game.IsWindowResizable = false;
				int monitor = Game.CurrentMonitor;
				int monitorWidth = Game.GetMonitorWidth(monitor);
				int monitorHeight = Game.GetMonitorHeight(monitor);
				int width = monitorHeight * 5 / 10;
				int height = monitorHeight * 2 / 10;
				targetWindowWidth = width;
				targetWindowHeight = height;
				Game.SetWindowPosition((monitorWidth - width) / 2, (monitorHeight - height) / 2);
				break;
			}
		}
		Game.SetWindowMinSize(minWindowSize);
		Game.SetWindowSize(
			targetWindowWidth.GreaterOrEquel(minWindowSize),
			targetWindowHeight.GreaterOrEquel(minWindowSize)
		);
		CurrentWindowMode = newMode;
		SettingInitialized = true;
	}


	private static void OpenHubItemPopup (int index) {
		CurrentProjectMenuIndex = index;
		GenericPopupUI.BeginPopup();
		GenericPopupUI.AddItem(BuiltInText.UI_EXPLORE, OpenProjectInExplorer);
		GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteProjectConfirm);
	}


	private static void OpenProjectInExplorer () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= Setting.Projects.Count) return;
		string path = Setting.Projects[CurrentProjectMenuIndex].Path;
		if (Util.FolderExists(path)) {
			Game.OpenUrl(path);
		}
	}


	private static void DeleteProjectConfirm () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= Setting.Projects.Count) return;
		string name = Util.GetNameWithoutExtension(Setting.Projects[CurrentProjectMenuIndex].Path);
		string msg = string.Format(DELETE_PROJECT_MSG, name);
		GenericDialogUI.SpawnDialog_Button(
			msg,
			BuiltInText.UI_DELETE, DeleteProject,
			BuiltInText.UI_CANCEL, Const.EmptyMethod
		);
		GenericDialogUI.SetItemTint(Color32.RED_BETTER);
	}


	private static void DeleteProject () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= Setting.Projects.Count) return;
		Setting.Projects.RemoveAt(CurrentProjectMenuIndex);
	}


	// Workflow
	private static void OpenProject (string projectPath) {
		if (!Project.IsValidProjectPath(projectPath)) return;
		if (CurrentProject != null && projectPath == CurrentProject.ProjectPath) return;
		CurrentProject = new Project(projectPath);
		LanguageEditor.Instance.SetLanguageRoot(AngePath.GetLanguageRoot(CurrentProject.UniversePath));
		PixelEditor.Instance.LoadSheetFromDisk(AngePath.GetSheetPath(CurrentProject.UniversePath));
		Game.SetWindowTitle($"{Game.DisplayTitle} - {Util.GetNameWithoutExtension(projectPath)}");
		Setting.LastOpenProject = projectPath;
	}


	private static void CloseProject () {
		CurrentProject = null;
		foreach (var ui in ALL_UI) {
			ui.Active = false;
			if (ui is WindowUI) {
				ui.OnInactivated();
			}
		}
		LanguageEditor.Instance.SetLanguageRoot("");
		PixelEditor.Instance.LoadSheetFromDisk("");
		Game.SetWindowTitle(Game.DisplayTitle);
	}


	private static void CreateNewProjectAt (string path) {
		if (string.IsNullOrEmpty(path)) return;
		if (Project.CreateProjectToDisk(path)) {
			AddExistsProjectAt(path);
		}
	}


	private static void AddExistsProjectAt (string path) {
		if (string.IsNullOrEmpty(path) || !Util.FolderExists(path)) return;
		if (Setting != null && Project.IsValidProjectPath(path) && !Setting.Projects.Any(data => data.Path == path)) {
			// Add to Path List
			Setting.Projects.Add(new EngineSetting.ProjectData() {
				Path = path,
				FolderExists = true,
			});
			// Save Setting
			JsonUtil.SaveJson(Setting, AngePath.PersistentDataPath);
		}
	}


	#endregion




}
