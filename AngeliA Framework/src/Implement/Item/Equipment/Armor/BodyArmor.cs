using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 



// Armor Wood
public class iArmorWood : BodyArmor<iArmorWoodBroken, iArmorWood> { }

[EntityAttribute.ExcludeInMapEditor]
public class iArmorWoodBroken : BodyArmor<iArmorWoodBroken, iArmorWood> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iItemWoodBoard), };
}



// Armor Iron
public class iArmorIron : BodyArmor<iArmorIronCracked, iArmorIron> { }

[EntityAttribute.ExcludeInMapEditor]
public class iArmorIronCracked : BodyArmor<iArmorIronBroken, iArmorIron> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iArmorIronBroken : BodyArmor<iArmorIronBroken, iArmorIronCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}



// Armor Gold
public class iArmorGold : BodyArmor<iArmorGoldDented, iArmorGold> { }
[EntityAttribute.ExcludeInMapEditor]
public class iArmorGoldDented : BodyArmor<iArmorGoldCracked, iArmorGold> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iArmorGoldCracked : BodyArmor<iArmorGoldBroken, iArmorGoldDented> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iArmorGoldBroken : BodyArmor<iArmorGoldBroken, iArmorGoldCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}



// Brave
public class iArmorBrave : BodyArmor<iArmorBraveCracked, iArmorBrave> { }
[EntityAttribute.ExcludeInMapEditor]
public class iArmorBraveCracked : BodyArmor<iArmorBraveBroken, iArmorBrave> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iArmorBraveBroken : BodyArmor<iArmorBraveBroken, iArmorBraveCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}



// Skull
public class iArmorSkull : BodyArmor<iArmorSkullBroken, iArmorSkull> { }
[EntityAttribute.ExcludeInMapEditor]
public class iArmorSkullBroken : BodyArmor<iArmorSkullBroken, iArmorSkull> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iSkull), typeof(iTreeStump), typeof(iItemWoodBoard), };
}



// Chain Mail
public class iChainMail : BodyArmor<iChainMailCracked, iChainMail> { }
[EntityAttribute.ExcludeInMapEditor]
public class iChainMailCracked : BodyArmor<iChainMailBroken, iChainMail> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iChainMailBroken : BodyArmor<iChainMailBroken, iChainMailCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}



// Clay
public class iArmorClay : BodyArmor<iArmorClayBroken, iArmorClay> { }
[EntityAttribute.ExcludeInMapEditor]
public class iArmorClayBroken : BodyArmor<iArmorClayBroken, iArmorClay> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iClay), };
}



// Velvet
public class iVelvetDress : BodyArmor<iVelvetDressBroken, iVelvetDress> { }
[EntityAttribute.ExcludeInMapEditor]
public class iVelvetDressBroken : BodyArmor<iVelvetDressBroken, iVelvetDress> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// Cloak 
public class iCloak : BodyArmor<iCloakBroken, iCloak> { }
[EntityAttribute.ExcludeInMapEditor]
public class iCloakBroken : BodyArmor<iCloakBroken, iCloak> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// Knight
public class iArmorKnight : BodyArmor<iArmorKnightCracked, iArmorKnight> { }
[EntityAttribute.ExcludeInMapEditor]
public class iArmorKnightCracked : BodyArmor<iArmorKnightBroken, iArmorKnight> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iArmorKnightBroken : BodyArmor<iArmorKnightBroken, iArmorKnightCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}



// Mage Robe
public class iMageRobe : BodyArmor<iMageRobeBroken, iMageRobe> { }
[EntityAttribute.ExcludeInMapEditor]
public class iMageRobeBroken : BodyArmor<iMageRobeBroken, iMageRobe> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}


// Leather
public class iArmorLeather : BodyArmor<iArmorLeatherBroken, iArmorLeather> { }

[EntityAttribute.ExcludeInMapEditor]
public class iArmorLeatherBroken : BodyArmor<iArmorLeatherBroken, iArmorLeather> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iLeather), };
}


// Studded
public class iArmorStudded : BodyArmor<iArmorStuddedBroken, iArmorStudded> { }
[EntityAttribute.ExcludeInMapEditor]
public class iArmorStuddedBroken : BodyArmor<iArmorStuddedBroken, iArmorStudded> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iItemWoodBoard), typeof(iBolt), };
}


// Practitioner Robe
public class iPractitionerRobe : BodyArmor<iPractitionerRobeBroken, iPractitionerRobe> { }
[EntityAttribute.ExcludeInMapEditor]
public class iPractitionerRobeBroken : BodyArmor<iPractitionerRobeBroken, iPractitionerRobe> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// Paladin
public class iArmorPaladin : BodyArmor<iArmorPaladinDented, iArmorPaladin> { }
[EntityAttribute.ExcludeInMapEditor]
public class iArmorPaladinDented : BodyArmor<iArmorPaladinCracked, iArmorPaladin> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iArmorPaladinCracked : BodyArmor<iArmorPaladinBroken, iArmorPaladinDented> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iArmorPaladinBroken : BodyArmor<iArmorPaladinBroken, iArmorPaladinCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}
