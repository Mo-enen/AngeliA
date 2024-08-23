using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class LightingSystem {




	#region --- SUB ---



	#endregion




	#region --- VAR ---


	// Api
	public static bool Enable { get; private set; } = true;

	// Data



	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Enable = !Game.IsToolApplication && Universe.BuiltInInfo.UseLightingSystem;
		if (!Enable) return;


	}


	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		if (!Enable) return;



	}


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---



	#endregion




}
