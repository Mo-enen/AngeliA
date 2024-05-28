using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 


// Hammer
public abstract class Hammer<B> : Hammer where B : MeleeBullet {
	public Hammer () => BulletID = typeof(B).AngeHash();
}
public abstract class Hammer : MeleeWeapon {
	public sealed override WeaponType WeaponType => WeaponType.Hammer;
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	protected override bool IgnoreGrabTwist => true;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 275;
	public override int RangeY => 432;

}



// Implement

public class iHammerWood : Hammer { }

public class iHammerIron : Hammer { }

public class iHammerGold : Hammer { }

public class iMaceRound : Hammer { }

public class iMaceSkull : Hammer { }

public class iBaseballBatWood : Hammer { }

public class iMaceSpiked : Hammer { }

public class iBian : Hammer {
	public override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
}

public class iHammerRiceCake : Hammer {
	public override WeaponHandheld Handheld => WeaponHandheld.Pole;
}

public class iHammerGoatHorn : Hammer { }

public class iBaseballBatIron : Hammer { }

public class iHammerThunder : Hammer {
	public override WeaponHandheld Handheld => WeaponHandheld.Pole;
}

public class iHammerMoai : Hammer {
	public override WeaponHandheld Handheld => WeaponHandheld.Pole;
}

public class iHammerPaladin : Hammer {
	public override WeaponHandheld Handheld => WeaponHandheld.Pole;
}

public class iHammerRuby : Hammer {
	public override WeaponHandheld Handheld => WeaponHandheld.Pole;
}
