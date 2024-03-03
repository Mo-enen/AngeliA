using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;


namespace AngeliA.Framework;


public static class GUI {




	#region --- VAR ---


	// Const
	private static readonly string[] NUMBER_CACHE = new string[100];
	private const int MAX_INPUT_CHAR = 256;

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
	private static readonly TextContent InputLabel = new() {
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
	private static int DraggingScrollbarID = 0;


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
		if (TypingTextFieldID != 0 && Input.AnyKeyHolding) {
			Input.UseAllHoldingKeys(ignoreMouse: true);
			Input.UnuseKeyboardKey(KeyboardKey.LeftArrow);
			Input.UnuseKeyboardKey(KeyboardKey.RightArrow);
			Input.UnuseKeyboardKey(KeyboardKey.Delete);
			Input.UnuseKeyboardKey(KeyboardKey.Escape);
		}
		if (!Input.MouseLeftButton) ScrollBarMouseDownPos = null;
	}


	[OnGameUpdateLater(4096)]
	internal static void LateUpdate () {

		if (TypingBuilder.Length > 0) TypingBuilder.Clear();

		// Cancel Typing Text Field
		if (TypingTextFieldID != 0) {
			if (
				(Input.AnyMouseButtonDown && !TypingTextFieldRect.MouseInside()) ||
				Game.PauselessFrame > TypingUpdateFrame
			) {
				CancelTyping();
			}
		}
	}


	// Unify
	public static int Unify (int value) => (value * Renderer.CameraRect.height / 1000f).RoundToInt();
	public static int Unify (float value) => (value * Renderer.CameraRect.height / 1000f).RoundToInt();
	public static int ReverseUnify (int value) => (value * 1000f / Renderer.CameraRect.height).RoundToInt();
	public static int UnifyMonitor (int value) => (value * Renderer.CameraRect.height * Game.MonitorHeight / 1000f / Game.ScreenHeight).RoundToInt();
	public static int UnifyMonitor (float value) => (value * Renderer.CameraRect.height * Game.MonitorHeight / 1000f / Game.ScreenHeight).RoundToInt();


	// Typing
	public static void StartTyping (int controlID) {
		TypingTextFieldID = controlID;
		BeamIndex = 0;
		BeamLength = 0;
		BeamBlinkFrame = Game.PauselessFrame;
	}


	public static void CancelTyping () {
		TypingTextFieldID = 0;
		TypingTextFieldRect = default;
		TypingBuilder.Clear();
		BeamIndex = 0;
		BeamLength = 0;
	}


	// Label
	public static void Label (string text, IRect rect, Color32 tint, out IRect bounds, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) => Label(TextContent.Get(text, tint, charSize, alignment, wrap), rect, out bounds);
	public static void Label (string text, IRect rect, out IRect bounds, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) => Label(TextContent.Get(text, charSize, alignment, wrap), rect, out bounds);
	public static void Label (string text, IRect rect, Color32 tint, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) => Label(TextContent.Get(text, tint, charSize, alignment, wrap), rect);
	public static void Label (string text, IRect rect, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) => Label(TextContent.Get(text, charSize, alignment, wrap), rect);
	public static void Label (TextContent content, IRect rect) => Label(content, rect, -1, 0, false, out _, out _, out _);
	public static void Label (TextContent content, IRect rect, out IRect bounds) => Label(content, rect, -1, 0, false, out bounds, out _, out _);
	public static void Label (TextContent content, IRect rect, int startIndex, bool drawInvisibleChar, out IRect bounds, out int endIndex) => Label(content, rect, -1, startIndex, drawInvisibleChar, out bounds, out _, out endIndex);
	private static void Label (TextContent content, IRect rect, int beamIndex, int startIndex, bool drawInvisibleChar, out IRect bounds, out IRect beamRect, out int endIndex) {
		endIndex = startIndex;
		bounds = rect;
		beamRect = new IRect(rect.x, rect.y, 1, rect.height);
		if (!Renderer.TextReady) return;

		// Draw BG
		var bgColor = content.BackgroundTint;
		int bgPadding = Unify(content.BackgroundPadding);
		var bgCell = bgColor.a > 0 ? Renderer.Draw(Const.PIXEL, rect, bgColor, z: int.MaxValue) : null;
		if (bgCell != null && bgPadding >= 0) {
			bgCell.X = bounds.x - bgPadding;
			bgCell.Y = bounds.y - bgPadding;
			bgCell.Z = int.MaxValue;
			bgCell.Width = bounds.width + bgPadding * 2;
			bgCell.Height = bounds.height + bgPadding * 2;
		}

		// Draw Chars
		Renderer.GetTextCells(out var cells, out int cellCount);
		TextUtil.DrawLabel(
			Renderer.RequireCharForPool, Renderer.DrawChar,
			Renderer.CameraRect.height, cellCount, cells,
			content, rect, beamIndex, startIndex, drawInvisibleChar,
			out bounds, out beamRect, out endIndex
		);

	}


