using System.Collections;
using System.Collections.Generic;
using JordanPeck;

namespace AngeliA;

internal class TestMapGenerator : MapGenerator {


	private static readonly SavingBool TestingNoise = new("Test.TestingNoise", false, SavingLocation.Global);


	public override void Initialize (long seed) {

	}


	public override MapGenerationResult GenerateMap (Int3 worldPosition, long seed, World world) {


		//System.Threading.Thread.Sleep(500);
		//Debug.Log(worldPosition);
		//System.Threading.Thread.Sleep(500);




		return MapGenerationResult.Skipped;
	}


	[CheatCode("NoiseTest")]
	internal static void EnableNoiseTesting () {
		if (TestingNoise.Value != QTest.Testing) {
			TestingNoise.Value = QTest.Testing;
		}
		TestingNoise.Value = !TestingNoise.Value;
		if (!TestingNoise.Value) {
			QTest.HideTest();
		} else {
			QTest.ShowTest();
		}
	}


	[OnGameUpdate]
	internal static void NoiseTestUpdate () {

		if (!TestingNoise.Value) return;

		// TEST
		QTest.ShowNotUpdatedData = false;
		if (!QTest.TryGetObject("noise", out FastNoiseLite noise)) {
			noise = QTest.SetObject("noise", new FastNoiseLite());
		}

		// Misc
		bool use3D = QTest.Bool("3D", true);

		QTest.Group("General");

		float speedX = QTest.Float("Speed X", 0.6f, 0f, 5f, 0.1f);
		float speedY = QTest.Float("Speed Y", 0.3f, 0f, 5f, 0.1f);
		noise.SetFrequency(QTest.Float(
			"Frequency", 0.02f, 0.01f, 0.10f, 0.01f
		));
		noise.SetNoiseType((NoiseType)QTest.Int(
			"Noise Type", 0, 0, 5,
			displayLabel: noise.CurrentNoiseType.ToString()
		));

		// Fractal
		QTest.Group("Fractal", folding: true);

		noise.SetFractalType((FractalType)QTest.Int(
			"Fractal Type", 3, 0, 5,
			displayLabel: noise.CurrentFractalType.ToString()
		));
		noise.SetFractalGain(QTest.Float(
			"Fractal Gain", 0.5f, 0f, 1f, 0.1f
		));
		noise.SetFractalLacunarity(QTest.Float(
			"Fractal Lacunarity", 2.0f, 0f, 5f, 0.1f
		));
		noise.SetFractalOctaves(QTest.Int(
			"Fractal Octaves", 3, 1, 6
		));
		if (noise.CurrentFractalType == FractalType.PingPong) {
			noise.SetFractalPingPongStrength(QTest.Float(
				"Fractal PingPongStrength", 2.0f, 0f, 5f, 0.1f
			));
		}
		if (noise.CurrentFractalType != FractalType.None) {
			noise.SetFractalWeightedStrength(QTest.Float(
				"Fractal WeightedStrength", 0f, 0f, 2f, 0.1f
			));
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



		// Domain Warp
		QTest.Group("Domain Warp", folding: true);
		if (
			noise.CurrentFractalType == FractalType.DomainWarpIndependent ||
			noise.CurrentFractalType == FractalType.DomainWarpProgressive
		) {
			noise.SetDomainWarpType((DomainWarpType)QTest.Int(
				"Domain Warp Type", 0, 0, 2,
				displayLabel: noise.CurrentDomainWarpType.ToString()
			));
			noise.SetDomainWarpAmp(QTest.Float(
				"Domain Warp Amp", 1.0f, 0f, 5f, 0.1f
			));
		}

		// View
		QTest.Group("");
		const int SIZE = 128;
		if (use3D) {
			QTest.StartDrawPixels("View 3D", SIZE, SIZE, clearPrevPixels: false);
			for (int j = 0; j < SIZE; j++) {
				for (int i = 0; i < SIZE; i++) {
					float value = noise.GetNoise(
						Game.GlobalFrame * speedX + i,
						Game.GlobalFrame * speedY + j
					);
					byte rgb = (byte)((value + 1f) * 128f).Clamp(0, 255);
					QTest.DrawPixel(i, j, new Color32(rgb, rgb, rgb, 255));
				}
			}
		} else {
			QTest.StartDrawColumn("View 2D", SIZE, clearPrevPixels: false);
			for (int i = 0; i < SIZE; i++) {
				float value = noise.GetNoise(
					Game.GlobalFrame * speedX + i,
					Game.GlobalFrame * speedY
				);
				QTest.DrawColumn(i, value, Color32.WHITE, Color32.GREY_12);
			}
		}

		// Update Testing
		if (!QTest.Testing) {
			TestingNoise.Value = false;
		}

	}


}
