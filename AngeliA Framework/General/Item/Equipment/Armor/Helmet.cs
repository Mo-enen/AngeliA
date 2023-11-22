using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iTreeTrunk), 1)]
	public class iHelmetWood : Armor<iHelmetWoodBroken, iHelmetWood> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iHelmetWoodBroken : Armor<iHelmetWoodBroken, iHelmetWood> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeTrunk), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	[EntityAttribute.ItemCombination(typeof(iHelmetWood), typeof(iIngotIron), 1)]
	public class iHelmetIron : Armor<iHelmetIronBroken, iHelmetIron> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iHelmetIronBroken : Armor<iHelmetIronBroken, iHelmetIron> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	[EntityAttribute.ItemCombination(typeof(iHelmetIron), typeof(iIngotGold), 1)]
	public class iHelmetGold : Armor<iHelmetGoldBroken, iHelmetGold> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iHelmetGoldBroken : Armor<iHelmetGoldBroken, iHelmetGold> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	[EntityAttribute.ItemCombination(typeof(iHelmetWood), typeof(iCone), 1)]
	public class iSafetyHelmet : Armor<iSafetyHelmetBroken, iSafetyHelmet> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iSafetyHelmetBroken : Armor<iSafetyHelmetBroken, iSafetyHelmet> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iTreeStump), typeof(iTreeTrunk), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	[EntityAttribute.ItemCombination(typeof(iSkull), typeof(iMeatBone), typeof(iMeatBone), typeof(iTopHat), 1)]
	public class iPirateHat : Armor<iPirateHatBroken, iPirateHat> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iPirateHatBroken : Armor<iPirateHatBroken, iPirateHat> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	[EntityAttribute.ItemCombination(typeof(iRuneCube), typeof(iTopHat), 1)]
	public class iWizardHat : Armor<iWizardHatBroken, iWizardHat> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iWizardHatBroken : Armor<iWizardHatBroken, iWizardHat> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	[EntityAttribute.ItemCombination(typeof(iBucketIron), typeof(iFabric), typeof(iRibbon), 1)]
	public class iTopHat : Armor<iTopHatBroken, iTopHat> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iTopHatBroken : Armor<iTopHatBroken, iTopHat> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	[EntityAttribute.ItemCombination(typeof(iItemWoodBoard), typeof(iPaw), typeof(iRope), 1)]
	public class iFoxMask : Armor<iFoxMaskBroken, iFoxMask> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iFoxMaskBroken : Armor<iFoxMaskBroken, iFoxMask> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iItemWoodBoard), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	[EntityAttribute.ItemCombination(typeof(iWheel), typeof(iIngotGold), 1)]
	public class iCirclet : Armor<iCircletBroken, iCirclet> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iCircletBroken : Armor<iCircletBroken, iCirclet> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	[EntityAttribute.ItemCombination(typeof(iHelmetIron), typeof(iIngotIron), 1)]
	public class iHelmetFull : Armor<iHelmetFullBroken, iHelmetFull> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iHelmetFullBroken : Armor<iHelmetFullBroken, iHelmetFull> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), typeof(iCirclet), 1)]
	public class iCrown : Armor<iCrownBroken, iCrown> {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
		protected override int Scale => 618;
	}
	public class iCrownBroken : Armor<iCrownBroken, iCrown> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotGold), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
		protected override int Scale => 618;
	}



	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iHelmetWood), 1)]
	public class iGasMask : Armor<iGasMaskBroken, iGasMask> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iGasMaskBroken : Armor<iGasMaskBroken, iGasMask> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iRunePoison), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	[EntityAttribute.ItemCombination(typeof(iHelmetIron), typeof(iHorn), typeof(iHorn), 1)]
	public class iHelmetViking : Armor<iHelmetVikingBroken, iHelmetViking> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iHelmetVikingBroken : Armor<iHelmetVikingBroken, iHelmetViking> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	[EntityAttribute.ItemCombination(typeof(iHelmetIron), typeof(iHelmetIron), 1)]
	public class iHelmetKnight : Armor<iHelmetKnightBroken, iHelmetKnight> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iHelmetKnightBroken : Armor<iHelmetKnightBroken, iHelmetKnight> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iIngotIron), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iFabric), 1)]
	public class iHelmetBandit : Armor<iHelmetBanditBroken, iHelmetBandit> { public override EquipmentType EquipmentType => EquipmentType.Helmet; }
	public class iHelmetBanditBroken : Armor<iHelmetBanditBroken, iHelmetBandit> {
		protected override System.Type[] RepairMaterials => new System.Type[] { typeof(iFabric), };
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}



}
