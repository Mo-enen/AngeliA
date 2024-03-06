using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliA;
using AngeliA.Framework;
using AngeliaRuntime;


namespace AngeliaEngine;


[RequireSpriteFromField]
[RequireLanguageFromField]
internal class Engine {




	#region --- VAR ---


	// SUB
	private enum WindowMode { Mascot, Float, Window, ConfirmQuit, }

	// Const
	private const int MASCOT_WIDTH = 360;
	private const int MASCOT_HEIGHT = 360;
	private static readonly SpriteCode UI_TAB = "UI.MainTab";
	private static readonly SpriteCode UI_INACTIVE_TAB = "UI.MainTabInactive";
	private static readonly SpriteCode UI_WINDOW_BG = "UI.MainBG";
	private static readonly SpriteCode UI_BTN = "UI.CommenButton";
	private static readonly SpriteCode ICON_CLOSE = "Icon.PixelClose";
	private static readonly WindowUI[] WINDOWS = {
		new ProjectHub(),
		new PixelEditor(),
		new LanguageEditor(),
	};
	private static readonly LanguageCode[] WINDOW_TITLES = {
		("Title.Hub", "Home"),
		("Title.Pixel", "Artwork"),
		("Title.Language", "Language"),
	};
	private static readonly List<EntityUI> ALL_UI = new();
	private static readonly LanguageCode QUIT_MSG = ("UI.QuitMessage", "Quit editor?");

	// Api
	public static Project CurrentProject { get; private set; } = null;

