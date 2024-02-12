using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using AngeliaFramework;
using Raylib_cs;

namespace AngeliaForRaylib;

public partial class GameForRaylib {


	// SUB
	private class GLRect {
		public IRect Rect;
		public Color Color;
	}


	private class GLTexture {
		public IRect Rect;
		public Texture2D Texture;
		public FRect UV;
	}


	private class FontData {

		private readonly Dictionary<char, (Image image, Texture2D texture)> Pool = new();
		private unsafe byte* PrioritizedData = null;
		private unsafe byte* FullsetData = null;
		private int PrioritizedByteSize = 0;
		private int FullsetByteSize = 0;

		public string Name;
		public int LayerIndex;
		public int Size;
		public float Scale = 1f;

		public unsafe void LoadData (string filePath, bool isPrioritized) {
			uint fileSize = 0;
			byte* fileData = Raylib.LoadFileData(filePath, ref fileSize);
			if (isPrioritized) {
				PrioritizedData = fileData;
				PrioritizedByteSize = (int)fileSize;
			} else {
				FullsetData = fileData;
				FullsetByteSize = (int)fileSize;
			}
		}

		public unsafe bool TryGetCharData (char c, out GlyphInfo info, out Texture2D texture) {

			info = default;
			texture = default;

			if (PrioritizedByteSize == 0 && FullsetByteSize == 0) return false;

			bool usingFullset = FullsetByteSize != 0 && (int)c >= 256;
			var data = usingFullset ? FullsetData : PrioritizedData;
			int dataSize = usingFullset ? FullsetByteSize : PrioritizedByteSize;
			int charInt = c;
			var infoPtr = Raylib.LoadFontData(data, dataSize, Size, &charInt, 1, FontType.Default);
			if (infoPtr == null) return false;

			info = infoPtr[0];
			var img = info.Image;
			if (img.Width * img.Height != 0) {
				texture = Raylib.LoadTextureFromImage(img);
				Raylib.SetTextureFilter(texture, TextureFilter.Bilinear);
				Pool.TryAdd(c, (img, texture));
				return true;
			}
			return false;
		}

		public void Unload () {
			foreach (var (_, (image, texture)) in Pool) {
				Raylib.UnloadImage(image);
				Raylib.UnloadTexture(texture);
			}
		}

		public bool TryGetTexture (char c, out Texture2D texture) {
			if (Pool.TryGetValue(c, out var result)) {
				texture = result.texture;
				return true;
			} else {
				texture = default;
				return false;
			}
		}

	}


	// Data
	private Texture2D EMPTY_TEXTURE;
	private static System.Random CA_Ran = new(2353456);
	private readonly GLRect[] GLRects = new GLRect[256 * 256].FillWithNewValue();
	private readonly GLTexture[] GLTextures = new GLTexture[256].FillWithNewValue();
	private readonly Shader[] ScreenEffectShaders = new Shader[Const.SCREEN_EFFECT_COUNT];
	private readonly bool[] ScreenEffectEnables = new bool[Const.SCREEN_EFFECT_COUNT].FillWithValue(false);
	private FontData[] Fonts;
	private int GLRectCount = 0;
	private int GLTextureCount = 0;
	private FRect CameraScreenRect = new(0, 0, 1f, 1f);
	private IRect ScreenRect;
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
	private void InitializeFont () {
		string fontRoot = Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Fonts");
		var fontList = new List<FontData>(8);
		foreach (var fontPath in Util.EnumerateFiles(fontRoot, true, "*.ttf")) {
			string name = Util.GetNameWithoutExtension(fontPath);
			if (!Util.TryGetIntFromString(name, 0, out int layerIndex, out _)) continue;
			var targetData = fontList.Find(data => data.LayerIndex == layerIndex);
			if (targetData == null) {
				int hashIndex = name.IndexOf('#');
				fontList.Add(targetData = new FontData() {
					Name = (hashIndex >= 0 ? name[..hashIndex] : name).TrimStart_Numbers(),
					LayerIndex = layerIndex,
					Size = 42,
					Scale = 1f,
				});
			}
			// Size
			int sizeTagIndex = name.IndexOf("#size=", System.StringComparison.OrdinalIgnoreCase);
			if (sizeTagIndex >= 0 && Util.TryGetIntFromString(name, sizeTagIndex + 6, out int size, out _)) {
				targetData.Size = Util.Max(42, size);
			}
			// Scale
			int scaleTagIndex = name.IndexOf("#scale=", System.StringComparison.OrdinalIgnoreCase);
			if (scaleTagIndex >= 0 && Util.TryGetIntFromString(name, scaleTagIndex + 7, out int scale, out _)) {
				targetData.Scale = (scale / 100f).Clamp(0.01f, 10f);
			}
			// Data
			targetData.LoadData(
				fontPath, !name.Contains("#fullset", System.StringComparison.OrdinalIgnoreCase)
			);
		}
		fontList.Sort((a, b) => a.LayerIndex.CompareTo(b.LayerIndex));
		Fonts = fontList.ToArray();
	}

