using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 


// Claw
public abstract class Claw<B> : Claw where B : MeleeBullet {
	public Claw () => BulletID = typeof(B).AngeHash();
}
public abstract class Claw : MeleeWeapon {
	public sealed override WeaponType WeaponType => WeaponType.Claw;
	public sealed override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
	public override int AttackDuration => 10;
	public override int AttackCooldown => 0;
	public override int? DefaultSpeedLoseOnAttack => 1000;
	public override int? WalkingSpeedLoseOnAttack => 1000;
	public override int? RunningSpeedLoseOnAttack => 1000;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 275;
	public override int RangeY => 432;
}


// Implement

public class iClawWood : Claw { }


public class iClawIron : Claw { }


public class iClawGold : Claw { }


public class iMandarinDuckAxe : Claw { }


public class iClawCat : Claw { }


public class iClawFox : Claw { }


public class iKatars : Claw { }


public class iKatarsTripple : Claw { }


public class iEmeiPiercer : Claw { }


public class iBaton : Claw { }


public class iKnuckleDuster : Claw { }


public class iEmeiFork : Claw { }


public class iWuXingHook : Claw { }


public class iKatarsRuby : Claw { }


public class iKatarsJagged : Claw { }
