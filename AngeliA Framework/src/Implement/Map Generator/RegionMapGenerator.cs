using System.Collections;
using System.Collections.Generic;
using JordanPeck;

namespace AngeliA;

public abstract class RegionMapGenerator : MapGenerator {




	#region --- VAR ---

	// Data
	private static readonly FastNoiseLite RegionNoise = new();
	private static int RegionHeight;



	#endregion




	#region --- MSG ---


	[OnMapGeneratorInitialized]
	internal static void OnMapGeneratorInitialized () {
		long seed = MapGenerationSystem.Seed;
		RegionNoise.SetSeed(seed);
		RegionNoise.SetFrequency(0.03f);
		RegionNoise.SetNoiseType(NoiseType.Value);


	}


	#endregion




	#region --- API ---





	#endregion




	#region --- LGC ---



	#endregion




}
