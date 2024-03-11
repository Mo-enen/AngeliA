using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using AngeliA;
using AngeliA.Framework;
using Raylib_cs;


namespace AngeliaRuntime.Framework;


public partial class RayGame {


	// Data
	private Texture2D EMPTY_TEXTURE;
	private readonly static System.Random CA_Ran = new(2353456);
	private readonly Shader[] ScreenEffectShaders = new Shader[Const.SCREEN_EFFECT_COUNT];
	private readonly bool[] ScreenEffectEnables = new bool[Const.SCREEN_EFFECT_COUNT].FillWithValue(false);
	private static readonly Dictionary<int, Texture2D> TexturePool = new();
	private FontData[] Fonts;
	private FRect CameraRange = new(0, 0, 1f, 1f);
	private IRect ScreenRenderRect;
	private Shader LerpShader;
	private Shader ColorShader;
	private Shader TextShader;
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


	// MSG
	private void InitializeShader () {

		// Shaders
		LerpShader = Raylib.LoadShaderFromMemory(BuiltInShader.BASIC_VS, BuiltInShader.LERP_FS);
		ColorShader = Raylib.LoadShaderFromMemory(BuiltInShader.BASIC_VS, BuiltInShader.COLOR_FS);
		TextShader = Raylib.LoadShaderFromMemory(BuiltInShader.BASIC_VS, BuiltInShader.TEXT_FS);

		// Effects
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			ScreenEffectShaders[i] = Raylib.LoadShaderFromMemory(
				BuiltInShader.BASIC_VS, BuiltInShader.EFFECTS[i]
			);
		}

