using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using AngeliA;
using AngeliA.Framework;
using Raylib_cs;

namespace AngeliaToRaylib.Framework;

public partial class RayGame : Game {


	// Const
	private const long TICK_GAP = TimeSpan.TicksPerSecond / 60;

	// Event
	private event Action OnGameQuitting = null;
	private event Func<bool> OnGameTryingToQuit = null;
	private event Action<bool> OnWindowFocusChanged = null;
	private event Action<char> OnTextInput = null;

	// Data
	private readonly Stopwatch GameWatch = new();
	private bool RequireQuitGame = false;
	private bool WindowFocused = true;
	private long NextUpdateTick = -1;

	// Saving
	private readonly SavingBool WindowMaximized = new("Game.WindowMaximized", false);


	// MSG
	public void Run () {
		InitializeGame();
		while (!RequireQuitGame) {
			try {
				UpdateGame();
			} catch (Exception ex) {
				Util.LogException(ex);
			}
		}
		QuitGame();
	}


	private void InitializeGame () {

		// Init Window
		Raylib.SetTraceLogLevel(IsEdittime ? TraceLogLevel.Warning : TraceLogLevel.None);
		Raylib.SetConfigFlags(
			ConfigFlags.ResizableWindow |
			ConfigFlags.AlwaysRunWindow |
			ConfigFlags.InterlacedHint
		);
		if (RequireTransparentWindowAttribute.Required) Raylib.SetWindowState(ConfigFlags.TransparentWindow);
		if (RequireEventWaitingAttribute.Required) Raylib.EnableEventWaiting();
		Raylib.InitWindow(1024 * 16 / 9, 1024, "");
		Raylib.SetExitKey(Raylib_cs.KeyboardKey.Null);

		// Debug
		Util.OnLogException += RaylibUtil.LogException;
		Util.OnLogError += RaylibUtil.LogError;
		Util.OnLog += RaylibUtil.Log;
		Util.OnLogWarning += RaylibUtil.LogWarning;

		// Pipeline
		Fonts = RayGUI.LoadFontDataFromFile(Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Fonts"));
		InitializeShader();
		InitializeAudio();
		EMPTY_TEXTURE = (Texture2D)GetTextureFromPixels(new Color32[1] { Color32.CLEAR }, 1, 1);

		// Init AngeliA
		Initialize();

		// Raylib Window
		GameWatch.Start();
		Raylib.SetWindowTitle(AngeliaGameTitleAttribute.Title);
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
		RayGUI.BeforeUpdate(OnTextInput);

		// Wait for Fixed Update
		long currentTick = GameWatch.ElapsedTicks;
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
		base.Update();

		// Update Gizmos
		GizmosRender.UpdateGizmos();

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
		foreach (var (_, texture) in TexturePool) UnloadTexture(texture);

		// Quit Game
		WindowMaximized.Value = !Raylib.IsWindowFullscreen() && Raylib.IsWindowMaximized();
		OnGameQuitting?.Invoke();
		Raylib.CloseWindow();
	}


}
