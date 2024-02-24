using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using AngeliA;
using Raylib_cs;

namespace AngeliaToRaylib;

public static class RaylibUtil {


	public static readonly Dictionary<int, Texture2D> TexturePool = new();
	public static readonly Dictionary<char, CharSprite> TextPool = new();
	private static readonly Cell[] CacheTextCells = new Cell[1024].FillWithNewValue();
	private static int TextCellCount = 0;
	private static FontData CacheFont = null;
	private static Shader? TextShader;
	private static bool TextPoolInitialized = false;

	// Text
	public static FontData[] LoadFontDataFromFile (string fontRoot) {
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
		return fontList.ToArray();
	}

	public static CharSprite CreateCharSprite (FontData fontData, char c) {
		if (!fontData.TryGetCharData(c, out var info, out _)) return null;
		float fontSize = fontData.Size / fontData.Scale;
		return new CharSprite {
			Char = c,
			Advance = info.AdvanceX / fontSize,
			Offset = c == ' ' ? new FRect(0.5f, 0.5f, 0.001f, 0.001f) : FRect.MinMaxRect(
				xmin: info.OffsetX / fontSize,
				ymin: (fontSize - info.OffsetY - info.Image.Height) / fontSize,
				xmax: (info.OffsetX + info.Image.Width) / fontSize,
				ymax: (fontSize - info.OffsetY) / fontSize
			)
		};
	}

	public static void DrawLabel (this FontData font, CellContent content, Rectangle rect) => DrawLabel(font, content, rect, -1, 0, false, out _, out _, out _);
	public static void DrawLabel (this FontData font, CellContent content, Rectangle rect, out Rectangle bounds) => DrawLabel(font, content, rect, -1, 0, false, out bounds, out _, out _);
	public static void DrawLabel (this FontData font, CellContent content, Rectangle rect, int beamIndex, int startIndex, bool drawInvisibleChar, out Rectangle bounds, out Rectangle beamRect, out int endIndex) {
		bounds = default;
		beamRect = default;
		endIndex = beamIndex;
		if (font == null) return;

		// Fill Cells
		CacheFont = font;
		TextCellCount = 0;
		int windowHeight = Raylib.GetRenderHeight();
		TextUtil.DrawLabel(
			RequireCharSpriteHander, DrawCharHandler, windowHeight, TextCellCount, CacheTextCells,
			content, new IRect(rect.X.RoundToInt(), windowHeight - rect.Y.RoundToInt() - rect.Height.RoundToInt(), rect.Width.RoundToInt(), rect.Height.RoundToInt()), beamIndex, startIndex, drawInvisibleChar,
			out var iBounds, out var iBeamRect, out endIndex
		);
		CacheFont = null;

		int screenHeight = Raylib.GetRenderHeight();

		// Draw Cells
		if (!TextShader.HasValue) {
			TextShader = Raylib.LoadShaderFromMemory(BuiltInShader.BASIC_VS, BuiltInShader.TEXT_FS);
		}
		Raylib.BeginShaderMode(TextShader.Value);
		for (int i = 0; i < TextCellCount; i++) {
			try {
				var cell = CacheTextCells[i];

				if (cell.TextSprite == null || !font.TryGetTexture(cell.TextSprite.Char, out var texture)) continue;

				// Source
				var source = new Rectangle(0, 0, texture.Width, texture.Height);

				// Pos
				var dest = new Rectangle(cell.X, screenHeight - cell.Y, cell.Width, cell.Height);

				// Draw
				Raylib.DrawTexturePro(
					texture, source.Shrink(0.02f), dest,
					new Vector2(0, dest.Height),
					rotation: 0, cell.Color.ToRaylib()
				);
			} catch (System.Exception ex) { Util.LogException(ex); }
		}
		Raylib.EndShaderMode();
		TextCellCount = 0;
	}


	// Render
	public static void Draw (this Sheet sheet, int id, Rectangle rect) => Draw(sheet, id, (int)rect.X, (int)rect.Y, 0, 0, 0, (int)rect.Width, (int)rect.Height, Color32.WHITE);
	public static void Draw (this Sheet sheet, int id, Rectangle rect, Color32 color) => Draw(sheet, id, (int)rect.X, (int)rect.Y, 0, 0, 0, (int)rect.Width, (int)rect.Height, color);
	public static void Draw (this Sheet sheet, int id, int x, int y, int rotation, float pivotX, float pivotY, int width, int height) => Draw(sheet, id, x, y, rotation, pivotX, pivotY, width, height, Color32.WHITE);
	public static void Draw (this Sheet sheet, int id, int x, int y, int rotation, float pivotX, float pivotY, int width, int height, Color32 color) {
		
		if (!sheet.SpritePool.TryGetValue(id, out var sprite)) return;
		if (!TextPoolInitialized) {
			TextPoolInitialized = true;
			TextureUtil.FillSheetIntoTexturePool(sheet, TexturePool);
		}
		if (!TexturePool.TryGetValue(id, out var texture)) return;

		width = width == Const.ORIGINAL_SIZE ? sprite.GlobalWidth : width;
		height = height == Const.ORIGINAL_SIZE ? sprite.GlobalHeight : height;

		float sourceL, sourceR, sourceD, sourceU;
		sourceL = 0f;
		sourceR = sprite.PixelWidth;
		sourceD = 0f;
		sourceU = sprite.PixelHeight;
		var source = new Rectangle(
			sourceL,
			sourceD,
			sourceR - sourceL,
			sourceU - sourceD
		);

		// Pos
		var dest = new Rectangle(x, y, width.Abs(), height.Abs());

		pivotX = width > 0 ? pivotX : 1f - pivotX;
		pivotY = height > 0 ? pivotY : 1f - pivotY;

		// Draw
		source.Width *= width.Sign();
		source.Height *= height.Sign();
		Raylib.DrawTexturePro(
			texture, source.Shrink(0.02f), dest.Expand(0.5f),
			new Vector2(
				pivotX * dest.Width,
				pivotY * dest.Height
			), rotation, color.ToRaylib()
		);

	}


	// LGC
	private static bool RequireCharSpriteHander (char c, out CharSprite sprite) {
		if (TextPool.TryGetValue(c, out var textSprite)) {
			// Get Exists
			sprite = textSprite;
		} else {
			// Require Char from Font
			sprite = RaylibUtil.CreateCharSprite(CacheFont, c);
			TextPool.Add(c, sprite);
		}
		return sprite != null;
	}

	private static Cell DrawCharHandler (CharSprite sprite, int x, int y, int width, int height, Color32 color) {
		if (TextCellCount >= CacheTextCells.Length || sprite == null) return null;
		var cell = CacheTextCells[TextCellCount];
		var uvOffset = sprite.Offset;

		cell.Sprite = null;
		cell.TextSprite = sprite;
		cell.Z = 0;
		cell.Order = TextCellCount;
		cell.Rotation = 0;
		cell.PivotX = 0;
		cell.PivotY = 0;
		cell.Color = color;
		cell.BorderSide = Alignment.Full;
		cell.Shift = Int4.zero;
		cell.X = x + (int)(width * uvOffset.x);
		cell.Y = y + (int)(height * uvOffset.y);
		cell.Width = (int)(width * uvOffset.width);
		cell.Height = (int)(height * uvOffset.height);

		TextCellCount++;
		return cell;
	}


}