using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

	private unsafe class FontData {

		private readonly int[] CACHE_CHAR_ARR = { 0 };
		private readonly Dictionary<char, (Image image, Texture2D texture)> Pool = new();
		private byte* PrioritizedData = null;
		private byte* FullsetData = null;
		private int PrioritizedByteSize = 0;
		private int FullsetByteSize = 0;

		public string Name;
		public int LayerIndex;
		public int Size;
		public float Scale = 1f;

		public void LoadData (string filePath, bool isPrioritized) {
			uint fileSize = 0;
			byte* fileData = Raylib.LoadFileData(filePath, ref fileSize);
			if (isPrioritized) {
				PrioritizedData = fileData;
				PrioritizedByteSize = (int)fileSize;
			} else {
				FullsetData = fileData;
				FullsetByteSize = (int)fileSize;
			}
		}

		public bool TryGetCharData (char c, out GlyphInfo info, out Texture2D texture) {
			info = default;
			texture = default;
			if (PrioritizedByteSize == 0 && FullsetByteSize == 0) return false;
			bool usingFullset = FullsetByteSize != 0 && (int)c >= 256;
			var data = usingFullset ? FullsetData : PrioritizedData;
			int dataSize = usingFullset ? FullsetByteSize : PrioritizedByteSize;
			CACHE_CHAR_ARR[0] = c;
			fixed (int* fontChar = CACHE_CHAR_ARR) {
				var infoPtr = Raylib.LoadFontData(data, dataSize, Size, fontChar, 1, FontType.Default);
				if (infoPtr == null) {
					return false;
				} else {
					info = infoPtr[0];
					var img = info.Image;
					if (img.Width * img.Height == 0) return false;
					texture = Raylib.LoadTextureFromImage(img);
					Marshal.FreeHGlobal((System.IntPtr)infoPtr);
					Pool.TryAdd(c, (img, texture));
					return true;
				}
			}
		}

		public void Unload () {
			foreach (var (_, (image, texture)) in Pool) {
				Raylib.UnloadImage(image);
				Raylib.UnloadTexture(texture);
			}
		}

		public bool TryGetTexture (char c, out Texture2D texture) {
			if (Pool.TryGetValue(c, out var result)) {
				texture = result.texture;
				return true;
			} else {
				texture = default;
				return false;
			}
		}

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
	private Shader TextShader;
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
			string name = Util.GetNameWithoutExtension(fontPath);
			if (!Util.TryGetIntFromString(name, 0, out int layerIndex, out _)) continue;
			var targetData = fontList.Find(data => data.LayerIndex == layerIndex);
			if (targetData == null) {
				fontList.Add(targetData = new FontData() {
					Name = name,
					LayerIndex = layerIndex,
					Size = 42,
					Scale = 1f,
				});
			}
			// Size
			int sizeTagIndex = name.IndexOf("#size=", System.StringComparison.OrdinalIgnoreCase);
			if (sizeTagIndex >= 0 && Util.TryGetIntFromString(name, sizeTagIndex + 6, out int size, out _)) {
				targetData.Size = Util.Max(42, size);
			}
			// Scale
			int scaleTagIndex = name.IndexOf("#scale=", System.StringComparison.OrdinalIgnoreCase);
			if (scaleTagIndex >= 0 && Util.TryGetIntFromString(name, scaleTagIndex + 7, out int scale, out _)) {
				targetData.Scale = (scale / 100f).Clamp(0.01f, 10f);
			}
			// Data
			targetData.LoadData(
				fontPath, !name.Contains("#fullset", System.StringComparison.OrdinalIgnoreCase)
			);
		}
		fontList.Sort((a, b) => a.LayerIndex.CompareTo(b.LayerIndex));
		Fonts = fontList.ToArray();

		// Init Shader
		string shaderRoot = Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Shaders");
		string lerpShaderPath = Util.CombinePaths(shaderRoot, "Lerp.fs");
		if (Util.FileExists(lerpShaderPath)) LerpShader = Raylib.LoadShader(null, lerpShaderPath);
		string colorShaderPath = Util.CombinePaths(shaderRoot, "Color.fs");
		if (Util.FileExists(colorShaderPath)) ColorShader = Raylib.LoadShader(null, colorShaderPath);
		string textShaderPath = Util.CombinePaths(shaderRoot, "Text.fs");
		if (Util.FileExists(textShaderPath)) TextShader = Raylib.LoadShader(null, textShaderPath);

		// Init Cache
		EMPTY_TEXTURE = (Texture2D)GetTextureFromPixels(new Byte4[1] { Const.CLEAR }, 1, 1);

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
		foreach (var font in Fonts) font.Unload();
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