	private void InitializeShader () {
		string shaderRoot = Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Shaders");
		// Shaders
		string lerpShaderPath = Util.CombinePaths(shaderRoot, "Lerp.fs");
		if (Util.FileExists(lerpShaderPath)) LerpShader = Raylib.LoadShader(null, lerpShaderPath);
		string colorShaderPath = Util.CombinePaths(shaderRoot, "Color.fs");
		if (Util.FileExists(colorShaderPath)) ColorShader = Raylib.LoadShader(null, colorShaderPath);
		string textShaderPath = Util.CombinePaths(shaderRoot, "Text.fs");
		if (Util.FileExists(textShaderPath)) TextShader = Raylib.LoadShader(null, textShaderPath);
		// Effects
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			string path = Util.CombinePaths(shaderRoot, $"{Const.SCREEN_EFFECT_NAMES[i]}.fs");
			if (Util.FileExists(path)) {
				ScreenEffectShaders[i] = Raylib.LoadShader(null, path);
			}
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

	private void UpdateGizmos () {

		var cameraRect = CellRenderer.CameraRect;
		int cameraL = cameraRect.x;
		int cameraR = cameraRect.xMax;
		int cameraD = cameraRect.y;
		int cameraU = cameraRect.yMax;
		int screenL = ScreenRect.x;
		int screenR = ScreenRect.xMax;
		int screenD = ScreenRect.y;
		int screenU = ScreenRect.yMax;

		// Texture
		for (int i = 0; i < GLTextureCount; i++) {
			var glTexture = GLTextures[i];
			var rTexture = glTexture.Texture;
			var rect = glTexture.Rect;
			var uv = glTexture.UV;
			Raylib.DrawTexturePro(
				rTexture,
				new Rectangle(
					uv.x * rTexture.Width,
					uv.y * rTexture.Height,
					uv.width * rTexture.Width,
					uv.height * rTexture.Height
				).Shrink(0.1f), new Rectangle(
					Util.RemapUnclamped(cameraL, cameraR, screenL, screenR, rect.x),
					Util.RemapUnclamped(cameraD, cameraU, screenU, screenD, rect.yMax),
					rect.width * ScreenRect.width / cameraRect.width,
					rect.height * ScreenRect.height / cameraRect.height
				).Expand(0.5f), new(0, 0), 0, Color.White
			);
		}
		GLTextureCount = 0;

		// Rect
		for (int i = 0; i < GLRectCount; i++) {
			var glRect = GLRects[i];
			var rect = glRect.Rect;
			Raylib.DrawRectangle(
				Util.RemapUnclamped(cameraL, cameraR, screenL, screenR, rect.x),
				Util.RemapUnclamped(cameraD, cameraU, screenU, screenD, rect.yMax),
				rect.width * ScreenRect.width / cameraRect.width,
				rect.height * ScreenRect.height / cameraRect.height,
				glRect.Color
			);
		}
		GLRectCount = 0;
	}

	private bool PrepareScreenEffects () {
		bool hasScreenEffectEnabled = false;
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			if (ScreenEffectEnables[i]) {
				hasScreenEffectEnabled = true;
				int screenW = Raylib.GetScreenWidth();
				int screenH = Raylib.GetScreenHeight();
				if (RenderTexture.Texture.Width != screenW || RenderTexture.Texture.Height != screenH) {
					RenderTexture = Raylib.LoadRenderTexture(screenW, screenH);
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


	// Render
	protected override void _OnRenderingLayerCreated (int index, string name, int sortingOrder, int capacity) { }

	protected override void _OnCameraUpdate () {
		ScreenRect = CameraScreenRect.x.AlmostZero() ?
			new IRect(0, 0, ScreenWidth, ScreenHeight) :
			new IRect(
				Util.LerpUnclamped(0, ScreenWidth, CameraScreenRect.x).RoundToInt(), 0,
				(ScreenWidth * CameraScreenRect.width).RoundToInt(), ScreenHeight
			);
	}

	protected override void _OnLayerUpdate (int layerIndex, bool isUiLayer, bool isTextLayer, Cell[] cells, int cellCount) {
		if (isTextLayer) {
			UpdateLayer_Text(layerIndex, cells, cellCount);
		} else {
			UpdateLayer_Cell(layerIndex, isUiLayer, cells, cellCount);
		}
	}

	private void UpdateLayer_Cell (int layerIndex, bool isUiLayer, Cell[] cells, int cellCount) {

		var cameraRect = CellRenderer.CameraRect;
		int cameraL = cameraRect.x;
		int cameraR = cameraRect.xMax;
		int cameraD = cameraRect.y;
		int cameraU = cameraRect.yMax;
		int screenL = ScreenRect.x;
		int screenR = ScreenRect.xMax;
		int screenD = ScreenRect.y;
		int screenU = ScreenRect.yMax;

		bool usingShader = false;
		bool usingBlend = false;

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

				// Cell
				var sprite = cell.Sprite;
				if (sprite == null || cell.Width == 0 || cell.Height == 0 || cell.Color.a == 0) continue;

				// UV
				float sourceL, sourceR, sourceD, sourceU;
				if (cell.BorderSide == Alignment.Full) {
					sourceL = 0f;
					sourceR = sprite.PixelWidth;
					sourceD = 0f;
					sourceU = sprite.PixelHeight;
				} else {
					AngeUtil.GetSlicedUvBorder(sprite, cell.BorderSide, out var bl, out _, out _, out var tr);
					sourceL = bl.x * sprite.PixelWidth;
					sourceR = tr.x * sprite.PixelWidth;
					sourceD = sprite.PixelHeight - tr.y * sprite.PixelHeight;
					sourceU = sprite.PixelHeight - bl.y * sprite.PixelHeight;
				}
				var source = new Rectangle(
					sourceL,
					sourceD,
					sourceR - sourceL,
					sourceU - sourceD
				);

				// Pos
				var dest = new Rectangle(
					Util.RemapUnclamped(cameraL, cameraR, screenL, screenR, cell.X),
					Util.RemapUnclamped(cameraD, cameraU, screenU, screenD, cell.Y),
					cell.Width.Abs() * ScreenRect.width / (float)cameraRect.width,
					cell.Height.Abs() * ScreenRect.height / (float)cameraRect.height
				);

				float pivotX = cell.Width > 0 ? cell.PivotX : 1f - cell.PivotX;
				float pivotY = cell.Height > 0 ? 1f - cell.PivotY : cell.PivotY;

				// Shift
				ShiftCell(cell, ref source, ref dest, ref pivotX, ref pivotY, out bool skipCell);
				if (skipCell) continue;

				// Draw
				source = source.Shrink(0.1f);
				source.Width *= cell.Width.Sign();
				source.Height *= cell.Height.Sign();
				Raylib.DrawTexturePro(
					(Texture2D)sprite.Texture, source, dest.Expand(0.5f),
					new(
						pivotX * dest.Width,
						pivotY * dest.Height
					), cell.Rotation, cell.Color.ToRaylib()
				);

			} catch (System.Exception ex) { LogException(ex); }
		}

		if (usingShader) Raylib.EndShaderMode();
		if (usingBlend) Raylib.EndBlendMode();

	}

	private void UpdateLayer_Text (int layerIndex, Cell[] cells, int cellCount) {

		var cameraRect = CellRenderer.CameraRect;
		int cameraL = cameraRect.x;
		int cameraR = cameraRect.xMax;
		int cameraD = cameraRect.y;
		int cameraU = cameraRect.yMax;
		int screenL = ScreenRect.x;
		int screenR = ScreenRect.xMax;
		int screenD = ScreenRect.y;
		int screenU = ScreenRect.yMax;

		bool usingShader = false;

		if (TextShader.Id != 0) {
			Raylib.BeginShaderMode(TextShader);
			usingShader = true;
		}

		for (int i = cellCount - 1; i >= 0; i--) {
			try {

				var cell = cells[i];
				var sprite = cell.TextSprite;

				if (sprite == null || cell.Width == 0 || cell.Height == 0 || cell.Color.a == 0) continue;

				var fontData = Fonts[layerIndex];
				if (!fontData.TryGetTexture((char)sprite.GlobalID, out var texture)) continue;

				// Source
				var source = new Rectangle(0, 0, texture.Width, texture.Height);

				// Pos
				var dest = new Rectangle(
					Util.RemapUnclamped(cameraL, cameraR, screenL, screenR, cell.X),
					Util.RemapUnclamped(cameraD, cameraU, screenU, screenD, cell.Y),
					cell.Width.Abs() * ScreenRect.width / (float)cameraRect.width,
					cell.Height.Abs() * ScreenRect.height / (float)cameraRect.height
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
					texture, source, dest,
					new(
						pivotX * dest.Width,
						pivotY * dest.Height
					), cell.Rotation, cell.Color.ToRaylib()
				);

			} catch (System.Exception ex) {
				LogException(ex);
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

	protected override void _SetSkyboxTint (Byte4 top, Byte4 bottom) { }


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

	protected override void _Effect_SetTintParams (Byte4 color) {
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
	protected override object _GetTextureFromPixels (Byte4[] pixels, int width, int height) {
		int len = width * height;
		if (len == 0) return EMPTY_TEXTURE;
		unsafe {
			Texture2D textureResult;
			var image = new Image() {
				Format = PixelFormat.UncompressedR8G8B8A8,
				Width = width,
				Height = height,
				Mipmaps = 1,
			};
			if (pixels != null && pixels.Length == len) {
				var bytes = new byte[pixels.Length * 4];
				int index = 0;
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						int i = (height - y - 1) * width + x;
						var p = pixels[i];
						bytes[index * 4 + 0] = p.r;
						bytes[index * 4 + 1] = p.g;
						bytes[index * 4 + 2] = p.b;
						bytes[index * 4 + 3] = p.a;
						index++;
					}
				}
				fixed (void* data = bytes) {
					image.Data = data;
					textureResult = Raylib.LoadTextureFromImage(image);
				}
			} else {
				textureResult = Raylib.LoadTextureFromImage(image);
			}
			Raylib.SetTextureFilter(textureResult, TextureFilter.Point);
			return textureResult;
		}
	}

	protected override Byte4[] _GetPixelsFromTexture (object texture) {
		if (texture is not Texture2D rTexture) return System.Array.Empty<Byte4>();
		var image = Raylib.LoadImageFromTexture(rTexture);
		unsafe {
			int width = image.Width;
			int height = image.Height;
			var result = new Byte4[width * height];
			var colors = Raylib.LoadImageColors(image);
			int index = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					int i = (height - y - 1) * width + x;
					result[index] = colors[i].ToAngelia();
					index++;
				}
			}
			Raylib.UnloadImageColors(colors);
			return result;
		}
	}

	protected override void _FillPixelsIntoTexture (Byte4[] pixels, object texture) {
		if (pixels == null || texture is not Texture2D rTexture) return;
		int width = rTexture.Width;
		int height = rTexture.Height;
		if (pixels.Length != width * height) return;
		var colors = new Color[pixels.Length];
		int index = 0;
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				int i = (height - y - 1) * width + x;
				colors[index] = pixels[i].ToRaylib();
				index++;
			}
		}
		Raylib.UpdateTexture(rTexture, colors);
	}

