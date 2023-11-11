using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {

	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iTreeBranch), 1)]
	public class iShoesWood : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesWood), typeof(iIngotIron), 1)]
	public class iShoesIron : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iShoesIron), 1)]
	public class iShoesGold : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iShoesVelvet), 1)]
	public class iShoesSki : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iPropeller), typeof(iShoesWood), 1)]
	public class iShoesWing : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iCuteGhost), 1)]
	public class iShoesFairy : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iRope), 1)]
	public class iShoesSand : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iFabric), typeof(iFabric), 1)]
	public class iShoesVelvet : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iRuneCube), 1)]
	public class iShoesMage : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesIron), typeof(iShoesIron), 1)]
	public class iShoesKnight : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iShoesVelvet), 1)]
	public class iShoesHiking : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iShoesWood), 1)]
	public class iWoodenClogs : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iIngotGold), 1)]
	public class iShoesPaladin : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iBolt), typeof(iBolt), typeof(iBolt), typeof(iShoesIron), 1)]
	public class iShoesStudded : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iSpikeBall), typeof(iShoesIron), 1)]
	public class iShoesSpike : Armor { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

}
