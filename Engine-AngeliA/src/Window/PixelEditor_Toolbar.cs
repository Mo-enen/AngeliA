using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public partial class PixelEditor {




	#region --- VAR ---


	// Const
	private const int INPUT_ID_N = 123631253;
	private const int INPUT_ID_W = 123631254;
	private const int INPUT_ID_H = 123631255;
	private const int INPUT_ID_BL = 123631256;
	private const int INPUT_ID_BR = 123631257;
	private const int INPUT_ID_BD = 123631258;
	private const int INPUT_ID_BU = 123631259;
	private const int INPUT_ID_PX = 123631260;
	private const int INPUT_ID_PY = 123631261;
	private static readonly SpriteCode ICON_DELETE_SPRITE = "Icon.DeleteSprite";
	private static readonly SpriteCode ICON_SHOW_BG = "Icon.ShowBackground";
	private static readonly LanguageCode TIP_SHOW_BG = ("Tip.ShowBG", "Show background");
	private static readonly LanguageCode TIP_RESET_CAMERA = ("Tip.ResetCamera", "Reset Camera");
	private static readonly LanguageCode TIP_DEL_SLICE = ("Tip.DeleteSlice", "Delete slice");
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
	private static readonly LanguageCode TIP_SLICE_NAME = ("Tip.SpriteName", "Name");
	private static readonly LanguageCode TIP_TRIGGER = ("Tip.Trigger", "Is trigger sprite");
	private static readonly LanguageCode LABEL_BORDER = ("Label.Border", "Border");
	private static readonly LanguageCode LABEL_PIVOT = ("Label.Pivot", "Pivot");
	private static readonly LanguageCode LABEL_SIZE = ("Label.Size", "Size");
	private static readonly LanguageCode LABEL_TRIGGER = ("Label.Trigger", "Trigger");

	// Data
	private bool SelectingAnyNonTiggerSprite;
	private bool SelectingAnySpriteWithBorder;
	private bool SelectingAnySpriteWithoutBorder;
	private string SliceNameInput = "";
	private string SliceWidthInput = "";
	private string SliceHeightInput = "";
	private string SliceBorderInputL = "";
	private string SliceBorderInputR = "";
	private string SliceBorderInputD = "";
	private string SliceBorderInputU = "";
	private string SlicePivotInputX = "";
	private string SlicePivotInputY = "";


	#endregion




	#region --- MSG ---


	private void Update_StageToolbar () {

		if (Sheet.Atlas.Count <= 0) return;

		int buttonWidth = Unify(30);
		int padding = Unify(4);
		var toolbarRect = StageRect.EdgeOutside(Direction4.Up, Unify(TOOLBAR_HEIGHT));

		// BG
		Renderer.DrawPixel(toolbarRect, Color32.GREY_20);
		toolbarRect = toolbarRect.Shrink(Unify(6));
		var rect = toolbarRect.EdgeInside(Direction4.Left, buttonWidth);

		if (SelectingSpriteCount == 0) {
			// --- General ---

			// Show BG
			ShowBackground.Value = GUI.ToggleButton(rect, ShowBackground.Value, ICON_SHOW_BG, GUISkin.SmallDarkButton);
			RequireToolLabel(rect, TIP_SHOW_BG);
			rect.SlideRight(padding);

			// Reset Camera
			if (GUI.Button(rect, BuiltInSprite.ICON_REFRESH, GUISkin.SmallDarkButton)) {
				ResetCamera();
			}
			RequireToolLabel(rect, TIP_RESET_CAMERA);
			rect.SlideRight(padding);

			// Import from PNG
			if (GUI.Button(rect, ICON_IMPORT_PNG, GUISkin.SmallDarkButton)) {
				ShowImportAtlasBrowser(false);
			}
			RequireToolLabel(rect, TIP_IMPORT_PNG);
			rect.SlideRight(padding);

		} else {
			// --- Slice ---

			Update_SliceToolbar_Name(ref rect);
			Update_SliceToolbar_Size(ref rect);
			Update_SliceToolbar_Border(ref rect);
			Update_SliceToolbar_Pivot(ref rect);
			Update_SliceToolbar_Alt(ref rect);

			// Delete Sprite
			rect.width = buttonWidth;
			if (GUI.Button(rect, ICON_DELETE_SPRITE, GUISkin.SmallDarkButton)) {
				DeleteAllSelectingSprite();
			}
			RequireToolLabel(rect, TIP_DEL_SLICE);
			rect.SlideRight(padding);
		}
	}


	private void Update_SliceToolbar_Name (ref IRect rect) {

		if (SelectingSpriteCount == 0) return;

		int padding = Unify(4);
		int fieldWidth = Unify(232);

		// Name
		rect.width = fieldWidth;
		var inputRect = rect.Shrink(0, 0, 0, padding);
		if (SelectingSpriteCount == 1) {
			SliceNameInput = GUI.InputField(
				INPUT_ID_N, inputRect, SliceNameInput, out _, out bool confirm, GUISkin.SmallInputField
			);
			if (confirm) {
				TryApplySliceInputFields(forceApply: true);
				RefreshSliceInputContent();
			}
		} else {
			using (Scope.GUIEnable(false)) {
				GUI.InputField(
					INPUT_ID_N, inputRect, "*", out _, out _, GUISkin.SmallInputField
				);
			}
		}
		RequireToolLabel(inputRect, TIP_SLICE_NAME);
		rect.SlideRight(padding);

		rect.x += padding;
	}


	private void Update_SliceToolbar_Size (ref IRect rect) {

		int padding = Unify(4);
		int boxPadding = Unify(2);
		int fieldWidth = Unify(36);

		// Box
		var box = Renderer.DrawPixel(rect, Color32.WHITE_12);

		// Size Label
		rect.x += padding;
		rect.width = Unify(42);
		GUI.Label(rect.Shrink(0, 0, 0, padding), LABEL_SIZE, out var labelBounds, GUISkin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		// Input
		rect.width = fieldWidth;
		var inputRect = rect.Shrink(0, 0, 0, padding);

		// Width
		SliceWidthInput = GUI.InputField(
			INPUT_ID_W, inputRect, SliceWidthInput, out _, out bool confirm, GUISkin.SmallInputField
		);
		if (confirm) {
			TryApplySliceInputFields(forceApply: true);
			RefreshSliceInputContent();
		}
		RequireToolLabel(inputRect, TIP_SIZE_X);
		rect.SlideRight(padding);
		inputRect.SlideRight(padding);

		// Height
		SliceHeightInput = GUI.InputField(
			INPUT_ID_H, inputRect, SliceHeightInput, out _, out confirm, GUISkin.SmallInputField
		);
		if (confirm) {
			TryApplySliceInputFields(forceApply: true);
			RefreshSliceInputContent();
		}
		RequireToolLabel(inputRect, TIP_SIZE_Y);
		rect.SlideRight(padding);
		inputRect.SlideRight(padding);

		// Final
		box.Width = rect.xMin - box.X + boxPadding * 2;
		box.X -= boxPadding;
		box.Y -= boxPadding;
		box.Height += boxPadding * 2;

		rect.x += padding * 2;
	}


	private void Update_SliceToolbar_Border (ref IRect rect) {

		int padding = Unify(4);
		int boxPadding = Unify(2);
		int fieldWidth = Unify(36);
		int buttonWidth = Unify(30);

		// Box
		var box = Renderer.DrawPixel(rect, Color32.WHITE_12);

		// Border Label
		rect.x += padding;
		rect.width = Unify(52);
		GUI.Label(rect.Shrink(0, 0, 0, padding), LABEL_BORDER, out var labelBounds, GUISkin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		// Make Border
		rect.width = buttonWidth;
		bool newSASWB = !GUI.Toggle(rect, !SelectingAnySpriteWithoutBorder);
		if (newSASWB != SelectingAnySpriteWithoutBorder) {
			SelectingAnySpriteWithoutBorder = newSASWB;
			MakeBorderForSelection(!newSASWB);
		}
		RequireToolLabel(rect, SelectingAnySpriteWithoutBorder ? TIP_ENABLE_BORDER : TIP_DISABLE_BORDER);
		rect.SlideRight(padding);

		// Borders
		if (SelectingAnySpriteWithBorder) {

			// Input Fields
			rect.width = fieldWidth;
			var inputRect = rect.Shrink(0, 0, 0, padding);

			// Border L
			SliceBorderInputL = GUI.InputField(
				INPUT_ID_BL, inputRect, SliceBorderInputL, out _, out bool confirm, GUISkin.SmallInputField
			);
			if (confirm) {
				TryApplySliceInputFields(forceApply: true);
				RefreshSliceInputContent();
			}
			RequireToolLabel(inputRect, TIP_BORDER_L);
			rect.SlideRight(padding);
			inputRect.SlideRight(padding);

			// Border R
			SliceBorderInputR = GUI.InputField(
				INPUT_ID_BR, inputRect, SliceBorderInputR, out _, out confirm, GUISkin.SmallInputField
			);
			if (confirm) {
				TryApplySliceInputFields(forceApply: true);
				RefreshSliceInputContent();
			}
			RequireToolLabel(inputRect, TIP_BORDER_R);
			rect.SlideRight(padding);
			inputRect.SlideRight(padding);

			// Border D
			SliceBorderInputD = GUI.InputField(
				INPUT_ID_BD, inputRect, SliceBorderInputD, out _, out confirm, GUISkin.SmallInputField
			);
			if (confirm) {
				TryApplySliceInputFields(forceApply: true);
				RefreshSliceInputContent();
			}
			RequireToolLabel(inputRect, TIP_BORDER_D);
			rect.SlideRight(padding);
			inputRect.SlideRight(padding);

			// Border U
			SliceBorderInputU = GUI.InputField(
				INPUT_ID_BU, inputRect, SliceBorderInputU, out _, out confirm, GUISkin.SmallInputField
			);
			if (confirm) {
				TryApplySliceInputFields(forceApply: true);
				RefreshSliceInputContent();
			}
			RequireToolLabel(inputRect, TIP_BORDER_U);
			rect.SlideRight(padding);

		}

		box.Width = rect.xMin - box.X + boxPadding * 2;
		box.X -= boxPadding;
		box.Y -= boxPadding;
		box.Height += boxPadding * 2;

		rect.x += padding * 2;
	}


	private void Update_SliceToolbar_Pivot (ref IRect rect) {

		int padding = Unify(4);
		int boxPadding = Unify(2);
		int fieldWidth = Unify(36);

		// Box
		var box = Renderer.DrawPixel(rect, Color32.WHITE_12);

		// Pivot Label
		rect.x += padding;
		rect.width = Unify(52);
		GUI.Label(rect.Shrink(0, 0, 0, padding), LABEL_PIVOT, out var labelBounds, GUISkin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		// Input Fields
		rect.width = fieldWidth;
		var inputRect = rect.Shrink(0, 0, 0, padding);

		// Pivot X
		SlicePivotInputX = GUI.InputField(
			INPUT_ID_PX, inputRect, SlicePivotInputX, out _, out bool confirm, GUISkin.SmallInputField
		);
		if (confirm) {
			TryApplySliceInputFields(forceApply: true);
			RefreshSliceInputContent();
		}
		RequireToolLabel(inputRect, TIP_PIVOT_X);
		rect.SlideRight(padding);
		inputRect.SlideRight(padding);

		// Pivot Y
		SlicePivotInputY = GUI.InputField(
			INPUT_ID_PY, inputRect, SlicePivotInputY, out _, out confirm, GUISkin.SmallInputField
		);
		if (confirm) {
			TryApplySliceInputFields(forceApply: true);
			RefreshSliceInputContent();
		}
		RequireToolLabel(inputRect, TIP_PIVOT_Y);
		rect.SlideRight(padding);

		// Final
		box.Width = rect.xMin - box.X + boxPadding * 2;
		box.X -= boxPadding;
		box.Y -= boxPadding;
		box.Height += boxPadding * 2;

		rect.x += padding * 2;
	}


	private void Update_SliceToolbar_Alt (ref IRect rect) {

		int padding = Unify(4);
		int boxPadding = Unify(2);
		int buttonWidth = Unify(30);

		// Box
		var box = Renderer.DrawPixel(rect, Color32.WHITE_12);

		// Trigger Label
		rect.x += padding;
		rect.width = Unify(52);
		GUI.Label(rect.Shrink(0, 0, 0, padding), LABEL_TRIGGER, out var labelBounds, GUISkin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		// Trigger Toggle
		rect.width = buttonWidth;
		bool newSNTS = !GUI.Toggle(rect, !SelectingAnyNonTiggerSprite);
		if (newSNTS != SelectingAnyNonTiggerSprite) {
			SelectingAnyNonTiggerSprite = newSNTS;
			MakeTriggerForSelection(!newSNTS);
		}
		RequireToolLabel(rect, TIP_TRIGGER);
		rect.SlideRight(padding);




		// Final
		box.Width = rect.xMin - box.X + boxPadding * 2;
		box.X -= boxPadding;
		box.Y -= boxPadding;
		box.Height += boxPadding * 2;

		rect.x += padding * 2;
	}


	#endregion




	#region --- LGC ---



	#endregion




}
