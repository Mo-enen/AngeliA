using System.Collections;
using System.Collections.Generic;
using JordanPeck;

namespace AngeliA;

public enum MapGenerationResult { Success, Skipped, Fail, CriticalError, }

public abstract class MapGenerator {





	#region --- VAR ---


	public virtual int Order => 0;
	public long Seed { get; internal set; }
	public string ErrorMessage { get; internal set; }
	public IBlockSquad Squad { get; internal set; }


	#endregion




	#region --- MSG ---


	public virtual void Initialize () { }


	public abstract MapGenerationResult GenerateMap (Int3 startPoint, Direction8? startDirection);


	#endregion




	#region --- API ---


	// Region Util
	protected static void InitNoiseForRegion (FastNoiseLite noise, long seed) {

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


	protected static int GetRegionIndex (FastNoiseLite noise, int unitX, int unitY, int unitZ, int regionCount) {
		float noise01 = noise.GetNoise01(unitX, unitY, unitZ);
		return (int)(noise01 * regionCount).Clamp(0, regionCount - 1);
	}


	protected static void FillAltitude (int[] altitudes, FastNoiseLite noise, int unitLeft, int unitDown, int z, int regionCount, int zoom = 1) {
		for (int i = 0; i < Const.MAP; i++) {
			int unitX = i * zoom + unitLeft;
			int currentIndex = i;
			int currentAltitude = 0;
			int currentAgent = -1;
			for (int j = 0; j < Const.MAP; j++) {
				int unitY = j * zoom + unitDown;
				int agent = GetRegionIndex(noise, unitX, unitY, z, regionCount);
				if (agent != currentAgent) {
					if (currentAgent == -1) {
						// First Block
						currentAltitude = 0;
						for (int safe = 1; safe < 2048; safe++) {
							if (agent != GetRegionIndex(noise, unitX, unitY - safe, z, regionCount)) break;
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




	#region --- LGC ---



	#endregion




}
