using System.Collections;
using System.Collections.Generic;
using System.Text;
using AngeliA.Internal;

namespace AngeliA.Framework;

public static class GUI {




	#region --- VAR ---


	// Const
	private const int MAX_INPUT_CHAR = 256;

	// Api
	public static bool IsTyping => TypingTextFieldID != 0;
	public static bool Enable { get; set; } = true;
	public static bool UnifyBasedOnMonitor { get; set; } = false;
	public static Color32 Color { get; set; } = Color32.WHITE;
	public static Color32 BodyColor { get; set; } = Color32.WHITE;
	public static Color32 ContentColor { get; set; } = Color32.WHITE;
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

	// Data
	private static readonly StringBuilder TypingBuilder = new();
	private static int BeamIndex = 0;
	private static int BeamLength = 0;
	private static int BeamBlinkFrame = int.MinValue;
	private static Int2? ScrollBarMouseDownPos = null;
	private static int DraggingScrollbarID = 0;


	#endregion




	#region --- API ---


	[OnGameUpdate(1023)]
	internal static void Update () {
		if (TypingTextFieldID != 0 && Input.AnyKeyHolding) {
			Input.UseAllHoldingKeys(ignoreMouse: true);
			Input.UnuseKeyboardKey(KeyboardKey.LeftArrow);
			Input.UnuseKeyboardKey(KeyboardKey.RightArrow);
			Input.UnuseKeyboardKey(KeyboardKey.Delete);
			Input.UnuseKeyboardKey(KeyboardKey.Escape);
		}
		if (!Input.MouseLeftButtonHolding) ScrollBarMouseDownPos = null;
	}


	[OnGameUpdateLater(4096)]
	internal static void LateUpdate () {
		if (TypingBuilder.Length > 0) TypingBuilder.Clear();
	}


	[OnGameUpdate(-4096)]
	internal static void Reset () {
		Enable = true;
		Color = Color32.WHITE;
		BodyColor = Color32.WHITE;
		ContentColor = Color32.WHITE;
	}


	// Unify
	public static int Unify (int value) => UnifyBasedOnMonitor ? UnifyMonitor(value) : (value * Renderer.CameraRect.height / 1000f).RoundToInt();
	public static int Unify (float value) => UnifyBasedOnMonitor ? UnifyMonitor(value) : (value * Renderer.CameraRect.height / 1000f).RoundToInt();
	private static int UnifyMonitor (int value) => (value / 1000f * Renderer.CameraRect.height * Game.MonitorHeight / Game.ScreenHeight).RoundToInt();
	private static int UnifyMonitor (float value) => (value / 1000f * Renderer.CameraRect.height * Game.MonitorHeight / Game.ScreenHeight).RoundToInt();


	// Typing
	public static void StartTyping (int controlID) {
		TypingTextFieldID = controlID;
		BeamIndex = 0;
		BeamLength = 0;
		BeamBlinkFrame = Game.PauselessFrame;
	}

	public static void CancelTyping () {
		TypingTextFieldID = 0;
		TypingBuilder.Clear();
		BeamIndex = 0;
		BeamLength = 0;
	}


