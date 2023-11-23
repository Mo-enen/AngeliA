using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	// Wood
	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iTreeBranch), 1)]
	public class iShoesWood : Armor<iShoesWoodBroken, iShoesWood> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesWoodBroken : Armor<iShoesWoodBroken, iShoesWood> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeBranch), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Iron
	[EntityAttribute.ItemCombination(typeof(iShoesWood), typeof(iIngotIron), 1)]
	public class iShoesIron : Armor<iShoesIronCracked, iShoesIron> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesIronCracked : Armor<iShoesIronBroken, iShoesIron> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesIronBroken : Armor<iShoesIronBroken, iShoesIronCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Gold
	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iShoesIron), 1)]
	public class iShoesGold : Armor<iShoesGoldDented, iShoesGold> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesGoldDented : Armor<iShoesGoldCracked, iShoesGold> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesGoldCracked : Armor<iShoesGoldBroken, iShoesGoldDented> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesGoldBroken : Armor<iShoesGoldBroken, iShoesGoldCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Ski
	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iShoesVelvet), 1)]
	public class iShoesSki : Armor<iShoesSkiBroken, iShoesSki> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesSkiBroken : Armor<iShoesSkiBroken, iShoesSki> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iCottonBall), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Wing
	[EntityAttribute.ItemCombination(typeof(iPropeller), typeof(iShoesWood), 1)]
	public class iShoesWing : Armor<iShoesWingBroken, iShoesWing> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesWingBroken : Armor<iShoesWingBroken, iShoesWing> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeBranch), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Fairy
	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iCuteGhost), 1)]
	public class iShoesFairy : Armor<iShoesFairyBroken, iShoesFairy> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesFairyBroken : Armor<iShoesFairyBroken, iShoesFairy> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Sand
	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iRope), 1)]
	public class iShoesSand : Armor<iShoesSandBroken, iShoesSand> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesSandBroken : Armor<iShoesSandBroken, iShoesSand> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Velvet
	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iFabric), typeof(iFabric), 1)]
	public class iShoesVelvet : Armor<iShoesVelvetBroken, iShoesVelvet> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesVelvetBroken : Armor<iShoesVelvetBroken, iShoesVelvet> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Mage
	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iRuneCube), 1)]
	public class iShoesMage : Armor<iShoesMageBroken, iShoesMage> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesMageBroken : Armor<iShoesMageBroken, iShoesMage> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Knight
	[EntityAttribute.ItemCombination(typeof(iShoesIron), typeof(iShoesIron), 1)]
	public class iShoesKnight : Armor<iShoesKnightCracked, iShoesKnight> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesKnightCracked : Armor<iShoesKnightBroken, iShoesKnight> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesKnightBroken : Armor<iShoesKnightBroken, iShoesKnightCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Hiking
	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iShoesVelvet), 1)]
	public class iShoesHiking : Armor<iShoesHikingBroken, iShoesHiking> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesHikingBroken : Armor<iShoesHikingBroken, iShoesHiking> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Clogs
	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iShoesWood), 1)]
	public class iWoodenClogs : Armor<iWoodenClogsBroken, iWoodenClogs> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iWoodenClogsBroken : Armor<iWoodenClogsBroken, iWoodenClogs> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeBranch), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Paladin
	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iIngotGold), 1)]
	public class iShoesPaladin : Armor<iShoesPaladinBroken, iShoesPaladin> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesPaladinBroken : Armor<iShoesPaladinBroken, iShoesPaladin> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Studded
	[EntityAttribute.ItemCombination(typeof(iShoesIron), typeof(iBolt), typeof(iBolt), typeof(iBolt), 1)]
	public class iShoesStudded : Armor<iShoesStuddedCracked, iShoesStudded> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesStuddedCracked : Armor<iShoesStuddedBroken, iShoesStudded> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesStuddedBroken : Armor<iShoesStuddedBroken, iShoesStuddedCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	// Spike
	[EntityAttribute.ItemCombination(typeof(iShoesIron), typeof(iSpikeBall), 1)]
	public class iShoesSpike : Armor<iShoesSpikeCracked, iShoesSpike> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesSpikeCracked : Armor<iShoesSpikeBroken, iShoesSpike> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iShoesSpikeBroken : Armor<iShoesSpikeBroken, iShoesSpikeCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



}
