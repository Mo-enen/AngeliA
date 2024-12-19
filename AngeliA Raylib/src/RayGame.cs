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


	// SUB
	private enum AltTextureMode { Gizmos, Doodle, }


	// Api
#if DEBUG
	private readonly bool CloseWindowsTerminalOnQuit = true;
#endif

	// Data
	private readonly Stopwatch GameWatch = new();
	private AltTextureMode CurrentAltTextureMode;
	private bool RequireQuitGame = false;
	private bool WindowFocused = true;
	private int IgnoreGizmosFrame = -1;
	private int CurrentBgmID = 0;
	private bool HasScreenEffectEnabled;
	private int PrevFPS = -1;

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
				} else {
					Raylib.EndDrawing();
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
		EMPTY_IMG = Raylib.LoadImageFromTexture(RayUtil.EMPTY_TEXTURE);

		// Blend
		RayUtil.SetBlendFactorsForGeneral();

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

	}


	private void UpdateGame () {

		// Music
		if (CurrentBGM != null) Raylib.UpdateMusicStream((Music)CurrentBGM);

		// Begin Draw
		HasScreenEffectEnabled = false;
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			if (ScreenEffectEnables[i]) {
				HasScreenEffectEnabled = true;
				break;
			}
		}

		// Render Textures
		UpdateRenderTextures();

		// File Drop
		if (Raylib.IsFileDropped()) {
			foreach (string path in Raylib.GetDroppedFiles()) {
				InvokeFileDropped(path);
			}
		}

		// Update AngeliA
		SwitchToGizmosTextureMode();
		Raylib.ClearBackground(Color.Blank);
		Update();
		Raylib.EndBlendMode();
		int fps = GetTargetFramerate();
		if (fps != PrevFPS) {
			PrevFPS = fps;
			Raylib.SetTargetFPS(fps);
		}

		// Black Side Border
		if (Renderer.CameraRange.x.NotAlmostZero()) {
			int currentScreenW = _GetScreenWidth();
			int currentScreenH = _GetScreenHeight();
			int borderWidth = (int)(currentScreenW * Renderer.CameraRange.x);
			Raylib.DrawRectangle(0, 0, borderWidth, currentScreenH, Color.Black);
			Raylib.DrawRectangle(currentScreenW - borderWidth, 0, borderWidth, currentScreenH, Color.Black);
		}

		// End Rendering
		DrawAllScreenEffects();
		Raylib.EndTextureMode();
		Raylib.BeginDrawing();
		Raylib.DrawTextureRec(
			RenderTexture.Texture,
			new Rectangle(0, 0, RenderTexture.Texture.Width, -RenderTexture.Texture.Height),
			default, Color.White
		);

		// Front Doodle
		if (GlobalFrame <= DoodleFrame + 1 && GlobalFrame <= DoodleOnTopOfUiFrame + 1) {
			DrawDoodleTexture();
		}

		// Front Gizmos
		if (GlobalFrame <= GizmosOnTopOfUiFrame + 1) {
			Raylib.DrawTextureRec(
				GizmosRenderTexture.Texture,
				new Rectangle(0, 0, GizmosRenderTexture.Texture.Width, -GizmosRenderTexture.Texture.Height),
				new Vector2(0, 0), Color.White
			);
		}

		// End
		Raylib.EndBlendMode();
		Raylib.EndDrawing();
	}


	private void UpdateRenderTextures () {

		int currentScreenW = _GetScreenWidth();
		int currentScreenH = _GetScreenHeight();

		// Texture Size Changed
		if (RenderTexture.Texture.Width != currentScreenW || RenderTexture.Texture.Height != currentScreenH) {
			Raylib.UnloadRenderTexture(RenderTexture);
			RenderTexture = Raylib.LoadRenderTexture(currentScreenW, currentScreenH);
			Raylib.SetTextureWrap(RenderTexture.Texture, TextureWrap.Clamp);
			//Debug.Log("Render Texture Reloaded.");
		}
		if (GizmosRenderTexture.Texture.Width != currentScreenW || GizmosRenderTexture.Texture.Height != currentScreenH) {
			Raylib.UnloadRenderTexture(GizmosRenderTexture);
			GizmosRenderTexture = Raylib.LoadRenderTexture(currentScreenW, currentScreenH);
			Raylib.SetTextureWrap(GizmosRenderTexture.Texture, TextureWrap.Clamp);
			//Debug.Log("Gizmos Texture Reloaded.");
		}
		if (AlphaLayerTexture.Texture.Width != currentScreenW || AlphaLayerTexture.Texture.Height != currentScreenH) {
			Raylib.UnloadRenderTexture(AlphaLayerTexture);
			AlphaLayerTexture = Raylib.LoadRenderTexture(currentScreenW, currentScreenH);
			Raylib.SetTextureWrap(AlphaLayerTexture.Texture, TextureWrap.Clamp);
			//Debug.Log("Alpha Texture Reloaded.");
		}
		int doodleTextureWidth = (currentScreenW - DoodleScreenPadding.horizontal).GreaterOrEquel(1);
		int doodleTextureHeight = (currentScreenH - DoodleScreenPadding.vertical).GreaterOrEquel(1);
		if (DoodleRenderTexture.Texture.Width != doodleTextureWidth || DoodleRenderTexture.Texture.Height != doodleTextureHeight) {
			Raylib.UnloadRenderTexture(DoodleRenderTexture);
			DoodleRenderTexture = Raylib.LoadRenderTexture(doodleTextureWidth, doodleTextureHeight);
			Raylib.SetTextureWrap(DoodleRenderTexture.Texture, TextureWrap.Repeat);
			//Debug.Log("Doodle Texture Reloaded.");
		}

		// Texture Not Loaded
		if (!Raylib.IsRenderTextureReady(RenderTexture)) {
			RenderTexture = Raylib.LoadRenderTexture(currentScreenW, currentScreenH);
			Raylib.SetTextureWrap(RenderTexture.Texture, TextureWrap.Clamp);
			Debug.LogWarning("Render Texture Force Reloaded. This should not happen.");
		}
		if (!Raylib.IsRenderTextureReady(GizmosRenderTexture)) {
			GizmosRenderTexture = Raylib.LoadRenderTexture(currentScreenW, currentScreenH);
			Raylib.SetTextureWrap(GizmosRenderTexture.Texture, TextureWrap.Clamp);
			Debug.LogWarning("Gizmos Render Texture Force Reloaded. This should not happen.");
		}
		if (!Raylib.IsRenderTextureReady(AlphaLayerTexture)) {
			AlphaLayerTexture = Raylib.LoadRenderTexture(currentScreenW, currentScreenH);
			Raylib.SetTextureWrap(AlphaLayerTexture.Texture, TextureWrap.Clamp);
			Debug.LogWarning("Alpha Layer Texture Force Reloaded. This should not happen.");
		}
		if (!Raylib.IsRenderTextureReady(DoodleRenderTexture)) {
			DoodleRenderTexture = Raylib.LoadRenderTexture(doodleTextureWidth, doodleTextureHeight);
			Raylib.SetTextureWrap(DoodleRenderTexture.Texture, TextureWrap.Repeat);
			Debug.LogWarning("Doodle Render Texture Force Reloaded. This should not happen.");
		}
	}


	private void DrawAllScreenEffects () {
		if (!HasScreenEffectEnabled) return;
		UpdateScreenEffect();
		var rect = new Rectangle(0, 0, RenderTexture.Texture.Width, RenderTexture.Texture.Height);
		rect.X += ScreenEffectPadding.left;
		rect.Width -= ScreenEffectPadding.left;
		rect.Y += ScreenEffectPadding.down;
		rect.Height -= ScreenEffectPadding.down;
		rect.Width -= ScreenEffectPadding.right;
		rect.Height -= ScreenEffectPadding.up;
		rect.Height = -rect.Height;
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			if (!ScreenEffectEnables[i]) continue;
			// Draw Texture with Shader
			Raylib.BeginShaderMode(ScreenEffectShaders[i]);
			Raylib.DrawTextureRec(RenderTexture.Texture, rect, new Vector2(ScreenEffectPadding.left, ScreenEffectPadding.down), Color.White);
			Raylib.EndShaderMode();
		}
	}


	private void QuitGame () {

		// Unload Shader
		Raylib.UnloadShader(LerpShader);
		Raylib.UnloadShader(ColorShader);
		Raylib.UnloadShader(InverseShader);

		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) Raylib.UnloadShader(ScreenEffectShaders[i]);

		// Unload Texture
		Raylib.UnloadRenderTexture(RenderTexture);

		// Quit Game
		WindowMaximized.Value = !Raylib.IsWindowFullscreen() && Raylib.IsWindowMaximized();
		try {
			InvokeGameQuitting();
		} catch (Exception ex) { Debug.LogException(ex); }

		Raylib.CloseWindow();