	// Label
	public static void Label (IRect rect, string text, GUIStyle style = null) => LabelLogic(rect, text, null, style, GUIState.Normal, -1, 0, false, out _, out _, out _);
	public static void Label (IRect rect, char[] text, GUIStyle style = null) => LabelLogic(rect, "", text, style, GUIState.Normal, -1, 0, false, out _, out _, out _);
	public static void Label (IRect rect, string text, out IRect bounds, GUIStyle style = null) => LabelLogic(rect, text, null, style, GUIState.Normal, -1, 0, false, out bounds, out _, out _);
	public static void Label (IRect rect, char[] text, out IRect bounds, GUIStyle style = null) => LabelLogic(rect, "", text, style, GUIState.Normal, -1, 0, false, out bounds, out _, out _);
	public static void Label (IRect rect, string text, int startIndex, bool drawInvisibleChar, out IRect bounds, out int endIndex, GUIStyle style = null) => LabelLogic(rect, text, null, style, GUIState.Normal, -1, startIndex, drawInvisibleChar, out bounds, out _, out endIndex);
	public static void Label (IRect rect, string text, int beamIndex, int startIndex, bool drawInvisibleChar, out IRect bounds, out IRect beamRect, out int endIndex, GUIStyle style = null) => LabelLogic(rect, text, null, style, GUIState.Normal, beamIndex, startIndex, drawInvisibleChar, out bounds, out beamRect, out endIndex);
	private static void LabelLogic (IRect rect, string text, char[] chars, GUIStyle style, GUIState state, int beamIndex, int startIndex, bool drawInvisibleChar, out IRect bounds, out IRect beamRect, out int endIndex) {
		if (!Renderer.TextReady) {
			endIndex = startIndex;
			bounds = rect;
			beamRect = new IRect(rect.x, rect.y, 1, rect.height);
			return;
		}
		// Draw
		style ??= GUISkin.Label;
		Renderer.GetTextCells(out var cells, out int cellCount);
		TextUtilInternal.DrawLabelInternal(
			Renderer.RequireCharForPool, Renderer.DrawChar, UnifyBasedOnMonitor ? UnifyMonitor : Unify,
			cellCount, cells, style,
			GetContentRect(rect, style, state), text, chars, Color * ContentColor * style.GetContentColor(state), beamIndex, startIndex, drawInvisibleChar,
			out bounds, out beamRect, out endIndex
		);
	}


	// Label Extra
	public static int ScrollLabel (string text, IRect rect, int scrollPosition, GUIStyle style) {
		style ??= GUISkin.Label;
		int before = Renderer.GetTextUsedCellCount();
		LabelLogic(rect, text, null, style, GUIState.Normal, -1, 0, false, out var bounds, out _, out _);
		if (bounds.height < rect.height) {
			scrollPosition = 0;
			return scrollPosition;
		}
		scrollPosition = scrollPosition.Clamp(0, bounds.height - rect.height + Unify(style.CharSize * 2));
		int after = Renderer.GetTextUsedCellCount();
		if (before == after) return scrollPosition;
		// Clamp
		if (Renderer.GetTextCells(out var cells, out int count)) {
			for (int i = before; i < after && i < count; i++) {
				var cell = cells[i];
				cell.Y += scrollPosition;
			}
		}
		Renderer.ClampTextCells(rect, before, after);
		return scrollPosition;
	}

	public static void BackgroundLabel (IRect rect, string text, Color32 backgroundColor, int backgroundPadding = 0, GUIStyle style = null) => BackgroundLabel(rect, text, backgroundColor, out _, backgroundPadding, style);
	public static void BackgroundLabel (IRect rect, string text, Color32 backgroundColor, out IRect bounds, int backgroundPadding = 0, GUIStyle style = null) {
		LabelLogic(rect, text, null, style, GUIState.Normal, -1, 0, false, out bounds, out _, out _);
		Renderer.Draw(Const.PIXEL, bounds.Expand(backgroundPadding), Color * BodyColor * backgroundColor, z: 0);
	}

	public static void ShadowLabel (IRect rect, string text, int shadowDistance = 3, GUIStyle style = null) {
		var oldC = ContentColor;
		ContentColor = Color32.GREY_20;
		LabelLogic(rect.Shift(0, -UnifyMonitor(shadowDistance)), text, null, style, GUIState.Normal, -1, 0, false, out _, out _, out _);
		ContentColor = oldC;
		LabelLogic(rect, text, null, style, GUIState.Normal, -1, 0, false, out _, out _, out _);
	}


	// Style
	public static void DrawStyleBody (IRect rect, GUIStyle style, GUIState state) => DrawStyleBody(rect, style, state, Color32.WHITE);
	public static void DrawStyleBody (IRect rect, GUIStyle style, GUIState state, Color32 tint) {
		int sprite = style.GetBodySprite(state);
		if (sprite == 0 || !Renderer.TryGetSprite(sprite, out var _sprite)) return;
		var color = tint * Color * BodyColor * style.GetBodyColor(state);
		if (color.a == 0) return;
		var border = style.BodyBorder ?? _sprite.GlobalBorder;
		if (UnifyBasedOnMonitor) {
			border.left = border.left * Game.MonitorHeight / Game.ScreenHeight;
			border.right = border.right * Game.MonitorHeight / Game.ScreenHeight;
			border.down = border.down * Game.MonitorHeight / Game.ScreenHeight;
			border.up = border.up * Game.MonitorHeight / Game.ScreenHeight;
		}
		Renderer.Draw_9Slice(_sprite, rect, border.left, border.right, border.down, border.up, color);
	}

