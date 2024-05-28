using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 



// Wood
public class iGlovesWood : Gloves<iGlovesWoodBroken, iGlovesWood> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesWoodBroken : Gloves<iGlovesWoodBroken, iGlovesWood> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeTrunk), typeof(iTreeBranch), };
}



// Iron
public class iGlovesIron : Gloves<iGlovesIronCracked, iGlovesIron> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesIronCracked : Gloves<iGlovesIronBroken, iGlovesIron> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesIronBroken : Gloves<iGlovesIronBroken, iGlovesIronCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}



// Gold
public class iGlovesGold : Gloves<iGlovesGoldDented, iGlovesGold> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesGoldDented : Gloves<iGlovesGoldCracked, iGlovesGold> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesGoldCracked : Gloves<iGlovesGoldBroken, iGlovesGoldDented> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesGoldBroken : Gloves<iGlovesGoldBroken, iGlovesGoldCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}



// Ski
public class iGlovesSki : Gloves<iGlovesSkiBroken, iGlovesSki> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesSkiBroken : Gloves<iGlovesSkiBroken, iGlovesSki> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iCottonBall), };
}



// Machine
public class iGlovesMachine : Gloves<iGlovesMachineCracked, iGlovesMachine> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesMachineCracked : Gloves<iGlovesMachineBroken, iGlovesMachine> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesMachineBroken : Gloves<iGlovesMachineBroken, iGlovesMachineCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}



// Gem
public class iGlovesGem : Gloves<iGlovesGemCracked, iGlovesGem> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesGemCracked : Gloves<iGlovesGemBroken, iGlovesGem> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iGemRed), typeof(iGemGreen), typeof(iGemBlue), typeof(iGemOrange), typeof(iGem), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesGemBroken : Gloves<iGlovesGemBroken, iGlovesGemCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iGemRed), typeof(iGemGreen), typeof(iGemBlue), typeof(iGemOrange), typeof(iGem), };
}



// Ice
public class iGlovesIce : Gloves<iGlovesIceCracked, iGlovesIce> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesIceCracked : Gloves<iGlovesIceBroken, iGlovesIce> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iRuneWater), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesIceBroken : Gloves<iGlovesIceBroken, iGlovesIceCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iRuneWater), };
}



// Fire
public class iGlovesFire : Gloves<iGlovesFireCracked, iGlovesFire> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesFireCracked : Gloves<iGlovesFireBroken, iGlovesFire> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iRuneFire), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesFireBroken : Gloves<iGlovesFireBroken, iGlovesFireCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iRuneFire), };
}



// Velvet
public class iGlovesVelvet : Gloves<iGlovesVelvetBroken, iGlovesVelvet> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesVelvetBroken : Gloves<iGlovesVelvetBroken, iGlovesVelvet> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// Orc
public class iGlovesOrc : Gloves<iGlovesOrcBroken, iGlovesOrc> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesOrcBroken : Gloves<iGlovesOrcBroken, iGlovesOrc> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeTrunk), typeof(iTreeBranch), };
}



// Boxing
public class iGlovesBoxing : Gloves<iGlovesBoxingBroken, iGlovesBoxing> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesBoxingBroken : Gloves<iGlovesBoxingBroken, iGlovesBoxing> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iCottonBall), };
}



// Oven
public class iGlovesOven : Gloves<iGlovesOvenBroken, iGlovesOven> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesOvenBroken : Gloves<iGlovesOvenBroken, iGlovesOven> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iCottonBall), };
}



// Paladin

public class iGlovesPaladin : Gloves<iGlovesPaladinBroken, iGlovesPaladin> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesPaladinBroken : Gloves<iGlovesPaladinBroken, iGlovesPaladin> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// Fairy

public class iGlovesFairy : Gloves<iGlovesFairyBroken, iGlovesFairy> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesFairyBroken : Gloves<iGlovesFairyBroken, iGlovesFairy> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// Mage

public class iGlovesMage : Gloves<iGlovesMageBroken, iGlovesMage> { }
[EntityAttribute.ExcludeInMapEditor]
public class iGlovesMageBroken : Gloves<iGlovesMageBroken, iGlovesMage> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}
