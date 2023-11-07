using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {

	public class CommonWater : Water { }

	// Source
	public class CommonWaterSourceLeft : WaterSource<CommonWater> {
		public override Direction4 Direction => Direction4.Left;
	}
	public class CommonWaterSourceRight : WaterSource<CommonWater> {
		public override Direction4 Direction => Direction4.Right;
	}
	public class CommonWaterSourceDown : WaterSource<CommonWater> {
		public override Direction4 Direction => Direction4.Down;
	}
	public class CommonWaterSourceUp : WaterSource<CommonWater> {
		public override Direction4 Direction => Direction4.Up;
	}

}