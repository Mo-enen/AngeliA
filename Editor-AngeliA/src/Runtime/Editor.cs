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


public class Editor {




	#region --- VAR ---


	public class TestWindow0 : Window {
		public TestWindow0 () {
			Title = "Title 0";
		}
		public override void DrawWindow (Rectangle windowRect) {
			//Raylib.DrawRectangle(36, 36, Raylib.GetRenderWidth() - 72, Raylib.GetRenderHeight() - 72, Color.DarkBlue);
		}
	}
	public class TestWindow1 : Window {
		public TestWindow1 () {
			Order = int.MaxValue;
			Title = "Title 1";
		}
		public override void DrawWindow (Rectangle windowRect) {
			//Raylib.DrawRectangle(36, 36, Raylib.GetRenderWidth() - 72, Raylib.GetRenderHeight() - 72, Color.DarkBlue);
		}
	}
	public class TestWindow2 : Window {
		public TestWindow2 () {
			Title = "Title 2";
		}
		public override void DrawWindow (Rectangle windowRect) {
			//Raylib.DrawRectangle(36, 36, Raylib.GetRenderWidth() - 72, Raylib.GetRenderHeight() - 72, Color.DarkBlue);
		}
	}


	// Const
	private const int FLOAT_WIDTH = 360;
	private const int FLOAT_HEIGHT = 360;
	private const int FLOAT_WINDOW_WIDTH = 1000;
	private const int FLOAT_WINDOW_HEIGHT = 800;
	private const int TAB_BAR_HEIGHT = 32;

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
	private bool ShowingFloatMascot = true;
	private bool FloatMascotDragged = false;


	#endregion




	#region --- MSG ---


	public static void Run () {
		var editor = new Editor();
		editor.Init_Editor();
		editor.Init_Resources();
		editor.Init_Window();
		while (!RequireQuit) {
			Raylib.BeginDrawing();
			editor.OnGUI();
			GizmosRender.UpdateGizmos();
			Raylib.EndDrawing();
			if (Raylib.WindowShouldClose() && (OnTryingToQuit == null || OnTryingToQuit.Invoke())) break;
		}
		editor.OnQuit();
		Raylib.CloseWindow();
		Util.InvokeAllStaticMethodWithAttribute<OnQuitAttribute>();
	}


	// Init
	private void Init_Editor () {
		RequireQuit = false;
		Util.LinkEventWithAttribute<OnTryingToQuitAttribute>(typeof(Editor), nameof(OnTryingToQuit));
		Setting = JsonUtil.LoadOrCreateJson<Setting>(AngePath.PersistentDataPath);
		Raylib.SetTraceLogLevel(TraceLogLevel.Warning);
		AngeliA.Util.OnLogException += LogException;
		AngeliA.Util.OnLogError += LogError;
		AngeliA.Util.OnLog += Log;
		AngeliA.Util.OnLogWarning += LogWarning;
		Raylib.InitWindow(1024 / 9 * 16, 1024, $"{AngeliaGameTitleAttribute.GetTitle()} {AngeliaVersionAttribute.GetVersionString()}");
		SwitchWindowMode(Setting.WindowMode);
		Raylib.EnableEventWaiting();
		Raylib.SetExitKey(KeyboardKey.Null);
	}


	private void Init_Resources () {
		string universePath = Util.CombinePaths(AngePath.ApplicationDataPath, "Universe");
		// Font
		var fonts = RaylibUtil.LoadFontDataFromFile(AngeliA.Util.CombinePaths(universePath, "Fonts"));
		Font = fonts != null && fonts.Length > 0 ? fonts[0] : new();
		// Sheet
		string sheetPath = AngePath.GetSheetPath(universePath);
		string artworkPath = AngePath.GetArtworkRoot(universePath);
		SheetUtil.RecreateSheetIfArtworkModified(sheetPath, artworkPath);
		Sheet.LoadFromDisk(sheetPath);
	}


