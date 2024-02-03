using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using Raylib_cs;

[assembly: AngeliA]

namespace AngeliaForRaylib;

public partial class Game : AngeliaFramework.Game {

	// SUB
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

	// Api
	public static Game InstanceRaylib => Instance as Game;

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

	// API
	public void Start () {

		var msgs = new List<(string msg, Color tint)>();
		foreach (var c in typeof(Character).AllChildClass()) {
			var tint = Color.RayWhite;
			msgs.Add((c.Name, tint));
		}



		////////////////////////////////////////

		// Init Window
		Raylib.SetConfigFlags(
			ConfigFlags.ResizableWindow |
			ConfigFlags.VSyncHint |
			ConfigFlags.AlwaysRunWindow
		);
		Raylib.InitWindow(1024, 1024, "Test");
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

		// init AngeliA
		Initialize();

		while (true) {
			try {

				// Window Focus
				bool windowFocus = Raylib.IsWindowFocused();
				if (windowFocus != WindowFocused) {
					WindowFocused = windowFocus;
					OnWindowFocusChanged?.Invoke(windowFocus);
				}

				// Text Input
				int currentChar;
				while ((currentChar = Raylib.GetCharPressed()) > 0) {
					OnTextInput?.Invoke((char)currentChar);
				}

				// Update Game
				Raylib.BeginDrawing();
				Raylib.DrawRectangleGradientV(
					0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(),
					Sky.SkyTintBottomColor.ToRaylib(), Sky.SkyTintTopColor.ToRaylib()
				);


				///////////////////////////
				int y = 12;
				foreach (var (msg, tint) in msgs) {
					Raylib.DrawText(msg, 12, y, 32, tint);
					y += 36;
				}
				///////////////////////////


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

				// Trying to Quit Check
				if (!RequireQuitGame && Raylib.WindowShouldClose()) {
					OnGameTryingToQuit?.Invoke();
				}

				// Real Quit Check
				if (RequireQuitGame) break;

				// Wait for Fixed Update




			} catch (System.Exception ex) {
				LogException(ex);
			}
		}

		// Quitting
		OnGameQuitting?.Invoke();

		// Close
		Raylib.CloseWindow();

	}

	// Util
	private static int Angelia_to_Raylib_X (int globalX) {
		var cameraRect = CellRenderer.CameraRect;
		var screenRect = InstanceRaylib.ScreenRect;
		return Util.RemapUnclamped(
			cameraRect.x, cameraRect.xMax,
			screenRect.x, screenRect.xMax,
			globalX
		);
	}

	private static int Angelia_to_Raylib_Y (int globalY) {
		var cameraRect = CellRenderer.CameraRect;
		var screenRect = InstanceRaylib.ScreenRect;
		return Util.RemapUnclamped(
			cameraRect.y, cameraRect.yMax,
			screenRect.yMax, screenRect.y,
			globalY
		);
	}

	private static IRect Angelia_to_Raylib_Rect (IRect globalRect) => IRect.MinMaxRect(
		Angelia_to_Raylib_X(globalRect.x),
		Angelia_to_Raylib_Y(globalRect.yMax),
		Angelia_to_Raylib_X(globalRect.xMax),
		Angelia_to_Raylib_Y(globalRect.y)
	);

}
