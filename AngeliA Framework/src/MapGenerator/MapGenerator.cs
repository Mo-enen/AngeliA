using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public enum MapGenerationResult { Success, Skipped, Fail, CriticalError, }


public abstract class MapGenerator {

	public int TypeID { get; init; }
	public string ErrorMessage { get; internal set; }

	public MapGenerator () => TypeID = GetType().AngeHash();

	public abstract MapGenerationResult GenerateMap (IBlockSquad squad, Int3 worldPos);

}
