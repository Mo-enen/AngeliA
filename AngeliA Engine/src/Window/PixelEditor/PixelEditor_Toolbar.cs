using System;
using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public partial class PixelEditor {




	#region --- SUB ---


	private enum InputName { Name, Width, Height, BorderL, BorderR, BorderD, BorderU, PivotX, PivotY, Z, Duration, }


	private class SpriteSorterByXY : IComparer<SpriteData> {
		public static readonly SpriteSorterByXY Instance = new();
		public int Compare (SpriteData a, SpriteData b) {

			int result = b.Sprite.PixelRect.y.CompareTo(a.Sprite.PixelRect.y);
			if (result != 0) return result;
			return a.Sprite.PixelRect.x.CompareTo(b.Sprite.PixelRect.x);
		}
	}


	#endregion




	#region --- VAR ---



	// Const
	private const int BASIC_INPUT_ID = 123631253;
	private static readonly string[] INPUT_TEXT = ["", "", "", "", "", "", "", "", "", "", "",];

	// Sprite
	private static readonly SpriteCode UI_RULE_PANEL = "UI.Artwork.RulePanel";
	private static readonly SpriteCode UI_TOOLBAR = "UI.Artwork.Toolbar";
	private static readonly SpriteCode UI_TOOL_BG = "UI.Artwork.ToolBG";
	private static readonly SpriteCode ICON_DELETE_SPRITE = "Icon.DeleteSprite";
	private static readonly SpriteCode ICON_SHOW_CHECKER = "Icon.ShowCheckerBoard";
	private static readonly SpriteCode ICON_SHOW_AXIS = "Icon.Axis";
	private static readonly SpriteCode ICON_NEW_SPRITE = "Icon.NewSprite";
	private static readonly SpriteCode ICON_TRIGGER_ON = "Icon.TriggerOn";
	private static readonly SpriteCode ICON_TRIGGER_OFF = "Icon.TriggerOff";
	private static readonly SpriteCode ICON_TRIGGER_MIX = "Icon.TriggerMix";
	private static readonly SpriteCode ICON_TAG = "Icon.Tag";
	private static readonly SpriteCode ICON_TAG_MARKED = "Icon.TagMarked";
	private static readonly SpriteCode ICON_RULE = "Icon.Rule";
	private static readonly SpriteCode ICON_MIX = "Icon.Mix";
	private static readonly SpriteCode ICON_RULE_SAME = "Icon.Same";
	private static readonly SpriteCode ICON_RULE_NOT_SAME = "Icon.NotSame";
	private static readonly SpriteCode ICON_RULE_ANY = "Icon.Any";
	private static readonly SpriteCode ICON_RULE_EMPTY = "Icon.Empty";
	private static readonly SpriteCode ICON_RULE_MODE_A = "Icon.RuleModeA";
	private static readonly SpriteCode ICON_RULE_MODE_B = "Icon.RuleModeB";
	private static readonly SpriteCode ICON_OPER_FLIP_H = "Icon.PixOperation.FlipH";
	private static readonly SpriteCode ICON_OPER_FLIP_V = "Icon.PixOperation.FlipV";
	private static readonly SpriteCode ICON_OPER_ROT_C = "Icon.PixOperation.RotC";
	private static readonly SpriteCode ICON_OPER_ROT_CC = "Icon.PixOperation.RotCC";
	private static readonly SpriteCode ICON_RESET_CAMERA = "Icon.ResetCamera";
	private static SpriteCode[] UI_TOOLS;

	// Language
	private static readonly LanguageCode TIP_PAINTING_COLOR = ("Tip.PaintingColor", "Current painting color");
	private static readonly LanguageCode TIP_SHOW_CHECKER = ("Tip.ShowCheckerBoard", "Show Checker Board");
	private static readonly LanguageCode TIP_SHOW_AXIS = ("Tip.ShowAxis", "Show Axis");
	private static readonly LanguageCode TIP_RESET_CAMERA = ("Tip.ResetCamera", "Reset canvas position");
	private static readonly LanguageCode TIP_NEW_SPRITE = ("Tip.NewSprite", "Create a New Sprite");
	private static readonly LanguageCode TIP_DEL_SPRITE = ("Tip.DeleteSprite", "Delete sprite");
	private static readonly LanguageCode TIP_ENABLE_BORDER = ("Tip.EnableBorder", "Enable borders");
	private static readonly LanguageCode TIP_DISABLE_BORDER = ("Tip.DisableBorder", "Disable borders");
	private static readonly LanguageCode TIP_SIZE_X = ("Tip.SizeX", "Width");
	private static readonly LanguageCode TIP_SIZE_Y = ("Tip.SizeY", "Height");
	private static readonly LanguageCode TIP_BORDER_L = ("Tip.BorderL", "Border left");
	private static readonly LanguageCode TIP_BORDER_R = ("Tip.BorderR", "Border right");
	private static readonly LanguageCode TIP_BORDER_D = ("Tip.BorderD", "Border bottom");
	private static readonly LanguageCode TIP_BORDER_U = ("Tip.BorderU", "Border top");
	private static readonly LanguageCode TIP_PIVOT_X = ("Tip.PivotX", "Pivot X");
	private static readonly LanguageCode TIP_PIVOT_Y = ("Tip.PivotY", "Pivot Y");
	private static readonly LanguageCode TIP_Z = ("Tip.Z", "Sprite Z");
	private static readonly LanguageCode TIP_DURATION = ("Tip.Dration", "Animation duration");
	private static readonly LanguageCode TIP_TRIGGER = ("Tip.Trigger", "Is trigger sprite");
	private static readonly LanguageCode TIP_TAG = ("Tip.Tag", "Tag");
	private static readonly LanguageCode TIP_RULE = ("Tip.Rule", "Tiling rule for auto level sprite");
	private static readonly LanguageCode TIP_RULE_WHATEVER = ("Tip.Rule.Whatever", "Whatever");
	private static readonly LanguageCode TIP_RULE_SAME = ("Tip.Rule.Same", "Same tile");
	private static readonly LanguageCode TIP_RULE_NOT_SAME = ("Tip.Rule.NotSame", "Not same tile");
	private static readonly LanguageCode TIP_RULE_ANY = ("Tip.Rule.Any", "Any tile");
	private static readonly LanguageCode TIP_RULE_EMPTY = ("Tip.Rule.Empty", "Empty tile");
	private static readonly LanguageCode TIP_RULE_MODE = ("Tip.Rule.Mode", "Rule setting mode. (Same/Not Same) or (Any/Empty)");
	private static readonly LanguageCode TIP_CREATE_SPRITE = ("Tip.CreateSprite", "Create a new sprite");
	private static readonly LanguageCode TIP_OPER_FLIP_H = ("Tip.PixOperation.FlipH", "Flip selected pixels horizontally");
	private static readonly LanguageCode TIP_OPER_FLIP_V = ("Tip.PixOperation.FlipV", "Flip selected pixels Vertically");
	private static readonly LanguageCode TIP_OPER_ROT_C = ("Tip.PixOperation.RotC", "Rotate selected pixels Clockwise");
	private static readonly LanguageCode TIP_OPER_ROT_CC = ("Tip.PixOperation.RotCC", "Rotate selected pixels Counter-Clockwise");
	private static readonly LanguageCode LABEL_BORDER = ("Label.Border", "Border");
	private static readonly LanguageCode LABEL_PIVOT = ("Label.Pivot", "Pivot");
	private static readonly LanguageCode LABEL_SIZE = ("Label.Size", "Size");
	private static readonly LanguageCode LABEL_DURATION = ("Label.Duration", "Duration");
	private static readonly LanguageCode RULE_HELP_MSG = ("UI.RuleHelpMsg", "Rule applies to auto-update blocks in map editor.\n\nThe sprite must be placed inside a group for this to work. \n\nName several sprites like: sprite 0, sprite 1, sprite 2... to make them a group.");
	private static readonly LanguageCode MENU_NEW_SPRITE = ("Menu.CreateNewSprite", "New Sprite");
	private static readonly LanguageCode MENU_NEW_PAL_SPRITE = ("Menu.CreateNewPalette", "New Palette");
	private static readonly LanguageCode MENU_NEW_CHAR_SPRITE = ("Menu.CreateNewCharacterSprite", "New Character Sprites");
	private static readonly LanguageCode MENU_NEW_SHEET_CHAR_SPRITE = ("Menu.CreateNewSheetCharacterSprite", "New Sheet Character Sprites");
	private static readonly LanguageCode MENU_NEW_ARMOR_SPRITE = ("Menu.CreateNewArmorSprite", "New Armor Sprites");
	private static readonly LanguageCode MENU_NEW_RULE_TILE_SPRITE = ("Menu.CreateNewRuleTileSprite", "New Rule Tile Sprites");
	private static LanguageCode[] TIP_TOOLS;

	// Data
	private readonly int ToolCount = typeof(Tool).EnumLength();
	private readonly int[] TagCheckedCountCache = new int[TagUtil.TAG_COUNT];
	private bool SelectingAnyTiggerSprite;
	private bool SelectingAnyNonTiggerSprite;
	private bool SelectingAnySpriteWithBorder;
	private bool SelectingAnySpriteWithoutBorder;
	private bool OpeningTilingRuleEditor = false;
	private bool? TilingRuleModeA = true;
	private bool FoldingColorField = true;
	private bool UsePivotLabel = false;
	private int RulePageIndex = 0;
	private string ColorFieldCode = "";
	private IRect RuleEditorRect = default;
	private IRect CreateSpriteBigButtonRect = default;
	private ColorF PaintingColorF = new(0, 0, 0, 0);
	private int Contains9Pivots = 0b_00000000;
	private (int h, int s, int v, int a) ColorAdjustData;


	#endregion




	#region --- MSG ---


	private void Update_Toolbar () {

		if (EditingSheet.Atlas.Count <= 0) return;

		int toolbarSize = GUI.ToolbarSize;
		var toolbarRect = StageRect.EdgeOutside(Direction4.Up, toolbarSize).Expand(0, toolbarSize, 0, 0);

		// BG
		GUI.DrawSlice(UI_TOOLBAR, toolbarRect);
		toolbarRect = toolbarRect.Shrink(Unify(6));
		var rect = toolbarRect.Edge(Direction4.Left, Unify(30));

		if (SelectingSpriteCount == 0) {
			// --- General ---
			Update_GeneralToolbar(toolbarRect, ref rect);
		} else {
			// --- Sprite ---
			Update_SpriteToolbar_Name(ref rect);
			Update_SpriteToolbar_Size(ref rect);
			Update_SpriteToolbar_Border(ref rect);
			Update_SpriteToolbar_Pivot(ref rect);
			Update_SpriteToolbar_Alt(ref rect);
			Update_RuleEditor();
		}

		// Tools
		int padding = Unify(4);
		var toolRect = WindowRect.EdgeRight(toolbarSize).ShrinkUp(toolbarSize);
		GUI.DrawSlice(UI_TOOL_BG, toolRect);
		toolRect = toolRect.Shrink(Unify(6));
		rect = toolRect.Edge(Direction4.Up, Unify(30));
		for (int i = 0; i < ToolCount; i++) {
			bool selecting = CurrentTool == (Tool)i;
			bool newSelecting = GUI.ToggleButton(rect, selecting, UI_TOOLS[i], Skin.SmallDarkButton);
			if (newSelecting && !selecting) {
				SetCurrentTool((Tool)i);
			}
			RequireTooltip(rect, TIP_TOOLS[i]);
			rect.SlideDown(padding);
		}

	}


	private void Update_GeneralToolbar (IRect toolbarRect, ref IRect rect) {

		int padding = Unify(4);

		// Create Sprite Big Button
		if (StagedSprites.Count == 0) {
			if (GUI.DarkButton(
				CreateSpriteBigButtonRect, BuiltInSprite.ICON_PLUS
			)) {
				CreateNewSprite();
				Input.UseMouseKey(0);
			}
			RequireTooltip(CreateSpriteBigButtonRect, TIP_CREATE_SPRITE);
		}

		// Create New Sprite
		if (GUI.Button(rect, ICON_NEW_SPRITE, Skin.SmallDarkButton)) {
			OpenCreateSpriteMenu(rect);
		}
		RequireTooltip(rect, TIP_NEW_SPRITE);
		rect.SlideRight(padding);

		// Show Checker Board
		ShowCheckerBoard.Value = GUI.ToggleButton(rect, ShowCheckerBoard.Value, ICON_SHOW_CHECKER, Skin.SmallDarkButton);
		RequireTooltip(rect, TIP_SHOW_CHECKER);
		rect.SlideRight(padding);

		// Show Axis
		ShowAxis.Value = GUI.ToggleButton(rect, ShowAxis.Value, ICON_SHOW_AXIS, Skin.SmallDarkButton);
		RequireTooltip(rect, TIP_SHOW_AXIS);
		rect.SlideRight(padding);

		// Reset Camera
		if (GUI.Button(rect, ICON_RESET_CAMERA, Skin.SmallDarkButton)) {
			ResetCamera();
		}
		RequireTooltip(rect, TIP_RESET_CAMERA);
		rect.SlideRight(padding);

		// Extra
		switch (CurrentTool) {
			case Tool.Rect:
			case Tool.Line:
			case Tool.Circle:
			case Tool.Bucket: {
				Update_GeneralToolbar_PickingColor(toolbarRect, ref rect);
				break;
			}

			case Tool.Select: {

				rect.x += padding;

				// Pixel Selection Operation
				using var _ = new GUIEnableScope(PixelSelectionPixelRect != default);
				// Flip H
				if (GUI.Button(rect, ICON_OPER_FLIP_H, Skin.SmallDarkButton)) {
					FlipPixelSelection(true);
				}
				RequireTooltip(rect, TIP_OPER_FLIP_H);
				rect.SlideRight(padding);

				// Flip V
				if (GUI.Button(rect, ICON_OPER_FLIP_V, Skin.SmallDarkButton)) {
					FlipPixelSelection(false);
				}
				RequireTooltip(rect, TIP_OPER_FLIP_V);
				rect.SlideRight(padding);

				// Rotate Clock
				if (GUI.Button(rect, ICON_OPER_ROT_C, Skin.SmallDarkButton)) {
					RotatePixelSelection(true);
				}
				RequireTooltip(rect, TIP_OPER_ROT_C);
				rect.SlideRight(padding);

				// Rotate C-Clock
				if (GUI.Button(rect, ICON_OPER_ROT_CC, Skin.SmallDarkButton)) {
					RotatePixelSelection(false);
				}
				RequireTooltip(rect, TIP_OPER_ROT_CC);
				rect.SlideRight(padding);

				// Color Adjustment
				Update_GeneralToolbar_ColorAdjustment(toolbarRect, ref rect);

				break;
			}
		}
	}


	private void Update_GeneralToolbar_PickingColor (IRect toolbarRect, ref IRect rect) {

		if (FoldingColorField && EngineSetting.AlwaysExpandPaintingColor.Value) {
			FoldingColorField = false;
		}
		rect.width = FoldingColorField ? rect.height : Util.Min(Unify(512), toolbarRect.xMax - rect.x);
		if (rect.width < rect.height) return;

		// Color Field
		int padding = Unify(4);
		var newColorF = GUI.HorizontalColorField(
			PaintingColorF, rect,
			stepped: false, alpha: true, folded: FoldingColorField
		);
		if (newColorF != PaintingColorF) {
			PaintingColorF = newColorF;
			PaintingColor = newColorF.ToColor32();
			if (!FoldingColorField) ColorFieldCode = Util.ColorToHtml(PaintingColor);
		}

		// Code Field
		if (!FoldingColorField) {
			var codeFieldRect = rect.EdgeOutside(Direction4.Right, Unify(108));
			rect = rect.Expand(0, codeFieldRect.width, 0, 0);
			ColorFieldCode = GUI.SmallInputField(
				10915243, codeFieldRect, ColorFieldCode, out _, out bool confirm
			);
			if (confirm) {
				if (Util.HtmlToColor(ColorFieldCode, out var newColor)) {
					PaintingColor = newColor;
					PaintingColorF = newColor.ToColorF();
				}
				ColorFieldCode = Util.ColorToHtml(PaintingColor);
			}
		}

		// Hovering / Click
		if (rect.MouseInside()) {
			if (FoldingColorField) {
				Cursor.SetCursorAsHand();
				if (Input.MouseLeftButtonDown) {
					FoldingColorField = false;
					ColorFieldCode = Util.ColorToHtml(PaintingColor);
				}
			} else if (rect.Edge(Direction4.Left, rect.height).MouseInside()) {
				if (Input.MouseLeftButtonDown) FoldingColorField = true;
				Cursor.SetCursorAsHand();
			}
		} else if (!FoldingColorField && Input.MouseLeftButtonDown) {
			FoldingColorField = true;
		}

		// Final
		RequireTooltip(rect, TIP_PAINTING_COLOR);
		rect.SlideRight(padding);
	}


	private void Update_GeneralToolbar_ColorAdjustment (IRect toolbarRect, ref IRect rect) {

		if (PixelSelectionPixelRect == default) return;

		rect.width = Util.Min(Unify(960), toolbarRect.xMax - rect.x);
		if (rect.width < rect.height) return;
		int padding = Unify(6);

		// Slider
		bool noStep = Input.HoldingAlt;

		var cell = Renderer.Draw(BuiltInSprite.COLOR_HUE_ALT, default);
		int newH = DrawSlider(239045, rect.Part(0, 4).Shrink(padding), "H", ColorAdjustData.h, -180, 180, step: noStep ? 0 : 10, out var sliderRect);
		cell.SetRect(sliderRect);

		cell = Renderer.Draw(BuiltInSprite.COLOR_WHITE_BAR, sliderRect, Color32.RED_BETTER);
		int newS = DrawSlider(239046, rect.Part(1, 4).Shrink(padding), "S", ColorAdjustData.s, -100, 100, step: noStep ? 0 : 10, out sliderRect);
		cell.SetRect(sliderRect);

		cell = Renderer.Draw(BuiltInSprite.COLOR_WHITE_BAR, sliderRect, Color32.WHITE);
		int newV = DrawSlider(239047, rect.Part(2, 4).Shrink(padding), "V", ColorAdjustData.v, -100, 100, step: noStep ? 0 : 10, out sliderRect);
		cell.SetRect(sliderRect);

		var chCell = Renderer.Draw(BuiltInSprite.CHECKER_BOARD_8, sliderRect, Color32.WHITE);
		cell = Renderer.Draw(BuiltInSprite.COLOR_WHITE_BAR, sliderRect, Color32.WHITE);
		int newA = DrawSlider(239048, rect.Part(3, 4).Shrink(padding), "A", ColorAdjustData.a, -255, 255, step: noStep ? 0 : 5, out sliderRect);
		cell.SetRect(sliderRect);
		chCell.SetRect(sliderRect);

		// Adjust Logic
		if (newH != ColorAdjustData.h || newS != ColorAdjustData.s || newV != ColorAdjustData.v || newA != ColorAdjustData.a) {
			if (PixelBufferSize == Int2.zero) {
				var oldSelectionRect = PixelSelectionPixelRect;
				SetSelectingPixelAsBuffer(removePixels: true, ignoreUndoStep: true);
				PixelSelectionPixelRect = oldSelectionRect;
				PixelBuffer.CopyTo(PixelBufferBeforeAdjusted, 0);
			}
			AdjustBuffer(
				PixelBufferBeforeAdjusted, PixelBuffer, PixelBufferSize,
				newH / 360f, newS / 100f, newV / 100f, newA / 255f
			);
			Game.FillPixelsIntoTexture(PixelBuffer, PixelBufferGizmosTexture);
			ColorAdjustData.h = newH;
			ColorAdjustData.s = newS;
			ColorAdjustData.v = newV;
			ColorAdjustData.a = newA;
		}
		rect.SlideRight(padding);

		// Func
		static int DrawSlider (int ctrlID, IRect rect, string label, int value, int min, int max, int step, out IRect sliderRect) {

			int labelWidth = Unify(48);
			int lineExp = Unify(2);

			// Slider
			sliderRect = rect.ShrinkLeft(labelWidth);
			value = GUI.BlankSlider(ctrlID, sliderRect, value, min, max, out _, step: step);

			int lineWidth = Unify(2);
			var lineRect = new IRect(
				sliderRect.CenterX(),
				sliderRect.y, lineWidth, sliderRect.height
			);

			// Middle Line
			var midLineRect = lineRect.VerticalMidHalf().Shift(-lineWidth / 2, 0);
			Renderer.Draw(BuiltInSprite.SOFT_LINE_V, midLineRect.ExpandHorizontal(lineExp), Color32.BLACK);
			Renderer.DrawPixel(midLineRect, Color32.GREY_196);

			// Draw Handle Manually 
			lineRect.x = Util.RemapUnclamped(min, max, sliderRect.x, sliderRect.xMax, value) - lineWidth / 2;
			Renderer.Draw(BuiltInSprite.SOFT_LINE_V, lineRect.ExpandHorizontal(lineExp), Color32.BLACK);
			Renderer.DrawPixel(lineRect);

			// Label
			GUI.IntLabel(rect.EdgeLeft(labelWidth).Shift(-GUI.FieldPadding, 0), value, out var bounds, GUI.Skin.SmallRightLabel);
			GUI.Label(
				bounds.EdgeOutside(Direction4.Left, labelWidth / 2).Shift(-GUI.FieldPadding, 0),
				label, GUI.Skin.SmallRightLabel
			);

			// Logic
			if (rect.MouseInside()) {
				if (Input.MouseWheelDelta != 0) {
					value += Input.MouseWheelDelta * (step == 0 ? 1 : step);
				}
				if (Input.MouseMidButtonDown) {
					value = 0;
				}
			}
			return value.Clamp(min, max);
		}
	}


	// Sprite Toolbar
	private void Update_SpriteToolbar_Name (ref IRect rect) {

		if (SelectingSpriteCount == 0) return;

		int padding = Unify(4);
		int fieldWidth = Unify(232);

		// Name
		rect.width = fieldWidth;
		var inputRect = rect.Shrink(0, 0, 0, padding);
		if (InputField(InputName.Name, inputRect)) {
			TryApplySpriteInputFields(forceApply: true);
			RefreshSpriteInputContent();
		}
		rect.SlideRight(padding);

		rect.x += padding;
	}


	private void Update_SpriteToolbar_Size (ref IRect rect) {

		int padding = Unify(4);
		int boxPadding = Unify(2);
		int fieldWidth = Unify(36);

		// Box
		var box = Renderer.DrawPixel(rect, Color32.WHITE_12);

		// Size Label
		rect.x += padding;
		rect.width = Unify(42);
		GUI.Label(rect.Shrink(0, 0, 0, padding), LABEL_SIZE, out var labelBounds, Skin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		// Input
		rect.width = fieldWidth;
		var inputRect = rect.Shrink(0, 0, 0, padding);

		// Width
		if (InputField(InputName.Width, inputRect)) {
			TryApplySpriteInputFields(forceApply: true);
			RefreshSpriteInputContent();
		}
		RequireTooltip(inputRect, TIP_SIZE_X);
		rect.SlideRight(padding);
		inputRect.SlideRight(padding);

		// Height
		if (InputField(InputName.Height, inputRect)) {
			TryApplySpriteInputFields(forceApply: true);
			RefreshSpriteInputContent();
		}
		RequireTooltip(inputRect, TIP_SIZE_Y);
		rect.SlideRight(padding);
		inputRect.SlideRight(padding);

		// Final
		box.Width = rect.xMin - box.X + boxPadding * 2;
		box.X -= boxPadding;
		box.Y -= boxPadding;
		box.Height += boxPadding * 2;

		rect.x += padding * 2;
	}


	private void Update_SpriteToolbar_Border (ref IRect rect) {

		int padding = Unify(4);
		int boxPadding = Unify(2);
		int fieldWidth = Unify(36);
		int buttonWidth = Unify(30);

		// Box
		var box = Renderer.DrawPixel(rect, Color32.WHITE_12);

		// Border Label
		rect.x += padding;
		rect.width = Unify(52);
		GUI.Label(rect.Shrink(0, 0, 0, padding), LABEL_BORDER, out var labelBounds, Skin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		// Make Border
		rect.width = buttonWidth;
		bool newSASWB = !GUI.Toggle(rect, !SelectingAnySpriteWithoutBorder);
		if (newSASWB != SelectingAnySpriteWithoutBorder) {
			SelectingAnySpriteWithoutBorder = newSASWB;
			MakeBorderForSelection(!newSASWB);
		}
		RequireTooltip(rect, SelectingAnySpriteWithoutBorder ? TIP_ENABLE_BORDER : TIP_DISABLE_BORDER);
		rect.SlideRight(padding);

		// Borders
		if (SelectingAnySpriteWithBorder) {

			// Input Fields
			rect.width = fieldWidth;
			var inputRect = rect.Shrink(0, 0, 0, padding);

			// Border L
			if (InputField(InputName.BorderL, inputRect)) {
				TryApplySpriteInputFields(forceApply: true);
				RefreshSpriteInputContent();
			}
			RequireTooltip(inputRect, TIP_BORDER_L);
			rect.SlideRight(padding);
			inputRect.SlideRight(padding);

			// Border R
			if (InputField(InputName.BorderR, inputRect)) {
				TryApplySpriteInputFields(forceApply: true);
				RefreshSpriteInputContent();
			}
			RequireTooltip(inputRect, TIP_BORDER_R);
			rect.SlideRight(padding);
			inputRect.SlideRight(padding);

			// Border D
			if (InputField(InputName.BorderD, inputRect)) {
				TryApplySpriteInputFields(forceApply: true);
				RefreshSpriteInputContent();
			}
			RequireTooltip(inputRect, TIP_BORDER_D);
			rect.SlideRight(padding);
			inputRect.SlideRight(padding);

			// Border U
			if (InputField(InputName.BorderU, inputRect)) {
				TryApplySpriteInputFields(forceApply: true);
				RefreshSpriteInputContent();
			}
			RequireTooltip(inputRect, TIP_BORDER_U);
			rect.SlideRight(padding);

		}

		box.Width = rect.xMin - box.X + boxPadding * 2;
		box.X -= boxPadding;
		box.Y -= boxPadding;
		box.Height += boxPadding * 2;

		rect.x += padding * 2;
	}


	private void Update_SpriteToolbar_Pivot (ref IRect rect) {

		int padding = Unify(4);
		int boxPadding = Unify(2);
		int fieldWidth = Unify(36);

		// Box
		var box = Renderer.DrawPixel(rect, Color32.WHITE_12);

		// Pivot Label
		rect.x += padding;
		rect.width = Unify(52);
		GUI.Label(rect.Shrink(0, 0, 0, padding), LABEL_PIVOT, out var labelBounds, Skin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		if (UsePivotLabel) {
			// Input Fields
			rect.width = fieldWidth;
			var inputRect = rect.Shrink(0, 0, 0, padding);

			// Pivot X
			if (InputField(InputName.PivotX, inputRect)) {
				TryApplySpriteInputFields(forceApply: true);
				RefreshSpriteInputContent();
			}
			RequireTooltip(inputRect, TIP_PIVOT_X);
			rect.SlideRight(padding);
			inputRect.SlideRight(padding);

			// Pivot Y
			if (InputField(InputName.PivotY, inputRect)) {
				TryApplySpriteInputFields(forceApply: true);
				RefreshSpriteInputContent();
			}
			RequireTooltip(inputRect, TIP_PIVOT_Y);
			rect.SlideRight(padding);
		} else {
			// Pivot 9-Button
			rect.width = rect.height;
			for (int i = 0; i < 9; i++) {
				int w = rect.width / 3;
				int h = rect.height / 3;
				var bRect = new IRect(rect.x + (i % 3) * w, rect.y + (i / 3) * h, w, h);
				// Btn
				if (GUI.BlankButton(bRect, out var state)) {
					SetAllSelectingSpritePivot((i % 3) * 500, (i / 3) * 500);
					RefreshSpriteInputContent();
					TryApplySpriteInputFields(forceApply: true);
					RefreshSpriteInputContent();
				}
				// Body
				Renderer.DrawPixel(bRect, Color32.GREY_46);
				// Frame
				using (new GUIColorScope(Color32.GREY_38)) {
					GUI.DrawSlice(BuiltInSprite.FRAME_16, bRect);
				}
				// Highlight
				if (state == GUIState.Hover) {
					Renderer.DrawPixel(bRect, Color32.WHITE_20);
				}
				// Mark
				if (Contains9Pivots.GetBit(i)) {
					Renderer.Draw(BuiltInSprite.CIRCLE_16, bRect, Skin.HighlightColorAlt);
				}
			}
			rect.SlideRight(padding);
		}

		// Switcher
		rect.width = rect.height / 2;
		if (GUI.Button(rect, BuiltInSprite.MENU_THREE_DOTS, Skin.SmallIconButton)) {
			UsePivotLabel = !UsePivotLabel;
		}
		rect.SlideRight(padding);

		// Final
		box.Width = rect.xMin - box.X + boxPadding * 2;
		box.X -= boxPadding;
		box.Y -= boxPadding;
		box.Height += boxPadding * 2;

		rect.x += padding * 2;
	}


	private void Update_SpriteToolbar_Alt (ref IRect rect) {

		int padding = Unify(4);
		int boxPadding = Unify(2);
		int buttonWidth = Unify(30);
		int fieldWidth = Unify(36);

		// Box
		var box = Renderer.DrawPixel(rect, Color32.WHITE_12);

		// Z Label
		rect.x += padding;
		rect.width = Unify(69);
		GUI.Label(rect.Shrink(0, 0, 0, padding), "z", out var labelBounds, Skin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		// Local Z
		rect.width = fieldWidth;
		var inputRect = rect.Shrink(0, 0, 0, padding);
		if (InputField(InputName.Z, inputRect)) {
			TryApplySpriteInputFields(forceApply: true);
			RefreshSpriteInputContent();
		}
		RequireTooltip(inputRect, TIP_Z);
		rect.SlideRight(padding);
		inputRect.SlideRight(padding);

		// Duration Label
		rect.x += padding;
		rect.width = Unify(42);
		GUI.Label(rect.Shrink(0, 0, 0, padding), LABEL_DURATION, out labelBounds, Skin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		// Duration
		rect.width = fieldWidth;
		inputRect = rect.Shrink(0, 0, 0, padding);
		if (InputField(InputName.Duration, inputRect)) {
			TryApplySpriteInputFields(forceApply: true);
			RefreshSpriteInputContent();
		}
		RequireTooltip(inputRect, TIP_DURATION);
		rect.SlideRight(padding * 2);

		// Trigger Toggle
		rect.width = buttonWidth;
		int triggerIcon =
			SelectingAnyNonTiggerSprite && SelectingAnyTiggerSprite ? ICON_TRIGGER_MIX :
			SelectingAnyTiggerSprite ? ICON_TRIGGER_ON : ICON_TRIGGER_OFF;
		bool newSNTS = !GUI.ToggleButton(rect, !SelectingAnyNonTiggerSprite, triggerIcon, Skin.SmallDarkButton);
		if (newSNTS != SelectingAnyNonTiggerSprite) {
			SelectingAnyNonTiggerSprite = newSNTS;
			MakeTriggerForSelection(!newSNTS);
		}
		RequireTooltip(rect, TIP_TRIGGER);
		rect.SlideRight(padding);

		// Tag
		if (GUI.Button(rect, SelectionTagCache == Tag.None ? ICON_TAG : ICON_TAG_MARKED, Skin.SmallDarkButton)) {
			OpenSpriteTagMenu();
		}
		if (SelectionTagCache != Tag.None && EngineSetting.ShowTagPreview.Value) {
			var tagPreviewRect = rect.EdgeOutside(Direction4.Down, Unify(16)).Shift(0, -Unify(4));
			int previewPadding = Unify(2);
			int startIndex = Renderer.GetUsedCellCount();
			for (int i = 0; i < TagUtil.TAG_COUNT; i++) {
				if (!SelectionTagCache.HasAny((Tag)(1 << i))) continue;
				GUI.BackgroundLabel(
					tagPreviewRect, TagUtil.ALL_TAG_NAMES[i],
					Skin.HighlightColorAlt, out var bounds, previewPadding, false, Skin.SmallLabel
				);
				tagPreviewRect.x = bounds.xMax + previewPadding * 4;
			}
			if (Renderer.GetCells(out var cells, out int count)) {
				int shift = (tagPreviewRect.x - rect.x) / 2 - rect.width / 2;
				for (int i = startIndex; i < count; i++) {
					cells[i].X -= shift;
				}
			}
		}
		RequireTooltip(rect, TIP_TAG);
		rect.SlideRight(padding);

		// Rule
		var atlas = EditingSheet.Atlas[CurrentAtlasIndex];
		if (CurrentProject.IsEngineInternalProject || atlas.Type == AtlasType.Level || atlas.Type == AtlasType.Background) {
			if (GUI.Button(rect, ICON_RULE, Skin.SmallDarkButton)) {
				OpeningTilingRuleEditor = !OpeningTilingRuleEditor;
				if (OpeningTilingRuleEditor) {
					StagedSprites.Sort(SpriteDataComparer.Instance);
					TilingRuleModeA = null;
				}
			}
			RequireTooltip(rect, TIP_RULE);
			rect.SlideRight(padding);
		}

		// Delete Sprite
		rect.width = buttonWidth;
		if (GUI.Button(rect, ICON_DELETE_SPRITE, Skin.SmallDarkButton)) {
			DeleteAllSelectingSprite();
		}
		RequireTooltip(rect, TIP_DEL_SPRITE);
		rect.SlideRight(padding);

		// Log Sprite Names
#if DEBUG
		if (GUI.Button(rect, BuiltInSprite.ICON_INFO, Skin.SmallDarkButton)) {
			string result = "";
			foreach (var data in StagedSprites) {
				if (!data.Selecting) continue;
				result += data.Sprite.RealName + "\n";
			}
			Debug.Log(result);
			Game.SetClipboardText(result);
		}
		RequireTooltip(rect, "Log and copy selecting sprite names.");
		rect.SlideRight(padding);
#endif

		// Final
		box.Width = rect.xMin - box.X + boxPadding * 2;
		box.X -= boxPadding;
		box.Y -= boxPadding;
		box.Height += boxPadding * 2;

		rect.x += padding * 2;
	}


	private void Update_RuleEditor () {

		if (!OpeningTilingRuleEditor || SelectingSpriteCount == 0) return;

		// BG
		GUI.DrawSlice(UI_RULE_PANEL, RuleEditorRect);

		var panelRect = RuleEditorRect.Shrink(Unify(8));
		int pageBarHeight = (panelRect.height - panelRect.width) / 2;

		// Label
		var labelRect = panelRect.Edge(Direction4.Up, pageBarHeight).Shift(0, -pageBarHeight);

		// Page
		int helpButtonWidth = pageBarHeight;
		RulePageIndex = RulePageIndex.Clamp(0, SelectingSpriteCount - 1);
		var rect = panelRect.CornerInside(Alignment.TopLeft, helpButtonWidth, pageBarHeight);

		// Switch Mode Button
		if (GUI.Button(rect, TilingRuleModeA.HasValue && TilingRuleModeA.Value ? ICON_RULE_MODE_A : ICON_RULE_MODE_B, Skin.SmallIconButton)) {
			TilingRuleModeA = !TilingRuleModeA.HasValue || !TilingRuleModeA.Value;
		}
		RequireTooltip(rect, TIP_RULE_MODE);
		rect.SlideRight();

		// Prev
		rect.width = (panelRect.width - helpButtonWidth * 2) / 3;
		if (GUI.Button(rect, BuiltInSprite.LEFT_ARROW, Skin.SmallDarkButton)) {
			RulePageIndex = (RulePageIndex - 1).Clamp(0, SelectingSpriteCount - 1);
		}
		rect.SlideRight();

		// Index
		GUI.IntLabel(rect, RulePageIndex, Skin.SmallCenterGreyLabel);
		rect.SlideRight();

		// Next
		if (GUI.Button(rect, BuiltInSprite.RIGHT_ARROW, Skin.SmallDarkButton)) {
			RulePageIndex = (RulePageIndex + 1).Clamp(0, SelectingSpriteCount - 1);
		}
		rect.SlideRight();

		// Help
		rect.width = helpButtonWidth;
		if (GUI.Button(rect, BuiltInSprite.ICON_QUESTION_MARK, Skin.SmallIconButton)) {
			GenericDialogUI.SpawnDialog_Button(RULE_HELP_MSG, BuiltInText.UI_OK, Const.EmptyMethod);
		}

		// Content
		panelRect = panelRect.Shrink(0, 0, 0, pageBarHeight * 2);
		int checkedCount = 0;
		int buttonSize = panelRect.width / 3;
		for (int i = 0; i < StagedSprites.Count && checkedCount < SelectingSpriteCount; i++) {

			var spData = StagedSprites[i];
			if (!spData.Selecting) continue;
			var bRule = spData.Sprite.Rule;

			// Auto Mode
			if (!TilingRuleModeA.HasValue) {
				for (int j = 0; j < 8; j++) {
					var value = bRule[j];
					if (value == Rule.Whatever) continue;
					TilingRuleModeA = (int)value < 3;
					break;
				}
			}

			// Page Index Check
			if (checkedCount < RulePageIndex) {
				checkedCount++;
				continue;
			}

			if (!TilingRuleModeA.HasValue) TilingRuleModeA = true;

			// Name Label
			if (labelRect.width > 0) {
				GUI.Label(labelRect, spData.Sprite.RealName, Skin.SmallCenterLabel);
			}

			// Icon
			using (new SheetIndexScope(EditingSheetIndex)) {
				Renderer.Draw(spData.Sprite, panelRect.CornerInside(Alignment.MidMid, buttonSize).Fit(spData.Sprite));
			}

			// Buttons
			for (int ruleIndex = 0; ruleIndex < 8; ruleIndex++) {
				var buttonRect = panelRect.CornerInside((Alignment)(ruleIndex < 4 ? ruleIndex : ruleIndex + 1), buttonSize);
				if (GUI.Button(buttonRect, RuleNumberToIcon(bRule[ruleIndex]), Skin.IconButton)) {
					var newRule = bRule;
					bool modeA = TilingRuleModeA.Value;
					newRule[ruleIndex] = bRule[ruleIndex] switch {
						Rule.Whatever => modeA ? Rule.SameTile : Rule.AnyTile,
						Rule.SameTile or Rule.AnyTile => modeA ? Rule.NotSameTile : Rule.Empty,
						Rule.NotSameTile or Rule.Empty => 0,
						_ => 0,
					};
					if (!bRule.IsSameWith(newRule)) {
						spData.Sprite.Rule = newRule;
						RegisterUndo(new SpriteRuleUndoItem() {
							SpriteID = spData.Sprite.ID,
							From = bRule,
							To = newRule,
						});
						SetDirty();
					}
				}
				Renderer.DrawSlice(BuiltInSprite.FRAME_16, buttonRect, Color32.GREY_128);
				RequireTooltip(buttonRect, RuleNumberToTip(bRule[ruleIndex]));
			}

			break;
		}

		// Func
		static int RuleNumberToIcon (Rule rule) => rule switch {
			Rule.Whatever => 0,
			Rule.SameTile => ICON_RULE_SAME,
			Rule.NotSameTile => ICON_RULE_NOT_SAME,
			Rule.AnyTile => ICON_RULE_ANY,
			Rule.Empty => ICON_RULE_EMPTY,
			_ => 0,
		};
		static string RuleNumberToTip (Rule rule) => rule switch {
			Rule.Whatever => TIP_RULE_WHATEVER,
			Rule.SameTile => TIP_RULE_SAME,
			Rule.NotSameTile => TIP_RULE_NOT_SAME,
			Rule.AnyTile => TIP_RULE_ANY,
			Rule.Empty => TIP_RULE_EMPTY,
			_ => "",
		};
	}


	#endregion




	#region --- LGC ---


	// Menu
	private void OpenSpriteTagMenu () {

		if (SelectingSpriteCount == 0) return;

		// Cache
		Array.Clear(TagCheckedCountCache);
		int checkedCount = 0;
		int noneCount = 0;
		for (int spriteIndex = 0; spriteIndex < StagedSprites.Count; spriteIndex++) {
			var spData = StagedSprites[spriteIndex];
			if (!spData.Selecting) continue;
			Tag tag = spData.Sprite.Tag;
			if (tag == Tag.None) {
				noneCount++;
			} else {
				for (int i = 0; i < TagUtil.TAG_COUNT; i++) {
					if (tag.HasFlag(TagUtil.GetTagAt(i))) {
						TagCheckedCountCache[i]++;
					}
				}
			}
			checkedCount++;
			if (checkedCount >= SelectingSpriteCount) break;
		}

		// Popup
		GenericPopupUI.BeginPopup();

		// None
		GenericPopupUI.AddItem(
			BuiltInText.UI_NONE,
			icon: 0,
			iconPosition: default,
			checkMarkSprite: noneCount == 0 ? 0 : noneCount >= SelectingSpriteCount ? BuiltInSprite.CHECK_MARK_32 : ICON_MIX,
			OnClickNone,
			enabled: true,
			@checked: noneCount > 0
		);
		// Tags
		for (int i = 0; i < TagUtil.TAG_COUNT; i++) {
			int tagedCount = TagCheckedCountCache[i];
			if (1 << i == (int)Tag.PhysicalDamage) GenericPopupUI.AddSeparator();
			GenericPopupUI.AddItem(
				TagUtil.ALL_TAG_NAMES[i],
				icon: 0,
				iconPosition: default,
				checkMarkSprite: tagedCount == 0 ? 0 : tagedCount >= SelectingSpriteCount ? BuiltInSprite.CHECK_MARK_32 : ICON_MIX,
				OnClick,
				enabled: true,
				@checked: tagedCount > 0,
				data: i
			);
			if (1 << i == (int)Tag.LightenDamage || 1 << i == (int)Tag.OnewayRight) GenericPopupUI.AddSeparator();
		}

		// Func
		static void OnClickNone () {
			int checkedCount = 0;
			var stagedSprites = Instance.StagedSprites;
			int selectingCount = Instance.SelectingSpriteCount;
			for (int i = 0; i < stagedSprites.Count && checkedCount < selectingCount; i++) {
				var spData = stagedSprites[i];
				if (!spData.Selecting) continue;
				checkedCount++;
				Tag oldTag = spData.Sprite.Tag;
				if (oldTag != Tag.None) {
					spData.Sprite.Tag = Tag.None;
					Instance.RegisterUndo(new SpriteTagUndoItem() {
						SpriteID = spData.Sprite.ID,
						From = oldTag,
						To = Tag.None,
					});
				}
			}
			Instance.SetDirty();
		}
		static void OnClick () {
			if (GenericPopupUI.InvokingItemData is not int tagIndex) return;
			if (tagIndex < 0 || tagIndex >= TagUtil.TAG_COUNT) return;
			int checkedCount = 0;
			var stagedSprites = Instance.StagedSprites;
			var checkedCache = Instance.TagCheckedCountCache;
			int selectingCount = Instance.SelectingSpriteCount;
			bool setFlag = checkedCache[tagIndex] < selectingCount;
			Tag flag = TagUtil.GetTagAt(tagIndex);
			for (int i = 0; i < stagedSprites.Count && checkedCount < selectingCount; i++) {
				var spData = stagedSprites[i];
				if (!spData.Selecting) continue;
				checkedCount++;
				Tag oldTag = spData.Sprite.Tag;
				Tag newTag = setFlag ? oldTag | flag : oldTag & ~flag;
				if (oldTag != newTag) {
					spData.Sprite.Tag = newTag;
					Instance.RegisterUndo(new SpriteTagUndoItem() {
						SpriteID = spData.Sprite.ID,
						From = oldTag,
						To = newTag,
					});
				}
			}
			Instance.SetDirty();
		}
	}


	private void OpenCreateSpriteMenu (IRect buttonRect) {
		GenericPopupUI.BeginPopup(buttonRect.position);
		var pixPos = Instance.Stage_to_Pixel(
			new Int2(buttonRect.xMax + Unify(32), buttonRect.y - Unify(32))
		);
		GenericPopupUI.AddItem(MENU_NEW_SPRITE, CreateNew, data: pixPos);
		GenericPopupUI.AddItem(MENU_NEW_PAL_SPRITE, NewPalette, data: pixPos);

		// For All Pose Characters
		GenericPopupUI.AddItem(MENU_NEW_CHAR_SPRITE, Const.EmptyMethod, data: pixPos);
		GenericPopupUI.BeginSubItem();
		GenericPopupUI.AddItem("(Enter Name Here)", NewPoseCharSprite, data: ("", pixPos), editable: true);
		foreach (var filePath in Util.EnumerateFiles(CurrentProject.Universe.CharacterMovementConfigRoot, true, AngePath.MOVEMENT_CONFIG_SEARCH_PATTERN)) {
			string name = Util.GetNameWithoutExtension(filePath);
			GenericPopupUI.AddItem(name, NewPoseCharSprite, data: (name, pixPos));
		}
		GenericPopupUI.EndSubItem();

		// For All Sheet Characters
		GenericPopupUI.AddItem(MENU_NEW_SHEET_CHAR_SPRITE, Const.EmptyMethod, data: pixPos);
		GenericPopupUI.BeginSubItem();
		GenericPopupUI.AddItem("(Enter Name Here)", NewSheetCharSprite, data: ("", pixPos), editable: true);
		foreach (var filePath in Util.EnumerateFiles(CurrentProject.Universe.CharacterMovementConfigRoot, true, AngePath.MOVEMENT_CONFIG_SEARCH_PATTERN)) {
			string name = Util.GetNameWithoutExtension(filePath);
			GenericPopupUI.AddItem(name, NewSheetCharSprite, data: (name, pixPos));
		}
		GenericPopupUI.EndSubItem();

		// Armor Sprites
		GenericPopupUI.AddItem(MENU_NEW_ARMOR_SPRITE, Const.EmptyMethod, data: pixPos);
		GenericPopupUI.BeginSubItem();
		GenericPopupUI.AddItem("(Enter Name Here)", NewArmorSprite, data: ("", pixPos), editable: true);
		GenericPopupUI.EndSubItem();

		// Rule Tiles
		GenericPopupUI.AddItem(MENU_NEW_RULE_TILE_SPRITE, Const.EmptyMethod, data: pixPos);
		GenericPopupUI.BeginSubItem();
		GenericPopupUI.AddItem("(Enter Name Here)", NewRuleTileSprite, data: ("", pixPos), editable: true);
		GenericPopupUI.EndSubItem();

		// Func
		static void CreateNew () {
			if (GenericPopupUI.InvokingItemData is not Int2 pixPos) return;
			pixPos.y -= 32;
			Instance.CreateNewSprite(pixelPos: pixPos);
		}
		static void NewPalette () {
			if (GenericPopupUI.InvokingItemData is not Int2 pixPos) return;
			Instance.CreateSpriteForPalette(false, pixelPos: pixPos);
		}
		static void NewPoseCharSprite () {
			if (GenericPopupUI.InvokingItemData is not (string name, Int2 pixPos)) return;
			string resultName = string.IsNullOrEmpty(name) ? GenericPopupUI.InvokingItemlabel : name;
			Instance.CreateSpritesFromTemplates(resultName, "CharacterTemplate", pixPos);
		}
		static void NewSheetCharSprite () {
			if (GenericPopupUI.InvokingItemData is not (string name, Int2 pixPos)) return;
			string resultName = string.IsNullOrEmpty(name) ? GenericPopupUI.InvokingItemlabel : name;
			Instance.CreateSpritesFromTemplates(resultName, "SheetCharacterTemplate", pixPos);
		}
		static void NewArmorSprite () {
			if (GenericPopupUI.InvokingItemData is not (string name, Int2 pixPos)) return;
			string resultName = string.IsNullOrEmpty(name) ? GenericPopupUI.InvokingItemlabel : name;
			Instance.CreateSpritesFromTemplates(resultName, "ArmorTemplate", pixPos);
		}
		static void NewRuleTileSprite () {
			if (GenericPopupUI.InvokingItemData is not (string name, Int2 pixPos)) return;
			string resultName = string.IsNullOrEmpty(name) ? GenericPopupUI.InvokingItemlabel : name;
			Instance.CreateSpritesFromTemplates(resultName, "RuleTileTemplate", pixPos);
		}
	}


	// Input Field
	private bool InputField (InputName name, IRect rect) {
		int index = (int)name;
		INPUT_TEXT[index] = GUI.SmallInputField(
			BASIC_INPUT_ID + index, rect, INPUT_TEXT[index],
			out _, out bool confirm
		);
		return confirm;
	}


	private void TryApplySpriteInputFields (bool forceApply = false, bool ignoreUndoStep = false) {

		if (SelectingSpriteCount == 0) return;

		string name = null;
		int sizeX = -1;
		int sizeY = -1;
		int borderL = -1;
		int borderR = -1;
		int borderD = -1;
		int borderU = -1;
		int pivotX = int.MinValue;
		int pivotY = int.MinValue;
		int z = int.MinValue;
		int duration = -1;

		// All Fields
		for (int i = 0; i < INPUT_TEXT.Length; i++) {
			string text = INPUT_TEXT[i];
			if (string.IsNullOrWhiteSpace(text) || text == "*") continue;
			if (forceApply || GUI.TypingTextFieldID == BASIC_INPUT_ID + i) {
				var type = (InputName)i;
				if (type == InputName.Name) {
					// Name Field
					name = text.TrimEnd();
				} else {
					// String Field
					if (!int.TryParse(text, out int result)) continue;
					switch (type) {
						case InputName.Width:
							sizeX = result.GreaterOrEquel(1);
							break;
						case InputName.Height:
							sizeY = result.GreaterOrEquel(1);
							break;
						case InputName.BorderL:
							borderL = (result * Const.ART_SCALE).GreaterOrEquelThanZero();
							break;
						case InputName.BorderR:
							borderR = (result * Const.ART_SCALE).GreaterOrEquelThanZero();
							break;
						case InputName.BorderD:
							borderD = (result * Const.ART_SCALE).GreaterOrEquelThanZero();
							break;
						case InputName.BorderU:
							borderU = (result * Const.ART_SCALE).GreaterOrEquelThanZero();
							break;
						case InputName.PivotX:
							pivotX = result * 10;
							break;
						case InputName.PivotY:
							pivotY = result * 10;
							break;
						case InputName.Z:
							z = result;
							break;
						case InputName.Duration:
							duration = result.GreaterOrEquelThanZero();
							break;
					}
				}
			}
		}

		// Any Valid
		if (
			name == null && sizeX < 0 && sizeY < 0 &&
			borderL < 0 && borderR < 0 && borderD < 0 && borderU < 0 &&
			pivotX == int.MinValue && pivotY == int.MinValue && z == int.MinValue && duration < 0
		) return;

		// Sort for Name
		if (!string.IsNullOrEmpty(name)) {
			var hoveringData = HoveringSpriteStageIndex >= 0 ? StagedSprites[HoveringSpriteStageIndex] : null;
			var resizingData = HoveringResizeStageIndex >= 0 ? StagedSprites[HoveringResizeStageIndex] : null;
			StagedSprites.Sort(SpriteSorterByXY.Instance);
			if (hoveringData != null) {
				HoveringSpriteStageIndex = StagedSprites.IndexOf(hoveringData);
			}
			if (resizingData != null) {
				HoveringResizeStageIndex = StagedSprites.IndexOf(resizingData);
			}
		}

		// Buffer for Rename
		int checkedCount = 0;
		if (!string.IsNullOrEmpty(name) && SelectingSpriteCount > 1) {
			string tempName = "soiweiusj-asdfius723jIUF";
			for (int i = 0; i < StagedSprites.Count && checkedCount < SelectingSpriteCount; i++) {
				var spData = StagedSprites[i];
				if (!spData.Selecting) continue;
				checkedCount++;
				var sprite = spData.Sprite;
				// Name
				bool renamed;
				string oldName = sprite.RealName;
				renamed = EditingSheet.RenameSprite(sprite, $"{tempName} {checkedCount - 1}");
				if (renamed) {
					RegisterUndo(new SpriteNameUndoItem() {
						SpriteID = sprite.ID,
						From = oldName,
						To = sprite.RealName,
					}, true);
				}
			}
		}

		// Final
		checkedCount = 0;
		for (int i = 0; i < StagedSprites.Count && checkedCount < SelectingSpriteCount; i++) {

			var spData = StagedSprites[i];
			if (!spData.Selecting) continue;
			checkedCount++;

			var sprite = spData.Sprite;
			var pixRect = sprite.PixelRect;

			// Border
			var oldBorder = sprite.GlobalBorder;
			var border = Int4.Direction(
				borderL >= 0 ? borderL.Clamp(0, sprite.GlobalWidth - oldBorder.right) : oldBorder.left,
				borderR >= 0 ? borderR.Clamp(0, sprite.GlobalWidth - oldBorder.left) : oldBorder.right,
				borderD >= 0 ? borderD.Clamp(0, sprite.GlobalHeight - oldBorder.up) : oldBorder.down,
				borderU >= 0 ? borderU.Clamp(0, sprite.GlobalHeight - oldBorder.down) : oldBorder.up
			);
			if (border != oldBorder) {
				sprite.GlobalBorder = border;
				RegisterUndo(new SpriteBorderUndoItem() {
					SpriteID = sprite.ID,
					From = oldBorder,
					To = border,
				}, ignoreUndoStep);
			}

			// Size Changed
			if ((sizeX > 0 && sizeX != pixRect.width) || (sizeY > 0 && sizeY != pixRect.height)) {
				var oldPixels = sprite.Pixels;
				var oldRect = sprite.PixelRect;
				var newRect = new IRect(pixRect.x, pixRect.y, sizeX, sizeY);
				RegisterUndo(new SpriteRectUndoItem() {
					SpriteID = sprite.ID,
					From = oldRect,
					To = newRect,
					Start = true,
				}, ignoreUndoStep);
				sprite.ResizePixelRect(
					newRect,
					resizeBorder: !border.IsZero,
					out bool contentChanged
				);
				spData.PixelDirty = true;
				if (contentChanged) {
					RegisterUndoForPixelChangesWhenResize(sprite, oldRect, oldPixels);
				}
				RegisterUndo(new SpriteRectUndoItem() {
					SpriteID = sprite.ID,
					From = oldRect,
					To = newRect,
					Start = false,
				}, ignoreUndoStep);
			}

			// Name
			if (!string.IsNullOrEmpty(name)) {
				bool renamed;
				string oldName = sprite.RealName;
				if (SelectingSpriteCount == 1) {
					renamed = EditingSheet.RenameSprite(sprite, name);
				} else {
					renamed = EditingSheet.RenameSprite(sprite, $"{name} {checkedCount - 1}");
				}
				if (renamed) {
					RegisterUndo(new SpriteNameUndoItem() {
						SpriteID = sprite.ID,
						From = oldName,
						To = sprite.RealName,
					}, true);
				}
			}

			// Pivot X
			if (pivotX != int.MinValue && pivotX != sprite.PivotX) {
				RegisterUndo(new SpritePivotUndoItem() {
					SpriteID = sprite.ID,
					From = sprite.PivotX,
					To = pivotX,
					X = true,
				}, ignoreUndoStep);
				sprite.PivotX = pivotX;
			}

			// Pivot Y
			if (pivotY != int.MinValue && pivotY != sprite.PivotY) {
				RegisterUndo(new SpritePivotUndoItem() {
					SpriteID = sprite.ID,
					From = sprite.PivotY,
					To = pivotY,
					X = false,
				}, ignoreUndoStep);
				sprite.PivotY = pivotY;
			}

			// Z
			if (z != int.MinValue && z != sprite.LocalZ) {
				RegisterUndo(new SpriteZUndoItem() {
					SpriteID = sprite.ID,
					From = sprite.LocalZ,
					To = z,
				}, ignoreUndoStep);
				sprite.LocalZ = z;
			}

			// Duration
			if (duration >= 0 && duration != sprite.Duration) {
				RegisterUndo(new SpriteDurationUndoItem() {
					SpriteID = sprite.ID,
					From = sprite.Duration,
					To = duration,
				}, ignoreUndoStep);
				sprite.Duration = duration;
			}

			// Final
			SetDirty();
		}

		GUI.CancelTyping();
	}


	private void RefreshSpriteInputContent () {

		if (SelectingSpriteCount == 0) {
			INPUT_TEXT.FillWithValue("");
			return;
		}

		string name = null;
		int sizeX = int.MinValue;
		int sizeY = int.MinValue;
		int borderL = int.MinValue;
		int borderR = int.MinValue;
		int borderD = int.MinValue;
		int borderU = int.MinValue;
		int pivotX = int.MinValue;
		int pivotY = int.MinValue;
		int z = int.MinValue;
		int duration = int.MinValue;
		int starCount = 0;

		foreach (var spData in StagedSprites) {

			if (!spData.Selecting) continue;
			var border = spData.Sprite.GlobalBorder;

			// Name
			if (name != "*") {
				if (name == null) {
					name = spData.Sprite.RealName;
				} else {
					name = "*";
					starCount++;
				}
			}

			// Width
			if (sizeX != int.MaxValue) {
				if (sizeX == int.MinValue) {
					sizeX = spData.Sprite.GlobalWidth;
				} else if (sizeX != spData.Sprite.GlobalWidth) {
					sizeX = int.MaxValue;
					starCount++;
				}
			}
			// Height
			if (sizeY != int.MaxValue) {
				if (sizeY == int.MinValue) {
					sizeY = spData.Sprite.GlobalHeight;
				} else if (sizeY != spData.Sprite.GlobalHeight) {
					sizeY = int.MaxValue;
					starCount++;
				}
			}
			// Border L
			if (borderL != int.MaxValue) {
				if (borderL == int.MinValue) {
					borderL = border.left;
				} else if (borderL != border.left) {
					borderL = int.MaxValue;
					starCount++;
				}
			}
			// Border R
			if (borderR != int.MaxValue) {
				if (borderR == int.MinValue) {
					borderR = border.right;
				} else if (borderR != border.right) {
					borderR = int.MaxValue;
					starCount++;
				}
			}
			// Border D
			if (borderD != int.MaxValue) {
				if (borderD == int.MinValue) {
					borderD = border.down;
				} else if (borderD != border.down) {
					borderD = int.MaxValue;
					starCount++;
				}
			}
			// Border U
			if (borderU != int.MaxValue) {
				if (borderU == int.MinValue) {
					borderU = border.up;
				} else if (borderU != border.up) {
					borderU = int.MaxValue;
					starCount++;
				}
			}

			// PivotX
			if (pivotX != int.MaxValue) {
				if (pivotX == int.MinValue) {
					pivotX = spData.Sprite.PivotX / 10;
				} else if (pivotX != spData.Sprite.PivotX / 10) {
					pivotX = int.MaxValue;
					starCount++;
				}
			}

			// PivotY
			if (pivotY != int.MaxValue) {
				if (pivotY == int.MinValue) {
					pivotY = spData.Sprite.PivotY / 10;
				} else if (pivotY != spData.Sprite.PivotY / 10) {
					pivotY = int.MaxValue;
					starCount++;
				}
			}

			// Z
			if (z != int.MaxValue) {
				if (z == int.MinValue) {
					z = spData.Sprite.LocalZ;
				} else if (z != spData.Sprite.LocalZ) {
					z = int.MaxValue;
					starCount++;
				}
			}

			// Duration
			if (duration != int.MaxValue) {
				if (duration == int.MinValue) {
					duration = spData.Sprite.Duration;
				} else if (duration != spData.Sprite.Duration) {
					duration = int.MaxValue;
					starCount++;
				}
			}

			// Star Check
			if (starCount >= 11) break;
		}

		// Final
		INPUT_TEXT[(int)InputName.Name] = name;
		INPUT_TEXT[(int)InputName.Width] = sizeX == int.MinValue || sizeX == int.MaxValue ? "*" : (sizeX / Const.ART_SCALE).ToString();
		INPUT_TEXT[(int)InputName.Height] = sizeY == int.MinValue || sizeY == int.MaxValue ? "*" : (sizeY / Const.ART_SCALE).ToString();
		INPUT_TEXT[(int)InputName.BorderL] = borderL == int.MinValue || borderL == int.MaxValue ? "*" : (borderL / Const.ART_SCALE).ToString();
		INPUT_TEXT[(int)InputName.BorderR] = borderR == int.MinValue || borderR == int.MaxValue ? "*" : (borderR / Const.ART_SCALE).ToString();
		INPUT_TEXT[(int)InputName.BorderD] = borderD == int.MinValue || borderD == int.MaxValue ? "*" : (borderD / Const.ART_SCALE).ToString();
		INPUT_TEXT[(int)InputName.BorderU] = borderU == int.MinValue || borderU == int.MaxValue ? "*" : (borderU / Const.ART_SCALE).ToString();
		INPUT_TEXT[(int)InputName.PivotX] = pivotX == int.MinValue || pivotX == int.MaxValue ? "*" : pivotX.ToString();
		INPUT_TEXT[(int)InputName.PivotY] = pivotY == int.MinValue || pivotY == int.MaxValue ? "*" : pivotY.ToString();
		INPUT_TEXT[(int)InputName.Z] = z == int.MinValue || z == int.MaxValue ? "*" : z.ToString();
		INPUT_TEXT[(int)InputName.Duration] = duration == int.MinValue || duration == int.MaxValue ? "*" : duration.ToString();

	}


	#endregion




}