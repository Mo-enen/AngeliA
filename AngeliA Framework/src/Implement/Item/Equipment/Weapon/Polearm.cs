using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 


// Polearm
public abstract class Polearm<B> : Polearm where B : MeleeBullet {
	public Polearm () => BulletID = typeof(B).AngeHash();
}
public abstract class Polearm : MeleeWeapon {
	public sealed override WeaponType WeaponType => WeaponType.Polearm;
	public sealed override WeaponHandheld Handheld => WeaponHandheld.Pole;
	protected override bool IgnoreGrabTwist => true;
	public override int AttackDuration => 18;
	public override int AttackCooldown => 2;
	public override int RangeXLeft => 384;
	public override int RangeXRight => 384;
	public override int RangeY => 432;

}


// Implement

public class iSpearWood : Polearm { }

public class iSpearIron : Polearm { }

public class iSpearGold : Polearm { }

public class iSpearHeavy : Polearm { }


public class iBoStaffWood : Polearm { }

public class iBoStaffIron : Polearm { }

public class iBoStaffGold : Polearm { }

public class iNaginata : Polearm { }

public class iHalberd : Polearm { }

public class iJi : Polearm { }

public class iMonkSpade : Polearm { }

public class iManCatcher : Polearm { }

public class iSwallow : Polearm { }

public class iFork : Polearm { }

public class iBrandistock : Polearm { }
