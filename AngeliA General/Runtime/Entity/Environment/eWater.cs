using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGeneral {

	public class eCommonWater : Water { }

	// Source
	public class eCommonWaterSourceLeft : WaterSource<eCommonWater> {
		public override Direction4 Direction => Direction4.Left;
	}
	public class eCommonWaterSourceRight : WaterSource<eCommonWater> {
		public override Direction4 Direction => Direction4.Right;
	}
	public class eCommonWaterSourceDown : WaterSource<eCommonWater> {
		public override Direction4 Direction => Direction4.Down;
	}
	public class eCommonWaterSourceUp : WaterSource<eCommonWater> {
		public override Direction4 Direction => Direction4.Up;
	}

}