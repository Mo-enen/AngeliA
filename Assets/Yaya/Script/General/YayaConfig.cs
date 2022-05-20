using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yaya {


	[System.Serializable]
	public class CheckPointConfig {
		[System.Serializable]
		public struct Data {
			public int Index;
			public int X;
			public int Y;
			public bool IsAltar;
		}
		public Data[] CPs = null;
	}







}
