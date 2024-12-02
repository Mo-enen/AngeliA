using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using AngeliA;
using Raylib_cs;

namespace AngeliaRaylib;

public partial class RayGame {



	// Data
	private readonly static Random CA_Ran = new(2353456);
	private readonly Shader[] ScreenEffectShaders = new Shader[Const.SCREEN_EFFECT_COUNT];
	private readonly bool[] ScreenEffectEnables = new bool[Const.SCREEN_EFFECT_COUNT].FillWithValue(false);
	private Shader LerpShader;
	private Shader ColorShader;
	private Shader InverseShader;
	private RenderTexture2D AlphaLayerTexture;
	private RenderTexture2D RenderTexture;
	private RenderTexture2D GizmosRenderTexture;
	private RenderTexture2D DoodleRenderTexture;
	private int ShaderPropIndex_DarkenAmount;
	private int ShaderPropIndex_LightenAmount;
	private int ShaderPropIndex_TintAmount;
	private int ShaderPropIndex_VignetteRadius;
	private int ShaderPropIndex_VignetteFeather;
	private int ShaderPropIndex_VignetteOffsetX;
	private int ShaderPropIndex_VignetteOffsetY;
	private int ShaderPropIndex_VignetteRound;
	private int ShaderPropIndex_VignetteAspect;
	private int ShaderPropIndex_INV_TEXTURE;
	private int ShaderPropIndex_INV_SCREEN_SIZE;
	private Float2 DoodleRenderingOffset;
	private float DoodleRenderingZoom;


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
		ShaderPropIndex_INV_TEXTURE = Raylib.GetShaderLocation(InverseShader, "screenTexture");
		ShaderPropIndex_INV_SCREEN_SIZE = Raylib.GetShaderLocation(InverseShader, "screenSize");
	}

	private void UpdateScreenEffect () {
		// Vig
		if (ScreenEffectEnables[Const.SCREEN_EFFECT_VIGNETTE]) {
			var vShader = ScreenEffectShaders[Const.SCREEN_EFFECT_VIGNETTE];
			Raylib.SetShaderValue(
				vShader, ShaderPropIndex_VignetteAspect,
				(float)ScreenWidth / ScreenHeight,
				ShaderUniformDataType.Float
			);
		}
	}


	// Render
	protected override void _BeforeAllLayersUpdate () {

		Raylib.BeginTextureMode(RenderTexture);

		// Sky
		var skyColorBottom = Sky.SkyTintBottomColor;
		var skyColorTop = Sky.SkyTintTopColor;
		if (skyColorBottom != skyColorTop) {
			Raylib.DrawRectangleGradientV(
				0, 0, ScreenWidth, ScreenHeight,
				 skyColorTop.ToRaylib(), skyColorBottom.ToRaylib()
			);
		} else {
			Raylib.ClearBackground(skyColorBottom.ToRaylib());
		}

	}

	protected override void _AfterAllLayersUpdate () { }

	protected override void _OnLayerUpdate (int layerIndex, Cell[] cells, int cellCount) {

		if (PauselessFrame < 4) return;

		// Apply Doodle Before UI
		if (GlobalFrame <= DoodleFrame && GlobalFrame > DoodleOnTopOfUiFrame && layerIndex == RenderLayer.UI) {
			Raylib.BeginBlendMode(BlendMode.CustomSeparate);
			DrawDoodleTexture();
			Raylib.EndBlendMode();
		}

		// Apply Gizmos Before UI
		if (GlobalFrame > GizmosOnTopOfUiFrame && layerIndex == RenderLayer.UI) {
			Raylib.BeginBlendMode(BlendMode.CustomSeparate);
			Raylib.DrawTextureRec(
				GizmosRenderTexture.Texture,
				new Rectangle(0, 0, GizmosRenderTexture.Texture.Width, -GizmosRenderTexture.Texture.Height),
				new Vector2(0, 0), Color.White
			);
			Raylib.EndBlendMode();
		}

		// Apply Prev Pixels for Transparent Layer
		var layerTint = Renderer.GetLayerTint(layerIndex);
		byte layerAlpha = layerTint.a;
		layerTint.a = 255;
		bool tinted = layerTint != Color32.WHITE;
		if (layerAlpha == 0) return;
		if (layerAlpha < 255) {
			Raylib.EndTextureMode();
			Raylib.BeginTextureMode(AlphaLayerTexture);
			Raylib.ClearBackground(Color.Blank);
		}

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
		bool isUiLayer = layerIndex == RenderLayer.UI;

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
			_ => BlendMode.CustomSeparate,
		});

		var span = new ReadOnlySpan<Cell>(cells);
		for (int i = 0; i < cellCount; i++) {
			try {

				var cell = span[isUiLayer ? cellCount - i - 1 : i];

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
						dest,
						new Vector2(
							pivotX * dest.Width,
							pivotY * dest.Height
						),
						cell.Rotation1000 / 1000f,
						tinted ? (cell.Color * layerTint).ToRaylib() : cell.Color.ToRaylib()
					);

				} else if (cell.TextSprite != null) {

					// === Render as Char ===

					var cSprite = cell.TextSprite;
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
						),
						rotation: 0,
						tinted ? (cell.Color * layerTint).ToRaylib() : cell.Color.ToRaylib()
					);
				}
			} catch (Exception ex) { Debug.LogException(ex); }
		}

		if (usingShader) Raylib.EndShaderMode();
		Raylib.EndBlendMode();

		// Apply for Transparent Layer
		if (layerAlpha < 255) {
			Debug.Log(Game.GlobalFrame);
			Raylib.EndTextureMode();
			Raylib.BeginTextureMode(RenderTexture);
			Raylib.DrawTextureRec(
				AlphaLayerTexture.Texture,
				new Rectangle(0, 0, AlphaLayerTexture.Texture.Width, -AlphaLayerTexture.Texture.Height),
				default,
				new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, layerAlpha)
			);
		}

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
		Raylib.SetShaderValue(
			shader, ShaderPropIndex_DarkenAmount,
			(amount * step).RoundToInt() / step,
			ShaderUniformDataType.Float
		);
	}

	protected override void _Effect_SetLightenParams (float amount, float step = 8f) {
		var shader = ScreenEffectShaders[Const.SCREEN_EFFECT_RETRO_LIGHTEN];
		Raylib.SetShaderValue(
			shader, ShaderPropIndex_LightenAmount,
			(amount * step).RoundToInt() / step,
			ShaderUniformDataType.Float
		);
	}

	protected override void _Effect_SetTintParams (Color32 color) {
		var shader = ScreenEffectShaders[Const.SCREEN_EFFECT_TINT];
		Raylib.SetShaderValue(
			shader,
			ShaderPropIndex_TintAmount,
			new Vector4(color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f),
			ShaderUniformDataType.Vec4
		);
	}

	protected override void _Effect_SetVignetteParams (float radius, float feather, float offsetX, float offsetY, float round) {
		var shader = ScreenEffectShaders[Const.SCREEN_EFFECT_VIGNETTE];
		Raylib.SetShaderValue(shader, ShaderPropIndex_VignetteRadius, radius, ShaderUniformDataType.Float);
		Raylib.SetShaderValue(shader, ShaderPropIndex_VignetteFeather, feather, ShaderUniformDataType.Float);
		Raylib.SetShaderValue(shader, ShaderPropIndex_VignetteOffsetX, offsetX, ShaderUniformDataType.Float);
		Raylib.SetShaderValue(shader, ShaderPropIndex_VignetteOffsetY, offsetY, ShaderUniformDataType.Float);
		Raylib.SetShaderValue(shader, ShaderPropIndex_VignetteRound, round, ShaderUniformDataType.Float);
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

	protected override bool _IsTextureReady (object texture) => RayUtil.IsTextureReady(texture);

	protected override object _GetResizedTexture (object texture, int newWidth, int newHeight) => RayUtil.GetResizedTexture(texture, newWidth, newHeight);


	// Gizmos
	protected override void _DrawGizmosRect (IRect rect, Color32 color) {
		if (PauselessFrame <= IgnoreGizmosFrame) return;
		if (CurrentAltTextureMode != AltTextureMode.Gizmos) {
			SwitchToGizmosTextureMode();
		}
		var cameraRect = Renderer.CameraRect;
		var screenRenderRect = Renderer.ScreenRenderRect;
		float minX = Util.RemapUnclamped(cameraRect.x, cameraRect.xMax, screenRenderRect.x, screenRenderRect.xMax, (float)rect.x);
		float maxX = Util.RemapUnclamped(cameraRect.x, cameraRect.xMax, screenRenderRect.x, screenRenderRect.xMax, (float)rect.xMax);
		float minY = Util.RemapUnclamped(cameraRect.y, cameraRect.yMax, screenRenderRect.yMax, screenRenderRect.y, (float)rect.yMax);
		float maxY = Util.RemapUnclamped(cameraRect.y, cameraRect.yMax, screenRenderRect.yMax, screenRenderRect.y, (float)rect.y);
		var gizmosRect = new Rectangle(minX, minY, maxX - minX, maxY - minY);
		Raylib.DrawRectangleRec(gizmosRect, color.ToRaylib());
	}

	protected override void _DrawGizmosRect (IRect rect, Color32 colorT, Color32 colorB) {
		if (PauselessFrame <= IgnoreGizmosFrame) return;
		if (CurrentAltTextureMode != AltTextureMode.Gizmos) {
			SwitchToGizmosTextureMode();
		}
		var cameraRect = Renderer.CameraRect;
		var screenRenderRect = Renderer.ScreenRenderRect;
		float minX = Util.RemapUnclamped(cameraRect.x, cameraRect.xMax, screenRenderRect.x, screenRenderRect.xMax, (float)rect.x);
		float maxX = Util.RemapUnclamped(cameraRect.x, cameraRect.xMax, screenRenderRect.x, screenRenderRect.xMax, (float)rect.xMax);
		float minY = Util.RemapUnclamped(cameraRect.y, cameraRect.yMax, screenRenderRect.yMax, screenRenderRect.y, (float)rect.yMax);
		float maxY = Util.RemapUnclamped(cameraRect.y, cameraRect.yMax, screenRenderRect.yMax, screenRenderRect.y, (float)rect.y);
		var gizmosRect = new Rectangle(minX, minY, maxX - minX, maxY - minY);
		Raylib.DrawRectangleGradientEx(
			gizmosRect,
			colorT.ToRaylib(), colorB.ToRaylib(), colorB.ToRaylib(), colorT.ToRaylib()
		);
	}

	protected override void _DrawGizmosRect (IRect rect, Color32 colorTL, Color32 colorTR, Color32 colorBL, Color32 colorBR) {
		if (PauselessFrame <= IgnoreGizmosFrame) return;
		if (CurrentAltTextureMode != AltTextureMode.Gizmos) {
			SwitchToGizmosTextureMode();
		}
		var cameraRect = Renderer.CameraRect;
		var screenRenderRect = Renderer.ScreenRenderRect;
		float minX = Util.RemapUnclamped(cameraRect.x, cameraRect.xMax, screenRenderRect.x, screenRenderRect.xMax, (float)rect.x);
		float maxX = Util.RemapUnclamped(cameraRect.x, cameraRect.xMax, screenRenderRect.x, screenRenderRect.xMax, (float)rect.xMax);
		float minY = Util.RemapUnclamped(cameraRect.y, cameraRect.yMax, screenRenderRect.yMax, screenRenderRect.y, (float)rect.yMax);
		float maxY = Util.RemapUnclamped(cameraRect.y, cameraRect.yMax, screenRenderRect.yMax, screenRenderRect.y, (float)rect.y);
		var bl = new Vector2(minX, minY);
		var br = new Vector2(maxX, minY);
		var tl = new Vector2(minX, maxY);
		var tr = new Vector2(maxX, maxY);
		var mm = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
		var cTL = colorBL.ToRaylib();
		var cTR = colorBR.ToRaylib();
		var cBL = colorTL.ToRaylib();
		var cBR = colorTR.ToRaylib();
		var cMM = new Color(
			(byte)((colorTL.r + colorTR.r + colorBL.r + colorBR.r) / 4),
			(byte)((colorTL.g + colorTR.g + colorBL.g + colorBR.g) / 4),
			(byte)((colorTL.b + colorTR.b + colorBL.b + colorBR.b) / 4),
			(byte)((colorTL.a + colorTR.a + colorBL.a + colorBR.a) / 4)
		);
		Raylib.DrawTriangle3Colors(bl, tl, mm, cBL, cTL, cMM);
		Raylib.DrawTriangle3Colors(tr, br, mm, cTR, cBR, cMM);
		Raylib.DrawTriangle3Colors(br, bl, mm, cBR, cBL, cMM);
		Raylib.DrawTriangle3Colors(tl, tr, mm, cTL, cTR, cMM);
	}

	protected override void _DrawGizmosLine (int startX, int startY, int endX, int endY, int thickness, Color32 color) {
		if (PauselessFrame <= IgnoreGizmosFrame) return;
		if (CurrentAltTextureMode != AltTextureMode.Gizmos) {
			SwitchToGizmosTextureMode();
		}
		var cameraRect = Renderer.CameraRect;
		var screenRenderRect = Renderer.ScreenRenderRect;
		Raylib.DrawLineEx(
			new Vector2(
				Util.RemapUnclamped(cameraRect.x, cameraRect.xMax, screenRenderRect.x, screenRenderRect.xMax, (float)startX),
				Util.RemapUnclamped(cameraRect.y, cameraRect.yMax, screenRenderRect.yMax, screenRenderRect.y, (float)startY)
			),
			new Vector2(
				Util.RemapUnclamped(cameraRect.x, cameraRect.xMax, screenRenderRect.x, screenRenderRect.xMax, (float)endX),
				Util.RemapUnclamped(cameraRect.y, cameraRect.yMax, screenRenderRect.yMax, screenRenderRect.y, (float)endY)
			),
			Util.RemapUnclamped(0, cameraRect.height, 0, screenRenderRect.height, (float)thickness),
			color.ToRaylib()
		);
	}

	protected override void _DrawGizmosTexture (IRect rect, FRect uv, object texture, Color32 tint, bool inverse) {
		if (PauselessFrame <= IgnoreGizmosFrame) return;
		if (texture is not Texture2D rTexture) return;
		if (CurrentAltTextureMode != AltTextureMode.Gizmos) {
			SwitchToGizmosTextureMode();
		}
		var cameraRect = Renderer.CameraRect;
		var screenRenderRect = Renderer.ScreenRenderRect;
		var gizmosRect = new Rectangle(
			Util.RemapUnclamped(cameraRect.x, cameraRect.xMax, screenRenderRect.x, screenRenderRect.xMax, (float)rect.x),
			Util.RemapUnclamped(cameraRect.y, cameraRect.yMax, screenRenderRect.yMax, screenRenderRect.y, (float)rect.yMax),
			(float)rect.width * screenRenderRect.width / cameraRect.width,
			(float)rect.height * screenRenderRect.height / cameraRect.height
		);
		var gizmosUV = new Rectangle(
			uv.x * rTexture.Width,
			uv.y * rTexture.Height,
			uv.width * rTexture.Width,
			uv.height * rTexture.Height
		);
		// Draw
		float yMin = rTexture.Height - (gizmosUV.Y + gizmosUV.Height);
		float yMax = rTexture.Height - gizmosUV.Y;
		gizmosUV.Y = yMin;
		gizmosUV.Height = yMax - yMin;
		if (inverse) {
			Raylib.BeginShaderMode(InverseShader);
			Raylib.SetShaderValueTexture(
				InverseShader, ShaderPropIndex_INV_TEXTURE, RenderTexture.Texture
			);
			Raylib.SetShaderValue(
				InverseShader, ShaderPropIndex_INV_SCREEN_SIZE,
				new Vector2(ScreenWidth, ScreenHeight), ShaderUniformDataType.Vec2
			);
		}
		Raylib.DrawTexturePro(
			rTexture,
			gizmosUV.ShrinkRectangle(0.001f),
			gizmosRect.ExpandRectangle(0.001f),
			new(0, 0), 0, tint.ToRaylib()
		);
		if (inverse) {
			Raylib.EndShaderMode();
		}
	}

	protected override void _IgnoreGizmos (int duration = 0) => IgnoreGizmosFrame = PauselessFrame + duration;


	// Doodle
	protected override void _ResetDoodle () {
		if (CurrentAltTextureMode != AltTextureMode.Doodle) {
			SwitchToDoodleTextureMode();
		}
		Raylib.ClearBackground(Color.Blank);
	}

	protected override void _SetDoodleOffset (Float2 screenOffset) => DoodleRenderingOffset = screenOffset;

	protected override void _SetDoodleZoom (float zoom) => DoodleRenderingZoom = zoom;

	protected override void _DoodleRect (FRect screenRect, Color32 color) {
		if (CurrentAltTextureMode != AltTextureMode.Doodle) {
			SwitchToDoodleTextureMode();
		}
		Raylib.DrawRectangleRec(screenRect.ToRaylib(), color.ToRaylib());
	}

	protected override void _DoodleWorld (IBlockSquad squad, FRect screenRect, IRect worldUnitRange, int z, bool ignoreLevel = false, bool ignoreBG = false, bool ignoreEntity = false, bool ignoreElement = true) {
		if (CurrentAltTextureMode != AltTextureMode.Doodle) {
			SwitchToDoodleTextureMode();
		}
		if (ignoreLevel && ignoreBG && ignoreEntity && ignoreElement) return;

		float pixW = screenRect.width / worldUnitRange.width;
		float pixH = screenRect.height / worldUnitRange.height;
		int unitRangeL = worldUnitRange.xMin;
		int unitRangeR = worldUnitRange.xMax;
		int unitRangeD = worldUnitRange.yMin;
		int unitRangeU = worldUnitRange.yMax;
		float screenL = screenRect.xMin;
		float screenR = screenRect.xMax;
		float screenD = screenRect.yMin;
		float screenU = screenRect.yMax;
		var rect = new Rectangle(0, 0, pixW, pixH);
		int doodleScreenWidth = ScreenWidth - DoodleScreenPadding.horizontal;
		int doodleScreenHeight = ScreenHeight - DoodleScreenPadding.vertical;

		var worldPoses = FrameworkUtil.ForAllExistsWorldInRange(squad, worldUnitRange, z, out int count);
		for (int worldIndex = 0; worldIndex < count; worldIndex++) {
			var worldPos = worldPoses[worldIndex];
			int unitL = Util.Max(worldPos.x * Const.MAP, unitRangeL);
			int unitR = Util.Min((worldPos.x + 1) * Const.MAP, unitRangeR);
			int unitD = Util.Max(worldPos.y * Const.MAP, unitRangeD);
			int unitU = Util.Min((worldPos.y + 1) * Const.MAP, unitRangeU);
			for (int j = unitD; j < unitU; j++) {
				rect.Y = Util.RemapUnclamped(unitRangeD, unitRangeU, screenD, screenU, j).UMod(doodleScreenHeight);
				for (int i = unitL; i < unitR; i++) {
					var (lv, bg, en, el) = squad.GetAllBlocksAt(i, j, z);
					int id =
						!ignoreEntity && en != 0 ? en :
						!ignoreElement && el != 0 ? el :
						!ignoreLevel && lv != 0 ? lv :
						!ignoreBG && bg != 0 ? bg : 0;
					if (id == 0) continue;
					if (!Renderer.TryGetSpriteForGizmos(id, out var sp)) continue;
					rect.X = Util.RemapUnclamped(unitRangeL, unitRangeR, screenL, screenR, i).UMod(doodleScreenWidth);
					Raylib.DrawRectangleRec(rect, sp.SummaryTint.ToRaylib());
				}
			}
		}

	}


	// Text
	protected override int _GetFontCount () => Fonts.Count;

	protected override bool _GetCharSprite (int fontIndex, char c, out CharSprite result) {
		result = null;
		if (fontIndex < 0 || fontIndex >= Fonts.Count) return false;
		bool got = Fonts[fontIndex].TryGetCharSprite(c, out result);
		if (result != null) {
			result.FontIndex = fontIndex;
		}
		return got;
	}

	protected override string _GetClipboardText () => Raylib.GetClipboardText_();

	protected override void _SetClipboardText (string text) => Raylib.SetClipboardText(text);

	protected override FontData CreateNewFontData () => new RayFontData();


}