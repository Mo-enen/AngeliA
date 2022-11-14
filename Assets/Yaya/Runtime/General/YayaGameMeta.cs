using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Yaya {
	[System.Serializable]
	public class YayaMeta {

		public int WaterSpeedLose = 400;
		public int QuickSandJumpoutSpeed = 48;
		public int QuickSandMaxRunSpeed = 4;
		public int QuickSandSinkSpeed = 1;
		public int QuickSandJumpSpeed = 12;
		public int SquadTransitionDuration = 32;
		public AnimationCurve SquadTransitionCurve = null;

	}
}