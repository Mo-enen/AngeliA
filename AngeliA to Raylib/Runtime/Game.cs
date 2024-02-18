using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using AngeliA;
using AngeliA.Framework;
using Raylib_cs;

[assembly: AngeliA]

namespace AngeliaToRaylib;

public partial class RaylibGame : Game {


	// Const
	private const long TICK_GAP = System.TimeSpan.TicksPerSecond / 60;

	// Event
	private event System.Action OnGameQuitting = null;
	private event System.Func<bool> OnGameTryingToQuit = null;
	private event System.Action<bool> OnWindowFocusChanged = null;
	private event System.Action<char> OnTextInput = null;

	// Data
	private bool RequireQuitGame = false;
	private bool WindowFocused = true;
	private long NextUpdateTick = -1;

	// Saving
	private readonly SavingBool WindowMaximized = new("Game.WindowMaximized", false);


	// MSG
	public RaylibGame () {
#if DEBUG
		string path = Util.GetParentPath(Environment.CurrentDirectory);
		for (int safe = 0; safe < 12; safe++) {
			foreach (var filePath in Util.EnumerateFiles(path, true, "*.csproj")) {
				Environment.CurrentDirectory = path;
				return;
			}
			path = Util.GetParentPath(path);
			if (string.IsNullOrEmpty(path)) break;
		}
#endif
	}



	public static void ExtractResourceToFile (string resourceName, string filename) {
		if (System.IO.File.Exists(filename)) return;
		using var s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
		using var fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);
		byte[] b = new byte[s.Length];
		s.Read(b, 0, b.Length);
		fs.Write(b, 0, b.Length);
	}


	public static void Run () {
		var game = new RaylibGame();
		game.InitializeGame();
		while (!game.RequireQuitGame) {
			try {
				game.UpdateGame();
			} catch (System.Exception ex) {
				LogException(ex);
			}
		}
		game.QuitGame();
	}


	private void InitializeGame () {

		// Init Window
		Raylib.SetTraceLogLevel(IsEdittime ? TraceLogLevel.Warning : TraceLogLevel.None);
		Raylib.SetConfigFlags(
			ConfigFlags.ResizableWindow |
			ConfigFlags.AlwaysRunWindow |
			ConfigFlags.InterlacedHint
		);
		Raylib.InitWindow(1024 * 16 / 9, 1024, "");
		Raylib.SetExitKey(Raylib_cs.KeyboardKey.Null);

		// Pipeline
		InitializeFont();
		InitializeShader();
		InitializeAudio();
		EMPTY_TEXTURE = (Texture2D)GetTextureFromPixels(new Color32[1] { Color32.CLEAR }, 1, 1);

		// Init AngeliA
		base.Initialize();

		// Raylib Window
		Raylib.SetWindowTitle(GameTitle);
		if (WindowMaximized.Value) {
			Raylib.MaximizeWindow();
		} else if (!Raylib.IsWindowFullscreen()) {
			Raylib.SetWindowPosition(
				(Raylib.GetMonitorWidth(Raylib.GetCurrentMonitor()) - _GetScreenWidth()) / 2,
				(Raylib.GetMonitorHeight(Raylib.GetCurrentMonitor()) - _GetScreenHeight()) / 2
			);
		}

	}


	private void UpdateGame () {

		// Text Input
		Update_TextInput();

		// Wait for Fixed Update
		long currentTick = TicksSinceStart;
		if (currentTick < NextUpdateTick) goto _CONTINUE_;
		NextUpdateTick = currentTick + TICK_GAP;

		// Window Focus
		bool windowFocus = Raylib.IsWindowFocused();
		if (windowFocus != WindowFocused) {
			WindowFocused = windowFocus;
			OnWindowFocusChanged?.Invoke(windowFocus);
		}

		// Music
		Raylib.UpdateMusicStream(CurrentBGM);

		// Begin Draw
		bool hasScreenEffectEnabled = PrepareScreenEffects();
		if (hasScreenEffectEnabled) {
			Raylib.BeginTextureMode(RenderTexture);
		} else {
			Raylib.BeginDrawing();
		}

		// Sky
		Raylib.DrawRectangleGradientV(
			0, 0, ScreenWidth, ScreenHeight,
			Sky.SkyTintBottomColor.ToRaylib(), Sky.SkyTintTopColor.ToRaylib()
		);

		// Update AngeliA
		base.GameUpdate();
		base.GraphicUpdate();

		// Update Gizmos
		UpdateGizmos();

		// End Update
		if (hasScreenEffectEnabled) {
			// Screen Effect >> Render Texture
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
			Raylib.EndTextureMode();

			// Render Texture >> Screen
			Raylib.BeginDrawing();
			Raylib.DrawTextureRec(
				RenderTexture.Texture,
				new Rectangle(0, 0, RenderTexture.Texture.Width, -RenderTexture.Texture.Height),
				new(0, 0),
				Color.White
			);
		}

		// Black Side Border
		if (CameraRange.x.NotAlmostZero()) {
			int borderWidth = (int)(ScreenWidth * CameraRange.x);
			Raylib.DrawRectangle(0, 0, borderWidth, ScreenHeight, Color.Black);
			Raylib.DrawRectangle(ScreenWidth - borderWidth, 0, borderWidth, ScreenHeight, Color.Black);
		}

		// Final
		Raylib.EndDrawing();
		_CONTINUE_:;

		// Trying to Quit Check
		if (!RequireQuitGame && Raylib.WindowShouldClose()) {
			RequireQuitGame = OnGameTryingToQuit != null && OnGameTryingToQuit.Invoke();
		}

	}


	private void QuitGame () {

		// Unload Font
		foreach (var font in Fonts) font.Unload();

		// Unload Shader
		Raylib.UnloadShader(LerpShader);
		Raylib.UnloadShader(ColorShader);
		Raylib.UnloadShader(TextShader);
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) Raylib.UnloadShader(ScreenEffectShaders[i]);

		// Unload Texture
		Raylib.UnloadTexture(EMPTY_TEXTURE);
		Raylib.UnloadRenderTexture(RenderTexture);
		foreach (var (_, texture) in TexturePool) Game.UnloadTexture(texture);

		// Quit Game
		WindowMaximized.Value = !Raylib.IsWindowFullscreen() && Raylib.IsWindowMaximized();
		OnGameQuitting?.Invoke();
		Raylib.CloseWindow();

	}


}
