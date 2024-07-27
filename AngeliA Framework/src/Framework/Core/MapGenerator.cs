using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class MapGenerator {




	#region --- VAR ---


	// Data
	private static bool Enable;
	private static WorldStream Stream;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Enable = Universe.BuiltIn.Info.UseProceduralMap;
		if (!Enable) return;
		Stream = WorldStream.GetOrCreateStreamFromPool(Universe.BuiltIn.UserMapRoot);
		if (Stream == null) Enable = false;
	}


	[OnGameUpdate]
	internal static void OnGameUpdate () {
		if (!Enable) return;

		



	}


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---



	#endregion




}
