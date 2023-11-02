using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	// General
	public class iTakodachi : ItemSummon<eTakodachi> {
		public override int MaxStackCount => 1;
	}
	public class iSucorn : ItemSummon<eSucorn> {
		public override int MaxStackCount => 1;
	}
	public class iOruyanke : ItemSummon<eOruyanke> {
		public override int MaxStackCount => 1;
	}

	// Food
	public class iTakoMilk : Food { }



}