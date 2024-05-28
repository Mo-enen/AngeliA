using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 


// Magic
public abstract class MagicWeapon<B> : MagicWeapon where B : MovableBullet {
	public MagicWeapon () => BulletID = typeof(B).AngeHash();
}
public abstract class MagicWeapon : ProjectileWeapon {
	public sealed override WeaponType WeaponType => WeaponType.Magic;
}


// Implement

public class iWand : MagicWeapon<iWand.WandBullet> {
	public class WandBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}



public class iWandOrb : MagicWeapon<iWandOrb.WandOrbBullet> {
	public class WandOrbBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}



public class iStaffFire : MagicWeapon<iStaffFire.StaffFireBullet> {
	public class StaffFireBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.Pole;
}



public class iStaffWater : MagicWeapon<iStaffWater.StaffWaterBullet> {
	public class StaffWaterBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.Pole;
}



public class iStaffLightning : MagicWeapon<iStaffLightning.StaffLightningBullet> {
	public class StaffLightningBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.Pole;
}



public class iStaffPoision : MagicWeapon<iStaffPoision.StaffPoisionBullet> {
	public class StaffPoisionBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.Pole;
}



public class iRitualSkull : MagicWeapon<iRitualSkull.RitualSkullBullet> {
	public class RitualSkullBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.Float;
}



public class iBambooSlips : MagicWeapon<iBambooSlips.BambooSlipsBullet> {
	public class BambooSlipsBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.Float;
}



public class iRitualRuneCube : MagicWeapon<iRitualRuneCube.RitualRuneCubeBullet> {
	public class RitualRuneCubeBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.Float;
}



public class iGoblinTrophy : MagicWeapon<iGoblinTrophy.GoblinTrophyBullet> {
	public class GoblinTrophyBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.Float;
}



public class iMagicOrb : MagicWeapon<iMagicOrb.MagicOrbBullet> {
	public class MagicOrbBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.Float;
}



public class iMagicEyeball : MagicWeapon<iMagicEyeball.MagicEyeballBullet> {
	public class MagicEyeballBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.Float;
}



public class iMagicPotion : MagicWeapon<iMagicPotion.MagicPotionBullet> {
	public class MagicPotionBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.Float;
}



public class iWandStar : MagicWeapon<iWandStar.WandStarBullet> {
	public class WandStarBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}



public class iWandFairy : MagicWeapon<iWandFairy.WandFairyBullet> {
	public class WandFairyBullet : MovableBullet {

	}
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}
