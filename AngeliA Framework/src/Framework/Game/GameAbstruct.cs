using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework;
public abstract partial class Game {


	// VAR
	private static readonly int[] ScreenEffectEnableFrames = new int[Const.SCREEN_EFFECT_COUNT].FillWithValue(-1);


	// System
	public static bool IsEdittime => Instance._GetIsEdittime();
	protected abstract bool _GetIsEdittime ();

	internal static bool IsFullscreen {
		get => _IsFullscreen.Value;
		set {
			_IsFullscreen.Value = value;
			Instance._SetFullscreen(value);
		}
	}
	protected abstract void _SetFullscreen (bool fullScreen);

	public static int ScreenWidth { get; private set; }
	protected abstract int _GetScreenWidth ();

	public static int ScreenHeight { get; private set; }
	protected abstract int _GetScreenHeight ();

	protected abstract void _SetWindowSize (int width, int height);

	public static void QuitApplication () => Instance._QuitApplication();
	protected abstract void _QuitApplication ();


	// Listener
	protected abstract void _AddGameQuittingCallback (System.Action callback);
	protected abstract void _AddGameTryingToQuitCallback (System.Func<bool> callback);
	protected abstract void _AddTextInputCallback (System.Action<char> callback);
	protected abstract void _AddFocusChangedCallback (System.Action<bool> callback);


	// Camera
	internal static FRect CameraScreenLocacion {
		get => Instance._GetCameraScreenLocacion();
		set => Instance._SetCameraScreenLocacion(value);
	}
	protected abstract FRect _GetCameraScreenLocacion ();
	protected abstract void _SetCameraScreenLocacion (FRect rect);


	// View
	public static int DefaultViewHeight => Instance._DefaultViewHeight;
	protected virtual int _DefaultViewHeight => 26 * Const.CEL;

	public static int MinViewHeight => Instance._MinViewHeight;
	protected virtual int _MinViewHeight => 16 * Const.CEL;

	public static int MaxViewHeight => Instance._MaxViewHeight;
	protected virtual int _MaxViewHeight => 60 * Const.CEL;

	public static int WorldBehindParallax => Instance._WorldBehindParallax;
	protected virtual int _WorldBehindParallax => 1300;

	public static byte WorldBehindAlpha => Instance._WorldBehindAlpha;
	protected virtual byte _WorldBehindAlpha => 64;


	// Render
	internal static void OnRenderingLayerCreated (int index, string name, int sortingOrder, int capacity) => Instance._OnRenderingLayerCreated(index, name, sortingOrder, capacity);
	protected abstract void _OnRenderingLayerCreated (int index, string name, int sortingOrder, int capacity);

	[OnGameUpdatePauseless(-4096)]
	internal static void OnCameraUpdate () => Instance._OnCameraUpdate();
	protected abstract void _OnCameraUpdate ();

	internal static void OnLayerUpdate (int layerIndex, bool isUiLayer, bool isTextLayer, Cell[] cells, int cellCount) => Instance._OnLayerUpdate(layerIndex, isUiLayer, isTextLayer, cells, cellCount);
	protected abstract void _OnLayerUpdate (int layerIndex, bool isUiLayer, bool isTextLayer, Cell[] cells, int cellCount);

	internal static void SetSkyboxTint (Color32 top, Color32 bottom) => Instance._SetSkyboxTint(top, bottom);
	protected abstract void _SetSkyboxTint (Color32 top, Color32 bottom);


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


	// GL Gizmos
	public static void DrawGizmosFrame (IRect rect, Color32 color, int thickness) {
		Instance._DrawGizmosRect(rect.EdgeInside(Direction4.Down, thickness), color);
		Instance._DrawGizmosRect(rect.EdgeInside(Direction4.Up, thickness), color);
		Instance._DrawGizmosRect(rect.EdgeInside(Direction4.Left, thickness), color);
		Instance._DrawGizmosRect(rect.EdgeInside(Direction4.Right, thickness), color);
	}
	public static void DrawGizmosRect (IRect rect, Color32 color) => Instance._DrawGizmosRect(rect, color);
	protected abstract void _DrawGizmosRect (IRect rect, Color32 color);

	public static void DrawGizmosTexture (IRect rect, object texture) => Instance._DrawGizmosTexture(rect, new FRect(0f, 0f, 1f, 1f), texture);
	public static void DrawGizmosTexture (IRect rect, FRect uv, object texture) => Instance._DrawGizmosTexture(rect, uv, texture);
	protected abstract void _DrawGizmosTexture (IRect rect, FRect uv, object texture);


	// Text
	internal static void OnTextLayerCreated (int index, string name, int sortingOrder, int capacity) => Instance._OnTextLayerCreated(index, name, sortingOrder, capacity);
	protected abstract void _OnTextLayerCreated (int index, string name, int sortingOrder, int capacity);

	public static int TextLayerCount => Instance._GetTextLayerCount();
	protected abstract int _GetTextLayerCount ();

