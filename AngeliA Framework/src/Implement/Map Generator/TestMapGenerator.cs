using System.Collections;
using System.Collections.Generic;
using JordanPeck;

namespace AngeliA;

internal class TestMapGenerator : MapGenerator {

	public override void Initialize (long seed) {

	}

	public override MapGenerationResult GenerateMap (Int3 worldPosition, long seed, in World world) {


		System.Threading.Thread.Sleep(500);
		//Debug.Log(worldPosition);
		System.Threading.Thread.Sleep(500);




		return MapGenerationResult.Skipped;
	}

	[OnGameUpdate]
	internal static void TestUpdate () {


		// TEST
		if (!QTest.TryGetObject("noise", out FastNoiseLite noise)) {
			noise = QTest.SetObject("noise", new FastNoiseLite());
		}

		// Misc
		noise.SetFrequency(QTest.Float(
			"Frequency", 0.02f, 0.01f, 0.1f, 0.01f
		));
		noise.SetNoiseType((FastNoiseLite.NoiseType)QTest.Int(
			"Noise Type", 0, 0, 5,
			displayLabel: noise.CurrentNoiseType.ToString()
		));

		// Fractal
		noise.SetFractalType((FastNoiseLite.FractalType)QTest.Int(
			"Fractal Type", 3, 0, 5,
			displayLabel: noise.CurrentFractalType.ToString(),
			separate: true
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
		noise.SetFractalPingPongStrength(QTest.Float(
			"Fractal PingPongStrength", 2.0f, 0f, 5f, 0.1f,
			enable: noise.CurrentFractalType == FastNoiseLite.FractalType.PingPong
		));
		noise.SetFractalWeightedStrength(QTest.Float(
			"Fractal WeightedStrength", 0f, 0f, 2f, 0.1f,
			enable: noise.CurrentFractalType != FastNoiseLite.FractalType.None
		));





		// View
		const int SIZE = 128;
		QTest.BeginPixels("view", SIZE, SIZE, clearPrevPixels: false);
		for (int j = 0; j < SIZE; j++) {
			for (int i = 0; i < SIZE; i++) {
				float value = noise.GetNoise(
					Game.GlobalFrame + i,
					Game.GlobalFrame + j
				);
				byte rgb = (byte)((value + 1f) * 128f);
				QTest.DrawPixel(i, j, new Color32(rgb, rgb, rgb, 255));
			}
		}

	}

}
