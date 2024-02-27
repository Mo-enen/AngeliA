using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using AngeliA;
using Raylib_cs;
using KeyboardKey = Raylib_cs.KeyboardKey;

namespace AngeliaToRaylib;

public static class RaylibUtil {


	public static bool IsTyping => TypingTextFieldID != 0;
	public static readonly Dictionary<int, Texture2D> TexturePool = new();
	public static readonly Dictionary<char, CharSprite> TextPool = new();
	public static readonly StringBuilder TypingBuilder = new();

	private static readonly bool[] DEFAULT_PART_IGNORE = new bool[9] { false, false, false, false, false, false, false, false, false, };
	private static readonly Cell[] CacheTextCells = new Cell[1024].FillWithNewValue();
	private static readonly Cell[] CacheNineSliceCells = new Cell[9].FillWithNewValue();
	private static int NineSliceCellCount = 0;
	private static int TextCellCount = 0;
	private static FontData CacheFont = null;
	private static Shader? TextShader;
	private static bool TexturePoolInitialized = false;
	private const int MAX_INPUT_CHAR = 256;
	private static readonly TextContent InputLabel = new() {
		Alignment = Alignment.MidLeft,
		Clip = true,
		Wrap = false,
	};
	private static int TypingTextFieldID = 0;
	private static int BeamIndex = 0;
	private static int BeamLength = 0;
	private static Vector2 LastInputMouseDownPos = default;


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

	public static void DrawLabel (this FontData font, TextContent content, Rectangle rect) => DrawLabel(font, content, rect, -1, 0, false, out _, out _, out _);
	public static void DrawLabel (this FontData font, TextContent content, Rectangle rect, out Rectangle bounds) => DrawLabel(font, content, rect, -1, 0, false, out bounds, out _, out _);
	public static void DrawLabel (this FontData font, TextContent content, Rectangle rect, int beamIndex, int startIndex, bool drawInvisibleChar, out Rectangle bounds, out Rectangle beamRect, out int endIndex) {
		DrawLabelIntoCacheCells(
			font, content, rect, beamIndex, startIndex, drawInvisibleChar,
			out bounds, out beamRect, out endIndex
		);
		DrawLabelFromCacheCells(font);
		TextCellCount = 0;
	}


	private static void DrawLabelIntoCacheCells (
		FontData font, TextContent content, Rectangle rect,
		int beamIndex, int startIndex, bool drawInvisibleChar,
		out Rectangle bounds, out Rectangle beamRect, out int endIndex
	) {
		bounds = default;
		beamRect = default;
		endIndex = beamIndex;
		if (font == null) return;
		CacheFont = font;
		TextCellCount = 0;
		int windowHeight = Raylib.GetRenderHeight();
		TextUtil.DrawLabel(
			RequireCharSpriteHander, DrawCharHandler, windowHeight, TextCellCount, CacheTextCells,
			content, new IRect(rect.X.RoundToInt(), windowHeight - rect.Y.RoundToInt() - rect.Height.RoundToInt(), rect.Width.RoundToInt(), rect.Height.RoundToInt()), beamIndex, startIndex, drawInvisibleChar,
			out var iBounds, out var iBeamRect, out endIndex
		);
		beamRect = iBeamRect.ToRaylib();
		bounds = iBounds.ToRaylib();
		CacheFont = null;
	}


