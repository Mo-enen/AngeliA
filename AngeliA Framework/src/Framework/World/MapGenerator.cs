using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public enum MapGenerationResult { Success, Skipped, Fail, CriticalError, }

public abstract class MapGenerator {

	public virtual int Order => 0;

	public abstract void Initialize (long seed);

	public abstract MapGenerationResult GenerateMap (Int3 worldPosition, long seed, World world);

}
