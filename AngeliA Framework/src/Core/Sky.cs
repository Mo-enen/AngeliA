using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Core system handles background rendering of the game
/// </summary>
public static class Sky {


	// Api
	/// <summary>
	/// Top color of the sky liner gradient
	/// </summary>
	public static Color32 SkyTintTopColor { get; private set; }
	/// <summary>
	/// Bottom color of the sky liner gradient
	/// </summary>
	public static Color32 SkyTintBottomColor { get; private set; }
	/// <summary>
	/// Tint color for day-light
	/// </summary>
	public static Color32 SunlightTintColor { get; private set; } = Color32.WHITE;
	/// <summary>
	/// Gradient for "SkyTintTopColor" along in-game time
	/// </summary>
	public static ColorGradient GradientTop { get; set; } = new ColorGradient(
		(new Color32(0, 41, 75), 0f / 4f),
		(new Color32(0, 75, 128), 1f / 4f),
		(new Color32(21, 110, 181), 2f / 4f),
		(new Color32(0, 75, 128), 3f / 4f),
		(new Color32(0, 41, 75), 4f / 4f)
	);
	/// <summary>
	/// Gradient for "SkyTintBottomColor" along in-game time
	/// </summary>
	public static ColorGradient GradientBottom { get; set; } = new ColorGradient(
		(new Color32(0, 41, 75), 0f / 4f),
		(new Color32(0, 75, 128), 1f / 4f),
		(new Color32(21, 110, 181), 2f / 4f),
		(new Color32(0, 75, 128), 3f / 4f),
		(new Color32(0, 41, 75), 4f / 4f)
	);
	/// <summary>
	/// Gradient for "SunlightTintColor" along in-game time
	/// </summary>
	public static ColorGradient SunlightTint { get; set; } = new ColorGradient(
		(new Color32(190, 230, 255, 255), 0f),
		(new Color32(255, 250, 240, 255), 0.5f),
		(new Color32(190, 230, 255, 255), 1f)
	);
	/// <summary>
	/// Current in-game time (0 means 0:00. 0.5 means 12:00. 1 means 24:00)
	/// </summary>
	public static float InGameDaytime01 { get; private set; }

	// Data
	private static int ForceBackgroundTintFrame = int.MinValue;
	private static float ForceInGameDaytimeValue = -1f;
	private static System.DateTime GameInitDate;


	// MSG
	[OnGameInitialize]
	internal static void OnGameInitialize () {
		GameInitDate = System.DateTime.Now;
		InGameDaytime01 = GetInGameDaytimeFromRealTime();
	}


	[OnGameUpdatePauseless]
	internal static void OnGameUpdatePauseless () {

		if (Game.IsToolApplication) return;

		bool everySec = Game.PauselessFrame % 60 == 0;

		// Refresh In-Game Daytime
		if (ForceInGameDaytimeValue >= 0f) {
			InGameDaytime01 = ForceInGameDaytimeValue;
		} else if (everySec) {
			InGameDaytime01 = GetInGameDaytimeFromRealTime();
		}

		// Refresh Sky Tint
		if (
			Game.PauselessFrame == ForceBackgroundTintFrame + 1 ||
			(everySec && Game.PauselessFrame > ForceBackgroundTintFrame)
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
	internal static void RefreshSkyTintFromDateTime () {
		if (Game.PauselessFrame <= ForceBackgroundTintFrame) return;
		SkyTintTopColor = GradientTop.Evaluate(InGameDaytime01);
		SkyTintBottomColor = GradientBottom.Evaluate(InGameDaytime01);
	}


	/// <summary>
	/// Make both top and bottom sky tint gradient color into given color for specified frames long
	/// </summary>
	public static void ForceSkyboxTint (Color32 color, int duration = 1) => ForceSkyboxTint(color, color, duration);


	/// <summary>
	/// Make top and bottom sky tint gradient color into given colors for specified frames long
	/// </summary>
	public static void ForceSkyboxTint (Color32 top, Color32 bottom, int duration = 1) {
		ForceBackgroundTintFrame = Game.PauselessFrame + duration;
		SkyTintTopColor = top;
		SkyTintBottomColor = bottom;
	}


	/// <summary>
	/// Set current in-game time. (0 means 0:00. 0.5 means 12:00. 1 means 24:00. -1 means real world time)
	/// </summary>
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


	/// <summary>
	/// Get in-game time from current date time in real world
	/// </summary>
	/// <returns>(0 means 0:00. 0.5 means 12:00. 1 means 24:00)</returns>
	public static float GetInGameDaytimeFromRealTime () {
		int initSec = GameInitDate.Hour * 3600 + GameInitDate.Minute * 60 + GameInitDate.Second;
		int frameSec = Game.PauselessFrame / 60;
		return Util.InverseLerp(0, 24 * 3600, initSec + frameSec);
	}


}
