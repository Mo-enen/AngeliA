using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

public static class Sky {


	// Api
	public static Color32 SkyTintTopColor { get; private set; }
	public static Color32 SkyTintBottomColor { get; private set; }
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
	public static float InGameDaytime01 { get; private set; }

	// Data
	private static int ForceBackgroundTintFrame = int.MinValue;


	// MSG
	[OnGameInitialize]
	internal static void OnGameInitialize () => InGameDaytime01 = GetInGameDaytime01();


	[OnGameUpdatePauseless]
	internal static void OnGameUpdatePauseless () {
		// Refresh In-Game Daytime
		if (Game.PauselessFrame % 3600 == 0) { // every minute
			InGameDaytime01 = GetInGameDaytime01();
		}
		// Refresh Sky Tint
		if (
			Game.PauselessFrame == ForceBackgroundTintFrame + 1 ||
			(Game.PauselessFrame % 3600 == 0 && Game.PauselessFrame > ForceBackgroundTintFrame)
		) {
			RefreshSkyTintFromDateTime();
		}
	}


	[OnGameRestart]
	public static void RefreshSkyTintFromDateTime () {
		SkyTintTopColor = GradientTop.Evaluate(InGameDaytime01);
		SkyTintBottomColor = GradientBottom.Evaluate(InGameDaytime01);
	}


	public static void ForceSkyboxTint (Color32 color, int duration = 1) => ForceSkyboxTint(color, color, duration);
	public static void ForceSkyboxTint (Color32 top, Color32 bottom, int duration = 1) {
		ForceBackgroundTintFrame = Game.PauselessFrame + duration;
		SkyTintTopColor = top;
		SkyTintBottomColor = bottom;
	}


	public static float GetInGameDaytime01 () {
		var date = System.DateTime.Now;
		return Util.InverseLerp(0, 24 * 3600, date.Hour * 3600 + date.Minute * 60 + date.Second);
	}


}
