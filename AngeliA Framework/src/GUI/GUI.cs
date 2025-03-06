using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AngeliA;

public static partial class GUI {




	#region --- VAR ---


	// Const
	private const int MAX_INPUT_CHAR = 256;

	// Api
	public static bool IsTyping => TypingTextFieldID != 0;
	public static bool Enable { get; set; } = true;
	public static bool Interactable { get; set; } = true;
	public static int TypingTextFieldID { get; private set; }
	public static Color32 Color { get; set; } = Color32.WHITE;
	public static Color32 BodyColor { get; set; } = Color32.WHITE;
	public static Color32 ContentColor { get; set; } = Color32.WHITE;
	public static int LabelWidth { get; set; } = 1;
	public static GUISkin Skin { get; set; } = GUISkin.Default;
	public static int FieldHeight { get; private set; } = 1;
	public static int FieldPadding { get; private set; } = 1;
	public static int ToolbarSize { get; private set; } = 42;
	public static int ScrollbarSize { get; private set; } = 12;
	public static int ContentVersion { get; private set; } = int.MinValue;

	// Data
	private static readonly char[] TypingBuilder = new char[1024];
	private static readonly IntToChars IntLabelChars = new();
	private static int TypingBuilderCount = 0;
	private static int BeamIndex = 0;
	private static int BeamLength = 0;
	private static int BeamBlinkFrame = int.MinValue;
	private static int DraggingScrollbarID = 0;
	private static int DraggingSliderID = 0;
	private static int InvokeTypingStartID = 0;
	private static int TypingTextFieldUpdateFrame = -1;
	private static (int downValue, bool hasValue) ScrollDraggingCache = (0, false);
	private static bool SliderDraggingCache = false;
	private static int CheckingContentVersion = int.MinValue;
	private static int InternalRequiringControlID = int.MinValue;
	private static int TextInputAnchoredIndex = -1;
	private static bool ForceUnifyBasedOnMonitor;


	#endregion