#if DEBUG
		if (CloseWindowsTerminalOnQuit) {
			Process.GetProcessesByName(
				"WindowsTerminal"
			).ToList().ForEach(item => item.CloseMainWindow());
		}
#endif

	}


	private void SwitchToGizmosTextureMode () {
		Raylib.BeginTextureMode(GizmosRenderTexture);
		Raylib.BeginBlendMode(BlendMode.AlphaPremultiply);
		CurrentAltTextureMode = AltTextureMode.Gizmos;
	}


	private void SwitchToDoodleTextureMode () {
		Raylib.BeginTextureMode(DoodleRenderTexture);
		Raylib.BeginBlendMode(BlendMode.AlphaPremultiply);
		CurrentAltTextureMode = AltTextureMode.Doodle;
	}


	private void DrawDoodleTexture () {
		var sourceRect = new Rectangle(
			DoodleRenderingOffset.x,
			-DoodleRenderingOffset.y,
			DoodleRenderTexture.Texture.Width,
			DoodleRenderTexture.Texture.Height
		);
		var dstRect = new Rectangle(
			DoodleScreenPadding.left, DoodleScreenPadding.down,
			DoodleRenderTexture.Texture.Width,
			DoodleRenderTexture.Texture.Height
		);
		if (DoodleRenderingZoom.NotAlmost(1f)) {
			float expX = (dstRect.Width * DoodleRenderingZoom - dstRect.Width) / 2f;
			float expY = (dstRect.Height * DoodleRenderingZoom - dstRect.Height) / 2f;
			dstRect = dstRect.ExpandRectangle(expX, expX, expY, expY);
		}
		Raylib.DrawTexturePro(
			DoodleRenderTexture.Texture,
			sourceRect, dstRect,
			Vector2.Zero, 0f, Color.White
		);
	}


}
