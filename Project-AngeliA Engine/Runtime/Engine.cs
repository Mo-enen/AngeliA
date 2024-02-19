using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;
using AngeliA;
using KeyboardKey = Raylib_cs.KeyboardKey;

namespace AngeliaEngine;

public class Engine {


	// Data
	private Window CurrentWindow = null;
	private EngineSetting Setting;


	// API
	public static void Open () {
		var engine = new Engine();
		engine.Setting = JsonUtil.LoadOrCreateJson<EngineSetting>(AngePath.PersistentDataPath);
		engine.Initialize();
		while (true) {
			Raylib.BeginDrawing();
			engine.OnGUI();
			Raylib.EndDrawing();
			if (Raylib.WindowShouldClose()) break;
		}
		engine.Setting.LoadValueFromWindow();
		JsonUtil.SaveJson(engine.Setting, AngePath.PersistentDataPath, prettyPrint: true);
		Util.InvokeAllStaticMethodWithAttribute<OnGameQuitAttribute>();
		Raylib.CloseWindow();
	}


	// MSG
	private void Initialize () {
		Raylib.SetTraceLogLevel(TraceLogLevel.Warning);
		Util.OnLogException += (ex) => Console.WriteLine($"{ex.GetType().Name}\n{ex.Source}\n{ex.Message}");
		Util.OnLogWarning += (msg) => {
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(msg);
			Console.ResetColor();
		};
		Raylib.InitWindow(1024 / 9 * 16, 1024, $"{AngeliaGameTitleAttribute.GetTitle()} {AngeliaVersionAttribute.GetVersionString()}");
		SwitchWindowMode(Setting.WindowMode);
		Raylib.EnableEventWaiting();
		Raylib.SetExitKey(KeyboardKey.Null);

	}


	private void OnGUI () {

		OnGUI_DragFloatToMove();

		Raylib.ClearBackground(new Color(0, Setting.WindowMode ? 64 : 0, 0, 255));

		Raylib.DrawRectangle(36, 36, Raylib.GetRenderWidth() - 72, Raylib.GetRenderHeight() - 72, Color.DarkBlue);

		if (Raylib.IsMouseButtonPressed(MouseButton.Middle)) {
			SwitchWindowMode(!Setting.WindowMode);
		}


	}


	private void OnGUI_DragFloatToMove () {
		if (Setting.WindowMode) return;
		var mousePos = Raylib.GetMousePosition();



	}


	// UTL
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
				Raylib.SetWindowPosition(Setting.WindowPositionX, Setting.WindowPositionY);
				Raylib.SetWindowSize(Setting.WindowSizeX, Setting.WindowSizeY);
			}
		} else {
			// Float
			Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
			Raylib.SetWindowState(ConfigFlags.TopmostWindow);
			Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
			Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
			CurrentWindow = null;
			Raylib.SetWindowSize(360, 360);
			Raylib.SetWindowPosition(Setting.FloatX, Setting.FloatY);
		}
		Setting.WindowMode = windowMode;
		Setting.Initialized = true;
	}


}
