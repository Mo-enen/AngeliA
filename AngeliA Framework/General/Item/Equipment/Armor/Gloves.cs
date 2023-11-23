using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	// Wood
	[EntityAttribute.ItemCombination(typeof(iTreeTrunk), typeof(iTreeBranch), 1)]
	public class iGlovesWood : Armor<iGlovesWoodBroken, iGlovesWood> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesWoodBroken : Armor<iGlovesWoodBroken, iGlovesWood> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeTrunk), typeof(iTreeBranch), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Iron
	[EntityAttribute.ItemCombination(typeof(iGlovesWood), typeof(iIngotIron), 1)]
	public class iGlovesIron : Armor<iGlovesIronCracked, iGlovesIron> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesIronCracked : Armor<iGlovesIronBroken, iGlovesIron> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesIronBroken : Armor<iGlovesIronBroken, iGlovesIronCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Gold
	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iIngotGold), 1)]
	public class iGlovesGold : Armor<iGlovesGoldDented, iGlovesGold> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesGoldDented : Armor<iGlovesGoldCracked, iGlovesGold> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesGoldCracked : Armor<iGlovesGoldBroken, iGlovesGoldDented> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesGoldBroken : Armor<iGlovesGoldBroken, iGlovesGoldCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Ski
	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iGlovesWood), 1)]
	public class iGlovesSki : Armor<iGlovesSkiBroken, iGlovesSki> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesSkiBroken : Armor<iGlovesSkiBroken, iGlovesSki> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iCottonBall), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Machine
	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iBolt), typeof(iElectricWire), typeof(iBattery), 1)]
	public class iGlovesMachine : Armor<iGlovesMachineCracked, iGlovesMachine> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesMachineCracked : Armor<iGlovesMachineBroken, iGlovesMachine> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesMachineBroken : Armor<iGlovesMachineBroken, iGlovesMachineCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Gem
	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iGemOrange), typeof(iGemRed), typeof(iGemBlue), 1)]
	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iGemGreen), typeof(iGemRed), typeof(iGemBlue), 1)]
	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iGemOrange), typeof(iGemGreen), typeof(iGemRed), 1)]
	[EntityAttribute.ItemCombination(typeof(iGlovesIron), typeof(iGemOrange), typeof(iGemGreen), typeof(iGemBlue), 1)]
	public class iGlovesGem : Armor<iGlovesGemCracked, iGlovesGem> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesGemCracked : Armor<iGlovesGemBroken, iGlovesGem> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iGemRed), typeof(iGemGreen), typeof(iGemBlue), typeof(iGemOrange), typeof(iGem), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesGemBroken : Armor<iGlovesGemBroken, iGlovesGemCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iGemRed), typeof(iGemGreen), typeof(iGemBlue), typeof(iGemOrange), typeof(iGem), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Ice
	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iGlovesGem), 1)]
	public class iGlovesIce : Armor<iGlovesIceCracked, iGlovesIce> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesIceCracked : Armor<iGlovesIceBroken, iGlovesIce> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iRuneWater), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesIceBroken : Armor<iGlovesIceBroken, iGlovesIceCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iRuneWater), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Fire
	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iGlovesGem), 1)]
	public class iGlovesFire : Armor<iGlovesFireCracked, iGlovesFire> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesFireCracked : Armor<iGlovesFireBroken, iGlovesFire> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iRuneFire), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesFireBroken : Armor<iGlovesFireBroken, iGlovesFireCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iRuneFire), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Velvet
	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iFabric), 1)]
	public class iGlovesVelvet : Armor<iGlovesVelvetBroken, iGlovesVelvet> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesVelvetBroken : Armor<iGlovesVelvetBroken, iGlovesVelvet> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Orc
	[EntityAttribute.ItemCombination(typeof(iGlovesWood), typeof(iGoblinHead), 1)]
	public class iGlovesOrc : Armor<iGlovesOrcBroken, iGlovesOrc> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesOrcBroken : Armor<iGlovesOrcBroken, iGlovesOrc> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeTrunk), typeof(iTreeBranch), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Boxing
	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iCottonBall), typeof(iCottonBall), typeof(iGlovesVelvet), 1)]
	public class iGlovesBoxing : Armor<iGlovesBoxingBroken, iGlovesBoxing> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesBoxingBroken : Armor<iGlovesBoxingBroken, iGlovesBoxing> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iCottonBall), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Oven
	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iGlovesVelvet), 1)]
	public class iGlovesOven : Armor<iGlovesOvenBroken, iGlovesOven> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesOvenBroken : Armor<iGlovesOvenBroken, iGlovesOven> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iCottonBall), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Paladin
	[EntityAttribute.ItemCombination(typeof(iGlovesVelvet), typeof(iIngotGold), 1)]
	public class iGlovesPaladin : Armor<iGlovesPaladinBroken, iGlovesPaladin> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesPaladinBroken : Armor<iGlovesPaladinBroken, iGlovesPaladin> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Fairy
	[EntityAttribute.ItemCombination(typeof(iGlovesVelvet), typeof(iCuteGhost), 1)]
	public class iGlovesFairy : Armor<iGlovesFairyBroken, iGlovesFairy> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesFairyBroken : Armor<iGlovesFairyBroken, iGlovesFairy> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



	// Mage
	[EntityAttribute.ItemCombination(typeof(iGlovesVelvet), typeof(iRuneCube), 1)]
	public class iGlovesMage : Armor<iGlovesMageBroken, iGlovesMage> { public override EquipmentType EquipmentType => EquipmentType.Gloves; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGlovesMageBroken : Armor<iGlovesMageBroken, iGlovesMage> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}



}
