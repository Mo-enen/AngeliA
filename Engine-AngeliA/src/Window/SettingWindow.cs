using System.Collections;
using System.Collections.Generic;
using AngeliA;


namespace AngeliaEngine;

[RequireLanguageFromField]
public class SettingWindow : WindowUI {





	#region --- VAR ---


	// Const
	private static readonly LanguageCode LABEL_ENGINE = ("Setting.Engine", "Engine");
	private static readonly LanguageCode LABEL_PIXEL_EDITOR = ("Setting.PixelEditorLabel", "Pixel Editor");
	private static readonly LanguageCode LABEL_PE_BG_COLOR = ("Setting.PE.BgColor", "Background Color");
	private static readonly LanguageCode LABEL_PE_CANVAS_COLOR = ("Setting.PE.CanvasBgColor", "Canvas Background Color");
	private static readonly LanguageCode LABEL_PE_SOLID_PAINTING = ("Setting.PE.SolidPaintingPreview", "Solid Painting Preview");
	private static readonly LanguageCode LABEL_OPEN_LAST_PROJECT_ON_START = ("Setting.OpenLastProjectOnStart", "Open Last Project on Start");
	private static readonly LanguageCode LABEL_ONLY_SPRITE_ON_OPTION = ("Setting.ASAOOHOK", "Only Modify Spirte on Holding Ctrl");
	private static readonly LanguageCode LABEL_USE_TOOLTIP = ("Setting.UseTooltip", "Show Tooltip");

	// Data
	private ColorF PixEditor_BackgroundColor;
	private ColorF PixEditor_CanvasBackgroundColor;
	private int MasterScroll = 0;
	private int UIHeight = 0;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		PixEditor_BackgroundColor = PixelEditor.BackgroundColor.Value.ToColorF();
		PixEditor_CanvasBackgroundColor = PixelEditor.CanvasBackgroundColor.Value.ToColorF();
	}


	public override void UpdateWindowUI () {
		int itemHeight = Unify(32);
		using var _ = Scope.GUILabelWidth(384);
		int extendedUISize = 1;
		using (var scroll = Scope.GUIScroll(WindowRect, MasterScroll, 0, UIHeight)) {
			MasterScroll = scroll.ScrollPosition;
			var rect = WindowRect.Shrink(
				Unify(96), Unify(96), Unify(42), Unify(42)
			).EdgeInside(Direction4.Up, itemHeight);

			DrawPanel(ref rect, 0);
			DrawPanel(ref rect, 1);

			extendedUISize = WindowRect.yMax - rect.yMax + Unify(128);
			UIHeight = (extendedUISize - WindowRect.height).GreaterOrEquelThanZero();
		}
		MasterScroll = GUI.ScrollBar(
			92645,
			WindowRect.EdgeInside(Direction4.Right, Unify(12)),
			MasterScroll,
			extendedUISize,
			WindowRect.height
		);
	}


	private void Update_Engine (ref IRect rect) {

		int itemPadding = Unify(4);

		// Label - Engine
		GUI.Label(rect.Shift(-Unify(32), 0), LABEL_ENGINE, GUISkin.SmallGreyLabel);
		rect.SlideDown(itemPadding);

		// Open Last Project on Start
		Engine.OpenLastProjectOnStart = GUI.Toggle(
			rect, Engine.OpenLastProjectOnStart, LABEL_OPEN_LAST_PROJECT_ON_START,
			labelStyle: GUISkin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Use Tooltip
		Engine.UseTooltip = GUI.Toggle(
			rect, Engine.UseTooltip, LABEL_USE_TOOLTIP,
			labelStyle: GUISkin.SmallLabel
		);
		rect.SlideDown(itemPadding);

	}


	private void Update_PixelEditor (ref IRect rect) {

		int itemPadding = Unify(4);

		// Label - PixEditor
		GUI.Label(rect.Shift(-Unify(32), 0), LABEL_PIXEL_EDITOR, GUISkin.SmallGreyLabel);
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

		// Allow Spirte Action Only On Holding Option Key
		PixelEditor.AllowSpirteActionOnlyOnHoldingOptionKey.Value = GUI.Toggle(
			rect, PixelEditor.AllowSpirteActionOnlyOnHoldingOptionKey.Value, LABEL_ONLY_SPRITE_ON_OPTION,
			labelStyle: GUISkin.SmallLabel
		);
		rect.SlideDown(itemPadding);

	}


	#endregion




	#region --- LGC ---


	private void DrawPanel (ref IRect rect, int panelID) {
		int labelOffset = Unify(32);
		int boxPadding = Unify(8);
		var box = Renderer.DrawPixel(default, Color32.WHITE_12);
		int boxTop = rect.yMax;
		int boxLeft = rect.xMin;
		int boxRight = rect.xMax;
		switch (panelID) {
			case 0: Update_Engine(ref rect); break;
			case 1: Update_PixelEditor(ref rect); break;
		}
		box.X = boxLeft - boxPadding - labelOffset;
		box.Y = rect.yMax - boxPadding;
		box.Width = boxRight - boxLeft + boxPadding * 2 + labelOffset;
		box.Height = boxTop - rect.yMax + boxPadding * 2;
		rect.y -= Unify(24);
	}


	#endregion




}