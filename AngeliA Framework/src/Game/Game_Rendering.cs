using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class Game {


	// View
	/// <summary>
	/// Calculate the camera rect based on given view rect
	/// </summary>
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


	/// <summary>
	/// Calculate view rect width based on given view rect height
	/// </summary>
	public static int GetViewWidthFromViewHeight (int viewHeight) => Universe.BuiltInInfo.ViewRatio * viewHeight / 1000;


	// Render
	/// <inheritdoc cref="_BeforeAllLayersUpdate"/>
	internal static void BeforeAllLayersUpdate () => Instance._BeforeAllLayersUpdate();
	/// <summary>
	/// This function is called before any rendering layer get updated
	/// </summary>
	protected abstract void _BeforeAllLayersUpdate ();


	/// <inheritdoc cref="_AfterAllLayersUpdate"/>
	internal static void AfterAllLayersUpdate () => Instance._AfterAllLayersUpdate();
	/// <summary>
	/// This function is called after all rendering layer get updated
	/// </summary>
	protected abstract void _AfterAllLayersUpdate ();


	/// <inheritdoc cref="_OnLayerUpdate"/>
	internal static void OnLayerUpdate (int layerIndex, Cell[] cells, int cellCount) => Instance._OnLayerUpdate(layerIndex, cells, cellCount);
	/// <summary>
	/// This function holds the logic to draw all rendering cells for the current frame.
	/// </summary>
	/// <param name="layerIndex">Index of the rendering layer</param>
	/// <param name="cells">All rendering cells that may need to be drawn</param>
	/// <param name="cellCount">How many rendering cells need to be drawn</param>
	protected abstract void _OnLayerUpdate (int layerIndex, Cell[] cells, int cellCount);


	// Effect
	/// <summary>
	/// Border size of all screen effects
	/// </summary>
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


	/// <summary>
	/// Active screen effect for given frames long.
	/// </summary>
	/// <param name="effectIndex">Use Const.SCREEN_EFFECT_XXXX for this index</param>
	/// <param name="duration"></param>
	public static void PassEffect (int effectIndex, int duration = 0) => ScreenEffectEnableFrames[effectIndex] = PauselessFrame + duration;


	/// <inheritdoc cref="_Effect_SetTintParams"/>
	public static void PassEffect_Tint (Color32 color, int duration = 0) {
		ScreenEffectEnableFrames[Const.SCREEN_EFFECT_TINT] = PauselessFrame + duration;
		Instance._Effect_SetTintParams(color);
	}
	/// <summary>
	/// Enable color tint screen effect and set the params
	/// </summary>
	protected abstract void _Effect_SetTintParams (Color32 color);


	/// <inheritdoc cref="_Effect_SetDarkenParams"/>
	public static void PassEffect_RetroDarken (float amount, float step = 8, int duration = 0) {
		ScreenEffectEnableFrames[Const.SCREEN_EFFECT_RETRO_DARKEN] = PauselessFrame + duration;
		Instance._Effect_SetDarkenParams(amount, step);
	}
	/// <summary>
	/// Enable darken screen effect and set the params
	/// </summary>
	/// /// <param name="amount">0 means no darken, 1 means full darken</param>
	/// <param name="step">How intermittent the darken should be. Default 8 steps.</param>
	protected abstract void _Effect_SetDarkenParams (float amount, float step);


	/// <inheritdoc cref="_Effect_SetLightenParams"/>
	public static void PassEffect_RetroLighten (float amount, float step = 8, int duration = 0) {
		ScreenEffectEnableFrames[Const.SCREEN_EFFECT_RETRO_LIGHTEN] = PauselessFrame + duration;
		Instance._Effect_SetLightenParams(amount, step);
	}
	/// <summary>
	/// Enable lighten screen effect and set the params
	/// </summary>
	/// <param name="amount">0 means no lighten, 1 means full lighten</param>
	/// <param name="step">How intermittent the lighten should be. Default 8 steps.</param>
	/// <example><code>
	/// using AngeliA;
	/// 
	/// namespace AngeliaGame;
	/// 
	/// public class Example {
	/// 
	/// 	[OnGameUpdate]
	/// 	internal static void OnGameUpdate () {
	/// 		Game.PassEffect_RetroLighten(
	/// 			QTest.Float("amount", 0f, 0f, 1f),
	/// 			QTest.Float("step", 8f, 2f, 16f),
	/// 			1
	/// 		);
	/// 	}
	/// 
	/// }
	/// </code></example>
	protected abstract void _Effect_SetLightenParams (float amount, float step);


	/// <inheritdoc cref="_Effect_SetVignetteParams"/>
	public static void PassEffect_Vignette (float radius, float feather, float offsetX, float offsetY, float round, int duration = 0) {
		ScreenEffectEnableFrames[Const.SCREEN_EFFECT_VIGNETTE] = PauselessFrame + duration;
		Instance._Effect_SetVignetteParams(radius, feather, offsetX, offsetY, round);
	}
	/// <summary>
	/// Enable vignette screen effect and set the params
	/// </summary>
	/// <param name="radius">Size of the circle view. 0 means no view. 1 means the view covers whole screen.</param>
	/// <param name="feather">How smooth is the edge of the circle view. 0 means sharp edge. 1 means smooth edge.</param>
	/// <param name="offsetX">Position offset of the circle view. 0 means view's center at left edge of screen. 1 for right edge.</param>
	/// <param name="offsetY">Position offset of the circle view. 0 means view's center at bottom edge of screen. 1 for top edge.</param>
	/// <param name="round">How perfect the circle view is. 0 means the aspect ratio is the same with window aspect ratio. 1 means perfect circle.</param>
	/// <example><code>
	/// using AngeliA;
	///
	/// namespace AngeliaGame;
	///
	/// public class Example {
	///
	///		[OnGameUpdate]
	///		internal static void OnGameUpdate () {
	///			Game.PassEffect_Vignette(
	///				QTest.Float("radius", 1f, 0f, 1f),
	///				QTest.Float("feather", 0f, 0f, 1f),
	///				QTest.Float("x", 0f, -1f, 1f),
	///				QTest.Float("y", 0f, -1f, 1f),
	///				QTest.Float("round", 0f, 0f, 1f),
	///				1
	///			);
	///		}
	///
	/// }
	/// </code></example>
	protected abstract void _Effect_SetVignetteParams (float radius, float feather, float offsetX, float offsetY, float round);

	/// <summary>
	/// Enable greyscale screen effect
	/// </summary>
	/// <example><code>
	/// using AngeliA;
	/// 
	/// namespace AngeliaGame;
	/// 
	/// public class Example {
	/// 
	/// 	[OnGameUpdate]
	/// 	internal static void OnGameUpdate () {
	/// 		Game.PassEffect_Greyscale(1);
	/// 	}
	/// 
	/// }
	/// </code></example>
	public static void PassEffect_Greyscale (int duration = 0) {
		ScreenEffectEnableFrames[Const.SCREEN_EFFECT_GREYSCALE] = PauselessFrame + duration;
	}

	/// <summary>
	/// Enable invert screen effect
	/// </summary>
	/// <example><code>
	/// using AngeliA;
	/// 
	/// namespace AngeliaGame;
	/// 
	/// public class Example {
	/// 
	/// 	[OnGameUpdate]
	/// 	internal static void OnGameUpdate () {
	/// 		Game.PassEffect_Invert(1);
	/// 	}
	/// 
	/// }
	/// </code></example>
	public static void PassEffect_Invert (int duration = 0) {
		ScreenEffectEnableFrames[Const.SCREEN_EFFECT_INVERT] = PauselessFrame + duration;
	}


	/// <inheritdoc cref="_GetEffectEnable"/>
	public static bool GetEffectEnable (int effectIndex) => Instance._GetEffectEnable(effectIndex);
	/// <summary>
	/// True if the given screen effect is currently enabled.
	/// </summary>
	/// <param name="effectIndex">Use Const.SCREEN_EFFECT_XXXX for this index</param>
	protected abstract bool _GetEffectEnable (int effectIndex);


	/// <inheritdoc cref="_SetEffectEnable"/>
	public static void SetEffectEnable (int effectIndex, bool enable) => Instance._SetEffectEnable(effectIndex, enable);
	/// <summary>
	/// Make given screen effect enable of disable.
	/// </summary>
	/// <param name="effectIndex">Use Const.SCREEN_EFFECT_XXXX for this index</param>
	/// <param name="enable"></param>
	protected abstract void _SetEffectEnable (int effectIndex, bool enable);



	// Texture
	/// <inheritdoc cref="_GetTextureFromPixels"/>
	public static object GetTextureFromPixels (Color32[] pixels, int width, int height) => Instance._GetTextureFromPixels(pixels, width, height);
	/// <summary>
	/// Create a new instance of texture from given pixels data. 
	/// </summary>
	/// <returns>Never return null and Don't throw exception.</returns>
	/// <param name="pixels">Pixel data. 0 means bottom-left corner. 1 makes it goes right for 1 pixel.</param>
	/// <param name="width">Width of the pixel data in pixel</param>
	/// <param name="height">Heigh of the pixel data in pixel</param>
	protected abstract object _GetTextureFromPixels (Color32[] pixels, int width, int height);


	/// <inheritdoc cref="_GetPixelsFromTexture"/>
	public static Color32[] GetPixelsFromTexture (object texture) => Instance._GetPixelsFromTexture(texture);
	/// <summary>
	/// Create a new instance of Color32 array from the given texture
	/// </summary>
	/// <returns>Return [] when invalid. Don't throw exception.</returns>
	protected abstract Color32[] _GetPixelsFromTexture (object texture);


	/// <inheritdoc cref="_FillPixelsIntoTexture"/>
	public static void FillPixelsIntoTexture (Color32[] pixels, object texture) => Instance._FillPixelsIntoTexture(pixels, texture);
	/// <summary>
	/// Set the given pixel data into the given texture instance.
	/// </summary>
	protected abstract void _FillPixelsIntoTexture (Color32[] pixels, object texture);


	/// <inheritdoc cref="_GetTextureSize"/>
	public static Int2 GetTextureSize (object texture) => Instance._GetTextureSize(texture);
	/// <summary>
	/// Get the size in pixel of the given texture instance
	/// </summary>
	/// <returns>Return default when invalid. Don't throw exception.</returns>
	protected abstract Int2 _GetTextureSize (object texture);


	/// <inheritdoc cref="_PngBytesToTexture"/>
	public static object PngBytesToTexture (byte[] bytes) => Instance._PngBytesToTexture(bytes);
	/// <summary>
	/// Create a new instance of texture from a byte array load from png file.
	/// </summary>
	/// <returns>Never return null and Don't throw exception.</returns>
	protected abstract object _PngBytesToTexture (byte[] bytes);


	/// <inheritdoc cref="_TextureToPngBytes"/>
	public static byte[] TextureToPngBytes (object texture) => Instance._TextureToPngBytes(texture);
	/// <summary>
	/// Encode the given texture instance into png byte array.
	/// </summary>
	/// <returns>Return [] when invalid. Don't throw exception.</returns>
	protected abstract byte[] _TextureToPngBytes (object texture);


	/// <inheritdoc cref="_UnloadTexture"/>
	public static void UnloadTexture (object texture) => Instance._UnloadTexture(texture);
	/// <summary>
	/// Unload the given texture instance from memory
	/// </summary>
	protected abstract void _UnloadTexture (object texture);


	/// <inheritdoc cref="_GetTextureID"/>
	public static uint? GetTextureID (object texture) => Instance._GetTextureID(texture);
	/// <summary>
	/// Get internal ID of the given texture instance.
	/// </summary>
	protected abstract uint? _GetTextureID (object texture);


	/// <inheritdoc cref="_IsTextureReady"/>
	public static bool IsTextureReady (object texture) => Instance._IsTextureReady(texture);
	/// <summary>
	/// True if the given texture instance is ready to use
	/// </summary>
	protected abstract bool _IsTextureReady (object texture);


	/// <inheritdoc cref="_GetResizedTexture"/>
	public static object GetResizedTexture (object texture, int newWidth, int newHeight, bool nearestNeighbor = true) => Instance._GetResizedTexture(texture, newWidth, newHeight, nearestNeighbor);
	/// <summary>
	/// Create a new instance of texture which is the resized version of the given texture.
	/// </summary>
	/// <param name="texture"></param>
	/// <param name="newWidth"></param>
	/// <param name="newHeight"></param>
	/// <param name="nearestNeighbor">True if the misaligned pixels should be averaged with it's nearby pixels</param>
	/// <returns>Return null if invalid. Don't throw exception.</returns>
	protected abstract object _GetResizedTexture (object texture, int newWidth, int newHeight, bool nearestNeighbor = true);


	// Gizmos
	/// <inheritdoc cref="DrawGizmosFrame(IRect, Color32, Int4, Int4)"/>
	public static void DrawGizmosFrame (IRect rect, Color32 color, int thickness, int gap = 0) => DrawGizmosFrame(rect, color, new Int4(thickness, thickness, thickness, thickness), new Int4(gap, gap, gap, gap));
	/// <summary>
	/// Draw a holo rectangle as gizmos for current frame
	/// </summary>
	/// <param name="rect">Rect position in global space</param>
	/// <param name="color">Color tint</param>
	/// <param name="thickness">Border size in global space</param>
	/// <param name="gap">How big the fracture part in the middle in global space</param>
	public static void DrawGizmosFrame (IRect rect, Color32 color, Int4 thickness, Int4 gap = default) {
		// Down
		if (thickness.down > 0) {
			var edge = rect.EdgeInside(Direction4.Down, thickness.down);
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
			var edge = rect.EdgeInside(Direction4.Up, thickness.up);
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
			var edge = rect.EdgeInside(Direction4.Left, thickness.left);
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
			var edge = rect.EdgeInside(Direction4.Right, thickness.right);
			if (gap.right == 0) {
				DrawGizmosRect(edge, color);
			} else {
				int shrink = edge.height - (edge.height - gap.right) / 2;
				DrawGizmosRect(edge.Shrink(0, 0, shrink, 0), color);
				DrawGizmosRect(edge.Shrink(0, 0, 0, shrink), color);
			}
		}
	}


	/// <inheritdoc cref="_DrawGizmosRect(IRect, Color32, Color32, Color32, Color32)"/>
	public static void DrawGizmosRect (IRect rect, Color32 color) => Instance._DrawGizmosRect(rect, color);
	/// <inheritdoc cref="_DrawGizmosRect(IRect, Color32, Color32, Color32, Color32)"/>
	protected abstract void _DrawGizmosRect (IRect rect, Color32 color);


	/// <inheritdoc cref="_DrawGizmosRect(IRect, Color32, Color32, Color32, Color32)"/>
	public static void DrawGizmosRect (IRect rect, Color32 colorT, Color32 colorB) => Instance._DrawGizmosRect(rect, colorT, colorB);
	/// <inheritdoc cref="_DrawGizmosRect(IRect, Color32, Color32, Color32, Color32)"/>
	protected abstract void _DrawGizmosRect (IRect rect, Color32 colorT, Color32 colorB);


	/// <inheritdoc cref="_DrawGizmosRect(IRect, Color32, Color32, Color32, Color32)"/>
	public static void DrawGizmosRect (IRect rect, Color32 colorTL, Color32 colorTR, Color32 colorBL, Color32 colorBR) => Instance._DrawGizmosRect(rect, colorTL, colorTR, colorBL, colorBR);
	/// <summary>
	/// Draw a rectangle as gizmos for current frame
	/// </summary>
	/// <param name="rect">Rect position in global space</param>
	/// <param name="colorTL">Color tint</param>
	/// <param name="colorTR">Color tint</param>
	/// <param name="colorBL">Color tint</param>
	/// <param name="colorBR">Color tint</param>
	/// <param name="colorT">Color tint</param>
	/// <param name="colorB">Color tint</param>
	/// <param name="color">Color tint</param>
	protected abstract void _DrawGizmosRect (IRect rect, Color32 colorTL, Color32 colorTR, Color32 colorBL, Color32 colorBR);


	/// <inheritdoc cref="_DrawGizmosTexture"/>
	public static void DrawGizmosTexture (IRect rect, object texture, bool inverse = false) => Instance._DrawGizmosTexture(rect, new FRect(0f, 0f, 1f, 1f), texture, Color32.WHITE, inverse);
	/// <inheritdoc cref="_DrawGizmosTexture"/>
	public static void DrawGizmosTexture (IRect rect, FRect uv, object texture, bool inverse = false) => Instance._DrawGizmosTexture(rect, uv, texture, Color32.WHITE, inverse);
	/// <inheritdoc cref="_DrawGizmosTexture"/>
	public static void DrawGizmosTexture (IRect rect, object texture, Color32 tint, bool inverse = false) => Instance._DrawGizmosTexture(rect, new FRect(0f, 0f, 1f, 1f), texture, tint, inverse);
	/// <inheritdoc cref="_DrawGizmosTexture"/>
	public static void DrawGizmosTexture (IRect rect, FRect uv, object texture, Color32 tint, bool inverse = false) => Instance._DrawGizmosTexture(rect, uv, texture, tint, inverse);
	/// <summary>
	/// Draw the given texture as gizmos for current frame
	/// </summary>
	/// <param name="rect">Rect position</param>
	/// <param name="uv">Which part of this texture should be draw. (0, 0, 1, 1) means all of them. (0, 0, 0.5f, 1) means left half.</param>
	/// <param name="texture"></param>
	/// <param name="tint">Color tint</param>
	/// <param name="inverse">True if the texture display as the inversed color of the current rendered pixel on screen.</param>
	protected abstract void _DrawGizmosTexture (IRect rect, FRect uv, object texture, Color32 tint, bool inverse);


	/// <inheritdoc cref="_DrawGizmosLine"/>
	public static void DrawGizmosLine (int startX, int startY, int endX, int endY, int thickness, Color32 color) => Instance._DrawGizmosLine(startX, startY, endX, endY, thickness, color);
	/// <summary>
	/// Draw a line as gizmos for current frame
	/// </summary>
	/// <param name="startX">Start point of the line in global space</param>
	/// <param name="startY">Start point of the line in global space</param>
	/// <param name="endX">End point of the line in global space</param>
	/// <param name="endY">End point of the line in global space</param>
	/// <param name="thickness">Thickness in global space</param>
	/// <param name="color">Color tint</param>
	protected abstract void _DrawGizmosLine (int startX, int startY, int endX, int endY, int thickness, Color32 color);


	/// <inheritdoc cref="_IgnoreGizmos"/>
	public static void IgnoreGizmos (int duration = 0) => Instance._IgnoreGizmos(duration);
	/// <summary>
	/// Hide all gizmos for given frames long.
	/// </summary>
	protected abstract void _IgnoreGizmos (int duration = 0);

	/// <summary>
	/// Gizmos should cover the UI rendering layer if current global frame less than this value
	/// </summary>
	protected int GizmosOnTopOfUiFrame { get; private set; } = -1;
	/// <summary>
	/// Make gizmos cover UI rendering layer for given frames long.
	/// </summary>
	public static void ForceGizmosOnTopOfUI (int duration = 0) => Instance.GizmosOnTopOfUiFrame = GlobalFrame + duration;
	/// <summary>
	/// Do not make gizmos cover UI rendering layer anymore.
	/// </summary>
	public static void CancelGizmosOnTopOfUI () => Instance.GizmosOnTopOfUiFrame = -1;

	// Doodle
	/// <summary>
	/// True if the doodle pixels are displaying at this frame
	/// </summary>
	public static bool ShowingDoodle => GlobalFrame <= Instance.DoodleFrame + 1;
	/// <summary>
	/// Border size in screen space for the doodle canvas
	/// </summary>
	public static Int4 DoodleScreenPadding { get; set; } = new(0, 0, 0, 0);
	/// <summary>
	/// Doodle pixels should be rendered of the global frame is less or equal to (this value + 1)
	/// </summary>
	protected int DoodleFrame { get; private set; } = -1;

	/// <summary>
	/// Doodle pixels should cover the UI rendering layer if current global frame less than this value
	/// </summary>
	protected int DoodleOnTopOfUiFrame { get; private set; } = -1;
	/// <summary>
	///  Make doodle pixels cover UI rendering layer for given frames long.
	/// </summary>
	public static void ForceDoodleOnTopOfUI (int duration = 0) => Instance.DoodleOnTopOfUiFrame = GlobalFrame + duration;
	/// <summary>
	/// Do not make doodle pixels cover UI rendering layer anymore.
	/// </summary>
	public static void CancelDoodleOnTopOfUI () => Instance.DoodleOnTopOfUiFrame = -1;

	/// <summary>
	/// Display the doodled pixels
	/// </summary>
	public static void ShowDoodle (int duration = 0) => Instance.DoodleFrame = GlobalFrame + duration;

	/// <summary>
	/// Do not display doodled pixels
	/// </summary>
	public static void HideDoodle () => Instance.DoodleFrame = -1;


	/// <inheritdoc cref="_ResetDoodle"/>
	public static void ResetDoodle () => Instance._ResetDoodle();
	/// <summary>
	/// Clear the doodle pixels canvas
	/// </summary>
	protected abstract void _ResetDoodle ();


	/// <inheritdoc cref="_SetDoodleOffset"/>
	public static void SetDoodleOffset (Float2 screenOffset) => Instance._SetDoodleOffset(screenOffset);
	/// <summary>
	/// Set position offset of the doodle pixels. x=1 means right shift the whole screen width.
	/// </summary>
	protected abstract void _SetDoodleOffset (Float2 screenOffset);


	/// <inheritdoc cref="_SetDoodleZoom"/>
	public static void SetDoodleZoom (float zoom) => Instance._SetDoodleZoom(zoom);
	/// <summary>
	/// Set zoom amount of the doodle pixels. 1 means general size. 2 means zoom-in to double the size.
	/// </summary>
	protected abstract void _SetDoodleZoom (float zoom);


	/// <summary>
	/// Doodle the given color as pixels into the screen-space canvas. Pixels remains on screen until you hide all doodle or reset the canvas.
	/// </summary>
	/// <param name="screenRect">Rect position in screen space that swap around when out of range</param>
	/// <param name="color"></param>
	public static void DoodleRectSwap (FRect screenRect, Color32 color) {
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


	/// <inheritdoc cref="_DoodleRect"/>
	public static void DoodleRect (FRect screenRect, Color32 color) => Instance._DoodleRect(screenRect, color);
	/// <summary>
	/// Doodle the given color as pixels into the screen-space canvas. Pixels remains on screen until you hide all doodle or reset the canvas.
	/// </summary>
	/// <param name="screenRect">Rect position in screen space</param>
	/// <param name="color"></param>
	protected abstract void _DoodleRect (FRect screenRect, Color32 color);


	/// <inheritdoc cref="_DoodleWorld"/>
	public static void DoodleWorld (IBlockSquad squad, FRect screenRect, IRect worldUnitRange, int z, bool ignoreLevel = false, bool ignoreBG = false, bool ignoreEntity = false, bool ignoreElement = true) => Instance._DoodleWorld(squad, screenRect, worldUnitRange, z, ignoreLevel, ignoreBG, ignoreEntity, ignoreElement);
	/// <summary>
	/// Doodle the given map data on screen based on summary tint from rendering sheet
	/// </summary>
	/// <param name="squad">Source of the map block data</param>
	/// <param name="screenRect">Position rect in screen space for the given world-unit-range</param>
	/// <param name="worldUnitRange">Rect range on the map in unit space</param>
	/// <param name="z">Position Z on the map</param>
	/// <param name="ignoreLevel">True if level blocks should be ignored</param>
	/// <param name="ignoreBG">True if background blocks should be ignored</param>
	/// <param name="ignoreEntity">True if entity blocks should be ignored</param>
	/// <param name="ignoreElement">True if element blocks should be ignored</param>
	protected abstract void _DoodleWorld (IBlockSquad squad, FRect screenRect, IRect worldUnitRange, int z, bool ignoreLevel = false, bool ignoreBG = false, bool ignoreEntity = false, bool ignoreElement = true);


	// Text
	public static int BuiltInFontCount { get; private set; } = 0;


	/// <inheritdoc cref="_GetFontCount"/>
	public static int FontCount => Instance._GetFontCount();
	/// <summary>
	/// Total count of loaded fonts
	/// </summary>
	protected abstract int _GetFontCount ();


	/// <inheritdoc cref="_GetClipboardText"/>
	public static string GetClipboardText () => Instance._GetClipboardText();
	/// <summary>
	/// Text content of the current system clipboard
	/// </summary>
	protected abstract string _GetClipboardText ();


	/// <inheritdoc cref="_SetClipboardText"/>
	public static void SetClipboardText (string text) => Instance._SetClipboardText(text);
	/// <summary>
	/// Set the text content of the system clipboard
	/// </summary>
	protected abstract void _SetClipboardText (string text);


	/// <inheritdoc cref="_GetCharSprite"/>
	public static bool GetCharSprite (int fontIndex, char c, out CharSprite result) => Instance._GetCharSprite(fontIndex, c, out result);
	/// <summary>
	/// Get artwork sprite for rendering a text character
	/// </summary>
	/// <param name="fontIndex"></param>
	/// <param name="c"></param>
	/// <param name="result"></param>
	/// <returns>True if the sprite is successfuly required</returns>
	protected abstract bool _GetCharSprite (int fontIndex, char c, out CharSprite result);

	/// <summary>
	/// Create a new instance of the internal data for font
	/// </summary>
	protected abstract FontData CreateNewFontData ();


}
