using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	// Wood
	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iTreeTrunk), 1)]
	public class iHelmetWood : Armor<iHelmetWoodBroken, iHelmetWood> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iHelmetWoodBroken : Armor<iHelmetWoodBroken, iHelmetWood> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeTrunk), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	// Iron
	[EntityAttribute.ItemCombination(typeof(iHelmetWood), typeof(iIngotIron), 1)]
	public class iHelmetIron : Armor<iHelmetIronCracked, iHelmetIron> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iHelmetIronCracked : Armor<iHelmetIronBroken, iHelmetIron> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iHelmetIronBroken : Armor<iHelmetIronBroken, iHelmetIronCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	// Gold
	[EntityAttribute.ItemCombination(typeof(iHelmetIron), typeof(iIngotGold), 1)]
	public class iHelmetGold : Armor<iHelmetGoldDented, iHelmetGold> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iHelmetGoldDented : Armor<iHelmetGoldCracked, iHelmetGold> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iHelmetGoldCracked : Armor<iHelmetGoldBroken, iHelmetGoldDented> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iHelmetGoldBroken : Armor<iHelmetGoldBroken, iHelmetGoldCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	// Safety
	[EntityAttribute.ItemCombination(typeof(iHelmetWood), typeof(iCone), 1)]
	public class iSafetyHelmet : Armor<iSafetyHelmetBroken, iSafetyHelmet> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iSafetyHelmetBroken : Armor<iSafetyHelmetBroken, iSafetyHelmet> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeTrunk), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	// Pirate
	[EntityAttribute.ItemCombination(typeof(iSkull), typeof(iMeatBone), typeof(iMeatBone), typeof(iTopHat), 1)]
	public class iPirateHat : Armor<iPirateHatBroken, iPirateHat> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iPirateHatBroken : Armor<iPirateHatBroken, iPirateHat> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	// Wizard
	[EntityAttribute.ItemCombination(typeof(iRuneCube), typeof(iTopHat), 1)]
	public class iWizardHat : Armor<iWizardHatBroken, iWizardHat> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iWizardHatBroken : Armor<iWizardHatBroken, iWizardHat> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	// TopHat
	[EntityAttribute.ItemCombination(typeof(iBucketIron), typeof(iFabric), typeof(iRibbon), 1)]
	public class iTopHat : Armor<iTopHatBroken, iTopHat> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iTopHatBroken : Armor<iTopHatBroken, iTopHat> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	// Fox
	[EntityAttribute.ItemCombination(typeof(iItemWoodBoard), typeof(iPaw), typeof(iRope), 1)]
	public class iFoxMask : Armor<iFoxMaskBroken, iFoxMask> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iFoxMaskBroken : Armor<iFoxMaskBroken, iFoxMask> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iItemWoodBoard), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	// Circlet
	[EntityAttribute.ItemCombination(typeof(iWheel), typeof(iIngotGold), 1)]
	public class iCirclet : Armor<iCircletBroken, iCirclet> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iCircletBroken : Armor<iCircletBroken, iCirclet> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	// Full
	[EntityAttribute.ItemCombination(typeof(iHelmetIron), typeof(iIngotIron), 1)]
	public class iHelmetFull : Armor<iHelmetFullCracked, iHelmetFull> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iHelmetFullCracked : Armor<iHelmetFullBroken, iHelmetFull> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iHelmetFullBroken : Armor<iHelmetFullBroken, iHelmetFullCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	// Crown
	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), typeof(iCirclet), 1)]
	public class iCrown : Armor<iCrownBroken, iCrown> {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
		protected override int Scale => 618;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iCrownBroken : Armor<iCrownBroken, iCrown> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
		protected override int Scale => 618;
	}



	// Gas Mask
	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iHelmetWood), 1)]
	public class iGasMask : Armor<iGasMaskBroken, iGasMask> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iGasMaskBroken : Armor<iGasMaskBroken, iGasMask> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iRunePoison), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	// Viking
	[EntityAttribute.ItemCombination(typeof(iHelmetIron), typeof(iHorn), typeof(iHorn), 1)]
	public class iHelmetViking : Armor<iHelmetVikingCracked, iHelmetViking> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iHelmetVikingCracked : Armor<iHelmetVikingBroken, iHelmetViking> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iHelmetVikingBroken : Armor<iHelmetVikingBroken, iHelmetVikingCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}


	// Knight
	[EntityAttribute.ItemCombination(typeof(iHelmetIron), typeof(iHelmetIron), 1)]
	public class iHelmetKnight : Armor<iHelmetKnightCracked, iHelmetKnight> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iHelmetKnightCracked : Armor<iHelmetKnightBroken, iHelmetKnight> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	[EntityAttribute.ExcludeInMapEditor]
	public class iHelmetKnightBroken : Armor<iHelmetKnightBroken, iHelmetKnightCracked> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	// Bandit
	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iFabric), 1)]
	public class iHelmetBandit : Armor<iHelmetBanditBroken, iHelmetBandit> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	[EntityAttribute.ExcludeInMapEditor]
	public class iHelmetBanditBroken : Armor<iHelmetBanditBroken, iHelmetBandit> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



}
