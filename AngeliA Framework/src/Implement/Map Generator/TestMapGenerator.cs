using System.Collections;
using System.Collections.Generic;
using JordanPeck;

namespace AngeliA;

#if DEBUG
internal class TestMapGeneratorAlt0 : TestMapGenerator { }
internal class TestMapGeneratorAlt1 : TestMapGenerator { public override float Priority => 5f; }
internal class TestMapGeneratorAlt2 : TestMapGenerator { }
internal class TestMapGeneratorAlt3 : TestMapGenerator { }

internal class TestMapGenerator : MapGenerator {


	private static readonly FastNoiseLite RegionNoise = new();
	private static readonly int[] AltitudeCache = new int[Const.MAP * Const.MAP];




	public override MapGenerationResult GenerateMap (IBlockSquad squad, Int3 startPoint, Direction8? startDirection) {




		return MapGenerationResult.Skipped;
	}


	// Noise
	[CheatCode("TestNoise")]
	internal static void StartNoiseTesting () {
		if (!Game.IsToolApplication) return;
		QTest.ClearAll();
		QTest.ShowTest();
		QTest.SetObject("testing", "noise");
	}


	[OnGameUpdate]
	internal static void NoiseTestUpdate () {

		if (!Game.IsToolApplication || !QTest.Testing) return;
		bool testingNoise = QTest.TryGetObject("testing", out string testingName) && testingName == "noise";
		if (!testingNoise) return;

		// TEST
		QTest.ShowNotUpdatedData = false;
		if (!QTest.TryGetObject("noise", out FastNoiseLite noise)) {
			noise = QTest.SetObject("noise", new FastNoiseLite());
		}

		// Misc
		int dimension = QTest.Int("Dimension", 3, 1, 3);

		QTest.Group("General");

		float speedX = QTest.Float("Speed X", 0.1f, 0f, 5f, 0.1f);
		float speedY = QTest.Float("Speed Y", 0.1f, 0f, 5f, 0.1f);
		float speedZ = dimension == 3 ? QTest.Float("Speed Z", 0.3f, 0f, 5f, 0.1f) : 0;
		QTest.TryGetObject("currentX", out float currentX);
		QTest.TryGetObject("currentY", out float currentY);
		QTest.TryGetObject("currentZ", out float currentZ);
		QTest.SetObject("currentX", currentX + speedX);
		QTest.SetObject("currentY", currentY + speedY);
		QTest.SetObject("currentZ", currentZ + speedZ);

		int fqScale = QTest.Int("Frequency Scale", 3, 1, 5);

		noise.SetFrequency(QTest.Float(
			"Frequency", 0.02f, 0.01f, 0.10f, 0.01f
		) * fqScale switch {
			1 => 0.01f,
			2 => 0.1f,
			3 => 1f,
			4 => 10f,
			5 => 100f,
			_ => 1f,
		});
		noise.SetNoiseType((NoiseType)QTest.Int(
			"Noise Type", 0, 0, 5,
			displayLabel: noise.CurrentNoiseType.ToString()
		));

		// Fractal
		QTest.Group("Fractal", folding: true);

		noise.SetFractalType((FractalType)QTest.Int(
			"Fractal Type", 3, 0, 3,
			displayLabel: noise.CurrentFractalType.ToString()
		));
		if (noise.CurrentFractalType != FractalType.None) {
			int oct = QTest.Int(
				"Fractal Octaves", 3, 1, 6
			);
			noise.SetFractalOctaves(oct);
			if (oct > 1) {
				noise.SetFractalGain(QTest.Float(
					"Fractal Gain", 0.5f, 0f, 1f, 0.1f
				));
				noise.SetFractalLacunarity(QTest.Float(
					"Fractal Lacunarity", 2.0f, 0f, 5f, 0.1f
				));
				noise.SetFractalWeightedStrength(QTest.Float(
					"Fractal WeightedStrength", 0f, 0f, 2f, 0.1f
				));
			}
			if (noise.CurrentFractalType == FractalType.PingPong) {
				noise.SetFractalPingPongStrength(QTest.Float(
					"Fractal PingPongStrength", 2.0f, 0f, 5f, 0.1f
				));
			}
		}

		// Cellular
		QTest.Group("Cellular", folding: true);

		if (noise.CurrentNoiseType == NoiseType.Cellular) {

			noise.SetCellularDistanceFunction((CellularDistanceFunction)QTest.Int(
				"Cellular Dis-Func", 1, 0, 3,
				displayLabel: noise.CurrentCellularDistanceFunction.ToString()
			));
			noise.SetCellularReturnType((CellularReturnType)QTest.Int(
				"Cellular Return Type", 1, 0, 6,
				displayLabel: noise.CurrentCellularReturnType.ToString()
			));
			noise.SetCellularJitter(QTest.Float(
				"Cellular Jitter", 1.0f, 0f, 5f, 0.1f
			));
		}

		// View
		QTest.Group("");
		const int SIZE = 196;
		switch (dimension) {
			case 1:
				QTest.StartDrawColumn("View 1D", SIZE, clearPrevPixels: false);
				for (int i = 0; i < SIZE; i++) {
					float value = noise.GetNoise01(currentX + i, currentY);
					QTest.DrawColumn(i, value, Color32.WHITE, Color32.GREY_12);
				}
				break;
			case 2:
				QTest.StartDrawPixels("View 2D", SIZE, SIZE, clearPrevPixels: false);
				for (int j = 0; j < SIZE; j++) {
					for (int i = 0; i < SIZE; i++) {
						float value = noise.GetNoise01(currentX + i, currentY + j);
						byte rgb = (byte)(value * 256f).Clamp(0, 255);
						QTest.DrawPixel(i, j, new Color32(rgb, rgb, rgb, 255));
					}
				}
				break;
			case 3:
				QTest.StartDrawPixels("View 3D", SIZE, SIZE, clearPrevPixels: false);
				for (int j = 0; j < SIZE; j++) {
					for (int i = 0; i < SIZE; i++) {
						float value = noise.GetNoise01(currentX + i, currentY + j, currentZ);
						byte rgb = (byte)(value * 256f).Clamp(0, 255);
						QTest.DrawPixel(i, j, new Color32(rgb, rgb, rgb, 255));
					}
				}
				break;
		}
	}