	internal static string GetTextLayerName (int index) => Instance._GetTextLayerName(index);
	protected abstract string _GetTextLayerName (int index);

	internal static int GetFontSize (int index) => Instance._GetFontSize(index);
	protected abstract int _GetFontSize (int index);

	internal static CharSprite GetCharSprite (int layerIndex, char c, int textSize) => Instance._GetCharSprite(layerIndex, c, textSize);
	protected abstract CharSprite _GetCharSprite (int layerIndex, char c, int textSize);

	public static string GetClipboardText () => Instance?._GetClipboardText();
	protected abstract string _GetClipboardText ();

	public static void SetClipboardText (string text) => Instance?._SetClipboardText(text);
	protected abstract void _SetClipboardText (string text);

	public static void SetImeCompositionMode (bool on) => Instance?._SetImeCompositionMode(on);
	protected abstract void _SetImeCompositionMode (bool on);


	// Music
	public static void PlayMusic (int id) => Instance._PlayMusic(id);
	protected abstract void _PlayMusic (int id);

	public static void StopMusic () => Instance._StopMusic();
	protected abstract void _StopMusic ();

	public static void PauseMusic () => Instance._PauseMusic();
	protected abstract void _PauseMusic ();

	public static void UnpauseMusic () => Instance._UnPauseMusic();
	protected abstract void _UnPauseMusic ();

	internal static void SetMusicVolume (int volume) {
		_MusicVolume.Value = volume;
		Instance._SetMusicVolume(volume);
	}
	protected abstract void _SetMusicVolume (int volume);

	internal static bool IsMusicPlaying => Instance._IsMusicPlaying();
	protected abstract bool _IsMusicPlaying ();


	// Sound
	public static void PlaySound (int id, float volume = 1f) => Instance._PlaySound(id, volume);
	protected abstract void _PlaySound (int id, float volume);

	internal static void StopAllSounds () => Instance._StopAllSounds();
	protected abstract void _StopAllSounds ();

	internal static void SetSoundVolume (int volume) {
		_SoundVolume.Value = volume;
		Instance._SetSoundVolume(volume);
	}
	protected abstract void _SetSoundVolume (int volume);


	// Cursor
	public static void ShowCursor () => Instance._ShowCursor();
	protected abstract void _ShowCursor ();

	public static void HideCursor () => Instance._HideCursor();
	protected abstract void _HideCursor ();

	public static bool CursorVisible => Instance._CursorVisible();
	protected abstract bool _CursorVisible ();

	public static void SetCursor (int index) => Instance._SetCursor(index);
	protected abstract void _SetCursor (int index);

	public static void SetCursorToNormal () => Instance._SetCursorToNormal();
	protected abstract void _SetCursorToNormal ();


	// Mouse
	internal static bool IsMouseAvailable => Instance._IsMouseAvailable();
	protected abstract bool _IsMouseAvailable ();

	internal static bool IsMouseLeftHolding => Instance._IsMouseLeftHolding();
	protected abstract bool _IsMouseLeftHolding ();

	internal static bool IsMouseMidHolding => Instance._IsMouseMidHolding();
	protected abstract bool _IsMouseMidHolding ();

	internal static bool IsMouseRightHolding => Instance._IsMouseRightHolding();
	protected abstract bool _IsMouseRightHolding ();

	internal static int MouseScrollDelta => Instance._GetMouseScrollDelta();
	protected abstract int _GetMouseScrollDelta ();

	internal static Int2 MouseScreenPosition => Instance._GetMouseScreenPosition();
	protected abstract Int2 _GetMouseScreenPosition ();


	// Keyboard
	internal static bool IsKeyboardAvailable => Instance._IsKeyboardAvailable();
	protected abstract bool _IsKeyboardAvailable ();

	internal static bool IsKeyboardKeyHolding (KeyboardKey key) => Instance._IsKeyboardKeyHolding(key);
	protected abstract bool _IsKeyboardKeyHolding (KeyboardKey key);


	// Gamepad
	internal static bool IsGamepadAvailable => Instance._IsGamepadAvailable();
	protected abstract bool _IsGamepadAvailable ();

	internal static bool IsGamepadKeyHolding (GamepadKey key) => Instance._IsGamepadKeyHolding(key);
	protected abstract bool _IsGamepadKeyHolding (GamepadKey key);

	internal static bool IsGamepadLeftStickHolding (Direction4 direction) => Instance._IsGamepadLeftStickHolding(direction);
	protected abstract bool _IsGamepadLeftStickHolding (Direction4 direction);

	internal static bool IsGamepadRightStickHolding (Direction4 direction) => Instance._IsGamepadRightStickHolding(direction);
	protected abstract bool _IsGamepadRightStickHolding (Direction4 direction);

	internal static Float2 GamepadLeftStickDirection => Instance._GetGamepadLeftStickDirection();
	protected abstract Float2 _GetGamepadLeftStickDirection ();

	internal static Float2 GamepadRightStickDirection => Instance._GetGamepadRightStickDirection();
	protected abstract Float2 _GetGamepadRightStickDirection ();


}