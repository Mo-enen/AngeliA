using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class Sky {


	// Api
	public static Color32 SkyTintTopColor { get; private set; }
	public static Color32 SkyTintBottomColor { get; private set; }
	public static Color32 DaylightTintColor { get; private set; } = Color32.WHITE;
	public static ColorGradient GradientTop { get; set; } = new ColorGradient(
		new ColorGradient.Data(new Color32(10, 12, 31, 255), 0f),
		new ColorGradient.Data(new Color32(13, 49, 76, 255), 0.25f),
		new ColorGradient.Data(new Color32(29, 156, 219, 255), 0.5f),
		new ColorGradient.Data(new Color32(13, 49, 76, 255), 0.75f),
		new ColorGradient.Data(new Color32(10, 12, 31, 255), 1f)
	);
	public static ColorGradient GradientBottom { get; set; } = new ColorGradient(
		new ColorGradient.Data(new Color32(10, 12, 31, 255), 0f),
		new ColorGradient.Data(new Color32(27, 69, 101, 255), 0.25f),
		new ColorGradient.Data(new Color32(52, 171, 230, 255), 0.5f),
		new ColorGradient.Data(new Color32(27, 69, 101, 255), 0.75f),
		new ColorGradient.Data(new Color32(10, 12, 31, 255), 1f)
	);
	public static ColorGradient DaylightTint { get; set; } = new ColorGradient(
		new ColorGradient.Data(new Color32(190, 230, 255, 255), 0f),
		new ColorGradient.Data(new Color32(255, 240, 220, 255), 0.5f),
		new ColorGradient.Data(new Color32(190, 230, 255, 255), 1f)
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
			DaylightTintColor = DaylightTint != null ? DaylightTint.Evaluate(InGameDaytime01) : Color32.WHITE;
		}

		// Daylight Tint
		if (DaylightTintColor != Color32.WHITE) {
			Game.PassEffect_Tint(DaylightTintColor, 1);
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
		InGameDaytime01 = ForceInGameDaytimeValue = newDaytime01.Clamp01();
		if (newDaytime01 < 0f) {
			InGameDaytime01 = GetInGameDaytimeFromRealTime();
		}
		RefreshSkyTintFromDateTime();
		DaylightTintColor = DaylightTint != null ? DaylightTint.Evaluate(InGameDaytime01) : Color32.WHITE;
	}


	public static float GetInGameDaytimeFromRealTime () {
		var date = System.DateTime.Now;
		return Util.InverseLerp(0, 24 * 3600, date.Hour * 3600 + date.Minute * 60 + date.Second);
	}


}