	// Data
	private static readonly GenericPopupUI PopupUI = new();
	private static readonly GenericDialogUI DialogUI = new();
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
		PopupUI.Active = false;
		DialogUI.Active = false;
		ALL_UI.Clear();
		ALL_UI.Add(PopupUI);
		ALL_UI.Add(DialogUI);
		ALL_UI.AddRange(WINDOWS);
		ALL_UI.ForEach(ui => ui.OnActivated());
		ProjectHub.Instance.LoadSettingFromDisk();
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
		ALL_UI.ForEach(ui => ui.OnInactivated());
		ProjectHub.Instance.SaveSettingToDisk();
	}


	// GUI
	[OnGameUpdateLater(-4096)]
	internal static void OnGUI () {

		// Switch to Mascot on Lost Focus
		if (CurrentWindowMode == WindowMode.Float && !Game.IsWindowFocused) {
			SwitchWindowMode(WindowMode.Mascot);
		}

		// Switch on Mid Click
		if (CurrentWindowMode != WindowMode.ConfirmQuit && Input.MouseMidButtonDown) {
			SwitchWindowMode(CurrentWindowMode == WindowMode.Window ? WindowMode.Float : WindowMode.Window);
		}

		// On GUI
		switch (CurrentWindowMode) {
			case WindowMode.Mascot:
				Sky.ForceSkyboxTint(Color32.CLEAR, Color32.CLEAR);
				OnGUI_Mascot_MouseLogic();
				OnGUI_Mascot_Render();
				break;
			case WindowMode.Float:
				Sky.ForceSkyboxTint(Color32.CLEAR, Color32.CLEAR);
				OnGUI_Window();
				break;
			case WindowMode.Window:
				Sky.ForceSkyboxTint(new Color32(38, 38, 38, 255), new Color32(38, 38, 38, 255));
				OnGUI_Window();
				break;
			case WindowMode.ConfirmQuit:
				Sky.ForceSkyboxTint(new Color32(38, 38, 38, 255), new Color32(38, 38, 38, 255));
				OnGUI_ConfirmQuit();
				break;
		}
	}


	// Window
	private static void OnGUI_Window () {

		// Tab Bar
		int barHeight = GUI.UnifyMonitor(38);
		int contentPadding = GUI.UnifyMonitor(8);
		int bodyBorder = GUI.UnifyMonitor(6);
		bool floating = CurrentWindowMode == WindowMode.Float;
		int closeButtonWidth = floating ? barHeight - contentPadding : bodyBorder;
		var cameraRect = Renderer.CameraRect;
		int tabWidth = (cameraRect.width - bodyBorder - closeButtonWidth) / WINDOWS.Length;
		var rect = new IRect(cameraRect.x + bodyBorder, cameraRect.yMax - barHeight, tabWidth, barHeight);
		var mousePos = Input.MouseGlobalPosition;
		bool mousePress = Input.MouseLeftButtonDown;

		// Content
		int windowLen = CurrentProject == null ? 1 : WINDOWS.Length;
		CurrentWindowIndex = CurrentWindowIndex.Clamp(0, windowLen - 1);
		for (int i = 0; i < windowLen; i++) {

			var window = WINDOWS[i];
			bool selecting = i == CurrentWindowIndex;
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
			var contentRect = rect.Shrink(contentPadding, closeButtonWidth, 0, contentPadding);

			// Icon
			int iconSize = contentRect.height;
			Renderer.Draw(window.TypeID, contentRect.EdgeInside(Direction4.Left, iconSize));

			// Label
			int labelCharSize = GUI.ReverseUnify(contentRect.height) * 5 / 12;
			GUI.Label(
				TextContent.Get(WINDOW_TITLES[i], Color32.GREY_196, charSize: labelCharSize, alignment: Alignment.MidLeft),
				contentRect.Shrink(iconSize, 0, 0, 0)
			);

			// Click
			if (mousePress && hovering) CurrentWindowIndex = i;

			// Next
			rect.x += rect.width;
		}

		// Close Button
		if (floating) {
			var btnRect = new IRect(
				cameraRect.xMax - closeButtonWidth,
				cameraRect.yMax - barHeight,
				closeButtonWidth,
				barHeight - contentPadding
			);
			GUI.Icon(btnRect, ICON_CLOSE, z: 1);
			if (GUI.SpriteButton(btnRect, UI_BTN, z: 0)) {
				if (CurrentWindowMode != WindowMode.ConfirmQuit) {
					SwitchWindowMode(WindowMode.ConfirmQuit);
				} else {
					Game.QuitApplication();
				}
			}
		}

		// Window BG
		Renderer.Draw_9Slice(
			UI_WINDOW_BG,
			cameraRect.EdgeInside(Direction4.Down, cameraRect.height - barHeight),
			bodyBorder, bodyBorder, bodyBorder, bodyBorder, z: int.MinValue
		);

		// Window Content
		WindowUI.ForceWindowRect(cameraRect.Shrink(0, 0, 0, barHeight));
		for (int i = 0; i < WINDOWS.Length; i++) {
			var win = WINDOWS[i];
			bool active = i == CurrentWindowIndex;
			if (active == win.Active) continue;
			win.Active = active;
			if (active) {
				win.OnActivated();
			} else {
				win.OnInactivated();
			}
		}
		foreach (var ui in ALL_UI) if (ui.Active) ui.BeforePhysicsUpdate();
		foreach (var ui in ALL_UI) if (ui.Active) ui.PhysicsUpdate();
		foreach (var ui in ALL_UI) if (ui.Active) ui.FrameUpdate();

		// Clip
		IWindowEntityUI.ClipTextForAllUI(ALL_UI, ALL_UI.Count);

	}


	// Mascot
	private static void OnGUI_Mascot_MouseLogic () {
		// Mouse Right Down
		//if (Raylib.IsMouseButtonDown(MouseButton.Right)) {
		//	//EditorUtil.BuildProject(,,);
		//}
		// Mouse Left Down
		if (Input.MouseLeftButton) {
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
		if (FloatMascotMouseDownPos.HasValue && !Input.MouseLeftButton) {
			// Mouse Up
			FloatMascotMouseDownPos = null;
			if (!FloatMascotDragged) {
				// Click
				SwitchWindowMode(WindowMode.Float);
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
			TextContent.Get(QUIT_MSG, charSize: 80, alignment: Alignment.MidMid, wrap: WrapMode.WordWrap),
			cameraRect.EdgeInside(Direction4.Up, cameraRect.height - buttonHeight).Shrink(GUI.UnifyMonitor(8))
		);

		// Buttons 
		var rect = new IRect(cameraRect.x, 0, cameraRect.width / 2, buttonHeight);
		if (GUI.LabelButton(
			rect.Shrink(btnPadding), UI_BTN, BuiltInText.UI_QUIT, Color32.WHITE, Color32.GREY_216
		)) {
			Game.QuitApplication();
		}
		rect.x += rect.width;
		if (GUI.LabelButton(
			rect.Shrink(btnPadding), UI_BTN, BuiltInText.UI_CANCEL, Color32.WHITE, Color32.GREY_216
		)) {
			SwitchWindowMode(Setting.WindowMode ? WindowMode.Window : WindowMode.Float);
		}

	}


	#endregion




	#region --- API ---


	public bool OpenProject (string projectPath) {
		if (!Project.IsValidProjectPath(projectPath)) return false;
		if (projectPath == CurrentProject.ProjectPath) return false;
		CurrentProject = new Project(projectPath);
		LanguageEditor.Instance.SetLanguageRoot(AngePath.GetLanguageRoot(CurrentProject.UniversePath));
		PixelEditor.Instance.SetSheetPath(AngePath.GetSheetPath(CurrentProject.UniversePath));
		return true;
	}


	public void CloseProject () {
		foreach (var win in WINDOWS) win.OnInactivated();
		LanguageEditor.Instance.SetLanguageRoot("");
		PixelEditor.Instance.SetSheetPath("");
		CurrentProject = null;
	}


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
			case WindowMode.Float: {
				// Float
				Game.IsWindowDecorated = false;
				Game.IsWindowTopmost = true;
				Game.IsWindowResizable = false;
				Game.IsWindowMaximized = false;
				int width = Game.MonitorHeight;
				int height = Game.MonitorHeight * 8 / 10;
				Game.SetWindowPosition((Game.MonitorWidth - width) / 2, (Game.MonitorHeight - height) / 2);
				targetWindowWidth = width;
				targetWindowHeight = height;
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


	#endregion




}
