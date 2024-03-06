using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;


namespace AngeliA;


public class TextUtil {


	public delegate bool RequireCharSpriteHander (char c, out CharSprite sprite);
	public delegate Cell DrawCharHandler (CharSprite sprite, int x, int y, int width, int height, Color32 color);


	public static bool IsLineBreakingChar (char c) =>
		char.IsWhiteSpace(c) || char.GetUnicodeCategory(c) switch {
			UnicodeCategory.DecimalDigitNumber => false,
			UnicodeCategory.LowercaseLetter => false,
			UnicodeCategory.LetterNumber => false,
			UnicodeCategory.UppercaseLetter => false,
			UnicodeCategory.MathSymbol => false,
			UnicodeCategory.TitlecaseLetter => false,
			_ => true,
		};


	public static void DrawLabel (
		RequireCharSpriteHander requireCharSprite, DrawCharHandler drawChar,
		int unifyHeight, int textCountInLayer, Cell[] textCells,
		TextContent content, IRect rect, int beamIndex, int startIndex, bool drawInvisibleChar,
		out IRect bounds, out IRect beamRect, out int endIndex
	) {

		endIndex = startIndex;
		bounds = rect;
		beamRect = new IRect(rect.x, rect.y, 1, rect.height);
		if (string.IsNullOrEmpty(content.Text)) content.Text = string.Empty;

		string text = content.Text;
		char[] chars = content.Chars;
		int count = content.FromString ? text.Length : chars.Length;

		int charSize = content.CharSize < 0 ? rect.height / 2 : Unify(content.CharSize, unifyHeight);
		int lineSpace = Unify(content.LineSpace, unifyHeight);
		var color = content.Tint;
		int charSpace = Unify(content.CharSpace, unifyHeight);
		var alignment = content.Alignment;
		var wrap = content.Wrap;
		bool clip = content.Clip;
		bool beamEnd = beamIndex >= count;
		requireCharSprite(' ', out var emptyCharSprite);

		// Content
		int maxLineCount = ((float)rect.height / (charSize + lineSpace)).FloorToInt();
		int line = 0;
		int x = rect.x;
		int y = rect.yMax - charSize;
		int startCellIndex = textCountInLayer;
		int nextWrapCheckIndex = 0;
		bool firstCharAtLine = true;
		int minX = int.MaxValue;
		int minY = int.MaxValue;
		int maxX = int.MinValue;
		int maxY = int.MinValue;
		int shadowOffset = Unify(content.ShadowOffset, unifyHeight);
		for (int i = startIndex; i < count; i++) {

			char c = content.FromString ? text[i] : chars[i];
			endIndex = i;
			if (c == '\r') goto CONTINUE;
			if (c == '\0') break;

			// Line
			if (c == '\n') {
				x = rect.x;
				y -= charSize + lineSpace;
				firstCharAtLine = true;
				line++;
				if (clip && line >= maxLineCount) break;
				goto CONTINUE;
			}

			// Require Char
			if (!requireCharSprite(c, out var sprite)) goto CONTINUE;

			int realCharSize = (sprite.Advance * charSize).RoundToInt();

			// Wrap Check for Word
			if (wrap == WrapMode.WordWrap && i >= nextWrapCheckIndex && !IsLineBreakingChar(c)) {
				if (!WordEnoughToFit(
					requireCharSprite, content, charSize, charSpace, i,
					rect.xMax - x - realCharSize, out int wordLength
				) && !firstCharAtLine) {
					x = rect.x;
					y -= charSize + lineSpace;
					line++;
					firstCharAtLine = true;
					if (clip && line >= maxLineCount) break;
				}
				nextWrapCheckIndex += wordLength - 1;
			}

			// Draw Char
			if (wrap != WrapMode.NoWrap && x > rect.xMax - realCharSize) {
				x = rect.x;
				y -= charSize + lineSpace;
				line++;
				if (char.IsWhiteSpace(c)) goto CONTINUE;
				if (clip && line >= maxLineCount) break;
			}
			var cell = drawChar(sprite, x, y, charSize, charSize, color) ?? Cell.EMPTY;
			if (cell != null && cell.TextSprite != null) textCountInLayer++;
			if (content.Shadow.a > 0 && shadowOffset != 0) {
				var shadowCell = drawChar(sprite, 0, 0, 1, 1, Color32.WHITE);
				if (shadowCell != null) {
					if (cell != null && cell.TextSprite != null) textCountInLayer++;
					shadowCell.CopyFrom(cell);
					shadowCell.Color = content.Shadow;
					shadowCell.Y -= shadowOffset;
					shadowCell.Z--;
				}
			}

			// Beam
			if (!beamEnd && beamIndex >= 0 && i >= beamIndex) {
				beamRect.x = x;
				beamRect.y = y;
				beamRect.width = Unify(2, unifyHeight);
				beamRect.height = charSize;
				beamIndex = -1;
			}
			if (beamEnd && beamIndex >= 0 && i >= count - 1) {
				beamRect.x = x + realCharSize + charSpace;
				beamRect.y = y;
				beamRect.width = Unify(2, unifyHeight);
				beamRect.height = charSize;
				beamIndex = -1;
			}

			// Next
			x += realCharSize + charSpace;
			minX = Util.Min(minX, cell.X);
			minY = Util.Min(minY, cell.Y);
			maxX = Util.Max(maxX, cell.X + cell.Width);
			maxY = Util.Max(maxY, cell.Y + cell.Height);
			firstCharAtLine = false;

			continue;

			CONTINUE:;
			if (drawInvisibleChar) {
				int cellCount = textCountInLayer.Clamp(0, textCells.Length) - startCellIndex - 1;
				int textCount = i - startIndex;
				int addCount = textCount - cellCount;
				for (int add = 0; add < addCount; add++) {
					var _cell = drawChar(emptyCharSprite, 0, 0, 0, 0, color);
					if (_cell != null && _cell.TextSprite != null) textCountInLayer++;
				}
			}

		}

		// Alignment
		if (count > 0) {
			int offsetX;
			int offsetY;
			int textSizeX = maxX - minX;
			int textSizeY = maxY - minY;
			offsetX = alignment switch {
				Alignment.TopRight or Alignment.MidRight or Alignment.BottomRight =>
					rect.xMax - maxX,
				Alignment.TopMid or Alignment.MidMid or Alignment.BottomMid =>
					rect.xMax - maxX - ((rect.width - textSizeX) / 2),
				_ =>
					rect.xMin - minX,
			};
			offsetY = alignment switch {
				Alignment.BottomLeft or Alignment.BottomMid or Alignment.BottomRight =>
					rect.yMin - minY,
				Alignment.MidLeft or Alignment.MidMid or Alignment.MidRight =>
					rect.yMax - maxY - ((rect.height - textSizeY) / 2),
				_ =>
					rect.yMax - maxY,
			};

			// Offset
			int cellCount = textCountInLayer.Clamp(0, textCells.Length);
			for (int i = startCellIndex; i < cellCount; i++) {
				var cell = textCells[i];
				cell.X += offsetX;
				cell.Y += offsetY;
			}
			beamRect.x += offsetX;
			beamRect.y += offsetY;

			// BG Size
			bounds.x = minX + offsetX;
			bounds.y = minY + offsetY;
			bounds.width = maxX - minX;
			bounds.height = maxY - minY;

			// Clip Cells
			if (clip) {
				Util.ClampCells(textCells, bounds, startCellIndex, textCountInLayer);
			}
		}

	}


	// LGC
	private static bool WordEnoughToFit (RequireCharSpriteHander requireCharSprite, TextContent content, int charSize, int charSpace, int startIndex, int room, out int wordLength) {
		int index = startIndex;
		int count = content.FromString ? content.Text.Length : content.Chars.Length;
		for (; index < count; index++) {
			char c = content.FromString ? content.Text[index] : content.Chars[index];
			if (IsLineBreakingChar(c)) break;
			if (!requireCharSprite(c, out var sprite)) continue;
			if (room > 0) {
				room -= (sprite.Advance * charSize).RoundToInt() + charSpace;
			}
		}
		wordLength = index - startIndex + 1;
		return room >= 0;
	}


	private static int Unify (int value, int cameraHeight) => (value * cameraHeight / 1000f).RoundToInt();


}