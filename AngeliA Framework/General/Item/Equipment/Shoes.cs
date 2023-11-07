using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {

	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iTreeBranch), 1)]
	public class iShoesWood : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesWood), typeof(iIngotIron), 1)]
	public class iShoesIron : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iShoesIron), 1)]
	public class iShoesGold : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iShoesVelvet), 1)]
	public class iShoesSki : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iPropeller), typeof(iShoesWood), 1)]
	public class iShoesWing : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iCuteGhost), 1)]
	public class iShoesFairy : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iRope), 1)]
	public class iShoesSand : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iFabric), typeof(iFabric), 1)]
	public class iShoesVelvet : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iRuneCube), 1)]
	public class iShoesMage : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesIron), typeof(iShoesIron), 1)]
	public class iShoesKnight : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iShoesVelvet), 1)]
	public class iShoesHiking : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iShoesWood), 1)]
	public class iWoodenClogs : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iIngotGold), 1)]
	public class iShoesPaladin : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iBolt), typeof(iBolt), typeof(iBolt), typeof(iShoesIron), 1)]
	public class iShoesStudded : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

	[EntityAttribute.ItemCombination(typeof(iSpikeBall), typeof(iShoesIron), 1)]
	public class iShoesSpike : Equipment { public override EquipmentType EquipmentType => EquipmentType.Shoes; }

}
