using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class Game {


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
	public static int GetViewWidthFromViewHeight (int viewHeight) => Universe.BuiltInInfo.ViewRatio * viewHeight / 1000;


	// Render
	internal static void BeforeAllLayersUpdate () => Instance._BeforeAllLayersUpdate();
	protected abstract void _BeforeAllLayersUpdate ();

	internal static void AfterAllLayersUpdate () => Instance._AfterAllLayersUpdate();
	protected abstract void _AfterAllLayersUpdate ();

	internal static void OnLayerUpdate (int layerIndex, Cell[] cells, int cellCount) => Instance._OnLayerUpdate(layerIndex, cells, cellCount);
	protected abstract void _OnLayerUpdate (int layerIndex, Cell[] cells, int cellCount);


	// Effect
	public static Int4 ScreenEffectPadding { get; set; } = default;

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


	// Gizmos
	public static void DrawGizmosFrame (IRect rect, Color32 color, int thickness, int gap = 0) => DrawGizmosFrame(rect, color, new Int4(thickness, thickness, thickness, thickness), new Int4(gap, gap, gap, gap));
	public static void DrawGizmosFrame (IRect rect, Color32 color, Int4 thickness, Int4 gap = default) {
		// Down
		if (thickness.down > 0) {
			var edge = rect.Edge(Direction4.Down, thickness.down);
			if (gap.down == 0) {
				DrawGizmosRect(edge, color);
			} else {
				int shrink = edge.width - (edge.width - gap.down) / 2;
				DrawGizmosRect(edge.Shrink(shrink, 0, 0, 0), color);
				DrawGizmosRect(edge.Shrink(0, shrink, 0, 0), color);
			}
		}
		// Up
		if (thickness.up > 0) {
			var edge = rect.Edge(Direction4.Up, thickness.up);
			if (gap.up == 0) {
				DrawGizmosRect(edge, color);
			} else {
				int shrink = edge.width - (edge.width - gap.up) / 2;
				DrawGizmosRect(edge.Shrink(shrink, 0, 0, 0), color);
				DrawGizmosRect(edge.Shrink(0, shrink, 0, 0), color);
			}
		}
		// Left
		if (thickness.left > 0) {
			var edge = rect.Edge(Direction4.Left, thickness.left);
			if (gap.left == 0) {
				DrawGizmosRect(edge, color);
			} else {
				int shrink = edge.height - (edge.height - gap.left) / 2;
				DrawGizmosRect(edge.Shrink(0, 0, shrink, 0), color);
				DrawGizmosRect(edge.Shrink(0, 0, 0, shrink), color);
			}
		}
		// Right
		if (thickness.right > 0) {
			var edge = rect.Edge(Direction4.Right, thickness.right);
			if (gap.right == 0) {
				DrawGizmosRect(edge, color);
			} else {
				int shrink = edge.height - (edge.height - gap.right) / 2;
				DrawGizmosRect(edge.Shrink(0, 0, shrink, 0), color);
				DrawGizmosRect(edge.Shrink(0, 0, 0, shrink), color);
			}
		}
	}
	public static void DrawGizmosRect (IRect rect, Color32 color) => Instance._DrawGizmosRect(rect, color);
	protected abstract void _DrawGizmosRect (IRect rect, Color32 color);
	public static void DrawGizmosRect (IRect rect, Color32 colorT, Color32 colorB) => Instance._DrawGizmosRect(rect, colorT, colorB);
	protected abstract void _DrawGizmosRect (IRect rect, Color32 colorT, Color32 colorB);
	public static void DrawGizmosRect (IRect rect, Color32 colorTL, Color32 colorTR, Color32 colorBL, Color32 colorBR) => Instance._DrawGizmosRect(rect, colorTL, colorTR, colorBL, colorBR);
	protected abstract void _DrawGizmosRect (IRect rect, Color32 colorTL, Color32 colorTR, Color32 colorBL, Color32 colorBR);

	public static void DrawGizmosTexture (IRect rect, object texture, bool inverse = false) => Instance._DrawGizmosTexture(rect, new FRect(0f, 0f, 1f, 1f), texture, Color32.WHITE, inverse);
	public static void DrawGizmosTexture (IRect rect, FRect uv, object texture, bool inverse = false) => Instance._DrawGizmosTexture(rect, uv, texture, Color32.WHITE, inverse);
	public static void DrawGizmosTexture (IRect rect, object texture, Color32 tint, bool inverse = false) => Instance._DrawGizmosTexture(rect, new FRect(0f, 0f, 1f, 1f), texture, tint, inverse);
	public static void DrawGizmosTexture (IRect rect, FRect uv, object texture, Color32 tint, bool inverse = false) => Instance._DrawGizmosTexture(rect, uv, texture, tint, inverse);
	protected abstract void _DrawGizmosTexture (IRect rect, FRect uv, object texture, Color32 tint, bool inverse);

	public static void DrawGizmosLine (int startX, int startY, int endX, int endY, int thickness, Color32 color) => Instance._DrawGizmosLine(startX, startY, endX, endY, thickness, color);
	protected abstract void _DrawGizmosLine (int startX, int startY, int endX, int endY, int thickness, Color32 color);

	public static void IgnoreGizmos (int duration = 0) => Instance._IgnoreGizmos(duration);
	protected abstract void _IgnoreGizmos (int duration = 0);

	protected int GizmosOnTopOfUiFrame { get; private set; } = -1;
	public static void ForceGizmosOnTopOfUI (int duration = 0) => Instance.GizmosOnTopOfUiFrame = GlobalFrame + duration;
	public static void CancelGizmosOnTopOfUI () => Instance.GizmosOnTopOfUiFrame = -1;

	// Doodle
	public static Int4 DoodleScreenPadding { get; set; } = new(0, 0, 0, 0);
	protected int DoodleFrame { get; private set; } = -1;

	protected int DoodleOnTopOfUiFrame { get; private set; } = -1;
	public static void ForceDoodleOnTopOfUI (int duration = 0) => Instance.DoodleOnTopOfUiFrame = GlobalFrame + duration;
	public static void CancelDoodleOnTopOfUI () => Instance.DoodleOnTopOfUiFrame = -1;

	public static void ShowDoodle (int duration = 0) => Instance.DoodleFrame = GlobalFrame + duration;
	public static void HideDoodle () => Instance.DoodleFrame = -1;

	public static void ResetDoodle () => Instance._ResetDoodle();
	protected abstract void _ResetDoodle ();

	public static void SetDoodleOffset (Float2 screenOffset) => Instance._SetDoodleOffset(screenOffset);
	protected abstract void _SetDoodleOffset (Float2 screenOffset);

	public static void DoodleRectWrap (FRect screenRect, Color32 color) {
		int cWidth = ScreenWidth - DoodleScreenPadding.horizontal;
		int cHeight = ScreenHeight - DoodleScreenPadding.vertical;
		var canvasRect = new FRect(0, 0, cWidth, cHeight);
		screenRect.x = screenRect.x.UMod(cWidth);
		screenRect.y = screenRect.y.UMod(cHeight);
		if (screenRect.CompleteInside(canvasRect)) {
			DoodleRect(screenRect, color);
		} else {
			var _clamped = screenRect.GetClamp(canvasRect);
			if (_clamped.width.NotAlmostZero() && _clamped.height.NotAlmostZero()) {
				DoodleRect(_clamped, color);
			}

			screenRect.x -= cWidth;
			_clamped = screenRect.GetClamp(canvasRect);
			if (_clamped.width.NotAlmostZero() && _clamped.height.NotAlmostZero()) {
				DoodleRect(_clamped, color);
			}

			screenRect.y -= cHeight;
			_clamped = screenRect.GetClamp(canvasRect);
			if (_clamped.width.NotAlmostZero() && _clamped.height.NotAlmostZero()) {
				DoodleRect(_clamped, color);
			}

			screenRect.x += cWidth;
			_clamped = screenRect.GetClamp(canvasRect);
			if (_clamped.width.NotAlmostZero() && _clamped.height.NotAlmostZero()) {
				DoodleRect(_clamped, color);
			}
		}
	}
	public static void DoodleRect (FRect screenRect, Color32 color) => Instance._DoodleRect(screenRect, color);
	protected abstract void _DoodleRect (FRect screenRect, Color32 color);

	public static void DoodleWorld (IBlockSquad squad, FRect screenRect, IRect worldUnitRange, int z, bool ignoreLevel = false, bool ignoreBG = false, bool ignoreEntity = false, bool ignoreElement = true) => Instance._DoodleWorld(squad, screenRect, worldUnitRange, z, ignoreLevel, ignoreBG, ignoreEntity, ignoreElement);
	protected abstract void _DoodleWorld (IBlockSquad squad, FRect screenRect, IRect worldUnitRange, int z, bool ignoreLevel = false, bool ignoreBG = false, bool ignoreEntity = false, bool ignoreElement = true);


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


}
