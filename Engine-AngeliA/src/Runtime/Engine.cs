using System.Linq;
using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;

[RequireSpriteFromField]
[RequireLanguageFromField]
internal class Engine {




	#region --- VAR ---


	// SUB
	private enum WindowMode { Mascot, Window, ConfirmQuit, }

	// Const
	private const int MASCOT_WIDTH = 360;
	private const int MASCOT_HEIGHT = 360;
	private static int WINDOW_UI_COUNT = 3;
	private static readonly SpriteCode UI_TAB = "UI.MainTab";
	private static readonly SpriteCode UI_INACTIVE_TAB = "UI.MainTabInactive";
	private static readonly SpriteCode UI_WINDOW_BG = "UI.MainBG";
	private static readonly EntityUI[] ALL_UI = {
		new GenericPopupUI() { Active = false, },
		new GenericDialogUI(){ Active = false, },
		new FileBrowserUI(){ Active = false, },
		new ProjectHub(OpenProject, CloseProject, GetCurrentProjectPath),
		new PixelEditor(),
		new LanguageEditor(),
	};
	private static readonly LanguageCode[] UI_TITLES = {
		("", ""),
		("", ""),
		("", ""),
		("Title.Hub", "Home"),
		("Title.Pixel", "Artwork"),
		("Title.Language", "Language"),
	};
	private static readonly LanguageCode QUIT_MSG = ("UI.QuitMessage", "Quit editor?");

	// Api
	public static Project CurrentProject { get; private set; } = null;

	// Data
	private static readonly GUIStyle MsgStyle = new(GUISkin.CenterLabel) { CharSize = 100 };
	private static readonly GUIStyle ConfirmBtnStyle = new(GUISkin.DarkButton) { CharSize = 100 };
	private static EngineSetting Setting;
	private static Int2? FloatMascotMouseDownPos = null;
	private static Int2 FloatMascotMouseDownGlobalPos = default;
	private static WindowMode CurrentWindowMode;
	private static bool FloatMascotDragged = false;
	private static bool SettingInitialized = false;
	private static int CurrentWindowIndex = 0;


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitialize () {
		Setting = JsonUtil.LoadOrCreateJson<EngineSetting>(AngePath.PersistentDataPath);
		SwitchWindowMode(Setting.WindowMode ? WindowMode.Window : WindowMode.Mascot);
		ALL_UI.ForEach<WindowUI>(win => win.OnActivated());
		WINDOW_UI_COUNT = ALL_UI.Count(ui => ui is WindowUI);
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
		if (CurrentWindowMode != WindowMode.ConfirmQuit) Setting.LoadValueFromWindow();
		JsonUtil.SaveJson(Setting, AngePath.PersistentDataPath, prettyPrint: true);
		ALL_UI.ForEach<WindowUI>(win => win.OnInactivated());
	}


	// GUI
	[OnGameUpdateLater(-4096)]
	internal static void OnGUI () {

		// Switch on Mid Click
		if (CurrentWindowMode != WindowMode.ConfirmQuit && Input.MouseMidButtonDown) {
			SwitchWindowMode(CurrentWindowMode == WindowMode.Window ? WindowMode.Mascot : WindowMode.Window);
		}

		// On GUI
		switch (CurrentWindowMode) {
			case WindowMode.Mascot:
				Sky.ForceSkyboxTint(Color32.CLEAR, Color32.CLEAR);
				OnGUI_Mascot_MouseLogic();
				OnGUI_Mascot_Render();
				break;
			case WindowMode.Window:
				Sky.ForceSkyboxTint(new Color32(38, 38, 38, 255), new Color32(38, 38, 38, 255));
				OnGUI_Window();
				var windowPos = Game.GetWindowPosition();
				if (windowPos.y < 24) {
					Game.SetWindowPosition(windowPos.x, 24);
				}
				break;
			case WindowMode.ConfirmQuit:
				Sky.ForceSkyboxTint(new Color32(38, 38, 38, 255), new Color32(38, 38, 38, 255));
				OnGUI_ConfirmQuit();
				break;
		}
	}


