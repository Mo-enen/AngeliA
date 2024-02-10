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

	private class FontData {

		private readonly Dictionary<char, (Image image, Texture2D texture)> Pool = new();
		private unsafe byte* PrioritizedData = null;
		private unsafe byte* FullsetData = null;
		private int PrioritizedByteSize = 0;
		private int FullsetByteSize = 0;

		public string Name;
		public int LayerIndex;
		public int Size;
		public float Scale = 1f;

		public unsafe void LoadData (string filePath, bool isPrioritized) {
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

		public unsafe bool TryGetCharData (char c, out GlyphInfo info, out Texture2D texture) {

			info = default;
			texture = default;

			if (PrioritizedByteSize == 0 && FullsetByteSize == 0) return false;

			bool usingFullset = FullsetByteSize != 0 && (int)c >= 256;
			var data = usingFullset ? FullsetData : PrioritizedData;
			int dataSize = usingFullset ? FullsetByteSize : PrioritizedByteSize;
			int charInt = c;
			var infoPtr = Raylib.LoadFontData(data, dataSize, Size, &charInt, 1, FontType.Default);
			if (infoPtr == null) return false;

			info = infoPtr[0];
			var img = info.Image;
			if (img.Width * img.Height != 0) {
				texture = Raylib.LoadTextureFromImage(img);
				Pool.TryAdd(c, (img, texture));
				return true;
			}
			return false;
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
	private readonly Shader[] ScreenEffectShaders = new Shader[Const.SCREEN_EFFECT_COUNT];
	private readonly bool[] ScreenEffectEnables = new bool[Const.SCREEN_EFFECT_COUNT].FillWithValue(false);
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
	private RenderTexture2D RenderTexture;
	private int ShaderPropIndex_DarkenAmount;
	private int ShaderPropIndex_LightenAmount;
	private int ShaderPropIndex_TintAmount;
	private int ShaderPropIndex_VignetteRadius;
	private int ShaderPropIndex_VignetteFeather;
	private int ShaderPropIndex_VignetteOffsetX;
	private int ShaderPropIndex_VignetteOffsetY;
	private int ShaderPropIndex_VignetteRound;
	private int ShaderPropIndex_VignetteAspect;
	private int ShaderPropIndex_CA_RED_X;
	private int ShaderPropIndex_CA_RED_Y;
	private int ShaderPropIndex_CA_GREEN_X;
	private int ShaderPropIndex_CA_GREEN_Y;
	private int ShaderPropIndex_CA_BLUE_X;
	private int ShaderPropIndex_CA_BLUE_Y;

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

		// Pipeline
		InitializeFont();
		InitializeShader();
		InitializeAudio();
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
		if (CellGUI.IsTyping) {
			int current;
			for (int safe = 0; (current = Raylib.GetCharPressed()) > 0 && safe < 1024; safe++) {
				OnTextInput?.Invoke((char)current);
			}
			for (int safe = 0; (current = Raylib.GetKeyPressed()) > 0 && safe < 1024; safe++) {
				switch ((Raylib_cs.KeyboardKey)current) {
					case Raylib_cs.KeyboardKey.Backspace:
						OnTextInput?.Invoke(Const.BACKSPACE_SIGN);
						break;
					case Raylib_cs.KeyboardKey.Enter:
						OnTextInput?.Invoke(Const.RETURN_SIGN);
						break;
					case Raylib_cs.KeyboardKey.C:
						if (Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftControl)) {
							OnTextInput?.Invoke(Const.CONTROL_COPY);
						}
						break;
					case Raylib_cs.KeyboardKey.X:
						if (Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftControl)) {
							OnTextInput?.Invoke(Const.CONTROL_CUT);
						}
						break;
					case Raylib_cs.KeyboardKey.V:
						if (Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftControl)) {
							OnTextInput?.Invoke(Const.CONTROL_PASTE);
						}
						break;
				}
			}
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

		// Music
		Raylib.UpdateMusicStream(CurrentBGM);

		// Prepare for Screen Effect
		bool hasScreenEffectEnabled = false;
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			if (ScreenEffectEnables[i]) {
				hasScreenEffectEnabled = true;
				int screenW = Raylib.GetScreenWidth();
				int screenH = Raylib.GetScreenHeight();
				if (RenderTexture.Texture.Width != screenW || RenderTexture.Texture.Height != screenH) {
					RenderTexture = Raylib.LoadRenderTexture(screenW, screenH);
					Raylib.SetTextureWrap(RenderTexture.Texture, TextureWrap.Clamp);
				}
				break;
			}
		}

		// Begin Draw
		if (hasScreenEffectEnabled) {
			Raylib.BeginTextureMode(RenderTexture);
		} else {
			Raylib.BeginDrawing();
		}

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
		if (CameraScreenRect.x.NotAlmostZero()) {
			int borderWidth = (int)(ScreenWidth * CameraScreenRect.x);
			Raylib.DrawRectangle(0, 0, borderWidth, ScreenRect.height, Color.Black);
			Raylib.DrawRectangle(ScreenWidth - borderWidth, 0, borderWidth, ScreenRect.height, Color.Black);
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
		Raylib.UnloadTexture(Texture);
		Raylib.UnloadTexture(EMPTY_TEXTURE);
		Raylib.UnloadRenderTexture(RenderTexture);

		// Quit Game
		WindowMaximized.Value = !Raylib.IsWindowFullscreen() && Raylib.IsWindowMaximized();
		OnGameQuitting?.Invoke();
		Raylib.CloseWindow();
	}


	#endregion




	#region --- LGC ---


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
