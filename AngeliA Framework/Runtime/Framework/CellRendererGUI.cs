using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;


namespace AngeliaFramework {


	public class CellContent {

		public static readonly CellContent Empty = new();
		private static readonly CellContent Temp = new();

		public string Text;
		public string ColorText;
		//public int Image;
		public Color32 Tint;
		public Color32 BackgroundTint;
		public Alignment Alignment;
		public int CharSize;
		public int CharSpace;
		public int LineSpace;
		public bool Wrap;
		public bool Clip;
		public bool TightBackground;


		public CellContent (string text = "") {
			Text = text;
			ColorText = "";
			Tint = Const.WHITE;
			BackgroundTint = Const.CLEAR;
			Alignment = Alignment.MidMid;
			CharSize = 24;
			CharSpace = 0;
			LineSpace = 5;
			Wrap = false;
			Clip = false;
			TightBackground = true;
		}


		public CellContent SetText (string newText) {
			Text = newText;
			return this;
		}


		public CellContent SetText (string newText, int charSize) {
			Text = newText;
			CharSize = charSize;
			return this;
		}


		public static CellContent Get (string text, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) {
			Temp.CharSize = charSize;
			Temp.Text = text;
			Temp.Alignment = alignment;
			Temp.Tint = Const.WHITE;
			Temp.Wrap = wrap;
			return Temp;
		}


		public static CellContent Get (string text, Color32 tint, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) {
			Temp.CharSize = charSize;
			Temp.Text = text;
			Temp.Alignment = alignment;
			Temp.Tint = tint;
			Temp.Wrap = wrap;
			return Temp;
		}


	}


	public static class CellRendererGUI {




		#region --- VAR ---


		// Const
		private static readonly Cell EMPTY_CELL = new() { Index = -1 };
		private static readonly string[] NUMBER_CACHE = new string[100];
		private const int MAX_INPUT_CHAR = 256;

		// Api
		public static bool IsTyping => TypingTextFieldID != 0;
		public static int TypingTextFieldID { get; private set; } = 0;

		// UI
		private static readonly CellContent InputLabel = new() {
			Alignment = Alignment.MidLeft,
			Clip = true,
			Wrap = false,
		};

		// Data
		private static readonly StringBuilder TypingBuilder = new();
		private static RectInt TypingTextFieldRect = default;
		private static int TypingUpdateFrame = int.MinValue;
		private static int BeamIndex = 0;
		private static int BeamLength = 0;
		private static int BeamBlinkFrame = int.MinValue;
		private static int PauselessFrame = 0;
		private static Vector2Int? ScrollBarMouseDownPos = null;


		#endregion




		#region --- API ---


		[OnGameInitialize(-128)]
		public static void Initialize () {
			InputSystem.onDeviceChange -= OnDeviceChange;
			InputSystem.onDeviceChange += OnDeviceChange;
			if (Keyboard.current != null) {
				Keyboard.current.onTextInput -= OnTextInput;
				Keyboard.current.onTextInput += OnTextInput;
			}
			for (int i = 0; i < NUMBER_CACHE.Length; i++) {
				NUMBER_CACHE[i] = i.ToString();
			}
		}


		internal static void Update (int pauselessFrame) {
			PauselessFrame = pauselessFrame;
			if (TypingTextFieldID != 0 && FrameInput.AnyKeyHolding) {
				FrameInput.UseAllHoldingKeys(ignoreMouse: true);
				FrameInput.UnuseKeyboardKey(Key.LeftArrow);
				FrameInput.UnuseKeyboardKey(Key.RightArrow);
				FrameInput.UnuseKeyboardKey(Key.Delete);
				FrameInput.UnuseKeyboardKey(Key.Escape);
			}

			if (!FrameInput.MouseLeftButton) ScrollBarMouseDownPos = null;

		}


		internal static void LateUpdate () {

			if (TypingBuilder.Length > 0) TypingBuilder.Clear();

			// Cancel Typing Text Field
			if (TypingTextFieldID != 0) {
				if (
					(FrameInput.AnyMouseButtonDown && !TypingTextFieldRect.Contains(FrameInput.MouseGlobalPosition)) ||
					PauselessFrame > TypingUpdateFrame
				) {
					CancelTyping();
				}
			}
		}