	// Scroll Label
	public static void ScrollLabel (TextContent content, IRect rect, ref int scrollPosition) {
		int before = Renderer.GetTextUsedCellCount();
		Label(content, rect, out var bounds);
		if (bounds.height < rect.height) {
			scrollPosition = 0;
			return;
		}
		scrollPosition = scrollPosition.Clamp(0, bounds.height - rect.height + Unify(content.CharSize * 2));
		int after = Renderer.GetTextUsedCellCount();
		if (before == after) return;
		if (Renderer.GetTextCells(out var cells, out int count)) {
			for (int i = before; i < after && i < count; i++) {
				var cell = cells[i];
				cell.Y += scrollPosition;
			}
		}
		Renderer.ClampTextCells(rect, before, after);
	}


	// Button
	public static bool Button (IRect rect, string label, int z, int charSize = -1, bool enable = true) => Button(rect, label, out _, z, charSize, enable);
	public static bool Button (IRect rect, string label, int z, Color32 labelTint, int charSize = -1, bool enable = true) => Button(rect, label, out _, z, labelTint, charSize, enable);
	public static bool Button (IRect rect, int sprite, string label, int z, Color32 buttonTint, Color32 labelTint, int charSize = -1, bool enable = true) => Button(rect, sprite, label, out _, z, buttonTint, labelTint, charSize, enable);
	public static bool Button (IRect rect, string label, out IRect labelBounds, int z, int charSize = -1, bool enable = true) => Button(rect, label, out labelBounds, z, Color32.WHITE, charSize, enable);
	public static bool Button (IRect rect, string label, out IRect labelBounds, int z, Color32 labelTint, int charSize = -1, bool enable = true) {
		charSize = charSize < 0 ? ReverseUnify(rect.height / 2) : charSize;
		Label(TextContent.Get(label, labelTint, charSize), rect, out labelBounds);
		return Button(rect, 0, Const.PIXEL, 0, 0, 0, 0, z, Color32.WHITE_12, default, enable);
	}
	public static bool Button (IRect rect, int sprite, string label, out IRect labelBounds, int z, Color32 buttonTint, Color32 labelTint, int charSize = -1, bool enable = true) {
		charSize = charSize < 0 ? ReverseUnify(rect.height / 2) : charSize;
		Label(TextContent.Get(label, labelTint, charSize), rect, out labelBounds);
		return Button(rect, sprite, sprite, sprite, 0, 0, 0, z, buttonTint, default, enable);
	}
	public static bool Button (IRect rect, int sprite, int spriteHover, int spriteDown, int icon, int buttonBorder, int iconPadding, int z, bool enable = true) => Button(rect, sprite, spriteHover, spriteDown, icon, buttonBorder, iconPadding, z, Color32.WHITE, Color32.WHITE, enable);
	public static bool Button (IRect rect, int sprite, int spriteHover, int spriteDown, int icon, int buttonBorder, int iconPadding, int z, Color32 buttonTint, Color32 iconTint, bool enable = true) {
		bool hover = rect.MouseInside();
		bool down = hover && enable && Input.MouseLeftButton;
		buttonTint.a = (byte)(enable ? buttonTint.a : buttonTint.a / 2);
		iconTint.a = (byte)(enable ? iconTint.a : iconTint.a / 2);
		// Button
		int spriteID = down ? spriteDown : hover ? spriteHover : sprite;
		if (spriteID != 0) {
			if (buttonBorder > 0) {
				Renderer.Draw_9Slice(
					spriteID, rect, buttonBorder, buttonBorder, buttonBorder, buttonBorder, buttonTint, z
				);
			} else {
				Renderer.Draw_9Slice(spriteID, rect, buttonTint, z);
			}
		}
		// Icon
		if (icon != 0 && Renderer.TryGetSprite(icon, out var iconSprite)) {
			Renderer.Draw(
				iconSprite,
				rect.Shrink(iconPadding).Fit(iconSprite),
				iconTint, z + 1
			);
		}
		// Cursor
		if (enable) Cursor.SetCursorAsHand(rect);
		// Click
		if (enable && hover && Input.MouseLeftButtonDown) {
			Input.UseMouseKey(0);
			Input.UseGameKey(Gamekey.Action);
			return true;
		}
		return false;
	}


