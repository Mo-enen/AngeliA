using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using Raylib_cs;
using AngeliA;
using AngeliaToRaylib;
using KeyboardKey = Raylib_cs.KeyboardKey;


namespace AngeliaEditor;


[RequireSpriteFromField]
public class Editor {




	#region --- VAR ---


	// SUB
	private enum WindowMode { Mascot, Float, Window, }

	// Const
	private const int FLOAT_WIDTH = 360;
	private const int FLOAT_HEIGHT = 360;
	private const int FLOAT_WINDOW_WIDTH = 1000;
	private const int FLOAT_WINDOW_HEIGHT = 800;
	private static readonly SpriteCode UI_TAB = "UI.Tab";
	private static readonly SpriteCode UI_INACTIVE_TAB = "UI.InactiveTab";
	private static readonly SpriteCode UI_WINDOW_BG = "UI.WindowBG";

	// Event
	private static event Func<bool> OnTryingToQuit;

	// Data
	private static bool RequireQuit = false;
	private static float UiScale = 1f;
	private readonly Sheet Sheet = new();
	private FontData Font;
	private Setting Setting;
	private Window[] Windows;
	private Vector2? FloatMascotMouseDownPos = null;
	private Vector2 FloatMascotMouseDownGlobalPos = default;
	private int CurrentWindowIndex = 0;
	private WindowMode CurrentWindowMode;
	private bool FloatMascotDragged = false;


	#endregion




	#region --- MSG ---


	public static void Run () {

		// Init Editor
		var editor = new Editor();
		RequireQuit = false;
		Util.LinkEventWithAttribute<OnTryingToQuitAttribute>(typeof(Editor), nameof(OnTryingToQuit));
		editor.Setting = JsonUtil.LoadOrCreateJson<Setting>(AngePath.PersistentDataPath);
		Raylib.SetTraceLogLevel(TraceLogLevel.Warning);
		Util.OnLogException += RaylibUtil.LogException;
		Util.OnLogError += RaylibUtil.LogError;
		Util.OnLog += RaylibUtil.Log;
		Util.OnLogWarning += RaylibUtil.LogWarning;
		Raylib.SetConfigFlags(ConfigFlags.TransparentWindow);
		Raylib.InitWindow(1024 / 9 * 16, 1024, $"{AngeliaGameTitleAttribute.GetTitle()} {AngeliaVersionAttribute.GetVersionString()}");
		Raylib.EnableEventWaiting();
		Raylib.SetExitKey(KeyboardKey.Null);
		editor.SwitchWindowMode(editor.Setting.WindowMode ? WindowMode.Window : WindowMode.Mascot);

		// Init Resources
		string universePath = Util.CombinePaths(AngePath.ApplicationDataPath, "Universe");
		var fonts = RaylibUtil.LoadFontDataFromFile(Util.CombinePaths(universePath, "Fonts"));
		editor.Font = fonts != null && fonts.Length > 0 ? fonts[0] : new();
		string sheetPath = AngePath.GetSheetPath(universePath);
		string artworkPath = AngePath.GetAsepriteRoot(universePath);
		SheetUtil.RecreateSheetIfArtworkModified(sheetPath, artworkPath);
		editor.Sheet.LoadFromDisk(sheetPath);

		// Init Window
		Window.CacheSheet = editor.Sheet;
		Window.CacheFont = editor.Font;
		var windowList = new List<Window>();
		foreach (var type in typeof(Window).AllChildClass()) {
			if (Activator.CreateInstance(type) is not Window window) continue;
			windowList.Add(window);
		}
		windowList.Sort((a, b) => a.Order.CompareTo(b.Order));
		editor.Windows = windowList.ToArray();
		if (windowList.Count == 0) RequireQuit = true;

		// Update
		while (!RequireQuit) {
			editor.TextUpdate();
			Raylib.BeginDrawing();
			editor.OnGUI();
			GizmosRender.UpdateGizmos();
			Raylib.EndDrawing();
			RaylibUtil.TypingBuilder.Clear();
			if (Raylib.WindowShouldClose() && (OnTryingToQuit == null || OnTryingToQuit.Invoke())) break;
		}

		// Quit
		editor.Font.Unload();
		editor.Setting.LoadValueFromWindow();
		JsonUtil.SaveJson(editor.Setting, AngePath.PersistentDataPath, prettyPrint: true);
		Raylib.CloseWindow();
		Util.InvokeAllStaticMethodWithAttribute<OnQuitAttribute>();
	}


	private void TextUpdate () {
		if (!RaylibUtil.IsTyping) return;
		int current;
		for (int safe = 0; (current = Raylib.GetCharPressed()) > 0 && safe < 1024; safe++) {
			RaylibUtil.TypingBuilder.Append((char)current);
		}
		for (int safe = 0; (current = Raylib.GetKeyPressed()) > 0 && safe < 1024; safe++) {
			switch ((KeyboardKey)current) {
				case KeyboardKey.Enter:
					RaylibUtil.TypingBuilder.Append(Const.RETURN_SIGN);
					break;
				case KeyboardKey.C:
					if (Raylib.IsKeyDown(KeyboardKey.LeftControl)) {
						RaylibUtil.TypingBuilder.Append(Const.CONTROL_COPY);
					}
					break;
				case KeyboardKey.X:
					if (Raylib.IsKeyDown(KeyboardKey.LeftControl)) {
						RaylibUtil.TypingBuilder.Append(Const.CONTROL_CUT);
					}
					break;
				case KeyboardKey.V:
					if (Raylib.IsKeyDown(KeyboardKey.LeftControl)) {
						RaylibUtil.TypingBuilder.Append(Const.CONTROL_PASTE);
					}
					break;
			}
		}
		if (RaylibUtil.IsKeyPressedOrRepeat(KeyboardKey.Backspace)) {
			RaylibUtil.TypingBuilder.Append(Const.BACKSPACE_SIGN);
		}
	}


