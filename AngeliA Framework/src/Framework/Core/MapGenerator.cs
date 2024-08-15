using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public enum MapGenerationResult { Success, Skipped, Fail, CriticalError, }

public abstract class MapGenerator {

	public abstract MapGenerationResult GenerateMap (Int3 worldPosition, IBlockSquad squad, int seed);

}