	// Gizmos
	public static Cell DrawLine (int fromX, int fromY, int toX, int toY, int thickness = 8, int z = int.MinValue) => DrawLine(fromX, fromY, toX, toY, thickness, Color32.WHITE, z);
	public static Cell DrawLine (int fromX, int fromY, int toX, int toY, int thickness, Color32 tint, int z = int.MinValue) => Renderer.Draw(
		Const.PIXEL, fromX, fromY, 500, 0,
		-Float2.SignedAngle(Float2.up, new Float2(toX - fromX, toY - fromY)).RoundToInt(),
		thickness, Util.DistanceInt(fromX, fromY, toX, toY),
		tint, z
	);


	// Text Field
	public static string TextField (int controlID, IRect rect, string text) => TextField(controlID, rect, InputLabel.SetText(text, ReverseUnify(rect.height / 2)), out _, out _);
	public static string TextField (int controlID, IRect rect, string text, out bool changed, out bool confirm) => TextField(controlID, rect, InputLabel.SetText(text, ReverseUnify(rect.height / 2)), out changed, out confirm);
	public static string TextField (int controlID, IRect rect, TextContent text) => TextField(controlID, rect, text, out _, out _);
	public static string TextField (int controlID, IRect rect, TextContent text, out bool changed, out bool confirm) {

		changed = false;
		confirm = false;
		bool startTyping = false;
		bool mouseDownPosInRect = rect.Contains(Input.MouseLeftDownGlobalPosition);
		bool mouseDragging = Input.MouseLeftButton && mouseDownPosInRect;
		bool inCamera = rect.Overlaps(Renderer.CameraRect);

		Cursor.SetCursorAsBeam(rect);

		if (!inCamera && TypingTextFieldID == controlID) TypingTextFieldID = 0;

		// Start Typing
		if (inCamera && Input.MouseLeftButtonDown && mouseDownPosInRect) {
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
			if (Input.KeyboardUp(KeyboardKey.Escape)) {
				beamIndex = BeamIndex = 0;
				confirm = true;
				CancelTyping();
				Input.UseKeyboardKey(KeyboardKey.Escape);
				Input.UseGameKey(Gamekey.Start);
			}

			// Move Beam
			if (Input.KeyboardDownGUI(KeyboardKey.LeftArrow)) {
				if (beamLength == 0) {
					beamIndex = BeamIndex = beamIndex - 1;
				} else if (beamLength < 0) {
					beamIndex = BeamIndex = beamIndex + beamLength;
				}
				beamLength = BeamLength = 0;
				BeamBlinkFrame = Game.PauselessFrame;
			}
			if (Input.KeyboardDownGUI(KeyboardKey.RightArrow)) {
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
					case Const.BACKSPACE_SIGN:
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
						Game.SetClipboardText(text.Text[beamStart..beamEnd]);
						if (c == Const.CONTROL_CUT) {
							RemoveSelection();
							changed = true;
						}
						break;
					case Const.CONTROL_PASTE:
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
			if (Input.KeyboardDownGUI(KeyboardKey.Delete)) {
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
				Input.UseKeyboardKey(KeyboardKey.Delete);
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
		int startCellIndex = Renderer.GetTextUsedCellCount();
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
			beamCell = Renderer.Draw(Const.PIXEL, beamRect, Color32.WHITE, int.MaxValue);
		}
		int endCellIndex = Renderer.GetTextUsedCellCount();

		if (startCellIndex != endCellIndex && Renderer.GetTextCells(out var cells, out int count)) {

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
						cell.Color = Color32.CLEAR;
					} else {
						cell.Color = new Color32(255, 255, 255, 96);
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
				Renderer.Draw(Const.PIXEL, selectionRect, Color32.ORANGE, int.MaxValue - 1);
			}

			if (typing && (startTyping || mouseDragging)) {
				int mouseBeamIndex = 0;
				int mouseX = Input.MouseGlobalPosition.x;
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
	public static int ScrollBar (
		int controlID, IRect contentRect, int z, int positionRow, int totalSize, int pageSize, int barSpriteId = Const.PIXEL
	) {
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
		bool focusingBar = DraggingScrollbarID == controlID;
		bool hoveringBar = barRect.MouseInside();

		Renderer.Draw(
			barSpriteId,
			barRect,
			hoveringBar || (focusingBar && ScrollBarMouseDownPos.HasValue) ? Color32.GREY_128 : Color32.GREY_64,
			z
		);

		// Dragging
		if (focusingBar && ScrollBarMouseDownPos.HasValue) {
			int mouseY = Input.MouseGlobalPosition.y;
			int mouseDownY = ScrollBarMouseDownPos.Value.x;
			int scrollDownY = ScrollBarMouseDownPos.Value.y;
			positionRow = scrollDownY + (mouseDownY - mouseY) * totalSize / contentRect.height;
		}

		// Mouse Down
		if (Input.MouseLeftButtonDown) {
			if (hoveringBar) {
				// Start Drag
				ScrollBarMouseDownPos = new Int2(
					Input.MouseGlobalPosition.y, positionRow
				);
				DraggingScrollbarID = controlID;
			} else if (contentRect.MouseInside()) {
				// Jump on Click
				int mouseY = Input.MouseGlobalPosition.y;
				positionRow = Util.RemapUnclamped(
					contentRect.y, contentRect.yMax,
					totalSize - pageSize / 2, -pageSize / 2,
					mouseY
				);
				ScrollBarMouseDownPos = new Int2(mouseY, positionRow);
				DraggingScrollbarID = controlID;
			}
		}

		return positionRow.Clamp(0, totalSize - pageSize);
	}


	// Highlight
	public static void HighlightCursor (int spriteID, IRect rect, int z) => HighlightCursor(spriteID, rect, z, Color32.GREEN);
	public static void HighlightCursor (int spriteID, IRect rect, int z, Color32 color) {
		int border = Unify(4);
		int thickness = Unify(8);
		Renderer.Draw_9Slice(
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
				case Const.BACKSPACE_SIGN:
				case Const.RETURN_SIGN:
				case Const.CONTROL_CUT:
				case Const.CONTROL_COPY:
				case Const.CONTROL_PASTE:
					break;
				default:
					return;
			}
		}
		TypingBuilder.Append(c);
	}


	#endregion




}