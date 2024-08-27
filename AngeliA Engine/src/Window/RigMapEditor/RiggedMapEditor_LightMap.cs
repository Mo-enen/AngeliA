using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public partial class RiggedMapEditor {


	// VAR
	private static readonly LanguageCode LABEL_PIXEL_STYLE = ("UI.RigEditor.PixelStyle", "Use Pixel Style");
	private static readonly LanguageCode LABEL_SELF_LERP = ("UI.RigEditor.SelfLerp", "Self Lerp");
	private static readonly LanguageCode LABEL_SOLID_ILLU = ("UI.RigEditor.SolidIllu", "Solid Illuminance");
	private static readonly LanguageCode LABEL_AIR_ILLU = ("UI.RigEditor.AirIllu", "Air Illuminance");
	private static readonly LanguageCode LABEL_BG_TINT = ("UI.RigEditor.BgTint", "Background Tint");
	public bool LightMapSettingChanged { get; set; } = false;


	// MSG
	private void DrawLightingPanel (ref IRect panelRect) {

		// Min Width
		int minWidth = Unify(396);
		if (panelRect.width < minWidth) {
			panelRect.xMin = panelRect.xMax - minWidth;
		}

		// Content
		int panelPadding = Unify(12);
		int padding = GUI.FieldPadding;
		int toolbarSize = Unify(28);
		int top = panelRect.y;
		int digitWidth = GUI.Unify(64);
		var rect = new IRect(panelRect.x, panelRect.y - toolbarSize, panelRect.width, toolbarSize);
		rect = rect.Shrink(panelPadding, panelPadding, 0, 0);
		GUI.BeginChangeCheck();

		// Pixel Style
		LightingSystem.PixelStyle = GUI.Toggle(
			rect, LightingSystem.PixelStyle, LABEL_PIXEL_STYLE,
			labelStyle: GUI.Skin.SmallLabel
		);
		rect.SlideDown(padding);

		// Self Lerp
		GUI.SmallLabel(rect, LABEL_SELF_LERP);
		LightingSystem.SelfLerp = GUI.HandleSlider(
			37423672, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(LightingSystem.SelfLerp * 1000), 0, 1000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(LightingSystem.SelfLerp * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Solid Illuminance
		GUI.SmallLabel(rect, LABEL_SOLID_ILLU);
		LightingSystem.SolidIlluminance = GUI.HandleSlider(
			37423673, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(LightingSystem.SolidIlluminance * 1000), 1000, 2000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(LightingSystem.SolidIlluminance * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Air Illuminance
		GUI.SmallLabel(rect, LABEL_AIR_ILLU);
		LightingSystem.AirIlluminance = GUI.HandleSlider(
			37423674, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(LightingSystem.AirIlluminance * 1000), 0, 1000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(LightingSystem.AirIlluminance * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Background Tint
		GUI.SmallLabel(rect, LABEL_BG_TINT);
		LightingSystem.BackgroundTint = GUI.HandleSlider(
			37423675, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(LightingSystem.BackgroundTint * 1000), 0, 1000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(LightingSystem.BackgroundTint * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Reset
		rect.SlideDown();
		if (GUI.DarkButton(rect, BuiltInText.UI_RESET)) {
			LightingSystem.PixelStyle = false;
			LightingSystem.SelfLerp = 0.9f;
			LightingSystem.SolidIlluminance = 1.05f;
			LightingSystem.AirIlluminance = 0.8f;
			LightingSystem.BackgroundTint = 0.8f;
			LightMapSettingChanged = true;
		}
		rect.SlideDown(padding);

		// Final
		panelRect.height = top - rect.yMax;
		panelRect.y -= panelRect.height;
		if (GUI.EndChangeCheck()) {
			LightMapSettingChanged = true;
		}

	}

}
