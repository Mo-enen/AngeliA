global using Debug = AngeliA.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using AngeliA;
using Raylib_cs;
using KeyboardKey = Raylib_cs.KeyboardKey;

namespace AngeliaRaylib;

public partial class RayGame : Game {


	// Api
#if DEBUG
	private readonly bool CloseWindowsTerminalOnQuit = true;
#endif

	// Data
	private readonly Stopwatch GameWatch = new();
	private bool RequireQuitGame = false;
	private bool WindowFocused = true;
	private int IgnoreGizmosFrame = -1;
	private int CurrentBgmID = 0;

	// Saving
	private static readonly SavingBool WindowMaximized = new("Game.WindowMaximized", false, SavingLocation.Global);


	// MSG
	static RayGame () => Util.AddAssembly(typeof(RayGame).Assembly);


	public RayGame (string[] args) : base(args) {
#if DEBUG
		CloseWindowsTerminalOnQuit = !args.Any(arg => arg.Equals("DontCloseCMD", StringComparison.OrdinalIgnoreCase));
#endif
	}


	public void Run () {
		InitializeGame();
		while (!RequireQuitGame) {
			try {
				UpdateWindow();
				if (!Raylib.IsWindowMinimized()) {
					UpdateGame();
				}
			} catch (Exception ex) {
				Debug.LogException(ex);
			}
		}
		QuitGame();
	}


	private void InitializeGame () {

		// Init Window
#if DEBUG
		Raylib.SetTraceLogLevel(TraceLogLevel.Error);
		Debug.OnLogException += LogException;
		Debug.OnLogError += LogError;
		Debug.OnLog += Log;
		Debug.OnLogWarning += LogWarning;
		static void Log (object msg) {
			Console.ResetColor();
			Console.WriteLine(msg);
		}
		static void LogWarning (object msg) {
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(msg);
			Console.ResetColor();
		}
		static void LogError (object msg) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(msg);
			Console.ResetColor();
		}
		static void LogException (Exception ex) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(ex.Source);
			Console.WriteLine(ex.GetType().Name);
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			Console.WriteLine();
			Console.ResetColor();
		}
#else
		Raylib.SetTraceLogLevel(TraceLogLevel.None);
