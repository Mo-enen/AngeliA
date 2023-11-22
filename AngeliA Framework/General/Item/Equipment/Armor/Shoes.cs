using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iTreeBranch), 1)]
	public class iShoesWood : Armor<iShoesWoodBroken, iShoesWood> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesWoodBroken : Armor<iShoesWoodBroken, iShoesWood> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeBranch), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iShoesWood), typeof(iIngotIron), 1)]
	public class iShoesIron : Armor<iShoesIronBroken, iShoesIron> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesIronBroken : Armor<iShoesIronBroken, iShoesIron> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iShoesIron), 1)]
	public class iShoesGold : Armor<iShoesGoldBroken, iShoesGold> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesGoldBroken : Armor<iShoesGoldBroken, iShoesGold> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iShoesVelvet), 1)]
	public class iShoesSki : Armor<iShoesSkiBroken, iShoesSki> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesSkiBroken : Armor<iShoesSkiBroken, iShoesSki> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iCottonBall), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iPropeller), typeof(iShoesWood), 1)]
	public class iShoesWing : Armor<iShoesWingBroken, iShoesWing> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesWingBroken : Armor<iShoesWingBroken, iShoesWing> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeBranch), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iCuteGhost), 1)]
	public class iShoesFairy : Armor<iShoesFairyBroken, iShoesFairy> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesFairyBroken : Armor<iShoesFairyBroken, iShoesFairy> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iRope), 1)]
	public class iShoesSand : Armor<iShoesSandBroken, iShoesSand> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesSandBroken : Armor<iShoesSandBroken, iShoesSand> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iFabric), typeof(iFabric), 1)]
	public class iShoesVelvet : Armor<iShoesVelvetBroken, iShoesVelvet> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesVelvetBroken : Armor<iShoesVelvetBroken, iShoesVelvet> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iRuneCube), 1)]
	public class iShoesMage : Armor<iShoesMageBroken, iShoesMage> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesMageBroken : Armor<iShoesMageBroken, iShoesMage> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iShoesIron), typeof(iShoesIron), 1)]
	public class iShoesKnight : Armor<iShoesKnightBroken, iShoesKnight> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesKnightBroken : Armor<iShoesKnightBroken, iShoesKnight> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iShoesVelvet), 1)]
	public class iShoesHiking : Armor<iShoesHikingBroken, iShoesHiking> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesHikingBroken : Armor<iShoesHikingBroken, iShoesHiking> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iShoesWood), 1)]
	public class iWoodenClogs : Armor<iWoodenClogsBroken, iWoodenClogs> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iWoodenClogsBroken : Armor<iWoodenClogsBroken, iWoodenClogs> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeBranch), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iShoesVelvet), typeof(iIngotGold), 1)]
	public class iShoesPaladin : Armor<iShoesPaladinBroken, iShoesPaladin> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesPaladinBroken : Armor<iShoesPaladinBroken, iShoesPaladin> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iBolt), typeof(iBolt), typeof(iBolt), typeof(iShoesIron), 1)]
	public class iShoesStudded : Armor<iShoesStuddedBroken, iShoesStudded> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesStuddedBroken : Armor<iShoesStuddedBroken, iShoesStudded> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



	[EntityAttribute.ItemCombination(typeof(iSpikeBall), typeof(iShoesIron), 1)]
	public class iShoesSpike : Armor<iShoesSpikeBroken, iShoesSpike> { public override EquipmentType EquipmentType => EquipmentType.Shoes; }
	public class iShoesSpikeBroken : Armor<iShoesSpikeBroken, iShoesSpike> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}



}
