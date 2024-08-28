using System.Collections;
using System.Collections.Generic;
using JordanPeck;

namespace AngeliA;


public sealed class FailbackRegionMapGenerator : RegionMapGenerator {
	public static FailbackRegionMapGenerator Instance { get; private set; }
	public FailbackRegionMapGenerator () => Instance = this;
	public override MapGenerationResult GenerateMap (Int3 worldPosition, long seed, World world) {
		return MapGenerationResult.Skipped;
	}
}


public class Test0 : RegionMapGenerator {
	protected override float Priority => 2f;
	public override MapGenerationResult GenerateMap (Int3 worldPosition, long seed, World world) {
		return MapGenerationResult.Skipped;
	}
}
public class Test1 : RegionMapGenerator {
	protected override float Priority => 1f;
	public override MapGenerationResult GenerateMap (Int3 worldPosition, long seed, World world) {
		return MapGenerationResult.Skipped;
	}
}
public class Test2 : RegionMapGenerator {
	protected override float Priority => 0.5f;
	public override MapGenerationResult GenerateMap (Int3 worldPosition, long seed, World world) {
		return MapGenerationResult.Skipped;
	}
}

public abstract class RegionMapGenerator : MapGenerator {




	#region --- VAR ---


	// Api
	protected virtual float Priority => 1f;

	// Data
	private static readonly FastNoiseLite RegionNoise = new();
	private static readonly RegionMapGenerator[] RegionGeneratorList = new RegionMapGenerator[1024];
	private static int RegionGeneratorCount = 0;


	#endregion




	#region --- MSG ---


	[BeforeAnyMapGeneratorInitialized]
	internal static void BeforeAnyMapGeneratorInitialized () {
		RegionGeneratorCount = 0;
	}


	public override void Initialize (long seed) {
		if (RegionGeneratorCount >= RegionGeneratorList.Length) return;
		if (this is FailbackRegionMapGenerator) return;
		RegionGeneratorList[RegionGeneratorCount] = this;
		RegionGeneratorCount++;
	}


	[AfterAllMapGeneratorInitialized]
	internal static void AfterAllMapGeneratorInitialized () {

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

		// Generator List
		int listLen = RegionGeneratorList.Length;
		if (RegionGeneratorCount > 0) {
			// Scale Region Generator List
			var finalList = new RegionMapGenerator[listLen];
			float totalPriority = 0f;
			for (int i = 0; i < RegionGeneratorCount; i++) {
				totalPriority += RegionGeneratorList[i].Priority;
			}
			if (totalPriority.AlmostZero()) {
				totalPriority = 1f;
			}
			int finalIndex = 0;
			for (int i = 0; i < RegionGeneratorCount; i++) {
				var gen = RegionGeneratorList[i];
				int secCount = i < RegionGeneratorCount - 1 ?
					Util.Max((int)(listLen * (gen.Priority / totalPriority)), 1) :
					listLen - finalIndex;
				int end = Util.Min(finalIndex + secCount, listLen);
				for (; finalIndex < end; finalIndex++) {
					finalList[finalIndex] = gen;
				}
			}
			finalList.CopyTo(RegionGeneratorList, 0);
		} else {
			// Failback
			RegionGeneratorList.FillWithValue(FailbackRegionMapGenerator.Instance);
		}

		// Shuffle List
		var ran = new System.Random((int)seed);
		for (int i = 0; i < listLen - 1; i++) {
			int target = ran.Next(i, listLen);
			(RegionGeneratorList[i], RegionGeneratorList[target]) = (RegionGeneratorList[target], RegionGeneratorList[i]);
		}

	}


	#endregion




	#region --- API ---


	public static int GetRegionIndex (int unitX, int unitY) {


		// TODO


		return 0;
	}


	#endregion




	#region --- LGC ---



	#endregion




}
