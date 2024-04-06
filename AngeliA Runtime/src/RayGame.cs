global using Debug = AngeliA.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using AngeliA;
using Raylib_cs;

namespace AngeliaRuntime;

public partial class RayGame : Game {


	// Data
	private readonly Stopwatch GameWatch = new();
	private bool RequireQuitGame = false;
	private bool WindowFocused = true;

	// Saving
	private readonly SavingBool WindowMaximized = new("Game.WindowMaximized", false);


	// MSG
	public void Run () {
		InitializeGame();
		while (!RequireQuitGame) {
			try {
				UpdateGame();
			} catch (Exception ex) {
				Debug.LogException(ex);
			}
		}
		QuitGame();
	}


	private void InitializeGame () {

		// Init Window
		Raylib.SetTraceLogLevel(IsEdittime ? TraceLogLevel.Warning : TraceLogLevel.None);
		Raylib.SetTargetFPS(60);
		var windowConfig = ConfigFlags.ResizableWindow | ConfigFlags.AlwaysRunWindow | ConfigFlags.InterlacedHint;
		Raylib.SetConfigFlags(windowConfig);
		Raylib.ClearWindowState(ConfigFlags.HighDpiWindow);
		Raylib.InitWindow(1024 * 16 / 9, 1024, "");
		Raylib.SetExitKey(Raylib_cs.KeyboardKey.Null);
		SetWindowMinSize(256);
		Debug.OnLogException += RayUtil.LogException;
		Debug.OnLogError += RayUtil.LogError;
		Debug.OnLog += RayUtil.Log;
		Debug.OnLogWarning += RayUtil.LogWarning;

		// Pipeline
		Fonts = RayUtil.LoadFontDataFromFile(Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Fonts"));
		InitializeShader();
		InitializeAudio();
		EMPTY_TEXTURE = (Texture2D)GetTextureFromPixels(new Color32[1] { Color32.CLEAR }, 1, 1);

		// Init AngeliA
		Initialize();

		// Raylib Window
		GameWatch.Start();
		if (ProjectType == ProjectType.Game) {
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


	private void UpdateGame () {

		// Trying to Quit Check
		if (!RequireQuitGame && Raylib.WindowShouldClose()) {
			RequireQuitGame = InvokeGameTryingToQuit();
		}

		// Text Input
		RayUtil.TextInputUpdate(GUI.OnTextInput);

		// Window Focus
		bool windowFocus = Raylib.IsWindowFocused();
		if (windowFocus != WindowFocused) {
			WindowFocused = windowFocus;
			InvokeWindowFocusChanged(windowFocus);
		}

		// Music
		Raylib.UpdateMusicStream(CurrentBGM);

		// Begin Draw
		bool hasScreenEffectEnabled = false;
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			if (ScreenEffectEnables[i]) {
				hasScreenEffectEnabled = true;
				break;
			}
		}
		bool hasCustomCursor = Cursor.CurrentCursorIndex == Const.CURSOR_CUSTOM && Cursor.CustomCursorID != 0 && Raylib.IsCursorOnScreen();
		bool usingTextureMode = hasScreenEffectEnabled || hasCustomCursor;
		if (usingTextureMode) {
			if (RenderTexture.Texture.Width != ScreenWidth || RenderTexture.Texture.Height != ScreenHeight) {
				RenderTexture = Raylib.LoadRenderTexture(ScreenWidth, ScreenHeight);
				Raylib.SetTextureWrap(RenderTexture.Texture, TextureWrap.Clamp);
			}
			Raylib.BeginTextureMode(RenderTexture);
		} else {
			Raylib.BeginDrawing();
		}

		// Sky
		var skyColorA = Sky.SkyTintBottomColor;
		var skyColorB = Sky.SkyTintTopColor;
		if (skyColorA != skyColorB) {
			Raylib.DrawRectangleGradientV(
				0, 0, ScreenWidth, ScreenHeight,
				skyColorA.ToRaylib(), skyColorB.ToRaylib()
			);
		} else {
			Raylib.ClearBackground(skyColorA.ToRaylib());
		}

		// Update AngeliA
		Update();

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
		}

		// Black Side Border
		if (CameraRange.x.NotAlmostZero()) {
			int borderWidth = (int)(ScreenWidth * CameraRange.x);
			Raylib.DrawRectangle(0, 0, borderWidth, ScreenHeight, Color.Black);
			Raylib.DrawRectangle(ScreenWidth - borderWidth, 0, borderWidth, ScreenHeight, Color.Black);
		}

		// Custom Cursor
		if (hasCustomCursor && Renderer.TryGetTextureFromSheet<Texture2D>(Cursor.CustomCursorID, -1, out var texture)) {
			Raylib.BeginShaderMode(CursorShader);
			Raylib.SetShaderValueTexture(CursorShader, ShaderPropIndex_CURSOR_TEXTURE, RenderTexture.Texture);
			Raylib.SetShaderValue(
				CursorShader, ShaderPropIndex_CURSOR_SIZE,
				new Vector2(ScreenWidth, ScreenHeight), ShaderUniformDataType.Vec2
			);
			float scale = ScreenHeight / 1024f;
			Raylib.DrawTextureEx(
				texture,
				Raylib.GetMousePosition() - new Vector2(
					texture.Width * scale / 2f,
					texture.Height * scale / 2f
				),
				rotation: 0, scale, Color.White
			);
			Raylib.EndShaderMode();
		}

		// Render Texture >> Screen
		if (usingTextureMode) {
			Raylib.EndTextureMode();
			Raylib.BeginDrawing();
			Raylib.DrawTextureRec(
				RenderTexture.Texture,
				new Rectangle(0, 0, RenderTexture.Texture.Width, -RenderTexture.Texture.Height),
				new Vector2(0, 0), Color.White
			);
		}

		// End Game Rendering
		Raylib.EndDrawing();

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

		// Quit Game
		WindowMaximized.Value = !Raylib.IsWindowFullscreen() && Raylib.IsWindowMaximized();
		InvokeGameQuitting();
		Raylib.CloseWindow();
	}


#if DEBUG
	[OnGameQuitting(int.MaxValue)]
	internal static void CloseCMD () => Process.GetProcessesByName("WindowsTerminal").ToList().ForEach(item => item.CloseMainWindow());
#endif


}
