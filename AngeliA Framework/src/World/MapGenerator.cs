using System.Collections;
using System.Collections.Generic;
using JordanPeck;

namespace AngeliA;

public enum MapGenerationResult { Success, Skipped, Fail, CriticalError, }


public abstract class MapGenerator (int typeID) {





	#region --- VAR ---


	public virtual bool IncludeInOpenWorld => true;
	public long Seed { get; internal set; }
	public string ErrorMessage { get; internal set; }
	public int TypeID { get; init; } = typeID;


	#endregion




	#region --- MSG ---


	public abstract MapGenerationResult GenerateMap (Int3 worldPos);


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---



	#endregion




}
