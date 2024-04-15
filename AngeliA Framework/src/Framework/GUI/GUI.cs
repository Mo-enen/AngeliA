using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AngeliA;

public static class GUI {




	#region --- VAR ---


	// Const
	private const int MAX_INPUT_CHAR = 256;

	// Api
	public static bool IsTyping => TypingTextFieldID != 0;
	public static bool Enable { get; set; } = true;
	public static bool UnifyBasedOnMonitor { get; set; } = false;
	public static int TypingTextFieldID {
		get => _TypingTextFieldID;
		private set {
			if (_TypingTextFieldID != value) {
				_TypingTextFieldID = value;
				Game.SetImeCompositionMode(value != 0);
			}
		}
	}
	public static Color32 Color { get; set; } = Color32.WHITE;
	public static Color32 BodyColor { get; set; } = Color32.WHITE;
	public static Color32 ContentColor { get; set; } = Color32.WHITE;
	public static int LabelWidth { get; set; } = 196;

	// Data
	private static readonly StringBuilder TypingBuilder = new();
	private static int BeamIndex = 0;
	private static int BeamLength = 0;
	private static int BeamBlinkFrame = int.MinValue;
	private static int DraggingScrollbarID = 0;
	private static int InvokeTypingStartID = 0;
	private static int _TypingTextFieldID = 0;
	private static int TypingTextFieldUpdateFrame = -1;
	private static Int2? ScrollBarMouseDownPos = null;


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
		if (TypingTextFieldID != 0 && Game.PauselessFrame > TypingTextFieldUpdateFrame) {
			CancelTyping();
		}
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
		InvokeTypingStartID = controlID;
	}

	public static void CancelTyping () {
		TypingTextFieldID = 0;
		TypingBuilder.Clear();
		BeamIndex = 0;
		BeamLength = 0;
		InvokeTypingStartID = 0;
	}


	// Label
	public static void Label (IRect rect, string text, GUIStyle style = null) => LabelLogic(rect, text, null, style, Enable ? GUIState.Normal : GUIState.Disable, -1, 0, false, out _, out _, out _);
	public static void Label (IRect rect, char[] text, GUIStyle style = null) => LabelLogic(rect, "", text, style, Enable ? GUIState.Normal : GUIState.Disable, -1, 0, false, out _, out _, out _);
	public static void Label (IRect rect, string text, out IRect bounds, GUIStyle style = null) => LabelLogic(rect, text, null, style, Enable ? GUIState.Normal : GUIState.Disable, -1, 0, false, out bounds, out _, out _);
	public static void Label (IRect rect, char[] text, out IRect bounds, GUIStyle style = null) => LabelLogic(rect, "", text, style, Enable ? GUIState.Normal : GUIState.Disable, -1, 0, false, out bounds, out _, out _);
	public static void Label (IRect rect, string text, int startIndex, bool drawInvisibleChar, out IRect bounds, out int endIndex, GUIStyle style = null) => LabelLogic(rect, text, null, style, Enable ? GUIState.Normal : GUIState.Disable, -1, startIndex, drawInvisibleChar, out bounds, out _, out endIndex);
	public static void Label (IRect rect, string text, int beamIndex, int startIndex, bool drawInvisibleChar, out IRect bounds, out IRect beamRect, out int endIndex, GUIStyle style = null) => LabelLogic(rect, text, null, style, Enable ? GUIState.Normal : GUIState.Disable, beamIndex, startIndex, drawInvisibleChar, out bounds, out beamRect, out endIndex);
	private static void LabelLogic (IRect rect, string text, char[] chars, GUIStyle style, GUIState state, int beamIndex, int startIndex, bool drawInvisibleChar, out IRect bounds, out IRect beamRect, out int endIndex) {

		if (!Renderer.TextReady) {
			endIndex = startIndex;
			bounds = rect;
			beamRect = new IRect(rect.x, rect.y, 1, rect.height);
			return;
		}

		// Draw
		style ??= GUISkin.Label;
		Renderer.GetTextCells(out var textCells, out int textCountInLayer);
		rect = GetContentRect(rect, style, state);
		var color = Color * ContentColor * style.GetContentColor(state);

		// Logic
		endIndex = startIndex;
		bounds = rect;
		beamRect = new IRect(rect.x, rect.y, 1, rect.height);

		bool fromString = chars == null;
		int count = fromString ? text.Length : chars.Length;
		int charSize = style.CharSize < 0 ? rect.height / 2 : Unify(style.CharSize);
		int lineSpace = Unify(style.LineSpace);
		int charSpace = Unify(style.CharSpace);
		var alignment = style.Alignment;
		var wrap = style.Wrap;
		bool clip = style.Clip;
		bool beamEnd = beamIndex >= count;
		Renderer.RequireCharForPool(' ', out var emptyCharSprite);

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
		for (int i = startIndex; i < count; i++) {

			char c = fromString ? text[i] : chars[i];
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
			if (!Renderer.RequireCharForPool(c, out var sprite)) goto CONTINUE;

			int realCharSize = (sprite.Advance * charSize).RoundToInt();

			// Wrap Check for Word
			if (wrap == WrapMode.WordWrap && i >= nextWrapCheckIndex && !IsLineBreakingChar(c)) {
				if (!WordEnoughToFit(
					text, chars, charSize, charSpace, i,
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
			var cell = Renderer.DrawChar(sprite, x, y, charSize, charSize, color) ?? Cell.EMPTY;
			if (cell != null && cell.TextSprite != null) textCountInLayer++;

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
				int cellCount = textCountInLayer.Clamp(0, textCells.Length) - startCellIndex - 1;
				int textCount = i - startIndex;
				int addCount = textCount - cellCount;
				for (int add = 0; add < addCount; add++) {
					var _cell = Renderer.DrawChar(emptyCharSprite, 0, 0, 0, 0, color);
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

	public static void BackgroundLabel (IRect rect, string text, Color32 backgroundColor, int backgroundPadding = 0, bool forceInside = false, GUIStyle style = null) => BackgroundLabel(rect, text, backgroundColor, out _, backgroundPadding, forceInside, style);
	public static void BackgroundLabel (IRect rect, string text, Color32 backgroundColor, out IRect bounds, int backgroundPadding = 0, bool forceInside = false, GUIStyle style = null) {
		int startIndex = Renderer.GetTextUsedCellCount();
		LabelLogic(rect, text, null, style, GUIState.Normal, -1, 0, false, out bounds, out _, out _);
		if (forceInside) {
			bounds = rect;
			Renderer.ClampTextCells(bounds, startIndex);
		}
		bounds = bounds.Expand(backgroundPadding);
		Renderer.DrawPixel(bounds, Color * BodyColor * backgroundColor, z: 0);
	}

	public static void BackgroundLabel (IRect rect, char[] chars, Color32 backgroundColor, int backgroundPadding = 0, GUIStyle style = null) => BackgroundLabel(rect, chars, backgroundColor, out _, backgroundPadding, style);
	public static void BackgroundLabel (IRect rect, char[] chars, Color32 backgroundColor, out IRect bounds, int backgroundPadding = 0, GUIStyle style = null) {
		LabelLogic(rect, null, chars, style, GUIState.Normal, -1, 0, false, out bounds, out _, out _);
		bounds = bounds.Expand(backgroundPadding);
		Renderer.DrawPixel(bounds, Color * BodyColor * backgroundColor, z: 0);
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
		Icon(rect, icon, style, state);
		return result;
	}
	public static bool BlankButton (IRect rect, out GUIState state) {
		state = GUIState.Normal;
		if (!Enable) {
			state = GUIState.Disable;
			return false;
		}
		bool hover = !Input.IgnoringMouseInput && rect.MouseInside();
		// Cursor
		if (hover) Cursor.SetCursorAsHand();
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
	public static bool ToggleButton (IRect rect, bool isOn, int icon, GUIStyle style = null) {
		style ??= GUISkin.DarkButton;
		isOn = BlankToggle(rect, isOn, out var state);
		DrawStyleBody(rect, style, state, isOn ? Color32.GREY_160 : Color32.WHITE);
		Icon(rect, icon, style, state);
		return isOn;
	}
	public static bool Toggle (IRect rect, bool isOn, GUIStyle bodyStyle = null, GUIStyle markStyle = null) =>
		Toggle(rect, isOn, null, bodyStyle, markStyle, null);
	public static bool Toggle (IRect rect, bool isOn, string label, GUIStyle bodyStyle = null, GUIStyle markStyle = null, GUIStyle labelStyle = null) {
		bodyStyle ??= GUISkin.Toggle;
		markStyle ??= GUISkin.ToggleMark;
		if (label != null) {
			labelStyle ??= GUISkin.Label;
			int labelWidth = Unify(LabelWidth);
			Label(rect.EdgeInside(Direction4.Left, labelWidth), label, labelStyle);
			rect = rect.Shrink(labelWidth, 0, 0, 0);
		}
		rect.width = rect.height;
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

		bool invokeStart = InvokeTypingStartID == controlID;
		bool startTyping = false;
		bool mouseDownPosInRect = rect.Contains(Input.MouseLeftDownGlobalPosition);
		bool mouseDragging = Input.MouseLeftButtonHolding && mouseDownPosInRect;
		bool inCamera = rect.Shift(
			-Input.MousePositionShift.x, -Input.MousePositionShift.y
		).Overlaps(Renderer.CameraRect);
		var state =
			(!Enable || !inCamera) ? GUIState.Disable :
			Input.MouseLeftButtonHolding && mouseDownPosInRect ? GUIState.Press :
			Input.MouseLeftButtonHolding && rect.MouseInside() ? GUIState.Hover :
			GUIState.Normal;
		if (invokeStart) InvokeTypingStartID = 0;

		Cursor.SetCursorAsBeam(rect);

		DrawStyleBody(rect, bodyStyle, state);

		if ((!inCamera || !Enable) && TypingTextFieldID == controlID) TypingTextFieldID = 0;

		// Start Typing
		if (Enable && inCamera && Input.MouseLeftButtonDown && mouseDownPosInRect) {
			TypingTextFieldID = controlID;
			BeamBlinkFrame = Game.PauselessFrame;
			startTyping = true;
			mouseDragging = false;
			InvokeTypingStartID = 0;
		}
		startTyping = startTyping || invokeStart;

		// Typing 
		bool typing = Enable && TypingTextFieldID == controlID;
		int beamIndex = typing ? BeamIndex : 0;
		int beamLength = typing ? BeamLength : 0;

		// Cancel on Click Outside
		using (Scope.IgnoreInput(ignoreKey: false, ignoreMouse: false)) {
			if (typing && !startTyping && Input.MouseLeftButtonDown && !rect.MouseInside()) {
				typing = false;
				confirm = true;
				TypingTextFieldID = 0;
				TypingBuilder.Clear();
				BeamIndex = beamIndex = 0;
				BeamLength = beamLength = 0;
			}
		}

		TypingTextFieldUpdateFrame = typing ? Game.PauselessFrame : TypingTextFieldUpdateFrame;

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
		var labelRect = rect;
		if (bodyStyle.ContentBorder.HasValue) {
			rect = rect.Shrink(bodyStyle.ContentBorder.Value);
		}
		int beamShrink = rect.height / 12;
		var beamRect = new IRect(
			labelRect.x, labelRect.y + beamShrink, Unify(2), labelRect.height - beamShrink * 2
		);

		// Draw Text
		if (!string.IsNullOrEmpty(text)) {
			LabelLogic(
				labelRect, text, null, bodyStyle, state,
				beamIndex, 0, false, out _, out beamRect, out _
			);
		}

		// Draw Beam
		Cell beamCell = null;
		if (!startTyping && typing && (Game.PauselessFrame - BeamBlinkFrame) % 56 < 28) {
			beamRect.y = labelRect.y + beamShrink;
			beamRect.height = labelRect.height - beamShrink * 2;
			beamCell = Renderer.DrawPixel(beamRect, Color32.WHITE, int.MaxValue);
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

			// Move Beam from Mouse
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
		int barHeight = (int)((long)(contentRect.height * pageSize) / totalSize);
		var barRect = new IRect(
			contentRect.x,
			RemapLarge(
				0, totalSize - pageSize,
				contentRect.yMax - barHeight, contentRect.y,
				positionRow
			),
			contentRect.width,
			barHeight
		);
		static int RemapLarge (int l, int r, int newL, int newR, int t) {
			return l == r ?
				newL :
				newL + (int)((long)(newR - newL) * (t - l) / (r - l));
		}
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


	// Color
	public static ColorF VerticalColorField (ColorF color, IRect rect, string label, GUIStyle labelStyle = null, bool hsv = true, bool alpha = false, bool stepped = true, ColorF? defaultColor = null) => ColorFieldInternal(color, defaultColor, rect, label, labelStyle, hsv, alpha, false, stepped ? 1f / 32f : 0f, stepped ? 1 / 20f : 0f);
	public static ColorF HorizontalColorField (ColorF color, IRect rect, string label, GUIStyle labelStyle = null, bool hsv = true, bool alpha = false, bool stepped = true, ColorF? defaultColor = null) => ColorFieldInternal(color, defaultColor, rect, label, labelStyle, hsv, alpha, true, stepped ? 1f / 32f : 0f, stepped ? 1 / 20f : 0f);
	public static ColorF VerticalColorField (ColorF color, IRect rect, bool hsv = true, bool alpha = false, bool stepped = true, ColorF? defaultColor = null) => ColorFieldInternal(color, defaultColor, rect, null, null, hsv, alpha, false, stepped ? 1f / 32f : 0f, stepped ? 1 / 20f : 0f);
	public static ColorF HorizontalColorField (ColorF color, IRect rect, bool hsv = true, bool alpha = false, bool stepped = true, ColorF? defaultColor = null) => ColorFieldInternal(color, defaultColor, rect, null, null, hsv, alpha, true, stepped ? 1f / 32f : 0f, stepped ? 1 / 20f : 0f);
	private static ColorF ColorFieldInternal (ColorF color, ColorF? defaultColor, IRect rect, string label, GUIStyle labelStyle, bool hsv, bool alpha, bool horizontal, float hueStep, float step) {

		// Label
		if (label != null) {
			labelStyle ??= GUISkin.Label;
			int labelWidth = Unify(LabelWidth);
			Label(rect.EdgeInside(Direction4.Left, labelWidth), label, labelStyle);
			rect = rect.Shrink(labelWidth, 0, 0, 0);
		}

		// Result
		var resultRect = rect.EdgeInside(Direction4.Left, rect.height);
		var resultColorRect = resultRect.Shrink(Unify(2));
		Renderer.DrawPixel(resultRect, Color32.BLACK);
		if (color.a.NotAlmost(1f)) {
			Renderer.Draw(BuiltInSprite.CHECKER_BOARD_8, resultColorRect);
		}
		Renderer.DrawPixel(resultColorRect, color.ToColor32());
		rect = rect.Shrink(resultRect.width, 0, 0, 0);

		// Default Rect
		IRect defaultRect = default;
		if (defaultColor.HasValue) {
			defaultRect = rect.EdgeInside(Direction4.Right, rect.height);
			rect.width -= rect.height;
		}

		// Editor
		if (horizontal) {
			rect = rect.EdgeInside(Direction4.Left, alpha ? rect.width / 4 : rect.width / 3);
		} else {
			rect = rect.EdgeInside(Direction4.Up, alpha ? rect.height / 4 : rect.height / 3);
		}
		int gapH = horizontal ? Unify(4) : 0;
		int gapV = horizontal ? 0 : Unify(4);
		if (hsv) {
			// HSV
			Util.RGBToHSV(color, out float h, out float s, out float v);
			float a = color.a;
			bool changed = false;
			// H
			changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref h, true, new ColorF(1f, 1f, 1f, s), new ColorF(v, v, v), hueStep) || changed;
			if (horizontal) rect.SlideRight(); else rect.SlideDown();
			// S
			changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref s, false, Util.HsvToRgbF(h, 1f, v).WithNewA(v), new ColorF(v, v, v), step) || changed;
			if (horizontal) rect.SlideRight(); else rect.SlideDown();
			// V
			changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref v, false, Util.HsvToRgbF(h, s, 1f), new ColorF(0, 0, 0), step) || changed;
			if (horizontal) rect.SlideRight(); else rect.SlideDown();
			// A
			if (alpha) {
				changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref a, false, color.WithNewA(1f), new ColorF(0, 0, 0, 0), step) || changed;
				if (horizontal) rect.SlideRight(); else rect.SlideDown();
			}
			// Final
			if (changed) {
				color = Util.HsvToRgbF(h.Clamp(0f, 0.99999f), s.Clamp(0.00001f, 1f), v.Clamp(0.00001f, 1f));
				color.a = a;
			}
		} else {
			// RGB
			bool changed = false;
			float r = color.r;
			float g = color.g;
			float b = color.b;
			float a = color.a;
			// R
			changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref r, false, new ColorF(1f, g, b), new ColorF(0f, g, b), step) || changed;
			if (horizontal) rect.SlideRight(); else rect.SlideDown();
			// G
			changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref g, false, new ColorF(r, 1f, b), new ColorF(r, 0f, b), step) || changed;
			if (horizontal) rect.SlideRight(); else rect.SlideDown();
			// B
			changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref b, false, new ColorF(r, g, 1f), new ColorF(r, g, 0f), step) || changed;
			if (horizontal) rect.SlideRight(); else rect.SlideDown();
			// A
			if (alpha) {
				changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref a, false, color.WithNewA(1f), new ColorF(0, 0, 0, 0), step) || changed;
				if (horizontal) rect.SlideRight(); else rect.SlideDown();
			}
			// Final
			if (changed) {
				color.r = r;
				color.g = g;
				color.b = b;
				color.a = a;
			}
		}

		// Default
		if (defaultColor.HasValue && Button(defaultRect, BuiltInSprite.ICON_REFRESH, GUISkin.SmallDarkButton)) {
			color = defaultColor.Value;
		}

		return color;

		// Func
		static bool Slider (IRect rect, ref float value, bool forHue, ColorF tintF, ColorF tintB, float step) {
			bool changed = false;
			int spriteID = forHue ? BuiltInSprite.COLOR_HUE : BuiltInSprite.COLOR_WHITE_BAR;
			// Bar
			Renderer.DrawPixel(rect, Color32.BLACK);
			// Background
			rect = rect.Shrink(Unify(2));
			if (tintB.a.NotAlmostZero()) {
				Renderer.DrawPixel(rect, tintB.ToColor32());
			} else {
				var cell = Renderer.Draw(BuiltInSprite.CHECKER_BOARD_16, rect.Envelope(1, 1));
				cell.Shift.down = cell.Shift.up = (cell.Height - rect.height) / 2;
				cell.Shift.left = cell.Shift.right = (cell.Width - rect.width) / 2;
			}
			// Foreground
			if (forHue) tintF.a = tintF.a.Clamp(0.5f, 1f);
			Renderer.Draw(spriteID, rect, tintF.ToColor32());
			// Cursor
			int cursorWidth = Unify(1);
			var cursorRect = new IRect(rect.x + (int)(rect.width * value) - cursorWidth / 2, rect.y, cursorWidth, rect.height);
			Renderer.DrawPixel(cursorRect.Expand(cursorWidth / 2, cursorWidth / 2, 0, 0), Color32.BLACK);
			Renderer.DrawPixel(cursorRect, Color32.WHITE);
			if (Enable) {
				// Wheel
				if (Input.MouseWheelDelta != 0 && rect.Contains(Input.MouseGlobalPosition)) {
					value += Input.MouseWheelDelta * (forHue ? 0.02f : 0.01f);
					if (forHue) value = value.UMod(1f);
					changed = true;
				}
				// Click
				if (rect.Contains(Input.MouseLeftDownGlobalPosition) && Input.MouseLeftButtonHolding) {
					value = Util.InverseLerp(rect.xMin, rect.xMax, Input.MouseGlobalPosition.x);
					if (step.NotAlmostZero()) {
						value = (value / step).RoundToInt() * step;
					}
					changed = true;
				}
			}
			value = value.Clamp01();
			return changed;
		}
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


	private static bool WordEnoughToFit (string text, char[] chars, int charSize, int charSpace, int startIndex, int room, out int wordLength) {
		int index = startIndex;
		bool fromString = chars == null;
		int count = fromString ? text.Length : chars.Length;
		for (; index < count; index++) {
			char c = fromString ? text[index] : chars[index];
			if (IsLineBreakingChar(c)) break;
			if (!Renderer.RequireCharForPool(c, out var sprite)) continue;
			if (room > 0) {
				room -= (sprite.Advance * charSize).RoundToInt() + charSpace;
			}
		}
		wordLength = index - startIndex + 1;
		return room >= 0;
	}


	#endregion




}