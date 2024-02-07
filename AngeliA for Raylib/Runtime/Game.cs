using System.Collections;
using System.Collections.Generic;
using System.Text;
using AngeliaFramework;
using Raylib_cs;

[assembly: AngeliA]

namespace AngeliaForRaylib;

public partial class GameForRaylib : Game {




	#region --- SUB ---


	private class GLRect {
		public IRect Rect;
		public Color Color;
	}

	private class GLTexture {
		public IRect Rect;
		public Texture2D Texture;
		public FRect UV;
	}

	private class FontData {
		public Font Font;
		public string Name;
		public bool IsFullSet;
	}


	#endregion




	#region --- VAR ---


	// Const
	private Texture2D EMPTY_TEXTURE;
	private const long TICK_GAP = System.TimeSpan.TicksPerSecond / 60;

	// Event
	private event System.Action OnGameQuitting = null;
	private event System.Func<bool> OnGameTryingToQuit = null;
	private event System.Action<bool> OnWindowFocusChanged = null;
	private event System.Action<char> OnTextInput = null;

	// Data
	private readonly GLRect[] GLRects = new GLRect[256 * 256].FillWithNewValue();
	private readonly GLTexture[] GLTextures = new GLTexture[256].FillWithNewValue();
	private FontData[] Fonts;
	private int GLRectCount = 0;
	private int GLTextureCount = 0;
	private bool RequireQuitGame = false;
	private bool WindowFocused = true;
	private long NextUpdateTick = -1;
	private FRect CameraScreenRect = new(0, 0, 1f, 1f);
	private IRect ScreenRect;
	private Shader LerpShader;
	private Shader ColorShader;
	private Texture2D Texture;

	// Saving
	private readonly SavingBool WindowMaximized = new("Game.WindowMaximized", false);


	#endregion




	#region --- MSG ---


	public static void Run () {
		var game = new GameForRaylib();
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
			int cpCount = 0;
			int[] cPoints = Raylib.LoadCodepoints("12345", ref cpCount);
			fontList.Add(new FontData() {
				Font = Raylib.LoadFontEx(fontPath, 48, cPoints, cpCount),
				Name = Util.GetNameWithoutExtension(fontPath),
			});
		}
		fontList.Sort((a, b) => a.Name.CompareTo(b.Name));
		Fonts = fontList.ToArray();

		// Init Shader
		string shaderRoot = Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Shaders");
		string lerpShaderPath = Util.CombinePaths(shaderRoot, "Lerp.fs");
		if (Util.FileExists(lerpShaderPath)) LerpShader = Raylib.LoadShader(null, lerpShaderPath);
		string colorShaderPath = Util.CombinePaths(shaderRoot, "Color.fs");
		if (Util.FileExists(colorShaderPath)) ColorShader = Raylib.LoadShader(null, colorShaderPath);

		// Init Cache
		unsafe {
			fixed (void* data = new byte[4]) {
				EMPTY_TEXTURE = Raylib.LoadTextureFromImage(new Image() {
					Data = data,
					Format = PixelFormat.UncompressedR8G8B8A8,
					Height = 1,
					Width = 1,
					Mipmaps = 1,
				});
			}
		}

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

		// Begin Update
		Raylib.BeginDrawing();

		// Sky
		Raylib.DrawRectangleGradientV(
			0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(),
			Sky.SkyTintBottomColor.ToRaylib(), Sky.SkyTintTopColor.ToRaylib()
		);

		// Update AngeliA
		base.GameUpdate();
		base.GraphicUpdate();

		// Update Gizmos
		UpdateGizmos();

		// End Update
		Raylib.EndDrawing();

		// Final
		_CONTINUE_:;

		// Trying to Quit Check
		if (!RequireQuitGame && Raylib.WindowShouldClose()) {
			RequireQuitGame = OnGameTryingToQuit != null && OnGameTryingToQuit.Invoke();
		}

	}


	private void QuitGame () {
		foreach (var font in Fonts) Raylib.UnloadFont(font.Font);
		Raylib.UnloadShader(LerpShader);
		Raylib.UnloadShader(ColorShader);
		Raylib.UnloadTexture(Texture);
		Raylib.UnloadTexture(EMPTY_TEXTURE);
		WindowMaximized.Value = !Raylib.IsWindowFullscreen() && Raylib.IsWindowMaximized();
		OnGameQuitting?.Invoke();
		Raylib.CloseWindow();
	}


	#endregion




	#region --- LGC ---


	private void UpdateGizmos () {

		var cameraRect = CellRenderer.CameraRect;
		int cameraL = cameraRect.x;
		int cameraR = cameraRect.xMax;
		int cameraD = cameraRect.y;
		int cameraU = cameraRect.yMax;
		int screenL = ScreenRect.x;
		int screenR = ScreenRect.xMax;
		int screenD = ScreenRect.y;
		int screenU = ScreenRect.yMax;

		// Texture
		for (int i = 0; i < GLTextureCount; i++) {
			var glTexture = GLTextures[i];
			var rTexture = glTexture.Texture;
			var rect = glTexture.Rect;
			var uv = glTexture.UV;
			Raylib.DrawTexturePro(
				rTexture,
				new Rectangle(
					uv.x * rTexture.Width,
					uv.y * rTexture.Height,
					uv.width * rTexture.Width,
					uv.height * rTexture.Height
				).Shrink(0.1f), new Rectangle(
					Util.RemapUnclamped(cameraL, cameraR, screenL, screenR, rect.x),
					Util.RemapUnclamped(cameraD, cameraU, screenU, screenD, rect.yMax),
					rect.width * ScreenRect.width / cameraRect.width,
					rect.height * ScreenRect.height / cameraRect.height
				).Expand(0.5f), new(0, 0), 0, Color.White
			);
		}
		GLTextureCount = 0;

		// Rect
		for (int i = 0; i < GLRectCount; i++) {
			var glRect = GLRects[i];
			var rect = glRect.Rect;
			Raylib.DrawRectangle(
				Util.RemapUnclamped(cameraL, cameraR, screenL, screenR, rect.x),
				Util.RemapUnclamped(cameraD, cameraU, screenU, screenD, rect.yMax),
				rect.width * ScreenRect.width / cameraRect.width,
				rect.height * ScreenRect.height / cameraRect.height,
				glRect.Color
			);
		}
		GLRectCount = 0;
	}


	private static void WritePixelsToConsole (Byte4[] pixels, int width) {

		int height = pixels.Length / width;
		int realWidth = Util.Min(width, 32);
		int realHeight = height * realWidth / width;
		int scale = width / realWidth;

		for (int y = realHeight - 1; y >= 0; y--) {
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
