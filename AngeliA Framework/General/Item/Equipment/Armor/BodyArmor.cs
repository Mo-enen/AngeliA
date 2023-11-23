using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	// Armor Wood
	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iItemWoodBoard), 1)]
	public class iArmorWood : Armor<iArmorWoodBroken, iArmorWood> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorWoodBroken : Armor<iArmorWoodBroken, iArmorWood> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iItemWoodBoard), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}



	// Armor Iron
	[EntityAttribute.ItemCombination(typeof(iArmorWood), typeof(iIngotIron), 1)]
	public class iArmorIron : Armor<iArmorIronCracked, iArmorIron> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorIronCracked : Armor<iArmorIronBroken, iArmorIron> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorIronBroken : Armor<iArmorIronBroken, iArmorIronCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}



	// Armor Gold
	[EntityAttribute.ItemCombination(typeof(iArmorIron), typeof(iIngotGold), 1)]
	public class iArmorGold : Armor<iArmorGoldDented, iArmorGold> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorGoldDented : Armor<iArmorGoldCracked, iArmorGold> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorGoldCracked : Armor<iArmorGoldBroken, iArmorGoldDented> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorGoldBroken : Armor<iArmorGoldBroken, iArmorGoldCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}



	// Brave
	[EntityAttribute.ItemCombination(typeof(iArmorIron), typeof(iFabric), 1)]
	public class iArmorBrave : Armor<iArmorBraveCracked, iArmorBrave> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorBraveCracked : Armor<iArmorBraveBroken, iArmorBrave> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorBraveBroken : Armor<iArmorBraveBroken, iArmorBraveCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}



	// Skull
	[EntityAttribute.ItemCombination(typeof(iSkull), typeof(iSkull), typeof(iArmorWood), 1)]
	public class iArmorSkull : Armor<iArmorSkullBroken, iArmorSkull> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorSkullBroken : Armor<iArmorSkullBroken, iArmorSkull> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iSkull), typeof(iTreeStump), typeof(iItemWoodBoard), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}



	// Chain Mail
	[EntityAttribute.ItemCombination(typeof(iArmorIron), typeof(iIngotIron), 1)]
	public class iChainMail : Armor<iChainMailCracked, iChainMail> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iChainMailCracked : Armor<iChainMailBroken, iChainMail> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iChainMailBroken : Armor<iChainMailBroken, iChainMailCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}



	// Clay
	[EntityAttribute.ItemCombination(typeof(iClay), typeof(iArmorWood), 1)]
	public class iArmorClay : Armor<iArmorClayBroken, iArmorClay> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorClayBroken : Armor<iArmorClayBroken, iArmorClay> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iClay), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}



	// Velvet
	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iFabric), typeof(iFabric), typeof(iFabric), 1)]
	public class iVelvetDress : Armor<iVelvetDressBroken, iVelvetDress> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iVelvetDressBroken : Armor<iVelvetDressBroken, iVelvetDress> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}



	// Cloak 
	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iFabric), typeof(iFabric), typeof(iFabric), 1)]
	public class iCloak : Armor<iCloakBroken, iCloak> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iCloakBroken : Armor<iCloakBroken, iCloak> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}



	// Knight
	[EntityAttribute.ItemCombination(typeof(iArmorIron), typeof(iArmorIron), 1)]
	public class iArmorKnight : Armor<iArmorKnightCracked, iArmorKnight> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorKnightCracked : Armor<iArmorKnightBroken, iArmorKnight> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorKnightBroken : Armor<iArmorKnightBroken, iArmorKnightCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}



	// Mage Robe
	[EntityAttribute.ItemCombination(typeof(iCloak), typeof(iRuneCube), 1)]
	public class iMageRobe : Armor<iMageRobeBroken, iMageRobe> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iMageRobeBroken : Armor<iMageRobeBroken, iMageRobe> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}


	// Leather
	[EntityAttribute.ItemCombination(typeof(iArmorWood), typeof(iLeather), typeof(iLeather), typeof(iLeather), 1)]
	public class iArmorLeather : Armor<iArmorLeatherBroken, iArmorLeather> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorLeatherBroken : Armor<iArmorLeatherBroken, iArmorLeather> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iLeather), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}


	// Studded
	[EntityAttribute.ItemCombination(typeof(iBolt), typeof(iBolt), typeof(iBolt), typeof(iArmorWood), 1)]
	public class iArmorStudded : Armor<iArmorStuddedBroken, iArmorStudded> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorStuddedBroken : Armor<iArmorStuddedBroken, iArmorStudded> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iItemWoodBoard), typeof(iBolt), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}


	// Practitioner Robe
	[EntityAttribute.ItemCombination(typeof(iEar), typeof(iNose), typeof(iFist), typeof(iCloak), 1)]
	public class iPractitionerRobe : Armor<iPractitionerRobeBroken, iPractitionerRobe> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iPractitionerRobeBroken : Armor<iPractitionerRobeBroken, iPractitionerRobe> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}



	// Paladin
	[EntityAttribute.ItemCombination(typeof(iArmorBrave), typeof(iIngotGold), 1)]
	public class iArmorPaladin : Armor<iArmorPaladinDented, iArmorPaladin> {
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorPaladinDented : Armor<iArmorPaladinCracked, iArmorPaladin> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorPaladinCracked : Armor<iArmorPaladinBroken, iArmorPaladinDented> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iArmorPaladinBroken : Armor<iArmorPaladinBroken, iArmorPaladinCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Body;
	}


}
