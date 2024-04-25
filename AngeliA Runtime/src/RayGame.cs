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
	private bool PrevHasInverseGizmos = false;

	// Saving
	private static readonly SavingBool WindowMaximized = new("Game.WindowMaximized", false);


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
		Raylib.SetTraceLogLevel(IsEdittime ? TraceLogLevel.Error : TraceLogLevel.None);
		Raylib.SetTargetFPS(60);
		var windowConfig = ConfigFlags.ResizableWindow | ConfigFlags.AlwaysRunWindow | ConfigFlags.InterlacedHint;
		Raylib.SetConfigFlags(windowConfig);
		Raylib.ClearWindowState(ConfigFlags.HighDpiWindow);
		Raylib.InitWindow(1024 * 16 / 9, 1024, "");
		Raylib.SetExitKey(Raylib_cs.KeyboardKey.Null);
		SetWindowMinSize(256);

#if DEBUG
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
			Console.WriteLine();
			Console.ResetColor();
		}
#endif

		Rlgl.SetBlendFactorsSeparate(
			Rlgl.SRC_ALPHA, Rlgl.ONE_MINUS_SRC_ALPHA, Rlgl.ONE, Rlgl.ONE, Rlgl.FUNC_ADD, Rlgl.MAX
		);

		// Pipeline
		Fonts = FontData.LoadFromFile(Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Fonts"));
		InitializeShader();
		InitializeAudio();
		EMPTY_TEXTURE = (Texture2D)GetTextureFromPixels(new Color32[1] { Color32.CLEAR }, 1, 1);

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
		if (RenderTexture.Texture.Width != ScreenWidth || RenderTexture.Texture.Height != ScreenHeight) {
			Raylib.UnloadRenderTexture(RenderTexture);
			RenderTexture = Raylib.LoadRenderTexture(ScreenWidth, ScreenHeight);
			Raylib.SetTextureWrap(RenderTexture.Texture, TextureWrap.Clamp);
		}
		Raylib.BeginTextureMode(RenderTexture);

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

		PrevHasInverseGizmos = GizmosRender.HasInverseGizmos;
		if (PrevHasInverseGizmos) {
			Raylib.BeginShaderMode(InverseShader);
			Raylib.SetShaderValueTexture(
				InverseShader, ShaderPropIndex_INV_TEXTURE, RenderTexture.Texture
			);
			Raylib.SetShaderValue(
				InverseShader, ShaderPropIndex_INV_SCREEN_SIZE,
				new Vector2(ScreenWidth, ScreenHeight), ShaderUniformDataType.Vec2
			);
			GizmosRender.UpdateInverse();
			Raylib.EndShaderMode();
		} else {
			GizmosRender.UpdateInverse();
		}

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
		if (CameraRange.x.NotAlmostZero()) {
			int borderWidth = (int)(ScreenWidth * CameraRange.x);
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
		Raylib.EndBlendMode();
		Raylib.EndDrawing();

	}


	private void QuitGame () {

		// Unload Shader
		Raylib.UnloadShader(LerpShader);
		Raylib.UnloadShader(ColorShader);
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
	internal static void CloseCMD () {
		if (UniverseSystem.BuiltInUniverse.Info.CloseCmdOnQuit) {
			Process.GetProcessesByName("WindowsTerminal").ToList().ForEach(item => item.CloseMainWindow());
		}
	}
#endif


}
