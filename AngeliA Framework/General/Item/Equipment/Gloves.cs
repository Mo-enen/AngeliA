using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {

	[EntityAttribute.ItemCombination(typeof(iTreeTrunk), typeof(iTreeBranch), 1)]
	public class iGlovesWood : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesWood), typeof(iIngotIron), 1)]
	public class iGlovesIron : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iIngotGold), 1)]
	public class iGlovesGold : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iGlovesWood), 1)]
	public class iGlovesSki : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iBolt), typeof(iElectricWire), typeof(iBattery), 1)]
	public class iGlovesMachine : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGemOrange), typeof(iGlovesIron), typeof(iGemRed), typeof(iGemBlue), 1)]
	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iGemGreen), typeof(iGemRed), typeof(iGemBlue), 1)]
	[EntityAttribute.ItemCombination(typeof(iGemOrange), typeof(iGlovesIron), typeof(iGemGreen), typeof(iGemRed), 1)]
	[EntityAttribute.ItemCombination(typeof(iGemOrange), typeof(iGlovesIron), typeof(iGemGreen), typeof(iGemBlue), 1)]
	public class iGlovesGem : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iGlovesGem), 1)]
	public class iGlovesIce : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iGlovesGem), 1)]
	public class iGlovesFire : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iFabric), 1)]
	public class iGlovesVelvet : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesWood), typeof(iGoblinHead), 1)]
	public class iGlovesOrc : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iGlovesVelvet), 1)]
	public class iGlovesBoxing : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iCottonBall), typeof(iCottonBall), typeof(iGlovesVelvet), 1)]
	public class iGlovesOven : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesVelvet), typeof(iIngotGold), 1)]
	public class iGlovesPaladin : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesVelvet), typeof(iCuteGhost), 1)]
	public class iGlovesFairy : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesVelvet), typeof(iRuneCube), 1)]
	public class iGlovesMage : Equipment { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

}