	protected override Int2 _GetTextureSize (object texture) => texture is Texture2D rTexture ? new Int2(rTexture.Width, rTexture.Height) : default;

	protected override object _PngBytesToTexture (byte[] bytes) {
		if (bytes == null || bytes.Length == 0) return EMPTY_TEXTURE;
		var image = Raylib.LoadImageFromMemory(".png", bytes);
		var result = Raylib.LoadTextureFromImage(image);
		Raylib.SetTextureFilter(result, TextureFilter.Point);
		return result;
	}

	protected override byte[] _TextureToPngBytes (object texture) {
		if (texture is not Texture2D rTexture) return System.Array.Empty<byte>();
		unsafe {
			var fileType = Marshal.StringToHGlobalAnsi(".png");
			int fileSize = 0;
			char* result = Raylib.ExportImageToMemory(
				Raylib.LoadImageFromTexture(rTexture),
				(sbyte*)fileType.ToPointer(),
				&fileSize
			);
			if (fileSize == 0) return System.Array.Empty<byte>();
			var resultBytes = new byte[fileSize];
			Marshal.Copy((nint)result, resultBytes, 0, fileSize);
			Marshal.FreeHGlobal((System.IntPtr)result);
			Marshal.FreeHGlobal(fileType);
			return resultBytes;
		}
	}

