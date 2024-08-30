using System.Collections;
using System.Collections.Generic;
using JordanPeck;

namespace AngeliA;


public sealed class RegionMapGenerator : MapGenerator {




	#region --- VAR ---


	// Const
	private const int AGENT_COUNT = 1024;

	// Data
	private static readonly FastNoiseLite GeographyNoise = new();
	private static readonly FastNoiseLite RegionNoise = new();
	private static readonly RegionMapAgent[] Agents = new RegionMapAgent[AGENT_COUNT];


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void InitializeAgents () {

		if (Game.IsToolApplication) return;

		// Create All Agents
		float totalPriority = 0f;
		var list = new List<RegionMapAgent>();
		var failbackType = typeof(FailbackAgent);
		foreach (var aType in typeof(RegionMapAgent).AllChildClass()) {
			if (aType == failbackType) continue;
			if (System.Activator.CreateInstance(aType) is not RegionMapAgent agent) continue;
			list.Add(agent);
			totalPriority += agent.Priority;
		}
		// Fill into Agent Array Based On Priority
		if (list.Count == 0 || totalPriority.AlmostZero()) return;
		int index = 0;
		foreach (var agent in list) {
			int currentCount = (int)(AGENT_COUNT * agent.Priority / totalPriority);
			currentCount = currentCount.Clamp(1, AGENT_COUNT - index);
			for (int i = 0; i < currentCount; i++) {
				Agents[index] = agent;
				index++;
			}
		}
		var lastAgent = list[^1];
		for (; index < AGENT_COUNT; index++) {
			Agents[index] = lastAgent;
		}

	}


	public override void Initialize (long seed) {
		InitNoiseForRegion(RegionNoise, seed, false);
		InitNoiseForRegion(GeographyNoise, seed, true);
	}


#if DEBUG
	// Test
	[CheatCode("TestRegion")]
	internal static void StartTestRegion () {
		QTest.ClearAll();
		QTest.ShowTest();
		QTest.SetObject("testing", "region");
		long newSeed = new System.Random(
			(int)(System.DateTime.Now.Ticks + System.Environment.UserName.AngeHash())
		).NextInt64(long.MinValue, long.MaxValue);
		InitNoiseForRegion(RegionNoise, newSeed, false);
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


	public override MapGenerationResult GenerateMap (Int3 worldPosition, long seed, World world) {


		// TODO


		return MapGenerationResult.Skipped;
	}


	#endregion




	#region --- API ---


	public static RegionMapAgent GetAgent (int unitX, int unitY, int unitZ) {
		float noise01 = RegionNoise.GetNoise01(unitX, unitY, unitZ);
		int index = (int)(noise01 * AGENT_COUNT).Clamp(0, AGENT_COUNT - 1);
		return Agents[index];
	}


	public static float GetGeography01 (int unitX, int unitY, int unitZ) {
		return GeographyNoise.GetNoise01(unitX, unitY, unitZ);
	}


	#endregion




	#region --- LGC ---


	private static void InitNoiseForRegion (FastNoiseLite noise, long seed, bool large) {
		noise.SetSeed(large ? seed + 1 : seed);
		noise.SetFrequency(large ? 0.0000618f : 0.000618f);
		noise.SetNoiseType(NoiseType.Cellular);
		noise.SetFractalType(FractalType.None);
		noise.SetCellularDistanceFunction(CellularDistanceFunction.Manhattan);
		noise.SetCellularReturnType(CellularReturnType.CellValue);
		noise.SetCellularJitter(0.7f);
	}


	#endregion




}
