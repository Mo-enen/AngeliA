using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {


	[System.Serializable]
	public class YayaMeta {

		public int WaterSpeedLose = 400;
		public int QuickSandJumpoutSpeed = 48;
		public int QuickSandMaxRunSpeed = 4;
		public int QuickSandSinkSpeed = 1;
		public int QuickSandJumpSpeed = 12;

	}



	[System.Serializable]
	public class YayaAsset {
		public AnimationCurve SquadTransitionCurve = null;
	}


}