	#region --- API ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		ForceUnifyBasedOnMonitor = Universe.BuiltInInfo.ScaleUiBasedOnMonitor;
	}


	[OnGameUpdate(1023)]
	internal static void Update () {
		if (TypingTextFieldID != 0 && Input.AnyKeyHolding) {
			Input.UseAllHoldingKeys(ignoreMouse: true);
			Input.UnuseKeyboardKey(KeyboardKey.LeftArrow);
			Input.UnuseKeyboardKey(KeyboardKey.RightArrow);
			Input.UnuseKeyboardKey(KeyboardKey.Delete);
			Input.UnuseKeyboardKey(KeyboardKey.Backspace);
			Input.UnuseKeyboardKey(KeyboardKey.Escape);
			Input.UnuseKeyboardKey(KeyboardKey.LeftCtrl);
			Input.UnuseKeyboardKey(KeyboardKey.LeftShift);
			Input.UnuseKeyboardKey(KeyboardKey.Tab);
		}
		if (!Game.IsMouseLeftHolding) {
			ScrollDraggingCache.hasValue = false;
			SliderDraggingCache = false;
		}
	}


	[OnGameUpdateLater(4096)]
	internal static void LateUpdate () {
		if (TypingBuilder.Length > 0) TypingBuilderCount = 0;
		if (TypingTextFieldID != 0 && Game.PauselessFrame > TypingTextFieldUpdateFrame) {
			CancelTyping();
		}
		if (InternalRequiringControlID != int.MinValue) {
			TypingTextFieldID = InternalRequiringControlID;
			InternalRequiringControlID = int.MinValue;
		}
	}


	[OnGameUpdate(-4096)]
	internal static void Reset () {
		Enable = true;
		Color = Color32.WHITE;
		BodyColor = Color32.WHITE;
		ContentColor = Color32.WHITE;
		RefreshAllCacheSize();
	}


	internal static void RefreshAllCacheSize () {
		FieldHeight = Unify(26);
		FieldPadding = Unify(8);
		ToolbarSize = Unify(42);
		ScrollbarSize = Unify(12);
		LabelWidth = Unify(196);
	}


	// Unify
	public static int Unify (int value) => ForceUnifyBasedOnMonitor ? UnifyMonitor(value) : (value * Renderer.CameraRect.height / 1000f).RoundToInt();
	public static int UnifyMonitor (int value) => (
		value / 1000f * Renderer.CameraRect.height * Game.MonitorHeight / Game.ScreenHeight.GreaterOrEquel(1)
	).RoundToInt();
	public static Int4 UnifyBorder (Int4 border) {
		border.left = Unify(border.left);
		border.right = Unify(border.right);
		border.down = Unify(border.down);
		border.up = Unify(border.up);
		return border;
	}

	public static int ReverseUnify (int value) => ForceUnifyBasedOnMonitor ? ReverseUnifyMonitor(value) : (value * 1000f / Renderer.CameraRect.height).RoundToInt();
	public static int ReverseUnifyMonitor (int value) => (value * 1000f / Renderer.CameraRect.height / Game.MonitorHeight * Game.ScreenHeight.GreaterOrEquel(1)).RoundToInt();


	public static IRect GetContentRect (IRect rect, GUIStyle style, GUIState state) {
		// Border
		var contentBorder = style.GetContentBorder(state);
		if (!contentBorder.IsZero) {
			contentBorder.left = Unify(contentBorder.left);
			contentBorder.right = Unify(contentBorder.right);
			contentBorder.down = Unify(contentBorder.down);
			contentBorder.up = Unify(contentBorder.up);
			rect = rect.Shrink(contentBorder);
		}
		// Final
		return rect;
	}


	// Draw
	public static void DrawStyleBody (IRect rect, GUIStyle style, GUIState state) => DrawStyleBody(rect, style, state, Color32.WHITE);
	public static void DrawStyleBody (IRect rect, GUIStyle style, GUIState state, Color32 tint) {
		int sprite = style.GetBodySprite(state);
		if (sprite == 0 || !Renderer.TryGetSprite(sprite, out var _sprite)) return;
		var color = BodyColor == Color32.WHITE ?
			tint * Color * style.GetBodyColor(state) :
			tint * BodyColor * Color;
		if (color.a == 0) return;
		var bodyBorder = style.GetBodyBorder(state);
		if (bodyBorder.IsZero) {
			bodyBorder = _sprite.GlobalBorder;
		}
		var border = UnifyBorder(bodyBorder);
		if (border.IsZero) {
			Renderer.Draw(_sprite, rect, color);
		} else {
			Renderer.DrawSlice(_sprite, rect, border.left, border.right, border.down, border.up, color);
		}
	}


	public static void DrawStyleContent (IRect rect, int sprite, GUIStyle style, GUIState state, bool ignoreSlice = false, bool fit = false) {
		if (!Renderer.TryGetSprite(sprite, out var _sprite)) return;
		var color = ContentColor == Color32.WHITE ?
			style.GetContentColor(state) * Color :
			ContentColor * Color;
		if (color.a == 0) return;
		var contentRect = GetContentRect(rect, style, state);
		if (fit) {
			contentRect = contentRect.Fit(rect.width, rect.height);
		}
		rect = contentRect;
		if (ignoreSlice || _sprite.GlobalBorder.IsZero) {
			Renderer.Draw(_sprite, rect, color);
		} else {
			var bodyBorder = style.GetBodyBorder(state);
			if (bodyBorder.IsZero) {
				bodyBorder = _sprite.GlobalBorder;
			}
			var border = UnifyBorder(bodyBorder);
			Renderer.DrawSlice(_sprite, rect, border.left, border.right, border.down, border.up, color);
		}
	}


	public static Cell[] DrawSlice (int spriteID, IRect rect) {
		if (Renderer.TryGetSprite(spriteID, out var sprite)) {
			return DrawSlice(sprite, rect);
		}
		return null;
	}
	public static Cell[] DrawSlice (AngeSprite sprite, IRect rect) {
		var border = sprite.GlobalBorder;
		border.left /= GUIStyle.BORDER_SCALE;
		border.right /= GUIStyle.BORDER_SCALE;
		border.down /= GUIStyle.BORDER_SCALE;
		border.up /= GUIStyle.BORDER_SCALE;
		border = UnifyBorder(border);
		return Renderer.DrawSlice(sprite, rect, border.left, border.right, border.down, border.up, Color);
	}


	// Typing
	public static void StartTyping (int controlID) {
		TypingTextFieldID = controlID;
		BeamIndex = 0;
		BeamLength = 0;
		BeamBlinkFrame = Game.PauselessFrame;
		InvokeTypingStartID = controlID;
		TypingTextFieldUpdateFrame = Game.PauselessFrame;
	}

	public static void CancelTyping () {
		TypingTextFieldID = 0;
		TypingBuilderCount = 0;
		BeamIndex = 0;
		BeamLength = 0;
		InvokeTypingStartID = 0;
	}


	// Label
	public static void SmallLabel (IRect rect, string text) => LabelLogic(rect, text, null, Skin.SmallLabel, Enable ? GUIState.Normal : GUIState.Disable, -1, 0, false, out _, out _, out _);
	public static void SmallLabel (IRect rect, char[] text) => LabelLogic(rect, "", text, Skin.SmallLabel, Enable ? GUIState.Normal : GUIState.Disable, -1, 0, false, out _, out _, out _);
	public static void SmallLabel (IRect rect, string text, out IRect bounds) => LabelLogic(rect, text, null, Skin.SmallLabel, Enable ? GUIState.Normal : GUIState.Disable, -1, 0, false, out bounds, out _, out _);
	public static void SmallLabel (IRect rect, char[] text, out IRect bounds) => LabelLogic(rect, "", text, Skin.SmallLabel, Enable ? GUIState.Normal : GUIState.Disable, -1, 0, false, out bounds, out _, out _);
	public static void SmallLabel (IRect rect, string text, int startIndex, bool drawInvisibleChar, out IRect bounds, out int endIndex) => LabelLogic(rect, text, null, Skin.SmallLabel, Enable ? GUIState.Normal : GUIState.Disable, -1, startIndex, drawInvisibleChar, out bounds, out _, out endIndex);
	public static void SmallLabel (IRect rect, string text, int beamIndex, int startIndex, bool drawInvisibleChar, out IRect bounds, out IRect beamRect, out int endIndex) => LabelLogic(rect, text, null, Skin.SmallLabel, Enable ? GUIState.Normal : GUIState.Disable, beamIndex, startIndex, drawInvisibleChar, out bounds, out beamRect, out endIndex);

	public static void Label (IRect rect, string text, GUIStyle style = null, int charSize = -2) => LabelLogic(rect, text, null, style, Enable ? GUIState.Normal : GUIState.Disable, -1, 0, false, out _, out _, out _, charSize);
	public static void Label (IRect rect, char[] text, GUIStyle style = null, int charSize = -2) => LabelLogic(rect, "", text, style, Enable ? GUIState.Normal : GUIState.Disable, -1, 0, false, out _, out _, out _, charSize);
	public static void Label (IRect rect, string text, out IRect bounds, GUIStyle style = null, int charSize = -2) => LabelLogic(rect, text, null, style, Enable ? GUIState.Normal : GUIState.Disable, -1, 0, false, out bounds, out _, out _, charSize);
	public static void Label (IRect rect, char[] text, out IRect bounds, GUIStyle style = null, int charSize = -2) => LabelLogic(rect, "", text, style, Enable ? GUIState.Normal : GUIState.Disable, -1, 0, false, out bounds, out _, out _, charSize);
	public static void Label (IRect rect, string text, int startIndex, bool drawInvisibleChar, out IRect bounds, out int endIndex, GUIStyle style = null, int charSize = -2) => LabelLogic(rect, text, null, style, Enable ? GUIState.Normal : GUIState.Disable, -1, startIndex, drawInvisibleChar, out bounds, out _, out endIndex, charSize);
	public static void Label (IRect rect, string text, int beamIndex, int startIndex, bool drawInvisibleChar, out IRect bounds, out IRect beamRect, out int endIndex, GUIStyle style = null, int charSize = -2) => LabelLogic(rect, text, null, style, Enable ? GUIState.Normal : GUIState.Disable, beamIndex, startIndex, drawInvisibleChar, out bounds, out beamRect, out endIndex, charSize);
	private static void LabelLogic (IRect rect, string text, char[] chars, GUIStyle style, GUIState state, int beamIndex, int startIndex, bool drawInvisibleChar, out IRect bounds, out IRect beamRect, out int endIndex, int charSize = -2) {

		if (Game.FontCount == 0 || (text == null && chars == null)) {
			endIndex = startIndex;
			bounds = rect;
			beamRect = new IRect(rect.x, rect.y, 1, rect.height);
			return;
		}

		// Draw
		style ??= Skin.Label;
		Renderer.GetCells(out var textCells, out int textCountInLayer);
		rect = GetContentRect(rect, style, state);
		var color = Color * ContentColor * style.GetContentColor(state);

		// Logic
		endIndex = startIndex;
		bounds = rect;
		beamRect = new IRect(rect.x, rect.y, 1, rect.height);

		bool fromString = chars == null;
		int count = fromString ? text.Length : chars.Length;
		if (charSize == -2) {
			charSize = style.CharSize < 0 ? rect.height : Unify(style.CharSize);
		} else {
			charSize = charSize < 0 ? rect.height : Unify(charSize);
		}
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
				FrameworkUtil.ClampCells(textCells, bounds, startCellIndex, textCountInLayer);
			}
		}

	}


	// Label Extra
	public static int ScrollLabel (string text, IRect rect, int scrollPosition, GUIStyle style) {
		style ??= Skin.Label;
		int before = Renderer.GetUsedCellCount();
		LabelLogic(rect, text, null, style, GUIState.Normal, -1, 0, false, out var bounds, out _, out _);
		if (bounds.height < rect.height) {
			scrollPosition = 0;
			return scrollPosition;
		}
		scrollPosition = scrollPosition.Clamp(0, bounds.height - rect.height + Unify(style.CharSize * 2));
		int after = Renderer.GetUsedCellCount();
		if (before == after) return scrollPosition;
		// Clamp
		if (Renderer.GetCells(out var cells, out int count)) {
			for (int i = before; i < after && i < count; i++) {
				var cell = cells[i];
				cell.Y += scrollPosition;
			}
		}
		Renderer.ClampCells(rect, before, after);
		return scrollPosition;
	}

	public static void BackgroundLabel (IRect rect, string text, Color32 backgroundColor, int backgroundPadding = 0, bool forceInside = false, GUIStyle style = null) => BackgroundLabel(rect, text, backgroundColor, out _, backgroundPadding, forceInside, style);
	public static void BackgroundLabel (IRect rect, string text, Color32 backgroundColor, out IRect bounds, int backgroundPadding = 0, bool forceInside = false, GUIStyle style = null) {
		int startIndex = Renderer.GetUsedCellCount();
		var bg = Renderer.DrawPixel(default, Color * BodyColor * backgroundColor, z: 0);
		LabelLogic(rect, text, null, style, GUIState.Normal, -1, 0, false, out bounds, out _, out _);
		if (forceInside) {
			bounds = rect;
			Renderer.ClampCells(bounds, startIndex);
		}
		bg.SetRect(bounds.Expand(backgroundPadding));
	}

	public static void BackgroundLabel (IRect rect, char[] chars, Color32 backgroundColor, int backgroundPadding = 0, GUIStyle style = null) => BackgroundLabel(rect, chars, backgroundColor, out _, backgroundPadding, style);
	public static void BackgroundLabel (IRect rect, char[] chars, Color32 backgroundColor, out IRect bounds, int backgroundPadding = 0, GUIStyle style = null) {
		var bg = Renderer.DrawPixel(default, Color * BodyColor * backgroundColor, z: 0);
		LabelLogic(rect, null, chars, style, GUIState.Normal, -1, 0, false, out bounds, out _, out _);
		bg.SetRect(bounds.Expand(backgroundPadding));
	}

	public static void ShadowLabel (IRect rect, string text, int shadowDistance = 3, GUIStyle style = null) {
		var oldC = ContentColor;
		ContentColor = Color32.GREY_20;
		LabelLogic(rect.Shift(0, -Unify(shadowDistance)), text, null, style, GUIState.Normal, -1, 0, false, out _, out _, out _);
		ContentColor = oldC;
		LabelLogic(rect, text, null, style, GUIState.Normal, -1, 0, false, out _, out _, out _);
	}
	public static void ShadowLabel (IRect rect, char[] chars, int shadowDistance = 3, GUIStyle style = null) {
		var oldC = ContentColor;
		ContentColor = Color32.GREY_20;
		LabelLogic(rect.Shift(0, -Unify(shadowDistance)), null, chars, style, GUIState.Normal, -1, 0, false, out _, out _, out _);
		ContentColor = oldC;
		LabelLogic(rect, null, chars, style, GUIState.Normal, -1, 0, false, out _, out _, out _);
	}

	public static void IntLabel (IRect rect, int number, GUIStyle style = null) => IntLabel(rect, number, out _, style);
	public static void IntLabel (IRect rect, int number, out IRect bounds, GUIStyle style = null) => LabelLogic(rect, "", IntLabelChars.GetChars(number), style, Enable ? GUIState.Normal : GUIState.Disable, -1, 0, false, out bounds, out _, out _);

	public static void FrameBasedIntLabel (IRect rect, FrameBasedInt number, Color32 greaterColor, Color32 lessColor, GUIStyle style = null) => FrameBasedIntLabel(rect, number, greaterColor, lessColor, out _, style);
	public static void FrameBasedIntLabel (IRect rect, FrameBasedInt number, Color32 greaterColor, Color32 lessColor, out IRect bounds, GUIStyle style = null) {
		int baseValue = number.BaseValue;
		int finalValue = number.FinalValue;
		// Base
		IntLabel(rect, baseValue, out bounds, style);
		if (finalValue != baseValue) {
			int padding = Unify(4);
			using var _ = new GUIContentColorScope(finalValue > baseValue ? greaterColor : lessColor);
			// +/-
			Label(bounds.EdgeOutsideRight(1).Shift(padding, 0), finalValue > baseValue ? "+" : "-", out var markBounds, style);
			// Delta
			IntLabel(markBounds.EdgeOutsideRight(1).Shift(padding, 0), (finalValue - baseValue).Abs(), out var deltaBounds, style);
			bounds.xMax = deltaBounds.xMax;
		}
	}


	// Button
	public static bool SmallLinkButton (IRect rect, string label, bool useUnderLine = true, int charSize = -2) => LinkButton(rect, label, out _, Skin.SmallLabel, useUnderLine, charSize);
	public static bool SmallLinkButton (IRect rect, string label, out IRect bounds, bool useUnderLine = true, int charSize = -2) => LinkButton(rect, label, out bounds, Skin.SmallLabel, useUnderLine, charSize);
	public static bool LinkButton (IRect rect, string label, GUIStyle labelStyle = null, bool useUnderLine = true, int charSize = -2) => LinkButton(rect, label, out _, labelStyle, useUnderLine, charSize);
	public static bool LinkButton (IRect rect, string label, out IRect bounds, GUIStyle labelStyle = null, bool useUnderLine = true, int charSize = -2) {

		bounds = default;
		if (string.IsNullOrEmpty(label)) return false;

		// Label
		labelStyle ??= Skin.Label;
		int cellStart = Renderer.GetUsedCellCount();
		LabelLogic(rect, label, null, labelStyle, GUIState.Normal, -1, 0, false, out bounds, out _, out _, charSize);

		// Button
		bool result = BlankButton(bounds, out var state);
		var tint = state == GUIState.Hover ? Skin.LinkTintHover : Skin.LinkTint;
		tint.a = (byte)(state == GUIState.Disable ? 128 : 255);

		// Tint
		if (Renderer.GetCells(out var cells, out int count)) {
			for (int i = cellStart; i < count; i++) {
				cells[i].Color *= tint;
			}
		}

		// Under Line
		if (useUnderLine) {
			Renderer.DrawPixel(bounds.EdgeOutsideDown(Unify(1)), tint);
		}

		return result;
	}

	public static bool DarkButton (IRect rect, string label, int charSize = -2) => Button(rect, label, Skin.DarkButton, charSize);
	public static bool DarkButton (IRect rect, int icon) => Button(rect, icon, Skin.DarkButton);
	public static bool Button (IRect rect, string label, GUIStyle style = null, int charSize = -2) => Button(rect, label, out _, style, charSize);
	public static bool Button (IRect rect, string label, out GUIState state, GUIStyle style = null, int charSize = -2) {
		style ??= Skin.Button;
		bool result = BlankButton(rect, out state);
		DrawStyleBody(rect, style, state);
		// Label
		if (!string.IsNullOrEmpty(label)) {
			LabelLogic(rect, label, null, style, state, -1, 0, false, out _, out _, out _, charSize);
		}
		return result;
	}
	public static bool Button (IRect rect, int icon, GUIStyle style = null) => Button(rect, icon, out _, style);
	public static bool Button (IRect rect, int icon, out GUIState state, GUIStyle style = null) {
		style ??= Skin.Button;
		bool result = BlankButton(rect, out state);
		DrawStyleBody(rect, style, state);
		Icon(rect, icon, style, state);
		return result;
	}

	public static bool BlankButton (IRect rect, out GUIState state) {
		state = GUIState.Normal;
		if (!Enable) {
			state = GUIState.Disable;
			return false;
		} else if (!Interactable) {
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
		markStyle ??= Skin.GreenPixel;
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
		bodyStyle ??= Skin.Toggle;
		labelStyle ??= Skin.Label;
		markStyle ??= Skin.ToggleMark;
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
		bodyStyle ??= Skin.DarkButton;
		isOn = BlankToggle(rect, isOn, out var state);
		DrawStyleBody(rect, bodyStyle, state, isOn ? Color32.GREY_160 : Color32.WHITE);
		LabelLogic(rect, label, null, bodyStyle, state, -1, 0, false, out _, out _, out _);
		return isOn;
	}
	public static bool ToggleButton (IRect rect, bool isOn, int icon, GUIStyle style = null) {
		style ??= Skin.DarkButton;
		isOn = BlankToggle(rect, isOn, out var state);
		var fakeState = state != GUIState.Disable ? state : isOn ? GUIState.Press : GUIState.Normal;
		DrawStyleBody(
			rect, style, fakeState,
			state != GUIState.Disable ? Color32.WHITE : Color32.WHITE_128
		);
		Icon(rect, icon, style, fakeState);
		return isOn;
	}
	public static bool ToggleFold (IRect rect, ref bool folding, int icon, string label, int paddingLeft = 0, int paddingRight = 0) {

		// Fold Icon
		Icon(rect.EdgeInside(Direction4.Left, rect.height * 3 / 4).Shift(-paddingLeft / 5, 0), icon);

		// Fold Label
		if (Button(rect.Expand(paddingLeft, paddingRight, 0, 0), 0, Skin.WeakHighlightPixel)) {
			folding = !folding;
		}
		Label(rect.ShrinkLeft(rect.height), label, Skin.SmallGreyLabel);

		// Fold Triangle
		using (new GUIColorScope(Color32.GREY_128)) {
			Icon(
				rect.EdgeOutsideLeft(rect.height / 2).Shift(-paddingLeft / 3, 0),
				folding ? BuiltInSprite.ICON_TRIANGLE_RIGHT : BuiltInSprite.ICON_TRIANGLE_DOWN
			);
		}

		return folding;
	}
	public static bool Toggle (IRect rect, bool isOn, GUIStyle bodyStyle = null, GUIStyle markStyle = null) =>
		Toggle(rect, isOn, null, bodyStyle, markStyle, null);
	public static bool Toggle (IRect rect, bool isOn, string label, GUIStyle bodyStyle = null, GUIStyle markStyle = null, GUIStyle labelStyle = null) {
		bodyStyle ??= Skin.Toggle;
		markStyle ??= Skin.ToggleMark;
		if (label != null) {
			labelStyle ??= Skin.Label;
			Label(rect.EdgeInside(Direction4.Left, LabelWidth), label, labelStyle);
			rect = rect.Shrink(LabelWidth, 0, 0, 0);
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
		if (BlankButton(rect, out state)) {
			isOn = !isOn;
			ContentVersion++;
		}
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
			DrawStyleContent(rect.Fit(icon), sprite, style, state, ignoreSlice: true, fit: true);
		} else {
			Renderer.Draw(icon, rect.Fit(icon), Color * ContentColor);
		}
	}
	public static void PopupTriangleIcon (IRect rect, int iconSprite = 0) {
		iconSprite = iconSprite == 0 ? BuiltInSprite.ICON_TRIANGLE_DOWN : iconSprite;
		Renderer.Draw(iconSprite, rect.Shrink(rect.height / 4).EdgeInsideSquareRight());
	}


	// Int Dial
	public static int SmallIntDial (IRect rect, int value, string label = null, int delta = 1, int min = int.MinValue, int max = int.MaxValue) => IntDial(rect, value, out _, label, Skin.SmallLabel, Skin.SmallCenterLabel, Skin.SmallDarkButton, delta, min, max);
	public static int SmallIntDial (IRect rect, int value, out bool changed, string label = null, int delta = 1, int min = int.MinValue, int max = int.MaxValue) => IntDial(rect, value, out changed, label, Skin.SmallLabel, Skin.SmallCenterLabel, Skin.SmallDarkButton, delta, min, max);
	public static int IntDial (IRect rect, int value, string label = null, GUIStyle labelStyle = null, GUIStyle bodyStyle = null, GUIStyle dialButtonStyle = null, int delta = 1, int min = int.MinValue, int max = int.MaxValue) => IntDial(rect, value, out _, label, labelStyle, bodyStyle, dialButtonStyle, delta, min, max);
	public static int IntDial (IRect rect, int value, out bool changed, string label = null, GUIStyle labelStyle = null, GUIStyle bodyStyle = null, GUIStyle dialButtonStyle = null, int delta = 1, int min = int.MinValue, int max = int.MaxValue) {

		bodyStyle ??= Skin.SmallCenterLabel;
		dialButtonStyle ??= Skin.SmallDarkButton;
		int oldValue = value;

		// Label
		if (label != null) {
			labelStyle ??= Skin.Label;
			Label(rect.EdgeInside(Direction4.Left, LabelWidth), label, labelStyle);
			rect = rect.ShrinkLeft(LabelWidth);
		}
		int buttonSize = Unify(42);

		// Value
		var labelRect = rect.ShrinkRight(buttonSize);
		DrawStyleBody(labelRect, bodyStyle, Enable ? GUIState.Normal : GUIState.Disable);
		IntLabel(labelRect, value, bodyStyle);

		// Buttons
		rect = rect.EdgeInside(Direction4.Right, buttonSize);
		if (Button(rect.TopHalf(), BuiltInSprite.ICON_TRIANGLE_UP, dialButtonStyle)) {
			value = (value + delta).Clamp(min, max);
		}
		if (Button(rect.BottomHalf(), BuiltInSprite.ICON_TRIANGLE_DOWN, dialButtonStyle)) {
			value = (value - delta).Clamp(min, max);
		}
		changed = value != oldValue;
		if (changed) ContentVersion++;

		return value;
	}


	// Scrollbar
	public static int ScrollBar (int controlID, IRect contentRect, int position, int totalSize, int pageSize, GUIStyle handleStyle = null, GUIStyle bgStyle = null, bool vertical = true) {

		if (pageSize >= totalSize) return 0;

		handleStyle ??= Skin.Scrollbar;
		bgStyle ??= Skin.WeakPixel;

		int barWidth = vertical ? contentRect.width : (int)((long)(contentRect.width * pageSize) / totalSize);
		int barHeight = vertical ? (int)((long)(contentRect.height * pageSize) / totalSize) : contentRect.height;
		var barRect = new IRect(
			vertical ? contentRect.x : RemapLarge(
				0, totalSize - pageSize,
				contentRect.x, contentRect.xMax - barWidth,
				position
			),
			vertical ? RemapLarge(
				0, totalSize - pageSize,
				contentRect.yMax - barHeight, contentRect.y,
				position
			) : contentRect.y,
			barWidth,
			barHeight
		);

		static int RemapLarge (int l, int r, int newL, int newR, int t) {
			return l == r ?
				newL :
				newL + (int)((long)(newR - newL) * (t - l) / (r - l));
		}
		bool focusingBar = DraggingScrollbarID == controlID;
		bool hoveringBar = barRect.MouseInside();
		bool dragging = focusingBar && ScrollDraggingCache.hasValue;

		// BG
		DrawStyleBody(contentRect, bgStyle, !Enable ? GUIState.Disable : GUIState.Normal);

		// Handle
		var state =
			!Enable ? GUIState.Disable :
			dragging ? GUIState.Press :
			hoveringBar ? GUIState.Hover :
			GUIState.Normal;
		DrawStyleBody(barRect, handleStyle, state);

		int axis = vertical ? 1 : 0;
		// Dragging
		if (dragging) {
			int mousePos = Input.MouseGlobalPosition[axis];
			int mouseDownPos = Input.MouseLeftDownGlobalPosition[axis];
			int scrollDownPos = ScrollDraggingCache.downValue;
			if (vertical) {
				position = scrollDownPos + (mouseDownPos - mousePos) * totalSize / contentRect.height;
			} else {
				position = scrollDownPos + (mouseDownPos - mousePos) * totalSize / -contentRect.width;
			}
		}

		// Mouse Down
		if (Interactable && Enable && Input.MouseLeftButtonDown) {
			if (hoveringBar) {
				// Start Drag
				ScrollDraggingCache = (position, true);
				DraggingScrollbarID = controlID;
			} else if (contentRect.MouseInside()) {
				// Jump on Click
				int mousePos = Input.MouseGlobalPosition[axis];
				position = Util.RemapUnclamped(
					vertical ? contentRect.y : contentRect.xMax,
					vertical ? contentRect.yMax : contentRect.x,
					totalSize - pageSize / 2, -pageSize / 2,
					mousePos
				);
				ScrollDraggingCache = (position, true);
				DraggingScrollbarID = controlID;
			}
		}

		return position.Clamp(0, totalSize - pageSize);
	}


	// Slider
	public static int FilledSlider (int controlID, IRect rect, int value, int min, int max, bool vertical = false, int step = 0) => Slider(controlID, rect, value, min, max, Skin.SliderBody, null, Skin.SliderFill, vertical, step);
	public static int HandleSlider (int controlID, IRect rect, int value, int min, int max, bool vertical = false, int step = 0) => Slider(controlID, rect, value, min, max, Skin.SliderBody, Skin.SliderHandle, null, vertical, step);
	public static int Slider (int controlID, IRect rect, int value, int min, int max, GUIStyle bodyStyle, GUIStyle handleStyle, GUIStyle fillStyle, bool vertical = false, int step = 0) {

		value = BlankSlider(controlID, rect, value, min, max, out var state, vertical, step);
		int axis = vertical ? 1 : 0;
		int valuePos = Util.Remap(min, max, rect.position[axis], rect.TopRight()[axis], (float)value).RoundToInt();

		// Body
		if (bodyStyle != null) {
			DrawStyleBody(rect, bodyStyle, GUIState.Normal);
		}

		// Fill
		if (fillStyle != null) {
			var fillRect = rect;
			var size = fillRect.size;
			size[axis] = valuePos - rect.position[axis];
			fillRect.size = size;
			DrawStyleBody(fillRect, fillStyle, GUIState.Normal);
		}

		// Handle
		if (handleStyle != null) {
			var handleRect = rect.EdgeInsideLeft(rect.height);
			handleRect.x = valuePos - handleRect.size[axis] / 2;
			DrawStyleBody(handleRect, handleStyle, state);
		}

		return value;
	}
	public static int BlankSlider (int controlID, IRect rect, int value, int min, int max, out GUIState state, bool vertical = false, int step = 0) {

		int oldValue = value;
		state = Enable ? GUIState.Normal : GUIState.Disable;

		if (!Enable) return value;
		int exH = vertical ? 0 : rect.height / 2;
		int exV = vertical ? rect.width / 2 : 0;
		var expandedRect = rect.Expand(exH, exH, exV, exV);
		bool hovering = expandedRect.MouseInside();
		bool dragging = DraggingSliderID == controlID && SliderDraggingCache;
		state = dragging ? GUIState.Press : hovering ? GUIState.Hover : GUIState.Normal;

		// Drag Start
		if (!dragging && Input.MouseLeftButtonDown && hovering) {
			DraggingSliderID = controlID;
			SliderDraggingCache = true;
			dragging = true;
		}

		// Dragging
		if (dragging) {
			int axis = vertical ? 1 : 0;
			// Set Value
			float valueF = Util.RemapUnclamped(
				rect.position[axis], rect.TopRight()[axis],
				min, max,
				(float)Input.MouseGlobalPosition[axis]
			);
			// Step Value
			if (step > 0) {
				valueF = (valueF / step).RoundToInt() * step;
			}
			valueF = valueF.Clamp(min, max);
			value = valueF.RoundToInt();
		}

		// End
		if (value != oldValue) ContentVersion++;
		return value;
	}


	// Highlight
	public static void HighlightCursor (int spriteID, IRect rect) => HighlightCursor(spriteID, rect, Color32.GREEN);
	public static void HighlightCursor (int spriteID, IRect rect, Color32 color) {
		int border = Unify(4);
		int thickness = Unify(8);
		Renderer.DrawSlice(
			spriteID, rect.Expand(Game.GlobalFrame.PingPong(thickness)),
			border, border, border, border,
			Color * BodyColor * color
		);
	}


	// Color
	public static ColorF VerticalColorField (ColorF color, IRect rect, string label, GUIStyle labelStyle = null, bool hsv = true, bool alpha = false, bool stepped = true, bool folded = false, ColorF? defaultColor = null) => ColorFieldInternal(color, defaultColor, rect, label, labelStyle, hsv, alpha, false, stepped ? 1f / 32f : 0f, stepped ? 1 / 20f : 0f, folded);
	public static ColorF HorizontalColorField (ColorF color, IRect rect, string label, GUIStyle labelStyle = null, bool hsv = true, bool alpha = false, bool stepped = true, bool folded = false, ColorF? defaultColor = null) => ColorFieldInternal(color, defaultColor, rect, label, labelStyle, hsv, alpha, true, stepped ? 1f / 32f : 0f, stepped ? 1 / 20f : 0f, folded);
	public static ColorF VerticalColorField (ColorF color, IRect rect, bool hsv = true, bool alpha = false, bool stepped = true, bool folded = false, ColorF? defaultColor = null) => ColorFieldInternal(color, defaultColor, rect, null, null, hsv, alpha, false, stepped ? 1f / 32f : 0f, stepped ? 1 / 20f : 0f, folded);
	public static ColorF HorizontalColorField (ColorF color, IRect rect, bool hsv = true, bool alpha = false, bool stepped = true, bool folded = false, ColorF? defaultColor = null) => ColorFieldInternal(color, defaultColor, rect, null, null, hsv, alpha, true, stepped ? 1f / 32f : 0f, stepped ? 1 / 20f : 0f, folded);
	private static ColorF ColorFieldInternal (ColorF color, ColorF? defaultColor, IRect rect, string label, GUIStyle labelStyle, bool hsv, bool alpha, bool horizontal, float hueStep, float step, bool folded) {

		// Label
		if (label != null) {
			labelStyle ??= Skin.Label;
			Label(rect.EdgeInside(Direction4.Left, LabelWidth), label, labelStyle);
			rect = rect.Shrink(LabelWidth, 0, 0, 0);
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

		if (!folded) {

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
			bool warnOnChange = color.a.AlmostZero();
			if (hsv) {
				// HSV
				Util.RGBToHSV(color, out float h, out float s, out float v);
				float a = color.a;
				bool changed = false;
				// H
				changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref h, true, new ColorF(1f, 1f, 1f, s), new ColorF(v, v, v), hueStep, false) || changed;
				if (horizontal) rect.SlideRight(); else rect.SlideDown();
				// S
				changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref s, false, Util.HsvToRgbF(h, 1f, v).WithNewA(v), new ColorF(v, v, v), step, false) || changed;
				if (horizontal) rect.SlideRight(); else rect.SlideDown();
				// V
				changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref v, false, Util.HsvToRgbF(h, s, 1f), new ColorF(0, 0, 0), step, false) || changed;
				if (horizontal) rect.SlideRight(); else rect.SlideDown();
				// A
				if (alpha) {
					changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref a, false, color.WithNewA(1f), new ColorF(0, 0, 0, 0), step, warnOnChange && changed) || changed;
					if (horizontal) rect.SlideRight(); else rect.SlideDown();
				}
				// Final
				if (changed) {
					color = Util.HsvToRgbF(h.Clamp(0f, 0.99999f), s.Clamp(0.00001f, 1f), v.Clamp(0.00001f, 1f));
					color.a = a;
					ContentVersion++;
				}
			} else {
				// RGB
				bool changed = false;
				float r = color.r;
				float g = color.g;
				float b = color.b;
				float a = color.a;
				// R
				changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref r, false, new ColorF(1f, g, b), new ColorF(0f, g, b), step, false) || changed;
				if (horizontal) rect.SlideRight(); else rect.SlideDown();
				// G
				changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref g, false, new ColorF(r, 1f, b), new ColorF(r, 0f, b), step, false) || changed;
				if (horizontal) rect.SlideRight(); else rect.SlideDown();
				// B
				changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref b, false, new ColorF(r, g, 1f), new ColorF(r, g, 0f), step, false) || changed;
				if (horizontal) rect.SlideRight(); else rect.SlideDown();
				// A
				if (alpha) {
					changed = Slider(rect.Shrink(gapH, gapH, gapV, gapV), ref a, false, color.WithNewA(1f), new ColorF(0, 0, 0, 0), step, warnOnChange && changed) || changed;
					if (horizontal) rect.SlideRight(); else rect.SlideDown();
				}
				// Final
				if (changed) {
					color.r = r;
					color.g = g;
					color.b = b;
					color.a = a;
					ContentVersion++;
				}
			}

			// Default
			if (defaultColor.HasValue && Button(defaultRect, BuiltInSprite.ICON_REFRESH, Skin.SmallDarkButton)) {
				color = defaultColor.Value;
				ContentVersion++;
			}
		}

		return color;

		// Func
		static bool Slider (IRect rect, ref float value, bool forHue, ColorF tintF, ColorF tintB, float step, bool drawWarn) {
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
			if (Enable && Interactable) {
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
			// Warn
			if (drawWarn) {
				Renderer.DrawPixel(rect, Color32.RED.WithNewA(196));
			}
			return changed;
		}
	}


	// Misc
	internal static void OnTextInput (char c) {
		if (TypingBuilderCount >= TypingBuilder.Length) return;
		if (char.IsControl(c)) {
			switch (c) {
				case Const.RETURN_SIGN:
				case Const.CONTROL_CUT:
				case Const.CONTROL_COPY:
				case Const.CONTROL_PASTE:
				case Const.CONTROL_SELECT_ALL:
					break;
				default:
					return;
			}
		}
		TypingBuilder[TypingBuilderCount] = c;
		TypingBuilderCount++;
	}


	public static void BeginChangeCheck () => CheckingContentVersion = ContentVersion;


	public static bool EndChangeCheck () => CheckingContentVersion != ContentVersion;


	public static void DrawAxis (Int2 position, Int2 length, Int2 stepCount, int stepNumberGap, int thickness, int stepThickness, int z, Color32 colorX, Color32 colorY, int labelHeight = 0, GUIStyle labelStyle = null, IRect clampRect = default) {

		if (length.x == 0 || length.y == 0) return;

		var rect = new IRect(position, length);
		IRect stepRect = default;
		var stepLabelRect = new IRect(0, 0, 1, labelHeight);
		int labelPadding = labelHeight / 4;
		labelStyle ??= Skin.AutoDarkLabel;

		// X
		var xRect = rect.EdgeInside(Direction4.Down, thickness);
		if (clampRect != default) xRect = xRect.Clamp(clampRect);
		Renderer.DrawPixel(xRect, color: colorX, z: z);
		if (stepCount.x > 1) {
			int stepLengthX = length.x / stepCount.x;
			float f_stepLenX = (float)length.x / stepCount.x;
			int stepL = (xRect.x - position.x).UDivide(stepLengthX) * stepLengthX + position.x;
			int stepR = rect.xMax;
			if (clampRect != default) stepR = Util.Min(stepR, clampRect.xMax);
			stepR = (stepR - position.x).UDivide(stepLengthX) * stepLengthX + position.x;
			stepRect.width = thickness;
			stepRect.height = stepThickness;
			stepRect.y = xRect.yMin;
			stepLabelRect.y = xRect.yMin;
			int currentIndex = (stepL - position.x) / stepLengthX;
			for (int i = stepL; i < stepR && currentIndex <= stepCount.x; currentIndex++) {
				i = position.x + (currentIndex * f_stepLenX).RoundToInt() - thickness / 2;
				if (currentIndex != 0) {
					stepRect.x = i;
					Renderer.DrawPixel(stepRect, color: colorX, z: z);
					if (labelHeight > 0) {
						stepLabelRect.x = i + labelPadding;
						IntLabel(stepLabelRect, currentIndex * stepNumberGap, labelStyle);
					}
				}
			}
		}

		// Y
		var yRect = rect.EdgeInside(Direction4.Left, thickness);
		if (clampRect.height > 0) yRect = yRect.Clamp(clampRect);
		Renderer.DrawPixel(yRect, color: colorY, z: z);
		if (stepCount.y > 1) {
			int stepLengthY = length.y / stepCount.y;
			float f_stepLenY = (float)length.y / stepCount.y;
			int stepD = (yRect.y - position.y).UDivide(stepLengthY) * stepLengthY + position.y;
			int stepU = rect.yMax;
			if (clampRect.height > 0) stepU = Util.Min(stepU, clampRect.yMax);
			stepU = (stepU - position.y).UDivide(stepLengthY) * stepLengthY + position.y;
			stepRect.width = stepThickness;
			stepRect.height = thickness;
			stepRect.x = yRect.xMin;
			stepLabelRect.x = yRect.xMin + labelPadding;
			int currentIndex = (stepD - position.y) / stepLengthY;
			for (int j = stepD; j < stepU && currentIndex <= stepCount.y; currentIndex++) {
				j = position.y + (currentIndex * f_stepLenY).RoundToInt() - thickness / 2;
				if (currentIndex != 0) {
					stepRect.y = j;
					Renderer.DrawPixel(stepRect, color: colorY, z: z);
				}
				if (labelHeight > 0 && currentIndex > 0) {
					stepLabelRect.y = j + labelPadding;
					IntLabel(stepLabelRect, currentIndex * stepNumberGap, labelStyle);
				}
			}
		}

	}


	public static void SetChange () => ContentVersion++;


	#endregion




	#region --- LGC ---


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