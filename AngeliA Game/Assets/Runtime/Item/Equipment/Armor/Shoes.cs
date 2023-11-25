using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	// Wood
	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iTreeBranch), 1)]
	public class iShoesWood : Shoes<iShoesWoodBroken, iShoesWood> { }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesWoodBroken : Shoes<iShoesWoodBroken, iShoesWood> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeBranch), };
	}



	// Iron
	[EntityAttribute.ItemCombination(typeof(iShoesWood), typeof(iIngotIron), 1)]
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
	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iShoesIron), 1)]
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
	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iShoesVelvet), 1)]
	public class iShoesSki : Shoes<iShoesSkiBroken, iShoesSki> { }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesSkiBroken : Shoes<iShoesSkiBroken, iShoesSki> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iCottonBall), };
	}



	// Wing
	[EntityAttribute.ItemCombination(typeof(iPropeller), typeof(iShoesWood), 1)]
	public class iShoesWing : Shoes<iShoesWingBroken, iShoesWing> { }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesWingBroken : Shoes<iShoesWingBroken, iShoesWing> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeBranch), };
	}



	// Fairy
	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iCuteGhost), 1)]
	public class iShoesFairy : Shoes<iShoesFairyBroken, iShoesFairy> { }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesFairyBroken : Shoes<iShoesFairyBroken, iShoesFairy> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
	}



	// Sand
	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iRope), 1)]
	public class iShoesSand : Shoes<iShoesSandBroken, iShoesSand> { }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesSandBroken : Shoes<iShoesSandBroken, iShoesSand> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
	}



	// Velvet
	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iFabric), typeof(iFabric), 1)]
	public class iShoesVelvet : Shoes<iShoesVelvetBroken, iShoesVelvet> { }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesVelvetBroken : Shoes<iShoesVelvetBroken, iShoesVelvet> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
	}



	// Mage
	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iRuneCube), 1)]
	public class iShoesMage : Shoes<iShoesMageBroken, iShoesMage> { }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesMageBroken : Shoes<iShoesMageBroken, iShoesMage> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
	}



	// Knight
	[EntityAttribute.ItemCombination(typeof(iShoesIron), typeof(iShoesIron), 1)]
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
	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iShoesVelvet), 1)]
	public class iShoesHiking : Shoes<iShoesHikingBroken, iShoesHiking> { }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesHikingBroken : Shoes<iShoesHikingBroken, iShoesHiking> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
	}



	// Clogs
	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iShoesWood), 1)]
	public class iWoodenClogs : Shoes<iWoodenClogsBroken, iWoodenClogs> { }
	[EntityAttribute.ExcludeInMapEditor]
	public class iWoodenClogsBroken : Shoes<iWoodenClogsBroken, iWoodenClogs> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeBranch), };
	}



	// Paladin
	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iIngotGold), 1)]
	public class iShoesPaladin : Shoes<iShoesPaladinBroken, iShoesPaladin> { }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesPaladinBroken : Shoes<iShoesPaladinBroken, iShoesPaladin> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
	}



	// Studded
	[EntityAttribute.ItemCombination(typeof(iShoesIron), typeof(iBolt), typeof(iBolt), typeof(iBolt), 1)]
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
	[EntityAttribute.ItemCombination(typeof(iShoesIron), typeof(iSpikeBall), 1)]
	public class iShoesSpike : Shoes<iShoesSpikeCracked, iShoesSpike> { }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesSpikeCracked : Shoes<iShoesSpikeBroken, iShoesSpike> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesSpikeBroken : Shoes<iShoesSpikeBroken, iShoesSpikeCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
	}



}