	private void Init_Window () {
		var windowList = new List<Window>();
		foreach (var type in typeof(Window).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not Window window) continue;
			windowList.Add(window);
		}
		windowList.Sort((a, b) => a.Order.CompareTo(b.Order));
		Windows = windowList.ToArray();
	}


	// Quit
	private void OnQuit () {
		Font.Unload();
		Setting.LoadValueFromWindow();
		JsonUtil.SaveJson(Setting, AngePath.PersistentDataPath, prettyPrint: true);
	}


	// GUI
	private void OnGUI () {
		UiScale = Raylib.GetMonitorHeight(Raylib.GetCurrentMonitor()) / 1000f;
		CurrentWindowIndex = CurrentWindowIndex.Clamp(0, Windows.Length - 1);
		if (Setting.WindowMode) {
			OnGUI_Window();
		} else if (ShowingFloatMascot) {
			OnGUI_FloatMascot();
		} else {
			OnGUI_FloatWindow();
		}
	}


	// Window
	private void OnGUI_Window () {

		Raylib.ClearBackground(new Color(0, 0, 0, 255));

		if (Raylib.IsMouseButtonPressed(MouseButton.Middle)) {
			SwitchWindowMode(!Setting.WindowMode);
			return;
		}

		int barHeight = Unify(TAB_BAR_HEIGHT);
		OnGUI_TabBar(barHeight);
		var window = Windows[CurrentWindowIndex];
		window.DrawWindow(new Rectangle(0, barHeight, Raylib.GetRenderWidth(), Raylib.GetRenderHeight() - barHeight));

	}


	// Float Window
	private void OnGUI_FloatWindow () {

		Raylib.ClearBackground(new Color(0, 0, 0, 255));

		if (!Raylib.IsWindowFocused()) {
			ShowingFloatMascot = true;
			Raylib.SetWindowSize(FLOAT_WIDTH, FLOAT_HEIGHT);
			Raylib.SetWindowPosition(Setting.FloatX, Setting.FloatY);
			return;
		}

		int width = Unify(FLOAT_WINDOW_WIDTH);
		int height = Unify(FLOAT_WINDOW_HEIGHT);
		int monitor = Raylib.GetCurrentMonitor();
		int monitorWidth = Raylib.GetMonitorWidth(monitor);
		int monitorHeight = Raylib.GetMonitorHeight(monitor);
		int barHeight = Unify(TAB_BAR_HEIGHT);
		Raylib.SetWindowPosition((monitorWidth - width) / 2, (monitorHeight - height) / 2);
		Raylib.SetWindowSize(width, height);
		OnGUI_TabBar(barHeight);
		var window = Windows[CurrentWindowIndex];
		window.DrawWindow(new Rectangle(0, barHeight, width, height - barHeight));

	}


	// Float Mascot
	private void OnGUI_FloatMascot () {
		OnGUI_FloatMascot_MouseLogic();
		OnGUI_FloatMascot_Render();
	}


	private void OnGUI_FloatMascot_MouseLogic () {
		// Switch Mode
		if (Raylib.IsMouseButtonPressed(MouseButton.Middle)) {
			SwitchWindowMode(!Setting.WindowMode);
			return;
		}
		// Mouse Logic
		var mousePos = Raylib.GetMousePosition();
		const float DRAG_TO_MOVE_GAP = 20f;
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
						FloatMascotMouseDownGlobalPos = Raylib.GetWindowPosition() + mousePos;
					} else {
						// Dragging
						var windowPos = Raylib.GetWindowPosition();
						var mouseGlobalPos = windowPos + mousePos;
						windowPos =
							(FloatMascotMouseDownGlobalPos - FloatMascotMouseDownPos.Value) +
							(mouseGlobalPos - FloatMascotMouseDownGlobalPos);
						Raylib.SetWindowPosition((int)windowPos.X, (int)windowPos.Y);
					}
				}
			}
		}
		if (FloatMascotMouseDownPos.HasValue && !Raylib.IsMouseButtonDown(MouseButton.Left)) {
			// Mouse Up
			FloatMascotMouseDownPos = null;
			if (!FloatMascotDragged) {
				// Click
				ShowingFloatMascot = false;
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


	private void OnGUI_FloatMascot_Render () {

		Raylib.ClearBackground(new Color(0, 0, 0, 0));
		var panelRect = new Rectangle(0, 0, Raylib.GetRenderWidth(), Raylib.GetRenderHeight());

		// BG



	}


	// Bar
	private void OnGUI_TabBar (int barHeight) {
		var barRect = new Rectangle(0, 0, Raylib.GetRenderWidth(), barHeight);

		Sheet.Draw("Circle16".AngeHash(), new Rectangle(12, 128, 1024, 1024), Color32.RED_BETTER);

		Font.DrawLabel(
			CellContent.Get("Test中文", Color32.WHITE, 96),
			new Rectangle(12, 128, 1024, 1024),
			-1, 0, false, out _, out _, out _
		);



	}


	#endregion




	#region --- API ---


	public static void Quit () => RequireQuit = true;

	public static int Unify (float value) => (int)(value * UiScale);

	public static int Unify (int value) => (int)(value * UiScale);


	#endregion




	#region --- LGC ---


	private void SwitchWindowMode (bool windowMode) {

		// Cache
		if (Setting.Initialized) {
			Setting.LoadValueFromWindow();
		}

		// Set
		if (windowMode) {
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
		} else {
			// Float
			Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
			Raylib.SetWindowState(ConfigFlags.TopmostWindow);
			Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
			Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
			Raylib.SetWindowSize(FLOAT_WIDTH, FLOAT_HEIGHT);
			Raylib.SetWindowPosition(Setting.FloatX, Setting.FloatY);
			ShowingFloatMascot = true;
		}
		Setting.WindowMode = windowMode;
		Setting.Initialized = true;
	}


	// Log
	private void Log (object msg) {
		System.Console.ResetColor();
		Console.WriteLine(msg);
	}

	private void LogWarning (object msg) {
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine(msg);
		Console.ResetColor();
	}

	private void LogError (object msg) {
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(msg);
		Console.ResetColor();
	}

	private void LogException (System.Exception ex) {
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(ex.Source);
		Console.WriteLine(ex.GetType().Name);
		Console.WriteLine(ex.Message);
		System.Console.WriteLine();
		Console.ResetColor();
	}


	#endregion




}
