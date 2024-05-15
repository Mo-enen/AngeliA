using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using AngeliA;
using Raylib_cs;

namespace AngeliaRaylib;

public partial class RayGame {



	// Data
	private readonly static System.Random CA_Ran = new(2353456);
	private readonly Shader[] ScreenEffectShaders = new Shader[Const.SCREEN_EFFECT_COUNT];
	private readonly bool[] ScreenEffectEnables = new bool[Const.SCREEN_EFFECT_COUNT].FillWithValue(false);
	private FontData[] Fonts;
	private Shader LerpShader;
	private Shader ColorShader;
	private Shader InverseShader;
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
	private int ShaderPropIndex_INV_TEXTURE;
	private int ShaderPropIndex_INV_SCREEN_SIZE;


	// MSG
	private void InitializeShader () {

		// Shaders
		LerpShader = Raylib.LoadShaderFromMemory(BuiltInShader.BASIC_VS, BuiltInShader.LERP_FS);
		ColorShader = Raylib.LoadShaderFromMemory(BuiltInShader.BASIC_VS, BuiltInShader.COLOR_FS);
		InverseShader = Raylib.LoadShaderFromMemory(BuiltInShader.BASIC_VS, BuiltInShader.INV_FS);

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
		ShaderPropIndex_INV_TEXTURE = Raylib.GetShaderLocation(InverseShader, "screenTexture");
		ShaderPropIndex_INV_SCREEN_SIZE = Raylib.GetShaderLocation(InverseShader, "screenSize");
	}