		// Shader Index
		ShaderPropIndex_DarkenAmount = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_RETRO_DARKEN], "Amount");
		ShaderPropIndex_LightenAmount = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_RETRO_LIGHTEN], "Amount");
		ShaderPropIndex_TintAmount = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_TINT], "Tint");
		ShaderPropIndex_VignetteRadius = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_VIGNETTE], "Radius");
		ShaderPropIndex_VignetteFeather = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_VIGNETTE], "Feather");
		ShaderPropIndex_VignetteOffsetX = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_VIGNETTE], "OffsetX");
		ShaderPropIndex_VignetteOffsetY = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_VIGNETTE], "OffsetY");
		ShaderPropIndex_VignetteRound = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_VIGNETTE], "Round");
		ShaderPropIndex_VignetteAspect = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_VIGNETTE], "Aspect");
		ShaderPropIndex_CA_RED_X = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_CHROMATIC_ABERRATION], "RedX");
		ShaderPropIndex_CA_RED_Y = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_CHROMATIC_ABERRATION], "RedY");
		ShaderPropIndex_CA_GREEN_X = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_CHROMATIC_ABERRATION], "GreenX");
		ShaderPropIndex_CA_GREEN_Y = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_CHROMATIC_ABERRATION], "GreenY");
		ShaderPropIndex_CA_BLUE_X = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_CHROMATIC_ABERRATION], "BlueX");
		ShaderPropIndex_CA_BLUE_Y = Raylib.GetShaderLocation(ScreenEffectShaders[Const.SCREEN_EFFECT_CHROMATIC_ABERRATION], "BlueY");
	}

	private bool PrepareScreenEffects () {
		bool hasScreenEffectEnabled = false;
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			if (ScreenEffectEnables[i]) {
				hasScreenEffectEnabled = true;
				if (RenderTexture.Texture.Width != ScreenWidth || RenderTexture.Texture.Height != ScreenHeight) {
					RenderTexture = Raylib.LoadRenderTexture(ScreenWidth, ScreenHeight);
					Raylib.SetTextureWrap(RenderTexture.Texture, TextureWrap.Clamp);
				}
				break;
			}
		}
		return hasScreenEffectEnabled;
	}

	private void UpdateScreenEffect () {
		// Chromatic Aberration
		if (ScreenEffectEnables[Const.SCREEN_EFFECT_CHROMATIC_ABERRATION]) {
			var caShader = ScreenEffectShaders[Const.SCREEN_EFFECT_CHROMATIC_ABERRATION];
			const float CA_PingPongTime = 0.618f;
			const float CA_PingPongMin = 0f;
			const float CA_PingPongMax = 0.015f;
			if (Raylib.GetTime() % CA_PingPongTime > CA_PingPongTime / 2f) {
				Raylib.SetShaderValue<float>(caShader, ShaderPropIndex_CA_RED_X, GetRandomAmount(0f), ShaderUniformDataType.Float);
				Raylib.SetShaderValue<float>(caShader, ShaderPropIndex_CA_RED_Y, GetRandomAmount(0.2f), ShaderUniformDataType.Float);
				Raylib.SetShaderValue<float>(caShader, ShaderPropIndex_CA_BLUE_X, GetRandomAmount(0.7f), ShaderUniformDataType.Float);
				Raylib.SetShaderValue<float>(caShader, ShaderPropIndex_CA_BLUE_Y, GetRandomAmount(0.4f), ShaderUniformDataType.Float);
			} else {
				Raylib.SetShaderValue<float>(caShader, ShaderPropIndex_CA_GREEN_X, GetRandomAmount(0f), ShaderUniformDataType.Float);
				Raylib.SetShaderValue<float>(caShader, ShaderPropIndex_CA_GREEN_Y, GetRandomAmount(0.8f), ShaderUniformDataType.Float);
				Raylib.SetShaderValue<float>(caShader, ShaderPropIndex_CA_BLUE_X, GetRandomAmount(0.4f), ShaderUniformDataType.Float);
				Raylib.SetShaderValue<float>(caShader, ShaderPropIndex_CA_BLUE_Y, GetRandomAmount(0.72f), ShaderUniformDataType.Float);
			}
			static float GetRandomAmount (float timeOffset) {
				int range = (int)(Util.RemapUnclamped(
					0f, CA_PingPongTime,
					CA_PingPongMin, CA_PingPongMax,
					Util.PingPong((float)Raylib.GetTime() + timeOffset * CA_PingPongTime, CA_PingPongTime)
				) * 100000f);
				return CA_Ran.Next(-range, range) / 100000f;
			}
		}
		// Vig
		if (ScreenEffectEnables[Const.SCREEN_EFFECT_VIGNETTE]) {
			var vShader = ScreenEffectShaders[Const.SCREEN_EFFECT_VIGNETTE];
			Raylib.SetShaderValue<float>(
				vShader, ShaderPropIndex_VignetteAspect,
				(float)ScreenWidth / ScreenHeight,
				ShaderUniformDataType.Float
			);
		}
	}

	[OnSheetLoaded]
	internal static void OnSheetLoaded () {
		foreach (var (_, texture) in TexturePool) UnloadTexture(texture);
		TexturePool.Clear();
		TextureUtil.FillSheetIntoTexturePool(Renderer.Sheet, TexturePool);
	}


	// Camera
	protected override FRect _GetCameraScreenLocacion () => CameraRange;
	protected override void _SetCameraScreenLocacion (FRect rect) => CameraRange = rect;


	// Render
	protected override void _OnRenderingLayerCreated (int index, string name, int sortingOrder, int capacity) { }

	protected override void _OnCameraUpdate () {
		ScreenRenderRect = CameraRange.x.AlmostZero() ?
			new IRect(0, 0, ScreenWidth, ScreenHeight) :
			new IRect(
				Util.LerpUnclamped(0, ScreenWidth, CameraRange.x).RoundToInt(),
				0,
				(ScreenWidth * CameraRange.width).RoundToInt(),
				ScreenHeight
			);
	}

	protected override void _OnLayerUpdate (int layerIndex, bool isUiLayer, bool isTextLayer, Cell[] cells, int cellCount) {
		if (PauselessFrame < 4) return;
		if (isTextLayer) {
			UpdateLayer_Text(layerIndex, cells, cellCount);
		} else {
			UpdateLayer_Cell(layerIndex, isUiLayer, cells, cellCount);
		}
	}

	private void UpdateLayer_Cell (int layerIndex, bool isUiLayer, Cell[] cells, int cellCount) {

		var cameraRect = Renderer.CameraRect;
		int cameraL = cameraRect.x;
		int cameraR = cameraRect.xMax;
		int cameraD = cameraRect.y;
		int cameraU = cameraRect.yMax;
		int screenL = ScreenRenderRect.x;
		int screenR = ScreenRenderRect.xMax;
		int screenD = ScreenRenderRect.y;
		int screenU = ScreenRenderRect.yMax;

		bool usingShader = false;
		bool usingBlend = false;
		bool useAlpha = !IsTransparentWindow;

		// Shader
		switch (layerIndex) {
			case RenderLayer.WALLPAPER:
			case RenderLayer.BEHIND:
				if (LerpShader.Id == 0) break;
				Raylib.BeginShaderMode(LerpShader);
				usingShader = true;
				break;
			case RenderLayer.SHADOW:
			case RenderLayer.COLOR:
				if (ColorShader.Id == 0) break;
				Raylib.BeginShaderMode(ColorShader);
				usingShader = true;
				break;
		}

		// Blend
		if (layerIndex == RenderLayer.MULT) {
			Raylib.BeginBlendMode(BlendMode.Multiplied);
			usingBlend = true;
		}

		if (layerIndex == RenderLayer.ADD) {
			Raylib.BeginBlendMode(BlendMode.Additive);
			usingBlend = true;
		}

		for (int i = 0; i < cellCount; i++) {
			try {

				var cell = cells[isUiLayer ? cellCount - i - 1 : i];
				cell.Color.a = (byte)(useAlpha ? cell.Color.a : 255);

				// Cell
				var sprite = cell.Sprite;
				if (sprite == null || cell.Width == 0 || cell.Height == 0 || cell.Color.a == 0) continue;

				if (!TexturePool.TryGetValue(sprite.GlobalID, out var texture)) continue;

				// UV
				int pixelWidth = sprite.PixelRect.width;
				int pixelHeight = sprite.PixelRect.height;
				float sourceL, sourceR, sourceD, sourceU;
				if (cell.BorderSide == Alignment.Full) {
					sourceL = 0f;
					sourceR = pixelWidth;
					sourceD = 0f;
					sourceU = pixelHeight;
				} else {
					Util.GetSlicedUvBorder(sprite, cell.BorderSide, out var bl, out _, out _, out var tr);
					sourceL = bl.x * sprite.PixelRect.width;
					sourceR = tr.x * sprite.PixelRect.width;
					sourceD = pixelHeight - tr.y * pixelHeight;
					sourceU = pixelHeight - bl.y * pixelHeight;
				}
				var source = new Rectangle(
					sourceL,
					sourceD,
					sourceR - sourceL,
					sourceU - sourceD
				);

				// Pos
				var dest = new Rectangle(
					Util.RemapUnclamped(cameraL, cameraR, screenL, screenR, (float)cell.X),
					Util.RemapUnclamped(cameraD, cameraU, screenU, screenD, (float)cell.Y),
					cell.Width.Abs() * ScreenRenderRect.width / (float)cameraRect.width,
					cell.Height.Abs() * ScreenRenderRect.height / (float)cameraRect.height
				);

				float pivotX = cell.Width > 0 ? cell.PivotX : 1f - cell.PivotX;
				float pivotY = cell.Height > 0 ? 1f - cell.PivotY : cell.PivotY;

				// Shift
				ShiftCell(cell, ref source, ref dest, ref pivotX, ref pivotY, out bool skipCell);
				if (skipCell) continue;

				// Draw
				source.Width *= cell.Width.Sign();
				source.Height *= cell.Height.Sign();
				Raylib.DrawTexturePro(
					texture, source.ShrinkRectangle(0.02f), dest.ExpandRectangle(0.5f),
					new Vector2(
						pivotX * dest.Width,
						pivotY * dest.Height
					), cell.Rotation, cell.Color.ToRaylib()
				);

			} catch (System.Exception ex) { Util.LogException(ex); }
		}

		if (usingShader) Raylib.EndShaderMode();
		if (usingBlend) Raylib.EndBlendMode();

	}

	private void UpdateLayer_Text (int layerIndex, Cell[] cells, int cellCount) {

		var cameraRect = Renderer.CameraRect;
		int cameraL = cameraRect.x;
		int cameraR = cameraRect.xMax;
		int cameraD = cameraRect.y;
		int cameraU = cameraRect.yMax;
		int screenL = ScreenRenderRect.x;
		int screenR = ScreenRenderRect.xMax;
		int screenD = ScreenRenderRect.y;
		int screenU = ScreenRenderRect.yMax;

		bool usingShader = false;

		if (TextShader.Id != 0) {
			Raylib.BeginShaderMode(TextShader);
			usingShader = true;
		}

		for (int i = 0; i < cellCount; i++) {
			try {

				var cell = cells[i];
				var sprite = cell.TextSprite;

				if (sprite == null || cell.Width == 0 || cell.Height == 0) continue;

				var fontData = Fonts[layerIndex];
				if (!fontData.TryGetTexture(sprite.Char, out var texture)) continue;

				// Source
				var source = new Rectangle(0, 0, texture.Width, texture.Height);

				// Pos
				var dest = new Rectangle(
					Util.RemapUnclamped(cameraL, cameraR, screenL, screenR, (float)cell.X),
					Util.RemapUnclamped(cameraD, cameraU, screenU, screenD, (float)cell.Y),
					cell.Width * ScreenRenderRect.width / (float)cameraRect.width,
					cell.Height * ScreenRenderRect.height / (float)cameraRect.height
				);

				float pivotX = 0f;
				float pivotY = 1f;

				// Shift
				ShiftCell(cell, ref source, ref dest, ref pivotX, ref pivotY, out bool skipCell);
				if (skipCell) continue;

				// Draw
				Raylib.DrawTexturePro(
					texture, source, dest,
					new Vector2(
						pivotX * dest.Width,
						pivotY * dest.Height
					), rotation: 0, cell.Color.ToRaylib()
				);

			} catch (System.Exception ex) {
				Util.LogException(ex);
			}
		}

		if (usingShader) Raylib.EndShaderMode();

	}

	private static void ShiftCell (Cell cell, ref Rectangle source, ref Rectangle dest, ref float pivotX, ref float pivotY, out bool skipCell) {

		skipCell = false;

		if (cell.Shift.IsZero) return;
		if (cell.Shift.horizontal >= cell.Width.Abs()) goto _SKIP_;
		if (cell.Shift.vertical >= cell.Height.Abs()) goto _SKIP_;

		float shiftL = ((float)cell.Shift.left / cell.Width.Abs()).Clamp01();
		float shiftR = ((float)cell.Shift.right / cell.Width.Abs()).Clamp01();
		float shiftD = ((float)cell.Shift.down / cell.Height.Abs()).Clamp01();
		float shiftU = ((float)cell.Shift.up / cell.Height.Abs()).Clamp01();

		// Shift Dest/Source
		var newDest = dest;
		var newSource = source;

		// L
		if (cell.Width != 0) {
			float shift = dest.Width * shiftL;
			newDest.X -= cell.Width < 0 ? shift : 0;
			newDest.Width -= shift;
			shift = source.Width * shiftL;
			newSource.X += shift;
			newSource.Width -= shift;
		}

		// R
		if (cell.Width != 0) {
			float shift = dest.Width * shiftR;
			newDest.X += cell.Width < 0 ? shift : 0;
			newDest.Width -= shift;
			newSource.Width -= source.Width * shiftR;
		}

		// D
		if (cell.Height != 0) {
			float shift = dest.Height * shiftD;
			newDest.Y += cell.Height < 0 ? shift : 0;
			newDest.Height -= dest.Height * shiftD;
			newSource.Height -= source.Height * shiftD;
		}

		// U
		if (cell.Height != 0) {
			float shift = dest.Height * shiftU;
			newDest.Y -= cell.Height < 0 ? shift : 0;
			newDest.Height -= shift;
			shift = source.Height * shiftU;
			newSource.Y += shift;
			newSource.Height -= shift;
		}

		if (newDest.Width.AlmostZero() || newDest.Height.AlmostZero()) goto _SKIP_;

		// Shift Pivot
		pivotX = (pivotX - shiftL) * dest.Width / newDest.Width;
		pivotY = (pivotY - shiftU) * dest.Height / newDest.Height;
		dest = newDest;
		source = newSource;

		return;
		_SKIP_:;
		skipCell = true;
	}


	// Effect
	protected override bool _GetEffectEnable (int effectIndex) => ScreenEffectEnables[effectIndex];

	protected override void _SetEffectEnable (int effectIndex, bool enable) => ScreenEffectEnables[effectIndex] = enable;

	protected override void _Effect_SetDarkenParams (float amount, float step = 8f) {
		var shader = ScreenEffectShaders[Const.SCREEN_EFFECT_RETRO_DARKEN];
		Raylib.SetShaderValue<float>(
			shader, ShaderPropIndex_DarkenAmount,
			(amount * step).RoundToInt() / step,
			ShaderUniformDataType.Float
		);
	}

	protected override void _Effect_SetLightenParams (float amount, float step = 8f) {
		var shader = ScreenEffectShaders[Const.SCREEN_EFFECT_RETRO_LIGHTEN];
		Raylib.SetShaderValue<float>(
			shader, ShaderPropIndex_LightenAmount,
			(amount * step).RoundToInt() / step,
			ShaderUniformDataType.Float
		);
	}

	protected override void _Effect_SetTintParams (Color32 color) {
		var shader = ScreenEffectShaders[Const.SCREEN_EFFECT_TINT];
		Raylib.SetShaderValue<Vector4>(
			shader,
			ShaderPropIndex_TintAmount,
			new Vector4(color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f),
			ShaderUniformDataType.Vec4
		);
	}

	protected override void _Effect_SetVignetteParams (float radius, float feather, float offsetX, float offsetY, float round) {
		var shader = ScreenEffectShaders[Const.SCREEN_EFFECT_VIGNETTE];
		Raylib.SetShaderValue<float>(shader, ShaderPropIndex_VignetteRadius, radius, ShaderUniformDataType.Float);
		Raylib.SetShaderValue<float>(shader, ShaderPropIndex_VignetteFeather, feather, ShaderUniformDataType.Float);
		Raylib.SetShaderValue<float>(shader, ShaderPropIndex_VignetteOffsetX, offsetX, ShaderUniformDataType.Float);
		Raylib.SetShaderValue<float>(shader, ShaderPropIndex_VignetteOffsetY, offsetY, ShaderUniformDataType.Float);
		Raylib.SetShaderValue<float>(shader, ShaderPropIndex_VignetteRound, round, ShaderUniformDataType.Float);
	}


	// Texture
	protected override object _GetTextureFromPixels (Color32[] pixels, int width, int height) {
		var result = TextureUtil.GetTextureFromPixels(pixels, width, height);
		return result ?? EMPTY_TEXTURE;
	}

	protected override Color32[] _GetPixelsFromTexture (object texture) {
		if (texture is not Texture2D rTexture) return System.Array.Empty<Color32>();
		return TextureUtil.GetPixelsFromTexture(rTexture);
	}

	protected override void _FillPixelsIntoTexture (Color32[] pixels, object texture) {
		if (texture is not Texture2D rTexture) return;
		TextureUtil.FillPixelsIntoTexture(pixels, rTexture);
	}

	protected override Int2 _GetTextureSize (object texture) => texture is Texture2D rTexture ? new Int2(rTexture.Width, rTexture.Height) : default;

	protected override object _PngBytesToTexture (byte[] bytes) {
		var result = TextureUtil.PngBytesToTexture(bytes);
		return result ?? EMPTY_TEXTURE;
	}

	protected override byte[] _TextureToPngBytes (object texture) {
		if (texture is not Texture2D rTexture) return System.Array.Empty<byte>();
		return TextureUtil.TextureToPngBytes(rTexture);
	}

	protected override void _UnloadTexture (object texture) {
		if (texture is not Texture2D rTexture) return;
		Raylib.UnloadTexture(rTexture);
	}


	// GL Gizmos
	protected override void _DrawGizmosRect (IRect rect, Color32 color) {
		var cameraRect = Renderer.CameraRect;
		GizmosRender.DrawGizmosRect(new Rectangle(
			Util.RemapUnclamped(cameraRect.x, cameraRect.xMax, ScreenRenderRect.x, ScreenRenderRect.xMax, rect.x),
			Util.RemapUnclamped(cameraRect.y, cameraRect.yMax, ScreenRenderRect.yMax, ScreenRenderRect.y, rect.yMax),
			rect.width * ScreenRenderRect.width / cameraRect.width,
			rect.height * ScreenRenderRect.height / cameraRect.height
		), color);
	}

	protected override void _DrawGizmosTexture (IRect rect, FRect uv, object texture) {
		if (texture is not Texture2D rTexture) return;
		var cameraRect = Renderer.CameraRect;
		GizmosRender.DrawGizmosTexture(new Rectangle(
			Util.RemapUnclamped(cameraRect.x, cameraRect.xMax, ScreenRenderRect.x, ScreenRenderRect.xMax, rect.x),
			Util.RemapUnclamped(cameraRect.y, cameraRect.yMax, ScreenRenderRect.yMax, ScreenRenderRect.y, rect.yMax),
			rect.width * ScreenRenderRect.width / cameraRect.width,
			rect.height * ScreenRenderRect.height / cameraRect.height
		), new Rectangle(
			uv.x * rTexture.Width,
			uv.y * rTexture.Height,
			uv.width * rTexture.Width,
			uv.height * rTexture.Height
		), rTexture);
	}


	// Text
	protected override void _OnTextLayerCreated (int index, string name, int sortingOrder, int capacity) { }

	protected override int _GetTextLayerCount () => Fonts.Length;

	protected override string _GetTextLayerName (int index) => Fonts[index].Name;

	protected override int _GetFontSize (int index) => Fonts[index].Size;

	protected override CharSprite _GetCharSprite (int layerIndex, char c, int textSize) => RayUtil.CreateCharSprite(Fonts[layerIndex], c);

	protected override string _GetClipboardText () => Raylib.GetClipboardText_();

	protected override void _SetClipboardText (string text) => Raylib.SetClipboardText(text);

	protected override void _SetImeCompositionMode (bool on) {

	}


}