	public static void DrawStyleContent (IRect rect, int sprite, GUIStyle style, GUIState state, bool ignoreSlice = false) {
		if (!Renderer.TryGetSprite(sprite, out var _sprite)) return;
		var color = Color * ContentColor * style.GetContentColor(state);
		if (color.a == 0) return;
		rect = GetContentRect(rect, style, state);
		if (ignoreSlice) {
			Renderer.Draw(_sprite, rect, color);
		} else {
			var border = style.BodyBorder ?? _sprite.GlobalBorder;
			if (UnifyBasedOnMonitor) {
				border.left = border.left * Game.MonitorHeight / Game.ScreenHeight;
				border.right = border.right * Game.MonitorHeight / Game.ScreenHeight;
				border.down = border.down * Game.MonitorHeight / Game.ScreenHeight;
				border.up = border.up * Game.MonitorHeight / Game.ScreenHeight;
			}
			Renderer.Draw_9Slice(_sprite, rect, border.left, border.right, border.down, border.up, color);
		}
	}


	// Button
	public static bool DarkButton (IRect rect, string label) => Button(rect, label, GUISkin.DarkButton);
	public static bool DarkButton (IRect rect, int icon) => Button(rect, icon, GUISkin.DarkButton);
	public static bool Button (IRect rect, string label, GUIStyle style = null) {
		style ??= GUISkin.Button;
		bool result = BlankButton(rect, out var state);
		DrawStyleBody(rect, style, state);
		// Label
		if (!string.IsNullOrEmpty(label)) {
			LabelLogic(rect, label, null, style, state, -1, 0, false, out _, out _, out _);
		}
		return result;
	}
	public static bool Button (IRect rect, int icon, GUIStyle style = null) {
		style ??= GUISkin.Button;
		bool result = BlankButton(rect, out var state);
		DrawStyleBody(rect, style, state);
		// Icon
		Icon(rect, icon, style, state);
		return result;
	}
	public static bool BlankButton (IRect rect, out GUIState state) {
		state = GUIState.Normal;
		if (!Enable) {
			state = GUIState.Disable;
			return false;
		}
		bool hover = rect.MouseInside();
		// Cursor
		Cursor.SetCursorAsHand(rect);
		// Click
		if (hover) {
			state = Input.MouseLeftButtonHolding ? GUIState.Press : GUIState.Hover;
			return Input.MouseLeftButtonDown;
		}
		return false;
	}


	// Toggle
	public static bool IconToggle (IRect rect, bool isOn, int icon, GUIStyle markStyle = null, GUIStyle iconStyle = null) {
		markStyle ??= GUISkin.GreenPixel;
		isOn = BlankToggle(rect, isOn, out var state);
		// Mark
		if (isOn) {
			DrawStyleBody(GetContentRect(rect, markStyle, state), markStyle, state);
		}
		// Icon
		if (iconStyle != null) {
			Icon(rect, icon, iconStyle, state);
		} else {
			Icon(rect, icon);
		}
		return isOn;
	}
	public static bool ToggleLeft (IRect rect, bool isOn, string label, GUIStyle bodyStyle = null, GUIStyle labelStyle = null, GUIStyle markStyle = null) {
		bodyStyle ??= GUISkin.Toggle;
		labelStyle ??= GUISkin.Label;
		markStyle ??= GUISkin.ToggleMark;
		var boxRect = rect.EdgeInside(Direction4.Left, rect.height);
		isOn = BlankToggle(boxRect, isOn, out var state);
		DrawStyleBody(boxRect, bodyStyle, state);
		Label(rect.Shrink(rect.height * 13 / 10, 0, 0, 0), label, labelStyle);
		if (isOn) {
			DrawStyleBody(GetContentRect(boxRect, markStyle, state), markStyle, state);
		}
		return isOn;
	}
	public static bool ToggleButton (IRect rect, bool isOn, string label, GUIStyle bodyStyle = null) {
		bodyStyle ??= GUISkin.DarkButton;
		isOn = BlankToggle(rect, isOn, out var state);
		DrawStyleBody(rect, bodyStyle, state, isOn ? Color32.GREY_160 : Color32.WHITE);
		LabelLogic(rect, label, null, bodyStyle, state, -1, 0, false, out _, out _, out _);
		return isOn;
	}
	public static bool Toggle (IRect rect, bool isOn, GUIStyle bodyStyle = null, GUIStyle markStyle = null) {
		bodyStyle ??= GUISkin.Toggle;
		markStyle ??= GUISkin.ToggleMark;
		isOn = BlankToggle(rect, isOn, out var state);
		DrawStyleBody(rect, bodyStyle, state);
		if (isOn) {
			DrawStyleBody(GetContentRect(rect, markStyle, state), markStyle, state);
		}
		return isOn;
	}
	public static bool BlankToggle (IRect rect, bool isOn, out GUIState state) {
		if (BlankButton(rect, out state)) isOn = !isOn;
		state =
			state == GUIState.Disable ? GUIState.Disable :
			isOn ? GUIState.Press :
			state == GUIState.Press ? GUIState.Hover :
			state;
		return isOn;
	}