	// Region
	[CheatCode("TestRegion")]
	internal static void StartTestRegion () {
		if (!Game.IsToolApplication) return;
		QTest.ClearAll();
		QTest.ShowTest();
		QTest.SetObject("testing", "region");
		long newSeed = new System.Random(
			(int)(System.DateTime.Now.Ticks + System.Environment.UserName.AngeHash())
		).NextInt64(long.MinValue, long.MaxValue);
		InitNoiseForRegion(RegionNoise, newSeed);
	}


	[OnGameUpdate]
	internal static void RegionTestUpdate () {

		if (!Game.IsToolApplication || !QTest.Testing) return;
		bool testingRegion = QTest.TryGetObject("testing", out string testingName) && testingName == "region";
		if (!testingRegion) return;

		// Config
		int rCount = QTest.Int("Region Count", 16, 1, 64);
		int speedX = QTest.Int("Speed X", 6, 0, 128);
		int speedY = QTest.Int("Speed Y", 6, 0, 128);
		int speedZ = QTest.Int("Speed Z", 0, 0, 128);
		int zoom = QTest.Int("Zoom", 16, 1, 128);
		bool drawChecker = QTest.Bool("Checker", false);
		int maxAltitude = QTest.Int("Altitude", 1024, 0, 2048);
		QTest.TryGetObject("currentX", out int currentX);
		QTest.TryGetObject("currentY", out int currentY);
		QTest.TryGetObject("currentZ", out int currentZ);
		QTest.SetObject("currentX", currentX + speedX);
		QTest.SetObject("currentY", currentY + speedY);
		QTest.SetObject("currentZ", currentZ + speedZ);

		// View
		QTest.Group("");
		QTest.StartDrawPixels("Region", 128, 128, false);
		if (maxAltitude > 0) {
			FillAltitude(AltitudeCache, RegionNoise, currentX, currentY, currentZ, rCount, zoom);
		}
		int index = 0;
		for (int j = 0; j < 128; j++) {
			for (int i = 0; i < 128; i++) {
				int unitX = currentX + i * zoom;
				int unitY = currentY + j * zoom;
				int rIndex = GetRegionIndex(RegionNoise, unitX, unitY, currentZ, rCount);
				float colorVolume = 1f;
				if (maxAltitude > 0) {
					int altitude = AltitudeCache[index];
					colorVolume = (altitude / (float)maxAltitude).Clamp01();
				}
				var color = Util.HsvToRgb((float)rIndex / rCount, 1f, colorVolume);
				if (drawChecker && (unitX.UDivide(Const.MAP) % 2 == 0) != (unitY.UDivide(Const.MAP) % 2 == 0)) {
					color = Color32.BLACK;
				}
				QTest.DrawPixel(i, j, color);
				index++;
			}
		}

	}


}
#endif
