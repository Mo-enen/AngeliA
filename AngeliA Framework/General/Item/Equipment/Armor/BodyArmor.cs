using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iItemWoodBoard), 1)]
	public class iArmorWood : Armor {
		protected override System.Type PrevEquipment => typeof(iArmorWoodBroken);
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	public class iArmorWoodBroken : Armor {
		protected override System.Type NextEquipment => typeof(iArmorWood);
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iItemWoodBoard), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}



	[EntityAttribute.ItemCombination(typeof(iArmorWood), typeof(iIngotIron), 1)]
	public class iArmorIron : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iArmorIron), 1)]
	public class iArmorGold : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }

	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iArmorIron), 1)]
	public class iArmorBrave : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }

	[EntityAttribute.ItemCombination(typeof(iSkull), typeof(iSkull), typeof(iArmorWood), 1)]
	public class iArmorSkull : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }

	[EntityAttribute.ItemCombination(typeof(iArmorIron), typeof(iIngotIron), 1)]
	public class iChainMail : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }

	[EntityAttribute.ItemCombination(typeof(iClay), typeof(iArmorWood), 1)]
	public class iArmorClay : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }

	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iFabric), typeof(iFabric), typeof(iFabric), 1)]
	public class iVelvetDress : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }

	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iFabric), typeof(iFabric), typeof(iFabric), 1)]
	public class iCloak : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }

	[EntityAttribute.ItemCombination(typeof(iArmorIron), typeof(iArmorIron), 1)]
	public class iArmorKnight : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }

	[EntityAttribute.ItemCombination(typeof(iCloak), typeof(iRuneCube), 1)]
	public class iMageRobe : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }

	[EntityAttribute.ItemCombination(typeof(iLeather), typeof(iLeather), typeof(iLeather), typeof(iArmorWood), 1)]
	public class iArmorLeather : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }

	[EntityAttribute.ItemCombination(typeof(iBolt), typeof(iBolt), typeof(iBolt), typeof(iArmorWood), 1)]
	public class iArmorStudded : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }

	[EntityAttribute.ItemCombination(typeof(iEar), typeof(iNose), typeof(iFist), typeof(iCloak), 1)]
	public class iPractitionerRobe : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iArmorStudded), 1)]
	public class iArmorPaladin : Armor { public override EquipmentType EquipmentType => EquipmentType.Body; }


}
