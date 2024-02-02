using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AngeliaFramework;
using Raylib_cs;


[assembly: AngeliA]


namespace AngeliaForRaylib;


public class Game : AngeliaFramework.Game {


	// Event
	private event System.Action OnGameQuitting = null;
	private event System.Func<bool> OnGameTryingToQuit = null;
	private event System.Action<bool> OnWindowFocusChanged = null;
	private event System.Action<char> OnTextInput = null;

	// Data
	private bool RequireQuitGame = false;
	private bool WindowFocused = true;
	private FRect CameraScreenRect = new(0, 0, 1f, 1f);
	private Texture2D Texture;


	// MSG
	public void Start () {

		var msgs = new List<(string msg, Color tint)>();
		foreach (var c in typeof(Character).AllChildClass()) {
			var tint = Color.RayWhite;
			msgs.Add((c.Name, tint));
		}



		////////////////////////////////////////

		Raylib.SetConfigFlags(
			ConfigFlags.ResizableWindow |
			ConfigFlags.VSyncHint |
			ConfigFlags.AlwaysRunWindow
		);
		Raylib.InitWindow(1024, 1024, "Test");
		Raylib.SetExitKey(Raylib_cs.KeyboardKey.Null);

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

				// Game
				Raylib.BeginDrawing();
				Raylib.DrawRectangleGradientV(
					0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(),
					Skybox.SkyTintBottomColor.ToRaylib(), Skybox.SkyTintTopColor.ToRaylib()
				);


				///////////////////////////
				int y = 12;
				foreach (var (msg, tint) in msgs) {
					Raylib.DrawText(msg, 12, y, 32, tint);
					y += 36;
				}
				///////////////////////////


				Raylib.EndDrawing();




				// Trying to Quit Check
				if (!RequireQuitGame && Raylib.WindowShouldClose()) {
					OnGameTryingToQuit?.Invoke();
				}

				// Real Quit Check
				if (RequireQuitGame) break;

			} catch (System.Exception ex) {
				LogException(ex);
			}
		}
		Raylib.CloseWindow();

		// Quitting
		OnGameQuitting?.Invoke();

	}


	// System
	protected override bool _GetIsEdittime () {
#if DEBUG
		return true;
#else
		return false;
#endif
	}

	protected override void _SetGraphicFramerate (int framerate) => Raylib.SetTargetFPS(framerate);

	protected override void _SetFullscreen (bool fullScreen) {
		if (Raylib.IsWindowFullscreen() == fullScreen) return;
		Raylib.ToggleFullscreen();
	}

	protected override int _GetScreenWidth () => Raylib.GetScreenWidth();

	protected override int _GetScreenHeight () => Raylib.GetScreenHeight();

	protected override void _QuitApplication () => RequireQuitGame = true;


	// Listener
	protected override void _AddGameQuittingCallback (System.Action callback) {
		OnGameQuitting -= callback;
		OnGameQuitting += callback;
	}

	protected override void _AddGameTryingToQuitCallback (System.Func<bool> callback) {
		OnGameTryingToQuit -= callback;
		OnGameTryingToQuit += callback;
	}

	protected override void _AddTextInputCallback (System.Action<char> callback) {
		OnTextInput -= callback;
		OnTextInput += callback;
	}

	protected override void _AddFocusChangedCallback (System.Action<bool> callback) {
		OnWindowFocusChanged -= callback;
		OnWindowFocusChanged += callback;
	}


	// Debug
	protected override void _Log (object target) {
		if (!IsEdittime) return;
		System.Console.ResetColor();
		System.Console.WriteLine(target.ToString());
	}

	protected override void _LogWarning (object target) {
		if (!IsEdittime) return;
		System.Console.ForegroundColor = System.ConsoleColor.Yellow;
		System.Console.WriteLine(target.ToString());
		System.Console.ResetColor();
	}

	protected override void _LogError (object target) {
		if (!IsEdittime) return;
		System.Console.ForegroundColor = System.ConsoleColor.Red;
		System.Console.WriteLine(target.ToString());
		System.Console.ResetColor();
	}

	protected override void _LogException (System.Exception ex) {
		if (!IsEdittime) return;
		System.Console.ForegroundColor = System.ConsoleColor.Red;
		System.Console.WriteLine(ex.GetType().Name);
		System.Console.WriteLine(ex.Message);
		System.Console.ResetColor();
	}


	// Camera
	protected override FRect _GetCameraScreenLocacion () => CameraScreenRect;
	protected override void _SetCameraScreenLocacion (FRect rect) => CameraScreenRect = rect;


	// Render
	protected override void _OnRenderingLayerCreated (int index, string name, int sortingOrder, int capacity) {
		// TODO
	}

	protected override void _OnCameraUpdate () { }

	protected override void _OnLayerUpdate (int layerIndex, bool isUiLayer, bool isTextLayer, Cell[] cells, int cellCount, ref int prevCellCount) {


		// TODO


	}

	protected override void _SetSkyboxTint (Byte4 top, Byte4 bottom) { }

	protected override void _SetTextureForRenderer (object texture) => Texture = (Texture2D)texture;


	// Effect
	protected override bool _GetEffectEnable (int effectIndex) {
		// TODO

		return false;
	}
	protected override void _SetEffectEnable (int effectIndex, bool enable) {
		// TODO
	}
	protected override void _Effect_SetDarkenParams (float amount, float step) {
		// TODO
	}
	protected override void _Effect_SetLightenParams (float amount, float step) {
		// TODO
	}
	protected override void _Effect_SetTintParams (Byte4 color) {
		// TODO
	}
	protected override void _Effect_SetVignetteParams (float radius, float feather, float offsetX, float offsetY, float round) {
		// TODO
	}


	// Texture
	protected override object _GetTextureFromPixels (Byte4[] pixels, int width, int height) {
		if (pixels == null || pixels.Length == 0) return null;
		unsafe {
			int len = pixels.Length;
			var colors = pixels.ToRaylib();
			fixed (Color* data = &colors[0]) {
				return Raylib.LoadTextureFromImage(new Image() {
					Data = data,
					Format = PixelFormat.UncompressedR32G32B32A32,
					Width = width,
					Height = height,
					Mipmaps = 0,
				});
			}
		}
	}

	protected override Byte4[] _GetPixelsFromTexture (object texture) {
		if (texture is not Texture2D rTexture) return System.Array.Empty<Byte4>();
		var image = Raylib.LoadImageFromTexture(rTexture);
		unsafe {
			int len = image.Width * image.Height;
			var result = new Byte4[len];
			var colors = Raylib.LoadImageColors(image);
			for (int i = 0; i < len; i++) {
				result[i] = colors[i].ToAngelia();
			}
			return result;
		}
	}

	protected override void _FillPixelsIntoTexture (Byte4[] pixels, object texture) {
		if (texture is not Texture2D rTexture) return;
		Raylib.UpdateTexture(rTexture, pixels.ToRaylib());
	}

	protected override Int2 _GetTextureSize (object texture) => texture is Texture2D rTexture ? new Int2(rTexture.Width, rTexture.Height) : default;

	protected override object _PngBytesToTexture (byte[] bytes) {
		if (bytes == null || bytes.Length == 0) return null;
		var image = Raylib.LoadImageFromMemory(".png", bytes);
		return Raylib.LoadTextureFromImage(image);
	}

	protected override byte[] _TextureToPngBytes (object texture) {
		if (texture is not Texture2D rTexture) return System.Array.Empty<byte>();
		unsafe {
			var fileType = Marshal.StringToHGlobalAnsi(".png");
			var fileSizePtr = new System.IntPtr();
			var resultPtr = new System.IntPtr(Raylib.ExportImageToMemory(
				Raylib.LoadImageFromTexture(rTexture),
				(sbyte*)fileType.ToPointer(),
				(int*)fileSizePtr.ToPointer()
			));
			var resultBytes = new byte[fileSizePtr.ToInt32()];
			Marshal.Copy(resultPtr, resultBytes, 0, resultBytes.Length);
			return resultBytes;
		}
	}


	// GL Gizmos
	protected override void _DrawGizmosRect (IRect rect, Byte4 color) {
		
		Raylib.DrawRectangle(,,,, color.ToRaylib());
	}


	protected override void _DrawGizmosTexture (IRect rect, FRect uv, object texture) {
		if (texture is not Texture2D rTexture) return;


	}



}
