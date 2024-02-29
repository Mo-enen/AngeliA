using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using Raylib_cs;
using AngeliA;
using AngeliaToRaylib;
using KeyboardKey = Raylib_cs.KeyboardKey;
using System.Linq;


namespace AngeliaEditor;


[RequireSpriteFromField]
public sealed class Editor {




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
	private bool RequireQuit = false;
	private readonly Sheet Sheet = new();
	private FontData Font;
	private Setting Setting;
	private Window[] Windows;
	private Vector2? FloatMascotMouseDownPos = null;
	private Vector2 FloatMascotMouseDownGlobalPos = default;
	private WindowMode CurrentWindowMode;
	private int CurrentWindowIndex = 0;
	private bool FloatMascotDragged = false;


	#endregion




	#region --- MSG ---


	public void Run () {

		// Init Editor
		RequireQuit = false;
		Setting = JsonUtil.LoadOrCreateJson<Setting>(AngePath.PersistentDataPath);
		Raylib.SetTraceLogLevel(TraceLogLevel.Warning);
		Util.OnLogException += RaylibUtil.LogException;
		Util.OnLogError += RaylibUtil.LogError;
		Util.OnLog += RaylibUtil.Log;
		Util.OnLogWarning += RaylibUtil.LogWarning;
		Raylib.SetConfigFlags(ConfigFlags.TransparentWindow);
		Raylib.InitWindow(1024 / 9 * 16, 1024, $"{AngeliaGameTitleAttribute.GetTitle()} {AngeliaVersionAttribute.GetVersionString()}");
		Raylib.EnableEventWaiting();
		Raylib.SetExitKey(KeyboardKey.Null);
		SwitchWindowMode(Setting.WindowMode ? WindowMode.Window : WindowMode.Mascot);

		// Init Resources
		string universePath = Util.CombinePaths(AngePath.ApplicationDataPath, "Universe");
		var fonts = RayGUI.LoadFontDataFromFile(Util.CombinePaths(universePath, "Fonts"));
		Font = fonts != null && fonts.Length > 0 ? fonts[0] : new();
		string sheetPath = AngePath.GetSheetPath(universePath);
		string artworkPath = AngePath.GetAsepriteRoot(universePath);
		SheetUtil.RecreateSheetIfArtworkModified(sheetPath, artworkPath);
		Sheet.LoadFromDisk(sheetPath);

		// Init Window
		Window.CacheSheet = Sheet;
		Window.CacheFont = Font;
		Windows = new Window[] {
			new LanguageEditor(CurrentProject, "Language", "Icon.Language".AngeHash()),
		};

		// Update
		while (!RequireQuit) {
			RayGUI.BeforeUpdate();
			Raylib.BeginDrawing();
			OnGUI();
			GizmosRender.UpdateGizmos();
			Raylib.EndDrawing();
			RayGUI.TypingBuilder.Clear();
			if (Raylib.WindowShouldClose()) {
				if (CurrentWindowMode != WindowMode.ConfirmQuit) {
					SwitchWindowMode(WindowMode.ConfirmQuit);
				} else {
					RequireQuit = true;
				}
			}
		}

		// Quit
		Font.Unload();
		if (CurrentWindowMode != WindowMode.ConfirmQuit) Setting.LoadValueFromWindow();
		JsonUtil.SaveJson(Setting, AngePath.PersistentDataPath, prettyPrint: true);
		Raylib.CloseWindow();
		Util.InvokeAllStaticMethodWithAttribute<OnQuitAttribute>();
	}