		// Unify
		public static int Unify (int value) => (value * CellRenderer.CameraRect.height / 1000f).RoundToInt();
		public static int Unify (float value) => (value * CellRenderer.CameraRect.height / 1000f).RoundToInt();
		public static int ReverseUnify (int value) => (value / (CellRenderer.CameraRect.height / 1000f)).RoundToInt();


		// Typing
		public static void CancelTyping () {
			TypingTextFieldID = 0;
			TypingTextFieldRect = default;
			if (TypingBuilder.Length > 0) TypingBuilder.Clear();
			BeamIndex = 0;
			BeamLength = 0;
		}


		// Label
		public static void Label (CellContent content, RectInt rect) => Label(content, rect, out _);
		public static void Label (CellContent content, RectInt rect, out RectInt bounds) => Label(content, rect, -1, out bounds, out _);
		public static void Label (CellContent content, RectInt rect, int beamIndex, out RectInt bounds, out RectInt beamRect) {

			bounds = rect;
			beamRect = new RectInt(rect.x, rect.y, 1, rect.height);
			if (!CellRenderer.TextReady) return;
			if (string.IsNullOrEmpty(content.Text)) content.Text = string.Empty;

			CellRenderer.RequestStringForFont(content.Text);

			string text = content.Text;
			string colorContent = content.ColorText;
			int charSize = Unify(content.CharSize);
			int lineSpace = Unify(content.LineSpace);
			var color = content.Tint;
			int charSpace = Unify(content.CharSpace);
			var alignment = content.Alignment;
			var bgColor = content.BackgroundTint;
			bool wrap = content.Wrap;
			bool tightBG = content.TightBackground;
			bool hasContent = !string.IsNullOrEmpty(text);
			bool clip = content.Clip;
			bool beamEnd = beamIndex >= text.Length;

			// Draw BG
			Cell bgCell = bgColor.a > 0 ? CellRenderer.Draw(Const.PIXEL, rect, bgColor) : null;

			// Content
			int count = text.Length;
			int maxLineCount = ((float)rect.height / (charSize + lineSpace)).FloorToInt();
			int line = 0;
			int x = rect.x;
			int y = rect.yMax - charSize;
			int startCellIndex = CellRenderer.GetTextUsedCellCount();
			int nextWrapCheckIndex = 0;
			bool firstCharAtLine = true;
			int minX = int.MaxValue;
			int minY = int.MaxValue;
			int maxX = int.MinValue;
			int maxY = int.MinValue;
			for (int i = 0; i < count; i++) {

				char c = text[i];
				if (c == '\r') continue;

				// Line
				if (c == '\n') {
					x = rect.x;
					y -= charSize + lineSpace;
					firstCharAtLine = true;
					line++;
					if (clip && line >= maxLineCount) break;
					continue;
				}

				CellRenderer.CharSprite sprite = null;

				// Require Char
				if (sprite == null && !CellRenderer.RequireChar(c, out sprite)) continue;

				// Wrap Check for Word
				if (wrap && i >= nextWrapCheckIndex && !IsLineBreakingChar(c)) {
					if (!WordEnoughToFit(
						text, charSize, charSpace, i, rect.xMax - x, out int wordLength
					) && !firstCharAtLine) {
						x = rect.x;
						y -= charSize + lineSpace;
						line++;
						if (clip && line >= maxLineCount) break;
					}
					nextWrapCheckIndex += wordLength;
				}

				// Draw Char
				int realCharSize = (sprite.Advance * charSize).RoundToInt();
				if (wrap && x > rect.xMax - realCharSize) {
					x = rect.x;
					y -= charSize + lineSpace;
					line++;
					if (char.IsWhiteSpace(c)) continue;
					if (clip && line >= maxLineCount) break;
				}
				if (colorContent != null && colorContent.Length > 1) {
					int cIndex = (i * 2).Clamp(0, colorContent.Length - 2);
					color = Util.IntToColor(Util.Char2ToInt(colorContent[cIndex], colorContent[cIndex + 1]));
				}
				var cell = DrawChar(c, x, y, charSize, charSize, color);

				// Beam
				if (!beamEnd && beamIndex >= 0 && i >= beamIndex) {
					beamRect.x = x;
					beamRect.y = y;
					beamRect.width = Unify(2);
					beamRect.height = charSize;
					beamIndex = -1;
				}
				if (beamEnd && beamIndex >= 0 && i >= count - 1) {
					beamRect.x = x + realCharSize + charSpace;
					beamRect.y = y;
					beamRect.width = Unify(2);
					beamRect.height = charSize;
					beamIndex = -1;
				}

				// Next
				x += realCharSize + charSpace;
				minX = Mathf.Min(minX, cell.X);
				minY = Mathf.Min(minY, cell.Y);
				maxX = Mathf.Max(maxX, cell.X + cell.Width);
				maxY = Mathf.Max(maxY, cell.Y + cell.Height);
				firstCharAtLine = false;

			}



			// Alignment
			if (hasContent) {
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
				CellRenderer.GetTextCells(out var cells, out int cellCount);
				for (int i = startCellIndex; i < cellCount; i++) {
					var cell = cells[i];
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
			}

			if (bgCell != null && tightBG) {
				bgCell.X = bounds.x;
				bgCell.Y = bounds.y;
				bgCell.Z = int.MaxValue;
				bgCell.Width = bounds.width;
				bgCell.Height = bounds.height;
			}

		}


		// Scroll Label
		public static void ScrollLabel (CellContent content, RectInt rect, ref int scrollPosition) {
			int before = CellRenderer.GetTextUsedCellCount();
			Label(content, rect, out var bounds);
			if (bounds.height < rect.height) {
				scrollPosition = 0;
				return;
			}
			scrollPosition = scrollPosition.Clamp(0, bounds.height - rect.height + Unify(content.CharSize * 2));
			int after = CellRenderer.GetTextUsedCellCount();
			if (before == after) return;
			if (CellRenderer.GetTextCells(out var cells, out int count)) {
				for (int i = before; i < after && i < count; i++) {
					var cell = cells[i];
					cell.Y += scrollPosition;
				}
			}
			CellRenderer.ClampTextCells(rect, before, after);
		}


		// Button
		public static bool Button (RectInt rect, int sprite, int spriteHover, int spriteDown, int icon, int buttonBorder, int iconPadding, int z) => Button(rect, sprite, spriteHover, spriteDown, icon, buttonBorder, iconPadding, z, Const.WHITE, Const.WHITE);
		public static bool Button (RectInt rect, int sprite, int spriteHover, int spriteDown, int icon, int buttonBorder, int iconPadding, int z, Color32 buttonTint, Color32 iconTint) {
			bool hover = rect.Contains(FrameInput.MouseGlobalPosition);
			bool down = hover && FrameInput.MouseLeftButton;
			CellRenderer.Draw_9Slice(
				down ? spriteDown : hover ? spriteHover : sprite,
				rect, buttonBorder, buttonBorder, buttonBorder, buttonBorder, buttonTint, z
			);
			if (icon != 0 && CellRenderer.TryGetSprite(icon, out var iconSprite)) {
				CellRenderer.Draw(
					iconSprite.GlobalID,
					rect.Shrink(iconPadding).Fit(iconSprite.GlobalWidth, iconSprite.GlobalHeight),
					iconTint, z + 1
				);
			}
			return hover && FrameInput.MouseLeftButtonDown;
		}


		// Gizmos
		public static Cell DrawLine (int fromX, int fromY, int toX, int toY, int thickness = 8, int z = int.MinValue) => DrawLine(fromX, fromY, toX, toY, thickness, Const.WHITE, z);
		public static Cell DrawLine (int fromX, int fromY, int toX, int toY, int thickness, Color32 tint, int z = int.MinValue) => CellRenderer.Draw(
			Const.PIXEL, fromX, fromY, 500, 0,
			-Vector2.SignedAngle(Vector2.up, new Vector2(toX - fromX, toY - fromY)).RoundToInt(),
			thickness, Util.DistanceInt(fromX, fromY, toX, toY),
			tint, z
		);


		// Text Field
		public static string TextField (int controlID, RectInt rect, string text) => TextField(controlID, rect, InputLabel.SetText(text, 24), out _);
		public static string TextField (int controlID, RectInt rect, string text, out bool changed) => TextField(controlID, rect, InputLabel.SetText(text, 24), out changed);
		public static string TextField (int controlID, RectInt rect, CellContent text) => TextField(controlID, rect, text, out _);
		public static string TextField (int controlID, RectInt rect, CellContent text, out bool changed) {

			changed = false;
			bool startTyping = false;
			bool mouseInRect = rect.Contains(FrameInput.MouseGlobalPosition);
			bool mouseDragging = FrameInput.MouseLeftButton && mouseInRect;

			// Start Typing
			if (FrameInput.MouseLeftButtonDown && mouseInRect) {
				TypingTextFieldID = controlID;
				BeamBlinkFrame = PauselessFrame;
				startTyping = true;
				mouseDragging = false;
			}

			// Typing 
			bool typing = TypingTextFieldID == controlID;
			if (typing) {

				// Clear
				if (FrameInput.KeyboardDown(Key.Escape)) {
					if (!string.IsNullOrEmpty(text.Text)) {
						text.Text = "";
						changed = true;
						BeamIndex = 0;
						CancelTyping();
					}
					FrameInput.UseKeyboardKey(Key.Escape);
				}

				// Move Beam
				if (FrameInput.KeyboardDownGUI(Key.LeftArrow)) {
					if (BeamLength == 0) {
						BeamIndex--;
					} else if (BeamLength < 0) {
						BeamIndex += BeamLength;
					}
					BeamLength = 0;
					BeamBlinkFrame = PauselessFrame;
				}
				if (FrameInput.KeyboardDownGUI(Key.RightArrow)) {
					if (BeamLength == 0) {
						BeamIndex++;
					} else if (BeamLength > 0) {
						BeamIndex += BeamLength;
					}
					BeamLength = 0;
					BeamBlinkFrame = PauselessFrame;
				}

				BeamIndex = BeamIndex.Clamp(0, text.Text.Length);
				BeamLength = BeamLength.Clamp(-BeamIndex, text.Text.Length - BeamIndex);
				TypingTextFieldRect = rect;
				TypingUpdateFrame = PauselessFrame;

				for (int i = 0; i < TypingBuilder.Length; i++) {
					char c = TypingBuilder[i];
					if (c == '\b') {
						// Backspace
						if (BeamLength == 0) {
							int removeIndex = BeamIndex - 1;
							if (removeIndex >= 0 && removeIndex < text.Text.Length) {
								text.Text = text.Text.Remove(removeIndex, 1);
								BeamIndex--;
								changed = true;
							}
						} else {
							RemoveSelection();
							changed = true;
						}
					} else if (c == '\r') {
						// Enter
						CancelTyping();
					} else if (text.Text.Length < MAX_INPUT_CHAR) {
						// Append Char
						if (BeamLength != 0) RemoveSelection();
						text.Text = text.Text.Insert(BeamIndex, c.ToString());
						BeamIndex++;
						changed = true;
					}
				}

				// Delete
				if (FrameInput.KeyboardDownGUI(Key.Delete)) {
					int removeIndex = BeamIndex;
					if (removeIndex >= 0 && removeIndex < text.Text.Length) {
						if (BeamLength == 0) {
							text.Text = text.Text.Remove(removeIndex, 1);
							changed = true;
						} else {
							RemoveSelection();
							changed = true;
						}
					}
					FrameInput.UseKeyboardKey(Key.Delete);
				}
				// Func
				void RemoveSelection () {
					int newBeamIndex = Mathf.Min(BeamIndex, BeamIndex + BeamLength);
					text.Text = text.Text.Remove(newBeamIndex, BeamLength.Abs());
					BeamIndex = newBeamIndex;
					BeamLength = 0;
				}
			}

			if (changed) BeamBlinkFrame = PauselessFrame;

			// Rendering
			int startCellIndex = CellRenderer.GetTextUsedCellCount();
			var labelRect = rect.Shrink(Unify(12), 0, 0, 0);
			int beamShrink = Unify(6);
			var beamRect = new RectInt(
				labelRect.x, labelRect.y + beamShrink, Unify(2), labelRect.height - beamShrink * 2
			);

			// Draw Text
			if (!string.IsNullOrEmpty(text.Text)) {
				Label(text, labelRect, BeamIndex, out _, out beamRect);
			}

			// Draw Beam
			Cell beamCell = null;
			if (typing && (PauselessFrame - BeamBlinkFrame) % 56 < 28) {
				beamRect.y = labelRect.y + beamShrink;
				beamRect.height = labelRect.height - beamShrink * 2;
				beamCell = CellRenderer.Draw(Const.PIXEL, beamRect, Const.WHITE, int.MaxValue);
			}
			int endCellIndex = CellRenderer.GetTextUsedCellCount();

			if (startCellIndex != endCellIndex && CellRenderer.GetTextCells(out var cells, out int count)) {

				// Scroll X from Beam 
				int beamCellIndex = (BeamIndex + startCellIndex).Clamp(startCellIndex, endCellIndex - 1);
				var beamCharCell = cells[beamCellIndex];

				// Shift for Beam Out
				int shiftX = 0;
				int labelRight = labelRect.xMax - Unify(22);
				if (beamCharCell.X + beamCharCell.Width / 2 >= labelRight) {
					shiftX = labelRight - beamCharCell.X;
					if (beamCell != null) beamCell.X += shiftX;
				}

				// Clip
				for (int i = startCellIndex; i < endCellIndex && i < count; i++) {
					var cell = cells[i];
					cell.X += shiftX;
					// Set Tint for Outside Clip
					if (
						cell.X + cell.Width > labelRect.x + labelRect.width ||
						cell.X < labelRect.x
					) {
						if (cell.X > labelRect.x + labelRect.width || cell.X + cell.Width < labelRect.x) {
							cell.Color = Const.CLEAR;
						} else {
							cell.Color = new Color32(255, 255, 255, 96);
						}
					}
				}

				// Get Beam Selection Rect
				if (BeamLength != 0) {
					int beamSelectionStartIndex = Mathf.Min(BeamIndex, BeamIndex + BeamLength);
					int beamSelectionEndIndex = Mathf.Max(BeamIndex, BeamIndex + BeamLength);
					var startCell = cells[(startCellIndex + beamSelectionStartIndex).Clamp(startCellIndex, endCellIndex - 1)];
					var endCell = cells[(startCellIndex + beamSelectionEndIndex - 1).Clamp(startCellIndex, endCellIndex - 1)];
					var selectionRect = new RectInt(
						startCell.X,
						labelRect.y + beamShrink,
						endCell.X + endCell.Width - startCell.X,
						labelRect.height - beamShrink * 2
					);
					CellRenderer.Draw(Const.PIXEL, selectionRect, Const.ORANGE, int.MaxValue - 1);
				}

				if (typing && (startTyping || mouseDragging)) {
					int mouseBeamIndex = 0;
					int mouseX = FrameInput.MouseGlobalPosition.x;
					for (int i = startCellIndex; i < endCellIndex && i < count; i++) {
						var cell = cells[i];
						int x = cell.X + cell.Width / 2;
						mouseBeamIndex = i - startCellIndex;
						// End Check
						if (i == endCellIndex - 1 && mouseX > x) {
							mouseBeamIndex = text.Text.Length;
						}
						if (x > mouseX) break;
					}
					// Set Beam on Click
					if (startTyping) {
						BeamIndex = mouseBeamIndex;
						BeamLength = 0;
					}
					// Set Selection on Drag
					if (mouseDragging) {
						BeamLength += BeamIndex - mouseBeamIndex;
						BeamIndex = mouseBeamIndex;
					}
				}
			}

			// Clamp
			if (text.Text.Length > MAX_INPUT_CHAR) {
				text.Text = text.Text[..MAX_INPUT_CHAR];
			}

			return text.Text;
		}


		// Scrollbar
		public static int ScrollBar (RectInt contentRect, int z, int positionRow, int totalSize, int pageSize, int barSpriteId = Const.PIXEL) {
			if (pageSize >= totalSize) return 0;
			int barHeight = contentRect.height * pageSize / totalSize;
			var barRect = new RectInt(
				contentRect.x,
				Util.RemapUnclamped(
					0, totalSize - pageSize,
					contentRect.yMax - barHeight, contentRect.y,
					positionRow
				),
				contentRect.width,
				barHeight
			);
			bool hoveringBar = barRect.Contains(FrameInput.MouseGlobalPosition);

			CellRenderer.Draw(
				barSpriteId,
				barRect,
				hoveringBar || ScrollBarMouseDownPos.HasValue ? Const.GREY_128 : Const.GREY_64,
				z
			);

			// Dragging
			if (ScrollBarMouseDownPos.HasValue) {
				int mouseY = FrameInput.MouseGlobalPosition.y;
				int mouseDownY = ScrollBarMouseDownPos.Value.x;
				int scrollDownY = ScrollBarMouseDownPos.Value.y;
				positionRow = scrollDownY + (mouseDownY - mouseY) * totalSize / contentRect.height;
			}

			// Mouse Down
			if (FrameInput.MouseLeftButtonDown) {
				if (hoveringBar) {
					// Start Drag
					ScrollBarMouseDownPos = new Vector2Int(
						FrameInput.MouseGlobalPosition.y, positionRow
					);
				} else if (contentRect.Contains(FrameInput.MouseGlobalPosition)) {
					// Jump on Click
					int mouseY = FrameInput.MouseGlobalPosition.y;
					positionRow = Util.RemapUnclamped(
						contentRect.y, contentRect.yMax,
						totalSize - pageSize / 2, -pageSize / 2,
						mouseY
					);
					ScrollBarMouseDownPos = new Vector2Int(mouseY, positionRow);
				}
			}

			return positionRow.Clamp(0, totalSize - pageSize);
		}


		// Highlight
		public static void HighlightCursor (int spriteID, RectInt rect, int z) => HighlightCursor(spriteID, rect, z, Const.GREEN);
		public static void HighlightCursor (int spriteID, RectInt rect, int z, Color32 color) {
			int border = Unify(4);
			int thickness = Unify(8);
			CellRenderer.Draw_9Slice(
				spriteID, rect.Expand(Game.GlobalFrame.PingPong(thickness)),
				border, border, border, border,
				color, z
			);
		}


		// Misc
		public static string GetNumberCache (int number) {
			if (number >= NUMBER_CACHE.Length) return "99+";
			if (number >= 0) return NUMBER_CACHE[number];
			return "";
		}


		#endregion




		#region --- LGC ---


		private static bool WordEnoughToFit (string content, int charSize, int charSpace, int startIndex, int room, out int wordLength) {
			int len = content.Length;
			int index = startIndex;
			for (; index < len; index++) {
				char c = content[index];
				if (IsLineBreakingChar(c)) break;
				if (!CellRenderer.RequireChar(c, out var sprite)) continue;
				// Room
				if (room > 0) {
					room -= (sprite.Advance * charSize).RoundToInt() + charSpace;
				}
			}
			wordLength = index - startIndex + 1;
			return room >= 0;
		}


		private static bool IsLineBreakingChar (char c) =>
			char.IsWhiteSpace(c) || char.GetUnicodeCategory(c) switch {
				UnicodeCategory.DecimalDigitNumber => false,
				UnicodeCategory.LowercaseLetter => false,
				UnicodeCategory.LetterNumber => false,
				UnicodeCategory.UppercaseLetter => false,
				UnicodeCategory.MathSymbol => false,
				UnicodeCategory.TitlecaseLetter => false,
				_ => true,
			};


		private static Cell DrawChar (char c, int x, int y, int width, int height, Color32 color) {

			if (!CellRenderer.TextReady) return EMPTY_CELL;

			// Require
			if (!CellRenderer.RequireChar(c, out var sprite)) return EMPTY_CELL;

			// Draw
			var cell = CellRenderer.Draw(c, x, y, 0, 0, 0, width, height, color);
			if (cell.Index < 0) { return cell; }
			var uvOffset = sprite.Offset;
			cell.X += (int)(cell.Width * uvOffset.x);
			cell.Y += (int)(cell.Height * uvOffset.y);
			cell.Width = (int)(cell.Width * uvOffset.width);
			cell.Height = (int)(cell.Height * uvOffset.height);
			return cell;
		}


		// MSG
		private static void OnDeviceChange (InputDevice device, InputDeviceChange change) {
			if (device is Keyboard && Keyboard.current != null) {
				Keyboard.current.onTextInput -= OnTextInput;
				Keyboard.current.onTextInput += OnTextInput;
			}
		}


		private static void OnTextInput (char c) {
			if (c != '\b' && c != '\r' && char.IsControl(c)) return;
			TypingBuilder.Append(c);
		}


		#endregion




	}
}