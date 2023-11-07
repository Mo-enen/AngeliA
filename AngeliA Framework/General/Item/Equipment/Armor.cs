using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iItemWoodBoard), 1)]
	public class iArmorWood : ProgressiveEquipment<iArmorWoodBroken, iArmorWood> {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorWoodBroken : ProgressiveEquipment<iArmorWoodBroken, iArmorWood> {
		protected override System.Type RepairMaterial => typeof(iTreeStump);
		protected override System.Type RepairMaterialAlt => typeof(iItemWoodBoard);
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}

	[EntityAttribute.ItemCombination(typeof(iArmorWood), typeof(iIngotIron), 1)]
	public class iArmorIron : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iArmorIron), 1)]
	public class iArmorGold : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }

	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iArmorIron), 1)]
	public class iArmorBrave : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }

	[EntityAttribute.ItemCombination(typeof(iSkull), typeof(iSkull), typeof(iArmorWood), 1)]
	public class iArmorSkull : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }

	[EntityAttribute.ItemCombination(typeof(iArmorIron), typeof(iIngotIron), 1)]
	public class iChainMail : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }

	[EntityAttribute.ItemCombination(typeof(iClay), typeof(iArmorWood), 1)]
	public class iArmorClay : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }

	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iFabric), typeof(iFabric), typeof(iFabric), 1)]
	public class iVelvetDress : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }

	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iFabric), typeof(iFabric), typeof(iFabric), 1)]
	public class iCloak : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }

	[EntityAttribute.ItemCombination(typeof(iArmorIron), typeof(iArmorIron), 1)]
	public class iArmorKnight : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }

	[EntityAttribute.ItemCombination(typeof(iCloak), typeof(iRuneCube), 1)]
	public class iMageRobe : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }

	[EntityAttribute.ItemCombination(typeof(iLeather), typeof(iLeather), typeof(iLeather), typeof(iArmorWood), 1)]
	public class iArmorLeather : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }

	[EntityAttribute.ItemCombination(typeof(iBolt), typeof(iBolt), typeof(iBolt), typeof(iArmorWood), 1)]
	public class iArmorStudded : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }

	[EntityAttribute.ItemCombination(typeof(iEar), typeof(iNose), typeof(iFist), typeof(iCloak), 1)]
	public class iPractitionerRobe : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iArmorStudded), 1)]
	public class iArmorPaladin : Equipment { public override EquipmentType EquipmentType => EquipmentType.BodySuit; }


}