	private static void DrawLabelFromCacheCells (FontData font) {
		if (font == null) return;
		if (!TextShader.HasValue) {
			TextShader = Raylib.LoadShaderFromMemory(BuiltInShader.BASIC_VS, BuiltInShader.TEXT_FS);
		}
		Raylib.BeginShaderMode(TextShader.Value);
		int screenHeight = Raylib.GetRenderHeight();
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
			} catch (Exception ex) { Util.LogException(ex); }
		}
		Raylib.EndShaderMode();
	}


	// Render
	public static void Draw (this Sheet sheet, int id, Rectangle rect) => Draw(sheet, id, (int)rect.X, (int)rect.Y, 0, 0, 0, (int)rect.Width, (int)rect.Height, Color32.WHITE);
	public static void Draw (this Sheet sheet, int id, Rectangle rect, Color32 color) => Draw(sheet, id, (int)rect.X, (int)rect.Y, 0, 0, 0, (int)rect.Width, (int)rect.Height, color);
	public static void Draw (this Sheet sheet, int id, int x, int y, int rotation, float pivotX, float pivotY, int width, int height) => Draw(sheet, id, x, y, rotation, pivotX, pivotY, width, height, Color32.WHITE);
	public static void Draw (this Sheet sheet, int id, int x, int y, int rotation, float pivotX, float pivotY, int width, int height, Color32 color, Alignment borderSide = Alignment.Full) {

		if (!sheet.SpritePool.TryGetValue(id, out var sprite)) return;
		if (!TexturePoolInitialized) {
			TexturePoolInitialized = true;
			TextureUtil.FillSheetIntoTexturePool(sheet, TexturePool);
		}
		if (!TexturePool.TryGetValue(id, out var texture)) return;

		width =
			width == Const.ORIGINAL_SIZE ? sprite.GlobalWidth :
			width == Const.ORIGINAL_SIZE_NEGATAVE ? -sprite.GlobalWidth : width;
		height =
			height == Const.ORIGINAL_SIZE ? sprite.GlobalHeight :
			height == Const.ORIGINAL_SIZE_NEGATAVE ? -sprite.GlobalHeight : height;

		float sourceL, sourceR, sourceD, sourceU;
		if (borderSide == Alignment.Full) {
			sourceL = 0f;
			sourceR = sprite.PixelWidth;
			sourceD = 0f;
			sourceU = sprite.PixelHeight;
		} else {
			borderSide = borderSide switch {
				Alignment.TopLeft => Alignment.BottomLeft,
				Alignment.TopMid => Alignment.BottomMid,
				Alignment.TopRight => Alignment.BottomRight,
				Alignment.BottomLeft => Alignment.TopLeft,
				Alignment.BottomMid => Alignment.TopMid,
				Alignment.BottomRight => Alignment.TopRight,
				_ => borderSide,
			};
			Util.GetSlicedUvBorder(sprite, borderSide, out var bl, out _, out _, out var tr);
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
		var dest = new Rectangle(x, y, width.Abs(), height.Abs());
		pivotX = width > 0 ? pivotX : 1f - pivotX;
		pivotY = height > 0 ? pivotY : 1f - pivotY;

		// Draw
		source.Width *= width.Sign();
		source.Height *= height.Sign();
		Raylib.DrawTexturePro(
			texture, source.Shrink(0.002f), dest.Expand(0.05f),
			new Vector2(
				pivotX * dest.Width,
				pivotY * dest.Height
			), rotation, color.ToRaylib()
		);

	}


	public static void Draw_9Slice (this Sheet sheet, int id, Rectangle rect) => Draw_9Slice(sheet, id, (int)rect.X, (int)rect.Y, 0, 0, 0, (int)rect.Width, (int)rect.Height, -1, -1, -1, -1, Color.White);
	public static void Draw_9Slice (this Sheet sheet, int id, Rectangle rect, Color color) => Draw_9Slice(sheet, id, (int)rect.X, (int)rect.Y, 0, 0, 0, (int)rect.Width, (int)rect.Height, -1, -1, -1, -1, color);
	public static void Draw_9Slice (this Sheet sheet, int id, int x, int y, float pivotX, float pivotY, int rotation, int width, int height) => Draw_9Slice(sheet, id, x, y, pivotX, pivotY, rotation, width, height, -1, -1, -1, -1, Color.White);
	public static void Draw_9Slice (this Sheet sheet, int id, int x, int y, float pivotX, float pivotY, int rotation, int width, int height, Color color) => Draw_9Slice(sheet, id, x, y, pivotX, pivotY, rotation, width, height, -1, -1, -1, -1, color);
	public static void Draw_9Slice (this Sheet sheet, int id, int x, int y, float pivotX, float pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU) => Draw_9Slice(sheet, id, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, Color.White);
	public static void Draw_9Slice (this Sheet sheet, int id, int x, int y, float pivotX, float pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Color color) {

		if (!sheet.SpritePool.TryGetValue(id, out var sprite)) return;
		NineSliceCellCount = 0;

		// 9-Slice
		int screenHeight = Raylib.GetRenderHeight();
		var cells = Util.NineSlice(
			DrawHandler, sprite, x, y,
			(int)(pivotX * 1000), (int)(pivotY * 1000),
			rotation, width, height,
			GetUnifyBorder(borderL >= 0 ? borderL : sprite.GlobalBorder.left, screenHeight),
			GetUnifyBorder(borderR >= 0 ? borderR : sprite.GlobalBorder.right, screenHeight),
			GetUnifyBorder(borderU >= 0 ? borderU : sprite.GlobalBorder.up, screenHeight),
			GetUnifyBorder(borderD >= 0 ? borderD : sprite.GlobalBorder.down, screenHeight),
			DEFAULT_PART_IGNORE, color.ToAngelia(), z: 0
		);

		// Draw
		foreach (var cell in cells) {
			Draw(
				sheet, id, cell.X, cell.Y, cell.Rotation, cell.PivotX, cell.PivotY,
				cell.Width, cell.Height, cell.Color, cell.BorderSide
			);
		}
	}

	public static int GetUnifyBorder (int spriteBorder, int screenHeight) => spriteBorder * screenHeight / 7000;


	// Text
	public static void CancelTyping () {
		TypingTextFieldID = 0;
		TypingBuilder.Clear();
		BeamIndex = 0;
		BeamLength = 0;
	}


	public static string TextField (FontData font, Sheet sheet, int controlID, Rectangle rect, string _text, out bool changed, out bool confirm) {

		InputLabel.SetText(_text);
		changed = false;
		confirm = false;
		int screenHeight = Raylib.GetRenderHeight();
		var cameraRect = new Rectangle(0, 0, Raylib.GetRenderWidth(), screenHeight);
		var mousePos = Raylib.GetMousePosition();
		bool mouseLeftHolding = Raylib.IsMouseButtonDown(MouseButton.Left);
		bool mouseLeftDown = Raylib.IsMouseButtonPressed(MouseButton.Left);
		bool startTyping = false;
		if (mouseLeftDown) LastInputMouseDownPos = mousePos;
		bool mouseDownPosInRect = rect.Contains(LastInputMouseDownPos);
		bool mouseDragging = mouseLeftHolding && mouseDownPosInRect;
		bool inCamera = rect.Overlaps(cameraRect);
		bool mouseInside = rect.Contains(mousePos);

		if (mouseLeftDown && !mouseInside && TypingTextFieldID == controlID) {
			CancelTyping();
		}

		if (mouseInside) {
			Raylib.SetMouseCursor(MouseCursor.IBeam);
		}

		if (!inCamera && TypingTextFieldID == controlID) TypingTextFieldID = 0;

		// Start Typing
		if (inCamera && mouseLeftDown && mouseDownPosInRect) {
			TypingTextFieldID = controlID;
			startTyping = true;
			mouseDragging = false;
		}

		// Typing 
		bool typing = TypingTextFieldID == controlID;
		int beamIndex = typing ? BeamIndex : 0;
		int beamLength = typing ? BeamLength : 0;
		if (typing) {

			// Clear
			if (Raylib.IsKeyReleased(KeyboardKey.Escape)) {
				beamIndex = BeamIndex = 0;
				confirm = true;
				CancelTyping();
			}

			// Move Beam
			if (IsKeyPressedOrRepeat(KeyboardKey.Left)) {
				if (beamLength == 0) {
					beamIndex = BeamIndex = beamIndex - 1;
				} else if (beamLength < 0) {
					beamIndex = BeamIndex = beamIndex + beamLength;
				}
				beamLength = BeamLength = 0;
			}
			if (IsKeyPressedOrRepeat(KeyboardKey.Right)) {
				if (beamLength == 0) {
					beamIndex = BeamIndex = beamIndex + 1;
				} else if (beamLength > 0) {
					beamIndex = BeamIndex = beamIndex + beamLength;
				}
				beamLength = BeamLength = 0;
			}

			beamIndex = BeamIndex = beamIndex.Clamp(0, InputLabel.Text.Length);
			beamLength = BeamLength = beamLength.Clamp(-beamIndex, InputLabel.Text.Length - beamIndex);

			for (int i = 0; i < TypingBuilder.Length; i++) {
				char c = TypingBuilder[i];
				switch (c) {
					case Const.BACKSPACE_SIGN:
						// Backspace
						if (beamLength == 0) {
							int removeIndex = beamIndex - 1;
							if (removeIndex >= 0 && removeIndex < InputLabel.Text.Length) {
								InputLabel.Text = InputLabel.Text.Remove(removeIndex, 1);
								beamIndex = BeamIndex = beamIndex - 1;
								changed = true;
							}
						} else {
							RemoveSelection();
							changed = true;
						}
						break;
					case Const.RETURN_SIGN:
						// Enter
						confirm = true;
						CancelTyping();
						break;
					case Const.CONTROL_COPY:
					case Const.CONTROL_CUT:
						if (beamLength == 0) break;
						int beamStart = Util.Min(beamIndex, beamIndex + beamLength);
						int beamEnd = Util.Max(beamIndex, beamIndex + beamLength);
						Raylib.SetClipboardText(InputLabel.Text[beamStart..beamEnd]);
						if (c == Const.CONTROL_CUT) {
							RemoveSelection();
							changed = true;
						}
						break;
					case Const.CONTROL_PASTE:
						string clipboardText = Raylib.GetClipboardText_();
						if (string.IsNullOrEmpty(clipboardText)) break;
						if (beamLength != 0) RemoveSelection();
						InputLabel.Text = InputLabel.Text.Insert(beamIndex, clipboardText);
						beamIndex = BeamIndex = beamIndex + clipboardText.Length;
						changed = true;
						break;
					default:
						if (InputLabel.Text.Length >= MAX_INPUT_CHAR) break;
						// Append Char
						if (beamLength != 0) RemoveSelection();
						InputLabel.Text = InputLabel.Text.Insert(beamIndex, c.ToString());
						beamIndex = BeamIndex = beamIndex + 1;
						changed = true;
						break;
				}
			}

			// Delete
			if (IsKeyPressedOrRepeat(KeyboardKey.Delete)) {
				int removeIndex = beamIndex;
				if (removeIndex >= 0 && removeIndex < InputLabel.Text.Length) {
					if (beamLength == 0) {
						// Delete One Char
						InputLabel.Text = InputLabel.Text.Remove(removeIndex, 1);
						changed = true;
					} else {
						// Delete Selection
						RemoveSelection();
						changed = true;
					}
				}
			}
			// Func
			void RemoveSelection () {
				int newBeamIndex = Util.Min(beamIndex, beamIndex + beamLength);
				InputLabel.Text = InputLabel.Text.Remove(newBeamIndex, beamLength.Abs());
				beamIndex = BeamIndex = newBeamIndex;
				beamLength = BeamLength = 0;
			}
		}

		// Rendering
		var labelRect = rect.Shrink(Unify(12, screenHeight), 0, 0, 0);
		float beamShrink = rect.Height / 12;
		var beamRect = new Rectangle(
			labelRect.X, labelRect.Y + beamShrink, Unify(2, screenHeight), labelRect.Height - beamShrink * 2
		);

		// Draw Text
		if (!string.IsNullOrEmpty(InputLabel.Text)) {
			DrawLabelIntoCacheCells(
				font, InputLabel, rect, beamIndex, 0, false,
				out _, out beamRect, out _
			);
		}

		float beamOffsetX = 0;
		var cells = CacheTextCells;
		if (TextCellCount != 0) {

			// Scroll X from Beam 
			int beamCellIndex = typing ? beamIndex.Clamp(0, TextCellCount - 1) : 0;
			var beamCharCell = cells[beamCellIndex];

			// Shift for Beam Out
			float shiftX = 0;
			float labelRight = labelRect.X + labelRect.Width - Unify(22f, screenHeight);
			if (beamCharCell.X + beamCharCell.Width / 2 >= labelRight) {
				shiftX = labelRight - beamCharCell.X;
				beamOffsetX = shiftX;
			}

			// Clip
			for (int i = 0; i < TextCellCount && i < TextCellCount; i++) {
				var cell = cells[i];
				cell.X += (int)shiftX;
				// Set Tint for Outside Clip
				if (
					cell.X + cell.Width > labelRect.X + labelRect.Width ||
					cell.X < labelRect.X
				) {
					if (cell.X > labelRect.X + labelRect.Width || cell.X + cell.Width < labelRect.X) {
						cell.Color = Color32.CLEAR;
					}
				}
			}

			// Get Beam Selection Rect
			if (!startTyping && typing && beamLength != 0) {
				int beamSelectionStartIndex = Util.Min(beamIndex, beamIndex + beamLength);
				int beamSelectionEndIndex = Util.Max(beamIndex, beamIndex + beamLength);
				var startCell = cells[(beamSelectionStartIndex).Clamp(0, TextCellCount - 1)];
				var endCell = cells[(beamSelectionEndIndex - 1).Clamp(0, TextCellCount - 1)];
				var selectionRect = IRect.MinMaxRect(
					(int)Util.Max(startCell.X, rect.X),
					(int)Util.Max(labelRect.Y + beamShrink, rect.Y),
					(int)Util.Min(endCell.X + endCell.Width, rect.X + rect.Width),
					(int)Util.Min(labelRect.Y + labelRect.Height - beamShrink, rect.Y + rect.Height)
				).ToRaylib();
				Draw(sheet, Const.PIXEL, selectionRect, Color32.ORANGE);
			}

			if (typing && (startTyping || mouseDragging)) {
				int mouseBeamIndex = 0;
				int mouseX = (int)mousePos.X;
				for (int i = 0; i < TextCellCount && i < TextCellCount; i++) {
					var cell = cells[i];
					int x = cell.X + cell.Width / 2;
					mouseBeamIndex = i;
					// End Check
					if (i == TextCellCount - 1 && mouseX > x) {
						mouseBeamIndex = InputLabel.Text.Length;
					}
					if (x > mouseX) break;
				}
				// Set Beam on Click
				if (startTyping) {
					beamIndex = BeamIndex = mouseBeamIndex;
					beamLength = BeamLength = 0;
				}
				// Set Selection on Drag
				if (mouseDragging) {
					BeamLength = beamLength + beamIndex - mouseBeamIndex;
					BeamIndex = mouseBeamIndex;
				}
			}
		}

		// Beam
		if (!startTyping && typing) {
			beamRect.X += beamOffsetX;
			beamRect.Y = labelRect.Y + beamShrink;
			beamRect.Height = labelRect.Height - beamShrink * 2;
			Draw(sheet, Const.PIXEL, beamRect);
		}

		// Draw Text
		DrawLabelFromCacheCells(font);
		TextCellCount = 0;

		// Clamp
		if (InputLabel.Text.Length > MAX_INPUT_CHAR) {
			InputLabel.Text = InputLabel.Text[..MAX_INPUT_CHAR];
		}

		return InputLabel.Text;
	}


	// Misc
	public static int Unify (int value, int screenHeight) => (value * screenHeight / 1000f).RoundToInt();
	public static int Unify (float value, float screenHeight) => (value * screenHeight / 1000f).RoundToInt();


	public static bool IsKeyPressedOrRepeat (KeyboardKey key) => Raylib.IsKeyPressed(key) || Raylib.IsKeyPressedRepeat(key);


	// Debug
	public static void WritePixelsToConsole (Color32[] pixels, int width) {

		int height = pixels.Length / width;

		for (int y = height - 1; y >= 0; y--) {
			System.Console.ResetColor();
			System.Console.WriteLine();
			for (int x = 0; x < width; x++) {
				var p = pixels[(y).Clamp(0, height - 1) * width + (x).Clamp(0, width - 1)];
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

	public static void Log (object msg) {
		Console.ResetColor();
		Console.WriteLine(msg);
	}

	public static void LogWarning (object msg) {
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine(msg);
		Console.ResetColor();
	}

	public static void LogError (object msg) {
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(msg);
		Console.ResetColor();
	}

	public static void LogException (Exception ex) {
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(ex.Source);
		Console.WriteLine(ex.GetType().Name);
		Console.WriteLine(ex.Message);
		Console.WriteLine();
		Console.ResetColor();
	}


	// LGC
	private static bool RequireCharSpriteHander (char c, out CharSprite sprite) {
		if (TextPool.TryGetValue(c, out var textSprite)) {
			// Get Exists
			sprite = textSprite;
		} else {
			// Require Char from Font
			sprite = CreateCharSprite(CacheFont, c);
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

	private static Cell DrawHandler (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z) {
		var cell = CacheNineSliceCells[NineSliceCellCount];
		NineSliceCellCount++;
		cell.X = x;
		cell.Y = y;
		cell.Rotation = rotation;
		cell.Width = width;
		cell.Height = height;
		cell.Color = color;
		return cell;
	}


}