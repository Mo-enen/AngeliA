using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework;

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

	// Data
	private static int ForceBackgroundTintFrame = int.MinValue;


	// MSG
	[OnGameUpdatePauseless]
	public static void OnGameUpdatePauseless () {
		if (
			Game.GlobalFrame == ForceBackgroundTintFrame + 1 ||
			(Game.GlobalFrame % 36000 == 0 && Game.GlobalFrame >= ForceBackgroundTintFrame)
		) {
			RefreshSkyTintFromDateTime();
		}
	}


	[OnGameRestart]
	public static void RefreshSkyTintFromDateTime () {
		var date = System.DateTime.Now;
		float time01 = Util.InverseLerp(0, 24 * 3600, date.Hour * 3600 + date.Minute * 60 + date.Second);
		SkyTintTopColor = GradientTop.Evaluate(time01);
		SkyTintBottomColor = GradientBottom.Evaluate(time01);
	}


	public static void ForceSkyboxTint (Color32 color) => ForceSkyboxTint(color, color);
	public static void ForceSkyboxTint (Color32 top, Color32 bottom) {
		ForceBackgroundTintFrame = Game.GlobalFrame + 1;
		SkyTintTopColor = top;
		SkyTintBottomColor = bottom;
	}


}
