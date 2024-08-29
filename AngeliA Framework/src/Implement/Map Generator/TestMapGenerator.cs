using System.Collections;
using System.Collections.Generic;
using JordanPeck;

namespace AngeliA;

#if DEBUG
internal class TestMapGenerator : MapGenerator {


	public override void Initialize (long seed) {

	}


	public override MapGenerationResult GenerateMap (Int3 worldPosition, long seed, World world) {


		//System.Threading.Thread.Sleep(500);
		//Debug.Log(worldPosition);
		//System.Threading.Thread.Sleep(500);




		return MapGenerationResult.Skipped;
	}


	[CheatCode("TestNoise")]
	internal static void StartNoiseTesting () {
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
					float value = noise.GetNoise01(
						Game.GlobalFrame * speedX + i,
						Game.GlobalFrame * speedY
					);
					QTest.DrawColumn(i, value, Color32.WHITE, Color32.GREY_12);
				}
				break;
			case 2:
				QTest.StartDrawPixels("View 2D", SIZE, SIZE, clearPrevPixels: false);
				for (int j = 0; j < SIZE; j++) {
					for (int i = 0; i < SIZE; i++) {
						float value = noise.GetNoise01(
							Game.GlobalFrame * speedX + i,
							Game.GlobalFrame * speedY + j
						);
						byte rgb = (byte)(value * 256f).Clamp(0, 255);
						QTest.DrawPixel(i, j, new Color32(rgb, rgb, rgb, 255));
					}
				}
				break;
			case 3:
				QTest.StartDrawPixels("View 3D", SIZE, SIZE, clearPrevPixels: false);
				for (int j = 0; j < SIZE; j++) {
					for (int i = 0; i < SIZE; i++) {
						float value = noise.GetNoise01(
							Game.GlobalFrame * speedX + i,
							Game.GlobalFrame * speedY + j,
							Game.GlobalFrame * speedZ
						);
						byte rgb = (byte)(value * 256f).Clamp(0, 255);
						QTest.DrawPixel(i, j, new Color32(rgb, rgb, rgb, 255));
					}
				}
				break;
		}
	}


}
#endif