	private void UpdateScreenEffect () {
		// Chromatic Aberration
		if (ScreenEffectEnables[Const.SCREEN_EFFECT_CHROMATIC_ABERRATION]) {
			var caShader = ScreenEffectShaders[Const.SCREEN_EFFECT_CHROMATIC_ABERRATION];
			const float CA_PingPongTime = 0.618f;
			const float CA_PingPongMin = 0f;
			const float CA_PingPongMax = 0.015f;
			if (Raylib.GetTime() % CA_PingPongTime > CA_PingPongTime / 2f) {
				Raylib.SetShaderValue(caShader, ShaderPropIndex_CA_RED_X, GetRandomAmount(0f), ShaderUniformDataType.Float);
				Raylib.SetShaderValue(caShader, ShaderPropIndex_CA_RED_Y, GetRandomAmount(0.2f), ShaderUniformDataType.Float);
				Raylib.SetShaderValue(caShader, ShaderPropIndex_CA_BLUE_X, GetRandomAmount(0.7f), ShaderUniformDataType.Float);
				Raylib.SetShaderValue(caShader, ShaderPropIndex_CA_BLUE_Y, GetRandomAmount(0.4f), ShaderUniformDataType.Float);
			} else {
				Raylib.SetShaderValue(caShader, ShaderPropIndex_CA_GREEN_X, GetRandomAmount(0f), ShaderUniformDataType.Float);
				Raylib.SetShaderValue(caShader, ShaderPropIndex_CA_GREEN_Y, GetRandomAmount(0.8f), ShaderUniformDataType.Float);
				Raylib.SetShaderValue(caShader, ShaderPropIndex_CA_BLUE_X, GetRandomAmount(0.4f), ShaderUniformDataType.Float);
				Raylib.SetShaderValue(caShader, ShaderPropIndex_CA_BLUE_Y, GetRandomAmount(0.72f), ShaderUniformDataType.Float);
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


	// Render
	protected override void _OnLayerUpdate (int layerIndex, bool isUiLayer, Cell[] cells, int cellCount) {

		if (PauselessFrame < 4) return;

		var cameraRect = Renderer.CameraRect;
		var screenRenderRect = Renderer.ScreenRenderRect;
		int cameraL = cameraRect.x;
		int cameraR = cameraRect.xMax;
		int cameraD = cameraRect.y;
		int cameraU = cameraRect.yMax;
		int screenL = screenRenderRect.x;
		int screenR = screenRenderRect.xMax;
		int screenD = screenRenderRect.y;
		int screenU = screenRenderRect.yMax;

		bool usingShader = false;

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
		Raylib.BeginBlendMode(layerIndex switch {
			RenderLayer.MULT => BlendMode.Multiplied,
			RenderLayer.ADD => BlendMode.Additive,
			RenderLayer.DEFAULT => BlendMode.CustomSeparate,
			_ => BlendMode.CustomSeparate,
		});

		for (int i = 0; i < cellCount; i++) {
			try {

				var cell = cells[isUiLayer ? cellCount - i - 1 : i];

				// Cell
				if (cell.Width == 0 || cell.Height == 0 || cell.Color.a == 0) continue;

				if (cell.Sprite != null) {

					// === Render as Artwork ===

					var sprite = cell.Sprite;
					if (!Renderer.TryGetTextureFromSheet<Texture2D>(sprite.ID, cell.SheetIndex, out var texture)) continue;

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
						cell.Width.Abs() * screenRenderRect.width / (float)cameraRect.width,
						cell.Height.Abs() * screenRenderRect.height / (float)cameraRect.height
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
						texture,
						source.ShrinkRectangle(0.001f),
						dest.ExpandRectangle(0.001f),
						new Vector2(
							pivotX * dest.Width,
							pivotY * dest.Height
						),
						cell.Rotation1000 / 1000f,
						cell.Color.ToRaylib()
					);

				} else if (cell.TextSprite != null) {

					// === Render as Char ===

					var cSprite = cell.TextSprite;

					var fontData = Fonts[cSprite.FontIndex];
					var textureObj = cSprite.Texture;
					if (textureObj is not Texture2D texture) continue;

					// Source
					var source = new Rectangle(0, 0, texture.Width, texture.Height);

					// Pos
					var dest = new Rectangle(
						Util.RemapUnclamped(cameraL, cameraR, screenL, screenR, (float)cell.X),
						Util.RemapUnclamped(cameraD, cameraU, screenU, screenD, (float)cell.Y),
						cell.Width * screenRenderRect.width / (float)cameraRect.width,
						cell.Height * screenRenderRect.height / (float)cameraRect.height
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
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}

		if (usingShader) Raylib.EndShaderMode();
		Raylib.EndBlendMode();

		// Func
		static void ShiftCell (Cell cell, ref Rectangle source, ref Rectangle dest, ref float pivotX, ref float pivotY, out bool skipCell) {

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

	}

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
	protected override object _GetTextureFromPixels (Color32[] pixels, int width, int height) => RayUtil.GetTextureFromPixels(pixels, width, height);

	protected override Color32[] _GetPixelsFromTexture (object texture) => RayUtil.GetPixelsFromTexture(texture);

	protected override void _FillPixelsIntoTexture (Color32[] pixels, object texture) => RayUtil.FillPixelsIntoTexture(pixels, texture);

	protected override Int2 _GetTextureSize (object texture) => RayUtil.GetTextureSize(texture);

	protected override object _PngBytesToTexture (byte[] bytes) => RayUtil.PngBytesToTexture(bytes);

	protected override byte[] _TextureToPngBytes (object texture) => RayUtil.TextureToPngBytes(texture);

	protected override void _UnloadTexture (object texture) => RayUtil.UnloadTexture(texture);

	protected override uint? _GetTextureID (object texture) => RayUtil.GetTextureID(texture);


	// GL Gizmos
	protected override void _DrawGizmosRect (IRect rect, Color32 color) {
		var cameraRect = Renderer.CameraRect;
		var screenRenderRect = Renderer.ScreenRenderRect;
		GizmosRender.AddGizmosRect(new Rectangle(
			Util.RemapUnclamped(cameraRect.x, cameraRect.xMax, screenRenderRect.x, screenRenderRect.xMax, rect.x),
			Util.RemapUnclamped(cameraRect.y, cameraRect.yMax, screenRenderRect.yMax, screenRenderRect.y, rect.yMax),
			rect.width * screenRenderRect.width / cameraRect.width,
			rect.height * screenRenderRect.height / cameraRect.height
		), color);
	}

	protected override void _DrawGizmosTexture (IRect rect, FRect uv, object texture, bool inverse) {
		if (texture is not Texture2D rTexture) return;
		var cameraRect = Renderer.CameraRect;
		var screenRenderRect = Renderer.ScreenRenderRect;
		GizmosRender.AddGizmosTexture(new Rectangle(
			Util.RemapUnclamped(cameraRect.x, cameraRect.xMax, screenRenderRect.x, screenRenderRect.xMax, (float)rect.x),
			Util.RemapUnclamped(cameraRect.y, cameraRect.yMax, screenRenderRect.yMax, screenRenderRect.y, (float)rect.yMax),
			(float)rect.width * screenRenderRect.width / cameraRect.width,
			(float)rect.height * screenRenderRect.height / cameraRect.height
		), new Rectangle(
			uv.x * rTexture.Width,
			uv.y * rTexture.Height,
			uv.width * rTexture.Width,
			uv.height * rTexture.Height
		), rTexture, inverse);
	}

	protected override void _IgnoreGizmos (int duration = 0) => GizmosRender.IgnoreGizmos(duration);

	protected override void _ForceRequireGizmosTexture (object texture) { }


	// Text
	protected override int _GetFontCount () => Fonts.Length;

	protected override bool _GetCharSprite (int fontIndex, char c, out CharSprite result) {
		var fontData = Fonts[fontIndex];
		result = null;
		if (!fontData.TryGetCharData(c, out var info, out var texture)) return true;
		bool fullset = c >= 256;
		float fontSize = fullset ?
			fontData.FullsetSize / fontData.FullsetScale :
			fontData.PrioritizedSize / fontData.PrioritizedScale;
		result = new CharSprite {
			Char = c,
			Advance = info.AdvanceX / fontSize,
			Offset = c == ' ' ? new FRect(0.5f, 0.5f, 0.001f, 0.001f) : FRect.MinMaxRect(
				xmin: info.OffsetX / fontSize,
				ymin: (fontSize - info.OffsetY - info.Image.Height) / fontSize,
				xmax: (info.OffsetX + info.Image.Width) / fontSize,
				ymax: (fontSize - info.OffsetY) / fontSize
			),
			FontIndex = fontIndex,
			Texture = texture,
		};
		return true;
	}

	protected override string _GetClipboardText () => Raylib.GetClipboardText_();

	protected override void _SetClipboardText (string text) => Raylib.SetClipboardText(text);


}