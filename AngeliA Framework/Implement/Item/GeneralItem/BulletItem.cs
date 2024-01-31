using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {

	[ItemCombination(typeof(iTreeBranchBundle), typeof(iFlint), typeof(iLeaf), 16)]
	public class iArrowWood : BulletItem { }
	[ItemCombination(typeof(iIronWire), typeof(iFlint), typeof(iLeaf), 16)]
	public class iArrowIron : BulletItem { }
	[ItemCombination(typeof(GoldWire), typeof(iFlint), typeof(iLeaf), 16)]
	public class iArrowGold : BulletItem { }


	[ItemCombination(typeof(iTreeBranchBundle), typeof(iFlint), 16)]
	public class iBoltsWood : BulletItem { }
	[ItemCombination(typeof(iIronWire), typeof(iFlint), 16)]
	public class iBoltsIron : BulletItem { }
	[ItemCombination(typeof(GoldWire), typeof(iFlint), 16)]
	public class iBoltsGold : BulletItem { }


	[ItemCombination(typeof(iTreeBranchBundle), typeof(iFlint), typeof(iLeaf), typeof(iRunePoison), 16)]
	public class iPoisonDarts : BulletItem { }


	[ItemCombination(typeof(iGlass), typeof(iRuneFire), 16)]
	public class iMarbles : BulletItem { }



}