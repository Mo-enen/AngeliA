using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public enum MapGenerationResult { Success, Skipped, Fail, CriticalError, }

public abstract class MapGenerator {

	public string ErrorMessage { get; private set; }
	public virtual int Order => 0;

	public virtual MapGenerationResult GenerateMap (Int3 worldPosition, IBlockSquad squad, int seed) {
		ErrorMessage = "";
		return MapGenerationResult.Success;
	}

}
