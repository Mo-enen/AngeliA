using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public enum MapGenerationResult { Success, Skipped, Fail, CriticalError, }


public abstract class MapGenerator (int typeID) {

	public int TypeID { get; init; } = typeID;
	public string ErrorMessage { get; internal set; }

	public abstract MapGenerationResult GenerateMap (WorldStream stream, Int3 worldPos);

}
