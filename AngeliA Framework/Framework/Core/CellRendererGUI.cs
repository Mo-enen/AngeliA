using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;


namespace AngeliaFramework {


	public class CellContent {

		public static readonly CellContent Empty = new();
		private static readonly CellContent Temp = new();

		public string Text;
		public char[] Chars;
		public bool FromString;
		public Byte4 Tint;
		public Byte4 BackgroundTint;
		public Alignment Alignment;
		public int CharSize;
		public int CharSpace;
		public int LineSpace;
		public int ShadowOffset;
		public bool Wrap;
		public bool Clip;
		public int BackgroundPadding;
		public Byte4 Shadow;

		public CellContent (string text = "") {
			Text = text;
			Chars = null;
			FromString = true;
			Tint = Const.WHITE;
			BackgroundTint = Const.CLEAR;
			Alignment = Alignment.MidMid;
			CharSize = 24;
			CharSpace = 0;
			LineSpace = 5;
			Wrap = false;
			Clip = false;
			BackgroundPadding = -1;
			Shadow = Const.CLEAR;
		}

		public CellContent SetText (string newText) {
			Text = newText;
			Chars = null;
			FromString = true;
			return this;
		}

		public CellContent SetText (string newText, int charSize) {
			Text = newText;
			Chars = null;
			CharSize = charSize;
			FromString = true;
			return this;
		}

		public static CellContent Get (string text, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) {
			Temp.CharSize = charSize;
			Temp.Text = text;
			Temp.Chars = null;
			Temp.Alignment = alignment;
			Temp.Tint = Const.WHITE;
			Temp.Wrap = wrap;
			Temp.FromString = true;
			Temp.BackgroundTint = default;
			Temp.Shadow = Const.CLEAR;
			return Temp;
		}

		public static CellContent Get (string text, Byte4 tint, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) {
			Temp.CharSize = charSize;
			Temp.Text = text;
			Temp.Chars = null;
			Temp.Alignment = alignment;
			Temp.Tint = tint;
			Temp.Wrap = wrap;
			Temp.FromString = true;
			Temp.BackgroundTint = default;
			Temp.Shadow = Const.CLEAR;
			return Temp;
		}

		public static CellContent Get (char[] chars, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) {
			Temp.CharSize = charSize;
			Temp.Chars = chars;
			Temp.Alignment = alignment;
			Temp.Tint = Const.WHITE;
			Temp.Wrap = wrap;
			Temp.FromString = false;
			Temp.BackgroundTint = default;
			Temp.Shadow = Const.CLEAR;
			return Temp;
		}

		public static CellContent Get (char[] chars, Byte4 tint, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) {
			Temp.CharSize = charSize;
			Temp.Chars = chars;
			Temp.Alignment = alignment;
			Temp.Tint = tint;
			Temp.Wrap = wrap;
			Temp.FromString = false;
			Temp.BackgroundTint = default;
			Temp.Shadow = Const.CLEAR;
			return Temp;
		}

	}


	public static class CellRendererGUI {




		#region --- VAR ---


		// Const
		private static readonly Cell EMPTY_CELL = new() { Sprite = null, TextSprite = null, };
		private static readonly string[] NUMBER_CACHE = new string[100];
		private const int MAX_INPUT_CHAR = 256;
		private const char CONTROL_CUT = (char)24;
		private const char CONTROL_COPY = (char)3;
		private const char CONTROL_PASTE = (char)22;

		// Api
		public static bool IsTyping => TypingTextFieldID != 0;
		public static int TypingTextFieldID {
			get => _TypingTextFieldID;
			private set {
				if (_TypingTextFieldID != value) {
					_TypingTextFieldID = value;
					Game.SetImeCompositionMode(value != 0);
				}
			}
		}
		public static int _TypingTextFieldID = 0;

		// UI
		private static readonly CellContent InputLabel = new() {
			Alignment = Alignment.MidLeft,
			Clip = true,
			Wrap = false,
		};

		// Data
		private static readonly StringBuilder TypingBuilder = new();
		private static IRect TypingTextFieldRect = default;
		private static int TypingUpdateFrame = int.MinValue;
		private static int BeamIndex = 0;
		private static int BeamLength = 0;
		private static int BeamBlinkFrame = int.MinValue;
		private static Int2? ScrollBarMouseDownPos = null;


