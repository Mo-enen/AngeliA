using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using Raylib_cs;

[assembly: AngeliA]

namespace AngeliaForRaylib;

public partial class GameForRaylib : Game {




	#region --- SUB ---


	private class GLRect {
		public IRect RaylibRect;
		public Color Color;
	}

	private class GLTexture {
		public IRect RaylibRect;
		public Texture2D Texture;
	}

	private class FontData {
		public Font Font;
		public string Name;
	}


	#endregion




	#region --- VAR ---


	// Const
	private const long TICK_GAP = System.TimeSpan.TicksPerSecond / 60;

	// Api
	public static GameForRaylib InstanceRaylib => Instance as GameForRaylib;

	// Event
	private event System.Action OnGameQuitting = null;
	private event System.Func<bool> OnGameTryingToQuit = null;
	private event System.Action<bool> OnWindowFocusChanged = null;
	private event System.Action<char> OnTextInput = null;

	// Data
	private readonly GLRect[] GLRects = new GLRect[4096].FillWithNewValue();
	private readonly GLTexture[] GLTextures = new GLTexture[196].FillWithNewValue();
	private FontData[] Fonts;
	private int GLRectCount = 0;
	private int GLTextureCount = 0;
	private bool RequireQuitGame = false;
	private bool WindowFocused = true;
	private long NextUpdateTick = -1;
	private FRect CameraScreenRect = new(0, 0, 1f, 1f);


	#endregion




	#region --- MSG ---


	public void Run () {
		InitializeGame();
		while (!RequireQuitGame) {
			try {
				UpdateGame();
			} catch (System.Exception ex) {
				LogException(ex);
			}
		}
		OnGameQuitting?.Invoke();
		Raylib.CloseWindow();
	}


	private void InitializeGame () {

		// Init Window
		Raylib.SetConfigFlags(
			ConfigFlags.ResizableWindow |
			ConfigFlags.VSyncHint |
			ConfigFlags.AlwaysRunWindow
		);
		Raylib.InitWindow(1024, 1024, "");
		Raylib.SetExitKey(Raylib_cs.KeyboardKey.Null);

		// Init Font
		string fontRoot = Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Fonts");
		var fontList = new List<FontData>(8);
		foreach (var fontPath in Util.EnumerateFiles(fontRoot, true, "*.ttf")) {
			fontList.Add(new FontData() {
				Font = Raylib.LoadFont(fontPath),
				Name = Util.GetNameWithoutExtension(fontPath),
			});
		}
		fontList.Sort((a, b) => a.Name.CompareTo(b.Name));
		Fonts = fontList.ToArray();

		// Init AngeliA
		base.Initialize();
		Raylib.SetWindowTitle(GameTitle);
		if (!Raylib.IsWindowFullscreen()) {
			Raylib.SetWindowPosition(
				(Raylib.GetMonitorWidth(Raylib.GetCurrentMonitor()) - _GetScreenWidth()) / 2,
				(Raylib.GetMonitorHeight(Raylib.GetCurrentMonitor()) - _GetScreenHeight()) / 2
			);
		}

	}


	private void UpdateGame () {

		// Text Input
		int currentChar;
		while ((currentChar = Raylib.GetCharPressed()) > 0) {
			OnTextInput?.Invoke((char)currentChar);
		}

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

		// Update Game
		Raylib.BeginDrawing();
		Raylib.DrawRectangleGradientV(
			0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(),
			Sky.SkyTintBottomColor.ToRaylib(), Sky.SkyTintTopColor.ToRaylib()
		);

		// Update AngeliA
		base.GameUpdate();
		base.GraphicUpdate();

		// Update GL Gizmos
		for (int i = 0; i < GLRectCount; i++) {
			var glRect = GLRects[i];
			var rRect = glRect.RaylibRect;
			Raylib.DrawRectangle(rRect.x, rRect.y, rRect.width, rRect.height, glRect.Color);
		}
		for (int i = 0; i < GLTextureCount; i++) {
			var glTexture = GLTextures[i];
			var rTexture = glTexture.Texture;
			Raylib.DrawTexturePro(
				rTexture,
				new Rectangle(0, 0, rTexture.Width, rTexture.Height),
				glTexture.RaylibRect.ToRaylib(),
				default, 0, Color.White
			);
		}
		GLRectCount = 0;
		GLTextureCount = 0;

		// End Draw
		Raylib.EndDrawing();

		// Final
		_CONTINUE_:;

		// Trying to Quit Check
		if (!RequireQuitGame && Raylib.WindowShouldClose()) {
			RequireQuitGame = OnGameTryingToQuit != null && OnGameTryingToQuit.Invoke();
		}

	}


	#endregion




	#region --- LGC ---


	private int Angelia_to_Raylib_X (int globalX) => Util.RemapUnclamped(
		CellRenderer.CameraRect.x, CellRenderer.CameraRect.xMax,
		ScreenRect.x, ScreenRect.xMax,
		globalX
	);


	private int Angelia_to_Raylib_Y (int globalY) => Util.RemapUnclamped(
		CellRenderer.CameraRect.y, CellRenderer.CameraRect.yMax,
		ScreenRect.yMax, ScreenRect.y,
		globalY
	);


	private IRect Angelia_to_Raylib_Rect (IRect globalRect) => IRect.MinMaxRect(
		Angelia_to_Raylib_X(globalRect.x),
		Angelia_to_Raylib_Y(globalRect.yMax),
		Angelia_to_Raylib_X(globalRect.xMax),
		Angelia_to_Raylib_Y(globalRect.y)
	);


	private static void WritePixelsToConsole (Byte4[] pixels, int width) {

		int height = pixels.Length / width;
		int realWidth = Util.Min(width, 24);
		int realHeight = height * realWidth / width;
		int scale = width / realWidth;

		for (int y = 0; y < realHeight; y++) {
			System.Console.ResetColor();
			System.Console.WriteLine();
			for (int x = 0; x < realWidth; x++) {
				var p = pixels[(y * scale).Clamp(0, height - 1) * width + (x * scale).Clamp(0, width - 1)];
				Util.RGBToHSV(p, out float h, out float s, out float v);
				System.Console.BackgroundColor = (v * s < 0.2f) ?
					(v < 0.33f ? System.ConsoleColor.Black : v > 0.66f ? System.ConsoleColor.White : System.ConsoleColor.Gray) :
					(h < 0.08f ? (v > 0.5f ? System.ConsoleColor.Red : System.ConsoleColor.DarkRed) :
					h < 0.25f ? (v > 0.5f ? System.ConsoleColor.Yellow : System.ConsoleColor.DarkYellow) :
					h < 0.42f ? (v > 0.5f ? System.ConsoleColor.Green : System.ConsoleColor.DarkGreen) :
					h < 0.58f ? (v > 0.5f ? System.ConsoleColor.Cyan : System.ConsoleColor.DarkCyan) :
					h < 0.75f ? (v > 0.5f ? System.ConsoleColor.Blue : System.ConsoleColor.DarkBlue) :
					h < 0.92f ? (v > 0.5f ? System.ConsoleColor.Magenta : System.ConsoleColor.DarkMagenta) :
					(v > 0.6f ? System.ConsoleColor.Red : System.ConsoleColor.DarkRed));
				System.Console.Write(" ");
			}
		}
		System.Console.ResetColor();
		System.Console.WriteLine();
	}


	#endregion




}