	// Icon
	public static void Icon (IRect rect, int sprite) => Icon(rect, sprite, null, default);
	public static void Icon (IRect rect, int sprite, GUIStyle style, GUIState state) {
		if (!Renderer.TryGetSprite(sprite, out var icon)) return;
		if (style != null) {
			DrawStyleContent(rect.Fit(icon), sprite, style, state, ignoreSlice: true);
		} else {
			Renderer.Draw(icon, rect.Fit(icon), Color * ContentColor);
		}
	}


	// Text Field
	public static string InputField (int controlID, IRect rect, string text, GUIStyle bodyStyle = null, GUIStyle selectionStyle = null) => InputField(controlID, rect, text, out _, out _, bodyStyle, selectionStyle);
	public static string InputField (int controlID, IRect rect, string text, out bool changed, out bool confirm, GUIStyle bodyStyle = null, GUIStyle selectionStyle = null) {

		bodyStyle ??= GUISkin.InputField;
		selectionStyle ??= GUISkin.GreenPixel;
		changed = false;
		confirm = false;
		bool startTyping = false;
		bool mouseDownPosInRect = rect.Contains(Input.MouseLeftDownGlobalPosition);
		bool mouseDragging = Input.MouseLeftButtonHolding && mouseDownPosInRect;
		bool inCamera = rect.Overlaps(Renderer.CameraRect);
		var state =
			(!Enable || !inCamera) ? GUIState.Disable :
			Input.MouseLeftButtonHolding && mouseDownPosInRect ? GUIState.Press :
			Input.MouseLeftButtonHolding && rect.MouseInside() ? GUIState.Hover :
			GUIState.Normal;

		Cursor.SetCursorAsBeam(rect);

		DrawStyleBody(rect, bodyStyle, state);

		if ((!inCamera || !Enable) && TypingTextFieldID == controlID) TypingTextFieldID = 0;

		// Start Typing
		if (Enable && inCamera && Input.MouseLeftButtonDown && mouseDownPosInRect) {
			TypingTextFieldID = controlID;
			BeamBlinkFrame = Game.PauselessFrame;
			startTyping = true;
			mouseDragging = false;
		}

		// Typing 
		bool typing = Enable && TypingTextFieldID == controlID;
		int beamIndex = typing ? BeamIndex : 0;
		int beamLength = typing ? BeamLength : 0;

		// Cancel on Click Outside
		if (typing && Input.MouseLeftButtonDown && !rect.MouseInside()) {
			typing = false;
			confirm = true;
			TypingTextFieldID = 0;
			TypingBuilder.Clear();
			BeamIndex = beamIndex = 0;
			BeamLength = beamLength = 0;
		}

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

			beamIndex = BeamIndex = beamIndex.Clamp(0, text.Length);
			beamLength = BeamLength = beamLength.Clamp(-beamIndex, text.Length - beamIndex);

			for (int i = 0; i < TypingBuilder.Length; i++) {
				char c = TypingBuilder[i];
				switch (c) {
					case Const.BACKSPACE_SIGN:
						// Backspace
						if (beamLength == 0) {
							int removeIndex = beamIndex - 1;
							if (removeIndex >= 0 && removeIndex < text.Length) {
								text = text.Remove(removeIndex, 1);
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
						// Copy/Cut
						if (beamLength == 0) break;
						int beamStart = Util.Min(beamIndex, beamIndex + beamLength);
						int beamEnd = Util.Max(beamIndex, beamIndex + beamLength);
						Game.SetClipboardText(text[beamStart..beamEnd]);
						if (c == Const.CONTROL_CUT) {
							RemoveSelection();
							changed = true;
						}
						break;
					case Const.CONTROL_PASTE:
						// Paste
						string clipboardText = Game.GetClipboardText();
						if (string.IsNullOrEmpty(clipboardText)) break;
						if (beamLength != 0) RemoveSelection();
						text = text.Insert(beamIndex, clipboardText);
						beamIndex = BeamIndex = beamIndex + clipboardText.Length;
						changed = true;
						break;
					default:
						if (text.Length >= MAX_INPUT_CHAR) break;
						// Append Char
						if (beamLength != 0) RemoveSelection();
						text = text.Insert(beamIndex, c.ToString());
						beamIndex = BeamIndex = beamIndex + 1;
						changed = true;
						break;
				}
			}

			// Delete
			if (Input.KeyboardDownGUI(KeyboardKey.Delete)) {
				Input.UseKeyboardKey(KeyboardKey.Delete);
				int removeIndex = beamIndex;
				if (removeIndex >= 0 && removeIndex < text.Length) {
					if (beamLength == 0) {
						// Delete One Char
						text = text.Remove(removeIndex, 1);
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
				text = text.Remove(newBeamIndex, beamLength.Abs());
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
		if (!string.IsNullOrEmpty(text)) {
			LabelLogic(labelRect, text, null, bodyStyle, state, beamIndex, 0, false, out _, out beamRect, out _);
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
				DrawStyleBody(selectionRect, selectionStyle, GUIState.Normal);
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
						mouseBeamIndex = text.Length;
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
		if (text.Length > MAX_INPUT_CHAR) {
			text = text[..MAX_INPUT_CHAR];
		}

		return text;
	}


	// Scrollbar
	public static int ScrollBar (int controlID, IRect contentRect, int positionRow, int totalSize, int pageSize, GUIStyle style = null) {
		if (pageSize >= totalSize) return 0;

		style ??= GUISkin.Scrollbar;

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
		bool dragging = focusingBar && ScrollBarMouseDownPos.HasValue;

		var state =
			!Enable ? GUIState.Disable :
			dragging ? GUIState.Press :
			hoveringBar ? GUIState.Hover :
			GUIState.Normal;
		DrawStyleBody(barRect, style, state);

		// Dragging
		if (dragging) {
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
	public static void HighlightCursor (int spriteID, IRect rect) => HighlightCursor(spriteID, rect, Color32.GREEN);
	public static void HighlightCursor (int spriteID, IRect rect, Color32 color) {
		int border = Unify(4);
		int thickness = Unify(8);
		Renderer.Draw_9Slice(
			spriteID, rect.Expand(Game.GlobalFrame.PingPong(thickness)),
			border, border, border, border,
			Color * BodyColor * color
		);
	}


	// Misc
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




	#region --- LGC ---


	private static IRect GetContentRect (IRect rect, GUIStyle style, GUIState state) {
		// Border
		if (style.ContentBorder.HasValue) {
			var border = style.ContentBorder.Value;
			if (UnifyBasedOnMonitor) {
				border.left = border.left * Game.MonitorHeight / Game.ScreenHeight;
				border.right = border.right * Game.MonitorHeight / Game.ScreenHeight;
				border.down = border.down * Game.MonitorHeight / Game.ScreenHeight;
				border.up = border.up * Game.MonitorHeight / Game.ScreenHeight;
			}
			rect = rect.Shrink(border);
		}
		// Shift
		var shift = style.GetContentShift(state);
		if (UnifyBasedOnMonitor) {
			shift.x = shift.x * Game.MonitorHeight / Game.ScreenHeight;
			shift.y = shift.y * Game.MonitorHeight / Game.ScreenHeight;
		}
		rect = rect.Shift(shift.x, shift.y);
		// Final
		return rect;
	}


	#endregion




}