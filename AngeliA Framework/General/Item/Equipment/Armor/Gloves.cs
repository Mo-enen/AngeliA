using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iTreeTrunk), typeof(iTreeBranch), 1)]
	public class iGlovesWood : Armor<iGlovesWoodBroken, iGlovesWood> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesWoodBroken : Armor<iGlovesWoodBroken, iGlovesWood> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeTrunk), typeof(iTreeBranch), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iGlovesWood), typeof(iIngotIron), 1)]
	public class iGlovesIron : Armor<iGlovesIronBroken, iGlovesIron> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesIronBroken : Armor<iGlovesIronBroken, iGlovesIron> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iIngotGold), 1)]
	public class iGlovesGold : Armor<iGlovesGoldBroken, iGlovesGold> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesGoldBroken : Armor<iGlovesGoldBroken, iGlovesGold> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iGlovesWood), 1)]
	public class iGlovesSki : Armor<iGlovesSkiBroken, iGlovesSki> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesSkiBroken : Armor<iGlovesSkiBroken, iGlovesSki> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iCottonBall), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iBolt), typeof(iElectricWire), typeof(iBattery), 1)]
	public class iGlovesMachine : Armor<iGlovesMachineBroken, iGlovesMachine> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesMachineBroken : Armor<iGlovesMachineBroken, iGlovesMachine> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iGemOrange), typeof(iGlovesIron), typeof(iGemRed), typeof(iGemBlue), 1)]
	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iGemGreen), typeof(iGemRed), typeof(iGemBlue), 1)]
	[EntityAttribute.ItemCombination(typeof(iGemOrange), typeof(iGlovesIron), typeof(iGemGreen), typeof(iGemRed), 1)]
	[EntityAttribute.ItemCombination(typeof(iGemOrange), typeof(iGlovesIron), typeof(iGemGreen), typeof(iGemBlue), 1)]
	public class iGlovesGem : Armor<iGlovesGemBroken, iGlovesGem> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesGemBroken : Armor<iGlovesGemBroken, iGlovesGem> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iGemRed), typeof(iGemGreen), typeof(iGemBlue), typeof(iGemOrange), typeof(iGem), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iGlovesGem), 1)]
	public class iGlovesIce : Armor<iGlovesIceBroken, iGlovesIce> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesIceBroken : Armor<iGlovesIceBroken, iGlovesIce> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iRuneWater), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iGlovesGem), 1)]
	public class iGlovesFire : Armor<iGlovesFireBroken, iGlovesFire> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesFireBroken : Armor<iGlovesFireBroken, iGlovesFire> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iRuneFire), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iFabric), 1)]
	public class iGlovesVelvet : Armor<iGlovesVelvetBroken, iGlovesVelvet> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesVelvetBroken : Armor<iGlovesVelvetBroken, iGlovesVelvet> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iGlovesWood), typeof(iGoblinHead), 1)]
	public class iGlovesOrc : Armor<iGlovesOrcBroken, iGlovesOrc> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesOrcBroken : Armor<iGlovesOrcBroken, iGlovesOrc> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeTrunk), typeof(iTreeBranch), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iCottonBall), typeof(iCottonBall), typeof(iGlovesVelvet), 1)]
	public class iGlovesBoxing : Armor<iGlovesBoxingBroken, iGlovesBoxing> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesBoxingBroken : Armor<iGlovesBoxingBroken, iGlovesBoxing> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iCottonBall), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iGlovesVelvet), 1)]
	public class iGlovesOven : Armor<iGlovesOvenBroken, iGlovesOven> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesOvenBroken : Armor<iGlovesOvenBroken, iGlovesOven> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iCottonBall), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iGlovesVelvet), typeof(iIngotGold), 1)]
	public class iGlovesPaladin : Armor<iGlovesPaladinBroken, iGlovesPaladin> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesPaladinBroken : Armor<iGlovesPaladinBroken, iGlovesPaladin> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iGlovesVelvet), typeof(iCuteGhost), 1)]
	public class iGlovesFairy : Armor<iGlovesFairyBroken, iGlovesFairy> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesFairyBroken : Armor<iGlovesFairyBroken, iGlovesFairy> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


	[EntityAttribute.ItemCombination(typeof(iGlovesVelvet), typeof(iRuneCube), 1)]
	public class iGlovesMage : Armor<iGlovesMageBroken, iGlovesMage> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	public class iGlovesMageBroken : Armor<iGlovesMageBroken, iGlovesMage> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}


}