	// GUI
	private void OnGUI () {
		Raylib.SetMouseCursor(MouseCursor.Default);
		CurrentWindowIndex = CurrentWindowIndex.Clamp(0, Windows.Length - 1);
		// Switch to Mascot on Lost Focus
		if (CurrentWindowMode == WindowMode.Float && !Raylib.IsWindowFocused()) {
			SwitchWindowMode(WindowMode.Mascot);
		}
		// Switch on Mid Click
		if (Raylib.IsMouseButtonPressed(MouseButton.Middle)) {
			SwitchWindowMode(CurrentWindowMode == WindowMode.Window ? WindowMode.Float : WindowMode.Window);
		}
		// On GUI
		switch (CurrentWindowMode) {
			case WindowMode.Mascot:
				Raylib.ClearBackground(Color.Black);
				OnGUI_Mascot_MouseLogic();
				OnGUI_Mascot_Render();
				break;
			case WindowMode.Float:
				Raylib.ClearBackground(Color.Blank);
				OnGUI_Window();
				break;
			case WindowMode.Window:
				Raylib.ClearBackground(new Color(38, 38, 38, 255));
				OnGUI_Window();
				break;
			case WindowMode.ConfirmQuit:
				Raylib.ClearBackground(new Color(38, 38, 38, 255));
				OnGUI_ConfirmQuit();
				break;
		}
	}


	// Window
	private void OnGUI_Window () {

		int screenWidth = Raylib.GetRenderWidth();
		int screenheight = Raylib.GetRenderHeight();

		// Tab Bar
		int barHeight = Unify(32);
		int barPadding = Sheet.SpritePool.TryGetValue(UI_WINDOW_BG, out var bgSprite) ?
			bgSprite.GlobalBorder.left : 5;
		OnGUI_TabBar(barHeight, RayGUI.GetUnifyBorder(barPadding));

		// Window BG
		Sheet.Draw_9Slice(
			UI_WINDOW_BG, 0, barHeight, 0, 0, 0, screenWidth, screenheight - barHeight,
			barPadding, barPadding, barPadding, barPadding
		);

		// Window Content
		var window = Windows[CurrentWindowIndex];
		window.DrawWindow(new Rectangle(0, barHeight, screenWidth, screenheight - barHeight));
	}


	private void OnGUI_TabBar (int barHeight, int padding) {

		bool floating = CurrentWindowMode == WindowMode.Float;
		int contentPadding = Unify(4);
		int closeButtonWidth = floating ? barHeight - contentPadding : padding;
		int screenWidth = Raylib.GetRenderWidth();
		int tabWidth = (screenWidth - padding - closeButtonWidth) / Windows.Length;
		var rect = new Rectangle(padding, 0, tabWidth, barHeight);
		var mousePos = Raylib.GetMousePosition();
		bool mousePress = Raylib.IsMouseButtonPressed(MouseButton.Left);

		// Content
		for (int i = 0; i < Windows.Length; i++) {
			var window = Windows[i];
			bool selecting = i == CurrentWindowIndex;
			bool hovering = rect.Contains(mousePos);

			// Cursor
			if (!selecting && hovering) {
				Raylib.SetMouseCursor(MouseCursor.PointingHand);
			}

			// Body
			Sheet.Draw_9Slice(
				selecting ? UI_TAB : UI_INACTIVE_TAB,
				rect.Shrink(0, 0, 0, contentPadding),
				selecting || hovering ? Color32.WHITE : Color32.GREY_196
			);
			var contentRect = rect.Shrink(contentPadding, closeButtonWidth, 0, contentPadding);

			// Icon
			float iconSize = contentRect.Height;
			if (window.Icon != 0) {
				Sheet.Draw(window.Icon, contentRect.EdgeInside(Direction4.Left, iconSize));
			}

			// Label
			Font.DrawLabel(
				TextContent.Get(window.Title, Color32.GREY_196, charSize: 12, alignment: Alignment.MidLeft),
				contentRect.Shrink(window.Icon != 0 ? iconSize : contentPadding, 0, 0, 0)
			);

			// Click
			if (mousePress && hovering) {
				CurrentWindowIndex = i;
			}

			// Next
			rect.X += rect.Width;
		}

		// Close Button
		if (floating) {
			if (Sheet.IconButton(
				new Rectangle(screenWidth - closeButtonWidth, contentPadding, closeButtonWidth, barHeight - contentPadding),
				UI_BTN, ICON_CLOSE
			)) {
				if (CurrentWindowMode != WindowMode.ConfirmQuit) {
					SwitchWindowMode(WindowMode.ConfirmQuit);
				} else {
					RequireQuit = true;
				}
			}
		}
	}