	// GUI
	private void OnGUI () {
		Raylib.SetMouseCursor(MouseCursor.Default);
		UiScale = Raylib.GetRenderHeight() / 1000f;
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
				int monitor = Raylib.GetCurrentMonitor();
				int monitorWidth = Raylib.GetMonitorWidth(monitor);
				int monitorHeight = Raylib.GetMonitorHeight(monitor);
				int width = FLOAT_WINDOW_WIDTH * monitorHeight / 1000;
				int height = FLOAT_WINDOW_HEIGHT * monitorHeight / 1000;
				Raylib.SetWindowPosition((monitorWidth - width) / 2, (monitorHeight - height) / 2);
				Raylib.SetWindowSize(width, height);
				OnGUI_Window();
				break;
			case WindowMode.Window:
				Raylib.ClearBackground(new Color(38, 38, 38, 255));
				OnGUI_Window();
				break;
		}
	}


	// Window
	private void OnGUI_Window () {

		int screenWidth = Raylib.GetRenderWidth();
		int screenheight = Raylib.GetRenderHeight();

		// Tab Bar
		int barPadding = Sheet.SpritePool.TryGetValue(UI_WINDOW_BG, out var bgSprite) ?
			bgSprite.GlobalBorder.left : 5;
		int barHeight = Unify(58);
		OnGUI_TabBar(barHeight, RaylibUtil.GetUnifyBorder(barPadding, screenheight));

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

		int contentPadding = Unify(12);
		int screenWidth = Raylib.GetRenderWidth();
		int tabWidth = (screenWidth - padding * 2) / Windows.Length;
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
				selecting || hovering ? Color.White : Color.LightGray
			);
			var contentRect = rect.Shrink(contentPadding, contentPadding, 0, contentPadding);

			// Icon
			float iconSize = contentRect.Height;
			if (window.Icon != 0) {
				Sheet.Draw(window.Icon, contentRect.EdgeInside(Direction4.Left, iconSize));
			}

			// Label
			Font.DrawLabel(
				TextContent.Get(window.Title, Color32.GREY_196, charSize: 20, alignment: Alignment.MidLeft),
				contentRect.Shrink(window.Icon != 0 ? iconSize : contentPadding, 0, 0, 0)
			);

			// Click
			if (mousePress && hovering) {
				CurrentWindowIndex = i;
			}

			// Next
			rect.X += rect.Width;
		}
	}


	// Mascot
	private void OnGUI_Mascot_MouseLogic () {
		// Mouse Logic
		var mousePos = Raylib.GetMousePosition();
		const float DRAG_TO_MOVE_GAP = 1600f;
		if (Raylib.IsMouseButtonDown(MouseButton.Left)) {
			if (!FloatMascotMouseDownPos.HasValue) {
				// Mouse Down
				FloatMascotMouseDownPos = mousePos;
				FloatMascotDragged = false;
			} else {
				if (FloatMascotDragged || Vector2.DistanceSquared(mousePos, FloatMascotMouseDownPos.Value) > DRAG_TO_MOVE_GAP) {
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


	#endregion




	#region --- API ---


	public static void Quit () => RequireQuit = true;

	public static int Unify (float value) => (int)(value * UiScale);

	public static int Unify (int value) => (int)(value * UiScale);


	#endregion




	#region --- LGC ---


	private void SwitchWindowMode (WindowMode newMode) {

		// Cache
		if (Setting.Initialized) {
			Setting.LoadValueFromWindow();
		}

		// Set
		switch (newMode) {
			case WindowMode.Mascot:
				// Mascot
				Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
				Raylib.SetWindowState(ConfigFlags.TopmostWindow);
				Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
				Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
				Raylib.SetWindowSize(FLOAT_WIDTH, FLOAT_HEIGHT);
				Raylib.SetWindowPosition(Setting.FloatX, Setting.FloatY);
				break;
			case WindowMode.Float:
				// Float
				Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
				Raylib.SetWindowState(ConfigFlags.TopmostWindow);
				Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
				Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
				int width = Unify(FLOAT_WINDOW_WIDTH);
				int height = Unify(FLOAT_WINDOW_HEIGHT);
				int _monitor = Raylib.GetCurrentMonitor();
				int monitorWidth = Raylib.GetMonitorWidth(_monitor);
				int monitorHeight = Raylib.GetMonitorHeight(_monitor);
				Raylib.SetWindowPosition((monitorWidth - width) / 2, (monitorHeight - height) / 2);
				Raylib.SetWindowSize(width, height);
				break;
			case WindowMode.Window:
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
				break;
		}
		CurrentWindowMode = newMode;
		Setting.WindowMode = newMode == WindowMode.Window;
		Setting.Initialized = true;
	}


	#endregion




}
