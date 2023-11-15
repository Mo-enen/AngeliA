using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {

	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iTreeTrunk), 1)]
	public class iHelmetWood : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }

	[EntityAttribute.ItemCombination(typeof(iHelmetWood), typeof(iIngotIron), 1)]
	public class iHelmetIron : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }

	[EntityAttribute.ItemCombination(typeof(iHelmetIron), typeof(iIngotGold), 1)]
	public class iHelmetGold : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }

	[EntityAttribute.ItemCombination(typeof(iHelmetWood), typeof(iCone), 1)]
	public class iSafetyHelmet : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }

	[EntityAttribute.ItemCombination(typeof(iSkull), typeof(iMeatBone), typeof(iMeatBone), typeof(iTopHat), 1)]
	public class iPirateHat : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }

	[EntityAttribute.ItemCombination(typeof(iRuneCube), typeof(iTopHat), 1)]
	public class iWizardHat : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }

	[EntityAttribute.ItemCombination(typeof(iBucketIron), typeof(iFabric), typeof(iRibbon), 1)]
	public class iTopHat : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }

	[EntityAttribute.ItemCombination(typeof(iItemWoodBoard), typeof(iPaw), typeof(iRope), 1)]
	public class iFoxMask : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }

	[EntityAttribute.ItemCombination(typeof(iWheel), typeof(iIngotGold), 1)]
	public class iCirclet : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }

	[EntityAttribute.ItemCombination(typeof(iHelmetIron), typeof(iIngotIron), 1)]
	public class iHelmetFull : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), typeof(iCirclet), 1)]
	public class iCrown : Armor {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
		protected override int Scale => 618;
	}

	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iHelmetWood), 1)]
	public class iGasMask : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }

	[EntityAttribute.ItemCombination(typeof(iHelmetIron), typeof(iHorn), typeof(iHorn), 1)]
	public class iHelmetViking : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }

	[EntityAttribute.ItemCombination(typeof(iHelmetIron), typeof(iHelmetIron), 1)]
	public class iHelmetKnight : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }

	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iFabric), 1)]
	public class iHelmetBandit : Armor { public override EquipmentType EquipmentType => EquipmentType.Helmet; }



}