	// Mascot
	private void OnGUI_Mascot_MouseLogic () {
		// Mouse Right Down
		if (Raylib.IsMouseButtonDown(MouseButton.Right)) {
			//EditorUtil.BuildProject(,,);
		}
		// Mouse Left Down
		if (Raylib.IsMouseButtonDown(MouseButton.Left)) {
			var mousePos = Raylib.GetMousePosition();
			if (!FloatMascotMouseDownPos.HasValue) {
				// Mouse Down
				FloatMascotMouseDownPos = mousePos;
				FloatMascotDragged = false;
			} else {
				if (FloatMascotDragged || Vector2.DistanceSquared(mousePos, FloatMascotMouseDownPos.Value) > 1600f) {
					if (!FloatMascotDragged) {
						// Drag Start
						FloatMascotDragged = true;
						FloatMascotMouseDownPos = mousePos;
						FloatMascotMouseDownGlobalPos = Raylib.GetWindowPosition() + mousePos;
					} else {
						// Dragging
						var windowPos = Raylib.GetWindowPosition();
						var mouseGlobalPos = windowPos + mousePos;
						var aimPos = (FloatMascotMouseDownGlobalPos - FloatMascotMouseDownPos.Value) +
							(mouseGlobalPos - FloatMascotMouseDownGlobalPos);
						Raylib.SetWindowPosition(
							(int)Util.LerpUnclamped(windowPos.X, aimPos.X, 0.1f),
							(int)Util.LerpUnclamped(windowPos.Y, aimPos.Y, 0.1f)
						);
					}
				}
			}
		}
		if (FloatMascotMouseDownPos.HasValue && !Raylib.IsMouseButtonDown(MouseButton.Left)) {
			// Mouse Up
			FloatMascotMouseDownPos = null;
			if (!FloatMascotDragged) {
				// Click
				SwitchWindowMode(WindowMode.Float);
				return;
			} else {
				// Drag End
				FloatMascotDragged = false;
				var windowPos = Raylib.GetWindowPosition();
				int windowWidth = Raylib.GetRenderWidth();
				int windowHeight = Raylib.GetRenderHeight();
				int monitor = Raylib.GetCurrentMonitor();
				windowPos.X = windowPos.X.Clamp(0, Raylib.GetMonitorWidth(monitor) - windowWidth);
				windowPos.Y = windowPos.Y.Clamp(0, Raylib.GetMonitorHeight(monitor) - windowHeight);
				Setting.FloatX = windowPos.X.RoundToInt();
				Setting.FloatY = windowPos.Y.RoundToInt();
				Raylib.SetWindowPosition(Setting.FloatX, Setting.FloatY);
			}
		}
	}


	private void OnGUI_Mascot_Render () {

		var panelRect = new Rectangle(0, 0, Raylib.GetRenderWidth(), Raylib.GetRenderHeight());

		// BG



	}


	// Confirm Quit
	private void OnGUI_ConfirmQuit () {

		int screenWidth = Raylib.GetRenderWidth();
		int screenHeight = Raylib.GetRenderHeight();
		int padding = screenHeight / 10;
		int buttonHeight = Unify(60);
		int btnPadding = Unify(6);
		bool isDirty = Windows.Any(w => w.IsDirty);

		// MSG 
		Font.DrawLabel(
			TextContent.Get(isDirty ? "Save all changes?" : "Quit editor?", charSize: 20, alignment: Alignment.MidMid, wrap: true),
			new Rectangle(0, 0, screenWidth, screenHeight - buttonHeight).Shrink(padding)
		);

		// Buttons 
		var rect = new Rectangle(0, screenHeight - buttonHeight, isDirty ? screenWidth / 3 : screenWidth / 2, buttonHeight);
		if (isDirty) {
			if (RayGUI.TextButton(Font, Sheet, rect.Shrink(btnPadding), UI_BTN, "Save")) {
				Windows.ForEach(w => w?.Save());
				RequireQuit = true;
			}
			rect.X += rect.Width;
			if (RayGUI.TextButton(Font, Sheet, rect.Shrink(btnPadding), UI_BTN, "Don't Save")) {
				RequireQuit = true;
			}
			rect.X += rect.Width;
			if (RayGUI.TextButton(Font, Sheet, rect.Shrink(btnPadding), UI_BTN, "Cancel")) {
				SwitchWindowMode(Setting.WindowMode ? WindowMode.Window : WindowMode.Float);
			}
		} else {
			if (RayGUI.TextButton(Font, Sheet, rect.Shrink(btnPadding), UI_BTN, "Quit")) {
				RequireQuit = true;
			}
			rect.X += rect.Width;
			if (RayGUI.TextButton(Font, Sheet, rect.Shrink(btnPadding), UI_BTN, "Cancel")) {
				SwitchWindowMode(Setting.WindowMode ? WindowMode.Window : WindowMode.Float);
			}
		}

	}


