using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;
using AngeliA;

namespace AngeliaEngine;

public class Engine {


	public static void Run () {
		var engine = new Engine();
		engine.Initialize();
		while (true) {
			Raylib.BeginDrawing();
			engine.OnGUI();
			Raylib.EndDrawing();
			if (Raylib.WindowShouldClose()) break;
		}
		Raylib.CloseWindow();
		Util.InvokeAllStaticMethodWithAttribute<OnGameQuitAttribute>();
	}


	private void Initialize () {
		Raylib.SetConfigFlags(
			ConfigFlags.AlwaysRunWindow |
			ConfigFlags.ResizableWindow |
			ConfigFlags.MaximizedWindow
		);
		Raylib.SetTraceLogLevel(TraceLogLevel.Warning);
		Util.OnLogException += (ex) => Console.WriteLine($"{ex.GetType().Name}\n{ex.Source}\n{ex.Message}");
		Util.OnLogWarning += (msg) => {
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(msg);
			Console.ResetColor();
		};
		Raylib.InitWindow(1024 / 9 * 16, 1024, "AngeliA Engine");
		Raylib.MaximizeWindow();
		Raylib.EnableEventWaiting();
		Raylib.SetExitKey(Raylib_cs.KeyboardKey.Null);
		Raylib.SetWindowTitle(AngeliaGameTitleAttribute.GetTitle());

	}


	private void OnGUI () {


	}


}
