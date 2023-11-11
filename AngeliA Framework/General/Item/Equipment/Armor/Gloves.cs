using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {

	[EntityAttribute.ItemCombination(typeof(iTreeTrunk), typeof(iTreeBranch), 1)]
	public class iGlovesWood : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesWood), typeof(iIngotIron), 1)]
	public class iGlovesIron : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iIngotGold), 1)]
	public class iGlovesGold : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iGlovesWood), 1)]
	public class iGlovesSki : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iBolt), typeof(iElectricWire), typeof(iBattery), 1)]
	public class iGlovesMachine : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGemOrange), typeof(iGlovesIron), typeof(iGemRed), typeof(iGemBlue), 1)]
	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iGemGreen), typeof(iGemRed), typeof(iGemBlue), 1)]
	[EntityAttribute.ItemCombination(typeof(iGemOrange), typeof(iGlovesIron), typeof(iGemGreen), typeof(iGemRed), 1)]
	[EntityAttribute.ItemCombination(typeof(iGemOrange), typeof(iGlovesIron), typeof(iGemGreen), typeof(iGemBlue), 1)]
	public class iGlovesGem : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iGlovesGem), 1)]
	public class iGlovesIce : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iGlovesGem), 1)]
	public class iGlovesFire : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iFabric), 1)]
	public class iGlovesVelvet : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesWood), typeof(iGoblinHead), 1)]
	public class iGlovesOrc : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iGlovesVelvet), 1)]
	public class iGlovesBoxing : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iCottonBall), typeof(iCottonBall), typeof(iGlovesVelvet), 1)]
	public class iGlovesOven : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesVelvet), typeof(iIngotGold), 1)]
	public class iGlovesPaladin : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesVelvet), typeof(iCuteGhost), 1)]
	public class iGlovesFairy : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

	[EntityAttribute.ItemCombination(typeof(iGlovesVelvet), typeof(iRuneCube), 1)]
	public class iGlovesMage : Armor { public override EquipmentType EquipmentType => EquipmentType.Gloves; }

}
