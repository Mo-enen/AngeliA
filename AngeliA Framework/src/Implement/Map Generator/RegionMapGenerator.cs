using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JordanPeck;

namespace AngeliA;


public sealed class RegionMapGenerator : MapGenerator {




	#region --- VAR ---


	// Const
	private const int AGENT_COUNT = 1024;

	// Data
	private static readonly FastNoiseLite RegionNoise = new();
	private static readonly RegionMapAgent[] Agents = new RegionMapAgent[AGENT_COUNT];
	private static readonly int[] AltitudeCache = new int[Const.MAP * Const.MAP];


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


	public override void Initialize () {
		InitNoiseForRegion(RegionNoise, Seed);
		var failback = new FailbackAgent();
		for (int i = 0; i < AGENT_COUNT; i++) {
			var agent = Agents[i] ??= failback;
			agent.Seed = Seed;
		}
	}


	public override MapGenerationResult GenerateMap (Int3 worldPosition, World world) {

		int left = worldPosition.x * Const.MAP;
		int right = left + Const.MAP;
		int down = worldPosition.y * Const.MAP;
		int up = down + Const.MAP;
		int z = worldPosition.z;

		// FillAltitude
		FillAltitude(AltitudeCache, left, down, z);

		// Generate All Blocks
		int index = 0;
		for (int j = down; j < up; j++) {
			for (int i = left; i < right; i++) {
				var agent = Agents[GetAgentIndex(i, j, z)];
				agent.UnitX = i;
				agent.UnitY = j;
				agent.UnitZ = z;
				agent.Altitude = AltitudeCache[index];
				agent.Generate();
				world.Levels[index] = agent.ResultLevel;
				world.Backgrounds[index] = agent.ResultBG;
				world.Entities[index] = agent.ResultEntity;
				world.Elements[index] = agent.ResultElement;
				index++;
			}
		}
		return MapGenerationResult.Success;
	}


	// Test
#if DEBUG
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
			FillAltitude(AltitudeCache, currentX, currentY, currentZ, zoom);
		}
		int index = 0;
		for (int j = 0; j < 128; j++) {
			for (int i = 0; i < 128; i++) {
				int unitX = currentX + i * zoom;
				int unitY = currentY + j * zoom;
				int rIndex = GetAgentIndex(unitX, unitY, currentZ);
				float colorVolume = 1f;
				if (maxAltitude > 0) {
					int altitude = AltitudeCache[index];
					colorVolume = (altitude / (float)maxAltitude).Clamp01();
				}
				var color = Util.HsvToRgb((int)((float)rIndex * rCount / AGENT_COUNT) / (float)rCount, 1f, colorVolume);
				if (drawChecker && (unitX.UDivide(Const.MAP) % 2 == 0) != (unitY.UDivide(Const.MAP) % 2 == 0)) {
					color = Color32.BLACK;
				}
				QTest.DrawPixel(i, j, color);
				index++;
			}
		}

	}
#endif


	#endregion




	#region --- LGC ---


	private static void InitNoiseForRegion (FastNoiseLite noise, long seed) {

		// V1
		//noise.SetSeed(seed);
		//noise.SetFrequency(0.0012f);
		//noise.SetNoiseType(NoiseType.Cellular);
		//noise.SetFractalType(FractalType.None);
		//noise.SetCellularDistanceFunction(CellularDistanceFunction.Manhattan);
		//noise.SetCellularReturnType(CellularReturnType.CellValue);
		//noise.SetCellularJitter(0.7f);

		// V2
		noise.SetSeed(seed);
		noise.SetFrequency(0.00028f);
		noise.SetNoiseType(NoiseType.Cellular);
		noise.SetFractalType(FractalType.FBm);
		noise.SetFractalOctaves(2);
		noise.SetFractalGain(0.2f);
		noise.SetFractalLacunarity(10f);
		noise.SetFractalWeightedStrength(0);
		noise.SetCellularDistanceFunction(CellularDistanceFunction.Euclidean);
		noise.SetCellularReturnType(CellularReturnType.CellValue);
		noise.SetCellularJitter(0.2f);




	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetAgentIndex (int unitX, int unitY, int unitZ) {
		float noise01 = RegionNoise.GetNoise01(unitX, unitY, unitZ);
		return (int)(noise01 * AGENT_COUNT).Clamp(0, AGENT_COUNT - 1);
	}


	private static void FillAltitude (int[] altitudes, int unitLeft, int unitDown, int z, int zoom = 1) {
		for (int i = 0; i < Const.MAP; i++) {
			int unitX = i * zoom + unitLeft;
			int currentIndex = i;
			int currentAltitude = 0;
			int currentAgent = -1;
			for (int j = 0; j < Const.MAP; j++) {
				int unitY = j * zoom + unitDown;
				int agent = GetAgentIndex(unitX, unitY, z);
				if (agent != currentAgent) {
					if (currentAgent == -1) {
						// First Block
						currentAltitude = 0;
						for (int safe = 1; safe < 2048; safe++) {
							if (agent != GetAgentIndex(unitX, unitY - safe, z)) break;
							currentAltitude += 1;
						}
					} else {
						// Agent Changed
						currentAltitude = 0;
					}
					currentAgent = agent;
				} else {
					// Same Agent
					currentAltitude += zoom;
				}
				altitudes[currentIndex] = currentAltitude;
				currentIndex += Const.MAP;
			}
		}
	}


	#endregion




}