	// Window
	private static void OnGUI_Window () {

		int barHeight = GUI.UnifyMonitor(38);
		int contentPadding = GUI.UnifyMonitor(8);
		int bodyBorder = GUI.UnifyMonitor(6);
		var cameraRect = Renderer.CameraRect;
		int windowLen = CurrentProject == null ? 1 : WINDOW_UI_COUNT;
		int tabWidth = (cameraRect.width - bodyBorder * 2) / windowLen;
		var rect = new IRect(cameraRect.x + bodyBorder, cameraRect.yMax - barHeight, tabWidth, barHeight);
		var mousePos = Input.MouseGlobalPosition;
		bool mousePress = Input.MouseLeftButtonDown;

		// Tab
		CurrentWindowIndex = CurrentWindowIndex.Clamp(0, windowLen - 1);
		int index = 0;
		for (int i = 0; i < ALL_UI.Length; i++) {

			if (ALL_UI[i] is not WindowUI window) continue;
			if (index >= windowLen) break;

			bool selecting = index == CurrentWindowIndex;
			bool hovering = rect.Contains(mousePos);

			// Cursor
			if (!selecting && hovering) Cursor.SetCursorAsHand();

			// Body
			Renderer.Draw_9Slice(
				selecting ? UI_TAB : UI_INACTIVE_TAB,
				rect.Shrink(0, 0, 0, contentPadding),
				bodyBorder, bodyBorder, bodyBorder, bodyBorder,
				selecting ? Color32.WHITE : Color32.GREY_196
			);
			var contentRect = rect.Shrink(contentPadding, contentPadding, 0, contentPadding);

			// Icon
			int iconSize = contentRect.height;
			Renderer.Draw(window.TypeID, contentRect.EdgeInside(Direction4.Left, iconSize));

			// Label
			GUI.Label(contentRect.Shrink(iconSize, 0, 0, 0), UI_TITLES[i]);

			// Click
			if (mousePress && hovering) CurrentWindowIndex = index;

			// Next
			rect.x += rect.width;
			index++;
		}

		// Window BG
		Renderer.Draw_9Slice(
			UI_WINDOW_BG,
			cameraRect.Shrink(0, 0, 0, barHeight),
			bodyBorder, bodyBorder, bodyBorder, bodyBorder
		);

		// Window Content
		WindowUI.ForceWindowRect(cameraRect.Shrink(0, 0, 0, barHeight + bodyBorder));
		index = 0;
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
		foreach (var ui in ALL_UI) if (ui.Active) ui.FirstUpdate();
		foreach (var ui in ALL_UI) if (ui.Active) ui.BeforeUpdate();
		foreach (var ui in ALL_UI) if (ui.Active) ui.Update();
		foreach (var ui in ALL_UI) if (ui.Active) ui.LateUpdate();

		// Clip
		IWindowEntityUI.ClipTextForAllUI(ALL_UI, ALL_UI.Length);

	}


	// Mascot
	private static void OnGUI_Mascot_MouseLogic () {
		// Mouse Right Down
		//if (Raylib.IsMouseButtonDown(MouseButton.Right)) {
		//	//EditorUtil.BuildProject(,,);
		//}
		// Mouse Left Down
		if (Input.MouseLeftButtonHolding) {
			var mousePos = Input.MouseScreenPosition;
			mousePos.y = Game.ScreenHeight - mousePos.y;
			if (!FloatMascotMouseDownPos.HasValue) {
				// Mouse Down
				FloatMascotMouseDownPos = mousePos;
				FloatMascotDragged = false;
			} else {
				if (FloatMascotDragged || Util.SquareDistance(mousePos, FloatMascotMouseDownPos.Value) > 1600) {
					if (!FloatMascotDragged) {
						// Drag Start
						FloatMascotDragged = true;
						FloatMascotMouseDownPos = mousePos;
						FloatMascotMouseDownGlobalPos = Game.GetWindowPosition() + mousePos;
					} else {
						// Dragging
						var windowPos = Game.GetWindowPosition();
						var mouseGlobalPos = windowPos + mousePos;
						var aimPos = (FloatMascotMouseDownGlobalPos - FloatMascotMouseDownPos.Value) +
							(mouseGlobalPos - FloatMascotMouseDownGlobalPos);
						Game.SetWindowPosition(
							(int)Util.LerpUnclamped(windowPos.x, aimPos.x, 0.4f),
							(int)Util.LerpUnclamped(windowPos.y, aimPos.y, 0.4f)
						);
					}
				}
			}
		}
		if (FloatMascotMouseDownPos.HasValue && !Input.MouseLeftButtonHolding) {
			// Mouse Up
			FloatMascotMouseDownPos = null;
			if (!FloatMascotDragged) {
				// Click
				SwitchWindowMode(WindowMode.Window);
				return;
			} else {
				// Drag End
				FloatMascotDragged = false;
				var windowPos = Game.GetWindowPosition();
				windowPos.x = windowPos.x.Clamp(0, Game.MonitorWidth - Game.ScreenWidth);
				windowPos.y = windowPos.y.Clamp(0, Game.MonitorHeight - Game.ScreenHeight);
				Setting.FloatX = windowPos.x;
				Setting.FloatY = windowPos.y;
				Game.SetWindowPosition(Setting.FloatX, Setting.FloatY);
				Game.SetWindowSize(MASCOT_WIDTH, MASCOT_HEIGHT);
			}
		}
	}


