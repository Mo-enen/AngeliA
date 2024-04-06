using System.Collections;
using System.Collections.Generic;
using AngeliA;


namespace AngeliaEngine;

[RequireLanguageFromField]
public class SettingWindow : WindowUI {





	#region --- VAR ---


	// Const
	private static readonly LanguageCode LABEL_PIXEL_EDITOR = ("Setting.PixelEditorLabel", "Pixel Editor");
	private static readonly LanguageCode LABEL_PE_BG_COLOR = ("Setting.PE.BgColor", "Background Color");
	private static readonly LanguageCode LABEL_PE_CANVAS_COLOR = ("Setting.PE.CanvasBgColor", "Canvas Background Color");
	private static readonly LanguageCode LABEL_PE_SOLID_PAINTING = ("Setting.PE.SolidPaintingPreview", "Solid Painting Preview");

	// Data
	private ColorF PixEditor_BackgroundColor;
	private ColorF PixEditor_CanvasBackgroundColor;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		PixEditor_BackgroundColor = PixelEditor.BackgroundColor.Value.ToColorF();
		PixEditor_CanvasBackgroundColor = PixelEditor.CanvasBackgroundColor.Value.ToColorF();
	}


	public override void UpdateWindowUI () {

		int contentPaddingH = Unify(42);
		int contentPaddingV = Unify(20);
		int fieldHeight = Unify(32);
		int itemPadding = Unify(4);
		var rect = WindowRect.Shrink(
			contentPaddingH, contentPaddingH, contentPaddingV, contentPaddingV
		).EdgeInside(Direction4.Up, fieldHeight);

		// Label - PixEditor
		GUI.Label(rect, LABEL_PIXEL_EDITOR, GUISkin.SmallGreyLabel);
		rect.SlideDown(itemPadding);

		// Background Color
		PixEditor_BackgroundColor = GUI.HorizontalColorField(
			PixEditor_BackgroundColor,
			rect,
			label: LABEL_PE_BG_COLOR,
			labelStyle: GUISkin.SmallLabel,
			defaultColor: PixelEditor.BackgroundColor.DefaultValue.ToColorF()
		);
		PixelEditor.BackgroundColor.Value = PixEditor_BackgroundColor.ToColor32();
		rect.SlideDown(itemPadding);

		// Canvas Background Color
		PixEditor_CanvasBackgroundColor = GUI.HorizontalColorField(
			PixEditor_CanvasBackgroundColor,
			rect,
			label: LABEL_PE_CANVAS_COLOR,
			labelStyle: GUISkin.SmallLabel,
			defaultColor: PixelEditor.CanvasBackgroundColor.DefaultValue.ToColorF()
		);
		PixelEditor.CanvasBackgroundColor.Value = PixEditor_CanvasBackgroundColor.ToColor32();
		rect.SlideDown(itemPadding);

		// Solid Painting Preview
		PixelEditor.SolidPaintingPreview.Value = GUI.Toggle(
			rect, PixelEditor.SolidPaintingPreview.Value, LABEL_PE_SOLID_PAINTING,
			labelStyle: GUISkin.SmallLabel
		);
		rect.SlideDown(itemPadding);


	}


	#endregion




	#region --- LGC ---



	#endregion




}