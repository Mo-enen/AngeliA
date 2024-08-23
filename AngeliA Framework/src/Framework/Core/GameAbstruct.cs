using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract partial class Game {


	// System
	internal static bool IsFullscreen {
		get => _IsFullscreen.Value;
		set {
			_IsFullscreen.Value = value;
			Instance._SetFullscreen(value);
		}
	}
	protected abstract void _SetFullscreen (bool fullScreen);

	public static int ScreenWidth { get; private set; }
	public static int MonitorWidth { get; private set; }
	protected abstract int _GetScreenWidth ();

	public static int ScreenHeight { get; private set; }
	public static int MonitorHeight { get; private set; }
	protected abstract int _GetScreenHeight ();

	public static void QuitApplication () => Instance._QuitApplication();
	protected abstract void _QuitApplication ();

	public static void OpenUrl (string url) => Instance._OpenUrl(url);
	protected abstract void _OpenUrl (string url);


	// Window
	public static bool IsWindowFocused => Instance._IsWindowFocused();
	protected abstract bool _IsWindowFocused ();

	public static void MakeWindowFocused () => Instance._MakeWindowFocused();
	protected abstract void _MakeWindowFocused ();

	public static void SetWindowPosition (int x, int y) => Instance._SetWindowPosition(x, y);
	protected abstract void _SetWindowPosition (int x, int y);

	public static Int2 GetWindowPosition () => Instance._GetWindowPosition();
	protected abstract Int2 _GetWindowPosition ();

	public static void SetWindowSize (int x, int y) => Instance._SetWindowSize(x, y);
	protected abstract void _SetWindowSize (int x, int y);

	public static int CurrentMonitor => Instance._GetCurrentMonitor();
	protected abstract int _GetCurrentMonitor ();

	public static int GetMonitorWidth (int monitor) => Instance._GetMonitorWidth(monitor);
	protected abstract int _GetMonitorWidth (int monitor);

	public static int GetMonitorHeight (int monitor) => Instance._GetMonitorHeight(monitor);
	protected abstract int _GetMonitorHeight (int monitor);

	public static bool IsWindowDecorated {
		get => Instance._GetWindowDecorated();
		set => Instance._SetWindowDecorated(value);
	}
	protected abstract bool _GetWindowDecorated ();
	protected abstract void _SetWindowDecorated (bool decorated);

	public static bool IsWindowTopmost {
		get => Instance._GetWindowTopmost();
		set => Instance._SetWindowTopmost(value);
	}
	protected abstract bool _GetWindowTopmost ();
	protected abstract void _SetWindowTopmost (bool topmost);

	public static bool IsWindowResizable {
		get => Instance._GetWindowResizable();
		set => Instance._SetWindowResizable(value);
	}
	protected abstract bool _GetWindowResizable ();
	protected abstract void _SetWindowResizable (bool resizable);

	public static bool IsWindowMaximized {
		get => Instance._GetWindowMaximized();
		set => Instance._SetWindowMaximized(value);
	}
	protected abstract bool _GetWindowMaximized ();
	protected abstract void _SetWindowMaximized (bool maximized);

	public static bool IsWindowMinimized {
		get => Instance._GetWindowMinimized();
		set => Instance._SetWindowMinimized(value);
	}
	protected abstract bool _GetWindowMinimized ();
	protected abstract void _SetWindowMinimized (bool minimized);

	public static void SetWindowTitle (string title) => Instance._SetWindowTitle(title);
	protected abstract void _SetWindowTitle (string title);

	public static void SetWindowMinSize (int size) => Instance._SetWindowMinSize(size);
	protected abstract void _SetWindowMinSize (int size);

	public static void SetEventWaiting (bool enable) => Instance._SetEventWaiting(enable);
	protected abstract void _SetEventWaiting (bool enable);


	// View
	public static IRect GetCameraRectFromViewRect (IRect viewRect) {
		float ratio = (float)ScreenWidth / ScreenHeight;
		var cRect = new IRect(
			viewRect.x,
			viewRect.y,
			(int)(viewRect.height * ratio),
			viewRect.height
		);
		int cOffsetX = (viewRect.width - cRect.width) / 2;
		cRect.x += cOffsetX;
		return cRect;
	}
	public static int GetViewWidthFromViewHeight (int viewHeight) => Const.VIEW_RATIO * viewHeight / 1000;

	public static int WorldBehindParallax => Instance._WorldBehindParallax;
	protected virtual int _WorldBehindParallax => 1300;

	public static byte WorldBehindAlpha => Instance._WorldBehindAlpha;
	protected virtual byte _WorldBehindAlpha => 64;


	// Render
	internal static void BeforeAllLayersUpdate () => Instance._BeforeAllLayersUpdate();
	protected abstract void _BeforeAllLayersUpdate ();

	internal static void AfterAllLayersUpdate () => Instance._AfterAllLayersUpdate();
	protected abstract void _AfterAllLayersUpdate ();

	internal static void OnLayerUpdate (int layerIndex, bool isUiLayer, Cell[] cells, int cellCount) => Instance._OnLayerUpdate(layerIndex, isUiLayer, cells, cellCount);
	protected abstract void _OnLayerUpdate (int layerIndex, bool isUiLayer, Cell[] cells, int cellCount);


	// Effect
	[OnGameUpdatePauseless(4096)]
	internal static void ScreenEffectUpdate () {
		var ins = Instance;
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			int frame = ScreenEffectEnableFrames[i];
			bool enable = PauselessFrame <= frame;
			if (enable != ins._GetEffectEnable(i)) {
				ins._SetEffectEnable(i, enable);
			}
		}
	}
	public static void PassEffect (int effectIndex, int duration = 0) => ScreenEffectEnableFrames[effectIndex] = PauselessFrame + duration;
	public static void PassEffect_ChromaticAberration (int duration = 0) {
		ScreenEffectEnableFrames[Const.SCREEN_EFFECT_CHROMATIC_ABERRATION] = PauselessFrame + duration;
	}
	public static void PassEffect_Tint (Color32 color, int duration = 0) {
		ScreenEffectEnableFrames[Const.SCREEN_EFFECT_TINT] = PauselessFrame + duration;
		Instance._Effect_SetTintParams(color);
	}
	public static void PassEffect_RetroDarken (float amount, float step = 8, int duration = 0) {
		ScreenEffectEnableFrames[Const.SCREEN_EFFECT_RETRO_DARKEN] = PauselessFrame + duration;
		Instance._Effect_SetDarkenParams(amount, step);
	}
	public static void PassEffect_RetroLighten (float amount, float step = 8, int duration = 0) {
		ScreenEffectEnableFrames[Const.SCREEN_EFFECT_RETRO_LIGHTEN] = PauselessFrame + duration;
		Instance._Effect_SetLightenParams(amount, step);
	}
	public static void PassEffect_Vignette (float radius, float feather, float offsetX, float offsetY, float round, int duration = 0) {
		ScreenEffectEnableFrames[Const.SCREEN_EFFECT_VIGNETTE] = PauselessFrame + duration;
		Instance._Effect_SetVignetteParams(radius, feather, offsetX, offsetY, round);
	}
	public static void PassEffect_Greyscale (int duration = 0) {
		ScreenEffectEnableFrames[Const.SCREEN_EFFECT_GREYSCALE] = PauselessFrame + duration;
	}
	public static void PassEffect_Invert (int duration = 0) {
		ScreenEffectEnableFrames[Const.SCREEN_EFFECT_INVERT] = PauselessFrame + duration;
	}
	public static bool GetEffectEnable (int effectIndex) => Instance._GetEffectEnable(effectIndex);
	public static void SetEffectEnable (int effectIndex, bool enable) => Instance._SetEffectEnable(effectIndex, enable);
	protected abstract bool _GetEffectEnable (int effectIndex);
	protected abstract void _SetEffectEnable (int effectIndex, bool enable);
	protected abstract void _Effect_SetDarkenParams (float amount, float step);
	protected abstract void _Effect_SetLightenParams (float amount, float step);
	protected abstract void _Effect_SetTintParams (Color32 color);
	protected abstract void _Effect_SetVignetteParams (float radius, float feather, float offsetX, float offsetY, float round);


	// Texture
	public static object GetTextureFromPixels (Color32[] pixels, int width, int height) => Instance._GetTextureFromPixels(pixels, width, height);
	protected abstract object _GetTextureFromPixels (Color32[] pixels, int width, int height);

	public static Color32[] GetPixelsFromTexture (object texture) => Instance._GetPixelsFromTexture(texture);
	protected abstract Color32[] _GetPixelsFromTexture (object texture);

	public static void FillPixelsIntoTexture (Color32[] pixels, object texture) => Instance._FillPixelsIntoTexture(pixels, texture);
	protected abstract void _FillPixelsIntoTexture (Color32[] pixels, object texture);

	public static Int2 GetTextureSize (object texture) => Instance._GetTextureSize(texture);
	protected abstract Int2 _GetTextureSize (object texture);

	public static object PngBytesToTexture (byte[] bytes) => Instance._PngBytesToTexture(bytes);
	protected abstract object _PngBytesToTexture (byte[] bytes);

	public static byte[] TextureToPngBytes (object texture) => Instance._TextureToPngBytes(texture);
	protected abstract byte[] _TextureToPngBytes (object texture);

	public static void UnloadTexture (object texture) => Instance._UnloadTexture(texture);
	protected abstract void _UnloadTexture (object texture);

	public static uint? GetTextureID (object texture) => Instance._GetTextureID(texture);
	protected abstract uint? _GetTextureID (object texture);

	public static bool IsTextureReady (object texture) => Instance._IsTextureReady(texture);
	protected abstract bool _IsTextureReady (object texture);

	public static object GetResizedTexture (object texture, int newWidth, int newHeight) => Instance._GetResizedTexture(texture, newWidth, newHeight);
	protected abstract object _GetResizedTexture (object texture, int newWidth, int newHeight);


	// GL Gizmos
	public static void DrawGizmosFrame (IRect rect, Color32 color, int thickness, int gap = 0) => DrawGizmosFrame(rect, color, new Int4(thickness, thickness, thickness, thickness), new Int4(gap, gap, gap, gap));
	public static void DrawGizmosFrame (IRect rect, Color32 color, Int4 thickness, Int4 gap = default) {
		// Down
		if (thickness.down > 0) {
			var edge = rect.Edge(Direction4.Down, thickness.down);
			if (gap.down == 0) {
				Instance._DrawGizmosRect(edge, color);
			} else {
				int shrink = edge.width - (edge.width - gap.down) / 2;
				Instance._DrawGizmosRect(edge.Shrink(shrink, 0, 0, 0), color);
				Instance._DrawGizmosRect(edge.Shrink(0, shrink, 0, 0), color);
			}
		}
		// Up
		if (thickness.up > 0) {
			var edge = rect.Edge(Direction4.Up, thickness.up);
			if (gap.up == 0) {
				Instance._DrawGizmosRect(edge, color);
			} else {
				int shrink = edge.width - (edge.width - gap.up) / 2;
				Instance._DrawGizmosRect(edge.Shrink(shrink, 0, 0, 0), color);
				Instance._DrawGizmosRect(edge.Shrink(0, shrink, 0, 0), color);
			}
		}
		// Left
		if (thickness.left > 0) {
			var edge = rect.Edge(Direction4.Left, thickness.left);
			if (gap.left == 0) {
				Instance._DrawGizmosRect(edge, color);
			} else {
				int shrink = edge.height - (edge.height - gap.left) / 2;
				Instance._DrawGizmosRect(edge.Shrink(0, 0, shrink, 0), color);
				Instance._DrawGizmosRect(edge.Shrink(0, 0, 0, shrink), color);
			}
		}
		// Right
		if (thickness.right > 0) {
			var edge = rect.Edge(Direction4.Right, thickness.right);
			if (gap.right == 0) {
				Instance._DrawGizmosRect(edge, color);
			} else {
				int shrink = edge.height - (edge.height - gap.right) / 2;
				Instance._DrawGizmosRect(edge.Shrink(0, 0, shrink, 0), color);
				Instance._DrawGizmosRect(edge.Shrink(0, 0, 0, shrink), color);
			}
		}
	}
	public static void DrawGizmosRect (IRect rect, Color32 color) => Instance._DrawGizmosRect(rect, color);
	protected abstract void _DrawGizmosRect (IRect rect, Color32 color);

	public static void DrawGizmosTexture (IRect rect, object texture, bool inverse = false) => Instance._DrawGizmosTexture(rect, new FRect(0f, 0f, 1f, 1f), texture, inverse);
	public static void DrawGizmosTexture (IRect rect, FRect uv, object texture, bool inverse = false) => Instance._DrawGizmosTexture(rect, uv, texture, inverse);
	protected abstract void _DrawGizmosTexture (IRect rect, FRect uv, object texture, bool inverse);

	public static void IgnoreGizmos (int duration = 0) => Instance._IgnoreGizmos(duration);
	protected abstract void _IgnoreGizmos (int duration = 0);


	// Text
	public static int BuiltInFontCount { get; private set; } = 0;
	public static int FontCount => Instance._GetFontCount();
	protected abstract int _GetFontCount ();

	public static string GetClipboardText () => Instance._GetClipboardText();
	protected abstract string _GetClipboardText ();

	public static void SetClipboardText (string text) => Instance._SetClipboardText(text);
	protected abstract void _SetClipboardText (string text);

	public static bool GetCharSprite (int fontIndex, char c, out CharSprite result) => Instance._GetCharSprite(fontIndex, c, out result);
	protected abstract bool _GetCharSprite (int fontIndex, char c, out CharSprite result);

	protected abstract FontData CreateNewFontData ();


	// Music
	public static void UnloadMusic (object music) => Instance._UnloadMusic(music);
	protected abstract void _UnloadMusic (object music);

	public static void PlayMusic (int id) => Instance._PlayMusic(id);
	protected abstract void _PlayMusic (int id);

	public static void StopMusic () => Instance._StopMusic();
	protected abstract void _StopMusic ();

	public static void PauseMusic () => Instance._PauseMusic();
	protected abstract void _PauseMusic ();

	public static void UnpauseMusic () => Instance._UnPauseMusic();
	protected abstract void _UnPauseMusic ();

	public static void SetMusicVolume (int volume) {
		_MusicVolume.Value = volume;
		Instance._SetMusicVolume(volume);
	}
	protected abstract void _SetMusicVolume (int volume);

	public static bool IsMusicPlaying => Instance._IsMusicPlaying();
	protected abstract bool _IsMusicPlaying ();

	public static int CurrentMusicID => Instance._GetCurrentMusicID();
	protected abstract int _GetCurrentMusicID ();


	// Sound
	public static object LoadSound (string filePath) => Instance._LoadSound(filePath);
	protected abstract object _LoadSound (string filePath);

	public static void UnloadSound (SoundData sound) => Instance._UnloadSound(sound);
	protected abstract void _UnloadSound (SoundData sound);

	public static void PlaySound (int id, float volume = 1f) => Instance._PlaySound(id, volume);
	protected abstract void _PlaySound (int id, float volume);

	public static void StopAllSounds () => Instance._StopAllSounds();
	protected abstract void _StopAllSounds ();

	public static void SetSoundVolume (int volume) {
		_SoundVolume.Value = volume;
		Instance._SetSoundVolume(volume);
	}
	protected abstract void _SetSoundVolume (int volume);


	// Cursor
	public static void ShowCursor () => Instance._ShowCursor();
	protected abstract void _ShowCursor ();

	public static void HideCursor () => Instance._HideCursor();
	protected abstract void _HideCursor ();

	public static void CenterCursor () => Instance._CenterCursor();
	protected abstract void _CenterCursor ();

	public static bool CursorVisible => Instance._CursorVisible();
	protected abstract bool _CursorVisible ();

	public static void SetCursor (int index) => Instance._SetCursor(index);
	protected abstract void _SetCursor (int index);

	public static void SetCursorToNormal () => Instance._SetCursorToNormal();
	protected abstract void _SetCursorToNormal ();

	public static bool CursorInScreen => Instance._CursorInScreen();
	protected abstract bool _CursorInScreen ();


	// Mouse
	public static bool IsMouseAvailable => Instance._IsMouseAvailable();
	protected abstract bool _IsMouseAvailable ();

	public static bool IsMouseLeftHolding => Instance._IsMouseLeftHolding();
	protected abstract bool _IsMouseLeftHolding ();

	public static bool IsMouseMidHolding => Instance._IsMouseMidHolding();
	protected abstract bool _IsMouseMidHolding ();

	public static bool IsMouseRightHolding => Instance._IsMouseRightHolding();
	protected abstract bool _IsMouseRightHolding ();

	public static int MouseScrollDelta => Instance._GetMouseScrollDelta();
	protected abstract int _GetMouseScrollDelta ();

	public static Int2 MouseScreenPosition => Instance._GetMouseScreenPosition();
	protected abstract Int2 _GetMouseScreenPosition ();


	// Keyboard
	public static bool IsKeyboardAvailable => Instance._IsKeyboardAvailable();
	protected abstract bool _IsKeyboardAvailable ();

	public static bool IsKeyboardKeyHolding (KeyboardKey key) => Instance._IsKeyboardKeyHolding(key);
	protected abstract bool _IsKeyboardKeyHolding (KeyboardKey key);

	public static IEnumerable<char> ForAllPressingCharsThisFrame () {
		for (int i = 0; i < Instance.PressingCharCount; i++) {
			yield return Instance.PressingCharsForCurrentFrame[i];
		}
	}
	protected abstract char GetCharPressed ();

	public static IEnumerable<KeyboardKey> ForAllPressingKeysThisFrame () {
		for (int i = 0; i < Instance.PressingKeyCount; i++) {
			yield return Instance.PressingKeysForCurrentFrame[i];
		}
	}
	protected abstract KeyboardKey? GetKeyPressed ();


	// Gamepad
	public static bool IsGamepadAvailable => Instance._IsGamepadAvailable();
	protected abstract bool _IsGamepadAvailable ();

	public static bool IsGamepadKeyHolding (GamepadKey key) => Instance._IsGamepadKeyHolding(key);
	protected abstract bool _IsGamepadKeyHolding (GamepadKey key);

	public static bool IsGamepadLeftStickHolding (Direction4 direction) => Instance._IsGamepadLeftStickHolding(direction);
	protected abstract bool _IsGamepadLeftStickHolding (Direction4 direction);

	public static bool IsGamepadRightStickHolding (Direction4 direction) => Instance._IsGamepadRightStickHolding(direction);
	protected abstract bool _IsGamepadRightStickHolding (Direction4 direction);

	public static Float2 GamepadLeftStickDirection => Instance._GetGamepadLeftStickDirection();
	protected abstract Float2 _GetGamepadLeftStickDirection ();

	public static Float2 GamepadRightStickDirection => Instance._GetGamepadRightStickDirection();
	protected abstract Float2 _GetGamepadRightStickDirection ();


}