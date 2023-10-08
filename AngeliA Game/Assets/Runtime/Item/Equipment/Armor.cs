using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using System;


namespace AngeliaGame {
	public class iArmorWood : ProgressiveEquipment<iArmorWoodBroken, iArmorWood> {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorWoodBroken : ProgressiveEquipment<iArmorWoodBroken, iArmorWood> {
		protected override Type RepairMaterial => typeof(iTreeStump);
		protected override Type RepairMaterialAlt => typeof(iItemWoodBoard);
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorBrave : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorSkull : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iChainMail : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorClay : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iVelvetDress : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iCloak : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorKnight : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iMageRobe : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorLeather : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorStudded : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iPractitionerRobe : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorPaladin : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
}