	protected override void _UnloadTexture (object texture) {
		if (texture is not Texture2D rTexture) return;
		Raylib.UnloadTexture(rTexture);
	}


	// GL Gizmos
	protected override void _DrawGizmosRect (IRect rect, Byte4 color) {
		if (GLRectCount >= GLRects.Length) return;
		var glRect = GLRects[GLRectCount];
		glRect.Rect = rect;
		glRect.Color = color.ToRaylib();
		GLRectCount++;
	}

	protected override void _DrawGizmosTexture (IRect rect, FRect uv, object texture) {
		if (texture is not Texture2D rTexture) return;
		if (GLTextureCount >= GLTextures.Length) return;
		var glTexture = GLTextures[GLTextureCount];
		glTexture.Rect = rect;
		glTexture.Texture = rTexture;
		glTexture.UV = uv;
		GLTextureCount++;
	}


	// Text
	protected override void _OnTextLayerCreated (int index, string name, int sortingOrder, int capacity) { }

	protected override int _GetTextLayerCount () => Fonts.Length;

	protected override string _GetTextLayerName (int index) => Fonts[index].Name;

	protected override int _GetFontSize (int index) => Fonts[index].Size;

	protected override CharSprite _FillCharSprite (int layerIndex, char c, int textSize, CharSprite charSprite, out bool filled) {

		var fontData = Fonts[layerIndex];
		if (!fontData.TryGetCharData(c, out var info, out var texture)) {
			filled = false;
			return charSprite;
		}

		float fontSize = fontData.Size / fontData.Scale;
		charSprite ??= new();
		charSprite.GlobalID = c;
		charSprite.Advance = info.AdvanceX / fontSize;
		charSprite.Offset = c == ' ' ? new FRect(0.5f, 0.5f, 0.001f, 0.001f) : FRect.MinMaxRect(
			xmin: info.OffsetX / fontSize,
			ymin: (fontSize - info.OffsetY - info.Image.Height) / fontSize,
			xmax: (info.OffsetX + info.Image.Width) / fontSize,
			ymax: (fontSize - info.OffsetY) / fontSize
		);
		charSprite.Rebuild = 0;

		filled = true;
		return charSprite;
	}