#endif

		Raylib.SetTargetFPS(60);

		var windowConfig =
			ConfigFlags.ResizableWindow |
			ConfigFlags.AlwaysRunWindow |
			ConfigFlags.VSyncHint;
		Raylib.SetConfigFlags(windowConfig);
		Raylib.ClearWindowState(ConfigFlags.HighDpiWindow);
		Raylib.InitWindow(1024 * 16 / 9, 1024, "");
		Raylib.SetExitKey(KeyboardKey.Null);
		SetWindowMinSize(256);
		Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

		// Blend
		Rlgl.SetBlendFactorsSeparate(
			Rlgl.SRC_ALPHA,
			Rlgl.ONE_MINUS_SRC_ALPHA,
			Rlgl.ONE,
			Rlgl.ONE_MINUS_SRC_ALPHA,
			Rlgl.FUNC_ADD,
			Rlgl.FUNC_ADD
		);

		// Pipeline
		InitializeShader();
		Raylib.InitAudioDevice();

		// Init AngeliA
		Initialize();

		// Raylib Window
		GameWatch.Start();
		if (!IsToolApplication) {
			if (WindowMaximized.Value) {
				Raylib.MaximizeWindow();
			} else if (!Raylib.IsWindowFullscreen()) {
				Raylib.SetWindowPosition(
					(Raylib.GetMonitorWidth(Raylib.GetCurrentMonitor()) - _GetScreenWidth()) / 2,
					(Raylib.GetMonitorHeight(Raylib.GetCurrentMonitor()) - _GetScreenHeight()) / 2
				);
			}
		}

	}


	private void UpdateWindow () {

		// Fix Window Pos in Screen
		var windowPos = new Rectangle(
			Raylib.GetWindowPosition(), new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight())
		).ToAngelia();
		int monitor = Raylib.GetCurrentMonitor();
		var monitorRect = new IRect(0, 0, Raylib.GetMonitorWidth(monitor), Raylib.GetMonitorHeight(monitor));
		monitorRect = monitorRect.Shrink(monitorRect.height / 20);
		if (!windowPos.Overlaps(monitorRect)) {
			windowPos.ClampPositionInside(monitorRect);
			Raylib.SetWindowPosition(windowPos.x, windowPos.y);
		}

		// Trying to Quit Check
		if (!RequireQuitGame && Raylib.WindowShouldClose()) {
			RequireQuitGame = InvokeGameTryingToQuit();
			if (Raylib.IsWindowMinimized()) Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
		}

		// Window Focus
		bool windowFocus = Raylib.IsWindowFocused();
		if (windowFocus != WindowFocused) {
			WindowFocused = windowFocus;
			InvokeWindowFocusChanged(windowFocus);
		}

		// Minimize Check
		if (Raylib.IsWindowMinimized()) {
			Raylib.EndDrawing();
		}

	}


	private void UpdateGame () {

		// Music
		if (CurrentBGM != null) Raylib.UpdateMusicStream((Music)CurrentBGM);

		// Begin Draw
		bool hasScreenEffectEnabled = false;
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			if (ScreenEffectEnables[i]) {
				hasScreenEffectEnabled = true;
				break;
			}
		}
		if (RenderTexture.Texture.Width != ScreenWidth || RenderTexture.Texture.Height != ScreenHeight) {
			Raylib.UnloadRenderTexture(RenderTexture);
			RenderTexture = Raylib.LoadRenderTexture(ScreenWidth, ScreenHeight);
			GizmosRenderTexture = Raylib.LoadRenderTexture(ScreenWidth, ScreenHeight);
			Raylib.SetTextureWrap(RenderTexture.Texture, TextureWrap.Clamp);
			Raylib.SetTextureWrap(GizmosRenderTexture.Texture, TextureWrap.Clamp);
			//Debug.Log("Render Texture Reloaded.");
		}
		if (!Raylib.IsRenderTextureReady(RenderTexture)) {
			RenderTexture = Raylib.LoadRenderTexture(ScreenWidth, ScreenHeight);
			Raylib.SetTextureWrap(RenderTexture.Texture, TextureWrap.Clamp);
			Debug.LogWarning("Render Texture Force Reloaded.");
		}
		if (!Raylib.IsRenderTextureReady(GizmosRenderTexture)) {
			GizmosRenderTexture = Raylib.LoadRenderTexture(ScreenWidth, ScreenHeight);
			Raylib.SetTextureWrap(GizmosRenderTexture.Texture, TextureWrap.Clamp);
			Debug.LogWarning("Gizmos Render Texture Force Reloaded.");
		}
		DrawGizmosAtFront = IsToolApplication;

		// File Drop
		if (Raylib.IsFileDropped()) {
			foreach (string path in Raylib.GetDroppedFiles()) {
				InvokeFileDropped(path);
			}
		}

		// Update AngeliA
		Raylib.BeginTextureMode(GizmosRenderTexture);
		Raylib.ClearBackground(Color.Blank);
		Raylib.BeginBlendMode(BlendMode.CustomSeparate);
		Update();
		Raylib.EndBlendMode();

		// Screen Effect >> Render Texture
		if (hasScreenEffectEnabled) {
			UpdateScreenEffect();
			for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
				if (!ScreenEffectEnables[i]) continue;
				// Draw Texture with Shader
				Raylib.BeginShaderMode(ScreenEffectShaders[i]);
				Raylib.DrawTextureRec(
					RenderTexture.Texture,
					new Rectangle(0, 0, RenderTexture.Texture.Width, -RenderTexture.Texture.Height),
					new(0, 0),
					Color.White
				);
				Raylib.EndShaderMode();
			}
		}

		// Black Side Border
		if (Renderer.CameraRange.x.NotAlmostZero()) {
			int borderWidth = (int)(ScreenWidth * Renderer.CameraRange.x);
			Raylib.DrawRectangle(0, 0, borderWidth, ScreenHeight, Color.Black);
			Raylib.DrawRectangle(ScreenWidth - borderWidth, 0, borderWidth, ScreenHeight, Color.Black);
		}

		// End Rendering
		Raylib.EndTextureMode();
		Raylib.BeginDrawing();
		Raylib.DrawTextureRec(
			RenderTexture.Texture,
			new Rectangle(0, 0, RenderTexture.Texture.Width, -RenderTexture.Texture.Height),
			default, Color.White
		);
		if (DrawGizmosAtFront) {
			Raylib.DrawTextureRec(
				GizmosRenderTexture.Texture,
				new Rectangle(0, 0, GizmosRenderTexture.Texture.Width, -GizmosRenderTexture.Texture.Height),
				new Vector2(0, 0), Color.White
			);
		}
		Raylib.EndBlendMode();
		Raylib.EndDrawing();
	}


	private void QuitGame () {

		// Unload Shader
		Raylib.UnloadShader(LerpShader);
		Raylib.UnloadShader(ColorShader);
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) Raylib.UnloadShader(ScreenEffectShaders[i]);

		// Unload Texture
		Raylib.UnloadRenderTexture(RenderTexture);

		// Quit Game
		WindowMaximized.Value = !Raylib.IsWindowFullscreen() && Raylib.IsWindowMaximized();
		InvokeGameQuitting();
		Raylib.CloseWindow();

#if DEBUG
		if (CloseWindowsTerminalOnQuit) {
			Process.GetProcessesByName(
				"WindowsTerminal"
			).ToList().ForEach(item => item.CloseMainWindow());
		}
#endif

	}


}
