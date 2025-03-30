using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class Sky {


	// Api
	public static Color32 SkyTintTopColor { get; private set; }
	public static Color32 SkyTintBottomColor { get; private set; }
	public static Color32 SunlightTintColor { get; private set; } = Color32.WHITE;
	public static ColorGradient GradientTop { get; set; } = new ColorGradient(
		(new Color32(0, 41, 75), 0f / 4f),
		(new Color32(0, 75, 128), 1f / 4f),
		(new Color32(21, 110, 181), 2f / 4f),
		(new Color32(0, 75, 128), 3f / 4f),
		(new Color32(0, 41, 75), 4f / 4f)
	);
	public static ColorGradient GradientBottom { get; set; } = new ColorGradient(
		(new Color32(0, 41, 75), 0f / 4f),
		(new Color32(0, 75, 128), 1f / 4f),
		(new Color32(21, 110, 181), 2f / 4f),
		(new Color32(0, 75, 128), 3f / 4f),
		(new Color32(0, 41, 75), 4f / 4f)
	);
	public static ColorGradient SunlightTint { get; set; } = new ColorGradient(
		(new Color32(190, 230, 255, 255), 0f),
		(new Color32(255, 250, 240, 255), 0.5f),
		(new Color32(190, 230, 255, 255), 1f)
	);
	public static float InGameDaytime01 { get; private set; }

	// Data
	private static int ForceBackgroundTintFrame = int.MinValue;
	private static float ForceInGameDaytimeValue = -1f;


	// MSG
	[OnGameInitialize]
	internal static void OnGameInitialize () => InGameDaytime01 = GetInGameDaytimeFromRealTime();


	[OnGameUpdatePauseless]
	internal static void OnGameUpdatePauseless () {

		if (Game.IsToolApplication) return;

		bool everyMinute = Game.PauselessFrame % 3600 == 0;

		// Refresh In-Game Daytime
		if (ForceInGameDaytimeValue >= 0f) {
			InGameDaytime01 = ForceInGameDaytimeValue;
		} else if (everyMinute) {
			InGameDaytime01 = GetInGameDaytimeFromRealTime();
		}

		// Refresh Sky Tint
		if (
			Game.PauselessFrame == ForceBackgroundTintFrame + 1 ||
			(everyMinute && Game.PauselessFrame > ForceBackgroundTintFrame)
		) {
			RefreshSkyTintFromDateTime();
			SunlightTintColor = SunlightTint != null ? SunlightTint.Evaluate(InGameDaytime01) : Color32.WHITE;
		}

		// Sunlight Tint
		if (SunlightTintColor != Color32.WHITE) {
			Renderer.MultLayerTint(RenderLayer.BEHIND, SunlightTintColor);
			Renderer.MultLayerTint(RenderLayer.DEFAULT, SunlightTintColor);
		}
	}


	[OnGameRestart]
	public static void RefreshSkyTintFromDateTime () {
		if (Game.PauselessFrame <= ForceBackgroundTintFrame) return;
		SkyTintTopColor = GradientTop.Evaluate(InGameDaytime01);
		SkyTintBottomColor = GradientBottom.Evaluate(InGameDaytime01);
	}


	public static void ForceSkyboxTint (Color32 color, int duration = 1) => ForceSkyboxTint(color, color, duration);
	public static void ForceSkyboxTint (Color32 top, Color32 bottom, int duration = 1) {
		ForceBackgroundTintFrame = Game.PauselessFrame + duration;
		SkyTintTopColor = top;
		SkyTintBottomColor = bottom;
	}


	public static void SetInGameDaytime (float newDaytime01) {
		if (newDaytime01 < 0f) {
			ForceInGameDaytimeValue = -1f;
			InGameDaytime01 = GetInGameDaytimeFromRealTime();
		} else {
			InGameDaytime01 = ForceInGameDaytimeValue = newDaytime01.Clamp01();
		}
		RefreshSkyTintFromDateTime();
		SunlightTintColor = SunlightTint != null ? SunlightTint.Evaluate(InGameDaytime01) : Color32.WHITE;
	}


	public static float GetInGameDaytimeFromRealTime () {
		var date = System.DateTime.Now;
		return Util.InverseLerp(0, 24 * 3600, date.Hour * 3600 + date.Minute * 60 + date.Second);
	}


}
