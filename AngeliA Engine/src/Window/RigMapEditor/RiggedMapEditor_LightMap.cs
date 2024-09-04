using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public partial class RiggedMapEditor {


	// VAR
	private static readonly LanguageCode LABEL_PIXEL_STYLE = ("UI.RigEditor.PixelStyle", "Use Pixel Style");
	private static readonly LanguageCode LABEL_SELF_LERP = ("UI.RigEditor.SelfLerp", "Self Lerp");
	private static readonly LanguageCode LABEL_SOLID_ILLU = ("UI.RigEditor.SolidIllu", "Solid Illuminance");
	private static readonly LanguageCode LABEL_AIR_ILLU_DAY = ("UI.RigEditor.AirIlluDay", "Air Illuminance (Day)");
	private static readonly LanguageCode LABEL_AIR_ILLU_NIGHT = ("UI.RigEditor.AirIlluNight", "Air Illuminance (Night)");
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
		var info = CurrentProject.Universe.Info;
		GUI.BeginChangeCheck();

		// Pixel Style
		info.LightMap_PixelStyle = GUI.Toggle(
			rect, info.LightMap_PixelStyle, LABEL_PIXEL_STYLE,
			labelStyle: GUI.Skin.SmallLabel
		);
		rect.SlideDown(padding);

		// Self Lerp
		GUI.SmallLabel(rect, LABEL_SELF_LERP);
		info.LightMap_SelfLerp = GUI.HandleSlider(
			37423672, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(info.LightMap_SelfLerp * 1000), 0, 1000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(info.LightMap_SelfLerp * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Solid Illuminance
		GUI.SmallLabel(rect, LABEL_SOLID_ILLU);
		info.LightMap_SolidIlluminance = GUI.HandleSlider(
			37423673, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(info.LightMap_SolidIlluminance * 1000), 1000, 2000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(info.LightMap_SolidIlluminance * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Air Illuminance D
		GUI.SmallLabel(rect, LABEL_AIR_ILLU_DAY);
		info.LightMap_AirIlluminanceDay = GUI.HandleSlider(
			37423674, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(info.LightMap_AirIlluminanceDay * 1000), 0, 1000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(info.LightMap_AirIlluminanceDay * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Air Illuminance N
		GUI.SmallLabel(rect, LABEL_AIR_ILLU_NIGHT);
		info.LightMap_AirIlluminanceNight = GUI.HandleSlider(
			37413674, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(info.LightMap_AirIlluminanceNight * 1000), 0, 1000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(info.LightMap_AirIlluminanceNight * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Background Tint
		GUI.SmallLabel(rect, LABEL_BG_TINT);
		info.LightMap_BackgroundTint = GUI.HandleSlider(
			37423675, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(info.LightMap_BackgroundTint * 1000), 0, 1000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(info.LightMap_BackgroundTint * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Reset
		rect.SlideDown();
		if (GUI.DarkButton(rect, BuiltInText.UI_RESET)) {
			info.LightMap_PixelStyle = false;
			info.LightMap_SelfLerp = 0.88f;
			info.LightMap_SolidIlluminance = 1f;
			info.LightMap_AirIlluminanceDay = 0.8f;
			info.LightMap_AirIlluminanceNight = 0.3f;
			info.LightMap_BackgroundTint = 0.5f;
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
