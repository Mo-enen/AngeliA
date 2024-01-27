using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {

	[ItemCombination(typeof(iTreeBranchBundle), typeof(iFlint), typeof(iLeaf), 16)]
	public class iArrowWood : ItemArrow { }
	[ItemCombination(typeof(iIronWire), typeof(iFlint), typeof(iLeaf), 16)]
	public class iArrowIron : ItemArrow { }
	[ItemCombination(typeof(GoldWire), typeof(iFlint), typeof(iLeaf), 16)]
	public class iArrowGold : ItemArrow { }


	[ItemCombination(typeof(iTreeBranchBundle), typeof(iFlint), 16)]
	public class iBoltsWood : ItemArrow { }
	[ItemCombination(typeof(iIronWire), typeof(iFlint), 16)]
	public class iBoltsIron : ItemArrow { }
	[ItemCombination(typeof(GoldWire), typeof(iFlint), 16)]
	public class iBoltsGold : ItemArrow { }


	[ItemCombination(typeof(iTreeBranchBundle), typeof(iFlint), typeof(iLeaf), typeof(iRunePoison), 16)]
	public class iPoisonDarts : ItemArrow { }


	[ItemCombination(typeof(iGlass), typeof(iRuneFire), 16)]
	public class iMarbles : ItemArrow { }



}