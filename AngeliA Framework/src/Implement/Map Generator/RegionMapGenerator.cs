using System.Collections;
using System.Collections.Generic;
using JordanPeck;

namespace AngeliA;

public abstract class RegionMapGenerator : MapGenerator {




	#region --- VAR ---

	// Data
	private static readonly FastNoiseLite RegionNoise = new();
	//private static int RegionHeight;



	#endregion




	#region --- MSG ---


	[OnMapGeneratorInitialized]
	internal static void OnMapGeneratorInitialized () {

		long seed = MapGenerationSystem.Seed;

		// Region Noise
		RegionNoise.SetSeed(seed);
		RegionNoise.SetFrequency(0.02f);
		RegionNoise.SetNoiseType(NoiseType.Value);
		RegionNoise.SetFractalType(FractalType.PingPong);
		RegionNoise.SetFractalGain(0.5f);
		RegionNoise.SetFractalLacunarity(5f);
		RegionNoise.SetFractalOctaves(2);
		RegionNoise.SetFractalPingPongStrength(4.5f);
		RegionNoise.SetFractalWeightedStrength(0.618f);

		


	}


	#endregion




	#region --- API ---





	#endregion




	#region --- LGC ---



	#endregion




}