	private static void OnGUI_Mascot_Render () {

		// BG
		Renderer.Draw(Const.PIXEL, Renderer.CameraRect);


	}


	// Confirm Quit
	private static void OnGUI_ConfirmQuit () {

		var cameraRect = Renderer.CameraRect;
		int buttonHeight = GUI.UnifyMonitor(64);
		int btnPadding = GUI.UnifyMonitor(8);

		// MSG 
		GUI.Label(
			cameraRect.EdgeInside(Direction4.Up, cameraRect.height - buttonHeight).Shrink(GUI.UnifyMonitor(8)),
			QUIT_MSG, MsgStyle
		);

		// Buttons 
		ConfirmBtnStyle.BodyBorder = Int4.one * GUI.UnifyMonitor(20);
		ConfirmBtnStyle.ContentShift = ConfirmBtnStyle.ContentShiftHover = ConfirmBtnStyle.ContentShiftDisable = new Int2(0, GUI.UnifyMonitor(5));

		var rect = new IRect(cameraRect.x, 0, cameraRect.width / 2, buttonHeight);
		using (GUIScope.BodyColor(Color32.RED_BETTER)) {
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




	#region --- API ---





	#endregion




	#region --- LGC ---


	private static void SwitchWindowMode (WindowMode newMode) {

		// Cache
		if (SettingInitialized && CurrentWindowMode != WindowMode.ConfirmQuit) {
			Setting.LoadValueFromWindow();
		}

		// Set
		int targetWindowWidth = Game.ScreenWidth;
		int targetWindowHeight = Game.ScreenHeight;
		int minWindowSize = Game.MonitorHeight / 5;
		switch (newMode) {
			case WindowMode.Mascot: {
				// Mascot
				Game.IsWindowDecorated = false;
				Game.IsWindowTopmost = true;
				Game.IsWindowResizable = false;
				Game.IsWindowMaximized = false;
				Setting.FloatX = Setting.FloatX.Clamp(0, Game.MonitorWidth - MASCOT_WIDTH);
				Setting.FloatY = Setting.FloatY.Clamp(0, Game.MonitorHeight - MASCOT_HEIGHT);
				Game.SetWindowPosition(Setting.FloatX, Setting.FloatY);
				targetWindowWidth = MASCOT_WIDTH;
				targetWindowHeight = MASCOT_HEIGHT;
				minWindowSize = Util.Min(MASCOT_WIDTH, MASCOT_HEIGHT);
				Setting.WindowMode = false;
				break;
			}
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
				Setting.WindowMode = true;
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


	// Workflow
	private static bool OpenProject (string projectPath) {
		LanguageEditor.Instance.SetLanguageRoot("");
		PixelEditor.Instance.SetSheetPath("");
		if (!ProjectUtil.IsValidProjectPath(projectPath)) return false;
		if (CurrentProject != null && projectPath == CurrentProject.ProjectPath) return false;
		CurrentProject = new Project(projectPath);
		LanguageEditor.Instance.SetLanguageRoot(AngePath.GetLanguageRoot(CurrentProject.UniversePath));
		PixelEditor.Instance.SetSheetPath(AngePath.GetSheetPath(CurrentProject.UniversePath));
		return true;
	}


	private static void CloseProject () {
		foreach (var win in ALL_UI) {
			if (win is WindowUI) win.OnInactivated();
		}
		LanguageEditor.Instance.SetLanguageRoot("");
		PixelEditor.Instance.SetSheetPath("");
		CurrentProject = null;
	}


	private static string GetCurrentProjectPath () => CurrentProject?.ProjectPath;


	#endregion




}
