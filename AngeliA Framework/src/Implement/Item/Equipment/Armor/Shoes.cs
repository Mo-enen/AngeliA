using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 



// Wood

public class iShoesWood : Shoes<iShoesWoodBroken, iShoesWood> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesWoodBroken : Shoes<iShoesWoodBroken, iShoesWood> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeBranch), };
}



// Iron

public class iShoesIron : Shoes<iShoesIronCracked, iShoesIron> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesIronCracked : Shoes<iShoesIronBroken, iShoesIron> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iShoesIronBroken : Shoes<iShoesIronBroken, iShoesIronCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}



// Gold

public class iShoesGold : Shoes<iShoesGoldDented, iShoesGold> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesGoldDented : Shoes<iShoesGoldCracked, iShoesGold> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iShoesGoldCracked : Shoes<iShoesGoldBroken, iShoesGoldDented> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iShoesGoldBroken : Shoes<iShoesGoldBroken, iShoesGoldCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}



// Ski

public class iShoesSki : Shoes<iShoesSkiBroken, iShoesSki> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesSkiBroken : Shoes<iShoesSkiBroken, iShoesSki> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iCottonBall), };
}



// Wing

public class iShoesWing : Shoes<iShoesWingBroken, iShoesWing> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesWingBroken : Shoes<iShoesWingBroken, iShoesWing> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeBranch), };
}



// Fairy

public class iShoesFairy : Shoes<iShoesFairyBroken, iShoesFairy> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesFairyBroken : Shoes<iShoesFairyBroken, iShoesFairy> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// Sand

public class iShoesSand : Shoes<iShoesSandBroken, iShoesSand> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesSandBroken : Shoes<iShoesSandBroken, iShoesSand> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// Velvet

public class iShoesVelvet : Shoes<iShoesVelvetBroken, iShoesVelvet> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesVelvetBroken : Shoes<iShoesVelvetBroken, iShoesVelvet> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// Mage

public class iShoesMage : Shoes<iShoesMageBroken, iShoesMage> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesMageBroken : Shoes<iShoesMageBroken, iShoesMage> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// Knight

public class iShoesKnight : Shoes<iShoesKnightCracked, iShoesKnight> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesKnightCracked : Shoes<iShoesKnightBroken, iShoesKnight> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iShoesKnightBroken : Shoes<iShoesKnightBroken, iShoesKnightCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}



// Hiking

public class iShoesHiking : Shoes<iShoesHikingBroken, iShoesHiking> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesHikingBroken : Shoes<iShoesHikingBroken, iShoesHiking> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
}



// Clogs

public class iWoodenClogs : Shoes<iWoodenClogsBroken, iWoodenClogs> { }
[EntityAttribute.ExcludeInMapEditor]
public class iWoodenClogsBroken : Shoes<iWoodenClogsBroken, iWoodenClogs> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeBranch), };
}



// Paladin

public class iShoesPaladin : Shoes<iShoesPaladinBroken, iShoesPaladin> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesPaladinBroken : Shoes<iShoesPaladinBroken, iShoesPaladin> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
}



// Studded

public class iShoesStudded : Shoes<iShoesStuddedCracked, iShoesStudded> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesStuddedCracked : Shoes<iShoesStuddedBroken, iShoesStudded> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iShoesStuddedBroken : Shoes<iShoesStuddedBroken, iShoesStuddedCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}



// Spike

public class iShoesSpike : Shoes<iShoesSpikeCracked, iShoesSpike> { }
[EntityAttribute.ExcludeInMapEditor]
public class iShoesSpikeCracked : Shoes<iShoesSpikeBroken, iShoesSpike> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}
[EntityAttribute.ExcludeInMapEditor]
public class iShoesSpikeBroken : Shoes<iShoesSpikeBroken, iShoesSpikeCracked> {
	protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
}