	protected override void _RequestStringForFont (int layerIndex, int textSize, char[] content) { }

	protected override void _RequestStringForFont (int layerIndex, int textSize, string content) { }

	protected override string _GetClipboardText () => Raylib.GetClipboardText_();

	protected override void _SetClipboardText (string text) => Raylib.SetClipboardText(text);

	protected override void _SetImeCompositionMode (bool on) {

	}


	// UTL
	private static void WritePixelsToConsole (Byte4[] pixels, int width) {

		int height = pixels.Length / width;
		int realWidth = Util.Min(width, 32);
		int realHeight = height * realWidth / width;
		int scale = width / realWidth;

		for (int y = realHeight - 1; y >= 0; y--) {
			System.Console.ResetColor();
			System.Console.WriteLine();
			for (int x = 0; x < realWidth; x++) {
				var p = pixels[(y * scale).Clamp(0, height - 1) * width + (x * scale).Clamp(0, width - 1)];
				Util.RGBToHSV(p, out float h, out float s, out float v);
				System.Console.BackgroundColor = (v * s < 0.2f) ?
					(v < 0.33f ? System.ConsoleColor.Black : v > 0.66f ? System.ConsoleColor.White : System.ConsoleColor.Gray) :
					(h < 0.08f ? (v > 0.5f ? System.ConsoleColor.Red : System.ConsoleColor.DarkRed) :
					h < 0.25f ? (v > 0.5f ? System.ConsoleColor.Yellow : System.ConsoleColor.DarkYellow) :
					h < 0.42f ? (v > 0.5f ? System.ConsoleColor.Green : System.ConsoleColor.DarkGreen) :
					h < 0.58f ? (v > 0.5f ? System.ConsoleColor.Cyan : System.ConsoleColor.DarkCyan) :
					h < 0.75f ? (v > 0.5f ? System.ConsoleColor.Blue : System.ConsoleColor.DarkBlue) :
					h < 0.92f ? (v > 0.5f ? System.ConsoleColor.Magenta : System.ConsoleColor.DarkMagenta) :
					(v > 0.6f ? System.ConsoleColor.Red : System.ConsoleColor.DarkRed));
				System.Console.Write(" ");
			}
		}
		System.Console.ResetColor();
		System.Console.WriteLine();
	}


}