	#endregion




	#region --- LGC ---


	private void SwitchWindowMode (WindowMode newMode) {

		// Cache
		if (Setting.Initialized && CurrentWindowMode != WindowMode.ConfirmQuit) {
			Setting.LoadValueFromWindow();
		}

		// Set
		switch (newMode) {
			case WindowMode.Mascot: {
				// Mascot
				Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
				Raylib.SetWindowState(ConfigFlags.TopmostWindow);
				Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
				Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
				Raylib.SetWindowSize(FLOAT_WIDTH, FLOAT_HEIGHT);
				Raylib.SetWindowPosition(Setting.FloatX, Setting.FloatY);
				Setting.WindowMode = false;
				break;
			}
			case WindowMode.Float: {
				// Float
				Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
				Raylib.SetWindowState(ConfigFlags.TopmostWindow);
				Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
				Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
				int monitor = Raylib.GetCurrentMonitor();
				int monitorWidth = Raylib.GetMonitorWidth(monitor);
				int monitorHeight = Raylib.GetMonitorHeight(monitor);
				int width = monitorHeight;
				int height = monitorHeight * 8 / 10;
				Raylib.SetWindowPosition((monitorWidth - width) / 2, (monitorHeight - height) / 2);
				Raylib.SetWindowSize(width, height);
				Setting.WindowMode = false;
				break;
			}
			case WindowMode.Window: {
				// Window
				Raylib.ClearWindowState(ConfigFlags.UndecoratedWindow);
				Raylib.ClearWindowState(ConfigFlags.TopmostWindow);
				Raylib.SetWindowState(ConfigFlags.ResizableWindow);
				if (Setting.Maximize) {
					int monitor = Raylib.GetCurrentMonitor();
					Raylib.SetWindowPosition(0, 0);
					Raylib.SetWindowSize(Raylib.GetMonitorWidth(monitor), Raylib.GetMonitorHeight(monitor));
					Raylib.SetWindowState(ConfigFlags.MaximizedWindow);
				} else {
					Raylib.SetWindowPosition(Setting.WindowPositionX, Setting.WindowPositionY.GreaterOrEquel(24));
					Raylib.SetWindowSize(Setting.WindowSizeX, Setting.WindowSizeY);
				}
				Setting.WindowMode = true;
				break;
			}
			case WindowMode.ConfirmQuit: {
				// ConfirmQuit
				Raylib.ClearWindowState(ConfigFlags.UndecoratedWindow);
				Raylib.ClearWindowState(ConfigFlags.TopmostWindow);
				Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
				int monitor = Raylib.GetCurrentMonitor();
				int monitorWidth = Raylib.GetMonitorWidth(monitor);
				int monitorHeight = Raylib.GetMonitorHeight(monitor);
				int width = monitorHeight * 5 / 10;
				int height = monitorHeight * 2 / 10;
				Raylib.SetWindowPosition((monitorWidth - width) / 2, (monitorHeight - height) / 2);
				Raylib.SetWindowSize(width, height);
				break;
			}
		}
		CurrentWindowMode = newMode;
		Setting.Initialized = true;
	}


	private int Unify (int value) => RayGUI.Unify(value);


	#endregion




}
