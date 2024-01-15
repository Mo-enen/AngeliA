using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public partial class Game {


		// System
		public static bool IsEdittime => Instance._GetIsEdittime();
		protected abstract bool _GetIsEdittime ();

		internal static int GraphicFramerate {
			get => _GraphicFramerate.Value.Clamp(30, 120);
			set {
				_GraphicFramerate.Value = value.Clamp(30, 120);
				Instance._SetGraphicFramerate(_GraphicFramerate.Value);
			}
		}
		protected abstract void _SetGraphicFramerate (int framerate);

		internal static bool VSync {
			get => _VSync.Value;
			set {
				Instance._SetVSync(value);
				_VSync.Value = value;
			}
		}
		protected abstract void _SetVSync (bool vsync);

		internal static FullscreenMode FullscreenMode {
			get => (FullscreenMode)_FullscreenMode.Value;
			set {
				_FullscreenMode.Value = (int)value;
				Instance._SetFullscreenMode(value);
			}
		}
		protected abstract void _SetFullscreenMode (FullscreenMode mode);

		public static int ScreenWidth => Instance._GetScreenWidth();
		protected abstract int _GetScreenWidth ();

		public static int ScreenHeight => Instance._GetScreenHeight();
		protected abstract int _GetScreenHeight ();

		public static void QuitApplication () => Instance._QuitApplication();
		protected abstract void _QuitApplication ();

		protected abstract IEnumerable<KeyValuePair<int, object>> _ForAllAudioClips ();


		// Listener
		protected abstract void _AddGameQuittingListener (System.Action callback);
		protected abstract void _AddGameTryingToQuitListener (System.Func<bool> callback);
		protected abstract void _AddTextInputListener (System.Action<char> callback);


		// Debug
		public static void Log (object target) => Instance?._Log(target);
		protected abstract void _Log (object target);

		public static void LogWarning (object target) => Instance?._LogWarning(target);
		protected abstract void _LogWarning (object target);

		public static void LogError (object target) => Instance?._LogError(target);
		protected abstract void _LogError (object target);

		public static void LogException (System.Exception ex) => Instance?._LogException(ex);
		protected abstract void _LogException (System.Exception ex);

		internal static void SetDebugEnable (bool enable) => Instance?._SetDebugEnable(enable);
		protected abstract void _SetDebugEnable (bool enable);

		internal static bool GetDebugEnable () => Instance != null && Instance._GetDebugEnable();
		protected abstract bool _GetDebugEnable ();


		// Camera
		internal static FRect CameraScreenLocacion {
			get => Instance._GetCameraScreenLocacion();
			set => Instance._SetCameraScreenLocacion(value);
		}
		protected abstract FRect _GetCameraScreenLocacion ();
		protected abstract void _SetCameraScreenLocacion (FRect rect);

		public static float CameraAspect => Instance._GetCameraAspect();
		protected abstract float _GetCameraAspect ();

		public static float CameraOrthographicSize => Instance._GetCameraOrthographicSize();
		protected abstract float _GetCameraOrthographicSize ();


		// Render
		internal static void OnRenderingLayerCreated (int index, string name, int sortingOrder, int capacity) => Instance._OnRenderingLayerCreated(index, name, sortingOrder, capacity);
		protected abstract void _OnRenderingLayerCreated (int index, string name, int sortingOrder, int capacity);

		[OnGameUpdatePauseless(-4096)]
		internal static void OnCameraUpdate () => Instance._OnCameraUpdate();
		protected abstract void _OnCameraUpdate ();

		internal static void OnLayerUpdate (int layerIndex, bool isUiLayer, bool isTextLayer, Cell[] cells, int cellCount, ref int prevCellCount) => Instance._OnLayerUpdate(layerIndex, isUiLayer, isTextLayer, cells, cellCount, ref prevCellCount);
		protected abstract void _OnLayerUpdate (int layerIndex, bool isUiLayer, bool isTextLayer, Cell[] cells, int cellCount, ref int prevCellCount);

		internal static void SetSkyboxTint (Byte4 top, Byte4 bottom) => Instance._SetSkyboxTint(top, bottom);
		protected abstract void _SetSkyboxTint (Byte4 top, Byte4 bottom);

		internal static void SetTextureForRenderer (object texture) => Instance._SetTextureForRenderer(texture);
		protected abstract void _SetTextureForRenderer (object texture);


		// Effect
		internal static bool GetEffectEnable (int effectIndex) => Instance._GetEffectEnable(effectIndex);
		protected abstract bool _GetEffectEnable (int effectIndex);

		internal static void SetEffectEnable (int effectIndex, bool enable) => Instance._SetEffectEnable(effectIndex, enable);
		protected abstract void _SetEffectEnable (int effectIndex, bool enable);

		internal static void Effect_SetDarkenAmount (float amount, float step = 8) => Instance._Effect_SetDarkenAmount(amount, step);
		protected abstract void _Effect_SetDarkenAmount (float amount, float step = 8);

		internal static void Effect_SetLightenAmount (float amount, float step = 8) => Instance._Effect_SetLightenAmount(amount, step);
		protected abstract void _Effect_SetLightenAmount (float amount, float step = 8);

		internal static void Effect_SetTint (Byte4 color) => Instance._Effect_SetTint(color);
		protected abstract void _Effect_SetTint (Byte4 color);

		internal static void Effect_SetVignetteRadius (float radius) => Instance._Effect_SetVignetteRadius(radius);
		protected abstract void _Effect_SetVignetteRadius (float radius);

		internal static void Effect_SetVignetteFeather (float feather) => Instance._Effect_SetVignetteFeather(feather);
		protected abstract void _Effect_SetVignetteFeather (float feather);

		internal static void Effect_SetVignetteOffsetX (float offsetX) => Instance._Effect_SetVignetteOffsetX(offsetX);
		protected abstract void _Effect_SetVignetteOffsetX (float offsetX);

		internal static void Effect_SetVignetteOffsetY (float offsetY) => Instance._Effect_SetVignetteOffsetY(offsetY);
		protected abstract void _Effect_SetVignetteOffsetY (float offsetY);

		internal static void Effect_SetVignetteRound (float round) => Instance._Effect_SetVignetteRound(round);
		protected abstract void _Effect_SetVignetteRound (float round);


		// Texture
		public static object GetTextureFromPixels (Byte4[] pixels, int width, int height) => Instance._GetTextureFromPixels(pixels, width, height);
		protected abstract object _GetTextureFromPixels (Byte4[] pixels, int width, int height);

		public static Byte4[] GetPixelsFromTexture (object texture) => Instance._GetPixelsFromTexture(texture);
		protected abstract Byte4[] _GetPixelsFromTexture (object texture);

		public static void FillPixelsIntoTexture (Byte4[] pixels, object texture) => Instance._FillPixelsIntoTexture(pixels, texture);
		protected abstract void _FillPixelsIntoTexture (Byte4[] pixels, object texture);

		public static Int2 GetTextureSize (object texture) => Instance._GetTextureSize(texture);
		protected abstract Int2 _GetTextureSize (object texture);

		public static string GetTextureName (object texture) => Instance._GetTextureName(texture);
		protected abstract string _GetTextureName (object texture);

		public static object PngBytesToTexture (byte[] bytes) => Instance._PngBytesToTexture(bytes);
		protected abstract object _PngBytesToTexture (byte[] bytes);

		public static byte[] TextureToPngBytes (object texture) => Instance._TextureToPngBytes(texture);
		protected abstract byte[] _TextureToPngBytes (object texture);


		// GL Gizmos
		public static void DrawGizmosFrame (IRect rect, Byte4 color, int thickness) {
			Instance._DrawGizmosRect(rect.EdgeInside(Direction4.Down, thickness), color);
			Instance._DrawGizmosRect(rect.EdgeInside(Direction4.Up, thickness), color);
			Instance._DrawGizmosRect(rect.EdgeInside(Direction4.Left, thickness), color);
			Instance._DrawGizmosRect(rect.EdgeInside(Direction4.Right, thickness), color);
		}
		public static void DrawGizmosRect (IRect rect, Byte4 color) => Instance._DrawGizmosRect(rect, color);
		protected abstract void _DrawGizmosRect (IRect rect, Byte4 color);

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

		internal static CharSprite FillCharSprite (int layerIndex, char c, int textSize, CharSprite charSprite, out bool filled) => Instance._FillCharSprite(layerIndex, c, textSize, charSprite, out filled);
		protected abstract CharSprite _FillCharSprite (int layerIndex, char c, int textSize, CharSprite charSprite, out bool filled);

		internal static void RequestStringForFont (int layerIndex, int textSize, string content) => Instance._RequestStringForFont(layerIndex, textSize, content);
		protected abstract void _RequestStringForFont (int layerIndex, int textSize, string content);

		internal static void RequestStringForFont (int layerIndex, int textSize, char[] content) => Instance._RequestStringForFont(layerIndex, textSize, content);
		protected abstract void _RequestStringForFont (int layerIndex, int textSize, char[] content);

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

		public static void UnPauseMusic () => Instance._UnPauseMusic();
		protected abstract void _UnPauseMusic ();

		internal static void SetMusicVolume (int volume) {
			_MusicVolume.Value = volume;
			Instance._SetMusicVolume(volume);
		}
		protected abstract void _SetMusicVolume (int volume);

		internal static bool IsMusicPlaying => Instance._IsMusicPlaying();
		protected abstract bool _IsMusicPlaying ();


		// Sound
		internal static void PlaySound (int id, float volume) => Instance._PlaySound(id, volume);
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
}