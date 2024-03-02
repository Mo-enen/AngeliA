using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;
using AngeliaRuntime;


namespace AngeliaEngine;


[RequireSpriteFromField]
internal class Engine {




	#region --- VAR ---


	// SUB
	private enum WindowMode { Mascot, Float, Window, ConfirmQuit, }

	// Const
	private const int FLOAT_WIDTH = 360;
	private const int FLOAT_HEIGHT = 360;
	private static readonly SpriteCode UI_TAB = "UI.Tab";
	private static readonly SpriteCode UI_INACTIVE_TAB = "UI.InactiveTab";
	private static readonly SpriteCode UI_WINDOW_BG = "UI.WindowBG";
	private static readonly SpriteCode UI_BTN = "UI.Button";
	private static readonly SpriteCode ICON_CLOSE = "Icon.Close";

	// Data
	private readonly Project CurrentProject = new();
	private static Setting Setting;
	private static Int2? FloatMascotMouseDownPos = null;
	private static Int2 FloatMascotMouseDownGlobalPos = default;
	private static WindowMode CurrentWindowMode;
	private static bool FloatMascotDragged = false;


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitialize () {
		Setting = JsonUtil.LoadOrCreateJson<Setting>(AngePath.PersistentDataPath);
		SwitchWindowMode(Setting.WindowMode ? WindowMode.Window : WindowMode.Mascot);
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
	}


