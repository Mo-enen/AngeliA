using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 



// Wood

public class iHelmetWood : Helmet<iHelmetWoodBroken, iHelmetWood> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override bool HideEar => true;
	protected override bool HideHorn => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iHelmetWoodBroken : Helmet<iHelmetWoodBroken, iHelmetWood> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeTrunk), };
}



// Iron

public class iHelmetIron : Helmet<iHelmetIronCracked, iHelmetIron> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override bool HideEar => true;
	protected override bool HideHorn => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iHelmetIronCracked : Helmet<iHelmetIronBroken, iHelmetIron> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iHelmetIronBroken : Helmet<iHelmetIronBroken, iHelmetIronCracked> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}



// Gold

public class iHelmetGold : Helmet<iHelmetGoldDented, iHelmetGold> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override bool HideEar => true;
	protected override bool HideHorn => true;
	protected override bool HideHair => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iHelmetGoldDented : Helmet<iHelmetGoldCracked, iHelmetGold> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
	protected override bool HideEar => true;
	protected override bool HideHair => true;
	protected override bool HideHorn => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iHelmetGoldCracked : Helmet<iHelmetGoldBroken, iHelmetGoldDented> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
	protected override bool HideEar => true;
	protected override bool HideHorn => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iHelmetGoldBroken : Helmet<iHelmetGoldBroken, iHelmetGoldCracked> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}



// Safety

public class iSafetyHelmet : Helmet<iSafetyHelmetBroken, iSafetyHelmet> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override bool HideEar => true;
	protected override bool HideHorn => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iSafetyHelmetBroken : Helmet<iSafetyHelmetBroken, iSafetyHelmet> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeTrunk), };
}



// Pirate

public class iPirateHat : Helmet<iPirateHatBroken, iPirateHat> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override bool HideEar => true;
	protected override bool HideHorn => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iPirateHatBroken : Helmet<iPirateHatBroken, iPirateHat> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// Wizard

public class iWizardHat : Helmet<iWizardHatBroken, iWizardHat> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override bool HideEar => true;
	protected override bool HideHorn => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iWizardHatBroken : Helmet<iWizardHatBroken, iWizardHat> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// TopHat

public class iTopHat : Helmet<iTopHatBroken, iTopHat> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override bool HideEar => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iTopHatBroken : Helmet<iTopHatBroken, iTopHat> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// Fox

public class iFoxMask : Helmet<iFoxMaskBroken, iFoxMask> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override bool HideFace => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iFoxMaskBroken : Helmet<iFoxMaskBroken, iFoxMask> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iItemWoodBoard), };
}



// Circlet

public class iCirclet : Helmet<iCircletBroken, iCirclet> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
}
[EntityAttribute.ExcludeInMapEditor]
public class iCircletBroken : Helmet<iCircletBroken, iCirclet> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}



// Full

public class iHelmetFull : Helmet<iHelmetFullCracked, iHelmetFull> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override bool HideFace => true;
	protected override bool HideEar => true;
	protected override bool HideHair => true;
	protected override bool HideHorn => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iHelmetFullCracked : Helmet<iHelmetFullBroken, iHelmetFull> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
	protected override bool HideEar => true;
	protected override bool HideHair => true;
	protected override bool HideHorn => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iHelmetFullBroken : Helmet<iHelmetFullBroken, iHelmetFullCracked> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}



// Crown

public class iCrown : Helmet<iCrownBroken, iCrown> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override int Scale => 618;
}
[EntityAttribute.ExcludeInMapEditor]
public class iCrownBroken : Helmet<iCrownBroken, iCrown> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
	protected override int Scale => 618;
}



// Gas Mask

public class iGasMask : Helmet<iGasMaskBroken, iGasMask> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override bool HideFace => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iGasMaskBroken : Helmet<iGasMaskBroken, iGasMask> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iRunePoison), };
}



// Viking

public class iHelmetViking : Helmet<iHelmetVikingCracked, iHelmetViking> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override bool HideEar => true;
	protected override bool HideHorn => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iHelmetVikingCracked : Helmet<iHelmetVikingBroken, iHelmetViking> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iHelmetVikingBroken : Helmet<iHelmetVikingBroken, iHelmetVikingCracked> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}


// Knight

public class iHelmetKnight : Helmet<iHelmetKnightCracked, iHelmetKnight> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override bool HideEar => true;
	protected override bool HideHorn => true;
	protected override bool HideFace => true;
	protected override bool HideHair => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iHelmetKnightCracked : Helmet<iHelmetKnightBroken, iHelmetKnight> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
	protected override bool HideEar => true;
	protected override bool HideHorn => true;
	protected override bool HideFace => true;
	protected override bool HideHair => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iHelmetKnightBroken : Helmet<iHelmetKnightBroken, iHelmetKnightCracked> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Cover;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}



// Bandit

public class iHelmetBandit : Helmet<iHelmetBanditBroken, iHelmetBandit> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override bool HideFace => true;
}
[EntityAttribute.ExcludeInMapEditor]
public class iHelmetBanditBroken : Helmet<iHelmetBanditBroken, iHelmetBandit> {
	protected override HelmetWearingMode WearingMode => HelmetWearingMode.Attach;
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}
