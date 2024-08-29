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


public abstract class RegionMapGenerator : MapGenerator {




	#region --- VAR ---


	// Const
	private const int REGION_LIST_COUNT = 1024;

	// Api
	protected virtual float Priority => 1f;

	// Data
	private static readonly FastNoiseLite RegionNoise = new();
	private static readonly RegionMapGenerator[] RegionGeneratorList = new RegionMapGenerator[REGION_LIST_COUNT];
	private static int RegionGeneratorCount = 0;


	#endregion




	#region --- MSG ---


	[BeforeAnyMapGeneratorInitialized]
	internal static void BeforeAnyMapGeneratorInitialized () {

		RegionGeneratorCount = 0;
		long seed = MapGenerationSystem.Seed;

		// Region Noise
		InitNoiseForRegion(RegionNoise, seed);


	}


	public override void Initialize (long seed) {
		if (RegionGeneratorCount >= REGION_LIST_COUNT) return;
		if (this is FailbackRegionMapGenerator) return;
		RegionGeneratorList[RegionGeneratorCount] = this;
		RegionGeneratorCount++;
	}


	[AfterAllMapGeneratorInitialized]
	internal static void AfterAllMapGeneratorInitialized () {

		long seed = MapGenerationSystem.Seed;

		// Generator List
		if (RegionGeneratorCount > 0) {
			// Scale Region Generator List
			var finalList = new RegionMapGenerator[REGION_LIST_COUNT];
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
					Util.Max((int)(REGION_LIST_COUNT * (gen.Priority / totalPriority)), 1) :
					REGION_LIST_COUNT - finalIndex;
				int end = Util.Min(finalIndex + secCount, REGION_LIST_COUNT);
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
		for (int i = 0; i < REGION_LIST_COUNT - 1; i++) {
			int target = ran.Next(i, REGION_LIST_COUNT);
			(RegionGeneratorList[i], RegionGeneratorList[target]) = (RegionGeneratorList[target], RegionGeneratorList[i]);
		}

	}


#if DEBUG
	[CheatCode("TestRegion")]
	internal static void StartTestRegion () {
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
		bool testingNoise = QTest.TryGetObject("testing", out string testingName) && testingName == "region";
		if (!testingNoise) return;

		// Config
		int rCount = QTest.Int("Region Count", 16, 1, 64);
		int speedX = QTest.Int("Speed X", 6, 0, 128);
		int speedY = QTest.Int("Speed Y", 6, 0, 128);
		int speedZ = QTest.Int("Speed Z", 2, 0, 128);
		int zoom = QTest.Int("Zoom", 42, 1, 128);

		// View
		QTest.Group("");
		QTest.StartDrawPixels("Region", 128, 128, false);
		int frame = Game.GlobalFrame;
		for (int j = 0; j < 128; j++) {
			for (int i = 0; i < 128; i++) {
				float r01 = RegionNoise.GetNoise01(
					frame * speedX + i * zoom,
					frame * speedY + j * zoom,
					frame * speedZ
				);
				int rIndex = (int)(r01 * rCount).Clamp(0, rCount - 1);
				QTest.DrawPixel(i, j, Util.HsvToRgb((float)rIndex / rCount, 1f, 1f));
			}
		}

	}
#endif


	#endregion




	#region --- API ---


	public static int GetRegionIndex (int unitX, int unitY, int unitZ) {
		float noise01 = RegionNoise.GetNoise01(unitX, unitY, unitZ);
		return (int)(noise01 * REGION_LIST_COUNT).Clamp(0, REGION_LIST_COUNT - 1);
	}


	#endregion




	#region --- LGC ---


	private static void InitNoiseForRegion (FastNoiseLite noise, long seed) {
		noise.SetSeed(seed);
		noise.SetFrequency(0.000618f);
		noise.SetNoiseType(NoiseType.Cellular);
		noise.SetFractalType(FractalType.None);
		noise.SetCellularDistanceFunction(CellularDistanceFunction.Manhattan);
		noise.SetCellularReturnType(CellularReturnType.CellValue);
		noise.SetCellularJitter(0.7f);
	}


	#endregion




}