	// GUI
	[OnGameUpdateLater]
	internal static void OnGUI () {

		// Switch to Mascot on Lost Focus
		if (CurrentWindowMode == WindowMode.Float && !Game.IsWindowFocused) {
			SwitchWindowMode(WindowMode.Mascot);
		}

		// Switch on Mid Click
		if (Input.MouseMidButtonDown) {
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
		int barHeight = GUI.Unify(48);
		int barPadding = Renderer.TryGetSprite(UI_WINDOW_BG, out var bgSprite) ?
			bgSprite.GlobalBorder.left : 12;
		OnGUI_TabBar(barHeight, GUI.Unify(barPadding));

		// Window BG
		Renderer.Draw_9Slice(
			UI_WINDOW_BG,
			Renderer.CameraRect.EdgeInside(Direction4.Down, Renderer.CameraRect.height - barHeight),
			barPadding, barPadding, barPadding, barPadding
		);
	}


	private static void OnGUI_TabBar (int barHeight, int padding) {

		//bool floating = CurrentWindowMode == WindowMode.Float;
		//int contentPadding = Unify(4);
		//int closeButtonWidth = floating ? barHeight - contentPadding : padding;
		//int screenWidth = Raylib.GetRenderWidth();
		//int tabWidth = (screenWidth - padding - closeButtonWidth) / Windows.Length;
		//var rect = new Rectangle(padding, 0, tabWidth, barHeight);
		//var mousePos = Raylib.GetMousePosition();
		//bool mousePress = Raylib.IsMouseButtonPressed(MouseButton.Left);
		//
		//// Content
		//for (int i = 0; i < Windows.Length; i++) {
		//	var window = Windows[i];
		//	bool selecting = i == CurrentWindowIndex;
		//	bool hovering = rect.Contains(mousePos);
		//
		//	// Cursor
		//	if (!selecting && hovering) {
		//		Raylib.SetMouseCursor(MouseCursor.PointingHand);
		//	}
		//
		//	// Body
		//	Sheet.Draw_9Slice(
		//		selecting ? UI_TAB : UI_INACTIVE_TAB,
		//		rect.Shrink(0, 0, 0, contentPadding),
		//		selecting || hovering ? Color32.WHITE : Color32.GREY_196
		//	);
		//	var contentRect = rect.Shrink(contentPadding, closeButtonWidth, 0, contentPadding);
		//
		//	// Icon
		//	float iconSize = contentRect.Height;
		//	if (window.Icon != 0) {
		//		Sheet.Draw(window.Icon, contentRect.EdgeInside(Direction4.Left, iconSize));
		//	}
		//
		//	// Label
		//	Font.DrawLabel(
		//		TextContent.Get(window.Title, Color32.GREY_196, charSize: 12, alignment: Alignment.MidLeft),
		//		contentRect.Shrink(window.Icon != 0 ? iconSize : contentPadding, 0, 0, 0)
		//	);
		//
		//	// Click
		//	if (mousePress && hovering) {
		//		CurrentWindowIndex = i;
		//	}
		//
		//	// Next
		//	rect.X += rect.Width;
		//}
		//
		//// Close Button
		//if (floating) {
		//	if (Sheet.IconButton(
		//		new Rectangle(screenWidth - closeButtonWidth, contentPadding, closeButtonWidth, barHeight - contentPadding),
		//		UI_BTN, ICON_CLOSE
		//	)) {
		//		if (CurrentWindowMode != WindowMode.ConfirmQuit) {
		//			SwitchWindowMode(WindowMode.ConfirmQuit);
		//		} else {
		//			RequireQuit = true;
		//		}
		//	}
		//}
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
				int windowWidth = Game.ScreenWidth;
				int windowHeight = Game.ScreenHeight;
				int monitor = Game.CurrentMonitor;
				windowPos.x = windowPos.x.Clamp(0, Game.GetMonitorWidth(monitor) - windowWidth);
				windowPos.y = windowPos.y.Clamp(0, Game.GetMonitorHeight(monitor) - windowHeight);
				Setting.FloatX = windowPos.x;
				Setting.FloatY = windowPos.y;
				Game.SetWindowPosition(Setting.FloatX, Setting.FloatY);
			}
		}
	}


	private static void OnGUI_Mascot_Render () {

		//var panelRect = new Rectangle(0, 0, Raylib.GetRenderWidth(), Raylib.GetRenderHeight());

		// BG



	}


	// Confirm Quit
	private static void OnGUI_ConfirmQuit () {

		//int screenWidth = Raylib.GetRenderWidth();
		//int screenHeight = Raylib.GetRenderHeight();
		//int padding = screenHeight / 10;
		//int buttonHeight = Unify(60);
		//int btnPadding = Unify(6);
		//bool isDirty = Windows.Any(w => w.IsDirty);
		//
		//// MSG 
		//Font.DrawLabel(
		//	TextContent.Get(isDirty ? "Save all changes?" : "Quit editor?", charSize: 20, alignment: Alignment.MidMid, wrap: true),
		//	new Rectangle(0, 0, screenWidth, screenHeight - buttonHeight).Shrink(padding)
		//);
		//
		//// Buttons 
		//var rect = new Rectangle(0, screenHeight - buttonHeight, isDirty ? screenWidth / 3 : screenWidth / 2, buttonHeight);
		//if (isDirty) {
		//	if (RayGUI.TextButton(Font, Sheet, rect.Shrink(btnPadding), UI_BTN, "Save")) {
		//		Windows.ForEach(w => w?.Save());
		//		RequireQuit = true;
		//	}
		//	rect.X += rect.Width;
		//	if (RayGUI.TextButton(Font, Sheet, rect.Shrink(btnPadding), UI_BTN, "Don't Save")) {
		//		RequireQuit = true;
		//	}
		//	rect.X += rect.Width;
		//	if (RayGUI.TextButton(Font, Sheet, rect.Shrink(btnPadding), UI_BTN, "Cancel")) {
		//		SwitchWindowMode(Setting.WindowMode ? WindowMode.Window : WindowMode.Float);
		//	}
		//} else {
		//	if (RayGUI.TextButton(Font, Sheet, rect.Shrink(btnPadding), UI_BTN, "Quit")) {
		//		RequireQuit = true;
		//	}
		//	rect.X += rect.Width;
		//	if (RayGUI.TextButton(Font, Sheet, rect.Shrink(btnPadding), UI_BTN, "Cancel")) {
		//		SwitchWindowMode(Setting.WindowMode ? WindowMode.Window : WindowMode.Float);
		//	}
		//}

	}


	#endregion




	#region --- LGC ---


	private static void SwitchWindowMode (WindowMode newMode) {

		// Cache
		if (Setting.Initialized && CurrentWindowMode != WindowMode.ConfirmQuit) {
			Setting.LoadValueFromWindow();
		}

		// Set
		switch (newMode) {
			case WindowMode.Mascot: {
				// Mascot
				Game.IsWindowDecorated = false;
				Game.IsWindowTopmost = true;
				Game.IsWindowResizable = false;
				Game.IsWindowMaximized = false;
				Game.SetWindowSize(FLOAT_WIDTH, FLOAT_HEIGHT);
				Game.SetWindowPosition(Setting.FloatX, Setting.FloatY);
				Setting.WindowMode = false;
				break;
			}
			case WindowMode.Float: {
				// Float
				Game.IsWindowDecorated = false;
				Game.IsWindowTopmost = true;
				Game.IsWindowResizable = false;
				Game.IsWindowMaximized = false;
				int monitor = Game.CurrentMonitor;
				int monitorWidth = Game.GetMonitorWidth(monitor);
				int monitorHeight = Game.GetMonitorHeight(monitor);
				int width = monitorHeight;
				int height = monitorHeight * 8 / 10;
				Game.SetWindowPosition((monitorWidth - width) / 2, (monitorHeight - height) / 2);
				Game.SetWindowSize(width, height);
				Setting.WindowMode = false;
				break;
			}
			case WindowMode.Window: {
				// Window
				Game.IsWindowDecorated = true;
				Game.IsWindowTopmost = false;
				Game.IsWindowResizable = true;
				if (Setting.Maximize) {
					int monitor = Game.CurrentMonitor;
					Game.SetWindowPosition(0, 0);
					Game.SetWindowSize(Game.GetMonitorWidth(monitor), Game.GetMonitorHeight(monitor));
					Game.IsWindowMaximized = true;
				} else {
					Game.IsWindowMaximized = false;
					Game.SetWindowPosition(Setting.WindowPositionX, Setting.WindowPositionY.GreaterOrEquel(24));
					Game.SetWindowSize(Setting.WindowSizeX, Setting.WindowSizeY);
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
				Game.SetWindowPosition((monitorWidth - width) / 2, (monitorHeight - height) / 2);
				Game.SetWindowSize(width, height);
				break;
			}
		}
		CurrentWindowMode = newMode;
		Setting.Initialized = true;
	}


	#endregion




}
