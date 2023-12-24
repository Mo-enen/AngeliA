using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public partial class Game {


		// System
		public static bool IsEdittime => Instance.GetIsEdittime();
		protected abstract bool GetIsEdittime ();

		internal static int GraphicFramerate {
			get => _GraphicFramerate.Value.Clamp(30, 120);
			set {
				_GraphicFramerate.Value = value.Clamp(30, 120);
				Instance.SetGraphicFramerate(_GraphicFramerate.Value);
			}
		}
		protected abstract void SetGraphicFramerate (int framerate);

		internal static bool VSync {
			get => _VSync.Value;
			set {
				Instance.SetVSync(value);
				_VSync.Value = value;
			}
		}
		protected abstract void SetVSync (bool vsync);

		internal static FullscreenMode FullscreenMode {
			get => (FullscreenMode)_FullscreenMode.Value;
			set {
				_FullscreenMode.Value = (int)value;
				Instance.SetFullscreenMode(value);
			}
		}
		protected abstract void SetFullscreenMode (FullscreenMode mode);

		public static int ScreenWidth => Instance.GetScreenWidth();
		protected abstract int GetScreenWidth ();

		public static int ScreenHeight => Instance.GetScreenHeight();
		protected abstract int GetScreenHeight ();

		// Debug
		public static void Log (object target) => Instance?.DebugLog(target);
		protected abstract void DebugLog (object target);

		public static void LogWarning (object target) => Instance?.DebugLogWarning(target);
		protected abstract void DebugLogWarning (object target);

		public static void LogError (object target) => Instance?.DebugLogError(target);
		protected abstract void DebugLogError (object target);

		public static void LogException (System.Exception ex) => Instance?.DebugLogException(ex);
		protected abstract void DebugLogException (System.Exception ex);

		internal static void SetDebugEnable (bool enable) => Instance?.SetDebugLoggerEnable(enable);
		protected abstract void SetDebugLoggerEnable (bool enable);

		internal static bool GetDebugEnable () => Instance != null && Instance.GetDebugLoggerEnable();
		protected abstract bool GetDebugLoggerEnable ();


		// Camera
		internal static FRect CameraScreenLocacion {
			get => Instance.GetCameraScreenLocacion();
			set => Instance.SetCameraScreenLocacion(value);
		}
		protected abstract FRect GetCameraScreenLocacion ();
		protected abstract void SetCameraScreenLocacion (FRect rect);

		public static float CameraAspect => Instance.GetCameraAspect();
		protected abstract float GetCameraAspect ();

		public static float CameraOrthographicSize => Instance.GetCameraOrthographicSize();
		protected abstract float GetCameraOrthographicSize ();


		// Render
		internal static void InvokeOnRenderingLayerCreated (int index, string name, int sortingOrder, int capacity) => Instance.OnRenderingLayerCreated(index, name, sortingOrder, capacity);
		protected abstract void OnRenderingLayerCreated (int index, string name, int sortingOrder, int capacity);

		internal static void InvokeOnTextLayerCreated (int index, string name, int sortingOrder, int capacity) => Instance.OnTextLayerCreated(index, name, sortingOrder, capacity);
		protected abstract void OnTextLayerCreated (int index, string name, int sortingOrder, int capacity);

		protected abstract void OnCameraUpdate ();

		internal static void InvokeLayerUpdate (int layerIndex, bool isTextLayer, Cell[] cells, int cellCount, ref int prevCellCount) => Instance.OnLayerUpdate(layerIndex, isTextLayer, cells, cellCount, ref prevCellCount);
		protected abstract void OnLayerUpdate (int layerIndex, bool isTextLayer, Cell[] cells, int cellCount, ref int prevCellCount);

		public static SpriteSheet LoadSpriteSheet () => Instance.LoadSheetFromDisk();
		protected virtual SpriteSheet LoadSheetFromDisk () => JsonUtil.LoadOrCreateJson<SpriteSheet>(AngePath.SheetRoot);

		public static object LoadSheetTexture () => Instance.LoadSheetTextureFromDisk();
		protected abstract object LoadSheetTextureFromDisk ();

		public static int TextLayerCount => Instance.GetTextLayerCount();
		protected abstract int GetTextLayerCount ();

		internal static string GetTextLayerNameAt (int index) => Instance.GetTextLayerName(index);
		protected abstract string GetTextLayerName (int index);

		internal static int GetFontSizeAt (int index) => Instance.GetFontSize(index);
		protected abstract int GetFontSize (int index);

		internal static CellRenderer.CharSprite FillCharSprite (int layerIndex, char c, int textSize, int rebuildVersion, CellRenderer.CharSprite charSprite, out bool filled) => Instance.FillInfoToCharSprite(layerIndex, c, textSize, rebuildVersion, charSprite, out filled);
		protected abstract CellRenderer.CharSprite FillInfoToCharSprite (int layerIndex, char c, int textSize, int rebuildVersion, CellRenderer.CharSprite charSprite, out bool filled);

		internal static void RequestStringForFont (int layerIndex, int textSize, string content) => Instance.RequestStringForFontTexture(layerIndex, textSize, content);
		protected abstract void RequestStringForFontTexture (int layerIndex, int textSize, string content);

		internal static void RequestStringForFont (int layerIndex, int textSize, char[] content) => Instance.RequestStringForFontTexture(layerIndex, textSize, content);
		protected abstract void RequestStringForFontTexture (int layerIndex, int textSize, char[] content);

		public static void SetBackgroundTint (Byte4 top, Byte4 bottom) {
			ForceBackgroundTintFrame = GlobalFrame + 1;
			Instance.SetSkyboxTint(top, bottom);
		}
		protected abstract void SetSkyboxTint (Byte4 top, Byte4 bottom);



	}
}