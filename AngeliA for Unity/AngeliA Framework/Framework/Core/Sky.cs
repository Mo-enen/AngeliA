using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public static class Sky {


		// Api
		public static Byte4 SkyTintTopColor { get; private set; }
		public static Byte4 SkyTintBottomColor { get; private set; }
		public static ColorGradient GradientTop { get; set; } = new ColorGradient(
			new ColorGradient.Data(new Byte4(10, 12, 31, 255), 0f),
			new ColorGradient.Data(new Byte4(13, 49, 76, 255), 0.25f),
			new ColorGradient.Data(new Byte4(29, 156, 219, 255), 0.5f),
			new ColorGradient.Data(new Byte4(13, 49, 76, 255), 0.75f),
			new ColorGradient.Data(new Byte4(10, 12, 31, 255), 1f)
		);
		public static ColorGradient GradientBottom { get; set; } = new ColorGradient(
			new ColorGradient.Data(new Byte4(10, 12, 31, 255), 0f),
			new ColorGradient.Data(new Byte4(27, 69, 101, 255), 0.25f),
			new ColorGradient.Data(new Byte4(52, 171, 230, 255), 0.5f),
			new ColorGradient.Data(new Byte4(27, 69, 101, 255), 0.75f),
			new ColorGradient.Data(new Byte4(10, 12, 31, 255), 1f)
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
			Game.SetSkyboxTint(SkyTintTopColor, SkyTintBottomColor);
		}


		public static void ForceSkyboxTint (Byte4 top, Byte4 bottom) {
			ForceBackgroundTintFrame = Game.GlobalFrame + 1;
			SkyTintTopColor = top;
			SkyTintBottomColor = bottom;
			Game.SetSkyboxTint(SkyTintTopColor, SkyTintBottomColor);
		}


	}
}
