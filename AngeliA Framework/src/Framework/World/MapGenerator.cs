using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public enum MapGenerationResult { Success, Skipped, Fail, CriticalError, }

public abstract class MapGenerator {

	public long Seed { get; internal set; }
	public virtual int Order => 0;

	public virtual void Initialize () { }

	public abstract MapGenerationResult GenerateMap (Int3 worldPosition, World world);

}