		#endregion




		#region --- API ---


		[OnGameInitialize(-128)]
		public static void Initialize () {
			for (int i = 0; i < NUMBER_CACHE.Length; i++) {
				NUMBER_CACHE[i] = i.ToString();
			}
		}


		[OnGameUpdate(1023)]
		internal static void Update () {
			if (TypingTextFieldID != 0 && FrameInput.AnyKeyHolding) {
				FrameInput.UseAllHoldingKeys(ignoreMouse: true);
				FrameInput.UnuseKeyboardKey(KeyboardKey.LeftArrow);
				FrameInput.UnuseKeyboardKey(KeyboardKey.RightArrow);
				FrameInput.UnuseKeyboardKey(KeyboardKey.Delete);
				FrameInput.UnuseKeyboardKey(KeyboardKey.Escape);
			}
			if (!FrameInput.MouseLeftButton) ScrollBarMouseDownPos = null;
		}


		[OnGameUpdateLater(4096)]
		internal static void LateUpdate () {

			if (TypingBuilder.Length > 0) TypingBuilder.Clear();

			// Cancel Typing Text Field
			if (TypingTextFieldID != 0) {
				if (
					(FrameInput.AnyMouseButtonDown && !TypingTextFieldRect.MouseInside()) ||
					Game.PauselessFrame > TypingUpdateFrame
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
			TypingBuilder.Clear();
			BeamIndex = 0;
			BeamLength = 0;
		}


		// Label
		public static void Label (CellContent content, IRect rect) => Label(content, rect, -1, 0, false, out _, out _, out _);
		public static void Label (CellContent content, IRect rect, out IRect bounds) => Label(content, rect, -1, 0, false, out bounds, out _, out _);
		public static void Label (CellContent content, IRect rect, int startIndex, bool drawInvisibleChar, out IRect bounds, out int endIndex) => Label(content, rect, -1, startIndex, drawInvisibleChar, out bounds, out _, out endIndex);
		private static void Label (CellContent content, IRect rect, int beamIndex, int startIndex, bool drawInvisibleChar, out IRect bounds, out IRect beamRect, out int endIndex) {

			endIndex = startIndex;
			bounds = rect;
			beamRect = new IRect(rect.x, rect.y, 1, rect.height);
			if (!CellRenderer.TextReady) return;
			if (string.IsNullOrEmpty(content.Text)) content.Text = string.Empty;

			if (content.FromString) {
				CellRenderer.RequestStringForFont(content.Text);
			} else {
				CellRenderer.RequestStringForFont(content.Chars);
			}

			string text = content.Text;
			char[] chars = content.Chars;
			int count = content.FromString ? text.Length : chars.Length;
			int charSize = Unify(content.CharSize);
			int lineSpace = Unify(content.LineSpace);
			var color = content.Tint;
			int charSpace = Unify(content.CharSpace);
			var alignment = content.Alignment;
			var bgColor = content.BackgroundTint;
			bool wrap = content.Wrap;
			int bgPadding = Unify(content.BackgroundPadding);
			bool hasContent = count > 0;
			bool clip = content.Clip;
			bool beamEnd = beamIndex >= count;

			// Draw BG
			Cell bgCell = bgColor.a > 0 ? CellRenderer.Draw(Const.PIXEL, rect, bgColor) : null;

			// Content
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
			int shadowOffset = Unify(content.ShadowOffset);
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
				if (!CellRenderer.RequireCharForPool(c, out var sprite)) goto CONTINUE;

				int realCharSize = (sprite.Advance * charSize).RoundToInt();

				// Wrap Check for Word
				if (wrap && i >= nextWrapCheckIndex && !IsLineBreakingChar(c)) {
					if (!WordEnoughToFit(
						content, charSize, charSpace, i, rect.xMax - x - realCharSize, out int wordLength
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
				if (wrap && x > rect.xMax - realCharSize) {
					x = rect.x;
					y -= charSize + lineSpace;
					line++;
					if (char.IsWhiteSpace(c)) goto CONTINUE;
					if (clip && line >= maxLineCount) break;
				}
				var cell = DrawChar(c, x, y, charSize, charSize, color) ?? EMPTY_CELL;
				if (content.Shadow.a > 0 && shadowOffset != 0) {
					var shadowCell = CellRenderer.DrawChar(c, 0, 0, 1, 1, Const.WHITE);
					shadowCell.CopyFrom(cell);
					shadowCell.Color = content.Shadow;
					shadowCell.Y -= shadowOffset;
					shadowCell.Z--;
				}

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
				minX = Util.Min(minX, cell.X);
				minY = Util.Min(minY, cell.Y);
				maxX = Util.Max(maxX, cell.X + cell.Width);
				maxY = Util.Max(maxY, cell.Y + cell.Height);
				firstCharAtLine = false;

				continue;

				CONTINUE:;
				if (drawInvisibleChar) {
					int cellCount = CellRenderer.GetTextUsedCellCount() - startCellIndex - 1;
					int textCount = i - startIndex;
					int addCount = textCount - cellCount;
					for (int add = 0; add < addCount; add++) {
						CellRenderer.DrawChar(' ', 0, 0, 0, 0, color);
					}
				}

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

			if (bgCell != null && bgPadding >= 0) {
				bgCell.X = bounds.x - bgPadding;
				bgCell.Y = bounds.y - bgPadding;
				bgCell.Z = int.MaxValue;
				bgCell.Width = bounds.width + bgPadding * 2;
				bgCell.Height = bounds.height + bgPadding * 2;
			}

		}


		// Scroll Label
		public static void ScrollLabel (CellContent content, IRect rect, ref int scrollPosition) {
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
		public static bool Button (IRect rect, string label, int z, int charSize = -1, bool enable = true) => Button(rect, label, out _, z, charSize, enable);
		public static bool Button (IRect rect, string label, int z, Byte4 labelTint, int charSize = -1, bool enable = true) => Button(rect, label, out _, z, labelTint, charSize, enable);
		public static bool Button (IRect rect, int sprite, string label, int z, Byte4 buttonTint, Byte4 labelTint, int charSize = -1, bool enable = true) => Button(rect, sprite, label, out _, z, buttonTint, labelTint, charSize, enable);
		public static bool Button (IRect rect, string label, out IRect labelBounds, int z, int charSize = -1, bool enable = true) => Button(rect, label, out labelBounds, z, Const.WHITE, charSize, enable);
		public static bool Button (IRect rect, string label, out IRect labelBounds, int z, Byte4 labelTint, int charSize = -1, bool enable = true) {
			charSize = charSize < 0 ? ReverseUnify(rect.height / 2) : charSize;
			Label(CellContent.Get(label, labelTint, charSize), rect, out labelBounds);
			return Button(rect, 0, Const.PIXEL, 0, 0, 0, 0, z, Const.WHITE_12, default, enable);
		}
		public static bool Button (IRect rect, int sprite, string label, out IRect labelBounds, int z, Byte4 buttonTint, Byte4 labelTint, int charSize = -1, bool enable = true) {
			charSize = charSize < 0 ? ReverseUnify(rect.height / 2) : charSize;
			Label(CellContent.Get(label, labelTint, charSize), rect, out labelBounds);
			return Button(rect, sprite, sprite, sprite, 0, 0, 0, z, buttonTint, default, enable);
		}
		public static bool Button (IRect rect, int sprite, int spriteHover, int spriteDown, int icon, int buttonBorder, int iconPadding, int z, bool enable = true) => Button(rect, sprite, spriteHover, spriteDown, icon, buttonBorder, iconPadding, z, Const.WHITE, Const.WHITE, enable);
		public static bool Button (IRect rect, int sprite, int spriteHover, int spriteDown, int icon, int buttonBorder, int iconPadding, int z, Byte4 buttonTint, Byte4 iconTint, bool enable = true) {
			bool hover = rect.MouseInside();
			bool down = hover && enable && FrameInput.MouseLeftButton;
			buttonTint.a = (byte)(enable ? buttonTint.a : buttonTint.a / 2);
			iconTint.a = (byte)(enable ? iconTint.a : iconTint.a / 2);
			// Button
			int spriteID = down ? spriteDown : hover ? spriteHover : sprite;
			if (spriteID != 0) {
				if (buttonBorder > 0) {
					CellRenderer.Draw_9Slice(
						spriteID, rect, buttonBorder, buttonBorder, buttonBorder, buttonBorder, buttonTint, z
					);
				} else {
					CellRenderer.Draw_9Slice(spriteID, rect, buttonTint, z);
				}
			}
			// Icon
			if (icon != 0 && CellRenderer.TryGetSprite(icon, out var iconSprite)) {
				CellRenderer.Draw(
					iconSprite,
					rect.Shrink(iconPadding).Fit(iconSprite),
					iconTint, z + 1
				);
			}
			// Cursor
			if (enable) CursorSystem.SetCursorAsHand(rect);
			// Click
			if (enable && hover && FrameInput.MouseLeftButtonDown) {
				FrameInput.UseMouseKey(0);
				FrameInput.UseGameKey(Gamekey.Action);
				return true;
			}
			return false;
		}


		// Gizmos
		public static Cell DrawLine (int fromX, int fromY, int toX, int toY, int thickness = 8, int z = int.MinValue) => DrawLine(fromX, fromY, toX, toY, thickness, Const.WHITE, z);
		public static Cell DrawLine (int fromX, int fromY, int toX, int toY, int thickness, Byte4 tint, int z = int.MinValue) => CellRenderer.Draw(
			Const.PIXEL, fromX, fromY, 500, 0,
			-Float2.SignedAngle(Float2.up, new Float2(toX - fromX, toY - fromY)).RoundToInt(),
			thickness, Util.DistanceInt(fromX, fromY, toX, toY),
			tint, z
		);


		// Text Field
		public static string TextField (int controlID, IRect rect, string text) => TextField(controlID, rect, InputLabel.SetText(text, ReverseUnify(rect.height / 2)), out _);
		public static string TextField (int controlID, IRect rect, string text, out bool changed) => TextField(controlID, rect, InputLabel.SetText(text, ReverseUnify(rect.height / 2)), out changed);
		public static string TextField (int controlID, IRect rect, CellContent text) => TextField(controlID, rect, text, out _);
		public static string TextField (int controlID, IRect rect, CellContent text, out bool changed) {

			changed = false;
			bool startTyping = false;
			bool mouseDownPosInRect = rect.Contains(FrameInput.MouseLeftDownGlobalPosition);
			bool mouseDragging = FrameInput.MouseLeftButton && mouseDownPosInRect;
			bool inCamera = rect.Overlaps(CellRenderer.CameraRect);

			CursorSystem.SetCursorAsBeam(rect);

			if (!inCamera && TypingTextFieldID == controlID) TypingTextFieldID = 0;

			// Start Typing
			if (inCamera && FrameInput.MouseLeftButtonDown && mouseDownPosInRect) {
				TypingTextFieldID = controlID;
				BeamBlinkFrame = Game.PauselessFrame;
				startTyping = true;
				mouseDragging = false;
			}

			// Typing 
			bool typing = TypingTextFieldID == controlID;
			int beamIndex = typing ? BeamIndex : 0;
			int beamLength = typing ? BeamLength : 0;
			if (typing) {

				// Clear
				if (FrameInput.KeyboardUp(KeyboardKey.Escape)) {
					beamIndex = BeamIndex = 0;
					CancelTyping();
					FrameInput.UseKeyboardKey(KeyboardKey.Escape);
					FrameInput.UseGameKey(Gamekey.Start);
				}

				// Move Beam
				if (FrameInput.KeyboardDownGUI(KeyboardKey.LeftArrow)) {
					if (beamLength == 0) {
						beamIndex = BeamIndex = beamIndex - 1;
					} else if (beamLength < 0) {
						beamIndex = BeamIndex = beamIndex + beamLength;
					}
					beamLength = BeamLength = 0;
					BeamBlinkFrame = Game.PauselessFrame;
				}
				if (FrameInput.KeyboardDownGUI(KeyboardKey.RightArrow)) {
					if (beamLength == 0) {
						beamIndex = BeamIndex = beamIndex + 1;
					} else if (beamLength > 0) {
						beamIndex = BeamIndex = beamIndex + beamLength;
					}
					beamLength = BeamLength = 0;
					BeamBlinkFrame = Game.PauselessFrame;
				}

				beamIndex = BeamIndex = beamIndex.Clamp(0, text.Text.Length);
				beamLength = BeamLength = beamLength.Clamp(-beamIndex, text.Text.Length - beamIndex);
				TypingTextFieldRect = rect;
				TypingUpdateFrame = Game.PauselessFrame;

				for (int i = 0; i < TypingBuilder.Length; i++) {
					char c = TypingBuilder[i];
					switch (c) {
						case '\b':
							// Backspace
							if (beamLength == 0) {
								int removeIndex = beamIndex - 1;
								if (removeIndex >= 0 && removeIndex < text.Text.Length) {
									text.Text = text.Text.Remove(removeIndex, 1);
									beamIndex = BeamIndex = beamIndex - 1;
									changed = true;
								}
							} else {
								RemoveSelection();
								changed = true;
							}
							break;
						case '\r':
							// Enter
							CancelTyping();
							break;
						case CONTROL_COPY:
						case CONTROL_CUT:
							if (beamLength == 0) break;
							int beamStart = Util.Min(beamIndex, beamIndex + beamLength);
							int beamEnd = Util.Max(beamIndex, beamIndex + beamLength);
							Game.SetClipboardText(text.Text[beamStart..beamEnd]);
							if (c == CONTROL_CUT) {
								RemoveSelection();
								changed = true;
							}
							break;
						case CONTROL_PASTE:
							string clipboardText = Game.GetClipboardText();
							if (string.IsNullOrEmpty(clipboardText)) break;
							if (beamLength != 0) RemoveSelection();
							text.Text = text.Text.Insert(beamIndex, clipboardText);
							beamIndex = BeamIndex = beamIndex + clipboardText.Length;
							changed = true;
							break;
						default:
							if (text.Text.Length >= MAX_INPUT_CHAR) break;
							// Append Char
							if (beamLength != 0) RemoveSelection();
							text.Text = text.Text.Insert(beamIndex, c.ToString());
							beamIndex = BeamIndex = beamIndex + 1;
							changed = true;
							break;
					}
				}

				// Delete
				if (FrameInput.KeyboardDownGUI(KeyboardKey.Delete)) {
					int removeIndex = beamIndex;
					if (removeIndex >= 0 && removeIndex < text.Text.Length) {
						if (beamLength == 0) {
							// Delete One Char
							text.Text = text.Text.Remove(removeIndex, 1);
							changed = true;
						} else {
							// Delete Selection
							RemoveSelection();
							changed = true;
						}
					}
					FrameInput.UseKeyboardKey(KeyboardKey.Delete);
				}
				// Func
				void RemoveSelection () {
					int newBeamIndex = Util.Min(beamIndex, beamIndex + beamLength);
					text.Text = text.Text.Remove(newBeamIndex, beamLength.Abs());
					beamIndex = BeamIndex = newBeamIndex;
					beamLength = BeamLength = 0;
				}
			}

			if (changed) BeamBlinkFrame = Game.PauselessFrame;

			// Rendering
			int startCellIndex = CellRenderer.GetTextUsedCellCount();
			var labelRect = rect.Shrink(Unify(12), 0, 0, 0);
			int beamShrink = rect.height / 12;
			var beamRect = new IRect(
				labelRect.x, labelRect.y + beamShrink, Unify(2), labelRect.height - beamShrink * 2
			);

			// Draw Text
			if (!string.IsNullOrEmpty(text.Text)) {
				Label(text, labelRect, beamIndex, 0, false, out _, out beamRect, out _);
			}

			// Draw Beam
			Cell beamCell = null;
			if (!startTyping && typing && (Game.PauselessFrame - BeamBlinkFrame) % 56 < 28) {
				beamRect.y = labelRect.y + beamShrink;
				beamRect.height = labelRect.height - beamShrink * 2;
				beamCell = CellRenderer.Draw(Const.PIXEL, beamRect, Const.WHITE, int.MaxValue);
			}
			int endCellIndex = CellRenderer.GetTextUsedCellCount();

			if (startCellIndex != endCellIndex && CellRenderer.GetTextCells(out var cells, out int count)) {

				// Scroll X from Beam 
				int beamCellIndex = typing ? (beamIndex + startCellIndex).Clamp(startCellIndex, endCellIndex - 1) : startCellIndex;
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
							cell.Color = new Byte4(255, 255, 255, 96);
						}
					}
				}

				// Get Beam Selection Rect
				if (!startTyping && typing && beamLength != 0) {
					int beamSelectionStartIndex = Util.Min(beamIndex, beamIndex + beamLength);
					int beamSelectionEndIndex = Util.Max(beamIndex, beamIndex + beamLength);
					var startCell = cells[(startCellIndex + beamSelectionStartIndex).Clamp(startCellIndex, endCellIndex - 1)];
					var endCell = cells[(startCellIndex + beamSelectionEndIndex - 1).Clamp(startCellIndex, endCellIndex - 1)];
					var selectionRect = IRect.MinMaxRect(
						Util.Max(startCell.X, rect.x),
						Util.Max(labelRect.y + beamShrink, rect.y),
						Util.Min(endCell.X + endCell.Width, rect.xMax),
						Util.Min(labelRect.yMax - beamShrink, rect.yMax)
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

			// Clamp
			if (text.Text.Length > MAX_INPUT_CHAR) {
				text.Text = text.Text[..MAX_INPUT_CHAR];
			}

			return text.Text;
		}


		// Scrollbar
		public static int ScrollBar (IRect contentRect, int z, int positionRow, int totalSize, int pageSize, int barSpriteId = Const.PIXEL) {
			if (pageSize >= totalSize) return 0;
			int barHeight = contentRect.height * pageSize / totalSize;
			var barRect = new IRect(
				contentRect.x,
				Util.RemapUnclamped(
					0, totalSize - pageSize,
					contentRect.yMax - barHeight, contentRect.y,
					positionRow
				),
				contentRect.width,
				barHeight
			);
			bool hoveringBar = barRect.MouseInside();

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
					ScrollBarMouseDownPos = new Int2(
						FrameInput.MouseGlobalPosition.y, positionRow
					);
				} else if (contentRect.MouseInside()) {
					// Jump on Click
					int mouseY = FrameInput.MouseGlobalPosition.y;
					positionRow = Util.RemapUnclamped(
						contentRect.y, contentRect.yMax,
						totalSize - pageSize / 2, -pageSize / 2,
						mouseY
					);
					ScrollBarMouseDownPos = new Int2(mouseY, positionRow);
				}
			}

			return positionRow.Clamp(0, totalSize - pageSize);
		}


		// Highlight
		public static void HighlightCursor (int spriteID, IRect rect, int z) => HighlightCursor(spriteID, rect, z, Const.GREEN);
		public static void HighlightCursor (int spriteID, IRect rect, int z, Byte4 color) {
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


		public static void OnTextInput (char c) {
			if (!IsTyping) return;
			if (char.IsControl(c)) {
				switch (c) {
					case '\b':
					case '\r':
					case CONTROL_CUT:
					case CONTROL_COPY:
					case CONTROL_PASTE:
						break;
					default:
						return;
				}
			}
			TypingBuilder.Append(c);
		}


		#endregion




		#region --- LGC ---


		private static bool WordEnoughToFit (CellContent content, int charSize, int charSpace, int startIndex, int room, out int wordLength) {
			int index = startIndex;
			int count = content.FromString ? content.Text.Length : content.Chars.Length;
			for (; index < count; index++) {
				char c = content.FromString ? content.Text[index] : content.Chars[index];
				if (IsLineBreakingChar(c)) break;
				if (!CellRenderer.RequireCharForPool(c, out var sprite)) continue;
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


		private static Cell DrawChar (char c, int x, int y, int width, int height, Byte4 color) {

			if (!CellRenderer.TextReady) return null;

			// Require
			if (!CellRenderer.RequireCharForPool(c, out var sprite)) return null;

			// Draw
			var cell = CellRenderer.DrawChar(c, x, y, width, height, color);
			if (cell.TextSprite == null) return cell;
			var uvOffset = sprite.Offset;
			cell.X += (int)(cell.Width * uvOffset.x);
			cell.Y += (int)(cell.Height * uvOffset.y);
			cell.Width = (int)(cell.Width * uvOffset.width);
			cell.Height = (int)(cell.Height * uvOffset.height);
			return cell;
		}


		#endregion




	}
}